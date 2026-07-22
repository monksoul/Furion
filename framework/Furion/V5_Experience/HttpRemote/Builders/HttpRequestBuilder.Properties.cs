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

using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     <see cref="HttpRequestBuilder" /> 类型的所有实例属性信息
    /// </summary>
    /// <remarks>避免在每次调用 <see cref="Clone" /> 方法时重复进行反射操作，提升性能。</remarks>
    internal static readonly Lazy<PropertyInfo[]> _cachedProperties = new(() =>
        typeof(HttpRequestBuilder).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                                 BindingFlags.DeclaredOnly));

    /// <inheritdoc cref="OnPostReceiveResponse" />
    internal Action<HttpResponseMessage>? _onPostReceiveResponse;

    /// <inheritdoc cref="OnPreSendRequest" />
    internal Action<HttpRequestMessage>? _onPreSendRequest;

    /// <inheritdoc cref="OnPreSetContent" />
    internal Action<HttpContent>? _onPreSetContent;

    /// <inheritdoc cref="UriBuilderConfigure" />
    internal Action<UriBuilder>? _uriBuilderConfigure;

    /// <summary>
    ///     请求地址
    /// </summary>
    public Uri? RequestUri { get; private set; }

    /// <summary>
    ///     请求方式
    /// </summary>
    public HttpMethod? HttpMethod { get; private set; }

    /// <summary>
    ///     跟踪标识
    /// </summary>
    /// <remarks>
    ///     <para>可为每个请求指定唯一标识符，用于请求的跟踪和调试。</para>
    ///     <para>唯一标识符将在 <see cref="HttpRequestMessage" /> 类型实例的 <c>Headers</c> 属性中通过 <c>X-Trace-ID</c> 作为键指定。</para>
    /// </remarks>
    public string? TraceIdentifier { get; private set; }

    /// <summary>
    ///     内容类型
    /// </summary>
    public string? ContentType { get; private set; }

    /// <summary>
    ///     内容编码
    /// </summary>
    public Encoding? ContentEncoding { get; private set; }

    /// <summary>
    ///     原始请求内容
    /// </summary>
    /// <remarks>此属性值最终将转换为 <see cref="HttpContent" /> 类型实例。</remarks>
    public object? RawContent { get; private set; }

    /// <summary>
    ///     请求标头集合
    /// </summary>
    public IDictionary<string, List<string?>>? Headers { get; private set; }

    /// <summary>
    ///     需要从请求中移除的标头集合
    /// </summary>
    public HashSet<string>? HeadersToRemove { get; private set; }

    /// <summary>
    ///     片段标识符
    /// </summary>
    /// <remarks>请求地址中的 <c>#</c> 符号后面的部分。</remarks>
    public string? Fragment { get; private set; }

    /// <summary>
    ///     路径片段集合
    /// </summary>
    /// <remarks>请求地址中位于主机端口之后且 <c>?</c> 符号之前的部分。</remarks>
    public List<string>? PathSegments { get; private set; }

    /// <summary>
    ///     需要从 URL 中移除的路径片段集合
    /// </summary>
    public HashSet<string>? PathSegmentsToRemove { get; private set; }

    /// <summary>
    ///     查询参数集合
    /// </summary>
    /// <remarks>请求地址中位于 <c>?</c> 符号之后且 <c>#</c> 符号之前的部分。</remarks>
    public IDictionary<string, List<object?>>? QueryParameters { get; private set; }

    /// <summary>
    ///     查询参数排序委托
    /// </summary>
    /// <remarks>接收所有查询键值对（包括原始 URL 参数和用户追加的参数），返回排序后的键值对序列。为 <c>null</c> 时不执行排序，保持参数的添加顺序。</remarks>
    public Func<IEnumerable<KeyValuePair<string, string?>>, IEnumerable<KeyValuePair<string, string?>>>?
        QueryParametersSorter { get; private set; }

    /// <summary>
    ///     需要从 URL 中移除的查询参数集合
    /// </summary>
    public HashSet<string>? QueryParametersToRemove { get; private set; }

    /// <summary>
    ///     路径参数集合
    /// </summary>
    /// <remarks>用于替换请求地址中符合 <c>\{\s*(\w+\s*(\.\s*\w+\s*)*)\s*\}</c> 正则表达式匹配的数据。</remarks>
    public IDictionary<string, string?>? PathParameters { get; private set; }

    /// <summary>
    ///     路径参数集合
    /// </summary>
    /// <remarks>支持自定义类类型。用于替换请求地址中符合 <c>\{\s*(\w+\s*(\.\s*\w+\s*)*)\s*\}</c> 正则表达式匹配的数据。</remarks>
    public IDictionary<string, object?>? ObjectPathParameters { get; private set; }

    /// <summary>
    ///     Cookies 集合
    /// </summary>
    /// <remarks>
    ///     <para>可为单次请求设置 Cookies。</para>
    ///     <para>Cookies 将在 <see cref="HttpRequestMessage" /> 类型实例的 <c>Headers</c> 属性中通过 <c>Cookie</c> 作为键指定。</para>
    ///     <para>使用该方式不会自动处理服务器返回的 <c>Set-Cookie</c> 头。</para>
    /// </remarks>
    public IDictionary<string, string?>? Cookies { get; private set; }

    /// <summary>
    ///     需要从请求中移除的 Cookie 集合
    /// </summary>
    public HashSet<string>? CookiesToRemove { get; private set; }

    /// <summary>
    ///     <see cref="HttpClient" /> 实例的配置名称
    /// </summary>
    /// <remarks>
    ///     <para>此属性用于指定 <see cref="IHttpClientFactory" /> 创建 <see cref="HttpClient" /> 实例时传递的名称。</para>
    ///     <para>该名称用于标识在服务容器中与特定 <see cref="HttpClient" /> 实例相关的配置。</para>
    /// </remarks>
    public string? HttpClientName { get; private set; }

    /// <summary>
    ///     响应内容最大缓存字节数
    /// </summary>
    /// <remarks>可为单次请求设置最大缓存字节数。</remarks>
    public long? MaxResponseContentBufferSize { get; private set; }

    /// <summary>
    ///     身份验证凭据请求授权标头
    /// </summary>
    /// <remarks>可为单次请求设置身份验证凭据请求授权标头。</remarks>
    public AuthenticationHeaderValue? AuthenticationHeader { get; private set; }

    /// <summary>
    ///     <see cref="HttpRequestMessage" /> 请求属性集合
    /// </summary>
    /// <remarks>用于添加 <see cref="HttpRequestMessage" /> 请求属性。该值将合并到 <c>HttpRequestMessage.Options</c> 属性中。</remarks>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public IDictionary<string, object?> Properties { get; private set; } =
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     请求基地址
    /// </summary>
    public Uri? BaseAddress { get; private set; }

    /// <summary>
    ///     HTTP 版本
    /// </summary>
    public Version? Version { get; private set; }

    /// <summary>
    ///     <see cref="HttpClient" /> 实例提供器
    /// </summary>
    /// <value>
    ///     <para>返回一个包含 <see cref="HttpClient" /> 实例及其释放方法的委托。</para>
    ///     <para>释放方法的委托用于在不再需要 <see cref="HttpClient" /> 实例时释放资源。</para>
    /// </value>
    public Func<(HttpClient Instance, Action<HttpClient>? Release)>? HttpClientProvider { get; private set; }

    /// <summary>
    ///     <see cref="IHttpContentProcessor" /> 集合提供器
    /// </summary>
    /// <value>返回多个包含实现 <see cref="IHttpContentProcessor" /> 集合的集合。</value>
    public IList<Func<IEnumerable<IHttpContentProcessor>>>? HttpContentProcessorProviders { get; private set; }

    /// <summary>
    ///     <see cref="IHttpContentConverter" /> 集合提供器
    /// </summary>
    /// <value>返回多个包含实现 <see cref="IHttpContentConverter" /> 集合的集合。</value>
    public IList<Func<IEnumerable<IHttpContentConverter>>>? HttpContentConverterProviders { get; private set; }

    /// <summary>
    ///     用于处理在设置 <see cref="HttpRequestMessage" /> 的请求消息的内容时的操作
    /// </summary>
    public Action<HttpContent>? OnPreSetContent { get => _onPreSetContent; private set => _onPreSetContent = value; }

    /// <summary>
    ///     用于处理在发送 HTTP 请求之前的操作
    /// </summary>
    public Action<HttpRequestMessage>? OnPreSendRequest
    {
        get => _onPreSendRequest;
        private set => _onPreSendRequest = value;
    }

    /// <summary>
    ///     用于处理在收到 HTTP 响应之后的操作
    /// </summary>
    public Action<HttpResponseMessage>? OnPostReceiveResponse
    {
        get => _onPostReceiveResponse;
        private set => _onPostReceiveResponse = value;
    }

    /// <summary>
    ///     用于处理在发送 HTTP 请求发生异常时的操作
    /// </summary>
    public Action<Exception, HttpResponseMessage?>? OnRequestFailed { get; private set; }

    /// <summary>
    ///     <inheritdoc cref="HttpMultipartFormDataBuilder" />
    /// </summary>
    public HttpMultipartFormDataBuilder? MultipartFormDataBuilder { get; private set; }

    /// <summary>
    ///     是否移除默认的内容的 <c>Content-Type</c>
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool OmitContentType { get; private set; }

    /// <summary>
    ///     HTTP 完成选项
    /// </summary>
    /// <remarks>控制发送请求返回响应的时机。</remarks>
    public HttpCompletionOption? CompletionOption { get; private set; }

    /// <summary>
    ///     HTTP 请求重试选项
    /// </summary>
    public HttpRetryOptions? RetryOptions { get; private set; }

    /// <summary>
    ///     HTTP 请求超时选项
    /// </summary>
    public HttpTimeoutOptions? TimeoutOptions { get; private set; }

    /// <summary>
    ///     获取 Access Token 时传递的自定义数据
    /// </summary>
    /// <remarks>该数据将被复制到 <see cref="HttpAccessTokenContext.Items" /> 中，供 <see cref="IHttpAccessTokenProvider" /> 使用。</remarks>
    public IDictionary<string, object?>? AccessTokenData { get; private set; }

    /// <summary>
    ///     如果 HTTP 响应的 <c>IsSuccessStatusCode</c> 属性是 <c>false</c>，则引发异常。
    /// </summary>
    /// <remarks>默认值为 <c>false</c>。</remarks>
    internal bool EnsureSuccessStatusCodeEnabled { get; private set; }

    /// <summary>
    ///     是否禁用 HTTP 缓存
    /// </summary>
    /// <remarks>可为单次请求设置禁用 HTTP 缓存。默认值为：<c>false</c>。</remarks>
    internal bool DisableCacheEnabled { get; private set; }

    /// <summary>
    ///     实现 <see cref="IHttpRequestEventHandler" /> 的类型
    /// </summary>
    internal Type? RequestEventHandlerType { get; private set; }

    /// <summary>
    ///     用于请求结束时需要释放的对象集合
    /// </summary>
    internal List<IDisposable>? Disposables { get; private set; }

    /// <summary>
    ///     <see cref="HttpClient" /> 实例管理器
    /// </summary>
    internal HttpClientPooling? HttpClientPooling { get; set; }

    /// <summary>
    ///     是否启用 <see cref="HttpClient" /> 的池化管理
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    internal bool HttpClientPoolingEnabled { get; private set; }

    /// <summary>
    ///     是否启用请求分析工具
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    internal bool ProfilerEnabled { get; private set; }

    /// <summary>
    ///     请求分析工具委托
    /// </summary>
    internal Action<HttpRemoteAnalyzer>? ProfilerPredicate { get; private set; }

    /// <summary>
    ///     是否启用性能优化
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    internal bool PerformanceOptimizationEnabled { get; private set; }

    /// <summary>
    ///     是否自动设置 <c>Host</c> 标头
    /// </summary>
    /// <remarks><c>Host</c> 标头是 <c>HTTP/1.1</c> 协议中的一个必需标头。默认值为：<c>false</c>，表示不默认添加 <c>Host</c> 标头。</remarks>
    internal bool AutoSetHostHeaderEnabled { get; private set; }

    /// <summary>
    ///     表示禁用请求分析工具标识
    /// </summary>
    /// <remarks>用于禁用全局请求分析工具。配置信息将写入到 <see cref="HttpRequestMessage.Options" /> 中。默认值为：<c>false</c>。</remarks>
    internal bool ProfilerDisabled { get; private set; }

    /// <summary>
    ///     状态码处理程序
    /// </summary>
    internal IDictionary<IEnumerable<object>, Func<HttpResponseMessage, CancellationToken, Task>>? StatusCodeHandlers
    {
        get;
        private set;
    }

    /// <summary>
    ///     异常抑制类型集合
    /// </summary>
    /// <remarks>当配置了异常抑制类型集合后，框架将抑制（即不抛出）该集合中匹配的异常类型。</remarks>
    internal HashSet<Type>? SuppressExceptionTypes { get; private set; }

    /// <summary>
    ///     是否开启断言功能
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    internal bool AssertionsEnabled { get; private set; }

    /// <summary>
    ///     断言委托集合
    /// </summary>
    internal List<HttpAssertion>? Assertions { get; private set; }

    /// <summary>
    ///     用于处理在构建最终请求 URL 的操作
    /// </summary>
    public Action<UriBuilder>? UriBuilderConfigure
    {
        get => _uriBuilderConfigure;
        private set => _uriBuilderConfigure = value;
    }

    /// <summary>
    ///     表示启用 JSON 响应反序列化包装器
    /// </summary>
    /// <remarks>
    ///     当配置了 <see cref="HttpClientOptions.JsonResponseWrapper" /> 时有效。配置信息将写入到
    ///     <see cref="HttpRequestMessage.Options" /> 中。默认值为：null。
    /// </remarks>
    internal bool? JsonResponseWrapperEnabled { get; private set; }

    /// <summary>
    ///     表示启用 JSON 响应内容字符串的解包处理（双重序列化）
    /// </summary>
    /// <remarks>
    ///     配置信息将写入到 <see cref="HttpRequestMessage.Options" /> 中。默认值为：null。
    /// </remarks>
    internal bool? JsonResponseStringUnwrapEnabled { get; private set; }

    /// <summary>
    ///     表示是否跳过框架的 Access Token 自动管理
    /// </summary>
    internal bool SuppressTokenManagement { get; private set; }
}