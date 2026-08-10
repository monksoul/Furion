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

namespace Furion.HttpRemote;

/// <summary>
///     cURL 提取器接口
/// </summary>
public interface IHttpCurlExtractor
{
    /// <summary>
    ///     从当前 Token 位置提取信息并构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <remarks>
    ///     <para>如果当前 Token 属于此提取器的管辖范围并成功消费，则返回 <c>true</c>；否则返回 <c>false</c>。</para>
    ///     <para>注意：当返回 <c>true</c> 时，实现类必须负责调用 <see cref="HttpCurlParsingContext.Advance" /> 推进游标，否则将导致解析死循环。</para>
    /// </remarks>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="context">
    ///     <see cref="HttpCurlParsingContext" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    bool TryExtract(HttpRequestBuilder httpRequestBuilder, HttpCurlParsingContext context);
}

/// <summary>
///     支持自定义优先级的 cURL 提取器接口
/// </summary>
public interface IOrderedHttpCurlExtractor : IHttpCurlExtractor
{
    /// <summary>
    ///     提取器的优先级
    /// </summary>
    /// <remarks>数值越小优先级越高。</remarks>
    int Order { get; }
}