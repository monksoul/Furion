using Fur.DataValidation;
using Fur.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 友好异常服务拓展类
    /// </summary>
    [SkipScan]
    public static class DataValidationServiceCollectionExtensions
    {
        /// <summary>
        /// 添加全局数据验证
        /// </summary>
        /// <typeparam name="TValidationMessageTypeProvider">验证类型消息提供器</typeparam>
        /// <param name="mvcBuilder"></param>
        /// <param name="enabledGlobalDataValidationFilter">启用全局验证过滤器</param>
        /// <param name="suppressImplicitRequiredAttributeForNonNullableReferenceTypes">禁止C# 8.0 验证非可空引用类型</param>
        /// <returns></returns>
        public static IMvcBuilder AddDataValidation<TValidationMessageTypeProvider>(this IMvcBuilder mvcBuilder, bool enabledGlobalDataValidationFilter = true, bool suppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            where TValidationMessageTypeProvider : class, IValidationMessageTypeProvider
        {
            var services = mvcBuilder.Services;

            // 添加全局数据验证
            mvcBuilder.AddDataValidation(enabledGlobalDataValidationFilter, suppressImplicitRequiredAttributeForNonNullableReferenceTypes);

            // 单例注册验证消息提供器
            services.AddSingleton<IValidationMessageTypeProvider, TValidationMessageTypeProvider>();

            return mvcBuilder;
        }

        /// <summary>
        /// 添加全局数据验证
        /// </summary>
        /// <typeparam name="TValidationMessageTypeProvider">验证类型消息提供器</typeparam>
        /// <param name="services"></param>
        /// <param name="enabledGlobalDataValidationFilter">启用全局验证过滤器</param>
        /// <param name="suppressImplicitRequiredAttributeForNonNullableReferenceTypes">禁止C# 8.0 验证非可空引用类型</param>
        /// <returns></returns>
        public static IServiceCollection AddDataValidation<TValidationMessageTypeProvider>(this IServiceCollection services, bool enabledGlobalDataValidationFilter = true, bool suppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            where TValidationMessageTypeProvider : class, IValidationMessageTypeProvider
        {
            // 添加全局数据验证
            services.AddDataValidation(enabledGlobalDataValidationFilter, suppressImplicitRequiredAttributeForNonNullableReferenceTypes);

            // 单例注册验证消息提供器
            services.AddSingleton<IValidationMessageTypeProvider, TValidationMessageTypeProvider>();

            return services;
        }

        /// <summary>
        /// 添加全局数据验证
        /// </summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="enabledGlobalDataValidationFilter">启用全局验证过滤器</param>
        /// <param name="suppressImplicitRequiredAttributeForNonNullableReferenceTypes">禁止C# 8.0 验证非可空引用类型</param>
        /// <returns></returns>
        public static IMvcBuilder AddDataValidation(this IMvcBuilder mvcBuilder, bool enabledGlobalDataValidationFilter = true, bool suppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
        {
            var services = mvcBuilder.Services;

            // 添加验证配置文件支持
            services.AddConfigurableOptions<ValidationTypeMessageSettingsOptions>();

            // 判断是否启用全局
            if (enabledGlobalDataValidationFilter)
            {
                // 使用自定义验证
                mvcBuilder.ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

                // 添加全局数据验证
                mvcBuilder.AddMvcOptions(options =>
                {
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = suppressImplicitRequiredAttributeForNonNullableReferenceTypes;
                    options.Filters.Add<DataValidationFilter>();
                });
            }

            return mvcBuilder;
        }

        /// <summary>
        /// 添加全局数据验证
        /// </summary>
        /// <param name="services"></param>
        /// <param name="enabledGlobalDataValidationFilter">启用全局验证过滤器</param>
        /// <param name="suppressImplicitRequiredAttributeForNonNullableReferenceTypes">禁止C# 8.0 验证非可空引用类型</param>
        /// <returns></returns>
        public static IServiceCollection AddDataValidation(this IServiceCollection services, bool enabledGlobalDataValidationFilter = true, bool suppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
        {
            // 添加验证配置文件支持
            services.AddConfigurableOptions<ValidationTypeMessageSettingsOptions>();

            // 判断是否启用全局
            if (enabledGlobalDataValidationFilter)
            {
                // 添加自定义验证
                services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

                // 添加全局验证
                services.Configure<MvcOptions>(options =>
                {
                    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = suppressImplicitRequiredAttributeForNonNullableReferenceTypes;
                    options.Filters.Add<DataValidationFilter>();
                });
            }

            return services;
        }
    }
}