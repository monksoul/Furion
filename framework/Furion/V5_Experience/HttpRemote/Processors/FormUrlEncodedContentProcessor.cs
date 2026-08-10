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
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Furion.HttpRemote;

/// <summary>
///     URL 编码的表单内容处理器
/// </summary>
public class FormUrlEncodedContentProcessor : HttpContentProcessorBase
{
    /// <inheritdoc />
    public override bool CanProcess(HttpContentProcessorContext context) =>
        context.RawContent is FormUrlEncodedContent ||
        context.ContentType.IsIn([MediaTypeNames.Application.FormUrlEncoded], StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override HttpContent? Process(HttpContentProcessorContext context)
    {
        // 尝试解析 HttpContent 类型
        if (TryProcess(context, out var httpContent))
        {
            return httpContent;
        }

        var rawContent = context.RawContent;

        // 检查是否是字符串类型或字符串存储的 JsonNode 和 JsonElement
        if (rawContent is string ||
            (rawContent is JsonNode jsonNode && jsonNode.GetValueKind() == JsonValueKind.String) ||
            rawContent is JsonElement { ValueKind: JsonValueKind.String })
        {
            // 初始化 StringContent 实例（该方式不会自动编码，如空格）
            // 如需设置编码可注册 StringContentForFormUrlEncodedContentProcessor 处理器
            var stringContent = new StringContent(rawContent.ToInvariantCultureString()!, context.Encoding,
                new MediaTypeHeaderValue(context.ContentType) { CharSet = context.Encoding?.WebName });

            return stringContent;
        }

        // 将原始请求类型转换为字符串字典类型
        var nameValueCollection = rawContent.ObjectToDictionary()!.ToDictionary(
            u => u.Key.ToInvariantCultureString()!,
            u => u.Value?.ToInvariantCultureString());

        // 初始化 FormUrlEncodedContent 实例
        var formUrlEncodedContent = new FormUrlEncodedContent(nameValueCollection);
        formUrlEncodedContent.Headers.ContentType =
            new MediaTypeHeaderValue(context.ContentType) { CharSet = context.Encoding?.WebName };

        return formUrlEncodedContent;
    }
}