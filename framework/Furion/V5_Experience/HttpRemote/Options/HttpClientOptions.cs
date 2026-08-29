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

using System.Text.Json;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpClient" /> 配置选项
/// </summary>
public sealed class HttpClientOptions
{
    /// <summary>
    ///     JSON 序列化配置
    /// </summary>
    /// <remarks>
    ///     在应用程序启动时，<c>IHttpClientBuilder.ConfigureOptions(Action)</c> 方法会读取
    ///     <see cref="HttpRemoteOptions.JsonSerializerOptions" /> 的值，并将其设置到此属性。
    /// </remarks>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = null!;

    /// <summary>
    ///     指定 JSON 响应反序列化包装器
    /// </summary>
    /// <remarks>
    ///     <para>使用时需明确调用 <see cref="HttpRequestBuilder.UseJsonResponseWrapper()" />。</para>
    ///     <para>若还需对响应做额外校验或转换，可通过 <see cref="JsonResponseWrapper.ResultHandler" /> 实现。</para>
    /// </remarks>
    public JsonResponseWrapper? JsonResponseWrapper { get; set; }

    /// <summary>
    ///     是否全局启用 JSON 响应反序列化包装器
    /// </summary>
    public bool? UseJsonResponseWrapper { get; set; }

    /// <summary>
    ///     Access Token 提供器配置
    /// </summary>
    public IHttpAccessTokenProvider? AccessTokenProvider { get; set; }

    /// <summary>
    ///     事件处理程序提供器配置
    /// </summary>
    public IHttpRequestEventHandler? RequestEventHandler { get; set; }

    /// <summary>
    ///     接口调用配额限制配置
    /// </summary>
    /// <remarks>
    ///     <para>用于对接像微信 API 这样对不同接口有独立调用限制的场景。需配合 <see cref="HttpRequestBuilder.SetQuotaKey(string)" /> 为每个请求指定对应的配额键。</para>
    ///     <para>推荐在 <c>appsettings.json</c> 等配置文件中定义，避免在代码中硬编码大量键值。示例如下：</para>
    ///     <code>
    ///     {
    ///       "HttpQuotas": {
    ///         "weixin": {
    ///           "wechat/accesstoken": { "MaxCount": 2000, "Strategy": "daily" },
    ///           "wechat/menu_create":  { "MaxCount": 1000, "Strategy": "weekly" },
    ///           "wechat/upload_media": { "MaxCount": 50000, "Strategy": "monthly" }
    ///         }
    ///       }
    ///     }
    ///     </code>
    /// </remarks>
    public Dictionary<string, HttpQuotaLimit>? QuotaLimits { get; set; }
}