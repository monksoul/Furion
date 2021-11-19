// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using Furion.DataValidation;
using Furion.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Furion.UnifyResult
{
    /// <summary>
    /// 规范化结构（请求成功）过滤器
    /// </summary>
    [SuppressSniffer]
    public class SucceededUnifyResultFilter : IAsyncActionFilter, IOrderedFilter
    {
        /// <summary>
        /// 过滤器排序
        /// </summary>
        internal const int FilterOrder = 8888;

        /// <summary>
        /// 排序属性
        /// </summary>
        public int Order => FilterOrder;

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

            // 如果出现异常，则不会进入该过滤器
            if (actionExecutedContext.Exception != null) return;

            // 获取控制器信息
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            // 判断是否支持 MVC 规范化处理
            if (!UnifyContext.CheckSupportMvcController(context.HttpContext, actionDescriptor, out _)) return;

            // 判断是否跳过规范化处理
            if (UnifyContext.CheckSucceededNonUnify(actionDescriptor.MethodInfo, out var unifyResult)) return;

            // 处理 BadRequestObjectResult 类型规范化处理
            if (actionExecutedContext.Result is BadRequestObjectResult badRequestObjectResult)
            {
                // 解析验证消息
                var validationMetadata = ValidatorContext.GetValidationMetadata(badRequestObjectResult.Value);

                var result = unifyResult.OnValidateFailed(context, validationMetadata);
                if (result != null) actionExecutedContext.Result = result;

                // 打印验证失败信息
                App.PrintToMiniProfiler("validation", "Failed", $"Validation Failed:\r\n{validationMetadata.Message}", true);
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
}