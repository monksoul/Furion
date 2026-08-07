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

using Furion.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mime;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="IActionResult" /> 内容转换器
/// </summary>
public class IActionResultContentConverter : HttpContentConverterBase<IActionResult>
{
    /// <inheritdoc />
    public override bool KeepsResponseAlive => true;

    /// <inheritdoc />
    public override IActionResult? Read(HttpContentConverterContext context,
        CancellationToken cancellationToken = default) =>
        AsyncUtility.RunSync(() => ReadAsync(context, cancellationToken));

    /// <inheritdoc />
    public override async Task<IActionResult?> ReadAsync(HttpContentConverterContext context,
        CancellationToken cancellationToken = default)
    {
        // 获取 HttpResponseMessage 实例
        var httpResponseMessage = context.ResponseMessage;

        // 尝试为无内容响应生成对应的 IActionResult
        if (TryGetEmptyContentResult(httpResponseMessage, out var statusCode, out var emptyContentResult))
        {
            return emptyContentResult;
        }

        // 获取响应内容标头
        var contentHeaders = httpResponseMessage.Content.Headers;

        // 获取内容类型
        var contentType = contentHeaders.ContentType;
        var mediaType = contentType?.MediaType;

        switch (mediaType)
        {
            case MediaTypeNames.Application.Json:
            case MediaTypeNames.Application.JsonPatch:
            case MediaTypeNames.Application.Xml:
            case MediaTypeNames.Application.XmlPatch:
            case MediaTypeNames.Text.Xml:
            case MediaTypeNames.Text.Html:
            case MediaTypeNames.Text.Plain:
            case MediaTypeNames.Application.Soap:
                {
                    // 读取流内容
                    var rawStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

                    // 尝试解压内容流，解决部分内容流被压缩的情况
                    var decompressedStream = Helpers.WrapDecompressionStream(rawStream, httpResponseMessage);

                    var streamResult = new FileStreamResult(decompressedStream, contentType?.ToString()!)
                    {
                        LastModified = contentHeaders.LastModified?.UtcDateTime
                    };

                    // 确保原始流被正确释放
                    if (decompressedStream != rawStream)
                    {
                        await rawStream.DisposeAsync();
                    }

                    return streamResult;
                }
            default:
                // 读取流内容
                var fileStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

                return new FileStreamResult(fileStream, contentType?.ToString() ?? MediaTypeNames.Application.Octet)
                {
                    // 尝试从响应标头 Content-Disposition 中解析文件名
                    FileDownloadName = Helpers.ExtractFileNameFromContentDisposition(contentHeaders.ContentDisposition),
                    LastModified = contentHeaders.LastModified?.UtcDateTime
                };
        }
    }

    /// <summary>
    ///     尝试为无内容响应生成对应的 <see cref="IActionResult" />
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <param name="emptyContentResult">
    ///     <see cref="IActionResult" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool TryGetEmptyContentResult(HttpResponseMessage httpResponseMessage,
        out HttpStatusCode statusCode, [NotNullWhen(true)] out IActionResult? emptyContentResult)
    {
        // 获取状态码
        statusCode = httpResponseMessage.StatusCode;

        emptyContentResult = statusCode switch
        {
            // 无响应内容的状态码
            HttpStatusCode.NoContent => new NoContentResult(),
            HttpStatusCode.ResetContent => new StatusCodeResult((int)HttpStatusCode.ResetContent),
            // 304 特殊处理：返回空内容但必须保留 ETag/Last-Modified 等验证头
            HttpStatusCode.NotModified => new StatusCodeResult((int)HttpStatusCode.NotModified),
            HttpStatusCode.SwitchingProtocols => new StatusCodeResult((int)HttpStatusCode.SwitchingProtocols),
            HttpStatusCode.Processing => new StatusCodeResult((int)HttpStatusCode.Processing),

            // 可能存在响应内容的状态码。这里不应该处理，因为它们通常包含错误详情
            // HttpStatusCode.BadRequest => new BadRequestResult(),
            // HttpStatusCode.Unauthorized => new UnauthorizedResult(),
            // HttpStatusCode.NotFound => new NotFoundResult(),
            // HttpStatusCode.Conflict => new ConflictResult(),
            // HttpStatusCode.UnsupportedMediaType => new UnsupportedMediaTypeResult(),
            // HttpStatusCode.UnprocessableEntity => new UnprocessableEntityResult(),
            _ => null
        };

        return emptyContentResult is not null;
    }
}