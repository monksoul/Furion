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
using System.Net.WebSockets;
using System.Text;

namespace Furion.HttpRemote;

/// <summary>
///     WebSocket 客户端
/// </summary>
public sealed partial class WebSocketClient : IDisposable
{
    /// <inheritdoc cref="ClientWebSocket" />
    internal ClientWebSocket? _clientWebSocket;

    /// <summary>
    ///     取消接收服务器消息标记
    /// </summary>
    internal CancellationTokenSource? _messageCancellationTokenSource;

    /// <summary>
    ///     接收服务器消息任务
    /// </summary>
    internal Task? _receiveMessageTask;

    /// <summary>
    ///     <inheritdoc cref="WebSocketClient" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    public WebSocketClient(string serverUri)
        : this(new WebSocketClientOptions(serverUri))
    {
    }

    /// <summary>
    ///     <inheritdoc cref="WebSocketClient" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    public WebSocketClient(Uri serverUri)
        : this(new WebSocketClientOptions(serverUri))
    {
    }

    /// <summary>
    ///     <inheritdoc cref="WebSocketClient" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    /// <param name="configure">用于配置 <see cref="ClientWebSocketOptions" /> 的操作</param>
    public WebSocketClient(string serverUri, Action<ClientWebSocketOptions> configure)
        : this(new WebSocketClientOptions(serverUri, configure))
    {
    }

    /// <summary>
    ///     <inheritdoc cref="WebSocketClient" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    /// <param name="configure">用于配置 <see cref="ClientWebSocketOptions" /> 的操作</param>
    public WebSocketClient(Uri serverUri, Action<ClientWebSocketOptions> configure)
        : this(new WebSocketClientOptions(serverUri, configure))
    {
    }

    /// <summary>
    ///     <inheritdoc cref="WebSocketClient" />
    /// </summary>
    /// <param name="options">
    ///     <see cref="WebSocketClientOptions" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public WebSocketClient(WebSocketClientOptions options)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(options);

        Options = options;
    }

    /// <inheritdoc cref="WebSocketState" />
    public WebSocketState? State => _clientWebSocket?.State;

    /// <summary>
    ///     <see cref="WebSocketClientOptions" />
    /// </summary>
    internal WebSocketClientOptions Options { get; }

    /// <summary>
    ///     当前重连次数
    /// </summary>
    internal int CurrentReconnectRetries { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        // 取消接收循环
        _messageCancellationTokenSource?.Cancel();
        _messageCancellationTokenSource?.Dispose();
        _messageCancellationTokenSource = null;

        // 释放 ClientWebSocket 实例
        _clientWebSocket?.Dispose();
        _clientWebSocket = null;

        // 清除所有事件订阅
        Connecting = null;
        Connected = null;
        Reconnecting = null;
        Reconnected = null;
        Closing = null;
        Closed = null;
        ReceivingStarted = null;
        ReceivingStopped = null;
        TextReceived = null;
        BinaryReceived = null;

        // 清除接收任务
        _receiveMessageTask = null;
    }

    /// <summary>
    ///     连接到服务器
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        // 初始化 ClientWebSocket 实例
        _clientWebSocket ??= new ClientWebSocket();

        // 调用用于配置 ClientWebSocketOptions 的操作
        Options.Configure?.Invoke(_clientWebSocket.Options);

        // 检查连接是否已经打开，如果是则直接返回
        if (State == WebSocketState.Open)
        {
            CurrentReconnectRetries = 0;
            return;
        }

        // 重试循环
        while (true)
        {
            // 创建关联的连接超时 Token 标识
            using var connectTimeoutCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // 设置连接超时时间控制
            if (Options.Timeout is not null && Options.Timeout.Value != TimeSpan.Zero)
            {
                connectTimeoutCancellationTokenSource.CancelAfter(Options.Timeout.Value);
            }

            // 触发开始连接事件
            await OnConnectingAsync();

            try
            {
                // 连接到服务器
                await _clientWebSocket.ConnectAsync(Options.ServerUri, connectTimeoutCancellationTokenSource.Token);

                // 判断是否为重连成功
                var wasReconnecting = CurrentReconnectRetries > 0;

                // 重置当前重连次数
                CurrentReconnectRetries = 0;

                // 触发连接成功事件（首次连接）
                await OnConnectedAsync();

                // 如果是重连成功，额外触发重新连接成功事件
                if (wasReconnecting)
                {
                    await OnReconnectedAsync();
                }

                // 启动后台消息监听（非阻塞）
                await ListenAsync(cancellationToken);

                return;
            }
            // 用户主动取消
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
                // 释放底层资源
                CleanupResources();

                throw;
            }
            catch (Exception)
            {
                // 释放底层资源
                CleanupResources();

                // 检查是否达到了最大重连次数
                if (CurrentReconnectRetries >= Options.MaxReconnectRetries)
                {
                    throw;
                }

                // 递增重连次数
                CurrentReconnectRetries++;

                // 触发开始重新连接事件
                await OnReconnectingAsync();

                // 等待重连间隔
                await Task.Delay(Options.ReconnectInterval, cancellationToken);

                // 重新创建 ClientWebSocket 实例
                _clientWebSocket = new ClientWebSocket();

                // 调用用于配置 ClientWebSocketOptions 的操作
                Options.Configure?.Invoke(_clientWebSocket.Options);
            }
        }
    }

    /// <summary>
    ///     等待接收服务器消息（阻塞）
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        // 检查连接是否处于打开状态
        if (State != WebSocketState.Open)
        {
            return;
        }

        // 初始化接收服务器消息任务
        _receiveMessageTask ??= ReceiveAsync(cancellationToken);

        // 检查是否传入了外部取消令牌
        if (cancellationToken.CanBeCanceled)
        {
            // 初始化 TaskCompletionSource 实例
            var taskCompletionSource =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // 注册当令牌被取消时，将 taskCompletionSource 设置为已取消状态
            await using (cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken)))
            {
                // 同时等待接收任务和取消任务
                var completedTask =
                    await Task.WhenAny(_receiveMessageTask, taskCompletionSource.Task).ConfigureAwait(false);

                // 如果取消任务先完成（即用户取消了令牌）
                if (completedTask == taskCompletionSource.Task)
                {
                    // 取消内部令牌，通知接收循环退出
                    if (_messageCancellationTokenSource is not null)
                    {
                        await _messageCancellationTokenSource.CancelAsync();
                    }

                    // 等待接收任务结束
                    await _receiveMessageTask.ConfigureAwait(false);

                    // 抛出 OperationCanceledException
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }
        else
        {
            // 等待接收任务结束
            await _receiveMessageTask.ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     向服务器发送消息
    /// </summary>
    /// <param name="message">字符串消息</param>
    /// <param name="endOfMessage">是否作为消息的最后一部分，默认值为：<c>true</c></param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    public Task SendAsync(string message, bool endOfMessage = true, CancellationToken cancellationToken = default) =>
        SendAsync(message, WebSocketMessageType.Text, endOfMessage, cancellationToken);

    /// <summary>
    ///     向服务器发送消息
    /// </summary>
    /// <param name="message">字符串消息</param>
    /// <param name="webSocketMessageType">
    ///     <see cref="WebSocketMessageType" />
    /// </param>
    /// <param name="endOfMessage">是否作为消息的最后一部分，默认值为：<c>true</c></param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task SendAsync(string message, WebSocketMessageType webSocketMessageType, bool endOfMessage = true,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(message);

        // 检查连接是否处于打开状态
        if (State != WebSocketState.Open)
        {
            throw new InvalidOperationException(
                "Cannot send message because the WebSocket connection is not open.");
        }

        // 空检查
        ArgumentNullException.ThrowIfNull(_clientWebSocket);

        // 将字符串编码为字节数组
        var buffer = Encoding.UTF8.GetBytes(message);

        // 初始化 ArraySegment 实例
        var arraySegment = new ArraySegment<byte>(buffer);

        // 向服务器发送消息
        await _clientWebSocket.SendAsync(arraySegment, webSocketMessageType, endOfMessage, cancellationToken);
    }

    /// <summary>
    ///     向服务器发送消息
    /// </summary>
    /// <param name="byteArray">二进制消息</param>
    /// <param name="endOfMessage">是否作为消息的最后一部分，默认值为：<c>true</c></param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task SendAsync(byte[] byteArray, bool endOfMessage = true,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(byteArray);

        // 检查连接是否处于打开状态
        if (State != WebSocketState.Open)
        {
            throw new InvalidOperationException(
                "Cannot send binary message because the WebSocket connection is not open.");
        }

        // 空检查
        ArgumentNullException.ThrowIfNull(_clientWebSocket);

        // 初始化 ArraySegment 实例
        var arraySegment = new ArraySegment<byte>(byteArray);

        // 向服务器发送二进制消息
        await _clientWebSocket.SendAsync(arraySegment, WebSocketMessageType.Binary, endOfMessage, cancellationToken);
    }

    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    public Task CloseAsync(CancellationToken cancellationToken = default) =>
        CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);

    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="closeStatus">
    ///     <see cref="WebSocketCloseStatus" />
    /// </param>
    /// <param name="closeDescription">关闭描述。默认值为：<c>Closing</c></param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task CloseAsync(WebSocketCloseStatus closeStatus, string closeDescription,
        CancellationToken cancellationToken = default)
    {
        // 检查连接是否处于关闭状态
        if (State is null or WebSocketState.CloseSent or WebSocketState.Closed)
        {
            return;
        }

        // 空检查
        ArgumentNullException.ThrowIfNull(_clientWebSocket);

        // 触发开始关闭连接事件
        await OnClosingAsync();

        try
        {
            // 发送关闭帧并关闭连接
            await _clientWebSocket.CloseAsync(closeStatus, closeDescription, cancellationToken);
        }
        finally
        {
            // 取消接收循环，并等待其结束
            if (_messageCancellationTokenSource is not null)
            {
                await _messageCancellationTokenSource.CancelAsync();
            }

            // 空检查
            if (_receiveMessageTask is not null)
            {
                try
                {
                    await _receiveMessageTask;
                }
                catch
                {
                    // ignored
                }
            }

            // 释放底层资源
            _clientWebSocket?.Dispose();
            _clientWebSocket = null;
            _messageCancellationTokenSource?.Dispose();
            _messageCancellationTokenSource = null;
            _receiveMessageTask = null;

            // 触发关闭连接完成事件
            await OnClosedAsync();

            // 重置当前重连次数
            CurrentReconnectRetries = 0;
        }
    }

    /// <summary>
    ///     释放底层资源
    /// </summary>
    internal void CleanupResources()
    {
        // 取消正在进行的接收
        _messageCancellationTokenSource?.Cancel();
        _messageCancellationTokenSource?.Dispose();
        _messageCancellationTokenSource = null;

        // 丢弃旧的接收任务
        _receiveMessageTask = null;

        // 释放 ClientWebSocket 实例
        _clientWebSocket?.Dispose();
        _clientWebSocket = null;
    }

    /// <summary>
    ///     启动后台消息监听（非阻塞）
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal Task ListenAsync(CancellationToken cancellationToken = default)
    {
        // 检查连接是否处于打开状态
        if (State == WebSocketState.Open)
        {
            // 创建新的接收任务
            _receiveMessageTask = ReceiveAsync(cancellationToken);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     接收服务器消息
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task ReceiveAsync(CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(_clientWebSocket);

        // 创建关联的取消接收服务器消息 Token 标识
        _messageCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 触发开始接收消息事件
        await OnReceivingStartedAsync();

        // 初始化缓冲区大小
        var buffer = new byte[Options.ReceiveBufferSize];

        // 用于拼装分片消息的流和当前消息类型
        MemoryStream? messageStream = null;
        var currentMessageType = WebSocketMessageType.Text;

        try
        {
            // 获取内部组合 Token，确保 CloseAsync 可以中断等待
            var receiveToken = _messageCancellationTokenSource.Token;

            // 循环读取服务器消息直到取消请求或连接处于非打开状态
            while (!receiveToken.IsCancellationRequested && State == WebSocketState.Open)
            {
                try
                {
                    // 获取接收到的数据帧
                    var receiveResult =
                        await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), receiveToken);

                    // 如果接收到关闭帧，则发送响应关闭帧并退出循环
                    if (receiveResult.MessageType == WebSocketMessageType.Close || receiveResult.CloseStatus.HasValue)
                    {
                        try
                        {
                            // 响应服务器关闭帧，完成关闭握手
                            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing",
                                CancellationToken.None);
                        }
                        catch
                        {
                            // ignored
                        }

                        break;
                    }

                    // 如果是第一帧，初始化消息流并记录消息类型
                    if (messageStream is null)
                    {
                        currentMessageType = receiveResult.MessageType;
                        messageStream = new MemoryStream();
                    }

                    // 将当前帧数据写入消息流
                    messageStream.Write(buffer, 0, receiveResult.Count);

                    // 检查是否是消息的最后一部分
                    if (!receiveResult.EndOfMessage)
                    {
                        continue;
                    }

                    switch (currentMessageType)
                    {
                        case WebSocketMessageType.Text:
                            // 解码完整文本消息
                            var text = Encoding.UTF8.GetString(messageStream.ToArray());
                            var textResult =
                                new WebSocketTextReceiveResult((int)messageStream.Length, true, null, null)
                                {
                                    Message = text
                                };

                            // 触发接收文本消息事件
                            await OnTextReceivedAsync(textResult);
                            break;
                        case WebSocketMessageType.Binary:
                            // 获取完整二进制数据
                            var bytes = messageStream.ToArray();
                            var binaryResult =
                                new WebSocketBinaryReceiveResult((int)messageStream.Length, true, null, null)
                                {
                                    Message = bytes
                                };

                            // 触发接收二进制消息事件
                            await OnBinaryReceivedAsync(binaryResult);
                            break;
                        case WebSocketMessageType.Close:
                        default:
                            throw new InvalidOperationException(
                                $"Unexpected WebSocket message type: {currentMessageType}.");
                    }

                    // 释放流并重置，准备接收下一条消息
                    await messageStream.DisposeAsync();
                    messageStream = null;
                }
                // 任务取消
                catch (OperationCanceledException) when (receiveToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
        finally
        {
            // 清理未完成的消息流
            if (messageStream is not null)
            {
                await messageStream.DisposeAsync();
            }

            // 触发停止接收消息事件
            await OnReceivingStoppedAsync();
        }
    }
}