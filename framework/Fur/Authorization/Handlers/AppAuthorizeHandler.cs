using Fur.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Fur.Authorization
{
    /// <summary>
    /// 授权策略执行程序
    /// </summary>
    [SkipScan]
    public abstract class AppAuthorizeHandler : IAuthorizationHandler
    {
        /// <summary>
        /// 授权验证核心方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            // 判断是否授权
            var isAuthenticated = context.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                // 获取所有未成功验证的需求
                var pendingRequirements = context.PendingRequirements;

                DefaultHttpContext httpContext;

                // 获取 httpContext 对象
                if (context.Resource is AuthorizationFilterContext filterContext) httpContext = (DefaultHttpContext)filterContext.HttpContext;
                else if (context.Resource is DefaultHttpContext defaultHttpContext) httpContext = defaultHttpContext;
                else httpContext = null;

                // 调用子类管道
                var pipeline = Pipeline(context, httpContext);
                if (!pipeline) context.Fail();
                else
                {
                    // 通过授权验证
                    foreach (var requirement in pendingRequirements)
                    {
                        if (requirement is AppAuthorizeRequirement authorizeRequirement)
                        {
                            // 验证策略管道
                            var policyPipeline = PolicyPipeline(context, httpContext, authorizeRequirement);

                            if (!policyPipeline) context.Fail();
                            else context.Succeed(requirement);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 验证管道
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual bool Pipeline(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            return true;
        }

        /// <summary>
        /// 策略验证管道
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        public virtual bool PolicyPipeline(AuthorizationHandlerContext context, DefaultHttpContext httpContext, AppAuthorizeRequirement requirement)
        {
            return true;
        }
    }
}