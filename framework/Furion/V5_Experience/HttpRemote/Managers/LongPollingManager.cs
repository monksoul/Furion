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
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Furion.HttpRemote;

/// <summary>
///     长轮询管理器
/// </summary>
internal sealed class LongPollingManager
{
    /// <inheritdoc cref="HttpLongPollingBuilder" />
    internal readonly HttpLongPollingBuilder _httpLongPollingBuilder;

    /// <inheritdoc cref="IHttpRemoteService" />
    internal readonly IHttpRemoteService _httpRemoteService;

    /// <summary>
    ///     <inheritdoc cref="LongPollingManager" />
    /// </summary>
    /// <param name="httpRemoteService">
    ///     <see cref="IHttpRemoteService" />
    /// </param>
    /// <param name="httpLongPollingBuilder">
    ///     <see cref="HttpLongPollingBuilder" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    internal LongPollingManager(IHttpRemoteService httpRemoteService, HttpLongPollingBuilder httpLongPollingBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteService);
        ArgumentNullException.ThrowIfNull(httpLongPollingBuilder);

        _httpRemoteService = httpRemoteService;
        _httpLongPollingBuilder = httpLongPollingBuilder;

        // 解析 IHttpLongPollingEventHandler 事件处理程序
        LongPollingEventHandler = (httpLongPollingBuilder.LongPollingEventHandlerType is not null
            ? httpRemoteService.ServiceProvider.GetService(httpLongPollingBuilder.LongPollingEventHandlerType)
            : null) as IHttpLongPollingEventHandler;

        // 构建 HttpRequestBuilder 实例
        RequestBuilder = httpLongPollingBuilder.Build(httpRemoteService.ServiceProvider
            .GetRequiredService<IOptions<HttpRemoteOptions>>().Value);
    }

    /// <summary>
    ///     当前重试次数
    /// </summary>
    internal int CurrentRetries { get; private set; }

    /// <summary>
    ///     <inheritdoc cref="HttpRequestBuilder" />
    /// </summary>
    internal HttpRequestBuilder RequestBuilder { get; }

    /// <summary>
    ///     <inheritdoc cref="IHttpLongPollingEventHandler" />
    /// </summary>
    internal IHttpLongPollingEventHandler? LongPollingEventHandler { get; }

    /// <summary>
    ///     开始请求
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    internal void Start(CancellationToken cancellationToken = default)
        => AsyncUtility.RunSync(() => StartAsync(cancellationToken));

    /// <summary>
    ///     开始请求
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // 初始化数据接收传输的通道
        var dataChannel = Channel.CreateUnbounded<HttpResponseMessage>(new UnboundedChannelOptions
        {
            SingleWriter = true, SingleReader = true, AllowSynchronousContinuations = true
        });

        // 初始化接收服务器响应数据任务
        var fetchResponseTask = FetchResponseAsync(dataChannel, cancellationToken);

        try
        {
            // 开始接收（核心）
            await StartCoreAsync(dataChannel.Writer, cancellationToken);
        }
        finally
        {
            // 关闭通道，通知接收任务结束
            dataChannel.Writer.TryComplete();

            // 等待接收服务器响应数据任务完成
            await fetchResponseTask;
        }
    }

    /// <summary>
    ///     开始请求
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{T}" />
    /// </returns>
    internal async IAsyncEnumerable<HttpResponseMessage> StartAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 初始化数据接收传输的通道
        var dataChannel = Channel.CreateUnbounded<HttpResponseMessage>(new UnboundedChannelOptions
        {
            SingleWriter = true, SingleReader = true, AllowSynchronousContinuations = true
        });

        // 开始接收（核心）
        var producerTask = StartCoreAsync(dataChannel.Writer, cancellationToken);

        try
        {
            // 从通道中读取响应
            await foreach (var httpResponseMessage in dataChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return httpResponseMessage;
            }
        }
        finally
        {
            // 关闭通道，通知接收任务结束
            dataChannel.Writer.TryComplete();

            // 等待接收服务器响应数据任务完成
            await producerTask;
        }
    }

    /// <summary>
    ///     开始接收（核心）
    /// </summary>
    /// <param name="writer">
    ///     <see cref="ChannelWriter{T}" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    private async Task StartCoreAsync(ChannelWriter<HttpResponseMessage> writer, CancellationToken cancellationToken)
    {
        try
        {
            // 循环读取数据直到取消请求或读取完毕
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 发送 HTTP 远程请求
                    var httpResponseMessage = await _httpRemoteService.SendAsync(RequestBuilder, cancellationToken);

                    // 空检查
                    if (httpResponseMessage is null)
                    {
                        continue;
                    }

                    // 初始化否应该终止长轮询和是否请求成功变量
                    var shouldTerminate = ShouldTerminatePolling(httpResponseMessage);
                    var isSuccess = httpResponseMessage.IsSuccessStatusCode;

                    // 检查是否应该终止长轮询
                    if (shouldTerminate)
                    {
                        // 检查响应标头中是否存在长轮询结束符
                        if (httpResponseMessage.Headers.TryGetValues(Constants.X_END_OF_STREAM_HEADER, out _))
                        {
                            await HandleEndOfStreamAsync(httpResponseMessage, cancellationToken);
                        }

                        // 释放 httpResponseMessage
                        httpResponseMessage.Dispose();

                        break;
                    }

                    // 发送响应数据对象到通道
                    try
                    {
                        await writer.WriteAsync(httpResponseMessage, cancellationToken);
                    }
                    catch
                    {
                        // 释放 httpResponseMessage
                        httpResponseMessage.Dispose();

                        throw;
                    }

                    // 检查是否请求成功
                    if (isSuccess)
                    {
                        // 重置当前重试次数
                        CurrentRetries = 0;
                    }
                }
                // 任务被取消
                catch (Exception e) when (cancellationToken.IsCancellationRequested || e is OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // 检查是否达到了最大当前重试次数
                    if (CurrentRetries >= _httpLongPollingBuilder.MaxRetries)
                    {
                        throw new InvalidOperationException(
                            $"Failed to establish server connection after `{_httpLongPollingBuilder.MaxRetries}` attempts.",
                            e);
                    }

                    // 递增当前重试次数
                    CurrentRetries++;

                    // 延迟并重试
                    await Task.Delay(_httpLongPollingBuilder.RetryInterval, cancellationToken);
                }
            }
        }
        finally
        {
            // 关闭通道，通知接收任务结束
            writer.TryComplete();
        }
    }

    /// <summary>
    ///     检查是否应该终止长轮询
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal bool ShouldTerminatePolling(HttpResponseMessage httpResponseMessage)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 检查响应标头中是否存在长轮询结束符
        if (httpResponseMessage.Headers.TryGetValues(Constants.X_END_OF_STREAM_HEADER, out _))
        {
            return true;
        }

        // 如果响应状态码不是成功的，则递增当前重试次数
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            CurrentRetries++;
        }

        return CurrentRetries >= _httpLongPollingBuilder.MaxRetries;
    }

    /// <summary>
    ///     接收服务器响应数据任务
    /// </summary>
    /// <param name="dataChannel">数据接收传输的通道</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task FetchResponseAsync(Channel<HttpResponseMessage> dataChannel,
        CancellationToken cancellationToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(dataChannel);

        // 空检查
        if (_httpLongPollingBuilder.OnDataReceived is null && _httpLongPollingBuilder.OnError is null &&
            LongPollingEventHandler is null)
        {
            return;
        }

        try
        {
            // 从数据接收传输的通道中读取所有的数据
            await foreach (var httpResponseMessage in dataChannel.Reader.ReadAllAsync(cancellationToken))
            {
                // 释放 httpResponseMessage
                using (httpResponseMessage)
                {
                    // 如果请求了取消，则抛出 OperationCanceledException
                    cancellationToken.ThrowIfCancellationRequested();

                    // 处理服务器响应数据
                    await HandleResponseAsync(httpResponseMessage, cancellationToken);
                }
            }
        }
        catch (Exception e) when (cancellationToken.IsCancellationRequested || e is OperationCanceledException)
        {
            // 任务被取消
        }
        catch (Exception)
        {
            // ignored
        }
    }

    /// <summary>
    ///     处理服务器响应数据
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task HandleResponseAsync(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 处理服务器返回 200~299 状态码的数据
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            await HandleDataReceivedAsync(httpResponseMessage, cancellationToken);
        }
        // 处理服务器返回非 200~299 状态码的数据
        else
        {
            await HandleErrorAsync(httpResponseMessage, cancellationToken);
        }
    }

    /// <summary>
    ///     处理服务器返回 <c>200~299</c> 状态码的数据
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task HandleDataReceivedAsync(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 空检查
        if (LongPollingEventHandler is not null)
        {
            await DelegateExtensions.TryInvokeAsync(LongPollingEventHandler.OnDataReceivedAsync, httpResponseMessage,
                cancellationToken);
        }

        await _httpLongPollingBuilder.OnDataReceived.TryInvokeAsync(httpResponseMessage, cancellationToken);
    }

    /// <summary>
    ///     处理服务器返回非 <c>200~299</c> 状态码的数据
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task HandleErrorAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 空检查
        if (LongPollingEventHandler is not null)
        {
            await DelegateExtensions.TryInvokeAsync(LongPollingEventHandler.OnErrorAsync, httpResponseMessage,
                cancellationToken);
        }

        await _httpLongPollingBuilder.OnError.TryInvokeAsync(httpResponseMessage, cancellationToken);
    }

    /// <summary>
    ///     处理服务器响应标头包含 <c>X-End-Of-Stream</c> 时触发的操作
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task HandleEndOfStreamAsync(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 空检查
        if (LongPollingEventHandler is not null)
        {
            await DelegateExtensions.TryInvokeAsync(LongPollingEventHandler.OnEndOfStreamAsync, httpResponseMessage,
                cancellationToken);
        }

        await _httpLongPollingBuilder.OnEndOfStream.TryInvokeAsync(httpResponseMessage, cancellationToken);
    }
}