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
///     JSON 解析上下文
/// </summary>
public sealed class HttpJsonParsingContext
{
    /// <summary>
    ///     <inheritdoc cref="HttpJsonParsingContext" />
    /// </summary>
    /// <param name="rootObject">根 JSON 对象</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpJsonParsingContext(JsonObject rootObject)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(rootObject);

        RootObject = rootObject;
    }

    /// <summary>
    ///     根 JSON 对象
    /// </summary>
    public JsonObject RootObject { get; }

    /// <summary>
    ///     尝试获取指定属性名的 <see cref="JsonNode" /> 节点
    /// </summary>
    /// <param name="propertyName">属性名</param>
    /// <param name="node">
    ///     <see cref="JsonNode" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool TryGetNode(string propertyName, out JsonNode? node)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(propertyName);

        return RootObject.TryGetPropertyValue(propertyName, out node) && node is not null;
    }

    /// <summary>
    ///     获取指定属性名的 <see cref="JsonNode" /> 节点
    /// </summary>
    /// <param name="propertyName">属性名</param>
    /// <returns>
    ///     <see cref="JsonNode" /> 或 <c>null</c>
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public JsonNode? GetNode(string propertyName)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(propertyName);

        RootObject.TryGetPropertyValue(propertyName, out var node);

        return node;
    }

    /// <summary>
    ///     检查 JSON 对象中是否包含指定属性
    /// </summary>
    /// <param name="propertyName">属性名</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool ContainsProperty(string propertyName)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(propertyName);

        return RootObject.ContainsKey(propertyName);
    }
}