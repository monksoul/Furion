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
using Furion.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

namespace Furion.HttpRemote.Extensions;

/// <summary>
///     HTTP 远程服务模块扩展类
/// </summary>
public static partial class HttpRemoteExtensions
{
    /// <summary>
    ///     添加 HTTP 远程请求分析工具处理委托
    /// </summary>
    /// <remarks>建议在生产环境中禁用或关闭。</remarks>
    /// <param name="builder">
    ///     <see cref="IHttpClientBuilder" />
    /// </param>
    /// <param name="disableIn">自定义禁用配置委托</param>
    /// <returns>
    ///     <see cref="IHttpClientBuilder" />
    /// </returns>
    public static IHttpClientBuilder AddProfilerDelegatingHandler(this IHttpClientBuilder builder,
        Func<bool>? disableIn = null)
    {
        // 检查是否禁用请求分析工具
        if (disableIn?.Invoke() == true)
        {
            return builder;
        }

        // 注册请求分析工具服务
        builder.Services.TryAddTransient<ProfilerDelegatingHandler>();

        // 添加请求分析工具处理委托
        return builder.AddHttpMessageHandler<ProfilerDelegatingHandler>();
    }

    /// <summary>
    ///     添加 HTTP 远程请求分析工具处理委托
    /// </summary>
    /// <param name="builder">
    ///     <see cref="IHttpClientBuilder" />
    /// </param>
    /// <param name="disableInProduction">是否在生产环境中禁用。默认值为：<c>false</c>。</param>
    /// <returns>
    ///     <see cref="IHttpClientBuilder" />
    /// </returns>
    public static IHttpClientBuilder AddProfilerDelegatingHandler(this IHttpClientBuilder builder,
        bool disableInProduction) =>
        builder.AddProfilerDelegatingHandler(() =>
            disableInProduction &&
            (string.Equals(GetHostEnvironmentName(builder.Services), "Production",
                StringComparison.OrdinalIgnoreCase) || !EnvironmentUtility.IsDevelopment));

    /// <summary>
    ///     配置 <see cref="HttpClient" /> 额外选项
    /// </summary>
    /// <param name="builder">
    ///     <see cref="IHttpClientBuilder" />
    /// </param>
    /// <param name="configure">自定义配置选项</param>
    /// <returns>
    ///     <see cref="IHttpClientBuilder" />
    /// </returns>
    public static IHttpClientBuilder ConfigureOptions(this IHttpClientBuilder builder,
        Action<HttpClientOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.AddOptions<HttpClientOptions>(builder.Name).Configure(options =>
        {
            options.IsDefault = false;
            configure.Invoke(options);
        });

        return builder;
    }

    /// <summary>
    ///     配置 <see cref="HttpClient" /> 额外选项
    /// </summary>
    /// <param name="builder">
    ///     <see cref="IHttpClientBuilder" />
    /// </param>
    /// <param name="configure">自定义配置选项</param>
    /// <returns>
    ///     <see cref="IHttpClientBuilder" />
    /// </returns>
    public static IHttpClientBuilder ConfigureOptions(this IHttpClientBuilder builder,
        Action<HttpClientOptions, IServiceProvider> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.AddOptions<HttpClientOptions>(builder.Name).Configure<IServiceProvider>((options, provider) =>
        {
            options.IsDefault = false;
            configure.Invoke(options, provider);
        });

        return builder;
    }

    /// <summary>
    ///     为 <see cref="HttpClient" /> 启用性能优化
    /// </summary>
    /// <param name="httpClient">
    ///     <see cref="HttpClient" />
    /// </param>
    public static void PerformanceOptimization(this HttpClient httpClient)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpClient);

        // 设置 Accept 头，表示可以接受任何类型的内容
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        // 添加 Accept-Encoding 头，支持 gzip、deflate 以及 Brotli 压缩算法
        // 这样服务器可以根据情况选择最合适的压缩方式发送响应，从而减少传输的数据量
        httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

        // 设置 Connection 头为 keep-alive，允许重用 TCP 连接，避免每次请求都重新建立连接带来的开销
        httpClient.DefaultRequestHeaders.ConnectionClose = false;
    }

    /// <summary>
    ///     分析 <see cref="HttpRequestMessage" /> 标头
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="httpClient">
    ///     <see cref="HttpClient" />
    /// </param>
    /// <param name="summary">摘要</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string? ProfilerHeaders(this HttpRequestMessage httpRequestMessage, HttpClient? httpClient = null,
        string? summary = "Request Headers") =>
        StringUtility.FormatKeyValuesSummary(
            (httpClient?.DefaultRequestHeaders).ConcatIgnoreNull(httpRequestMessage.Headers)
            .ConcatIgnoreNull(httpRequestMessage.Content?.Headers), summary);

    /// <summary>
    ///     分析 <see cref="HttpResponseMessage" /> 标头
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="summary">摘要</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string? ProfilerHeaders(this HttpResponseMessage httpResponseMessage,
        string? summary = "Response Headers") =>
        StringUtility.FormatKeyValuesSummary(
            httpResponseMessage.Headers.ConcatIgnoreNull(httpResponseMessage.Content.Headers),
            summary);

    /// <summary>
    ///     分析常规和 <see cref="HttpResponseMessage" /> 标头
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="responseSummary">响应标头摘要</param>
    /// <param name="generalSummary">常规摘要</param>
    /// <param name="generalCustomKeyValues">自定义常规摘要键值集合</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string ProfilerGeneralAndHeaders(this HttpResponseMessage httpResponseMessage,
        string? responseSummary = "Response Headers", string? generalSummary = "General",
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? generalCustomKeyValues = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 获取 HttpRequestMessage 实例
        var httpRequestMessage = httpResponseMessage.RequestMessage;

        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestMessage);

        // 获取 HttpContent 实例
        var httpContent = httpRequestMessage.Content;

        // 格式化 HTTP 声明式条目
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? declarativeKeyValues =
            httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<string>(Constants.DECLARATIVE_METHOD_KEY),
                out var methodSignature)
                ? [new KeyValuePair<string, IEnumerable<string>>("Declarative", [methodSignature])]
                : null;

        // 根据 HTTP 响应消息解析出 HttpClient 实例的配置名称
        var httpClientName = httpResponseMessage.ResolveHttpClientName();
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? httpClientKeyValues = httpClientName is not null
            ? [new KeyValuePair<string, IEnumerable<string>>("HttpClient Name", [httpClientName])]
            : null;

        // 格式化常规条目
        var generalEntry = StringUtility.FormatKeyValuesSummary(new[]
            {
                new KeyValuePair<string, IEnumerable<string>>("Request URL",
                    [httpRequestMessage.RequestUri?.OriginalString!]),
                new KeyValuePair<string, IEnumerable<string>>("HTTP Method", [httpRequestMessage.Method.ToString()]),
                new KeyValuePair<string, IEnumerable<string>>("Status Code",
                [
                    httpResponseMessage.GetColoredText(
                        $"{(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode}")
                ]),
                new KeyValuePair<string, IEnumerable<string>>("HTTP Version", [httpResponseMessage.Version.ToString()]),
                new KeyValuePair<string, IEnumerable<string>>("HTTP Content",
                    [$"{httpContent?.GetType().Name}"])
            }.ConcatIgnoreNull(httpClientKeyValues).ConcatIgnoreNull(declarativeKeyValues)
            .ConcatIgnoreNull(generalCustomKeyValues), generalSummary);

        // 格式化响应条目
        var responseEntry = httpResponseMessage.ProfilerHeaders(responseSummary);

        return Helpers.JoinNonEmptyLines(generalEntry, responseEntry);
    }

    /// <summary>
    ///     分析 <see cref="HttpContent" /> 内容
    /// </summary>
    /// <remarks>建议在生产环境中禁用或关闭。</remarks>
    /// <param name="httpContent">
    ///     <see cref="HttpContent" />
    /// </param>
    /// <param name="summary">摘要</param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static async Task<string?> ProfilerAsync(this HttpContent? httpContent, string? summary = "Request Body",
        HttpResponseMessage? httpResponseMessage = null, CancellationToken cancellationToken = default)
    {
        // 空检查
        if (httpContent is null)
        {
            return null;
        }

        // 修复无效的响应内容字符编码
        httpContent.FixInvalidCharset();

        // 新增最大处理大小限制，避免内存溢出（OOM）或缓冲区溢出
        const long maxAllowedSize = 5 * 1024 * 1024; // 5MB

        // 检查内容是否包含 Content-Length 标头
        if (httpContent.Headers.ContentLength.HasValue)
        {
            // 获取内容长度
            var contentLength = httpContent.Headers.ContentLength.Value;

            // 检查内容长度是否大于 maxAllowedSize
            if (contentLength > maxAllowedSize)
            {
                return StringUtility.FormatKeyValuesSummary(
                    [
                        new KeyValuePair<string, IEnumerable<string>>(string.Empty,
                            [$"\e[33m[Skipped: content too large ({contentLength} bytes) > {maxAllowedSize}]\e[0m"])
                    ],
                    $"{summary} ({httpContent.GetType().Name}, total: {contentLength} bytes)");
            }
        }

        try
        {
            // 尝试将 HttpContent 缓冲到内存，但限制最大大小以防止内存溢出（OOM）
#if NET8_0
            await httpContent.LoadIntoBufferAsync(maxAllowedSize);
#else
        await httpContent.LoadIntoBufferAsync(maxAllowedSize, cancellationToken);
#endif
        }
        catch
        {
            return StringUtility.FormatKeyValuesSummary(
                [
                    new KeyValuePair<string, IEnumerable<string>>(string.Empty,
                        [$"\e[33m[Skipped: content unreadable or exceeds {maxAllowedSize} bytes]\e[0m"])
                ], $"{summary} ({httpContent.GetType().Name}, Skipped due to size)");
        }

        // 获取已缓冲的内部流
        var stream = await httpContent.ReadAsStreamAsync(cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);

        // 默认只读取 8KB 的内容
        const int maxBytesToDisplay = 8 * 1024;

        // 获取响应内容 Content-Encoding 标头
        string? contentEncoding = null;

        // 空检查
        if (httpResponseMessage is not null)
        {
            contentEncoding = httpResponseMessage.Content.Headers.ContentEncoding.FirstOrDefault();
        }

        // 从流中按需解压并读取前 (maxBytesToDisplay + 1) 字节，用于判断是否发生截断
        var (partialBuffer, totalRead, isTruncated) =
            await ReadAndDecompressFirstBytesAsync(stream, contentEncoding, maxBytesToDisplay + 1, cancellationToken);

        // 重置流位置
        stream.Seek(0, SeekOrigin.Begin);

        // 注册 CodePagesEncodingProvider，使得程序能够识别并使用 Windows 代码页中的各种编码
        EncodingUtility.Initialize();

        // 获取内容编码
        var charset = httpContent.Headers.ContentType?.CharSet ?? "utf-8";
        var bytesToShow = isTruncated ? maxBytesToDisplay : totalRead;
        var partialContent = Encoding.GetEncoding(charset).GetString(partialBuffer, 0, bytesToShow);

        // 解决退格导致显示不全问题：保留 \n 和 \r，仅过滤其他 ASCII 控制字符（ASCII < 32 且不是 \n 或 \r）
        partialContent = new string(partialContent
            .Where(c => c >= 32 || c == '\n' || c == '\r')
            .ToArray());

        // 检查是否是完整的 Unicode 转义字符串
        if (!isTruncated && UnicodeEscapeRegex().IsMatch(partialContent))
        {
            partialContent = Regex.Unescape(partialContent);
        }

        // 空检查
        if (httpResponseMessage is not null)
        {
            // 对响应内容进行着色
            partialContent = httpResponseMessage.GetColoredText(partialContent, false);
        }

        // 如果未发生截断，则直接返回；否则，添加省略号表示内容被截断
        var bodyString = !isTruncated
            ? partialContent
            : partialContent + $"\e[36m\e[1m ... [truncated, > {maxBytesToDisplay} bytes]\e[0m";

        return StringUtility.FormatKeyValuesSummary(
            [new KeyValuePair<string, IEnumerable<string>>(string.Empty, [bodyString])],
            $"{summary} ({httpContent.GetType().Name}, total: {(isTruncated ? $"> {maxBytesToDisplay}" : totalRead.ToString())} bytes)");
    }

    /// <summary>
    ///     从流中读取最多指定数量的解压后字节
    /// </summary>
    /// <param name="compressedStream">压缩数据流</param>
    /// <param name="contentEncoding">内容编码（gzip, deflate, br 等）</param>
    /// <param name="maxBytes">最多读取的字节数（解压后）</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Tuple{T1,T2,T3}" />：包含解压后字节数组、实际读取字节数以及是否截断的值元组
    /// </returns>
    internal static async Task<(byte[] buffer, int totalRead, bool isTruncated)> ReadAndDecompressFirstBytesAsync(
        Stream compressedStream, string? contentEncoding, int maxBytes, CancellationToken cancellationToken)
    {
        Stream? decompressor = null;

        // 空检查
        if (!string.IsNullOrWhiteSpace(contentEncoding))
        {
            // 检查是否不是 WebAssembly 应用
            if (!OperatingSystem.IsBrowser())
            {
                decompressor = contentEncoding.Trim().ToLowerInvariant() switch
                {
                    "gzip" => new GZipStream(compressedStream, CompressionMode.Decompress, true),
                    "deflate" => new DeflateStream(compressedStream, CompressionMode.Decompress, true),
                    "br" => new BrotliStream(compressedStream, CompressionMode.Decompress, true),
                    _ => null
                };
            }
        }

        var readStream = decompressor ?? compressedStream;
        try
        {
            // 初始化最大读取字节数的缓冲区
            var buffer = new byte[maxBytes];
            var totalRead = 0;

            // 循环读取直到填满缓冲区或流已结束
            while (totalRead < buffer.Length)
            {
                // 从流中读取数据并写入缓冲区
                var read = await readStream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead),
                    cancellationToken);

                // 检查流结束
                if (read == 0)
                {
                    break;
                }

                totalRead += read;
            }

            // 读满了缓冲区，说明可能还有更多数据
            var isTruncated = totalRead == maxBytes;
            // 裁剪缓冲区，只返回实际读取的字节
            var resultBuffer = totalRead == 0 ? [] : buffer[..totalRead];

            return (resultBuffer, totalRead, isTruncated);
        }
        finally
        {
            // 空检查
            if (decompressor is not null)
            {
                // 确保解压流被正确释放
                await decompressor.DisposeAsync();
            }
        }
    }

    /// <summary>
    ///     获取带颜色的文本
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="text">文本</param>
    /// <param name="bold">是否加粗显示</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GetColoredText(this HttpResponseMessage httpResponseMessage, string? text, bool bold = true)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 初始化 StringBuilder 实例
        var stringBuilder = new StringBuilder();

        // 检查是否是成功请求状态码
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            // 输出绿色内容
            stringBuilder.Append("\e[32m");
        }
        else
        {
            // 检查是否是重定向状态码
            stringBuilder.Append(httpResponseMessage.StatusCode is HttpStatusCode.Ambiguous or HttpStatusCode.Moved
                or HttpStatusCode.Redirect
                or HttpStatusCode.RedirectMethod
                // 输出黄色内容
                ? "\e[33m"
                // 输出红色内容
                : "\e[31m");
        }

        // 加粗处理
        if (bold)
        {
            stringBuilder.Append("\e[1m");
        }

        // 追加完整内容
        stringBuilder.Append($"{text}\e[0m");

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     克隆 <see cref="HttpRequestMessage" />
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestMessage" />
    /// </returns>
    public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestMessage);

        // 初始化克隆的 HttpRequestMessage 实例
        var clonedHttpRequestMessage = new HttpRequestMessage(httpRequestMessage.Method, httpRequestMessage.RequestUri);

        // 复制请求标头
        foreach (var header in httpRequestMessage.Headers)
        {
            clonedHttpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // 复制 httpRequestMessage.Options 选项
        foreach (var (key, value) in httpRequestMessage.Options)
        {
            clonedHttpRequestMessage.Options.TryAdd(key, value); // TODO: 思考是 TryAdd 还是 AddOrUpdate
        }

        // 检查是否包含请求内容
        if (httpRequestMessage.Content is null)
        {
            return clonedHttpRequestMessage;
        }

        // 复制请求内容
        var memoryStream = new MemoryStream();
        await httpRequestMessage.Content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        // 设置请求内容
        clonedHttpRequestMessage.Content = new StreamContent(memoryStream);

        // 复制请求内容标头
        foreach (var header in httpRequestMessage.Content.Headers)
        {
            clonedHttpRequestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clonedHttpRequestMessage;
    }

    /// <summary>
    ///     克隆 <see cref="HttpRequestMessage" />
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestMessage" />
    /// </returns>
    public static HttpRequestMessage Clone(this HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken = default) =>
        AsyncUtility.RunSync(() => httpRequestMessage.CloneAsync(cancellationToken));

    /// <summary>
    ///     尝试获取响应标头 <c>Set-Cookie</c> 集合
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="setCookies">响应标头 <c>Set-Cookie</c> 集合</param>
    /// <param name="rawSetCookies">原始响应标头 <c>Set-Cookie</c> 集合</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool TryGetSetCookies(this HttpResponseMessage httpResponseMessage,
        [NotNullWhen(true)] out IList<SetCookieHeaderValue>? setCookies,
        [NotNullWhen(true)] out List<string>? rawSetCookies)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        return httpResponseMessage.Headers.TryGetSetCookies(out setCookies, out rawSetCookies);
    }

    /// <summary>
    ///     修复无效的响应内容字符编码
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    public static void FixInvalidCharset(this HttpResponseMessage? httpResponseMessage) =>
        httpResponseMessage?.Content.FixInvalidCharset();

    /// <summary>
    ///     修复无效的响应内容字符编码
    /// </summary>
    /// <param name="httpContent">
    ///     <see cref="HttpContent" />
    /// </param>
    public static void FixInvalidCharset(this HttpContent? httpContent)
    {
        // 空检查
        if (httpContent?.Headers.ContentType?.CharSet is null)
        {
            return;
        }

        // 获取内容字符编码
        var charset = httpContent.Headers.ContentType.CharSet.Trim();

        // 修复 "utf8"、"utf 8" 和 "utf-8;" 的错误写法（不区分大小写/空格）
        if (charset.Equals("utf8", StringComparison.OrdinalIgnoreCase) ||
            charset.Equals("utf 8", StringComparison.OrdinalIgnoreCase) ||
            charset.Equals("utf-8;", StringComparison.OrdinalIgnoreCase))
        {
            httpContent.Headers.ContentType.CharSet = "utf-8";
        }
    }

    /// <summary>
    ///     尝试获取响应标头 <c>Set-Cookie</c> 集合
    /// </summary>
    /// <param name="responseHeaders">
    ///     <see cref="HttpResponseHeaders" />
    /// </param>
    /// <param name="setCookies">响应标头 <c>Set-Cookie</c> 集合</param>
    /// <param name="rawSetCookies">原始响应标头 <c>Set-Cookie</c> 集合</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool TryGetSetCookies(this HttpResponseHeaders responseHeaders,
        [NotNullWhen(true)] out IList<SetCookieHeaderValue>? setCookies,
        [NotNullWhen(true)] out List<string>? rawSetCookies)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(responseHeaders);

        // 检查响应标头是否包含 Set-Cookie 设置
        if (!responseHeaders.TryGetValues(HeaderNames.SetCookie, out var setCookieValues))
        {
            setCookies = null;
            rawSetCookies = null;

            return false;
        }

        rawSetCookies = setCookieValues.ToList();
        setCookies = SetCookieHeaderValue.ParseList(rawSetCookies);

        return true;
    }

    /// <summary>
    ///     检查 HTTP 响应的内容类型是否为 XML 媒体类型
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool IsXmlContent(this HttpResponseMessage httpResponseMessage) =>
        httpResponseMessage.Content.Headers.ContentType?.MediaType.IsIn(
            [MediaTypeNames.Application.Xml, MediaTypeNames.Application.XmlPatch, MediaTypeNames.Text.Xml],
            StringComparer.OrdinalIgnoreCase) == true;

    /// <summary>
    ///     根据 HTTP 响应消息解析出 <see cref="HttpClient" /> 实例的配置名称
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string? ResolveHttpClientName(this HttpResponseMessage? httpResponseMessage) =>
        httpResponseMessage?.RequestMessage?.Options.TryGetValue(
            new HttpRequestOptionsKey<string>(Constants.HTTP_CLIENT_NAME), out var httpClientName) != true
            ? null
            : httpClientName;

    /// <summary>
    ///     将对象转换为 JSON 字符串
    /// </summary>
    /// <param name="obj">
    ///     <see cref="object" />
    /// </param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string ToJsonString(this object? obj, JsonSerializerOptions? jsonSerializerOptions = null) =>
        JsonSerializer.Serialize(obj, jsonSerializerOptions ?? HttpRemoteOptions.JsonSerializerOptionsDefault);

    /// <summary>
    ///     检查是否启用 JSON 响应反序列化包装器
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool ShouldUseJsonResponseWrapper(this HttpResponseMessage? httpResponseMessage,
        IServiceProvider? serviceProvider)
    {
        // 检查是否局部启用或禁用 JSON 响应反序列化包装器
        if (httpResponseMessage?.RequestMessage?.Options.TryGetValue(
                new HttpRequestOptionsKey<string>(Constants.ENABLE_JSON_RESPONSE_WRAPPER_KEY), out var enableValue) ==
            true)
        {
            return enableValue == "TRUE";
        }

        // 否则使用全局配置
        return HttpRemoteUtility.ResolveHttpClientOptions(httpResponseMessage, serviceProvider)
            ?.UseJsonResponseWrapper == true;
    }

    /// <summary>
    ///     检查是否启用 JSON 响应内容字符串的解包处理（双重序列化）
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool ShouldJsonResponseStringUnwrap(this HttpResponseMessage? httpResponseMessage)
    {
        // 检查是否启用或禁用 JSON 响应反序列化包装器
        if (httpResponseMessage?.RequestMessage?.Options.TryGetValue(
                new HttpRequestOptionsKey<string>(Constants.ENABLE_JSON_RESPONSE_STRING_UNWRAP_KEY),
                out var enableValue) ==
            true)
        {
            return enableValue == "TRUE";
        }

        return false;
    }

    /// <summary>
    ///     解析经过双重序列化的 JSON 字符串，并将其反序列化为指定类型
    /// </summary>
    /// <param name="httpContent">
    ///     <see cref="HttpContent" />
    /// </param>
    /// <param name="resultType">目标类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    internal static async Task<object?> ReadAndUnwrapFromJsonAsync(this HttpContent httpContent, Type resultType,
        JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        // 读取响应内容字符串
        var responseString = await httpContent.ReadAsStringAsync(cancellationToken);

        // 处理双重序列化问题
        var innerJson = JsonSerializer.Deserialize<string>(responseString);

        return innerJson is null
            ? null
            : JsonSerializer.Deserialize(innerJson, resultType, jsonSerializerOptions);
    }

    /// <summary>
    ///     获取主机环境名
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? GetHostEnvironmentName(IServiceCollection services)
    {
        // 获取主机环境对象
        var hostEnvironment = services
            .FirstOrDefault(u => u.ServiceType.FullName == "Microsoft.Extensions.Hosting.IHostEnvironment")
            ?.ImplementationInstance;

        // 空检查
        return hostEnvironment is null
            ? null
            : Convert.ToString(hostEnvironment.GetType().GetProperty("EnvironmentName")?.GetValue(hostEnvironment));
    }

    /// <summary>
    ///     Unicode 转义正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(@"\\u([0-9a-fA-F]{4})")]
    private static partial Regex UnicodeEscapeRegex();
}