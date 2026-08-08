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

using Microsoft.AspNetCore.Http;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpContext" /> 转发配置选项
/// </summary>
public sealed class HttpContextForwardOptions
{
    /// <summary>
    ///     是否转发查询参数（URL 参数）
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithQueryParameters { get; set; } = true;

    /// <summary>
    ///     是否转发请求标头
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithRequestHeaders { get; set; } = true;

    /// <summary>
    ///     是否转发响应状态码
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithResponseStatusCode { get; set; } = true;

    /// <summary>
    ///     是否转发响应标头
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithResponseHeaders { get; set; } = true;

    /// <summary>
    ///     是否转发响应内容标头
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithResponseContentHeaders { get; set; } = true;

    /// <summary>
    ///     是否重新设置 <c>Host</c> 请求标头
    /// </summary>
    /// <remarks>在一些目标服务器中，可能需要校验该请求标头。默认值为：<c>false</c>。</remarks>
    public bool ResetHostRequestHeader { get; set; }

    /// <summary>
    ///     忽略在转发时需要跳过的查询参数（URL 参数）列表
    /// </summary>
    public string[]? IgnoreQueryParameters { get; set; }

    /// <summary>
    ///     忽略在转发时需要跳过的请求标头列表
    /// </summary>
    public string[]? IgnoreRequestHeaders { get; set; }

    /// <summary>
    ///     忽略在转发时需要跳过的响应标头列表
    /// </summary>
    /// <remarks>
    ///     若响应标头中包含 <c>Content-Length</c>，且其值与实际响应内容大小不符，则可能引发“Error while copying content to a
    ///     stream.”。忽略此标头有助于解决因长度不匹配引起的错误。
    /// </remarks>
    public string[]? IgnoreResponseHeaders { get; set; }

    /// <summary>
    ///     允许转发的目标主机白名单
    /// </summary>
    /// <remarks>
    ///     <para>用于防范服务端请求伪造（SSRF）攻击。仅当目标地址匹配此列表中的项时，转发才会被允许。</para>
    ///     <para>支持的格式：</para>
    ///     <list type="bullet">
    ///         <item><c>"example.com"</c> - 仅主机名，匹配任意协议（http/https）的默认端口（80/443）。</item>
    ///         <item><c>"example.com:8080"</c> - 主机+端口，匹配任意协议的指定端口。</item>
    ///         <item><c>"example.com:*"</c> - 主机+端口通配符，匹配任意协议下的任意端口。</item>
    ///         <item><c>"https://example.com"</c> - 协议+主机，只匹配指定协议的默认端口。</item>
    ///         <item><c>"http://example.com:8080"</c> - 协议+主机+端口，精确匹配。</item>
    ///         <item><c>"https://example.com:*"</c> - 协议+主机+端口通配符，只匹配指定协议的任意端口。</item>
    ///     </list>
    ///     <para>匹配规则：忽略大小写；主机部分必须完全一致；端口部分支持数字或 <c>*</c> 通配符。</para>
    ///     <para><b>特殊通配符</b>：若集合中包含独立的 <c>"*"</c>（不带主机），则允许转发到 <b>任意</b> 主机和协议，完全绕过验证。</para>
    ///     <para>如果该集合为 <c>null</c> 或空，所有通过 <c>X-Forward-To</c> 请求标头指定的目标地址都将被拒绝。</para>
    /// </remarks>
    public ICollection<string>? AllowedHosts { get; set; }

    /// <summary>
    ///     用于在转发响应之前执行自定义操作
    /// </summary>
    public Action<HttpContext, HttpResponseMessage>? OnForward { get; set; }
}