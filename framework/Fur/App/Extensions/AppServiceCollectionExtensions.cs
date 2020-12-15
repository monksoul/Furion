using Fur;
using Fur.DependencyInjection;
using Fur.UnifyResult;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 应用服务集合拓展类（由框架内部调用）
    /// </summary>
    [SkipScan]
    public static class AppServiceCollectionExtensions
    {
        /// <summary>
        /// MiniProfiler 插件路径
        /// </summary>
        private const string MiniProfilerRouteBasePath = "/index-mini-profiler";

        /// <summary>
        /// Mvc 注入基础配置（带Swagger）
        /// </summary>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <returns>IMvcBuilder</returns>
        public static IMvcBuilder AddInject(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddSpecificationDocuments()
                      .AddDynamicApiControllers()
                      .AddDataValidation()
                      .AddFriendlyException();

            return mvcBuilder;
        }

        /// <summary>
        /// 服务注入基础配置（带Swagger）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>IMvcBuilder</returns>
        public static IServiceCollection AddInject(this IServiceCollection services)
        {
            services.AddSpecificationDocuments()
                    .AddDynamicApiControllers()
                    .AddDataValidation()
                    .AddFriendlyException();

            return services;
        }

        /// <summary>
        /// Mvc 注入基础配置
        /// </summary>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <param name="includeDynamicApiController"></param>
        /// <returns>IMvcBuilder</returns>
        public static IMvcBuilder AddInjectBase(this IMvcBuilder mvcBuilder, bool includeDynamicApiController = true)
        {
            if (includeDynamicApiController) mvcBuilder.AddDynamicApiControllers();

            mvcBuilder.AddDataValidation()
                      .AddFriendlyException();

            return mvcBuilder;
        }

        /// <summary>
        /// Mvc 注入基础配置
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="includeDynamicApiController"></param>
        /// <returns>IMvcBuilder</returns>
        public static IServiceCollection AddInjectBase(this IServiceCollection services, bool includeDynamicApiController = true)
        {
            if (includeDynamicApiController) services.AddDynamicApiControllers();

            services.AddDataValidation()
                    .AddFriendlyException();

            return services;
        }

        /// <summary>
        /// Mvc 注入基础配置和规范化结果
        /// </summary>
        /// <param name="mvcBuilder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddInjectWithUnifyResult(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddInject()
                      .AddUnifyResult();

            return mvcBuilder;
        }

        /// <summary>
        /// 注入基础配置和规范化结果
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInjectWithUnifyResult(this IServiceCollection services)
        {
            services.AddInject()
                    .AddUnifyResult();

            return services;
        }

        /// <summary>
        /// Mvc 注入基础配置和规范化结果
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="mvcBuilder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddInjectWithUnifyResult<TUnifyResultProvider>(this IMvcBuilder mvcBuilder)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            mvcBuilder.AddInject()
                      .AddUnifyResult<TUnifyResultProvider>();

            return mvcBuilder;
        }

        /// <summary>
        /// Mvc 注入基础配置和规范化结果
        /// </summary>
        /// <typeparam name="TUnifyResultProvider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInjectWithUnifyResult<TUnifyResultProvider>(this IServiceCollection services)
            where TUnifyResultProvider : class, IUnifyResultProvider
        {
            services.AddInject()
                    .AddUnifyResult<TUnifyResultProvider>();

            return services;
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

            // 添加 HttContext 访问器
            services.AddHttpContextAccessor();

            // 注册分布式内存缓存
            services.AddDistributedMemoryCache();

            // 注册全局 Startup 扫描
            services.AddStartup();

            // 注册MiniProfiler 组件
            if (App.Settings.InjectMiniProfiler == true)
            {
                services.AddMiniProfiler(options =>
                {
                    options.RouteBasePath = MiniProfilerRouteBasePath;
                }).AddEntityFramework();
            }

            // 注册全局依赖注入
            services.AddDependencyInjection();

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
        internal static IServiceCollection AddStartup(this IServiceCollection services)
        {
            // 扫描所有继承 AppStartup 的类
            var startups = App.CanBeScanTypes
                .Where(u => typeof(AppStartup).IsAssignableFrom(u) && u.IsClass && !u.IsAbstract && !u.IsGenericType)
                .OrderByDescending(u => GetOrder(u));

            // 注册自定义 starup
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
        private static int GetOrder(Type type)
        {
            return !type.IsDefined(typeof(AppStartupAttribute), true)
                ? 0
                : type.GetCustomAttribute<AppStartupAttribute>(true).Order;
        }
    }
}