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
using Microsoft.Net.Http.Headers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using ContentDispositionHeaderValue = System.Net.Http.Headers.ContentDispositionHeaderValue;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求模块帮助类
/// </summary>
internal static class Helpers
{
    /// <summary>
    ///     HTTP QUERY <see cref="HttpMethod" /> 静态实例
    /// </summary>
    internal static readonly HttpMethod HttpQuery = ParseHttpMethod("QUERY");

    /// <summary>
    ///     从互联网 URL 地址中加载流
    /// </summary>
    /// <param name="requestUri">互联网 URL 地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="maxResponseContentBufferSize">响应内容的最大缓存大小。默认值为：<c>100MB</c>。</param>
    /// <param name="httpMethod"><see cref="HttpMethod" />，默认值为：<see cref="HttpMethod.Get" /></param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal static Stream GetStreamFromRemote(string requestUri,
        Action<HttpClient, HttpRequestMessage>? configure = null, long maxResponseContentBufferSize = 104857600L,
        HttpMethod? httpMethod = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);

        // 检查 URL 地址是否是互联网地址
        if (!NetworkUtility.IsWebUrl(requestUri))
        {
            throw new ArgumentException($"Invalid internet address: `{requestUri}`.", nameof(requestUri));
        }

        // 初始化 HttpClient 实例
        using var httpClient = new HttpClient();

        // 限制流大小
        httpClient.MaxResponseContentBufferSize = maxResponseContentBufferSize;

        // 启用性能优化（返回 Stream 内容时，请勿启用此配置，否则流将因压缩而变得不可读）
        // httpClient.PerformanceOptimization();

        // 设置默认 User-Agent
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, UserAgents.Edge.PC);

        try
        {
            // 初始化 HttpRequestMessage 实例
            var httpRequestMessage = new HttpRequestMessage(httpMethod ?? HttpMethod.Get, requestUri);

            // 调用自定义配置委托
            configure?.Invoke(httpClient, httpRequestMessage);

            // 发送 HTTP 远程请求
            var httpResponseMessage = httpClient.Send(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

            // 确保请求成功
            httpResponseMessage.EnsureSuccessStatusCode();

            // 读取流并返回
            return httpResponseMessage.Content.ReadAsStream();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to load stream from internet address: `{requestUri}`.", e);
        }
    }

    /// <summary>
    ///     从 <see cref="Uri" /> 中解析文件的名称
    /// </summary>
    /// <param name="uri">
    ///     <see cref="Uri" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? GetFileNameFromUri(Uri? uri)
    {
        // 空检查
        if (uri is null)
        {
            return null;
        }

        // 获取 URL 的绝对路径
        var path = uri.AbsolutePath;

        // 使用 / 分割路径，并获取最后一个部分作为潜在的文件的名称
        var parts = path.Split('/');
        var fileName = parts.Length > 0 ? parts[^1] : string.Empty;

        // 检查文件的名称是否为空或仅由点组成
        if (string.IsNullOrEmpty(fileName) || fileName.Trim('.').Length == 0)
        {
            return string.Empty;
        }

        // 查找文件的名称中的查询字符串开始位置。如果存在查询字符串，则去除它
        var queryStartIndex = fileName.IndexOf('?');
        if (queryStartIndex != -1)
        {
            fileName = fileName[..queryStartIndex];
        }

        // 检查文件的名称是否包含有效的扩展名
        var lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex == -1 || lastDotIndex == fileName.Length - 1)
        {
            return string.Empty;
        }

        // 将字符串转换为其未转义表示形式
        return Uri.UnescapeDataString(fileName).Trim('"');
    }

    /// <summary>
    ///     解析 HTTP 谓词
    /// </summary>
    /// <param name="httpMethod">HTTP 谓词</param>
    /// <returns>
    ///     <see cref="HttpMethod" />
    /// </returns>
    internal static HttpMethod ParseHttpMethod(string? httpMethod)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(httpMethod);

        return HttpMethod.Parse(httpMethod);
    }

    /// <summary>
    ///     检查 HTTP 状态码是否是重定向状态码并返回重定向时应使用的 HTTP 请求方法
    /// </summary>
    /// <param name="statusCode">
    ///     <see cref="HttpStatusCode" />
    /// </param>
    /// <param name="originalHttpMethod">
    ///     <see cref="HttpMethod" />
    /// </param>
    /// <param name="httpMethod">
    ///     <see cref="HttpMethod" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool DetermineRedirectMethod(HttpStatusCode statusCode, HttpMethod originalHttpMethod,
        [NotNullWhen(true)] out HttpMethod? httpMethod)
    {
        switch (statusCode)
        {
            // 300, 301, 302, 303 使用 GET 请求
            case HttpStatusCode.Ambiguous or HttpStatusCode.Moved or HttpStatusCode.Redirect
                or HttpStatusCode.RedirectMethod:
                // Query 同样使用 Get 重定向，参考文献：https://www.rfc-editor.org/info/rfc10008/#appendix-A.5-3
                httpMethod = HttpMethod.Get;
                return true;
            // 307, 308 保持原来请求
            case HttpStatusCode.RedirectKeepVerb:
            case var code when (int)code == 308:
                httpMethod = originalHttpMethod;
                return true;
            default:
                httpMethod = null;
                return false;
        }
    }

    /// <summary>
    ///     从给定的绝对 URI 中解析出基础地址
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <returns>
    ///     <see cref="Uri" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static Uri ParseBaseAddress(Uri? requestUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(requestUri);

        // 检查是否是绝对地址
        if (!requestUri.IsAbsoluteUri)
        {
            throw new ArgumentException("The requestUri must be an absolute URI.", nameof(requestUri));
        }

        return new Uri(
            $"{requestUri.Scheme}://{requestUri.Host}{(requestUri.IsDefaultPort ? string.Empty : $":{requestUri.Port}")}");
    }

    /// <summary>
    ///     根据原始内容推断内容类型，失败时返回默认值
    /// </summary>
    /// <param name="rawContent">原始请求内容</param>
    /// <param name="defaultContentType">默认请求内容类型</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GetContentTypeOrDefault(object? rawContent, string? defaultContentType) =>
        rawContent switch
        {
            JsonContent => MediaTypeNames.Application.Json,
            FormUrlEncodedContent => MediaTypeNames.Application.FormUrlEncoded,
            (byte[] or Stream or ByteArrayContent or StreamContent or ReadOnlyMemoryContent or ReadOnlyMemory<byte>)
                and not StringContent => MediaTypeNames.Application
                    .Octet,
            MultipartContent => MediaTypeNames.Multipart.FormData,
            MultipartFile => MediaTypeNames.Application.Octet,
            FileInfo fileInfo => FileTypeMapper.GetContentType(fileInfo.Name),
            _ => defaultContentType ?? MediaTypeNames.Text.Plain
        };

    /// <summary>
    ///     尝试从响应标头 <c>Content-Disposition</c> 中解析文件名
    /// </summary>
    /// <param name="contentDisposition">
    ///     <see cref="ContentDispositionHeaderValue" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ExtractFileNameFromContentDisposition(ContentDispositionHeaderValue? contentDisposition)
    {
        // 空检查
        if (!string.IsNullOrWhiteSpace(contentDisposition?.FileNameStar))
        {
            // 将字符串转换为其未转义表示形式
            return Uri.UnescapeDataString(contentDisposition.FileNameStar.Trim('"'));
        }

        // 空检查
        // ReSharper disable once InvertIf
        if (!string.IsNullOrWhiteSpace(contentDisposition?.FileName))
        {
            var decodedFileName = contentDisposition.FileName.Trim('"');

            // 获取原始 "filename=" 参数值
            var rawFileName = contentDisposition.Parameters
                .First(p => string.Equals(p.Name, "filename", StringComparison.OrdinalIgnoreCase)).Value!;

            // 检查首尾是否包含双引号
            // ReSharper disable once InvertIf
            if (rawFileName.StartsWith('"') && rawFileName.EndsWith('"'))
            {
                rawFileName = rawFileName.Trim('"');

                // 检查是否为 MIME 编码格式（如 =?UTF-8?B?...?=），若是则跳过乱码修复
                if (!(rawFileName.StartsWith("=?") && rawFileName.EndsWith("?=")))
                {
                    // 尝试修复乱码，如 UTF-8 字节被错误解释为 ISO-8859-1
                    decodedFileName = Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(rawFileName));
                }
            }

            // 将字符串转换为其未转义表示形式
            return Uri.UnescapeDataString(decodedFileName);
        }

        return null;
    }

    /// <summary>
    ///     将传入的字符串数组中所有非空白字符串用换行符（\r\n）拼接成一个字符串
    /// </summary>
    /// <param name="lines">字符串数组</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string JoinNonEmptyLines(params string?[]? lines) =>
        lines is null or { Length: 0 }
            ? string.Empty
            : string.Join("\r\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));

    /// <summary>
    ///     组合基地址和请求地址
    /// </summary>
    /// <param name="baseUrl">基地址</param>
    /// <param name="requestUrl">请求地址</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string CombineUrl(string? baseUrl, string? requestUrl)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return requestUrl ?? string.Empty;
        }

        // 空检查
        if (string.IsNullOrWhiteSpace(requestUrl))
        {
            return baseUrl;
        }

        // 检查 requestUrl 是否是绝对 URL
        if (Uri.TryCreate(requestUrl, UriKind.Absolute, out _))
        {
            return requestUrl;
        }

        return baseUrl.TrimEnd('/') + "/" + requestUrl.TrimStart('/');
    }
}