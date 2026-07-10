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
using Furion.HttpRemote.Extensions;
using Furion.Utilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

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

        // 解析 IHttpRequestEventHandler 事件处理程序
        var requestEventHandler =
            (httpRequestBuilder.RequestEventHandlerType is not null
                ? ServiceProvider.GetService(httpRequestBuilder.RequestEventHandlerType)
                : null) as IHttpRequestEventHandler;

        // 创建带有默认值的 HttpClient 实例
        var httpClientPooling = CreateHttpClientWithDefaults(httpRequestBuilder);
        var httpClient = httpClientPooling.Instance;

        // 构建 HttpRequestMessage 实例
        var httpRequestMessage =
            httpRequestBuilder.Build(_httpRemoteOptions, _httpContentProcessorFactory,
                httpClient.BaseAddress ?? _httpRemoteOptions.FallbackBaseAddress);

        // 处理发送 HTTP 请求之前
        HandlePreSendRequest(httpRequestBuilder, requestEventHandler, httpRequestMessage);

        // 初始化 HttpRemoteAnalyzer 实例
        HttpRemoteAnalyzer? httpRemoteAnalyzer = null;

        // 检查是否启用请求分析工具
        if (httpRequestBuilder.ProfilerEnabled)
        {
            // 标记已打印，解决重复打印问题
            httpRequestMessage.Options.TryAdd(Constants.PROFILER_PRINTED_KEY, "TRUE");

            // 初始化 HttpRemoteAnalyzer 实例
            httpRemoteAnalyzer = httpRequestBuilder.ProfilerPredicate is not null ? new HttpRemoteAnalyzer() : null;

            await ProfilerDelegatingHandler.LogRequestAsync(_logger, _httpRemoteOptions, httpRequestMessage,
                httpRemoteAnalyzer, httpClient, cancellationToken);
        }

        // 创建关联的超时 Token 标识
        using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var timeoutCancellationToken = timeoutCancellationTokenSource.Token;

        // 定义标志位，用于判断是否引发了超时操作
        var isTimeoutTriggered = false;

        // 设置单次请求超时时间控制（若超时时间为 TimeSpan.Zero 将立即取消请求）
        if (httpRequestBuilder.Timeout is not null)
        {
            // 确保 HttpRequestBuilder 的 Timeout 属性值小于 HttpClient 的 Timeout 属性值（默认 100秒）
            if (httpRequestBuilder.Timeout.Value > httpClient.Timeout)
            {
                throw new InvalidOperationException(
                    "HttpRequestBuilder's Timeout cannot be greater than HttpClient's Timeout, which defaults to 100 seconds.");
            }

            // 调用超时发生时要执行的操作
            if (httpRequestBuilder.TimeoutAction is not null)
            {
                timeoutCancellationToken.Register(httpRequestBuilder.TimeoutAction.TryInvoke);
            }

            // 注册回调，用于标记是否是超时触发的取消
            timeoutCancellationToken.Register(() => isTimeoutTriggered = true);

            // 延迟指定时间后取消任务
            timeoutCancellationTokenSource.CancelAfter(httpRequestBuilder.Timeout.Value);
        }

        HttpResponseMessage? httpResponseMessage = null;
        long requestDuration = 0;

        // 初始化 Stopwatch 实例并开启计时操作
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 调用发送 HTTP 请求委托
            httpResponseMessage = sendAsyncMethod is not null
                ? await sendAsyncMethod(httpClient, httpRequestMessage, completionOption, timeoutCancellationToken)
                : sendMethod!(httpClient, httpRequestMessage, completionOption, timeoutCancellationToken);

            // 修复无效的响应内容字符编码
            httpResponseMessage.FixInvalidCharset();

            // 初始化当前重定向次数和原始请求方法
            var redirections = 0;
            var originalHttpMethod = httpRequestBuilder.HttpMethod!;

            // 处理请求重定向
            while (Helpers.DetermineRedirectMethod(httpResponseMessage.StatusCode, originalHttpMethod,
                       out var redirectMethod) && _httpRemoteOptions.AllowAutoRedirect &&
                   redirections < _httpRemoteOptions.MaximumAutomaticRedirections)
            {
                // 获取重定向地址
                var redirectUrl = httpResponseMessage.Headers.Location;

                // 空检查
                if (redirectUrl is null)
                {
                    break;
                }

                // 构建新的 HttpRequestMessage 实例
                var redirectHttpRequestMessage = httpRequestBuilder
                    .ConfigureForRedirect(
                        redirectUrl.IsAbsoluteUri
                            ? redirectUrl
                            : new Uri(Helpers.ParseBaseAddress(httpRequestMessage.RequestUri), redirectUrl),
                        redirectMethod).Build(_httpRemoteOptions, _httpContentProcessorFactory,
                        httpClient.BaseAddress ?? _httpRemoteOptions.FallbackBaseAddress);

                // 释放前一个 HttpResponseMessage 实例
                httpResponseMessage.Dispose();

                // 重新调用发送 HTTP 请求委托
                httpResponseMessage = sendAsyncMethod is not null
                    ? await sendAsyncMethod(httpClient, redirectHttpRequestMessage, completionOption,
                        timeoutCancellationToken)
                    : sendMethod!(httpClient, redirectHttpRequestMessage, completionOption, timeoutCancellationToken);

                // 修复无效的响应内容字符编码
                httpResponseMessage.FixInvalidCharset();

                // 递增重定向次数
                redirections++;
            }

            // 获取请求耗时
            requestDuration = stopwatch.ElapsedMilliseconds;

            // 调用状态码处理程序
            if (sendAsyncMethod is not null)
            {
                await InvokeStatusCodeHandlersAsync(httpRequestBuilder, httpResponseMessage, timeoutCancellationToken);
            }
            else
            {
                // ReSharper disable once MethodHasAsyncOverload
                InvokeStatusCodeHandlers(httpRequestBuilder, httpResponseMessage, timeoutCancellationToken);
            }

            // 检查 HTTP 响应内容长度是否在设定的最大缓冲区大小限制内
            CheckContentLengthWithinLimit(httpRequestBuilder, httpResponseMessage);

            // 如果 HTTP 响应的 IsSuccessStatusCode 属性是 false，则引发异常
            if (httpRequestBuilder.EnsureSuccessStatusCodeEnabled)
            {
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            // 执行断言委托操作
            await ExecuteAssertionsAsync(httpRequestBuilder, httpResponseMessage, requestDuration, ServiceProvider);

            return (httpResponseMessage, requestDuration);
        }
        catch (Exception e)
        {
            // 输出请求异常日志
            _logger.LogError(e, "An error occurred while sending HTTP request to {Url} using {Method}.",
                httpRequestMessage.RequestUri?.ToString() ?? "unknown", httpRequestMessage.Method);

            // 处理发送 HTTP 请求发生异常
            HandleRequestFailed(httpRequestBuilder, requestEventHandler, e, httpResponseMessage);

            // 检查是否启用异常抑制机制
            if (ShouldSuppressException(httpRequestBuilder.SuppressExceptionTypes, e))
            {
                return (httpResponseMessage, requestDuration);
            }

            // 检查是否是超时导致的取消，如果是则抛出 TaskCanceledException(TimeoutException) 超时异常
            if (e is OperationCanceledException oce && oce.CancellationToken == timeoutCancellationToken &&
                isTimeoutTriggered)
            {
                throw new TaskCanceledException(
                    $"The request was canceled due to the configured HttpRequestBuilder.Timeout of {httpRequestBuilder.Timeout?.TotalSeconds:0.###} seconds elapsing.",
                    new TimeoutException("The operation was canceled.", oce));
            }

            throw;
        }
        finally
        {
            // 停止计时
            stopwatch.Stop();

            // 处理收到 HTTP 响应之后
            HandlePostReceiveResponse(httpRequestBuilder, requestEventHandler, httpResponseMessage);

            // 释放资源集合
            if (!httpRequestBuilder.HttpClientPoolingEnabled)
            {
                httpRequestBuilder.ReleaseResources();
            }

            // 检查是否启用请求分析工具
            if (httpResponseMessage is not null && httpRequestBuilder.ProfilerEnabled)
            {
                await ProfilerDelegatingHandler.LogResponseAsync(_logger, _httpRemoteOptions, httpResponseMessage,
                    requestDuration, httpRemoteAnalyzer, cancellationToken);

                // 调用请求分析工具委托
                httpRequestBuilder.ProfilerPredicate?.TryInvoke(httpRemoteAnalyzer!);
            }
        }
    }

    /// <summary>
    ///     处理发送 HTTP 请求之前
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="requestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </param>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal static void HandlePreSendRequest(HttpRequestBuilder httpRequestBuilder,
        IHttpRequestEventHandler? requestEventHandler, HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (requestEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(requestEventHandler.OnPreSendRequest, httpRequestMessage);
        }

        httpRequestBuilder.OnPreSendRequest.TryInvoke(httpRequestMessage);
    }

    /// <summary>
    ///     处理收到 HTTP 响应之后
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="requestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    internal static void HandlePostReceiveResponse(HttpRequestBuilder httpRequestBuilder,
        IHttpRequestEventHandler? requestEventHandler, HttpResponseMessage? httpResponseMessage)
    {
        // 空检查
        if (httpResponseMessage is null)
        {
            return;
        }

        // 空检查
        if (requestEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(requestEventHandler.OnPostReceiveResponse, httpResponseMessage);
        }

        httpRequestBuilder.OnPostReceiveResponse.TryInvoke(httpResponseMessage);
    }

    /// <summary>
    ///     处理发送 HTTP 请求发生异常
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="requestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </param>
    /// <param name="e">
    ///     <see cref="Exception" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    internal static void HandleRequestFailed(HttpRequestBuilder httpRequestBuilder,
        IHttpRequestEventHandler? requestEventHandler, Exception e, HttpResponseMessage? httpResponseMessage)
    {
        // 空检查
        if (requestEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(requestEventHandler.OnRequestFailed, e, httpResponseMessage);
        }

        httpRequestBuilder.OnRequestFailed.TryInvoke(e, httpResponseMessage);
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
    ///     检查 HTTP 响应内容长度是否在设定的最大缓冲区大小限制内
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <exception cref="HttpRequestException"></exception>
    internal static void CheckContentLengthWithinLimit(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage)
    {
        // 空检查
        if (httpRequestBuilder.MaxResponseContentBufferSize is null)
        {
            return;
        }

        // 检查响应内容长度
        if (httpResponseMessage.Content.Headers.ContentLength is { } contentLength &&
            contentLength > httpRequestBuilder.MaxResponseContentBufferSize)
        {
            throw new HttpRequestException(
                $"Cannot write more bytes to the buffer than the configured maximum buffer size: `{httpRequestBuilder.MaxResponseContentBufferSize}`.");
        }
    }

    /// <summary>
    ///     调用状态码处理程序
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static void InvokeStatusCodeHandlers(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default) =>
        AsyncUtility.RunSync(() =>
            InvokeStatusCodeHandlersAsync(httpRequestBuilder, httpResponseMessage, cancellationToken));

    /// <summary>
    ///     调用状态码处理程序
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task InvokeStatusCodeHandlersAsync(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 空检查
        if (httpRequestBuilder.StatusCodeHandlers is null || httpRequestBuilder.StatusCodeHandlers.Count == 0)
        {
            return;
        }

        // 获取响应状态码
        var statusCode = (int)httpResponseMessage.StatusCode;

        // 查找响应状态码所有处理程序
        var statusCodeHandlers = httpRequestBuilder.StatusCodeHandlers
            .Where(u => u.Key.Any(code => IsMatchedStatusCode(code, statusCode)))
            .Select(u => u.Value).ToList();

        // 空检查
        if (statusCodeHandlers.Count == 0)
        {
            return;
        }

        // 并行执行所有的处理程序，并等待所有任务完成
        await Task.WhenAll(statusCodeHandlers.Select(handler =>
            handler.TryInvokeAsync(httpResponseMessage, cancellationToken)));
    }

    /// <summary>
    ///     检查状态码代码是否匹配响应状态码
    /// </summary>
    /// <param name="code">状态码代码</param>
    /// <param name="statusCode">响应状态码</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsMatchedStatusCode(object code, int statusCode)
    {
        switch (code)
        {
            // 处理正整数类型
            case int intStatusCode when intStatusCode == statusCode:
                return true;
            // 处理 HttpStatusCode 枚举类型
            case HttpStatusCode httpStatusCode when (int)httpStatusCode == statusCode:
                return true;
            // 处理特殊字符串
            case "*" or '*':
                return true;
            // 处理字符串类型
            case string stringStatusCode when !stringStatusCode.Contains('+') &&
                                              int.TryParse(stringStatusCode, out var intStatusCodeResult) &&
                                              intStatusCodeResult == statusCode:
                return true;
            // 处理字符串区间类型，如 200-500 或 200~500
            case string stringStatusCode when StatusCodeRangeRegex().IsMatch(stringStatusCode):
                // 根据 - 或 ~ 符号切割
                var parts = stringStatusCode.Split(['-', '~'], StringSplitOptions.RemoveEmptyEntries);

                // 比较状态码区间
                if (parts.Length == 2 && int.TryParse(parts[0], out var start) && int.TryParse(parts[1], out var end))
                {
                    return statusCode >= start && statusCode <= end;
                }

                break;
            // 处理包含比较符号的类型：如：>=200, <=300, <100, =100, >100
            case string compareStatusCode when StatusCodeCompareRegex().IsMatch(compareStatusCode):
                // 提取正则表达式内容并获取符号和数字部分
                var match = StatusCodeCompareRegex().Match(compareStatusCode);
                var symbolPart = match.Groups[1].Value;
                var numberPart = match.Groups[2].Value;

                // 获取状态码
                if (!int.TryParse(numberPart, out var number))
                {
                    return false;
                }

                return symbolPart switch
                {
                    ">=" => statusCode >= number,
                    "<=" => statusCode <= number,
                    ">" => statusCode > number,
                    "<" => statusCode < number,
                    "=" => statusCode == number,
                    _ => false
                };
        }

        return false;
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
            httpRemoteResultType.CreatePropertySetter(httpRemoteResultType.GetProperty(
                nameof(HttpRemoteResult<object>.Result),
                bindingFlags)!);
        var setRequestDurationDelegate =
            httpRemoteResultType.CreatePropertySetter(httpRemoteResultType.GetProperty(
                nameof(HttpRemoteResult<object>.RequestDuration),
                bindingFlags)!);

        // 设置 Result 和 RequestDuration 属性值
        setResultDelegate(httpRemoteResult, result);
        setRequestDurationDelegate(httpRemoteResult, requestDuration);

        return httpRemoteResult;
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
    ///     执行断言委托操作
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="requestDuration">请求耗时（毫秒）</param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    internal static async Task ExecuteAssertionsAsync(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage, long requestDuration, IServiceProvider serviceProvider)
    {
        // 检查断言是否启用且已配置委托集合
        if (httpRequestBuilder is { AssertionsEnabled: true, Assertions.Count: > 0 })
        {
            // 初始化 HttpAssertionContext 实例
            var httpAssertionContext = new HttpAssertionContext(httpResponseMessage, requestDuration, serviceProvider);

            // 逐个调用断言委托
            foreach (var httpAssertion in httpRequestBuilder.Assertions)
            {
                await httpAssertion(httpAssertionContext);
            }
        }
    }

    /// <summary>
    ///     状态码区间正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(@"^\d+[-~]\d+$")]
    private static partial Regex StatusCodeRangeRegex();

    /// <summary>
    ///     状态码比较正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(@"^([<>]=?|=|>|<)(\d+)$")]
    private static partial Regex StatusCodeCompareRegex();
}