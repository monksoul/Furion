// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.RemoteRequest;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 远程请求服务拓展类
    /// </summary>
    [SuppressSniffer]
    public static class RemoteRequestServiceCollectionExtensions
    {
        /// <summary>
        /// 注册远程请求
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <param name="inludeDefaultHttpClient">是否包含默认客户端</param>
        /// <returns></returns>
        public static IServiceCollection AddRemoteRequest(this IServiceCollection services, Action<IServiceCollection> configure = null, bool inludeDefaultHttpClient = true)
        {
            // 注册远程请求代理接口
            services.AddScopedDispatchProxyForInterface<HttpDispatchProxy, IHttpDispatchProxy>();

            // 注册默认请求客户端
            if (inludeDefaultHttpClient) services.AddHttpClient();

            // 注册其他客户端
            configure?.Invoke(services);

            return services;
        }
    }
}