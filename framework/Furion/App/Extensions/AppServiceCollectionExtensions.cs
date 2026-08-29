// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

using Furion;
using Furion.DistributedIDGenerator;
using Furion.JsonSerialization;
using Furion.UnifyResult;
using Furion.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 应用服务集合扩展类（由框架内部调用）
/// </summary>
public static class AppServiceCollectionExtensions
{
    /// <summary>
    /// AddHostedService 泛型方法缓存
    /// </summary>
    private static readonly MethodInfo AddHostedServiceMethodInfo = typeof(ServiceCollectionHostedServiceExtensions)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .FirstOrDefault(u => u.Name.Equals("AddHostedService") && u.IsGenericMethod && u.GetParameters().Length == 1);

    /// <summary>
    /// Mvc 注入基础配置（带Swagger）
    /// </summary>
    /// <param name="mvcBuilder">Mvc构建器</param>
    /// <param name="configure"></param>
    /// <returns>IMvcBuilder</returns>
    public static IMvcBuilder AddInject(this IMvcBuilder mvcBuilder, Action<AddInjectOptions> configure = null)
    {
        mvcBuilder.Services.AddInject(configure);

        return mvcBuilder;
    }

    /// <summary>
    /// 服务注入基础配置（带Swagger）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure"></param>
    /// <returns>IMvcBuilder</returns>
    public static IServiceCollection AddInject(this IServiceCollection services, Action<AddInjectOptions> configure = null)
    {
        // 载入服务配置选项
        var configureOptions = new AddInjectOptions();
        configure?.Invoke(configureOptions);

        services.AddSpecificationDocuments(AddInjectOptions.SwaggerGenConfigure)
                .AddDynamicApiControllers()
                .AddDataValidation(AddInjectOptions.DataValidationConfigure)
                .AddFriendlyException(AddInjectOptions.FriendlyExceptionConfigure);

        return services;
    }

    /// <summary>
    /// MiniAPI 服务注入基础配置（带Swagger）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure"></param>
    /// <returns>IMvcBuilder</returns>
    /// <remarks>https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0</remarks>
    public static IServiceCollection AddInjectMini(this IServiceCollection services, Action<AddInjectOptions> configure = null)
    {
        // 载入服务配置选项
        var configureOptions = new AddInjectOptions();
        configure?.Invoke(configureOptions);

        services.AddSpecificationDocuments(AddInjectOptions.SwaggerGenConfigure)
                .AddDataValidation(AddInjectOptions.DataValidationConfigure)
                .AddFriendlyException(AddInjectOptions.FriendlyExceptionConfigure);

        return services;
    }

    /// <summary>
    /// Mvc 注入基础配置
    /// </summary>
    /// <param name="mvcBuilder">Mvc构建器</param>
    /// <param name="configure"></param>
    /// <returns>IMvcBuilder</returns>
    public static IMvcBuilder AddInjectBase(this IMvcBuilder mvcBuilder, Action<AddInjectOptions> configure = null)
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
    public static IServiceCollection AddInjectBase(this IServiceCollection services, Action<AddInjectOptions> configure = null)
    {
        // 载入服务配置选项
        var configureOptions = new AddInjectOptions();
        configure?.Invoke(configureOptions);

        services.AddDataValidation(AddInjectOptions.DataValidationConfigure)
                .AddFriendlyException(AddInjectOptions.FriendlyExceptionConfigure);

        return services;
    }

    /// <summary>
    /// Mvc 注入基础配置和规范化结果
    /// </summary>
    /// <param name="mvcBuilder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IMvcBuilder AddInjectWithUnifyResult(this IMvcBuilder mvcBuilder, Action<AddInjectOptions> configure = null)
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
    public static IServiceCollection AddInjectWithUnifyResult(this IServiceCollection services, Action<AddInjectOptions> configure = null)
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
    public static IMvcBuilder AddInjectWithUnifyResult<TUnifyResultProvider>(this IMvcBuilder mvcBuilder, Action<AddInjectOptions> configure = null)
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
    public static IServiceCollection AddInjectWithUnifyResult<TUnifyResultProvider>(this IServiceCollection services, Action<AddInjectOptions> configure = null)
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
        // 获取已注册的 IHostedService 实现类型
        var existingHostedServiceTypes = services
            .Where(c => c.ServiceType == typeof(IHostedService))
            .Select(c => c.ImplementationType)
            .Where(t => t != null)
            .ToHashSet();

        // 获取所有 BackgroundService 类型，排除泛型主机，并过滤已注册的类型
        var backgroundServiceTypes = App.EffectiveTypes.Where(u => !u.IsAbstract && !u.IsInterface && !u.IsGenericType
                    && typeof(IHostedService).IsAssignableFrom(u) && u.Name != "GenericWebHostService"
                    && !existingHostedServiceTypes.Contains(u));

        foreach (var type in backgroundServiceTypes)
        {
            AddHostedServiceMethodInfo.MakeGenericMethod(type).Invoke(null, [services]);
        }

        return services;
    }

    /// <summary>
    /// 添加应用配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="configure">服务配置</param>
    /// <returns>服务集合</returns>
    internal static IServiceCollection AddApp(this IServiceCollection services, IConfiguration configuration, Action<IServiceCollection> configure = null)
    {
        // 注册全局配置选项
        services.AddConfigurableOptions<AppSettingsOptions>(configuration);

        // 注册内存和分布式内存
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();

        // 注册全局依赖注入
        services.AddDependencyInjection();

        // 注册默认服务（JSON 序列化和分布式 ID 生成器）
        services.TryAddSingleton<IJsonSerializerProvider, DefaultJsonSerializerProvider>();
        services.TryAddSingleton<IDistributedIDGenerator, SequentialGuidIDGenerator>();
        services.TryAddSingleton<SequentialGuidIDGenerator>();

        // 检查是否禁用了 AppStartup 扫描（满足某些特殊场景，早期未考虑到，折中处理）
        if (!(App.Settings.DisableAppStartupScan == true || (AppContext.TryGetSwitch(nameof(AppSettingsOptions.DisableAppStartupScan), out var isEnabled) && isEnabled)))
        {
            // 注册全局 Startup 扫描
            services.AddStartups(configuration);
        }

        // 添加对象映射
        services.AddObjectMapper();

        // 注册 CodePagesEncodingProvider，使得程序能够识别并使用 Windows 代码页中的各种编码
        EncodingUtility.Initialize();

        // 自定义服务
        configure?.Invoke(services);

        return services;
    }

    /// <summary>
    /// 添加 Startup 自动扫描
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    internal static IServiceCollection AddStartups(this IServiceCollection services, IConfiguration configuration)
    {
        // 扫描所有继承 AppStartup 或标记 [AppStartup] 特性的类
        var startups = App.EffectiveTypes
            .Where(u => (typeof(AppStartup).IsAssignableFrom(u) || u.IsDefined(typeof(AppStartupAttribute), true)) && u.IsClass && !u.IsAbstract && !u.IsGenericType)
            .OrderByDescending(GetStartupOrder);

        foreach (var type in startups)
        {
            // 获取所有符合依赖注入格式的方法：
            // 返回值 void，参数个数为 1 或 2，第一个参数类型为 IServiceCollection，
            // 如果参数个数为 2，第二个参数类型为 IConfiguration
            var serviceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(u => u.ReturnType == typeof(void)
                            && u.GetParameters().Length >= 1 && u.GetParameters().Length <= 2
                            && u.GetParameters()[0].ParameterType == typeof(IServiceCollection)
                            && (u.GetParameters().Length == 1 || u.GetParameters()[1].ParameterType == typeof(IConfiguration)))
                .ToList();

            if (serviceMethods.Count == 0) continue;

            var isAppStartup = typeof(AppStartup).IsAssignableFrom(type);

            // 确定是否需要创建实例
            object instance = null;
            if (isAppStartup)
            {
                instance = Activator.CreateInstance(type);
                App.AppStartups.Add((AppStartup)instance);
            }
            else if (serviceMethods.Exists(m => !m.IsStatic))
            {
                instance = Activator.CreateInstance(type);
            }

            foreach (var method in serviceMethods)
            {
                var target = method.IsStatic ? null : instance;

                // 根据参数个数传入不同的参数
                var parameters = method.GetParameters().Length == 1
                    ? new object[] { services }
                    : new object[] { services, configuration };
                method.Invoke(target, parameters);
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