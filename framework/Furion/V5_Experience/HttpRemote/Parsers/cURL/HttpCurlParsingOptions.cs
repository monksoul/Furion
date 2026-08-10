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
///     cURL 解析选项
/// </summary>
public sealed class HttpCurlParsingOptions
{
    /// <summary>
    ///     <inheritdoc cref="HttpCurlParsingOptions" />
    /// </summary>
    internal HttpCurlParsingOptions() => Extractors.AddRange(GetDefaultExtractors());

    /// <summary>
    ///     <see cref="IHttpCurlExtractor" /> 提取器集合
    /// </summary>
    public List<IHttpCurlExtractor> Extractors { get; } = [];

    /// <summary>
    ///     移除指定类型的 <see cref="IHttpCurlExtractor" /> 提取器
    /// </summary>
    /// <typeparam name="TExtractor">
    ///     <see cref="IHttpCurlExtractor" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="HttpCurlParsingOptions" />
    /// </returns>
    public HttpCurlParsingOptions RemoveExtractor<TExtractor>() where TExtractor : IHttpCurlExtractor
    {
        Extractors.RemoveAll(u => u is TExtractor);

        return this;
    }

    /// <summary>
    ///     添加自定义 <see cref="IHttpCurlExtractor" /> 提取器
    /// </summary>
    /// <param name="extractor">
    ///     <see cref="IHttpCurlExtractor" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpCurlParsingOptions" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpCurlParsingOptions AddExtractor(IHttpCurlExtractor extractor)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(extractor);

        Extractors.Add(extractor);

        return this;
    }

    /// <summary>
    ///     批量添加自定义 <see cref="IHttpCurlExtractor" /> 提取器
    /// </summary>
    /// <param name="extractors">
    ///     <see cref="IHttpCurlExtractor" /> 集合
    /// </param>
    /// <returns>
    ///     <see cref="HttpCurlParsingOptions" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpCurlParsingOptions AddExtractors(params IEnumerable<IHttpCurlExtractor> extractors)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(extractors);

        Extractors.AddRange(extractors);

        return this;
    }

    /// <summary>
    ///     获取默认的内置提取器集合
    /// </summary>
    /// <returns>
    ///     <see cref="IEnumerable{T}" />
    /// </returns>
    internal static IEnumerable<IHttpCurlExtractor> GetDefaultExtractors()
    {
        yield return new CurlMethodExtractor();
        yield return new CurlHeadExtractor();
        yield return new CurlHeaderExtractor();
        yield return new CurlCookieExtractor();
        yield return new CurlDataExtractor();
        yield return new CurlAuthExtractor();
        yield return new CurlUserAgentExtractor();
        yield return new CurlRefererExtractor();
        yield return new CurlFormExtractor();
        yield return new CurlTimeoutExtractor();
        yield return new CurlVersionExtractor();
        yield return new CurlUrlExtractor();
    }
}