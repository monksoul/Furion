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
using Furion.DynamicApiController;
using Furion.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 依赖注入扩展类
/// </summary>
public static class DependencyInjectionServiceCollectionExtensions
{
    /// <summary>
    /// 添加依赖注入接口
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        // 添加外部程序集配置
        services.AddConfigurableOptions<DependencyInjectionSettingsOptions>();

        // 添加内部依赖注入扫描扩展
        services.AddInnerDependencyInjection();

        // 注册命名服务
        services.AddTransient(typeof(INamedServiceProvider<>), typeof(NamedServiceProvider<>));

        return services;
    }

    /// <summary>
    /// 添加接口代理
    /// </summary>
    /// <typeparam name="TDispatchProxy">代理类</typeparam>
    /// <typeparam name="TIDispatchProxy">被代理接口依赖</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="dependencyType"></param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDispatchProxyForInterface<TDispatchProxy, TIDispatchProxy>(this IServiceCollection services, Type dependencyType)
        where TDispatchProxy : DispatchProxyAsync, IDispatchProxy
        where TIDispatchProxy : class
    {
        // 注册代理类
        var lifetime = TryGetServiceLifetime(dependencyType);
        services.TryAdd(ServiceDescriptor.Describe(typeof(DispatchProxyAsync), typeof(TDispatchProxy), lifetime));

        // 代理依赖接口类型
        var proxyType = typeof(TDispatchProxy);
        var typeDependency = typeof(TIDispatchProxy);

        // 获取所有的代理接口类型
        var dispatchProxyInterfaceTypes = App.EffectiveTypes
            .Where(u => typeDependency.IsAssignableFrom(u) && u.IsInterface && u != typeDependency);

        // 注册代理类型
        foreach (var interfaceType in dispatchProxyInterfaceTypes)
        {
            AddDispatchProxy(services, dependencyType, default, proxyType, interfaceType, false);
        }

        return services;
    }

    /// <summary>
    /// 添加扫描注入
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    private static IServiceCollection AddInnerDependencyInjection(this IServiceCollection services)
    {
        // 查找所有需要依赖注入的类型
        var injectTypes = App.EffectiveTypes
            .Where(u => typeof(IPrivateDependency).IsAssignableFrom(u) && u.IsClass && !u.IsInterface && !u.IsAbstract)
            .Select(u => new { Type = u, Order = GetOrder(u) })
            .OrderBy(x => x.Order)
            .Select(x => x.Type)
            .ToList();

        var projectAssemblies = App.Assemblies;
        var lifetimeInterfaces = new[] { typeof(ITransient), typeof(IScoped), typeof(ISingleton) };

        // 执行依赖注入
        foreach (var type in injectTypes)
        {
            // 获取注册方式
            var injectionAttribute = !type.IsDefined(typeof(InjectionAttribute)) ? new InjectionAttribute() : type.GetCustomAttribute<InjectionAttribute>();

            var interfaces = type.GetInterfaces();

            // 获取所有能注册的接口
            var canInjectInterfaces = interfaces.Where(u => !injectionAttribute.ExceptInterfaces.Contains(u)
                            && u != typeof(IDisposable)
                            && u != typeof(IAsyncDisposable)
                            && u != typeof(IPrivateDependency)
                            && u != typeof(IDynamicApiController)
                            && !lifetimeInterfaces.Contains(u)
                            && projectAssemblies.Contains(u.Assembly)
                            && (
                                (!type.IsGenericType && !u.IsGenericType)
                                || (type.IsGenericType && u.IsGenericType && type.GetGenericArguments().Length == u.GetGenericArguments().Length))
                            ).ToList();

            // 获取生存周期类型
            var dependencyType = interfaces.Last(u => lifetimeInterfaces.Contains(u));

            // 注册服务
            RegisterService(services, dependencyType, type, injectionAttribute, canInjectInterfaces);

            // 缓存类型注册
            var typeNamed = injectionAttribute.Named ?? type.Name;
            TypeNamedCollection.TryAdd(typeNamed, type);
        }

        // 注册外部配置服务
        RegisterExternalServices(services);

        // 注册命名服务（接口多实现）
        RegisterNamedService<ITransient>(services);
        RegisterNamedService<IScoped>(services);
        RegisterNamedService<ISingleton>(services);

        return services;
    }

    /// <summary>
    /// 注册服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="dependencyType"></param>
    /// <param name="type">类型</param>
    /// <param name="injectionAttribute">注入特性</param>
    /// <param name="canInjectInterfaces">能被注册的接口</param>
    private static void RegisterService(IServiceCollection services, Type dependencyType, Type type, InjectionAttribute injectionAttribute, IEnumerable<Type> canInjectInterfaces)
    {
        // 注册自己
        if (injectionAttribute.Pattern is InjectionPatterns.Self or InjectionPatterns.All or InjectionPatterns.SelfWithFirstInterface)
        {
            Register(services, dependencyType, type, injectionAttribute);
        }

        if (!canInjectInterfaces.Any()) return;

        // 只注册第一个接口
        if (injectionAttribute.Pattern is InjectionPatterns.FirstInterface or InjectionPatterns.SelfWithFirstInterface)
        {
            Register(services, dependencyType, type, injectionAttribute, canInjectInterfaces.Last());
        }
        // 注册多个接口
        else if (injectionAttribute.Pattern is InjectionPatterns.ImplementedInterfaces or InjectionPatterns.All)
        {
            foreach (var inter in canInjectInterfaces)
            {
                Register(services, dependencyType, type, injectionAttribute, inter);
            }
        }
    }

    /// <summary>
    /// 注册类型
    /// </summary>
    /// <param name="services">服务</param>
    /// <param name="dependencyType"></param>
    /// <param name="type">类型</param>
    /// <param name="injectionAttribute">注入特性</param>
    /// <param name="inter">接口</param>
    private static void Register(IServiceCollection services, Type dependencyType, Type type, InjectionAttribute injectionAttribute, Type inter = null)
    {
        // 修复泛型注册类型
        var fixedType = FixedGenericType(type);
        var fixedInter = inter == null ? null : FixedGenericType(inter);
        var lifetime = TryGetServiceLifetime(dependencyType);

        switch (injectionAttribute.Action)
        {
            case InjectionActions.Add:
                if (fixedInter == null) services.Add(ServiceDescriptor.Describe(fixedType, fixedType, lifetime));
                else
                {
                    services.Add(ServiceDescriptor.Describe(fixedInter, fixedType, lifetime));
                    AddDispatchProxy(services, dependencyType, fixedType, injectionAttribute.Proxy, fixedInter, true);
                }
                break;

            case InjectionActions.TryAdd:
                if (fixedInter == null) services.TryAdd(ServiceDescriptor.Describe(fixedType, fixedType, lifetime));
                else services.Add(ServiceDescriptor.Describe(fixedInter, fixedType, lifetime));
                break;

            default: break;
        }
    }

    /// <summary>
    /// 创建服务代理
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="dependencyType"></param>
    /// <param name="type">拦截的类型</param>
    /// <param name="proxyType">代理类型</param>
    /// <param name="inter">代理接口</param>
    /// <param name="hasTarget">是否有实现类</param>
    private static void AddDispatchProxy(IServiceCollection services, Type dependencyType, Type type, Type proxyType, Type inter, bool hasTarget = true)
    {
        proxyType ??= GlobalServiceProxyType;
        if (proxyType == null || (type != null && type.IsDefined(typeof(SuppressProxyAttribute), true))) return;

        var lifetime = TryGetServiceLifetime(dependencyType);

        // 注册代理类型
        services.TryAdd(ServiceDescriptor.Describe(typeof(DispatchProxyAsync), proxyType, lifetime));

        // 注册服务
        services.Add(ServiceDescriptor.Describe(inter, provider =>
        {
            var proxyObj = DispatchProxyAsync.Create(inter, proxyType);

            if (proxyObj is IDispatchProxy proxy)
            {
                proxy.Services = provider;
                if (hasTarget)
                {
                    proxy.Target = provider.GetService(type);
                }
            }

            return proxyObj;
        }, lifetime));
    }

    /// <summary>
    /// 注册命名服务（接口多实现）
    /// </summary>
    /// <typeparam name="TDependency"></typeparam>
    /// <param name="services"></param>
    private static void RegisterNamedService<TDependency>(IServiceCollection services)
        where TDependency : IPrivateDependency
    {
        // 注册命名服务
        services.Add(ServiceDescriptor.Describe(typeof(Func<string, TDependency, object>), provider =>
        {
            object ResolveService(string named, TDependency _)
            {
                var isRegister = TypeNamedCollection.TryGetValue(named, out var serviceType);
                if (!isRegister) return null;

                return provider.GetService(serviceType);
            }
            return (Func<string, TDependency, object>)ResolveService;
        }, ServiceLifetime.Transient));
    }

    /// <summary>
    /// 注册外部服务
    /// </summary>
    /// <param name="services"></param>
    private static void RegisterExternalServices(IServiceCollection services)
    {
        // 获取选项
        var externalServices = App.GetConfig<DependencyInjectionSettingsOptions>("DependencyInjectionSettings", true);

        if (externalServices is { Definitions: not null })
        {
            // 排序
            var extServices = externalServices.Definitions.OrderBy(u => u.Order);
            foreach (var externalService in extServices)
            {
                var injectionAttribute = new InjectionAttribute
                {
                    Action = externalService.Action,
                    Named = externalService.Named,
                    Order = externalService.Order,
                    Pattern = externalService.Pattern
                };

                // 加载代理拦截
                if (!string.IsNullOrWhiteSpace(externalService.Proxy)) injectionAttribute.Proxy = Reflect.GetStringType(externalService.Proxy);

                // 解析注册类型
                var dependencyType = externalService.RegisterType switch
                {
                    RegisterType.Transient => typeof(ITransient),
                    RegisterType.Scoped => typeof(IScoped),
                    RegisterType.Singleton => typeof(ISingleton),
                    _ => throw new InvalidOperationException("Unknown lifetime type.")
                };

                RegisterService(services, dependencyType,
                    Reflect.GetStringType(externalService.Service),
                    injectionAttribute,
                    [Reflect.GetStringType(externalService.Interface)]);
            }
        }
    }

    /// <summary>
    /// 修复泛型类型注册类型问题
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    private static Type FixedGenericType(Type type)
    {
        if (!type.IsGenericType) return type;

        return type.GetGenericTypeDefinition();
    }

    /// <summary>
    /// 获取 注册 排序
    /// </summary>
    /// <param name="type">排序类型</param>
    /// <returns>int</returns>
    private static int GetOrder(Type type)
    {
        return !type.IsDefined(typeof(InjectionAttribute), true) ? 0 : type.GetCustomAttribute<InjectionAttribute>(true).Order;
    }

    /// <summary>
    /// 根据依赖接口类型解析 ServiceLifetime 对象
    /// </summary>
    /// <param name="dependencyType"></param>
    /// <returns></returns>
    private static ServiceLifetime TryGetServiceLifetime(Type dependencyType)
    {
        if (dependencyType == typeof(ITransient))
        {
            return ServiceLifetime.Transient;
        }
        else if (dependencyType == typeof(IScoped))
        {
            return ServiceLifetime.Scoped;
        }
        else if (dependencyType == typeof(ISingleton))
        {
            return ServiceLifetime.Singleton;
        }
        else
        {
            throw new InvalidCastException("Invalid service registration lifetime.");
        }
    }

    /// <summary>
    /// 类型名称集合
    /// </summary>
    private static readonly ConcurrentDictionary<string, Type> TypeNamedCollection;

    /// <summary>
    /// 全局服务代理类型
    /// </summary>
    private static readonly Type GlobalServiceProxyType;

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static DependencyInjectionServiceCollectionExtensions()
    {
        // 获取全局代理类型
        GlobalServiceProxyType = App.EffectiveTypes
            .FirstOrDefault(u => typeof(DispatchProxyAsync).IsAssignableFrom(u) && typeof(IGlobalDispatchProxy).IsAssignableFrom(u) && u.IsClass && !u.IsInterface && !u.IsAbstract);

        TypeNamedCollection = new ConcurrentDictionary<string, Type>();
    }
}