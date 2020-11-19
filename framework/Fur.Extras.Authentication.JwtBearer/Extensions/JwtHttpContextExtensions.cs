namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// HttpContext 拓展类
    /// </summary>
    public static class JwtHttpContextExtensions
    {
        /// <summary>
        /// 获取 JWT Token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetJwtToken(this HttpContext httpContext)
        {
            // 判断请求报文头中是否有 "Authorization" 报文头
            var bearerToken = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(bearerToken)) return default;

            return bearerToken[7..];
        }

        /// <summary>
        /// 获取 JWT Token
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <returns></returns>
        public static string GetJwtToken(this IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext.GetJwtToken();
        }
    }
}