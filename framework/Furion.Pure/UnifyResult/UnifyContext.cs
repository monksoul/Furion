// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.Extensions;
using Furion.FriendlyException;
using Furion.Templates.Extensions;
using Furion.UnifyResult.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;

namespace Furion.UnifyResult
{
    /// <summary>
    /// 规范化结果上下文
    /// </summary>
    [SuppressSniffer]
    public static class UnifyContext
    {
        /// <summary>
        /// 是否启用规范化结果
        /// </summary>
        internal static bool EnabledUnifyHandler = false;

        /// <summary>
        /// 规范化结果类型
        /// </summary>
        internal static Type RESTfulResultType = typeof(RESTfulResult<>);

        /// <summary>
        /// 规范化结果额外数据键
        /// </summary>
        internal static string UnifyResultExtrasKey = "UNIFY_RESULT_EXTRAS";

        /// <summary>
        /// 获取异常元数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ExceptionMetadata GetExceptionMetadata(ExceptionContext context)
        {
            object errorCode = default;
            object errors = default;
            var statusCode = StatusCodes.Status500InternalServerError;
            var isValidationException = false; // 判断是否是验证异常
            var isFriendlyException = false;

            // 判断是否是友好异常
            if (context.Exception is AppFriendlyException friendlyException)
            {
                isFriendlyException = true;
                errorCode = friendlyException.ErrorCode;
                statusCode = friendlyException.StatusCode;
                isValidationException = friendlyException.ValidationException;
                errors = friendlyException.ErrorMessage;
            }

            // 处理验证失败异常
            if (!isValidationException)
            {
                // 判断是否定义了全局类型异常
                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

                // 查找所有全局定义异常
                var ifExceptionAttributes = actionDescriptor.MethodInfo
                            .GetCustomAttributes<IfExceptionAttribute>(true)
                            .Where(u => u.ErrorCode == null);

                // 处理全局异常
                if (ifExceptionAttributes.Any())
                {
                    // 首先判断是否有相同类型的异常
                    var actionIfExceptionAttribute = ifExceptionAttributes.FirstOrDefault(u => u.ExceptionType ==
                                                    (isFriendlyException && context.Exception.InnerException != null
                                                                        ? context.Exception.InnerException.GetType()
                                                                        : context.Exception.GetType()))
                            ?? ifExceptionAttributes.FirstOrDefault(u => u.ExceptionType == null);

                    // 支持渲染配置文件
                    if (actionIfExceptionAttribute is { ErrorMessage: not null }) errors = actionIfExceptionAttribute.ErrorMessage.Render();
                }
                else errors = context.Exception?.InnerException?.Message ?? context.Exception.Message;
            }

            return new ExceptionMetadata
            {
                StatusCode = statusCode,
                ErrorCode = errorCode,
                Errors = errors
            };
        }

        /// <summary>
        /// 填充附加信息
        /// </summary>
        /// <param name="extras"></param>
        public static void Fill(object extras)
        {
            var items = App.HttpContext?.Items;
            if (items.ContainsKey(UnifyResultExtrasKey)) items.Remove(UnifyResultExtrasKey);
            items.Add(UnifyResultExtrasKey, extras);
        }

        /// <summary>
        /// 读取附加信息
        /// </summary>
        public static object Take()
        {
            object extras = null;
            App.HttpContext?.Items?.TryGetValue(UnifyResultExtrasKey, out extras);
            return extras;
        }

        /// <summary>
        /// 设置响应状态码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statusCode"></param>
        /// <param name="unifyResultSettings"></param>
        public static void SetResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings)
        {
            // 篡改响应状态码
            if (unifyResultSettings.AdaptStatusCodes != null && unifyResultSettings.AdaptStatusCodes.Length > 0)
            {
                var adaptStatusCode = unifyResultSettings.AdaptStatusCodes.FirstOrDefault(u => u[0] == statusCode);
                if (adaptStatusCode[0] > 0)
                {
                    context.Response.StatusCode = adaptStatusCode[1];
                    return;
                }
            }

            // 如果为 null，则所有请求错误的状态码设置为 200
            if (unifyResultSettings.Return200StatusCodes == null) context.Response.StatusCode = 200;
            // 否则只有里面的才设置为 200
            else if (unifyResultSettings.Return200StatusCodes.Contains(statusCode)) context.Response.StatusCode = 200;
            else { }
        }

        /// <summary>
        /// 检查请求成功是否进行规范化处理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="unifyResult"></param>
        /// <param name="isWebRequest"></param>
        /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
        internal static bool CheckSucceededNonUnify(MethodInfo method, out IUnifyResultProvider unifyResult, bool isWebRequest = true)
        {
            // 判断是否跳过规范化处理
            var isSkip = !EnabledUnifyHandler
                  || method.GetRealReturnType().HasImplementedRawGeneric(RESTfulResultType)
                  || method.CustomAttributes.Any(x => typeof(NonUnifyAttribute).IsAssignableFrom(x.AttributeType) || typeof(ProducesResponseTypeAttribute).IsAssignableFrom(x.AttributeType) || typeof(IApiResponseMetadataProvider).IsAssignableFrom(x.AttributeType))
                  || method.ReflectedType.IsDefined(typeof(NonUnifyAttribute), true);

            if (!isWebRequest)
            {
                unifyResult = null;
                return isSkip;
            }

            unifyResult = isSkip ? null : App.RootServices.GetService<IUnifyResultProvider>();
            return unifyResult == null || isSkip;
        }

        /// <summary>
        /// 检查请求失败（验证失败、抛异常）是否进行规范化处理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="unifyResult"></param>
        /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
        internal static bool CheckFailedNonUnify(MethodInfo method, out IUnifyResultProvider unifyResult)
        {
            // 判断是否跳过规范化处理
            var isSkip = !EnabledUnifyHandler
                    || method.CustomAttributes.Any(x => typeof(NonUnifyAttribute).IsAssignableFrom(x.AttributeType))
                    || (
                            !method.CustomAttributes.Any(x => typeof(ProducesResponseTypeAttribute).IsAssignableFrom(x.AttributeType) || typeof(IApiResponseMetadataProvider).IsAssignableFrom(x.AttributeType))
                            && method.ReflectedType.IsDefined(typeof(NonUnifyAttribute), true)
                        );

            unifyResult = isSkip ? null : App.RootServices.GetService<IUnifyResultProvider>();
            return unifyResult == null || isSkip;
        }

        /// <summary>
        /// 检查短路状态码（>=400）是否进行规范化处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="unifyResult"></param>
        /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
        internal static bool CheckStatusCodeNonUnify(HttpContext context, out IUnifyResultProvider unifyResult)
        {
            // 获取终点路由特性
            var endpointFeature = context.Features.Get<IEndpointFeature>();
            if (endpointFeature == null) return (unifyResult = null) == null;

            // 判断是否跳过规范化处理
            var isSkip = !EnabledUnifyHandler
                    || context.GetMetadata<NonUnifyAttribute>() != null
                    || endpointFeature?.Endpoint?.Metadata?.GetMetadata<NonUnifyAttribute>() != null;

            unifyResult = isSkip ? null : context.RequestServices.GetService<IUnifyResultProvider>();
            return unifyResult == null || isSkip;
        }

        /// <summary>
        /// 判断是否支持 Mvc 控制器规范化处理
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="actionDescriptor"></param>
        /// <param name="unifyResultSettings"></param>
        /// <returns></returns>
        internal static bool CheckSupportMvcController(HttpContext httpContext, ControllerActionDescriptor actionDescriptor, out UnifyResultSettingsOptions unifyResultSettings)
        {
            // 获取规范化配置选项
            unifyResultSettings = httpContext.RequestServices.GetService<IOptions<UnifyResultSettingsOptions>>()?.Value;

            // 如果未启用 MVC 规范化处理，则跳过
            if (unifyResultSettings?.SupportMvcController == false && typeof(Controller).IsAssignableFrom(actionDescriptor.ControllerTypeInfo)) return false;

            return true;
        }

        /// <summary>
        /// 检查是否是有效的结果（可进行规范化的结果）
        /// </summary>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool CheckVaildResult(IActionResult result, out object data)
        {
            data = default;

            // 排除以下结果，跳过规范化处理
            var isDataResult = result switch
            {
                ViewResult => false,
                PartialViewResult => false,
                FileResult => false,
                ChallengeResult => false,
                SignInResult => false,
                SignOutResult => false,
                RedirectToPageResult => false,
                RedirectToRouteResult => false,
                RedirectResult => false,
                RedirectToActionResult => false,
                LocalRedirectResult => false,
                ForbidResult => false,
                ViewComponentResult => false,
                PageResult => false,
                _ => true,
            };

            // 目前支持返回值 ActionResult
            if (isDataResult) data = result switch
            {
                // 处理内容结果
                ContentResult content => content.Content,
                // 处理对象结果
                ObjectResult obj => obj.Value,
                // 处理 JSON 对象
                JsonResult json => json.Value,
                _ => null,
            };

            return isDataResult;
        }
    }
}