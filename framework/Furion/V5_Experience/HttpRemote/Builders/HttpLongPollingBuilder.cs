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

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 长轮询构建器
/// </summary>
/// <remarks>使用 <c>HttpRequestBuilder.LongPolling(httpMethod, requestUri, onDataReceived)</c> 静态方法创建。</remarks>
public sealed class HttpLongPollingBuilder : HttpRequestBuilderConfigurator<HttpLongPollingBuilder>
{
    /// <summary>
    ///     <inheritdoc cref="HttpLongPollingBuilder" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpLongPollingBuilder(HttpMethod httpMethod, Uri? requestUri)
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
    public HttpMethod HttpMethod { get; }

    /// <summary>
    ///     轮询重试间隔
    /// </summary>
    /// <remarks>默认值为 2 秒。</remarks>
    public TimeSpan RetryInterval { get; private set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     最大重试次数
    /// </summary>
    /// <remarks>默认最大重试次数为 100。</remarks>
    public int MaxRetries { get; private set; } = 100;

    /// <summary>
    ///     用于接收服务器返回 <c>200~299</c> 状态码的数据的操作
    /// </summary>
    public Func<HttpResponseMessage, CancellationToken, Task>? OnDataReceived { get; private set; }

    /// <summary>
    ///     用于接收服务器返回非 <c>200~299</c> 状态码的数据的操作
    /// </summary>
    public Func<HttpResponseMessage, CancellationToken, Task>? OnError { get; private set; }

    /// <summary>
    ///     用于响应标头包含 <c>X-End-Of-Stream</c> 时触发的操作
    /// </summary>
    public Func<HttpResponseMessage, CancellationToken, Task>? OnEndOfStream { get; private set; }

    /// <summary>
    ///     实现 <see cref="IHttpLongPollingEventHandler" /> 的类型
    /// </summary>
    internal Type? LongPollingEventHandlerType { get; private set; }

    /// <summary>
    ///     设置轮询重试间隔
    /// </summary>
    /// <param name="retryInterval">轮询重试间隔</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpLongPollingBuilder SetRetryInterval(TimeSpan retryInterval)
    {
        // 小于或等于 0 检查
        if (retryInterval <= TimeSpan.Zero)
        {
            throw new ArgumentException("Retry interval must be greater than 0.", nameof(retryInterval));
        }

        RetryInterval = retryInterval;

        return this;
    }

    /// <summary>
    ///     设置最大重试次数
    /// </summary>
    /// <param name="maxRetries">最大重试次数</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpLongPollingBuilder SetMaxRetries(int maxRetries)
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
    ///     设置在接收服务器返回 <c>200~299</c> 状态码的数据的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpLongPollingBuilder SetOnDataReceived(Func<HttpResponseMessage, CancellationToken, Task> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnDataReceived = configure;

        return this;
    }

    /// <summary>
    ///     设置在接收服务器返回非 <c>200~299</c> 状态码的数据的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpLongPollingBuilder SetOnError(Func<HttpResponseMessage, CancellationToken, Task> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnError = configure;

        return this;
    }

    /// <summary>
    ///     设置在响应标头包含 <c>X-End-Of-Stream</c> 时触发的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpLongPollingBuilder SetOnEndOfStream(Func<HttpResponseMessage, CancellationToken, Task> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnEndOfStream = configure;

        return this;
    }

    /// <summary>
    ///     设置长轮询事件处理程序
    /// </summary>
    /// <param name="longPollingEventHandlerType">实现 <see cref="IHttpLongPollingEventHandler" /> 接口的类型</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public HttpLongPollingBuilder SetEventHandler(Type longPollingEventHandlerType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(longPollingEventHandlerType);

        // 检查类型是否实现了 IHttpLongPollingEventHandler 接口
        if (!typeof(IHttpLongPollingEventHandler).IsAssignableFrom(longPollingEventHandlerType))
        {
            throw new ArgumentException(
                $"`{longPollingEventHandlerType}` type is not assignable from `{typeof(IHttpLongPollingEventHandler)}`.",
                nameof(longPollingEventHandlerType));
        }

        LongPollingEventHandlerType = longPollingEventHandlerType;

        return this;
    }

    /// <summary>
    ///     设置长轮询事件处理程序
    /// </summary>
    /// <typeparam name="TLongPollingEventHandler">
    ///     <see cref="IHttpLongPollingEventHandler" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public HttpLongPollingBuilder SetEventHandler<TLongPollingEventHandler>()
        where TLongPollingEventHandler : IHttpLongPollingEventHandler =>
        SetEventHandler(typeof(TLongPollingEventHandler));

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

        // 初始化 HttpRequestBuilder 实例
        var httpRequestBuilder = HttpRequestBuilder.Create(HttpMethod, RequestUri).DisableCache();

        // 检查是否设置了事件处理程序且该处理程序实现了 IHttpRequestEventHandler 接口，如果有则设置给 httpRequestBuilder
        if (LongPollingEventHandlerType is not null &&
            typeof(IHttpRequestEventHandler).IsAssignableFrom(LongPollingEventHandlerType))
        {
            httpRequestBuilder.SetEventHandler(LongPollingEventHandlerType);
        }

        // 调用自定义配置委托
        Configure?.Invoke(httpRequestBuilder);

        return httpRequestBuilder;
    }
}