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

using Furion.FriendlyException;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Furion.DataValidation;

/// <summary>
/// 数据验证拦截器（Razor Pages）
/// </summary>
public sealed class DataValidationPageFilter : IAsyncPageFilter
{
    /// <summary>
    /// Api 行为配置选项
    /// </summary>
    private readonly ApiBehaviorOptions _apiBehaviorOptions;

    /// <summary>
    /// 友好异常存储键
    /// </summary>
    private const string FriendlyExceptionKey = nameof(DataValidationFilter) + nameof(AppFriendlyException);

    /// <summary>
    /// 验证元数据存储键
    /// </summary>
    private const string ValidationMetadataKey = nameof(DataValidationFilter) + nameof(ValidationMetadata);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public DataValidationPageFilter(IOptions<ApiBehaviorOptions> options)
    {
        _apiBehaviorOptions = options.Value;
    }

    /// <summary>
    /// 是否是可重复使用的
    /// </summary>
    public static bool IsReusable => true;

    /// <summary>
    /// 模型绑定拦截
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 拦截请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // 排除 WebSocket 请求处理
        if (context.HttpContext.IsWebSocketRequest())
        {
            await next();
            return;
        }

        // 跳过验证类型
        var nonValidationAttributeType = typeof(NonValidationAttribute);
        var method = context.HandlerMethod?.MethodInfo;
        // 处理 Blazor Server
        if (method == null)
        {
            await CallUnHandleResult(context, next);
            return;
        }

        // 获取验证状态
        var modelState = context.ModelState;

        // 如果参数数量为 0 或贴了 [NonValidation] 特性 或所在类型贴了 [NonValidation] 特性或验证成功或已经设置了结果，则跳过验证
        if (context.HandlerArguments.Count == 0 ||
            method.IsDefined(nonValidationAttributeType, true) ||
            method.DeclaringType.IsDefined(nonValidationAttributeType, true) ||
            modelState.IsValid ||
            context.Result != null)
        {
            await CallUnHandleResult(context, next);
            return;
        }

        // 处理执行前验证信息
        var handledResult = HandleValidation(context, modelState);

        // 处理 Mvc 未处理结果情况
        if (!handledResult)
        {
            await CallUnHandleResult(context, next);
        }
    }

    /// <summary>
    /// 调用未处理的结果类型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    private async Task CallUnHandleResult(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // 处理执行后验证信息
        var resultContext = await next();

        // 如果异常不为空且属于友好验证异常
        if (resultContext.Exception != null && resultContext.Exception is AppFriendlyException friendlyException && friendlyException.ValidationException)
        {
            // 存储验证执行结果
            context.HttpContext.Items[FriendlyExceptionKey] = resultContext;

            // 处理验证信息
            _ = HandleValidation(context, friendlyException.ErrorMessage, resultContext, friendlyException);
        }
    }

    /// <summary>
    /// 内部处理异常
    /// </summary>
    /// <param name="context"></param>
    /// <param name="errors"></param>
    /// <param name="resultContext"></param>
    /// <param name="friendlyException"></param>
    /// <returns>返回 false 表示结果没有处理</returns>
    private bool HandleValidation(PageHandlerExecutingContext context, object errors, PageHandlerExecutedContext resultContext = default, AppFriendlyException friendlyException = default)
    {
        // 解析验证消息
        var validationMetadata = ValidatorContext.GetValidationMetadata(errors);
        validationMetadata.ErrorCode = friendlyException?.ErrorCode;
        validationMetadata.OriginErrorCode = friendlyException?.OriginErrorCode;
        validationMetadata.StatusCode = friendlyException?.StatusCode;
        validationMetadata.Data = friendlyException?.Data;

        // 存储验证信息
        context.HttpContext.Items[ValidationMetadataKey] = validationMetadata;

        // 返回自定义错误页面
        IActionResult finalResult = new BadPageResult(StatusCodes.Status400BadRequest)
        {
            Code = validationMetadata.Message
        };

        if (resultContext != null)
            resultContext.Result = finalResult;
        else
            context.Result = finalResult;

        return true;
    }
}