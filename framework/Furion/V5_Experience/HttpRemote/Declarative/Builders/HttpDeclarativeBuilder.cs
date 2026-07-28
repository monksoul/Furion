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

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

using Furion.Extensions;
using System.Collections.Concurrent;
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 声明式远程请求构建器
/// </summary>
/// <remarks>使用 <c>HttpRequestBuilder.Declarative(method, args)</c> 静态方法创建。</remarks>
public sealed class HttpDeclarativeBuilder
{
    /// <summary>
    ///     HTTP 声明式 <see cref="IHttpDeclarativeExtractor" /> 提取器集合
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, IHttpDeclarativeExtractor> _extractors = new([
        new(typeof(BaseAddressDeclarativeExtractor), new BaseAddressDeclarativeExtractor()),
        new(typeof(ValidationDeclarativeExtractor), new ValidationDeclarativeExtractor()),
        new(typeof(AutoSetHostHeaderDeclarativeExtractor), new AutoSetHostHeaderDeclarativeExtractor()),
        new(typeof(PerformanceOptimizationDeclarativeExtractor), new PerformanceOptimizationDeclarativeExtractor()),
        new(typeof(HttpClientNameDeclarativeExtractor), new HttpClientNameDeclarativeExtractor()),
        new(typeof(TraceIdentifierDeclarativeExtractor), new TraceIdentifierDeclarativeExtractor()),
        new(typeof(ProfilerDeclarativeExtractor), new ProfilerDeclarativeExtractor()),
        new(typeof(SimulateBrowserDeclarativeExtractor), new SimulateBrowserDeclarativeExtractor()),
        new(typeof(AcceptLanguageDeclarativeExtractor), new AcceptLanguageDeclarativeExtractor()),
        new(typeof(DisableCacheDeclarativeExtractor), new DisableCacheDeclarativeExtractor()),
        new(typeof(EnsureSuccessStatusCodeDeclarativeExtractor), new EnsureSuccessStatusCodeDeclarativeExtractor()),
        new(typeof(RetryDeclarativeExtractor), new RetryDeclarativeExtractor()),
        new(typeof(TimeoutDeclarativeExtractor), new TimeoutDeclarativeExtractor()),
        new(typeof(PathSegmentDeclarativeExtractor), new PathSegmentDeclarativeExtractor()),
        new(typeof(QueryParamDeclarativeExtractor), new QueryParamDeclarativeExtractor()),
        new(typeof(PathDeclarativeExtractor), new PathDeclarativeExtractor()),
        new(typeof(CookieDeclarativeExtractor), new CookieDeclarativeExtractor()),
        new(typeof(RefererDeclarativeExtractor), new RefererDeclarativeExtractor()),
        new(typeof(HeaderDeclarativeExtractor), new HeaderDeclarativeExtractor()),
        new(typeof(PropertyDeclarativeExtractor), new PropertyDeclarativeExtractor()),
        new(typeof(HttpVersionDeclarativeExtractor), new HttpVersionDeclarativeExtractor()),
        new(typeof(SuppressExceptionsDeclarativeExtractor), new SuppressExceptionsDeclarativeExtractor()),
        new(typeof(RequestEventHandlerDeclarativeExtractor), new RequestEventHandlerDeclarativeExtractor()),
        new(typeof(JsonResponseWrapperDeclarativeExtractor), new JsonResponseWrapperDeclarativeExtractor()),
        new(typeof(JsonResponseStringUnwrapDeclarativeExtractor), new JsonResponseStringUnwrapDeclarativeExtractor()),
        new(typeof(SuppressTokenManagementDeclarativeExtractor), new SuppressTokenManagementDeclarativeExtractor()),
        new(typeof(RemoveTrailingSlashDeclarativeExtractor), new RemoveTrailingSlashDeclarativeExtractor()),
        new(typeof(QuotaKeyDeclarativeExtractor), new QuotaKeyDeclarativeExtractor()),
        new(typeof(BodyDeclarativeExtractor), new BodyDeclarativeExtractor())
    ]);

    /// <summary>
    ///     HTTP 声明式 <see cref="IHttpDeclarativeExtractor" /> 提取器集合（冻结）
    /// </summary>
    /// <remarks>该集合用于确保某些 HTTP 声明式提取器始终位于最后。</remarks>
    internal static readonly ConcurrentDictionary<Type, IFrozenHttpDeclarativeExtractor> _frozenExtractors = new([
        new(typeof(HttpRequestMessageDeclarativeExtractor), new HttpRequestMessageDeclarativeExtractor()),
        new(typeof(MultipartDeclarativeExtractor), new MultipartDeclarativeExtractor()),
        new(typeof(HttpMultipartFormDataBuilderDeclarativeExtractor),
            new HttpMultipartFormDataBuilderDeclarativeExtractor()),
        new(typeof(HttpRequestBuilderDeclarativeExtractor), new HttpRequestBuilderDeclarativeExtractor())
    ]);

    /// <summary>
    ///     标识是否已加载自定义 HTTP 声明式提取器
    /// </summary>
    internal static int _hasLoadedExtractors;

    /// <summary>
    ///     HTTP 声明式合并后的提取器缓存
    /// </summary>
    internal static volatile Lazy<IHttpDeclarativeExtractor[]> _lazyExtractors =
        new(() => _extractors.Values.Concat(_frozenExtractors.Values.OrderByDescending(e => e.Order)).ToArray(),
            LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     <inheritdoc cref="HttpDeclarativeBuilder" />
    /// </summary>
    /// <param name="method">被调用方法</param>
    /// <param name="args">被调用方法的参数值数组</param>
    /// <param name="interfaceType">实际被代理的接口类型</param>
    internal HttpDeclarativeBuilder(MethodInfo method, object?[] args, Type? interfaceType = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(args);

        Method = method;
        Args = args;
        InterfaceType = interfaceType ?? Method.DeclaringType ?? throw new ArgumentNullException(nameof(interfaceType));
    }

    /// <summary>
    ///     被调用方法
    /// </summary>
    public MethodInfo Method { get; }

    /// <summary>
    ///     被调用方法的参数值数组
    /// </summary>
    public object?[] Args { get; }

    /// <summary>
    ///     实际被代理的接口类型
    /// </summary>
    public Type InterfaceType { get; }

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal HttpRequestBuilder Build(HttpRemoteOptions httpRemoteOptions, IServiceProvider? serviceProvider)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);

        // 初始化方法信息友好字符串
        var declaringType = Method.DeclaringType;
        var declaringTypeFriendlyString = declaringType?.ToFriendlyString();
        var methodFriendlyString = Method.ToFriendlyString();

        // 获取 HttpMethodAttribute 实例并检查被调用方法是否贴有 [HttpMethod] 特性
        var httpMethodAttribute = Method.GetCustomAttribute<HttpMethodAttribute>(true);
        if (httpMethodAttribute is null)
        {
            throw new InvalidOperationException(
                $"No `[HttpMethod]` annotation was found in method `{methodFriendlyString}` of type `{declaringTypeFriendlyString}`.");
        }

        // 初始化 HttpRequestBuilder 实例并添加声明式方法签名
        var httpRequestBuilder = HttpRequestBuilder
            .Create(httpMethodAttribute.HttpMethod, httpMethodAttribute.RequestUri).WithProperty(
                Constants.DECLARATIVE_METHOD_KEY,
                $"\e[36m\e[3m{methodFriendlyString} | {declaringTypeFriendlyString}{(InterfaceType != declaringType ? $" | {InterfaceType.ToFriendlyString()}" : string.Empty)}\e[0m");

        // 初始化 HttpDeclarativeExtractorContext 实例
        var httpDeclarativeExtractorContext = new HttpDeclarativeExtractorContext(Method, Args,
            new HttpDeclarativeMethodMetadata(Method, InterfaceType), serviceProvider);

        // 检查是否已加载自定义 HTTP 声明式提取器
        if (Interlocked.CompareExchange(ref _hasLoadedExtractors, 1, 0) == 0)
        {
            // 批量添加自定义 HTTP 声明式提取器列表
            var customExtractors = httpRemoteOptions.HttpDeclarativeExtractors?.SelectMany(u => u.Invoke()) ?? [];
            foreach (var extractor in customExtractors)
            {
                // 获取 HTTP 声明式提取器类型
                var extractorType = extractor.GetType();

                // 检查 HTTP 声明式提取器是否实现 IFrozenHttpDeclarativeExtractor 接口
                if (extractor is IFrozenHttpDeclarativeExtractor frozenExtractor)
                {
                    _frozenExtractors.TryAdd(extractorType, frozenExtractor);
                }
                else
                {
                    _extractors.TryAdd(extractorType, extractor);
                }
            }

            // 自定义提取器已变更，重建 Lazy 缓存
            var newLazy = new Lazy<IHttpDeclarativeExtractor[]>(
                () => _extractors.Values.Concat(_frozenExtractors.Values.OrderByDescending(e => e.Order)).ToArray(),
                LazyThreadSafetyMode.ExecutionAndPublication);

            Interlocked.Exchange(ref _lazyExtractors, newLazy);
        }

        // 获取当前合并后的提取器列表
        var extractors = _lazyExtractors.Value;

        // 遍历 HTTP 声明式提取器集合
        foreach (var extractor in extractors)
        {
            // 提取方法信息构建 HttpRequestBuilder 实例
            extractor.Extract(httpRequestBuilder, httpDeclarativeExtractorContext);
        }

        return httpRequestBuilder;
    }
}