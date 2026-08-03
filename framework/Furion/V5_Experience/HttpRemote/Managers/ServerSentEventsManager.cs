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
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace Furion.HttpRemote;

/// <summary>
///     Server-Sent Events 管理器
/// </summary>
/// <remarks>参考文献：https://developer.mozilla.org/zh-CN/docs/Web/API/Server-sent_events/Using_server-sent_events。</remarks>
internal sealed class ServerSentEventsManager
{
    /// <inheritdoc cref="IHttpRemoteService" />
    internal readonly IHttpRemoteService _httpRemoteService;

    /// <inheritdoc cref="HttpServerSentEventsBuilder" />
    internal readonly HttpServerSentEventsBuilder _httpServerSentEventsBuilder;

    /// <summary>
    ///     <inheritdoc cref="ServerSentEventsManager" />
    /// </summary>
    /// <param name="httpRemoteService">
    ///     <see cref="IHttpRemoteService" />
    /// </param>
    /// <param name="httpServerSentEventsBuilder">
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    internal ServerSentEventsManager(IHttpRemoteService httpRemoteService,
        HttpServerSentEventsBuilder httpServerSentEventsBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteService);
        ArgumentNullException.ThrowIfNull(httpServerSentEventsBuilder);

        _httpRemoteService = httpRemoteService;
        _httpServerSentEventsBuilder = httpServerSentEventsBuilder;
        CurrentRetryInterval = httpServerSentEventsBuilder.DefaultRetryInterval;
        CurrentRetries = 0;

        // 解析 IHttpServerSentEventsEventHandler 事件处理程序
        ServerSentEventsEventHandler = (httpServerSentEventsBuilder.ServerSentEventsEventHandlerType is not null
            ? httpRemoteService.For(httpServerSentEventsBuilder.ServerSentEventsEventHandlerType)
            : null) as IHttpServerSentEventsEventHandler;

        // 构建 HttpRequestBuilder 实例
        RequestBuilder =
            httpServerSentEventsBuilder.Build(httpRemoteService.For<IOptionsMonitor<HttpRemoteOptions>>().CurrentValue);
    }

    /// <summary>
    ///     当前重新连接的时间（毫秒）
    /// </summary>
    internal int CurrentRetryInterval { get; private set; }

    /// <summary>
    ///     当前重试次数
    /// </summary>
    internal int CurrentRetries { get; private set; }

    /// <summary>
    ///     <inheritdoc cref="HttpRequestBuilder" />
    /// </summary>
    internal HttpRequestBuilder RequestBuilder { get; }

    /// <summary>
    ///     <inheritdoc cref="IHttpServerSentEventsEventHandler" />
    /// </summary>
    internal IHttpServerSentEventsEventHandler? ServerSentEventsEventHandler { get; }

    /// <summary>
    ///     开始接收
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    internal void Start(CancellationToken cancellationToken = default)
        => AsyncUtility.RunSync(() => StartAsync(cancellationToken));

    /// <summary>
    ///     开始接收
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // 初始化事件消息传输的通道
        var messageChannel = Channel.CreateUnbounded<ServerSentEventsData>(new UnboundedChannelOptions
        {
            SingleWriter = true, SingleReader = true, AllowSynchronousContinuations = true
        });

        // 初始化接收事件消息任务
        var receiveDataTask = ReceiveDataAsync(messageChannel, cancellationToken);

        try
        {
            // 开始接收（核心）
            await StartCoreAsync(messageChannel.Writer, cancellationToken);
        }
        finally
        {
            // 关闭通道，通知接收任务结束
            messageChannel.Writer.TryComplete();

            // 等待接收事件消息任务完成
            await receiveDataTask;

            // 释放资源集合
            RequestBuilder.ReleaseResources();
        }
    }

    /// <summary>
    ///     开始接收
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{T}" />
    /// </returns>
    internal async IAsyncEnumerable<ServerSentEventsData> StartAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 初始化事件消息传输的通道
        var messageChannel = Channel.CreateUnbounded<ServerSentEventsData>(new UnboundedChannelOptions
        {
            SingleWriter = true, SingleReader = true, AllowSynchronousContinuations = true
        });

        // 开始接收（核心）
        var producerTask = StartCoreAsync(messageChannel.Writer, cancellationToken);

        try
        {
            // 从通道中读取事件
            await foreach (var eventData in messageChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return eventData;
            }
        }
        finally
        {
            // 关闭通道，通知接收任务结束
            messageChannel.Writer.TryComplete();

            // 释放资源集合
            RequestBuilder.ReleaseResources();

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
    private async Task StartCoreAsync(ChannelWriter<ServerSentEventsData> writer, CancellationToken cancellationToken)
    {
        try
        {
            // 重试循环
            while (CurrentRetries <= _httpServerSentEventsBuilder.MaxRetries)
            {
                try
                {
                    // 发送 HTTP 远程请求
                    using var httpResponseMessage = await _httpRemoteService.SendAsync(RequestBuilder,
                        HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    // 空检查
                    if (httpResponseMessage is null)
                    {
                        return;
                    }

                    // 处理与事件源的连接打开
                    HandleOpen();

                    // 获取 HTTP 响应内容中的内容流
                    await using var stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

                    // 初始化 StreamReader 实例
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);

                    // 声明当前正在构建的事件数据
                    ServerSentEventsData? currentEvent = null;

                    // 循环读取数据直到取消请求或读取完毕
                    while (!cancellationToken.IsCancellationRequested &&
                           await streamReader.ReadLineAsync(cancellationToken) is { } line)
                    {
                        // 空检查
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            // 检查是否有待派发的事件
                            if (currentEvent is not null && IsEventComplete(currentEvent))
                            {
                                // 重置当前重试次数
                                CurrentRetries = 0;

                                // 发送事件数据到通道
                                await writer.WriteAsync(currentEvent, cancellationToken);
                            }

                            // 重置构建器，准备接收下一个事件
                            currentEvent = null;

                            continue;
                        }

                        // 尝试解析事件消息行文本
                        TryParseEventLine(line, ref currentEvent);
                    }

                    break;
                }
                // 任务被取消
                catch (Exception e) when (cancellationToken.IsCancellationRequested || e is OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // 处理与事件源的连接错误
                    HandleError(e);

                    // 检查是否达到了最大当前重试次数
                    if (CurrentRetries >= _httpServerSentEventsBuilder.MaxRetries)
                    {
                        throw new InvalidOperationException(
                            $"Failed to establish Server-Sent Events connection after `{_httpServerSentEventsBuilder.MaxRetries}` attempts.",
                            e);
                    }

                    // 递增当前重试次数
                    CurrentRetries++;

                    // 延迟并重试
                    await Task.Delay(CurrentRetryInterval, cancellationToken);
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
    ///     检查是否已经收集了一个完整的事件
    /// </summary>
    /// <param name="serverSentEventsData">
    ///     <see cref="ServerSentEventsData" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsEventComplete(ServerSentEventsData serverSentEventsData)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serverSentEventsData);

        return serverSentEventsData.Data.Length > 0;
    }

    /// <summary>
    ///     尝试解析事件消息行文本
    /// </summary>
    /// <param name="line">消息行文本</param>
    /// <param name="serverSentEventsData">
    ///     <see cref="ServerSentEventsData" />
    /// </param>
    internal void TryParseEventLine(string line, ref ServerSentEventsData? serverSentEventsData)
    {
        // 空检查（忽略空白行和注释行）
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith(':'))
        {
            return;
        }

        // 初始化 ServerSentEventsData 实例
        serverSentEventsData ??= new ServerSentEventsData();

        string key;
        string value;
        // 获取首个冒号位置
        var colonIndex = line.IndexOf(':');

        // 如果没有找到冒号，则认为整行为字段名，字段值为空
        if (colonIndex == -1)
        {
            key = line.Trim();
            value = string.Empty;
        }
        // 提取字段名和字段值
        else
        {
            key = line[..colonIndex].Trim();
            value = line[(colonIndex + 1)..].TrimStart(' ');
        }

        switch (key)
        {
            case "event":
                serverSentEventsData.Event = value;
                break;
            case "data":
                serverSentEventsData.AppendData(value);
                break;
            case "id":
                serverSentEventsData.Id = value;
                break;
            case "retry":
                CurrentRetryInterval = serverSentEventsData.Retry = int.TryParse(value, out var retryInterval)
                    ? retryInterval
                    : _httpServerSentEventsBuilder.DefaultRetryInterval;
                break;
            // 其他的字段名存储在 CustomFields 属性中
            default:
                serverSentEventsData.AddCustomField(key, value);
                break;
        }
    }

    /// <summary>
    ///     接收事件消息任务
    /// </summary>
    /// <param name="messageChannel">事件消息传输的通道</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task ReceiveDataAsync(Channel<ServerSentEventsData> messageChannel,
        CancellationToken cancellationToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(messageChannel);

        // 空检查
        if (_httpServerSentEventsBuilder.OnMessage is null && ServerSentEventsEventHandler is null)
        {
            return;
        }

        try
        {
            // 从事件消息传输的通道中读取所有的事件消息
            await foreach (var serverSentEventsData in messageChannel.Reader.ReadAllAsync(cancellationToken))
            {
                // 如果请求了取消，则抛出 OperationCanceledException
                cancellationToken.ThrowIfCancellationRequested();

                // 处理服务器发送的事件消息
                await HandleMessageReceivedAsync(serverSentEventsData, cancellationToken);
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
    ///     处理与事件源的连接打开
    /// </summary>
    internal void HandleOpen()
    {
        // 空检查
        if (ServerSentEventsEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(ServerSentEventsEventHandler.OnOpen);
        }

        _httpServerSentEventsBuilder.OnOpen.TryInvoke();
    }

    /// <summary>
    ///     处理与事件源的连接错误
    /// </summary>
    /// <param name="e">
    ///     <see cref="Exception" />
    /// </param>
    internal void HandleError(Exception e)
    {
        // 空检查
        if (ServerSentEventsEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(ServerSentEventsEventHandler.OnError, e);
        }

        _httpServerSentEventsBuilder.OnError.TryInvoke(e);
    }

    /// <summary>
    ///     处理服务器发送的事件消息
    /// </summary>
    /// <param name="serverSentEventsData">
    ///     <see cref="ServerSentEventsData" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task HandleMessageReceivedAsync(ServerSentEventsData serverSentEventsData,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serverSentEventsData);

        // 空检查
        if (ServerSentEventsEventHandler is not null)
        {
            await DelegateExtensions.TryInvokeAsync(ServerSentEventsEventHandler.OnMessageAsync, serverSentEventsData,
                cancellationToken);
        }

        await _httpServerSentEventsBuilder.OnMessage.TryInvokeAsync(serverSentEventsData, cancellationToken);
    }
}