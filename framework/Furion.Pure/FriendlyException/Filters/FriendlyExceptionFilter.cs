// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion;
using Furion.DataValidation;
using Furion.DependencyInjection;
using Furion.FriendlyException;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// 友好异常拦截器
    /// </summary>
    [SuppressSniffer]
    public sealed class FriendlyExceptionFilter : IAsyncExceptionFilter
    {
        /// <summary>
        /// 异常拦截
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            // 解析异常处理服务，实现自定义异常额外操作，如记录日志等
            var globalExceptionHandler = context.HttpContext.RequestServices.GetService<IGlobalExceptionHandler>();
            if (globalExceptionHandler != null)
            {
                await globalExceptionHandler.OnExceptionAsync(context);
            }

            // 如果异常在其他地方被标记了处理，那么这里不再处理
            if (context.ExceptionHandled) return;

            // 获取控制器信息
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            // 解析异常信息
            var exceptionMetadata = UnifyContext.GetExceptionMetadata(context);

            // 判断是否是验证异常
            var isValidationException = context.Exception is AppFriendlyException friendlyException && friendlyException.ValidationException;

            // 判断是否跳过规范化结果，如果是，则只处理为友好异常消息
            if (UnifyContext.CheckFailedNonUnify(actionDescriptor.MethodInfo, out var unifyResult))
            {
                // 如果是验证异常，返回 400
                if (isValidationException) context.Result = new BadRequestResult();
                else
                {
                    // 返回友好异常
                    context.Result = new ContentResult()
                    {
                        Content = exceptionMetadata.Errors.ToString(),
                        StatusCode = exceptionMetadata.StatusCode
                    };
                }
            }
            else
            {
                // 判断是否支持 MVC 规范化处理
                if (!UnifyContext.CheckSupportMvcController(context.HttpContext, actionDescriptor, out _)) return;

                // 执行规范化异常处理
                context.Result = unifyResult.OnException(context, exceptionMetadata);
            }

            // 判断异常消息是否是验证异常（比如数据验证异常，业务抛出异常）
            if (isValidationException)
            {
                // 解析验证消息
                var validationMetadata = ValidatorContext.GetValidationMetadata((context.Exception as AppFriendlyException).ErrorMessage);

                App.PrintToMiniProfiler("Validation", "Failed", $"Validation Failed:\r\n{validationMetadata.Message}", true);
            }
            else PrintToMiniProfiler(context.Exception);
        }

        /// <summary>
        /// 打印错误到 MiniProfiler 中
        /// </summary>
        /// <param name="exception"></param>
        internal static void PrintToMiniProfiler(Exception exception)
        {
            // 判断是否注入 MiniProfiler 组件
            if (App.Settings.InjectMiniProfiler != true) return;

            // 获取异常堆栈
            var traceFrame = new StackTrace(exception, true).GetFrame(0);

            // 获取出错的文件名
            var exceptionFileName = traceFrame.GetFileName();

            // 获取出错的行号
            var exceptionFileLineNumber = traceFrame.GetFileLineNumber();

            // 打印错误文件名和行号
            if (!string.IsNullOrWhiteSpace(exceptionFileName) && exceptionFileLineNumber > 0)
            {
                App.PrintToMiniProfiler("errors", "Locator", $"{exceptionFileName}:line {exceptionFileLineNumber}", true);
            }

            // 打印完整的堆栈信息
            App.PrintToMiniProfiler("errors", "StackTrace", exception.ToString(), true);
        }
    }
}