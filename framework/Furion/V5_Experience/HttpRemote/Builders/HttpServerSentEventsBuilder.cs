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

namespace Furion.HttpRemote;

/// <summary>
///     HTTP Server-Sent Events 构建器
/// </summary>
/// <remarks>
///     <para>使用 <c>HttpRequestBuilder.ServerSentEvents(requestUri, onMessage)</c> 静态方法创建。</para>
///     <para>参考文献：https://developer.mozilla.org/zh-CN/docs/Web/API/Server-sent_events/Using_server-sent_events。</para>
/// </remarks>
public sealed class HttpServerSentEventsBuilder : HttpRequestBuilderConfigurator<HttpServerSentEventsBuilder>
{
    /// <summary>
    ///     <inheritdoc cref="HttpServerSentEventsBuilder" />
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    internal HttpServerSentEventsBuilder(Uri? requestUri)
        : this(HttpMethod.Get, requestUri)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="HttpServerSentEventsBuilder" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpServerSentEventsBuilder(HttpMethod httpMethod, Uri? requestUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);

        HttpMethod = httpMethod;
        RequestUri = requestUri;
    }

    /// <summary>
    ///     请求地址
    /// </summary>
    public Uri? RequestUri { get; }

    /// <summary>
    ///     请求方式
    /// </summary>
    /// <remarks>默认请求为：<c>GET</c>。</remarks>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    ///     默认重新连接的间隔时间（毫秒）
    /// </summary>
    /// <remarks>默认值为 2000 毫秒。</remarks>
    public int DefaultRetryInterval { get; private set; } = 2000;

    /// <summary>
    ///     最大重试次数
    /// </summary>
    /// <remarks>默认最大重试次数为 100。</remarks>
    public int MaxRetries { get; private set; } = 100;

    /// <summary>
    ///     用于在与事件源的连接打开时的操作
    /// </summary>
    public Action? OnOpen { get; private set; }

    /// <summary>
    ///     用于在从事件源接收到数据时的操作
    /// </summary>
    public Func<ServerSentEventsData, CancellationToken, Task>? OnMessage { get; private set; }

    /// <summary>
    ///     用于在事件源连接未能打开时的操作
    /// </summary>
    public Action<Exception>? OnError { get; private set; }

    /// <summary>
    ///     实现 <see cref="IHttpServerSentEventsEventHandler" /> 的类型
    /// </summary>
    internal Type? ServerSentEventsEventHandlerType { get; private set; }

    /// <summary>
    ///     是否自动修正请求方法
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool AutoCorrectMethod { get; private set; } = true;

    /// <summary>
    ///     设置默认重新连接的间隔时间
    /// </summary>
    /// <param name="retryInterval">默认重新连接的间隔时间</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpServerSentEventsBuilder SetDefaultRetryInterval(int retryInterval)
    {
        // 小于或等于 0 检查
        if (retryInterval <= 0)
        {
            throw new ArgumentException("Retry interval must be greater than 0.", nameof(retryInterval));
        }

        DefaultRetryInterval = retryInterval;

        return this;
    }

    /// <summary>
    ///     设置最大重试次数
    /// </summary>
    /// <param name="maxRetries">最大重试次数</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpServerSentEventsBuilder SetMaxRetries(int maxRetries)
    {
        // 小于或等于 0 检查
        if (maxRetries <= 0)
        {
            throw new ArgumentException("Max retries must be greater than 0.", nameof(maxRetries));
        }

        MaxRetries = maxRetries;

        return this;
    }

    /// <summary>
    ///     设置用于在与事件源的连接打开时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpServerSentEventsBuilder SetOnOpen(Action configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnOpen = configure;

        return this;
    }

    /// <summary>
    ///     设置用于在从事件源接收到数据时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpServerSentEventsBuilder SetOnMessage(Func<ServerSentEventsData, CancellationToken, Task> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnMessage = configure;

        return this;
    }

    /// <summary>
    ///     设置用于在事件源连接未能打开时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpServerSentEventsBuilder SetOnError(Action<Exception> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnError = configure;

        return this;
    }

    /// <summary>
    ///     设置 Server-Sent Events 事件处理程序
    /// </summary>
    /// <param name="serverSentEventsEventHandlerType">实现 <see cref="IHttpServerSentEventsEventHandler" /> 接口的类型</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public HttpServerSentEventsBuilder SetEventHandler(Type serverSentEventsEventHandlerType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serverSentEventsEventHandlerType);

        // 检查类型是否实现了 IHttpServerSentEventsEventHandler 接口
        if (!typeof(IHttpServerSentEventsEventHandler).IsAssignableFrom(serverSentEventsEventHandlerType))
        {
            throw new ArgumentException(
                $"`{serverSentEventsEventHandlerType}` type is not assignable from `{typeof(IHttpServerSentEventsEventHandler)}`.",
                nameof(serverSentEventsEventHandlerType));
        }

        ServerSentEventsEventHandlerType = serverSentEventsEventHandlerType;

        return this;
    }

    /// <summary>
    ///     设置 Server-Sent Events 事件处理程序
    /// </summary>
    /// <typeparam name="TServerSentEventsEventHandler">
    ///     <see cref="IHttpServerSentEventsEventHandler" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public HttpServerSentEventsBuilder SetEventHandler<TServerSentEventsEventHandler>()
        where TServerSentEventsEventHandler : IHttpServerSentEventsEventHandler =>
        SetEventHandler(typeof(TServerSentEventsEventHandler));

    /// <summary>
    ///     设置是否自动修正请求方法
    /// </summary>
    /// <param name="enabled">若为 <c>true</c> 则当请求为 GET 或 HEAD 且包含请求内容时，自动将方法改为 POST；默认值为：<c>true</c>。</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public HttpServerSentEventsBuilder SetAutoCorrectMethod(bool enabled)
    {
        AutoCorrectMethod = enabled;

        return this;
    }

    /// <summary>
    ///     设置 MCP (Model Context Protocol) 2.0 消息内容
    /// </summary>
    /// <param name="clientName">客户端名称</param>
    /// <param name="method">方法名称</param>
    /// <param name="params">方法参数</param>
    /// <param name="protocolVersion"> MCP (Model Context Protocol) 协议版本，默认为 "2026-07-28"</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public HttpServerSentEventsBuilder SetMcpContent(string clientName, string method, object? @params,
        string protocolVersion = "2026-07-28") =>
        With(builder => builder.SetMcpContent(clientName, method, @params, protocolVersion));

    /// <summary>
    ///     设置 MCP (Model Context Protocol) 2.0 消息内容
    /// </summary>
    /// <param name="clientName">客户端名称</param>
    /// <param name="data">
    ///     <see cref="McpMessageData" />
    /// </param>
    /// <param name="protocolVersion"> MCP (Model Context Protocol) 协议版本，默认为 "2026-07-28"</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public HttpServerSentEventsBuilder SetMcpContent(string clientName, McpMessageData data,
        string protocolVersion = "2026-07-28") =>
        With(builder => builder.SetMcpContent(clientName, data, protocolVersion));

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpRequestBuilder Build(HttpRemoteOptions httpRemoteOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);

        // 初始化 HttpRequestBuilder 实例，并确保请求标头中添加了 Accept: text/event-stream；同时启用 HttpClient 池化管理；如果请求失败，则应抛出异常
        var httpRequestBuilder = HttpRequestBuilder.Create(HttpMethod, RequestUri)
            .WithHeader(nameof(HttpRequestHeaders.Accept), "text/event-stream", replace: true).DisableCache()
            .UseHttpClientPool().EnsureSuccessStatusCode();

        // 检查是否设置了事件处理程序且该处理程序实现了 IHttpRequestEventHandler 接口，如果有则设置给 httpRequestBuilder
        if (ServerSentEventsEventHandlerType is not null &&
            typeof(IHttpRequestEventHandler).IsAssignableFrom(ServerSentEventsEventHandlerType))
        {
            httpRequestBuilder.SetEventHandler(ServerSentEventsEventHandlerType);
        }

        // 调用自定义配置委托
        Configure?.Invoke(httpRequestBuilder);

        // 自动修正携带请求内容的 GET 或 HEAD 请求，并将请求方式重设为 POST
        if (AutoCorrectMethod && httpRequestBuilder.RawContent is not null &&
            (httpRequestBuilder.HttpMethod == HttpMethod.Get || httpRequestBuilder.HttpMethod == HttpMethod.Head))
        {
            httpRequestBuilder.SetHttpMethod(HttpMethod.Post);
        }

        return httpRequestBuilder;
    }
}