// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.SensitiveDetection;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 脱敏词汇处理服务
    /// </summary>
    [SuppressSniffer]
    public static class SensitiveDetectionServiceCollectionExtensions
    {
        /// <summary>
        /// 添加脱敏词汇服务
        /// <para>需要在入口程序集目录下创建 sensitive-words.txt</para>
        /// </summary>
        /// <param name="mvcBuilder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddSensitiveDetection(this IMvcBuilder mvcBuilder)
        {
            var services = mvcBuilder.Services;
            services.AddSensitiveDetection();

            return mvcBuilder;
        }

        /// <summary>
        /// 添加脱敏词汇服务
        /// </summary>
        /// <typeparam name="TSensitiveDetectionProvider"></typeparam>
        /// <param name="mvcBuilder"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static IMvcBuilder AddSensitiveDetection<TSensitiveDetectionProvider>(this IMvcBuilder mvcBuilder, Action<IServiceCollection> handle = default)
            where TSensitiveDetectionProvider : class, ISensitiveDetectionProvider
        {
            var services = mvcBuilder.Services;

            // 注册脱敏词汇服务
            services.AddSensitiveDetection<TSensitiveDetectionProvider>(handle);

            return mvcBuilder;
        }

        /// <summary>
        /// 添加脱敏词汇服务
        /// <para>需要在入口程序集目录下创建 sensitive-words.txt</para>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSensitiveDetection(this IServiceCollection services)
        {
            return services.AddSensitiveDetection<SensitiveDetectionProvider>();
        }

        /// <summary>
        /// 添加脱敏词汇服务
        /// </summary>
        /// <typeparam name="TSensitiveDetectionProvider"></typeparam>
        /// <param name="services"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static IServiceCollection AddSensitiveDetection<TSensitiveDetectionProvider>(this IServiceCollection services, Action<IServiceCollection> handle = default)
            where TSensitiveDetectionProvider : class, ISensitiveDetectionProvider
        {
            // 注册脱敏词汇服务
            services.AddSingleton<ISensitiveDetectionProvider, TSensitiveDetectionProvider>();

            // 自定义配置
            handle?.Invoke(services);

            return services;
        }
    }
}