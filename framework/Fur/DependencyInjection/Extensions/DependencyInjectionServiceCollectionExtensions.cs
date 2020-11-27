using Fur;
using Fur.DependencyInjection;
using Mapster;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 依赖注入拓展类
    /// </summary>
    [SkipScan]
    public static class DependencyInjectionServiceCollectionExtensions
    {
        /// <summary>
        /// 全局服务代理类型
        /// </summary>
        private static Type GlobalServiceProxyType;

        /// <summary>
        /// 添加服务代理
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServiceProxy<TServiceProxy>(this IServiceCollection services)
            where TServiceProxy : DispatchProxy, IDispatchProxy
        {
            GlobalServiceProxyType = typeof(TServiceProxy);

            return services;
        }

        /// <summary>
        /// 添加依赖注入接口
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            // 添加外部程序集配置
            services.AddConfigurableOptions<DependencyInjectionSettingsOptions>();

            services.AddAutoScanInjection();
            return services;
        }

        /// <summary>
        /// 添加接口代理
        /// </summary>
        /// <typeparam name="TDispatchProxy">代理类</typeparam>
        /// <typeparam name="TIDispatchProxy">被代理接口依赖</typeparam>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddScopedDispatchProxyForInterface<TDispatchProxy, TIDispatchProxy>(this IServiceCollection services)
            where TDispatchProxy : DispatchProxy, IDispatchProxy
            where TIDispatchProxy : class
        {
            // 注册代理类
            services.AddScoped<DispatchProxy, TDispatchProxy>();

            // 代理依赖接口类型
            var proxyType = typeof(TDispatchProxy);
            var typeDependency = typeof(TIDispatchProxy);

            // 获取所有的代理接口类型
            var dispatchProxyInterfaceTypes = App.CanBeScanTypes
                .Where(u => typeDependency.IsAssignableFrom(u) && u.IsInterface && u != typeDependency);

            // 注册代理类型
            foreach (var interfaceType in dispatchProxyInterfaceTypes)
            {
                AddScopedDispatchProxy(services, default, proxyType, interfaceType, false);
            }

            return services;
        }

        /// <summary>
        /// 添加自动扫描注入
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        private static IServiceCollection AddAutoScanInjection(this IServiceCollection services)
        {
            // 查找所有需要依赖注入的类型
            var injectTypes = App.CanBeScanTypes
                .Where(u => typeof(IPrivateDependency).IsAssignableFrom(u) && u.IsClass && !u.IsInterface && !u.IsAbstract)
                .OrderBy(u => GetOrder(u));

            // 执行依赖注入
            foreach (var type in injectTypes)
            {
                // 获取注册方式
                var injectionAttribute = !type.IsDefined(typeof(InjectionAttribute)) ? new InjectionAttribute() : type.GetCustomAttribute<InjectionAttribute>();

                // 获取所有能注册的接口
                var canInjectInterfaces = type.GetInterfaces().Where(u => !typeof(IPrivateDependency).IsAssignableFrom(u));

                // 注册暂时服务
                if (typeof(ITransient).IsAssignableFrom(type))
                {
                    RegisterService(services, Fur.DependencyInjection.RegisterType.Transient, type, injectionAttribute, canInjectInterfaces);
                }
                // 注册作用域服务
                else if (typeof(IScoped).IsAssignableFrom(type))
                {
                    RegisterService(services, Fur.DependencyInjection.RegisterType.Scoped, type, injectionAttribute, canInjectInterfaces);
                }
                // 注册单例服务
                else if (typeof(ISingleton).IsAssignableFrom(type))
                {
                    RegisterService(services, Fur.DependencyInjection.RegisterType.Singleton, type, injectionAttribute, canInjectInterfaces);
                }

                // 缓存类型注册
                var typeNamed = injectionAttribute.Named ?? type.Name;
                TypeNamedCollection.TryAdd(typeNamed, type);
            }

            // 注册外部服务（appsetting.json）
            RegisterExternalServices(services);

            // 注册命名服务
            RegisterNamed(services);

            return services;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="registerType">类型作用域</param>
        /// <param name="type">类型</param>
        /// <param name="injectionAttribute">注入特性</param>
        /// <param name="canInjectInterfaces">能被注册的接口</param>
        private static void RegisterService(IServiceCollection services, RegisterType registerType, Type type, InjectionAttribute injectionAttribute, IEnumerable<Type> canInjectInterfaces)
        {
            // 注册自己
            if (injectionAttribute.Pattern is InjectionPatterns.Self or InjectionPatterns.All or InjectionPatterns.SelfWithFirstInterface)
            {
                RegisterType(services, registerType, type, injectionAttribute);
            }

            if (!canInjectInterfaces.Any()) return;

            // 只注册第一个接口
            if (injectionAttribute.Pattern is InjectionPatterns.FirstInterface or InjectionPatterns.SelfWithFirstInterface)
            {
                RegisterType(services, registerType, type, injectionAttribute, canInjectInterfaces.First());
            }
            // 注册多个接口
            else if (injectionAttribute.Pattern is InjectionPatterns.ImplementedInterfaces or InjectionPatterns.All)
            {
                foreach (var inter in canInjectInterfaces)
                {
                    RegisterType(services, registerType, type, injectionAttribute, inter);
                }
            }
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        /// <param name="services">服务</param>
        /// <param name="registerType">注册类型</param>
        /// <param name="type">类型</param>
        /// <param name="injectionAttribute">注入特性</param>
        /// <param name="inter">接口</param>
        private static void RegisterType(IServiceCollection services, RegisterType registerType, Type type, InjectionAttribute injectionAttribute, Type inter = null)
        {
            // 修复泛型注册类型
            var fixedType = FixedGenericType(type);
            var fixedInter = inter == null ? null : FixedGenericType(inter);

            if (registerType == Fur.DependencyInjection.RegisterType.Transient) RegisterTransientType(services, fixedType, injectionAttribute, fixedInter);
            if (registerType == Fur.DependencyInjection.RegisterType.Scoped) RegisterScopeType(services, fixedType, injectionAttribute, fixedInter);
            if (registerType == Fur.DependencyInjection.RegisterType.Singleton) RegisterSingletonType(services, fixedType, injectionAttribute, fixedInter);
        }

        /// <summary>
        /// 注册瞬时接口实例类型
        /// </summary>
        /// <param name="services">服务</param>
        /// <param name="type">类型</param>
        /// <param name="injectionAttribute">注入特性</param>
        /// <param name="inter">接口</param>
        private static void RegisterTransientType(IServiceCollection services, Type type, InjectionAttribute injectionAttribute, Type inter = null)
        {
            switch (injectionAttribute.Action)
            {
                case InjectionActions.Add:
                    if (inter == null) services.AddTransient(type);
                    else
                    {
                        services.AddTransient(inter, type);
                        AddTransientDispatchProxy(services, type, injectionAttribute.Proxy, inter, true);
                    }
                    break;

                case InjectionActions.TryAdd:
                    if (inter == null) services.TryAddTransient(type);
                    else services.TryAddTransient(inter, type);
                    break;

                default: break;
            }
        }

        /// <summary>
        /// 注册作用域接口实例类型
        /// </summary>
        /// <param name="services">服务</param>
        /// <param name="type">类型</param>
        /// <param name="injectionAttribute">注入特性</param>
        /// <param name="inter">接口</param>
        private static void RegisterScopeType(IServiceCollection services, Type type, InjectionAttribute injectionAttribute, Type inter = null)
        {
            switch (injectionAttribute.Action)
            {
                case InjectionActions.Add:
                    if (inter == null) services.AddScoped(type);
                    else
                    {
                        services.AddScoped(inter, type);
                        AddScopedDispatchProxy(services, type, injectionAttribute.Proxy, inter, true);
                    }
                    break;

                case InjectionActions.TryAdd:
                    if (inter == null) services.TryAddScoped(type);
                    else services.TryAddScoped(inter, type);
                    break;

                default: break;
            }
        }

        /// <summary>
        /// 注册单例接口实例类型
        /// </summary>
        /// <param name="services">服务</param>
        /// <param name="type">类型</param>
        /// <param name="injectionAttribute">注入特性</param>
        /// <param name="inter">接口</param>
        private static void RegisterSingletonType(IServiceCollection services, Type type, InjectionAttribute injectionAttribute, Type inter = null)
        {
            switch (injectionAttribute.Action)
            {
                case InjectionActions.Add:
                    if (inter == null) services.AddSingleton(type);
                    else
                    {
                        services.AddSingleton(inter, type);
                        AddSingletonDispatchProxy(services, type, injectionAttribute.Proxy, inter, true);
                    }
                    break;

                case InjectionActions.TryAdd:
                    if (inter == null) services.TryAddSingleton(type);
                    else services.TryAddSingleton(inter, type);
                    break;

                default: break;
            }
        }

        /// <summary>
        /// 创建暂时瞬时代理
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="type">拦截的类型</param>
        /// <param name="proxyType">代理类型</param>
        /// <param name="inter">代理接口</param>
        /// <param name="hasTarget">是否有实现类</param>
        private static void AddTransientDispatchProxy(IServiceCollection services, Type type, Type proxyType, Type inter, bool hasTarget = true)
        {
            proxyType ??= GlobalServiceProxyType;
            if (proxyType == null || type.IsDefined(typeof(SkipProxyAttribute), true)) return;

            RegisterDispatchProxy(services, typeof(ITransient), proxyType);
            services.AddTransient(inter, provider =>
            {
                dynamic proxy = DispatchCreateMethod.MakeGenericMethod(inter, proxyType).Invoke(null, null);
                proxy.Services = provider;
                if (hasTarget)
                {
                    proxy.Target = provider.GetService(type);
                }

                return proxy;
            });
        }

        /// <summary>
        /// 创建作用域代理
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="type">被代理类型</param>
        /// <param name="proxyType">代理类型</param>
        /// <param name="inter">拦截接口</param>
        /// <param name="hasTarget">是否有实例</param>
        private static void AddScopedDispatchProxy(IServiceCollection services, Type type, Type proxyType, Type inter, bool hasTarget = true)
        {
            proxyType ??= GlobalServiceProxyType;
            if (proxyType == null || type.IsDefined(typeof(SkipProxyAttribute), true)) return;

            RegisterDispatchProxy(services, typeof(IScoped), proxyType);
            services.AddScoped(inter, provider =>
            {
                dynamic proxy = DispatchCreateMethod.MakeGenericMethod(inter, proxyType).Invoke(null, null);
                proxy.Services = provider;
                if (hasTarget)
                {
                    proxy.Target = provider.GetService(type);
                }

                return proxy;
            });
        }

        /// <summary>
        /// 创建作用域代理
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="type">被代理类型</param>
        /// <param name="proxyType">代理类型</param>
        /// <param name="inter">拦截接口</param>
        /// <param name="hasTarget">是否有实例</param>
        private static void AddSingletonDispatchProxy(IServiceCollection services, Type type, Type proxyType, Type inter, bool hasTarget = true)
        {
            proxyType ??= GlobalServiceProxyType;
            if (proxyType == null || type.IsDefined(typeof(SkipProxyAttribute), true)) return;

            RegisterDispatchProxy(services, typeof(ISingleton), proxyType);
            services.AddSingleton(inter, provider =>
            {
                dynamic proxy = DispatchCreateMethod.MakeGenericMethod(inter, proxyType).Invoke(null, null);
                proxy.Services = provider;
                if (hasTarget)
                {
                    proxy.Target = provider.GetService(type);
                }

                return proxy;
            });
        }

        /// <summary>
        /// 注册代理类型
        /// </summary>
        /// <param name="services"></param>
        /// <param name="lifetime"></param>
        /// <param name="proxyType"></param>
        private static void RegisterDispatchProxy(IServiceCollection services, Type lifetime, Type proxyType)
        {
            if (RegisterDispatchProxies.Contains((lifetime, proxyType))) return;

            if (lifetime == typeof(ITransient))
            {
                services.AddTransient(typeof(DispatchProxy), proxyType);
            }
            else if (lifetime == typeof(IScoped))
            {
                services.AddScoped(typeof(DispatchProxy), proxyType);
            }
            else if (lifetime == typeof(ISingleton))
            {
                services.AddSingleton(typeof(DispatchProxy), proxyType);
            }
            else { }

            RegisterDispatchProxies.Add((lifetime, proxyType));
        }

        /// <summary>
        /// 注册命名服务
        /// </summary>
        /// <param name="services">服务集合</param>
        private static void RegisterNamed(IServiceCollection services)
        {
            // 注册暂时命名服务
            services.AddTransient(provider =>
            {
                object ResolveService(string named, ITransient transient)
                {
                    var isRegister = TypeNamedCollection.TryGetValue(named, out var serviceType);
                    return isRegister ? provider.GetService(serviceType) : null;
                }
                return (Func<string, ITransient, object>)ResolveService;
            });

            // 注册作用域命名服务
            services.AddScoped(provider =>
            {
                object ResolveService(string named, IScoped scoped)
                {
                    var isRegister = TypeNamedCollection.TryGetValue(named, out var serviceType);
                    return isRegister ? provider.GetService(serviceType) : null;
                }
                return (Func<string, IScoped, object>)ResolveService;
            });

            // 注册单例命名服务
            services.AddSingleton(provider =>
            {
                object ResolveService(string named, ISingleton singleton)
                {
                    var isRegister = TypeNamedCollection.TryGetValue(named, out var serviceType);
                    return isRegister ? provider.GetService(serviceType) : null;
                }
                return (Func<string, ISingleton, object>)ResolveService;
            });
        }

        /// <summary>
        /// 注册外部服务
        /// </summary>
        /// <param name="services"></param>
        private static void RegisterExternalServices(IServiceCollection services)
        {
            var externalServices = App.GetDuplicateOptions<DependencyInjectionSettingsOptions>();
            if (externalServices is { Definitions: not null })
            {
                // 排序
                var extServices = externalServices.Definitions.OrderBy(u => u.Order);
                foreach (var externalService in extServices)
                {
                    var injectionAttribute = externalService.Adapt<InjectionAttribute>();
                    // 加载代理拦截
                    if (!string.IsNullOrEmpty(externalService.Proxy)) injectionAttribute.Proxy = LoadStringType(externalService.Proxy);

                    RegisterService(services, externalService.RegisterType,
                        LoadStringType(externalService.Service),
                        injectionAttribute,
                        new[] { LoadStringType(externalService.Interface) });
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

            return type.Assembly.GetType($"{type.Namespace}.{type.Name}", true, true);
        }

        /// <summary>
        /// 获取 注册 排序
        /// </summary>
        /// <param name="type">排序类型</param>
        /// <returns>int</returns>
        private static int GetOrder(Type type)
        {
            return !type.IsDefined(typeof(InjectionAttribute), true)
                ? 0
                : type.GetCustomAttribute<InjectionAttribute>(true).Order;
        }

        /// <summary>
        /// 加载字符串类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static Type LoadStringType(string str)
        {
            var typeDefinitions = str.Split(";");
            var assembly = App.Assemblies.First(u => u.GetName().Name == typeDefinitions[0]);
            return assembly.GetType(typeDefinitions[1], true, true);
        }

        /// <summary>
        /// 类型名称集合
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> TypeNamedCollection;

        /// <summary>
        /// 已经注册的代理类
        /// </summary>
        private static readonly ConcurrentBag<(Type, Type)> RegisterDispatchProxies;

        /// <summary>
        /// 创建代理方法
        /// </summary>
        private static readonly MethodInfo DispatchCreateMethod;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static DependencyInjectionServiceCollectionExtensions()
        {
            TypeNamedCollection = new ConcurrentDictionary<string, Type>();
            DispatchCreateMethod = typeof(DispatchProxy).GetMethod(nameof(DispatchProxy.Create));
            RegisterDispatchProxies = new ConcurrentBag<(Type, Type)>();
        }
    }
}