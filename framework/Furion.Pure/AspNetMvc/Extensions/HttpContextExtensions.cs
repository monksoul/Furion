// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;
using System.Text;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Http 拓展类
    /// </summary>
    [SuppressSniffer]
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
        /// 获取 控制器/Action 描述器
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static ControllerActionDescriptor GetControllerActionDescriptor(this HttpContext httpContext)
        {
            return httpContext.GetEndpoint()?.Metadata?.FirstOrDefault(u => u is ControllerActionDescriptor) as ControllerActionDescriptor;
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
        /// 设置规范化文档退出登录
        /// </summary>
        /// <param name="httpContext"></param>
        public static void SignoutToSwagger(this HttpContext httpContext)
        {
            httpContext.Response.Headers["access-token"] = "invalid_token";
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
        /// 获取完整请求地址
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetRequestUrlAddress(this HttpRequest request)
        {
            return new StringBuilder()
                    .Append(request.Scheme)
                    .Append("://")
                    .Append(request.Host)
                    .Append(request.PathBase)
                    .Append(request.Path)
                    .Append(request.QueryString)
                    .ToString();
        }

        /// <summary>
        /// 获取来源地址
        /// </summary>
        /// <param name="request"></param>
        /// <param name="refererHeaderKey"></param>
        /// <returns></returns>
        public static string GetRefererUrlAddress(this HttpRequest request, string refererHeaderKey = "Referer")
        {
            return request.Headers[refererHeaderKey].ToString();
        }
    }
}