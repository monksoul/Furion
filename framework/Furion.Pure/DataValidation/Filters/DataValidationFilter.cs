// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.DynamicApiController;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Furion.DataValidation
{
    /// <summary>
    /// 数据验证控制器
    /// </summary>
    [SuppressSniffer]
    public sealed class DataValidationFilter : IActionFilter, IOrderedFilter
    {
        /// <summary>
        /// Api 行为配置选项
        /// </summary>
        private readonly ApiBehaviorOptions _apiBehaviorOptions;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public DataValidationFilter(IOptions<ApiBehaviorOptions> options)
        {
            _apiBehaviorOptions = options.Value;
        }

        /// <summary>
        /// 过滤器排序
        /// </summary>
        internal const int FilterOrder = -1000;

        /// <summary>
        /// 排序属性
        /// </summary>
        public int Order => FilterOrder;

        /// <summary>
        /// 是否是可重复使用的
        /// </summary>
        public static bool IsReusable => true;

        /// <summary>
        /// 动作方法执行之前操作
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 获取控制器/方法信息
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            // 跳过验证类型
            var nonValidationAttributeType = typeof(NonValidationAttribute);
            var method = actionDescriptor.MethodInfo;

            // 如果参数为 0或贴了 [NonValidation] 特性 或所在类型贴了 [NonValidation] 特性，则跳过验证
            if (actionDescriptor.Parameters.Count == 0 ||
                method.IsDefined(nonValidationAttributeType, true) ||
                method.DeclaringType.IsDefined(nonValidationAttributeType, true)) return;

            // 获取验证状态
            var modelState = context.ModelState;

            // 判断是否验证成功
            if (modelState.IsValid) return;

            // 如果其他过滤器已经设置了结果，那么跳过
            if (context.Result != null) return;

            // 解析验证消息
            var validationMetadata = ValidatorContext.GetValidationMetadata(modelState);

            // 判断是否跳过规范化结果，如果跳过，返回 400 BadRequestResult
            if (UnifyContext.CheckFailedNonUnify(actionDescriptor.MethodInfo, out var unifyResult))
            {
                if (!Penetrates.IsApiController(method.DeclaringType))
                {
                    context.Result = new BadRequestResult();
                }
                else context.Result = _apiBehaviorOptions.InvalidModelStateResponseFactory(context);
            }
            else
            {
                // 判断是否支持 MVC 规范化处理
                if (!UnifyContext.CheckSupportMvcController(context.HttpContext, actionDescriptor, out _)) return;

                context.Result = unifyResult.OnValidateFailed(context, validationMetadata);
            }

            // 打印验证失败信息
            App.PrintToMiniProfiler("validation", "Failed", $"Validation Failed:\r\n{validationMetadata.Message}", true);
        }

        /// <summary>
        /// 动作方法执行完成操作
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}