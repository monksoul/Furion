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

using System.Text.Json.Nodes;

namespace Furion.HttpRemote;

/// <summary>
///     JSON 请求内容提取器
/// </summary>
/// <remarks>
///     <para>当 <c>data</c> 属性存在时，<c>contentType</c> 为可选。</para>
///     <para>若未指定 <c>contentType</c>，将直接传入 <see cref="JsonNode" />，由底层的 <c>GetContentTypeOrDefault</c> 方法推断内容类型。</para>
/// </remarks>
internal sealed class JsonDataExtractor : HttpJsonExtractorBase
{
    /// <inheritdoc />
    protected override string PropertyName => "data";

    /// <inheritdoc />
    protected override void Extract(HttpRequestBuilder httpRequestBuilder, JsonNode node,
        HttpJsonParsingContext context)
    {
        // 获取内容类型
        string? contentType = null;

        // 检查是否配置了内容类型
        if (context.TryGetNode("contentType", out var contentTypeNode) &&
            contentTypeNode is JsonValue contentTypeValue && contentTypeValue.TryGetValue<string>(out var ct) &&
            !string.IsNullOrWhiteSpace(ct))
        {
            contentType = ct;
        }

        // 设置请求内容
        httpRequestBuilder.SetContent(node, contentType).AddStringContentForFormUrlEncodedContentProcessor();

        // 检查是否配置了内容编码
        if (context.TryGetNode("encoding", out var encodingNode) && encodingNode is JsonValue encodingValue &&
            encodingValue.TryGetValue<string>(out var encoding) && !string.IsNullOrWhiteSpace(encoding))
        {
            // 设置内容编码
            httpRequestBuilder.SetContentEncoding(encoding);
        }
    }
}