// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.JsonSerialization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Json 序列化服务拓展类
    /// </summary>
    [SuppressSniffer]
    public static class JsonSerializationServiceCollectionExtensions
    {
        /// <summary>
        /// 配置 Json 序列化提供器
        /// </summary>
        /// <typeparam name="TJsonSerializerProvider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonSerialization<TJsonSerializerProvider>(this IServiceCollection services)
            where TJsonSerializerProvider : class, IJsonSerializerProvider
        {
            services.AddSingleton<IJsonSerializerProvider, TJsonSerializerProvider>();
            return services;
        }

        /// <summary>
        /// 配置 JsonOptions 序列化选项
        /// <para>主要给非 Web 环境使用</para>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonOptions(this IServiceCollection services, Action<JsonOptions> configure)
        {
            // 手动添加配置
            services.Configure<JsonOptions>(options =>
            {
                configure?.Invoke(options);
            });

            return services;
        }
    }
}