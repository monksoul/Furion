using Fur.DependencyInjection;
using Fur.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 友好异常服务拓展类
    /// </summary>
    [SkipScan]
    public static class FriendlyExceptionServiceCollectionExtensions
    {
        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <typeparam name="TErrorCodeTypeProvider">异常错误码提供器</typeparam>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <param name="enabledGlobalExceptionFilter">是否启用全局异常过滤器</param>
        /// <returns></returns>
        public static IMvcBuilder AddFriendlyException<TErrorCodeTypeProvider>(this IMvcBuilder mvcBuilder, bool enabledGlobalExceptionFilter = true)
            where TErrorCodeTypeProvider : class, IErrorCodeTypeProvider
        {
            var services = mvcBuilder.Services;

            // 添加全局异常过滤器
            mvcBuilder.AddFriendlyException(enabledGlobalExceptionFilter);

            // 单例注册异常状态码提供器
            services.AddSingleton<IErrorCodeTypeProvider, TErrorCodeTypeProvider>();

            return mvcBuilder;
        }

        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <typeparam name="TErrorCodeTypeProvider">异常错误码提供器</typeparam>
        /// <param name="services"></param>
        /// <param name="enabledGlobalExceptionFilter">是否启用全局异常过滤器</param>
        /// <returns></returns>
        public static IServiceCollection AddFriendlyException<TErrorCodeTypeProvider>(this IServiceCollection services, bool enabledGlobalExceptionFilter = true)
            where TErrorCodeTypeProvider : class, IErrorCodeTypeProvider
        {
            // 添加全局异常过滤器
            services.AddFriendlyException(enabledGlobalExceptionFilter);

            // 单例注册异常状态码提供器
            services.AddSingleton<IErrorCodeTypeProvider, TErrorCodeTypeProvider>();

            return services;
        }

        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <param name="mvcBuilder">Mvc构建器</param>
        /// <param name="enabledGlobalExceptionFilter">是否启用全局异常过滤器</param>
        /// <returns></returns>
        public static IMvcBuilder AddFriendlyException(this IMvcBuilder mvcBuilder, bool enabledGlobalExceptionFilter = true)
        {
            // 新增基础配置
            AddBaseConfigure(mvcBuilder.Services);

            // 添加全局异常过滤器
            if (enabledGlobalExceptionFilter)
                mvcBuilder.AddMvcOptions(options => options.Filters.Add<FriendlyExceptionFilter>());

            return mvcBuilder;
        }

        /// <summary>
        /// 添加友好异常服务拓展服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="enabledGlobalExceptionFilter">是否启用全局异常过滤器</param>
        /// <returns></returns>
        public static IServiceCollection AddFriendlyException(this IServiceCollection services, bool enabledGlobalExceptionFilter = true)
        {
            // 新增基础配置
            AddBaseConfigure(services);

            // 添加全局异常过滤器
            if (enabledGlobalExceptionFilter)
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add<FriendlyExceptionFilter>();
                });
            }

            return services;
        }

        /// <summary>
        /// 新增基础配置
        /// </summary>
        /// <param name="services"></param>
        private static void AddBaseConfigure(IServiceCollection services)
        {
            // 添加友好异常配置文件支持
            services.AddConfigurableOptions<FriendlyExceptionSettingsOptions>();

            // 添加异常配置文件支持
            services.AddConfigurableOptions<ErrorCodeMessageSettingsOptions>();
        }
    }
}