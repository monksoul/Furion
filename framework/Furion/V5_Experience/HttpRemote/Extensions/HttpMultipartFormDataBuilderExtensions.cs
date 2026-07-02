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

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpMultipartFormDataBuilder" /> 扩展类
/// </summary>
public static class HttpMultipartFormDataBuilderExtensions
{
    /// <summary>
    ///     添加文件
    /// </summary>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="formFile">
    ///     <see cref="IFormFile" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public static HttpMultipartFormDataBuilder AddFile(this HttpMultipartFormDataBuilder httpMultipartFormDataBuilder,
        IFormFile formFile, string? name = null, string? fileName = null, string? contentType = null,
        Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(formFile);

        // 添加文件流
        return httpMultipartFormDataBuilder.AddStream(formFile.OpenReadStream(), name ?? formFile.Name,
            fileName ?? formFile.FileName, contentType ?? formFile.ContentType, contentEncoding,
            true);
    }

    /// <summary>
    ///     添加多个文件
    /// </summary>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="formFiles">
    ///     <see cref="IFormFile" /> 集合
    /// </param>
    /// <param name="name">表单名称</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public static HttpMultipartFormDataBuilder AddFiles(this HttpMultipartFormDataBuilder httpMultipartFormDataBuilder,
        IEnumerable<IFormFile> formFiles, string? name = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(formFiles);

        // 逐条添加文件
        foreach (var formFile in formFiles)
        {
            httpMultipartFormDataBuilder.AddFile(formFile, name ?? formFile.Name);
        }

        return httpMultipartFormDataBuilder;
    }

    /// <summary>
    ///     添加文件
    /// </summary>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="browserFile">
    ///     <see cref="IBrowserFile" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="maxAllowedSize">流可以提供的最大字节数，默认值为：<c>500KB</c></param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public static HttpMultipartFormDataBuilder AddFile(this HttpMultipartFormDataBuilder httpMultipartFormDataBuilder,
        IBrowserFile browserFile, string? name = null, string? fileName = null, string? contentType = null,
        Encoding? contentEncoding = null, long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(browserFile);

        // 添加文件流
        return httpMultipartFormDataBuilder.AddStream(browserFile.OpenReadStream(maxAllowedSize, cancellationToken),
            name ?? "file", fileName ?? browserFile.Name, contentType ?? browserFile.ContentType, contentEncoding,
            true);
    }

    /// <summary>
    ///     添加多个文件
    /// </summary>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="browserFiles">
    ///     <see cref="IBrowserFile" /> 集合
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="maxAllowedSize">流可以提供的最大字节数，默认值为：<c>500KB</c></param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public static HttpMultipartFormDataBuilder AddFiles(this HttpMultipartFormDataBuilder httpMultipartFormDataBuilder,
        IEnumerable<IBrowserFile> browserFiles, string? name = null, long maxAllowedSize = 512000,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(browserFiles);

        // 逐条添加文件
        foreach (var browserFile in browserFiles)
        {
            httpMultipartFormDataBuilder.AddFile(browserFile, name ?? "file", maxAllowedSize: maxAllowedSize,
                cancellationToken: cancellationToken);
        }

        return httpMultipartFormDataBuilder;
    }
}