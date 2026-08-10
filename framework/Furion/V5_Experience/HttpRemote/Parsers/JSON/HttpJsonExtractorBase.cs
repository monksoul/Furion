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
///     JSON 属性提取器抽象基类
/// </summary>
public abstract class HttpJsonExtractorBase : IHttpJsonExtractor
{
    /// <summary>
    ///     当前提取器负责的 JSON 属性名（主键）
    /// </summary>
    /// <remarks>如 <c>"method"</c>、<c>"url"</c>。</remarks>
    protected abstract string PropertyName { get; }

    /// <summary>
    ///     当前提取器负责的 JSON 属性别名集合
    /// </summary>
    /// <remarks>如 <c>["queries", "query"]</c>。默认为 <c>null</c>。</remarks>
    protected virtual string[]? Aliases => null;

    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpJsonParsingContext context)
    {
        // 尝试匹配主属性名
        if (context.TryGetNode(PropertyName, out var node))
        {
            // 调用派生类的提取信息并构建 HttpRequestBuilder 实例
            Extract(httpRequestBuilder, node!, context);

            return;
        }

        // 空检查
        if (Aliases is null)
        {
            return;
        }

        // 尝试匹配别名
        if (!Aliases.Any(alias => context.TryGetNode(alias, out node)))
        {
            return;
        }

        // 调用派生类的提取信息并构建 HttpRequestBuilder 实例
        Extract(httpRequestBuilder, node!, context);
    }

    /// <summary>
    ///     提取信息并构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="node">当前属性对应的 <see cref="JsonNode" /> 节点</param>
    /// <param name="context">
    ///     <see cref="HttpJsonParsingContext" />
    /// </param>
    protected abstract void Extract(HttpRequestBuilder httpRequestBuilder, JsonNode node,
        HttpJsonParsingContext context);
}