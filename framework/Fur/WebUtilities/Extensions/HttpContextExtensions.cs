using Fur.DependencyInjection;
using System.Linq;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Http 拓展类
    /// </summary>
    [SkipScan]
    public static class HttpContextExtensions
    {
        /// <summary>
        /// 获取 Action 特性
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static TAttribute GetMetadata<TAttribute>(this HttpContext httpContext)
            where TAttribute : class
        {
            return httpContext.GetEndpoint()?.Metadata?.GetMetadata<TAttribute>();
        }

        /// <summary>
        /// 设置规范化文档自动登录
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="accessToken"></param>
        public static void SigninToSwagger(this HttpContext httpContext, string accessToken)
        {
            // 设置 Swagger 刷新自动授权
            httpContext.Response.Headers["access-token"] = accessToken;
        }

        /// <summary>
        /// 设置规范化文档自动登录
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="accessToken"></param>
        public static void SigninToSwagger(this IHttpContextAccessor httpContextAccessor, string accessToken)
        {
            httpContextAccessor.HttpContext.SigninToSwagger(accessToken);
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetClientIPAddress(this HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }
            return ip;
        }

        /// <summary>
        /// 判断是否是 Ajax 请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            var result = false;

            if (request.Headers.ContainsKey("x-requested-with"))
            {
                result = request.Headers["x-requested-with"] == "XMLHttpRequest";
            }

            return result;
        }
    }
}