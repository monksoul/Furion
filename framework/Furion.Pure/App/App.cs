// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ConfigurableOptions;
using Furion.DependencyInjection;
using Furion.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Profiling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;

namespace Furion
{
    /// <summary>
    /// 全局应用类
    /// </summary>
    [SuppressSniffer]
    public static class App
    {
        /// <summary>
        /// 私有设置，避免重复解析
        /// </summary>
        internal static AppSettingsOptions _settings;

        /// <summary>
        /// 应用全局配置
        /// </summary>
        public static AppSettingsOptions Settings => _settings ??= GetConfig<AppSettingsOptions>("AppSettings", true);

        /// <summary>
        /// 全局配置选项
        /// </summary>
        public static IConfiguration Configuration => InternalApp.Configuration;

        /// <summary>
        /// 获取Web主机环境，如，是否是开发环境，生产环境等
        /// </summary>
        public static IWebHostEnvironment WebHostEnvironment => InternalApp.WebHostEnvironment;

        /// <summary>
        /// 获取泛型主机环境，如，是否是开发环境，生产环境等
        /// </summary>
        public static IHostEnvironment HostEnvironment => InternalApp.HostEnvironment;

        /// <summary>
        /// 存储根服务，可能为空
        /// </summary>
        public static IServiceProvider RootServices => InternalApp.RootServices;

        /// <summary>
        /// 应用有效程序集
        /// </summary>
        public static readonly IEnumerable<Assembly> Assemblies;

        /// <summary>
        /// 有效程序集类型
        /// </summary>
        public static readonly IEnumerable<Type> EffectiveTypes;

        /// <summary>
        /// 获取请求上下文
        /// </summary>
        public static HttpContext HttpContext => RootServices?.GetService<IHttpContextAccessor>()?.HttpContext;

        /// <summary>
        /// 获取请求上下文用户
        /// </summary>
        /// <remarks>只有授权访问的页面或接口才存在值，否则为 null</remarks>
        public static ClaimsPrincipal User => HttpContext?.User;

        /// <summary>
        /// 未托管的对象集合
        /// </summary>
        public static readonly ConcurrentBag<IDisposable> UnmanagedObjects;

        /// <summary>
        /// 解析服务提供器
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(Type serviceType)
        {
            // 处理控制台应用程序
            if (HostEnvironment == default) return RootServices;

            // 第一选择，判断是否是单例注册且单例服务不为空，如果是直接返回根服务提供器
            if (RootServices != null && InternalApp.InternalServices.Where(u => u.ServiceType == (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType))
                                                                    .Any(u => u.Lifetime == ServiceLifetime.Singleton)) return RootServices;

            // 第二选择是获取 HttpContext 对象的 RequestServices
            var httpContext = HttpContext;
            if (httpContext?.RequestServices != null) return httpContext.RequestServices;
            // 第三选择，创建新的作用域并返回服务提供器
            else if (RootServices != null)
            {
                var scoped = RootServices.CreateScope();
                UnmanagedObjects.Add(scoped);
                return scoped.ServiceProvider;
            }
            // 第四选择，构建新的服务对象（性能最差）
            else
            {
                var serviceProvider = InternalApp.InternalServices.BuildServiceProvider();
                UnmanagedObjects.Add(serviceProvider);
                return serviceProvider;
            }
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static TService GetService<TService>(IServiceProvider serviceProvider = default)
            where TService : class
        {
            return GetService(typeof(TService), serviceProvider) as TService;
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static object GetService(Type type, IServiceProvider serviceProvider = default)
        {
            return (serviceProvider ?? GetServiceProvider(type)).GetService(type);
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static TService GetRequiredService<TService>(IServiceProvider serviceProvider = default)
            where TService : class
        {
            return GetRequiredService(typeof(TService), serviceProvider) as TService;
        }

        /// <summary>
        /// 获取请求生存周期的服务
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static object GetRequiredService(Type type, IServiceProvider serviceProvider = default)
        {
            return (serviceProvider ?? GetServiceProvider(type)).GetRequiredService(type);
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="TOptions">强类型选项类</typeparam>
        /// <param name="path">配置中对应的Key</param>
        /// <param name="loadPostConfigure"></param>
        /// <returns>TOptions</returns>
        public static TOptions GetConfig<TOptions>(string path, bool loadPostConfigure = false)
        {
            var options = Configuration.GetSection(path).Get<TOptions>();

            // 加载默认选项配置
            if (loadPostConfigure && typeof(IConfigurableOptions).IsAssignableFrom(typeof(TOptions)))
            {
                var postConfigure = typeof(TOptions).GetMethod("PostConfigure");
                if (postConfigure != null)
                {
                    options ??= Activator.CreateInstance<TOptions>();
                    postConfigure.Invoke(options, new object[] { options, Configuration });
                }
            }

            return options;
        }

        /// <summary>
        /// 获取选项
        /// </summary>
        /// <typeparam name="TOptions">强类型选项类</typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns>TOptions</returns>
        public static TOptions GetOptions<TOptions>(IServiceProvider serviceProvider = default)
            where TOptions : class, new()
        {
            return GetService<IOptions<TOptions>>(serviceProvider ?? RootServices)?.Value;
        }

        /// <summary>
        /// 获取选项
        /// </summary>
        /// <typeparam name="TOptions">强类型选项类</typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns>TOptions</returns>
        public static TOptions GetOptionsMonitor<TOptions>(IServiceProvider serviceProvider = default)
            where TOptions : class, new()
        {
            return GetService<IOptionsMonitor<TOptions>>(serviceProvider ?? RootServices)?.CurrentValue;
        }

        /// <summary>
        /// 获取选项
        /// </summary>
        /// <typeparam name="TOptions">强类型选项类</typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns>TOptions</returns>
        public static TOptions GetOptionsSnapshot<TOptions>(IServiceProvider serviceProvider = default)
            where TOptions : class, new()
        {
            // 这里不能从根服务解析，因为是 Scoped 作用域
            return GetService<IOptionsSnapshot<TOptions>>(serviceProvider)?.Value;
        }

        /// <summary>
        /// 打印验证信息到 MiniProfiler
        /// </summary>
        /// <param name="category">分类</param>
        /// <param name="state">状态</param>
        /// <param name="message">消息</param>
        /// <param name="isError">是否为警告消息</param>
        public static void PrintToMiniProfiler(string category, string state, string message = null, bool isError = false)
        {
            if (!CanBeMiniProfiler()) return;

            // 打印消息
            var titleCaseCategory = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(category);
            var customTiming = MiniProfiler.Current?.CustomTiming(category, string.IsNullOrWhiteSpace(message) ? $"{titleCaseCategory} {state}" : message, state);
            if (customTiming == null) return;

            // 判断是否是警告消息
            if (isError) customTiming.Errored = true;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        static App()
        {
            // 未托管的对象
            UnmanagedObjects = new ConcurrentBag<IDisposable>();

            // 加载程序集
            var assObject = GetAssemblies();
            Assemblies = assObject.Assemblies;
            ExternalAssemblies = assObject.ExternalAssemblies;

            // 获取有效的类型集合
            EffectiveTypes = Assemblies.SelectMany(u => u.GetTypes()
                .Where(u => u.IsPublic && !u.IsDefined(typeof(SuppressSnifferAttribute), false)));

            AppStartups = new ConcurrentBag<AppStartup>();
        }

        /// <summary>
        /// 应用所有启动配置对象
        /// </summary>
        internal static ConcurrentBag<AppStartup> AppStartups;

        /// <summary>
        /// 外部程序集
        /// </summary>
        internal static IEnumerable<Assembly> ExternalAssemblies;

        /// <summary>
        /// 获取应用有效程序集
        /// </summary>
        /// <returns>IEnumerable</returns>
        private static (IEnumerable<Assembly> Assemblies, IEnumerable<Assembly> ExternalAssemblies) GetAssemblies()
        {
            // 需排除的程序集后缀
            var excludeAssemblyNames = new string[] {
                "Database.Migrations"
            };

            // 读取应用配置
            var supportPackageNamePrefixs = Settings.SupportPackageNamePrefixs ?? Array.Empty<string>();

            var dependencyContext = DependencyContext.Default;

            // 读取项目程序集或 Furion 官方发布的包，或手动添加引用的dll，或配置特定的包前缀
            var scanAssemblies = dependencyContext.RuntimeLibraries
                .Where(u =>
                       (u.Type == "project" && !excludeAssemblyNames.Any(j => u.Name.EndsWith(j))) ||
                       (u.Type == "package" && (u.Name.StartsWith(nameof(Furion)) || supportPackageNamePrefixs.Any(p => u.Name.StartsWith(p)))) ||
                       (Settings.EnabledReferenceAssemblyScan == true && u.Type == "reference"))    // 判断是否启用引用程序集扫描
                .Select(u => Reflect.GetAssembly(u.Name));

            IEnumerable<Assembly> externalAssemblies = Array.Empty<Assembly>();

            // 加载 `appsetting.json` 配置的外部程序集
            if (Settings.ExternalAssemblies != null && Settings.ExternalAssemblies.Any())
            {
                foreach (var externalAssembly in Settings.ExternalAssemblies)
                {
                    // 加载外部程序集
                    var assemblyFileFullPath = Path.Combine(AppContext.BaseDirectory
                        , externalAssembly.EndsWith(".dll") ? externalAssembly : $"{externalAssembly}.dll");

                    // 根据路径加载程序集
                    var loadedAssembly = Reflect.LoadAssembly(assemblyFileFullPath);
                    if (loadedAssembly == default) continue;
                    var assembly = new[] { loadedAssembly };

                    // 合并程序集
                    scanAssemblies = scanAssemblies.Concat(assembly);
                    externalAssemblies = externalAssemblies.Concat(assembly);
                }
            }

            return (scanAssemblies, externalAssemblies);
        }

        /// <summary>
        /// 判断是否启用 MiniProfiler
        /// </summary>
        /// <returns></returns>
        internal static bool CanBeMiniProfiler()
        {
            // 减少不必要的监听
            if (Settings.InjectMiniProfiler != true || HttpContext == null
                || !(HttpContext.Request.Headers.TryGetValue("request-from", out var value) && value == "swagger")) return false;

            return true;
        }

        /// <summary>
        /// 释放所有未托管的对象
        /// </summary>
        public static void DisposeUnmanagedObjects()
        {
            UnmanagedObjects.Clear();
        }
    }
}