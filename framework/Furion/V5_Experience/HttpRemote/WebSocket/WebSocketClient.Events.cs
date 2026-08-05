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

namespace Furion.HttpRemote;

/// <summary>
///     WebSocket 客户端
/// </summary>
public sealed partial class WebSocketClient
{
    /// <summary>
    ///     开始连接时触发事件
    /// </summary>
    public event AsyncEventHandler<EventArgs>? Connecting;

    /// <summary>
    ///     连接成功时触发事件
    /// </summary>
    public event AsyncEventHandler<EventArgs>? Connected;

    /// <summary>
    ///     开始重新连接时触发事件
    /// </summary>
    public event AsyncEventHandler<EventArgs>? Reconnecting;

    /// <summary>
    ///     重新连接成功时触发事件
    /// </summary>
    public event AsyncEventHandler<EventArgs>? Reconnected;

    /// <summary>
    ///     开始关闭连接时触发事件
    /// </summary>
    public event AsyncEventHandler<EventArgs>? Closing;

    /// <summary>
    ///     关闭连接成功时触发事件
    /// </summary>
    public event AsyncEventHandler<EventArgs>? Closed;

    /// <summary>
    ///     开始接收消息时触发事件
    /// </summary>
    public event AsyncEventHandler<EventArgs>? ReceivingStarted;

    /// <summary>
    ///     停止接收消息时触发事件
    /// </summary>
    public event AsyncEventHandler<EventArgs>? ReceivingStopped;

    /// <summary>
    ///     接收文本消息事件
    /// </summary>
    public event AsyncEventHandler<WebSocketTextReceiveResult>? TextReceived;

    /// <summary>
    ///     接收二进制消息事件
    /// </summary>
    public event AsyncEventHandler<WebSocketBinaryReceiveResult>? BinaryReceived;

    /// <summary>
    ///     触发开始连接事件
    /// </summary>
    internal async Task OnConnectingAsync() =>
        await Connecting.TryInvokeAsync(this, EventArgs.Empty);

    /// <summary>
    ///     触发连接成功事件
    /// </summary>
    internal async Task OnConnectedAsync() =>
        await Connected.TryInvokeAsync(this, EventArgs.Empty);

    /// <summary>
    ///     触发开始重新连接事件
    /// </summary>
    internal async Task OnReconnectingAsync() =>
        await Reconnecting.TryInvokeAsync(this, EventArgs.Empty);

    /// <summary>
    ///     触发重新连接成功事件
    /// </summary>
    internal async Task OnReconnectedAsync() =>
        await Reconnected.TryInvokeAsync(this, EventArgs.Empty);

    /// <summary>
    ///     触发开始关闭连接事件
    /// </summary>
    internal async Task OnClosingAsync() =>
        await Closing.TryInvokeAsync(this, EventArgs.Empty);

    /// <summary>
    ///     触发关闭连接成功事件
    /// </summary>
    internal async Task OnClosedAsync() =>
        await Closed.TryInvokeAsync(this, EventArgs.Empty);

    /// <summary>
    ///     触发开始接收消息事件
    /// </summary>
    internal async Task OnReceivingStartedAsync() =>
        await ReceivingStarted.TryInvokeAsync(this, EventArgs.Empty);

    /// <summary>
    ///     触发停止接收消息事件
    /// </summary>
    internal async Task OnReceivingStoppedAsync() =>
        await ReceivingStopped.TryInvokeAsync(this, EventArgs.Empty);

    /// <summary>
    ///     触发接收文本消息事件
    /// </summary>
    /// <param name="receiveResult">
    ///     <see cref="WebSocketTextReceiveResult" />
    /// </param>
    internal async Task OnTextReceivedAsync(WebSocketTextReceiveResult receiveResult) =>
        await TextReceived.TryInvokeAsync(this, receiveResult);

    /// <summary>
    ///     触发接收二进制消息事件
    /// </summary>
    /// <param name="receiveResult">
    ///     <see cref="WebSocketBinaryReceiveResult" />
    /// </param>
    internal async Task OnBinaryReceivedAsync(WebSocketBinaryReceiveResult receiveResult) =>
        await BinaryReceived.TryInvokeAsync(this, receiveResult);
}