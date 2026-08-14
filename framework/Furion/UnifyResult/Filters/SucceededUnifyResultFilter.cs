// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

using Furion.DataValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Furion.UnifyResult;

/// <summary>
/// 规范化结构（请求成功）过滤器
/// </summary>
public class SucceededUnifyResultFilter : IAsyncActionFilter
{
    /// <summary>
    /// 处理规范化结果
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 执行 Action 并获取结果
        var actionExecutedContext = await next();

        // 排除 WebSocket 请求处理
        if (actionExecutedContext.HttpContext.IsWebSocketRequest()) return;

        // 处理已经含有状态码结果的 Result
        if (actionExecutedContext.Result is IStatusCodeActionResult statusCodeResult && statusCodeResult.StatusCode != null)
        {
            // 小于 200 或者 大于 299 都不是成功值，直接跳过
            if (statusCodeResult.StatusCode.Value < 200 || statusCodeResult.StatusCode.Value > 299)
            {
                // 处理规范化结果
                if (!UnifyContext.CheckExceptionHttpContextNonUnify(context.HttpContext, out var unifyRes))
                {
                    var httpContext = context.HttpContext;
                    var statusCode = statusCodeResult.StatusCode.Value;

                    // 解决刷新 Token 时间和 Token 时间相近问题
                    if (statusCodeResult.StatusCode.Value == StatusCodes.Status401Unauthorized
                        && httpContext.Response.Headers.ContainsKey("access-token")
                        && httpContext.Response.Headers.ContainsKey("x-access-token"))
                    {
                        httpContext.Response.StatusCode = statusCode = StatusCodes.Status403Forbidden;
                    }

                    // 如果 Response 已经完成输出，则禁止写入
                    if (httpContext.Response.HasStarted) return;

                    // 检查是否启用状态码拦截中间件
                    if (UnifyContext.EnabledStatusCodesMiddleware)
                    {
                        // 获取授权失败设置的状态码
                        if (httpContext.Items.TryGetValue(AuthorizationHandlerContextExtensions.FAIL_STATUSCODE_KEY, out var authorizationFailStatusCode)
                            && authorizationFailStatusCode != null)
                        {
                            statusCode = Convert.ToInt32(authorizationFailStatusCode);
                        }

                        await unifyRes.OnResponseStatusCodes(httpContext, statusCode, httpContext.RequestServices.GetService<IOptions<UnifyResultSettingsOptions>>()?.Value);
                    }
                }

                return;
            }
        }

        // 如果出现异常，则不会进入该过滤器
        if (actionExecutedContext.Exception != null) return;

        // 获取控制器信息
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

        // 判断是否支持 MVC 规范化处理或特定检查
        if (!UnifyContext.CheckSupportMvcController(context.HttpContext, actionDescriptor, out _)
            || UnifyContext.CheckHttpContextNonUnify(context.HttpContext)) return;

        // 判断是否跳过规范化处理
        if (UnifyContext.CheckSucceededNonUnify(actionDescriptor.MethodInfo, out var unifyResult))
        {
            // 处理禁用规范化处理但配置了 JSON 序列化情况
            var (extractedValue, serializerSettings, isTargetType) = actionExecutedContext.Result switch
            {
                ObjectResult objectResult => (objectResult.Value, UnifyContext.GetSerializerSettings(context), true),
                JsonResult jsonResult => (jsonResult.Value, jsonResult.SerializerSettings ?? UnifyContext.GetSerializerSettings(context), true),
                _ => (null, null, false)
            };

            if (isTargetType && serializerSettings is not null)
            {
                actionExecutedContext.Result = new JsonResult(extractedValue, serializerSettings);
            }

            return;
        }

        // 处理 BadRequestObjectResult 类型规范化处理
        if (actionExecutedContext.Result is BadRequestObjectResult badRequestObjectResult)
        {
            // 解析验证消息
            var validationMetadata = ValidatorContext.GetValidationMetadata(badRequestObjectResult.Value);
            var unifyResultSettingsOptions = context.HttpContext.RequestServices.GetService<IOptions<UnifyResultSettingsOptions>>()?.Value;
            validationMetadata.SingleValidationErrorDisplay = unifyResultSettingsOptions?.SingleValidationErrorDisplay ?? false;

            var result = unifyResult.OnValidateFailed(context, validationMetadata);
            if (result != null) actionExecutedContext.Result = result;
        }
        else
        {
            IActionResult result = default;

            // 检查是否是有效的结果（可进行规范化的结果）
            if (UnifyContext.CheckVaildResult(actionExecutedContext.Result, out var data))
            {
                result = unifyResult.OnSucceeded(actionExecutedContext, data);
            }

            // 如果是不能规范化的结果类型，则跳过
            if (result == null) return;

            actionExecutedContext.Result = result;
        }
    }
}