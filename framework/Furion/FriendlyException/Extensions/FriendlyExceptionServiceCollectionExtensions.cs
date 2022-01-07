// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 友好异常服务拓展类
    /// </summary>
    [SuppressSniffer]
    public static class FriendlyExceptionServiceCollectionExtensions
    {
        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <typeparam name="TErrorCodeTypeProvider">异常错误码提供器</typeparam>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <param name="configure">是否启用全局异常过滤器</param>
        /// <returns></returns>
        public static IMvcBuilder AddFriendlyException<TErrorCodeTypeProvider>(this IMvcBuilder mvcBuilder, Action<FriendlyExceptionServiceOptions> configure = null)
            where TErrorCodeTypeProvider : class, IErrorCodeTypeProvider
        {
            mvcBuilder.Services.AddFriendlyException<TErrorCodeTypeProvider>(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <typeparam name="TErrorCodeTypeProvider">异常错误码提供器</typeparam>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddFriendlyException<TErrorCodeTypeProvider>(this IServiceCollection services, Action<FriendlyExceptionServiceOptions> configure = null)
            where TErrorCodeTypeProvider : class, IErrorCodeTypeProvider
        {
            // 添加全局异常过滤器
            services.AddFriendlyException(configure);

            // 单例注册异常状态码提供器
            services.AddSingleton<IErrorCodeTypeProvider, TErrorCodeTypeProvider>();

            return services;
        }

        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IMvcBuilder AddFriendlyException(this IMvcBuilder mvcBuilder, Action<FriendlyExceptionServiceOptions> configure = null)
        {
            mvcBuilder.Services.AddFriendlyException(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddFriendlyException(this IServiceCollection services, Action<FriendlyExceptionServiceOptions> configure = null)
        {
            // 添加友好异常配置文件支持
            services.AddConfigurableOptions<FriendlyExceptionSettingsOptions>();

            // 添加异常配置文件支持
            services.AddConfigurableOptions<ErrorCodeMessageSettingsOptions>();

            // 载入服务配置选项
            var configureOptions = new FriendlyExceptionServiceOptions();
            configure?.Invoke(configureOptions);

            // 添加全局异常过滤器
            if (configureOptions.EnabledGlobalFriendlyException)
                services.AddMvcFilter<FriendlyExceptionFilter>();

            return services;
        }
    }
}