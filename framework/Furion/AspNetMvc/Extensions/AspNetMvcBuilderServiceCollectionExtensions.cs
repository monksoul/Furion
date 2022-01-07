// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion;
using Furion.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// ASP.NET Mvc 服务拓展类
    /// </summary>
    [SuppressSniffer]
    public static class AspNetMvcBuilderServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 Mvc 过滤器
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="mvcBuilder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IMvcBuilder AddMvcFilter<TFilter>(this IMvcBuilder mvcBuilder, Action<MvcOptions> configure = default)
            where TFilter : IFilterMetadata
        {
            mvcBuilder.Services.AddMvcFilter<TFilter>(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// 注册 Mvc 过滤器
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddMvcFilter<TFilter>(this IServiceCollection services, Action<MvcOptions> configure = default)
            where TFilter : IFilterMetadata
        {
            // 非 Web 环境跳过注册
            if (App.WebHostEnvironment == default) return services;

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<TFilter>();

                // 其他额外配置
                configure?.Invoke(options);
            });

            return services;
        }
    }
}