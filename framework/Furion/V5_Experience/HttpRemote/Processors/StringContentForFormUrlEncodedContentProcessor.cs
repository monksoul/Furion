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
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace Furion.HttpRemote;

/// <summary>
///     URL 编码的表单内容处理器
/// </summary>
/// <remarks>
///     解决 <see cref="FormUrlEncodedContent" /> 无法设置编码问题。<see cref="FormUrlEncodedContent" /> 的编码格式不是 <c>utf-8</c>，而是
///     <c>Encoding.Latin1</c>。
/// </remarks>
public class StringContentForFormUrlEncodedContentProcessor : FormUrlEncodedContentProcessor
{
    /// <summary>
    ///     是否对表单数据进行 URL 编码
    /// </summary>
    /// <remarks>
    ///     <para>默认值为：<c>true</c>。</para>
    ///     <para>设置为 <c>false</c> 时，表单数据将保持原始格式发送，不进行 URL 编码转义。</para>
    ///     <para>注意：禁用编码可能导致特殊字符（如 <c>&amp;</c>, <c>=</c>, 空格等）被服务器错误解析。</para>
    /// </remarks>
    public bool UrlEncode { get; set; } = true;

    /// <inheritdoc />
    public override HttpContent? Process(HttpContentProcessorContext context)
    {
        // 尝试解析 HttpContent 类型
        if (TryProcess(context, out var httpContent))
        {
            return httpContent;
        }

        string content;
        var rawContent = context.RawContent;

        // 检查是否是字符串类型或字符串存储的 JsonNode 和 JsonElement
        if (rawContent is string ||
            (rawContent is JsonNode jsonNode && jsonNode.GetValueKind() == JsonValueKind.String) ||
            rawContent is JsonElement { ValueKind: JsonValueKind.String })
        {
            // 解析 URL 编码格式字符串
            var nameValueCollection = HttpUtility.ParseQueryString(rawContent.ToInvariantCultureString()!);

            content = GetContentString(
                nameValueCollection.AllKeys.Where(k => k is not null).SelectMany(k =>
                    (nameValueCollection.GetValues(k) ?? []).Select(v => new KeyValuePair<string, string?>(k!, v))),
                UrlEncode);
        }
        else
        {
            // 将原始请求类型转换为字符串字典类型
            content = GetContentString(rawContent.ObjectToDictionary()!.ToDictionary(
                u => u.Key.ToInvariantCultureString()!,
                u => u.Value?.ToInvariantCultureString()), UrlEncode);
        }

        // 初始化 StringContent 实例
        var stringContent = new StringContent(content, context.Encoding,
            new MediaTypeHeaderValue(context.ContentType) { CharSet = context.Encoding?.WebName });

        return stringContent;
    }

    /// <summary>
    ///     获取 URL 编码的表单内容格式
    /// </summary>
    /// <param name="nameValueCollection">键值对集合</param>
    /// <param name="urlEncode">是否对表单数据进行 URL 编码</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GetContentString(IEnumerable<KeyValuePair<string, string?>> nameValueCollection,
        bool urlEncode = true)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(nameValueCollection);

        // 初始化 StringBuilder 实例
        var stringBuilder = new StringBuilder();

        // 生成 {key}={value}&... 格式
        foreach (var nameValue in nameValueCollection)
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append('&');
            }

            if (urlEncode)
            {
                // 进行 URL 编码
                stringBuilder.Append(Encode(nameValue.Key));
                stringBuilder.Append('=');
                stringBuilder.Append(Encode(nameValue.Value));
            }
            else
            {
                // 不进行编码，直接拼接原始值
                stringBuilder.Append(nameValue.Key);
                stringBuilder.Append('=');
                stringBuilder.Append(nameValue.Value ?? string.Empty);
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     对数据进行 URL 编码
    /// </summary>
    /// <param name="data">数据</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string Encode(string? data) =>
        string.IsNullOrEmpty(data) ? string.Empty : Uri.EscapeDataString(data).Replace("%20", "+");
}