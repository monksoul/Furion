// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion;
using Furion.DependencyInjection;
using Furion.UnifyResult;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 应用服务集合拓展类（由框架内部调用）
    /// </summary>
    [SuppressSniffer]
    public static class AppServiceCollectionExtensions
    {
        /// <summary>
        /// Mvc 注入基础配置（带Swagger）
        /// </summary>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <param name="configure"></param>
        /// <returns>IMvcBuilder</returns>
        public static IMvcBuilder AddInject(this IMvcBuilder mvcBuilder, Action<InjectServiceOptions> configure = null)
        {
            mvcBuilder.Services.AddInject(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// 服务注入基础配置（带Swagger）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>IMvcBuilder</returns>
        /// <param name="configure"></param>
        public static IServiceCollection AddInject(this IServiceCollection services, Action<InjectServiceOptions> configure = null)
        {
            // 载入服务配置选项
            var configureOptions = new InjectServiceOptions();
            configure?.Invoke(configureOptions);

            services.AddSpecificationDocuments(configureOptions?.SpecificationDocumentConfigure)
                    .AddDynamicApiControllers()
                    .AddDataValidation(configureOptions?.DataValidationConfigure)
                    .AddFriendlyException(configureOptions?.FriendlyExceptionConfigure);

            return services;
        }

        /// <summary>
        /// Mvc 注入基础配置
        /// </summary>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <param name="configure"></param>
        /// <returns>IMvcBuilder</returns>
        public static IMvcBuilder AddInjectBase(this IMvcBuilder mvcBuilder, Action<InjectServiceOptions> configure = null)
        {
            mvcBuilder.Services.AddInjectBase(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// Mvc 注入基础配置
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure"></param>
        /// <returns>IMvcBuilder</returns>
        public static IServiceCollection AddInjectBase(this IServiceCollection services, Action<InjectServiceOptions> configure = null)
        {
            // 载入服务配置选项
            var configureOptions = new InjectServiceOptions();
            configure?.Invoke(configureOptions);

            services.AddDataValidation(configureOptions?.DataValidationConfigure)
                    .AddFriendlyException(configureOptions?.FriendlyExceptionConfigure);

            return services;
        }

        /// <summary>
        /// Mvc 注入基础配置和规范化结果
        /// </summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IMvcBuilder AddInjectWithUnifyResult(this IMvcBuilder mvcBuilder, Action<InjectServiceOptions> configure = null)
        {
            mvcBuilder.Services.AddInjectWithUnifyResult(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// 注入基础配置和规范化结果
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddInjectWithUnifyResult(this IServiceCollection services, Action<InjectServiceOptions> configure = null)
        {
            services.AddInject(configure)
                    .AddUnifyResult();

            return services;
        }

        /// <summary>
        /// Mvc 注入基础配置和规范化结果
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="mvcBuilder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IMvcBuilder AddInjectWithUnifyResult<TUnifyResultProvider>(this IMvcBuilder mvcBuilder, Action<InjectServiceOptions> configure = null)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            mvcBuilder.Services.AddInjectWithUnifyResult<TUnifyResultProvider>(configure);

            return mvcBuilder;
        }

        /// <summary>
        /// Mvc 注入基础配置和规范化结果
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="configure"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInjectWithUnifyResult<TUnifyResultProvider>(this IServiceCollection services, Action<InjectServiceOptions> configure = null)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            services.AddInject(configure)
                    .AddUnifyResult<TUnifyResultProvider>();

            return services;
        }

        /// <summary>
        /// 自动添加主机服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppHostedService(this IServiceCollection services)
        {
            // 获取所有 BackgroundService 类型
            var backgroundServiceTypes = App.EffectiveTypes.Where(u => typeof(BackgroundService).IsAssignableFrom(u));
            var addHostServiceMethod = typeof(ServiceCollectionHostedServiceExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .Where(u => u.Name.Equals("AddHostedService") && u.IsGenericMethod && u.GetParameters().Length == 1)
                                .FirstOrDefault();

            foreach (var type in backgroundServiceTypes)
            {
                addHostServiceMethod.MakeGenericMethod(type).Invoke(null, new object[] { services });
            }

            return services;
        }

        /// <summary>
        /// 供控制台构建根服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static void Build(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider(false);
            // 存储根服务
            InternalApp.RootServices = serviceProvider;
        }

        /// <summary>
        /// 添加应用配置
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">服务配置</param>
        /// <returns>服务集合</returns>
        internal static IServiceCollection AddApp(this IServiceCollection services, Action<IServiceCollection> configure = null)
        {
            // 注册全局配置选项
            services.AddConfigurableOptions<AppSettingsOptions>();

            // 注册内存和分布式内存
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            // 注册全局依赖注入
            services.AddDependencyInjection();

            // 注册全局 Startup 扫描
            services.AddStartups();

            // 添加对象映射
            services.AddObjectMapper();

            // 自定义服务
            configure?.Invoke(services);

            return services;
        }

        /// <summary>
        /// 添加 Startup 自动扫描
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        internal static IServiceCollection AddStartups(this IServiceCollection services)
        {
            // 扫描所有继承 AppStartup 的类
            var startups = App.EffectiveTypes
                .Where(u => typeof(AppStartup).IsAssignableFrom(u) && u.IsClass && !u.IsAbstract && !u.IsGenericType)
                .OrderByDescending(u => GetStartupOrder(u));

            // 注册自定义 startup
            foreach (var type in startups)
            {
                var startup = Activator.CreateInstance(type) as AppStartup;
                App.AppStartups.Add(startup);

                // 获取所有符合依赖注入格式的方法，如返回值void，且第一个参数是 IServiceCollection 类型
                var serviceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(u => u.ReturnType == typeof(void)
                        && u.GetParameters().Length > 0
                        && u.GetParameters().First().ParameterType == typeof(IServiceCollection));

                if (!serviceMethods.Any()) continue;

                // 自动安装属性调用
                foreach (var method in serviceMethods)
                {
                    method.Invoke(startup, new[] { services });
                }
            }

            return services;
        }

        /// <summary>
        /// 获取 Startup 排序
        /// </summary>
        /// <param name="type">排序类型</param>
        /// <returns>int</returns>
        private static int GetStartupOrder(Type type)
        {
            return !type.IsDefined(typeof(AppStartupAttribute), true) ? 0 : type.GetCustomAttribute<AppStartupAttribute>(true).Order;
        }
    }
}