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
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Furion.HttpRemote;

/// <summary>
///     字符串内容处理器
/// </summary>
public class StringContentProcessor : HttpContentProcessorBase
{
    /// <inheritdoc />
    public override bool CanProcess(HttpContentProcessorContext context) =>
        context.RawContent is StringContent or JsonContent ||
        context.ContentType.IsIn([
            MediaTypeNames.Application.Json,
            MediaTypeNames.Application.JsonPatch,
            MediaTypeNames.Application.Xml,
            MediaTypeNames.Application.XmlPatch,
            MediaTypeNames.Text.Xml,
            MediaTypeNames.Text.Html,
            MediaTypeNames.Text.Plain,
            MediaTypeNames.Application.Soap // SOAP 1.2
        ], StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override HttpContent? Process(HttpContentProcessorContext context)
    {
        // 尝试解析 HttpContent 类型
        if (TryProcess(context, out var httpContent))
        {
            return httpContent;
        }

        // 将原始请求内容转换为字符串
        var content = context.RawContent is JsonElement or JsonNode || context.RawContent!.GetType().IsBasicType()
            ? context.RawContent.ToInvariantCultureString()
            : context.RawContent.ToJsonString(ResolveJsonSerializerOptions(context.HttpClientName));

        // 初始化 StringContent 实例
        var stringContent = new StringContent(content!, context.Encoding,
            new MediaTypeHeaderValue(context.ContentType) { CharSet = context.Encoding?.WebName });

        return stringContent;
    }
}