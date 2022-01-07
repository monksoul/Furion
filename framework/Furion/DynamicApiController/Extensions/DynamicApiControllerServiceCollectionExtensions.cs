// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion;
using Furion.DependencyInjection;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///动态接口控制器拓展类
    /// </summary>
    [SuppressSniffer]
    public static class DynamicApiControllerServiceCollectionExtensions
    {
        /// <summary>
        /// 添加动态接口控制器服务
        /// </summary>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <returns>Mvc构建器</returns>
        public static IMvcBuilder AddDynamicApiControllers(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.Services.AddDynamicApiControllers();

            return mvcBuilder;
        }

        /// <summary>
        /// 添加动态接口控制器服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns>Mvc构建器</returns>
        public static IServiceCollection AddDynamicApiControllers(this IServiceCollection services)
        {
            var partManager = services.FirstOrDefault(s => s.ServiceType == typeof(ApplicationPartManager))?.ImplementationInstance as ApplicationPartManager
                ?? throw new InvalidOperationException($"`{nameof(AddDynamicApiControllers)}` must be invoked after `{nameof(MvcServiceCollectionExtensions.AddControllers)}`.");

            // 载入模块化/插件程序集部件
            if (App.ExternalAssemblies.Any())
            {
                foreach (var assembly in App.ExternalAssemblies)
                {
                    partManager.ApplicationParts.Add(new AssemblyPart(assembly));
                }
            }

            // 添加控制器特性提供器
            partManager.FeatureProviders.Add(new DynamicApiControllerFeatureProvider());

            // 添加配置
            services.AddConfigurableOptions<DynamicApiControllerSettingsOptions>();

            // 配置 Mvc 选项
            services.Configure<MvcOptions>(options =>
            {
                // 添加应用模型转换器
                options.Conventions.Add(new DynamicApiControllerApplicationModelConvention());
            });

            return services;
        }

        /// <summary>
        /// 添加外部程序集部件集合
        /// </summary>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <param name="assemblies"></param>
        /// <returns>Mvc构建器</returns>
        public static IMvcBuilder AddExternalAssemblyParts(this IMvcBuilder mvcBuilder, IEnumerable<Assembly> assemblies)
        {
            // 载入程序集部件
            if (assemblies != null && assemblies.Any())
            {
                foreach (var assembly in assemblies)
                {
                    mvcBuilder.AddApplicationPart(assembly);
                }
            }

            return mvcBuilder;
        }
    }
}