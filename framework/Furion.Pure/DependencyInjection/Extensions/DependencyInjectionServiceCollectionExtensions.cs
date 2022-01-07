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
using Furion.Reflection;
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
    [SuppressSniffer]
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
            // 添加内部依赖注入扫描拓展
            services.AddInnerDependencyInjection();

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
            where TDispatchProxy : AspectDispatchProxy, IDispatchProxy
            where TIDispatchProxy : class
        {
            // 注册代理类
            services.AddScoped<AspectDispatchProxy, TDispatchProxy>();

            // 代理依赖接口类型
            var proxyType = typeof(TDispatchProxy);
            var typeDependency = typeof(TIDispatchProxy);

            // 获取所有的代理接口类型
            var dispatchProxyInterfaceTypes = App.EffectiveTypes
                .Where(u => typeDependency.IsAssignableFrom(u) && u.IsInterface && u != typeDependency);

            // 注册代理类型
            foreach (var interfaceType in dispatchProxyInterfaceTypes)
            {
                AddDispatchProxy(services, typeof(IScoped), default, proxyType, interfaceType, false);
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
                .OrderBy(u => GetOrder(u));

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
                                && u != typeof(IPrivateDependency)
                                && u != typeof(IDynamicApiController)
                                && !lifetimeInterfaces.Contains(u)
                                && projectAssemblies.Contains(u.Assembly)
                                && (
                                    (!type.IsGenericType && !u.IsGenericType)
                                    || (type.IsGenericType && u.IsGenericType && type.GetGenericArguments().Length == u.GetGenericArguments().Length))
                                );

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

            switch (injectionAttribute.Action)
            {
                case InjectionActions.Add:
                    if (fixedInter == null) services.InnerAdd(dependencyType, fixedType);
                    else
                    {
                        services.InnerAdd(dependencyType, fixedInter, fixedType);
                        AddDispatchProxy(services, dependencyType, fixedType, injectionAttribute.Proxy, fixedInter, true);
                    }
                    break;

                case InjectionActions.TryAdd:
                    if (fixedInter == null) services.InnerTryAdd(dependencyType, fixedType);
                    else services.InnerTryAdd(dependencyType, fixedInter, fixedType);
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

            // 注册代理类型
            services.InnerAdd(dependencyType, typeof(AspectDispatchProxy), proxyType);

            // 注册服务
            services.InnerAdd(dependencyType, inter, provider =>
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
        /// 注册命名服务（接口多实现）
        /// </summary>
        /// <typeparam name="TDependency"></typeparam>
        /// <param name="services"></param>
        private static void RegisterNamedService<TDependency>(IServiceCollection services)
            where TDependency : IPrivateDependency
        {
            // 注册命名服务
            services.InnerAdd(typeof(TDependency), provider =>
             {
                 object ResolveService(string named, TDependency _)
                 {
                     var isRegister = TypeNamedCollection.TryGetValue(named, out var serviceType);
                     return isRegister ? provider.GetService(serviceType) : null;
                 }
                 return (Func<string, TDependency, object>)ResolveService;
             });
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
                    if (!string.IsNullOrWhiteSpace(externalService.Proxy)) injectionAttribute.Proxy = GetProxyType(externalService.Proxy);

                    // 解析注册类型
                    var dependencyType = externalService.RegisterType switch
                    {
                        RegisterType.Transient => typeof(ITransient),
                        RegisterType.Scoped => typeof(IScoped),
                        RegisterType.Singleton => typeof(ISingleton),
                        _ => throw new InvalidOperationException("Unknown lifetime type.")
                    };

                    RegisterService(services, dependencyType,
                        GetProxyType(externalService.Service),
                        injectionAttribute,
                        new[] { GetProxyType(externalService.Interface) });
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

            return Reflect.GetType(type.Assembly, $"{type.Namespace}.{type.Name}");
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
        /// 加载代理程序集
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static Type GetProxyType(string str)
        {
            var typeDefinitions = str.Split(";");
            return Reflect.GetType(typeDefinitions[0], typeDefinitions[1]);
        }

        /// <summary>
        /// 类型名称集合
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> TypeNamedCollection;

        /// <summary>
        /// 创建代理方法
        /// </summary>
        private static readonly MethodInfo DispatchCreateMethod;

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
                .FirstOrDefault(u => typeof(AspectDispatchProxy).IsAssignableFrom(u) && typeof(IGlobalDispatchProxy).IsAssignableFrom(u) && u.IsClass && !u.IsInterface && !u.IsAbstract);

            TypeNamedCollection = new ConcurrentDictionary<string, Type>();
            DispatchCreateMethod = typeof(AspectDispatchProxy).GetMethod(nameof(AspectDispatchProxy.Create));
        }
    }
}