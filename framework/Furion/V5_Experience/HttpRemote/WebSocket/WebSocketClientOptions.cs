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

using System.Net.WebSockets;

namespace Furion.HttpRemote;

/// <summary>
///     WebSocket 客户端配置选项
/// </summary>
public sealed class WebSocketClientOptions
{
    /// <summary>
    ///     <inheritdoc cref="WebSocketClientOptions" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    public WebSocketClientOptions(string serverUri)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(serverUri);

        ServerUri = new Uri(serverUri);
    }

    /// <summary>
    ///     <inheritdoc cref="WebSocketClientOptions" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    public WebSocketClientOptions(Uri serverUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serverUri);

        ServerUri = serverUri;
    }

    /// <summary>
    ///     <inheritdoc cref="WebSocketClientOptions" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    /// <param name="configure">用于配置 <see cref="ClientWebSocketOptions" /> 的操作</param>
    public WebSocketClientOptions(string serverUri, Action<ClientWebSocketOptions> configure)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(serverUri);
        ArgumentNullException.ThrowIfNull(configure);

        ServerUri = new Uri(serverUri);
        Configure = configure;
    }

    /// <summary>
    ///     <inheritdoc cref="WebSocketClientOptions" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    /// <param name="configure">用于配置 <see cref="ClientWebSocketOptions" /> 的操作</param>
    public WebSocketClientOptions(Uri serverUri, Action<ClientWebSocketOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serverUri);
        ArgumentNullException.ThrowIfNull(configure);

        ServerUri = serverUri;
        Configure = configure;
    }

    /// <summary>
    ///     服务器地址
    /// </summary>
    public Uri ServerUri { get; }

    /// <summary>
    ///     重连的间隔时间（毫秒）
    /// </summary>
    /// <remarks>默认值为 2 秒。</remarks>
    public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     最大重连次数
    /// </summary>
    /// <remarks>默认最大重连次数为 10。</remarks>
    public int MaxReconnectRetries { get; set; } = 10;

    /// <summary>
    ///     连接超时时间
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    ///     接收服务器新消息缓冲区大小
    /// </summary>
    /// <remarks>以字节为单位，默认值为：<c>4 KB</c>。</remarks>
    public int ReceiveBufferSize { get; set; } = 1024 * 4;

    /// <summary>
    ///     用于配置 <see cref="ClientWebSocketOptions" /> 的操作
    /// </summary>
    public Action<ClientWebSocketOptions>? Configure { get; set; }
}