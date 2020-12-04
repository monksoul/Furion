using Fur.DependencyInjection;

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
        /// 获取本机 IPv4地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetLocalIpAddressToIPv4(this HttpContext context)
        {
            return context.Connection.LocalIpAddress?.MapToIPv4()?.ToString();
        }

        /// <summary>
        /// 获取本机 IPv6地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetLocalIpAddressToIPv6(this HttpContext context)
        {
            return context.Connection.LocalIpAddress?.MapToIPv6()?.ToString();
        }

        /// <summary>
        /// 获取远程 IPv4地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRemoteIpAddressToIPv4(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();
        }

        /// <summary>
        /// 获取远程 IPv6地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRemoteIpAddressToIPv6(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.MapToIPv6()?.ToString();
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