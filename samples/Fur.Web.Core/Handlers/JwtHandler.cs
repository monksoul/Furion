using Fur.Authorization;
using Fur.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fur.Web.Core
{
    /// <summary>
    /// JWT 授权自定义处理程序
    /// </summary>
    public class JwtHandler : AppAuthorizeHandler
    {
        /// <summary>
        /// 验证管道
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public override bool Pipeline(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {
            // 检查权限
            return CheckAuthorzie(httpContext);
        }

        /// <summary>
        /// 检查权限
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private static bool CheckAuthorzie(DefaultHttpContext httpContext)
        {
            // 获取权限特性
            var securityDefineAttribute = httpContext.GetMetadata<SecurityDefineAttribute>();
            if (securityDefineAttribute == null) return true;

            // 解析服务
            var authorizationManager = httpContext.RequestServices.GetService<IAuthorizationManager>();

            // 检查授权
            return authorizationManager.CheckSecurity(securityDefineAttribute.ResourceId);
        }
    }
}