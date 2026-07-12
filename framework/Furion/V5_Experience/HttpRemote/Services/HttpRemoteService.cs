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
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     <inheritdoc cref="IHttpRemoteService" />
/// </summary>
internal sealed partial class HttpRemoteService : IHttpRemoteService
{
    /// <inheritdoc cref="IHttpClientFactory" />
    internal readonly IHttpClientFactory _httpClientFactory;

    /// <inheritdoc cref="IHttpContentConverterFactory" />
    internal readonly IHttpContentConverterFactory _httpContentConverterFactory;

    /// <inheritdoc cref="IHttpContentProcessorFactory" />
    internal readonly IHttpContentProcessorFactory _httpContentProcessorFactory;

    /// <inheritdoc cref="HttpRemoteOptions" />
    internal readonly HttpRemoteOptions _httpRemoteOptions;

    /// <inheritdoc cref="IHttpRemoteLogger" />
    internal readonly IHttpRemoteLogger _logger;

    /// <summary>
    ///     <inheritdoc cref="HttpRemoteService" />
    /// </summary>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <param name="logger">
    ///     <see cref="IHttpRemoteLogger" />
    /// </param>
    /// <param name="httpClientFactory">
    ///     <see cref="IHttpClientFactory" />
    /// </param>
    /// <param name="httpContentProcessorFactory">
    ///     <see cref="IHttpContentProcessorFactory" />
    /// </param>
    /// <param name="httpContentConverterFactory">
    ///     <see cref="IHttpContentConverterFactory" />
    /// </param>
    /// <param name="httpRemoteOptions">
    ///     <see cref="IOptions{TOptions}" />
    /// </param>
    public HttpRemoteService(IServiceProvider serviceProvider, IHttpRemoteLogger logger,
        IHttpClientFactory httpClientFactory,
        IHttpContentProcessorFactory httpContentProcessorFactory,
        IHttpContentConverterFactory httpContentConverterFactory,
        IOptions<HttpRemoteOptions> httpRemoteOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(httpContentProcessorFactory);
        ArgumentNullException.ThrowIfNull(httpContentConverterFactory);
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);

        ServiceProvider = serviceProvider;
        _httpRemoteOptions = httpRemoteOptions.Value;

        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _httpContentProcessorFactory = httpContentProcessorFactory;
        _httpContentConverterFactory = httpContentConverterFactory;
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public HttpResponseMessage? Send(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        Send(httpRequestBuilder, HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage? Send(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, _) = AsyncUtility.RunSync(() => SendCoreAsync(httpRequestBuilder, completionOption,
            null, (httpClient, httpRequestMessage, option, token) => httpClient.Send(httpRequestMessage, option, token),
            cancellationToken));

        return httpResponseMessage;
    }

    /// <inheritdoc />
    public Task<HttpResponseMessage?> SendAsync(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAsync(httpRequestBuilder, HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpResponseMessage?> SendAsync(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, _) = await SendCoreAsync(httpRequestBuilder, completionOption,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.SendAsync(httpRequestMessage, option, token), null, cancellationToken);

        return httpResponseMessage;
    }

    /// <inheritdoc />
    public TResult? SendAs<TResult>(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAs<TResult>(httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public TResult? SendAs<TResult>(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = AsyncUtility.RunSync(() => SendCoreAsync(httpRequestBuilder,
            completionOption, null,
            (httpClient, httpRequestMessage, option, token) => httpClient.Send(httpRequestMessage, option, token),
            cancellationToken));

        // 获取结果类型
        var resultType = typeof(TResult);

        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(resultType))
        {
            // 将 HttpResponseMessage 转换为 TResult 实例
            try
            {
                return _httpContentConverterFactory.Read<TResult>(httpResponseMessage,
                    httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
                    cancellationToken);
            }
            finally
            {
                // 释放 httpResponseMessage 实例
                if (httpResponseMessage is not null &&
                    _httpContentConverterFactory.CurrentConverter?.KeepsResponseAlive == false)
                {
                    httpResponseMessage.Dispose();
                }
            }
        }

        // 将 HttpResponseMessage 转换为 HttpRemoteResult<T> 泛型类型 T 的实例
        var result = _httpContentConverterFactory.Read(resultType.GetGenericArguments()[0],
            httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
            cancellationToken);

        // 动态创建 HttpRemoteResult<TResult> 实例并转换为 TResult 实例
        return (TResult?)DynamicCreateHttpRemoteResult(resultType, httpResponseMessage, result, requestDuration);
    }

    /// <inheritdoc />
    public string? SendAsString(HttpRequestBuilder httpRequestBuilder, CancellationToken cancellationToken = default) =>
        SendAs<string>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public string? SendAsString(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAs<string>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public byte[]? SendAsByteArray(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAs<byte[]>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public byte[]? SendAsByteArray(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAs<byte[]>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public Stream? SendAsStream(HttpRequestBuilder httpRequestBuilder, CancellationToken cancellationToken = default) =>
        SendAs<Stream>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public Stream? SendAsStream(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAs<Stream>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> SendAsAsync<TResult>(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAsAsync<TResult>(httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public async Task<TResult?> SendAsAsync<TResult>(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = await SendCoreAsync(httpRequestBuilder, completionOption,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.SendAsync(httpRequestMessage, option, token), null, cancellationToken);

        // 获取结果类型
        var resultType = typeof(TResult);

        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(resultType))
        {
            // 将 HttpResponseMessage 转换为 TResult 实例
            try
            {
                return await _httpContentConverterFactory.ReadAsync<TResult>(httpResponseMessage,
                    httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
                    cancellationToken);
            }
            finally
            {
                // 释放 httpResponseMessage 实例
                if (httpResponseMessage is not null &&
                    _httpContentConverterFactory.CurrentConverter?.KeepsResponseAlive == false)
                {
                    httpResponseMessage.Dispose();
                }
            }
        }

        // 将 HttpResponseMessage 转换为 HttpRemoteResult<T> 泛型类型 T 的实例
        var result = await _httpContentConverterFactory.ReadAsync(resultType.GetGenericArguments()[0],
            httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
            cancellationToken);

        // 动态创建 HttpRemoteResult<TResult> 实例并转换为 TResult 实例
        return (TResult?)DynamicCreateHttpRemoteResult(resultType, httpResponseMessage, result, requestDuration);
    }

    /// <inheritdoc />
    public Task<string?> SendAsStringAsync(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<string>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public Task<string?> SendAsStringAsync(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<string>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> SendAsByteArrayAsync(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<byte[]>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> SendAsByteArrayAsync(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default) =>
        SendAsAsync<byte[]>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> SendAsStreamAsync(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<Stream>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> SendAsStreamAsync(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<Stream>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public object? SendAs(Type resultType, HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAs(resultType, httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public object? SendAs(Type resultType, HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = AsyncUtility.RunSync(() => SendCoreAsync(httpRequestBuilder,
            completionOption, null,
            (httpClient, httpRequestMessage, option, token) => httpClient.Send(httpRequestMessage, option, token),
            cancellationToken));

        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(resultType))
        {
            // 将 HttpResponseMessage 转换为 resultType 类型实例
            try
            {
                return _httpContentConverterFactory.Read(resultType, httpResponseMessage,
                    httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
                    cancellationToken);
            }
            finally
            {
                // 释放 httpResponseMessage 实例
                if (httpResponseMessage is not null &&
                    _httpContentConverterFactory.CurrentConverter?.KeepsResponseAlive == false)
                {
                    httpResponseMessage.Dispose();
                }
            }
        }

        // 将 HttpResponseMessage 转换为 HttpRemoteResult<T> 泛型类型 T 的实例
        var result = _httpContentConverterFactory.Read(resultType.GetGenericArguments()[0],
            httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
            cancellationToken);

        // 动态创建 HttpRemoteResult<TResult> 实例并转换为 resultType 类型实例
        return DynamicCreateHttpRemoteResult(resultType, httpResponseMessage, result, requestDuration);
    }

    /// <inheritdoc />
    public Task<object?> SendAsAsync(Type resultType, HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAsAsync(resultType, httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public async Task<object?> SendAsAsync(Type resultType, HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = await SendCoreAsync(httpRequestBuilder, completionOption,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.SendAsync(httpRequestMessage, option, token), null, cancellationToken);

        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(resultType))
        {
            // 将 HttpResponseMessage 转换为 resultType 类型实例
            try
            {
                return await _httpContentConverterFactory.ReadAsync(resultType, httpResponseMessage,
                    httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
                    cancellationToken);
            }
            finally
            {
                // 释放 httpResponseMessage 实例
                if (httpResponseMessage is not null &&
                    _httpContentConverterFactory.CurrentConverter?.KeepsResponseAlive == false)
                {
                    httpResponseMessage.Dispose();
                }
            }
        }

        // 将 HttpResponseMessage 转换为 HttpRemoteResult<T> 泛型类型 T 的实例
        var result = await _httpContentConverterFactory.ReadAsync(resultType.GetGenericArguments()[0],
            httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
            cancellationToken);

        // 动态创建 HttpRemoteResult<TResult> 实例并转换为 resultType 类型实例
        return DynamicCreateHttpRemoteResult(resultType, httpResponseMessage, result, requestDuration);
    }

    /// <inheritdoc />
    public HttpRemoteResult<TResult>? Send<TResult>(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        Send<TResult>(httpRequestBuilder, HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult>? Send<TResult>(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = AsyncUtility.RunSync(() => SendCoreAsync(httpRequestBuilder,
            completionOption, null,
            (httpClient, httpRequestMessage, option, token) => httpClient.Send(httpRequestMessage, option, token),
            cancellationToken));

        // 空检查
        if (httpResponseMessage is null)
        {
            return null;
        }

        // 将 HttpResponseMessage 转换为 TResult 实例
        var result = _httpContentConverterFactory.Read<TResult>(httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(), cancellationToken);

        // 初始化 HttpRemoteResult 实例
        var httpRemoteResult = new HttpRemoteResult<TResult>(httpResponseMessage)
        {
            Result = result, RequestDuration = requestDuration
        };

        return httpRemoteResult;
    }

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>?> SendAsync<TResult>(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAsync<TResult>(httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpRemoteResult<TResult>?> SendAsync<TResult>(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = await SendCoreAsync(httpRequestBuilder, completionOption,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.SendAsync(httpRequestMessage, option, token), null, cancellationToken);

        // 空检查
        if (httpResponseMessage is null)
        {
            return null;
        }

        // 将 HttpResponseMessage 转换为 TResult 实例
        var result = await _httpContentConverterFactory.ReadAsync<TResult>(httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(), cancellationToken);

        // 初始化 HttpRemoteResult 实例
        var httpRemoteResult = new HttpRemoteResult<TResult>(httpResponseMessage)
        {
            Result = result, RequestDuration = requestDuration
        };

        return httpRemoteResult;
    }

    /// <summary>
    ///     发送 HTTP 远程请求并处理 <see cref="HttpResponseMessage" /> 实例
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="sendAsyncMethod">异步发送 HTTP 请求的委托</param>
    /// <param name="sendMethod">同步发送 HTTP 请求的委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Tuple{T1, T2}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task<(HttpResponseMessage? ResponseMessage, long RequestDuration)> SendCoreAsync(
        HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        Func<HttpClient, HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>>?
            sendAsyncMethod,
        Func<HttpClient, HttpRequestMessage, HttpCompletionOption, CancellationToken, HttpResponseMessage>? sendMethod,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);

        // 空检查
        if (sendAsyncMethod is null && sendMethod is null)
        {
            throw new InvalidOperationException("Both `sendAsyncMethod` and `sendMethod` cannot be null.");
        }

        // 创建带有默认值的 HttpClient 实例
        var httpClientPooling = CreateHttpClientWithDefaults(httpRequestBuilder);
        var httpClient = httpClientPooling.Instance;

        // 统一发送请求的委托
        var sendAsync = sendAsyncMethod ?? ((client, request, option, token) =>
            Task.FromResult(sendMethod!(client, request, option, token)));

        // 初始化 HttpRequestPipelineContext 实例
        var httpRequestPipelineContext = new HttpRequestPipelineContext(httpRequestBuilder, httpClient,
            completionOption, sendAsync, cancellationToken);

        // 获取所有注册的请求管道处理器类型并解析其实例
        var pipelineHandlers = _httpRemoteOptions.PipelineHandlerTypes
            .Select(type => (IHttpRequestPipelineHandler)ServiceProvider.GetRequiredService(type)).Reverse().ToArray();

        // 初始化下一个处理器的委托
        var pipeline = () => Task.FromResult<HttpResponseMessage?>(null);

        // 遍历请求管道处理器并构建调用链
        foreach (var handler in pipelineHandlers)
        {
            var next = pipeline;
            var current = handler;

            // 构建下一个处理器的委托
            pipeline = () => current.HandleAsync(httpRequestPipelineContext, next);
        }

        try
        {
            // 执行管道（发送 HTTP 请求）
            var httpResponseMessage = await pipeline();

            return (httpResponseMessage, httpRequestPipelineContext.RequestDuration);
        }
        catch (Exception e)
        {
            // 检查是否启用异常抑制机制
            if (ShouldSuppressException(httpRequestBuilder.SuppressExceptionTypes, e))
            {
                return (httpRequestPipelineContext.ResponseMessage, httpRequestPipelineContext.RequestDuration);
            }

            throw;
        }
        finally
        {
            // 释放资源集合
            if (!httpRequestBuilder.HttpClientPoolingEnabled)
            {
                httpRequestBuilder.ReleaseResources();
            }
        }
    }

    /// <summary>
    ///     检查是否启用异常抑制机制
    /// </summary>
    /// <param name="suppressExceptionTypes">受抑制的异常类型列表</param>
    /// <param name="exception">
    ///     <see cref="Exception" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool ShouldSuppressException(HashSet<Type>? suppressExceptionTypes, Exception? exception)
    {
        // 空检查
        if (suppressExceptionTypes is null or { Count: 0 } || exception is null)
        {
            return false;
        }

        return suppressExceptionTypes.Any(u => u.IsInstanceOfType(exception));
    }

    /// <summary>
    ///     创建带有默认值的 <see cref="HttpClient" /> 实例
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpClientPooling" />
    /// </returns>
    internal HttpClientPooling CreateHttpClientWithDefaults(HttpRequestBuilder httpRequestBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);

        // 检查是否已经存在 HttpClientPooling 实例
        if (httpRequestBuilder.HttpClientPooling is not null)
        {
            return httpRequestBuilder.HttpClientPooling;
        }

        // 使用锁确保线程安全
        lock (httpRequestBuilder)
        {
            return httpRequestBuilder.HttpClientPooling ?? CreateHttpClientPooling(httpRequestBuilder);
        }
    }

    /// <summary>
    ///     创建 <see cref="HttpClient" /> 实例管理器
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpClientPooling" />
    /// </returns>
    internal HttpClientPooling CreateHttpClientPooling(HttpRequestBuilder httpRequestBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);

        Action<HttpClient>? release = null;
        HttpClient httpClient;

        // 检查是否设置了 HttpClient 实例提供器
        if (httpRequestBuilder.HttpClientProvider is null)
        {
            httpClient = string.IsNullOrWhiteSpace(httpRequestBuilder.HttpClientName)
                ? _httpClientFactory.CreateClient()
                : _httpClientFactory.CreateClient(httpRequestBuilder.HttpClientName);
        }
        else
        {
            // 调用 HttpClient 实例提供器
            var provider = httpRequestBuilder.HttpClientProvider();
            httpClient = provider.Instance;
            release = provider.Release;
        }

        // 空检查
        ArgumentNullException.ThrowIfNull(httpClient);

        // 添加默认的 User-Agent 标头
        AddDefaultUserAgentHeader(httpClient, httpRequestBuilder);

        // 存储 HttpClientPooling 实例并返回
        return httpRequestBuilder.HttpClientPooling = new HttpClientPooling(httpClient, release);
    }

    /// <summary>
    ///     向 <see cref="HttpClient" /> 添加默认的 <c>User-Agent</c> 标头
    /// </summary>
    /// <remarks>解决某些服务器可能需要这个头部信息才能正确响应请求。</remarks>
    /// <param name="httpClient">
    ///     <see cref="HttpClient" />
    /// </param>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    internal static void AddDefaultUserAgentHeader(HttpClient httpClient, HttpRequestBuilder httpRequestBuilder)
    {
        // 空检查
        if (httpClient.DefaultRequestHeaders.UserAgent.Count != 0 ||
            httpRequestBuilder.HeadersToRemove?.Contains(HeaderNames.UserAgent) == true ||
            httpRequestBuilder.Headers?.ContainsKey(HeaderNames.UserAgent) == true)
        {
            return;
        }

        // 设置默认 User-Agent
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, UserAgents.Edge.PC);
    }

    /// <summary>
    ///     动态创建 <see cref="HttpRemoteResult{TResult}" /> 实例
    /// </summary>
    /// <param name="httpRemoteResultType"><see cref="HttpRemoteResult{TResult}" /> 类型</param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="result"><see cref="HttpRemoteResult{TResult}" /> 泛型类型的实例</param>
    /// <param name="requestDuration">请求耗时（毫秒）</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static object? DynamicCreateHttpRemoteResult(Type httpRemoteResultType,
        HttpResponseMessage? httpResponseMessage, object? result, long requestDuration)
    {
        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(httpRemoteResultType))
        {
            throw new ArgumentException(
                $"`{httpRemoteResultType}` type is not assignable from `{typeof(HttpRemoteResult<>)}`.",
                nameof(httpRemoteResultType));
        }

        // 空检查
        if (httpResponseMessage is null)
        {
            return null;
        }

        // 反射创建 HttpRemoteResult<TResult> 实例
        var httpRemoteResult = Activator.CreateInstance(httpRemoteResultType, httpResponseMessage);

        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteResult);

        // 初始化反射搜索成员方式
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // 获取 Result 和 RequestDuration 属性设置器
        var setResultDelegate =
            httpRemoteResultType.CreatePropertySetter(
                httpRemoteResultType.GetProperty(nameof(HttpRemoteResult<object>.Result), bindingFlags)!);
        var setRequestDurationDelegate = httpRemoteResultType.CreatePropertySetter(
            httpRemoteResultType.GetProperty(nameof(HttpRemoteResult<object>.RequestDuration), bindingFlags)!);

        // 设置 Result 和 RequestDuration 属性值
        setResultDelegate(httpRemoteResult, result);
        setRequestDurationDelegate(httpRemoteResult, requestDuration);

        return httpRemoteResult;
    }
}