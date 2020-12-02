using Fur.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Fur.UnifyResult
{
    /// <summary>
    /// 规范化结构（请求成功）过滤器
    /// </summary>
    [SkipScan]
    public class SuccessUnifyResultFilter : IAsyncActionFilter, IOrderedFilter
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
            // 排除 Mvc 视图
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (typeof(Controller).IsAssignableFrom(actionDescriptor.ControllerTypeInfo))
            {
                await next();
                return;
            }

            var actionExecutedContext = await next();

            // 如果没有异常再执行
            if (actionExecutedContext.Exception == null && !UnifyResultContext.IsSkipOnSuccessUnifyHandler(actionDescriptor.MethodInfo, out var unifyResult))
            {
                // 处理规范化结果
                if (unifyResult != null && context.Result == null)
                {
                    var result = unifyResult.OnSuccessed(actionExecutedContext);
                    if (result != null) actionExecutedContext.Result = result;
                }
            }
        }
    }
}