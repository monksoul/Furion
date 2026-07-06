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

using Furion.Extensions;
using Furion.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求构建器
/// </summary>
public sealed class HttpRemoteBuilder
{
    /// <summary>
    ///     <see cref="GenericHttpContentConverter" /> 集合
    /// </summary>
    internal IList<Func<IEnumerable<GenericHttpContentConverter>>>? _genericHttpContentConverterProviders;

    /// <summary>
    ///     <see cref="IHttpContentConverter" /> 集合
    /// </summary>
    internal IList<Func<IEnumerable<IHttpContentConverter>>>? _httpContentConverterProviders;

    /// <summary>
    ///     <see cref="IHttpContentProcessor" /> 集合
    /// </summary>
    internal IList<Func<IEnumerable<IHttpContentProcessor>>>? _httpContentProcessorProviders;

    /// <summary>
    ///     <see cref="IHttpDeclarativeExtractor" /> 集合
    /// </summary>
    internal IList<Func<IEnumerable<IHttpDeclarativeExtractor>>>? _httpDeclarativeExtractors;

    /// <summary>
    ///     <see cref="IHttpDeclarative" /> 类型集合
    /// </summary>
    internal HashSet<Type>? _httpDeclarativeTypes;

    /// <summary>
    ///     <see cref="IObjectContentConverterFactory" /> 实现类型
    /// </summary>
    internal Type? _objectContentConverterFactoryType;

    /// <summary>
    ///     添加 <see cref="IHttpContentProcessor" /> 请求内容处理器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure"><see cref="IHttpContentProcessor" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    public HttpRemoteBuilder AddHttpContentProcessors(Func<IEnumerable<IHttpContentProcessor>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        _httpContentProcessorProviders ??= new List<Func<IEnumerable<IHttpContentProcessor>>>();

        _httpContentProcessorProviders.Add(configure);

        return this;
    }

    /// <summary>
    ///     添加 <see cref="IHttpContentConverter" /> 响应内容转换器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure"><see cref="IHttpContentConverter" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    public HttpRemoteBuilder AddHttpContentConverters(Func<IEnumerable<IHttpContentConverter>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        _httpContentConverterProviders ??= new List<Func<IEnumerable<IHttpContentConverter>>>();

        _httpContentConverterProviders.Add(configure);

        return this;
    }

    /// <summary>
    ///     添加 <see cref="GenericHttpContentConverter" /> 响应内容转换器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure"><see cref="GenericHttpContentConverter" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    public HttpRemoteBuilder AddGenericHttpContentConverters(Func<IEnumerable<GenericHttpContentConverter>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        _genericHttpContentConverterProviders ??= new List<Func<IEnumerable<GenericHttpContentConverter>>>();

        _genericHttpContentConverterProviders.Add(configure);

        return this;
    }

    /// <summary>
    ///     设置 <see cref="IObjectContentConverterFactory" /> 对象内容转换器工厂
    /// </summary>
    /// <typeparam name="TFactory">
    ///     <see cref="IObjectContentConverterFactory" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    public HttpRemoteBuilder UseObjectContentConverterFactory<TFactory>()
        where TFactory : IObjectContentConverterFactory =>
        UseObjectContentConverterFactory(typeof(TFactory));

    /// <summary>
    ///     设置 <see cref="IObjectContentConverterFactory" /> 对象内容转换器工厂
    /// </summary>
    /// <param name="factoryType">
    ///     <see cref="IObjectContentConverterFactory" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRemoteBuilder UseObjectContentConverterFactory(Type factoryType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(factoryType);

        // 检查类型是否实现了 IObjectContentConverterFactory 接口
        if (!typeof(IObjectContentConverterFactory).IsAssignableFrom(factoryType))
        {
            throw new ArgumentException(
                $"`{factoryType}` type is not assignable from `{typeof(IObjectContentConverterFactory)}`.",
                nameof(factoryType));
        }

        _objectContentConverterFactoryType = factoryType;

        return this;
    }

    /// <summary>
    ///     添加 HTTP 声明式服务
    /// </summary>
    /// <typeparam name="TDeclarative">
    ///     <see cref="IHttpDeclarative" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    public HttpRemoteBuilder AddHttpDeclarative<TDeclarative>()
        where TDeclarative : IHttpDeclarative =>
        AddHttpDeclarative(typeof(TDeclarative));

    /// <summary>
    ///     添加 HTTP 声明式服务
    /// </summary>
    /// <remarks>支持封闭泛型类型注册。</remarks>
    /// <param name="declarativeType">
    ///     <see cref="IHttpDeclarative" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRemoteBuilder AddHttpDeclarative(Type declarativeType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(declarativeType);

        // 检查类型是否是接口且实现了 IHttpDeclarative 接口
        if (!declarativeType.IsInterface ||
            declarativeType is { IsGenericType: true, ContainsGenericParameters: true } ||
            !typeof(IHttpDeclarative).IsAssignableFrom(declarativeType))
        {
            throw new ArgumentException(
                $"The type `{declarativeType}` must be a closed or non-generic interface that implements `{typeof(IHttpDeclarative)}`.",
                nameof(declarativeType));
        }

        _httpDeclarativeTypes ??= [];

        _httpDeclarativeTypes.Add(declarativeType);

        return this;
    }

    /// <summary>
    ///     添加 HTTP 声明式服务
    /// </summary>
    /// <remarks>支持封闭泛型类型注册。</remarks>
    /// <param name="declarativeTypes">
    ///     <see cref="IHttpDeclarative" /> 集合
    /// </param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRemoteBuilder AddHttpDeclaratives(params IEnumerable<Type> declarativeTypes)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(declarativeTypes);

        foreach (var declarativeType in declarativeTypes)
        {
            AddHttpDeclarative(declarativeType);
        }

        return this;
    }

    /// <summary>
    ///     扫描程序集并添加 HTTP 声明式服务
    /// </summary>
    /// <param name="assemblies"><see cref="Assembly" /> 集合</param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    public HttpRemoteBuilder AddHttpDeclarativesFromAssemblies(params IEnumerable<Assembly?> assemblies)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(assemblies);

        AddHttpDeclaratives(assemblies.SelectMany(ass =>
            (ass?.GetExportedTypes() ?? Enumerable.Empty<Type>()).Where(t =>
                t is { IsInterface: true, IsGenericType: false } && typeof(IHttpDeclarative).IsAssignableFrom(t))));

        return this;
    }

    /// <summary>
    ///     添加 HTTP 声明式 <see cref="IHttpDeclarativeExtractor" /> 提取器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure"><see cref="IHttpDeclarativeExtractor" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    public HttpRemoteBuilder AddHttpDeclarativeExtractors(Func<IEnumerable<IHttpDeclarativeExtractor>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        _httpDeclarativeExtractors ??= new List<Func<IEnumerable<IHttpDeclarativeExtractor>>>();

        _httpDeclarativeExtractors.Add(configure);

        return this;
    }

    /// <summary>
    ///     扫描程序集并添加 HTTP 声明式 <see cref="IHttpDeclarativeExtractor" /> 提取器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="assemblies"><see cref="Assembly" /> 集合</param>
    /// <returns>
    ///     <see cref="HttpRemoteBuilder" />
    /// </returns>
    public HttpRemoteBuilder AddHttpDeclarativeExtractorsFromAssemblies(params IEnumerable<Assembly?> assemblies)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(assemblies);

        return AddHttpDeclarativeExtractors(() => assemblies.SelectMany(ass =>
            (ass?.GetTypes() ?? Enumerable.Empty<Type>())
            .Where(t => t.HasDefinePublicParameterlessConstructor() &&
                        typeof(IHttpDeclarativeExtractor).IsAssignableFrom(t))
            .Select(t => (IHttpDeclarativeExtractor)Activator.CreateInstance(t)!)));
    }

    /// <summary>
    ///     构建模块服务
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    internal void Build(IServiceCollection services)
    {
        // 注册 CodePagesEncodingProvider，使得程序能够识别并使用 Windows 代码页中的各种编码
        EncodingUtility.Initialize();

        // 注册日志服务
        services.AddLogging();

        // 注册默认 HttpClient 客户端
        if (services.All(u => u.ServiceType != typeof(IHttpClientFactory)))
        {
            // 注册默认 HttpClient 客户端
            var httpClientBuilder = services.AddHttpClient(string.Empty);

            // 检查是否不是 WebAssembly 应用
            if (!OperatingSystem.IsBrowser())
            {
                // 默认启用 gzip、deflate 和 brotli 自动解压
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.All
                });
            }
        }

        // 检查是否配置（注册）了日志程序
        var isLoggingRegistered = services.Any(u => u.ServiceType == typeof(ILoggerProvider));

        // 注册 HTTP 远程请求日志服务
        services.TryAddSingleton<IHttpRemoteLogger>(provider =>
            ActivatorUtilities.CreateInstance<HttpRemoteLogger>(provider, isLoggingRegistered));

        // 注册并配置 HttpRemoteOptions 选项服务
        services.Configure<HttpRemoteOptions>(options =>
        {
            options.HttpDeclarativeExtractors = _httpDeclarativeExtractors?.AsReadOnly();
        });

        // 注册内容提供器
        RegisterContentProviders(services);

        // 注册 HttpContent 内容处理器工厂
        services.TryAddSingleton<IHttpContentProcessorFactory, HttpContentProcessorFactory>();

        // 注册 HttpContent 内容转换器工厂
        services.TryAddSingleton<IHttpContentConverterFactory, HttpContentConverterFactory>();

        // 注册对象内容转换器工厂
        services.TryAddSingleton<IObjectContentConverterFactory, ObjectContentConverterFactory>();

        // 注册 HTTP 远程请求服务
        services.TryAddSingleton<IHttpRemoteService, HttpRemoteService>();

        // 检查是否自定义了对象内容转换器工厂，如果存在则替换
        if (_objectContentConverterFactoryType is not null &&
            _objectContentConverterFactoryType != typeof(ObjectContentConverterFactory))
        {
            services.Replace(ServiceDescriptor.Singleton(typeof(IObjectContentConverterFactory),
                _objectContentConverterFactoryType));
        }

        // 构建 HTTP 声明式远程请求服务
        BuildHttpDeclarativeServices(services);
    }

    /// <summary>
    ///     注册内容提供器
    /// </summary>
    /// <remarks>处理重复注册远程请求服务时不能合并多个提供器问题。</remarks>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    internal void RegisterContentProviders(IServiceCollection services)
    {
        // 注册所有请求内容处理器
        if (_httpContentProcessorProviders is not null)
        {
            foreach (var processor in _httpContentProcessorProviders.SelectMany(u => u()))
            {
                services.AddSingleton(processor);
            }
        }

        // 注册所有响应内容转换器
        if (_httpContentConverterProviders is not null)
        {
            foreach (var converter in _httpContentConverterProviders.SelectMany(u => u()))
            {
                services.AddSingleton(converter);
            }
        }

        // 注册所有泛型响应内容转换器
        // ReSharper disable once InvertIf
        if (_genericHttpContentConverterProviders != null)
        {
            foreach (var converter in _genericHttpContentConverterProviders.SelectMany(u => u()))
            {
                services.AddSingleton(converter);
            }
        }
    }

    /// <summary>
    ///     构建 HTTP 声明式远程请求服务
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    internal void BuildHttpDeclarativeServices(IServiceCollection services)
    {
        // 空检查
        if (_httpDeclarativeTypes is null)
        {
            return;
        }

        // 初始化 HTTP 声明式远程请求代理类类型
        var httpDeclarativeDispatchProxyType = typeof(HttpDeclarativeDispatchProxy);

        // 遍历 HTTP 声明式远程请求类型并注册为服务
        foreach (var httpDeclarativeType in _httpDeclarativeTypes)
        {
            services.TryAddSingleton(httpDeclarativeType, provider =>
            {
                // 创建 HTTP 声明式远程请求代理实例
                var httpDeclarative =
                    DispatchProxyAsync.Create(httpDeclarativeType, httpDeclarativeDispatchProxyType) as
                        HttpDeclarativeDispatchProxy;

                // 空检查
                ArgumentNullException.ThrowIfNull(httpDeclarative);

                // 解析 IHttpRemoteService 服务并设置给 RemoteService 属性
                httpDeclarative.RemoteService = provider.GetRequiredService<IHttpRemoteService>();

                // 将实际被代理的接口类型设置给 InterfaceType 属性
                httpDeclarative.InterfaceType = httpDeclarativeType;

                return httpDeclarative;
            });
        }
    }
}