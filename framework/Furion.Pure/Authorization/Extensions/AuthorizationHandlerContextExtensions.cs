// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// 授权处理上下文拓展类
    /// </summary>
    [SuppressSniffer]
    public static class AuthorizationHandlerContextExtensions
    {
        /// <summary>
        /// 获取当前 HttpContext 上下文
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DefaultHttpContext GetCurrentHttpContext(this AuthorizationHandlerContext context)
        {
            DefaultHttpContext httpContext;

            // 获取 httpContext 对象
            if (context.Resource is AuthorizationFilterContext filterContext) httpContext = (DefaultHttpContext)filterContext.HttpContext;
            else if (context.Resource is DefaultHttpContext defaultHttpContext) httpContext = defaultHttpContext;
            else httpContext = null;

            return httpContext;
        }
    }
}