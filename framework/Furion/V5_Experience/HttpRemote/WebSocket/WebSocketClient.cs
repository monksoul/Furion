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
        // 释放底层资源
        CleanupResources();

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
    }

    /// <summary>
    ///     释放底层资源
    /// </summary>
    internal void CleanupResources()
    {
        // 释放 ClientWebSocket 实例
        _clientWebSocket?.Dispose();
        _clientWebSocket = null;

        // 停止接收
        _messageCancellationTokenSource?.Cancel();
        _messageCancellationTokenSource?.Dispose();
        _messageCancellationTokenSource = null;

        // 等待接收任务完成
        try
        {
            _receiveMessageTask?.Wait();
        }
        catch (AggregateException)
        {
        }
        finally
        {
            _receiveMessageTask = null;
        }
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

        // 检查连接是否处于正在连接或打开状态，如果是则跳过
        if (State is WebSocketState.Connecting or WebSocketState.Open)
        {
            if (State == WebSocketState.Open)
            {
                // 重置当前重连次数
                CurrentReconnectRetries = 0;
            }

            return;
        }

        // 创建关联的连接超时 Token 标识
        using var connectTimeoutCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 设置连接超时时间控制
        if (Options.Timeout is not null && Options.Timeout.Value != TimeSpan.Zero)
        {
            connectTimeoutCancellationTokenSource.CancelAfter(Options.Timeout.Value);
        }

        // 触发开始连接事件
        var onConnecting = OnConnecting;
        onConnecting.TryInvoke();

        try
        {
            // 连接到服务器
            await _clientWebSocket.ConnectAsync(Options.ServerUri, connectTimeoutCancellationTokenSource.Token);

            // 重置当前重连次数
            CurrentReconnectRetries = 0;

            // 触发连接成功事件
            var onConnected = OnConnected;
            onConnected.TryInvoke();

            // 开始监听服务器消息（非阻塞）
            await ListenAsync(cancellationToken);
        }
        // 任务被取消
        catch (Exception e) when (cancellationToken.IsCancellationRequested || e is OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // 释放底层资源
            CleanupResources();

            // 检查是否达到了最大重连次数
            if (CurrentReconnectRetries < Options.MaxReconnectRetries)
            {
                // 触发开始重新连接事件
                var onReconnecting = OnReconnecting;
                onReconnecting.TryInvoke();

                // 重新连接到服务器
                await ReconnectAsync(cancellationToken);
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    ///     重新连接到服务器
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task ReconnectAsync(CancellationToken cancellationToken = default)
    {
        // 递增当前重连次数
        CurrentReconnectRetries++;

        // 根据配置的重连的间隔时间延迟重新开始连接
        await Task.Delay(Options.ReconnectInterval, cancellationToken);

        // 重新连接到服务器
        await ConnectAsync(cancellationToken);

        // 触发重新连接成功事件
        var onReconnected = OnReconnected;
        onReconnected.TryInvoke();
    }

    /// <summary>
    ///     开始监听服务器消息（非阻塞）
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    internal Task ListenAsync(CancellationToken cancellationToken = default)
    {
        // 检查连接是否处于打开状态
        if (State == WebSocketState.Open)
        {
            // 初始化接收服务器消息任务
            _receiveMessageTask ??= ReceiveAsync(cancellationToken);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     等待接收服务器消息（阻塞）
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        // 检查连接是否处于打开状态
        if (State != WebSocketState.Open)
        {
            return;
        }

        // 空检查
        if (_receiveMessageTask is not null)
        {
            await _receiveMessageTask;
        }
        else
        {
            await ReceiveAsync(cancellationToken);
        }
    }

    /// <summary>
    ///     接收服务器消息
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task ReceiveAsync(CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(_clientWebSocket);

        // 创建关联的取消接收服务器消息 Token 标识
        _messageCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 触发开始接收消息事件
        var onReceivingStarted = OnReceivingStarted;
        onReceivingStarted.TryInvoke();

        // 初始化缓冲区大小
        var buffer = new byte[Options.ReceiveBufferSize];

        try
        {
            // 循环读取服务器消息直到取消请求或连接处于非打开状态
            while (!cancellationToken.IsCancellationRequested && State == WebSocketState.Open)
            {
                try
                {
                    // 获取接收到的数据
                    var receiveResult =
                        await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    // 如果接收到关闭帧，则退出循环
                    if (receiveResult.MessageType == WebSocketMessageType.Close || receiveResult.CloseStatus.HasValue)
                    {
                        break;
                    }

                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (receiveResult.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            // 解码接收到的文本消息
                            var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

                            // 初始化 WebSocketTextReceiveResult 实例
                            var textReceiveResult = new WebSocketTextReceiveResult(receiveResult.Count,
                                receiveResult.EndOfMessage, receiveResult.CloseStatus,
                                receiveResult.CloseStatusDescription) { Message = message };

                            // 触发接收文本消息事件
                            var onTextReceived = OnTextReceived;
                            onTextReceived.TryInvoke(textReceiveResult);
                            break;
                        case WebSocketMessageType.Binary:
                            // 将接收到的数据从原始缓冲区复制到新创建的字节数组中
                            var bytes = new byte[receiveResult.Count];
                            Buffer.BlockCopy(buffer, 0, bytes, 0, receiveResult.Count);

                            // 初始化 WebSocketBinaryReceiveResult 实例
                            var binaryReceiveResult = new WebSocketBinaryReceiveResult(receiveResult.Count,
                                receiveResult.EndOfMessage, receiveResult.CloseStatus,
                                receiveResult.CloseStatusDescription) { Message = bytes };

                            // 触发接收二进制消息事件
                            var onBinaryReceived = OnBinaryReceived;
                            onBinaryReceived.TryInvoke(binaryReceiveResult);
                            break;
                    }

                    // 如果这是消息的最后一部分，则清空缓冲区
                    if (receiveResult.EndOfMessage)
                    {
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                }
                // 任务被取消
                catch (Exception e) when (cancellationToken.IsCancellationRequested || e is OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        finally
        {
            // 触发停止接收消息事件
            var onReceivingStopped = OnReceivingStopped;
            onReceivingStopped.TryInvoke();
        }
    }

    /// <summary>
    ///     向服务器发送消息
    /// </summary>
    /// <param name="message">字符串消息</param>
    /// <param name="endOfMessage">是否作为消息的最后一部分，默认值为 <c>true</c>。</param>
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
    /// <param name="endOfMessage">是否作为消息的最后一部分，默认值为 <c>true</c>。</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    public async Task SendAsync(string message, WebSocketMessageType webSocketMessageType, bool endOfMessage = true,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(message);

        // 检查连接是否处于打开状态
        if (State != WebSocketState.Open)
        {
            return;
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
    /// <param name="endOfMessage">是否作为消息的最后一部分，默认值为 <c>true</c>。</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    public async Task SendAsync(byte[] byteArray, bool endOfMessage = true,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(byteArray);

        // 检查连接是否处于打开状态
        if (State != WebSocketState.Open)
        {
            return;
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
    /// <param name="closeDescription">关闭描述。默认值为：<c>Closing</c>。</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
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
        var onClosing = OnClosing;
        onClosing.TryInvoke();

        try
        {
            // 发送关闭帧并关闭连接
            await _clientWebSocket.CloseAsync(closeStatus, closeDescription, cancellationToken);
        }
        finally
        {
            // 触发关闭连接完成事件
            var onClosed = OnClosed;
            onClosed.TryInvoke();

            // 释放所有资源
            Dispose();

            // 重置当前重连次数
            CurrentReconnectRetries = 0;
        }
    }
}