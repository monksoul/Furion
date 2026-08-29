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
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;

namespace Furion.HttpRemote.Extensions;

/// <summary>
///     HTTP 远程服务模块扩展类
/// </summary>
public static partial class HttpRemoteExtensions
{
    /// <summary>
    ///     <see cref="StreamContent" /> 内部流字段缓存
    /// </summary>
    /// <remarks>用于请求分析工具反射读取内部流。</remarks>
    internal static readonly FieldInfo[] StreamContentInternalFields =
        typeof(StreamContent).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

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
    /// <param name="disableInProduction">是否在生产环境中禁用。默认值为：<c>false</c></param>
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
    /// <exception cref="ArgumentNullException"></exception>
    public static IHttpClientBuilder ConfigureOptions(this IHttpClientBuilder builder,
        Action<HttpClientOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        return builder.ConfigureOptions((options, _) => configure(options));
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
            // 获取 HttpRemoteOptions 的 JsonSerializerOptions 选项
            options.JsonSerializerOptions = new JsonSerializerOptions(
                provider.GetRequiredService<IOptionsMonitor<HttpRemoteOptions>>().CurrentValue.JsonSerializerOptions ??
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                HttpRemoteOptions.JsonSerializerOptionsDefault);

            configure.Invoke(options, provider);
        });

        return builder;
    }

    /// <summary>
    ///     为 <see cref="HttpClient" /> 启用标准请求标头
    /// </summary>
    /// <param name="httpClient">
    ///     <see cref="HttpClient" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void UseStandardRequestHeaders(this HttpClient httpClient)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpClient);

        // 获取默认请求标头
        var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

        // 设置 Accept 头 (避免被 WAF 拦截)
        if (defaultRequestHeaders.Accept.Count == 0)
        {
            defaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            defaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain", 0.9));
            defaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
        }

        // 显式声明 Keep-Alive
        defaultRequestHeaders.ConnectionClose = false;
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

        // 检查是否从（ETag）内存缓存返回
        var fromMemoryCache =
            httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<bool>(Constants.ETAG_CACHED_KEY),
                out var value) && value;

        // 获取原始 cURL 命令
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? curlKeyValues =
            httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<string>(Constants.CURL_COMMAND_KEY),
                out var curlCommand)
                ? [new KeyValuePair<string, IEnumerable<string>>("cURL Command", [$"\e[36m\e[3m{curlCommand}\e[0m"])]
                : null;

        // 获取原始 JSON 命令
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? jsonKeyValues =
            httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<string>(Constants.JSON_COMMAND_KEY),
                out var jsonCommand)
                ? [new KeyValuePair<string, IEnumerable<string>>("JSON Command", [$"\e[36m\e[3m{jsonCommand}\e[0m"])]
                : null;

        // 格式化常规条目
        var generalEntry = StringUtility.FormatKeyValuesSummary(new[]
                {
                    new KeyValuePair<string, IEnumerable<string>>("Request URL",
                        [httpRequestMessage.RequestUri?.OriginalString!]),
                    new KeyValuePair<string, IEnumerable<string>>("Request Method",
                        [httpRequestMessage.Method.ToString()]),
                    new KeyValuePair<string, IEnumerable<string>>("Status Code",
                    [
                        httpResponseMessage.GetColoredText(
                            $"{(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode}") +
                        (!fromMemoryCache ? string.Empty : " \e[90m(from memory cache)\e[0m")
                    ]),
                    new KeyValuePair<string, IEnumerable<string>>("HTTP Version",
                        [httpResponseMessage.Version.ToString()]),
                    new KeyValuePair<string, IEnumerable<string>>("HTTP Content", [$"{httpContent?.GetType().Name}"]),
                    new KeyValuePair<string, IEnumerable<string>>("Content Type",
                        [$"{httpContent?.Headers.ContentType}"])
                }.ConcatIgnoreNull(httpClientKeyValues).ConcatIgnoreNull(declarativeKeyValues)
                .ConcatIgnoreNull(curlKeyValues).ConcatIgnoreNull(jsonKeyValues)
                .ConcatIgnoreNull(generalCustomKeyValues),
            generalSummary, true);

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
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<string?> ProfilerAsync(this HttpContent? httpContent, string? summary = "Request Body",
        HttpResponseMessage? httpResponseMessage = null, HttpRequestMessage? httpRequestMessage = null,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        if (httpContent is null)
        {
            return null;
        }

        // 修复无效的响应内容字符编码
        httpContent.FixInvalidCharset();

        // 最大处理大小限制，避免内存溢出（OOM）或缓冲区溢出
        const long maxAllowedSize = 5 * 1024 * 1024; // 5MB

        // 默认只读取 5KB 的内容
        const int maxBytesToDisplay = 5 * 1024; // 5KB

        // 判断当前内容来自请求还是响应
        var isResponse = httpResponseMessage is not null;

        // 获取内容类型
        var contentType = httpContent.Headers.ContentType?.ToString();

        // 初始化 只进流无法回退且缓冲会破坏请求 的原因常量
        const string skipReasonForwardOnly = "Forward-only stream, reading would break request";

        // 判断内容是否来自请求中的 MultipartContent
        // 主要用于解决 MultipartContent.LoadIntoBufferAsync 会递归消费所有 Part 的底层流，导致流指针移到末尾（损坏）
        var isMultipartRequest = !isResponse && httpContent is MultipartContent;

        if (!isMultipartRequest)
        {
            // 检查内容大小是否最大限制
            if (httpContent.Headers.ContentLength is > maxAllowedSize)
            {
                return StringUtility.FormatKeyValuesSummary(
                    [
                        new KeyValuePair<string, IEnumerable<string>>(string.Empty,
                        [
                            $"\e[36m\e[1m[Skipped: content too large ({httpContent.Headers.ContentLength.Value} bytes) > {maxAllowedSize}]\e[0m"
                        ])
                    ],
                    $"{summary} ({httpContent.GetType().Name}, total: {httpContent.Headers.ContentLength.Value} bytes)");
            }

            // 处理请求内容是 StreamContent 情况，尝试反射获取内部流
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (!isResponse && httpContent is StreamContent)
            {
                Stream? topInnerStream = null;

                // 遍历查找内部流
                foreach (var field in StreamContentInternalFields)
                {
                    if (!typeof(Stream).IsAssignableFrom(field.FieldType))
                    {
                        continue;
                    }

                    // 获取内部流的值
                    topInnerStream = field.GetValue(httpContent) as Stream;

                    // 空检查
                    if (topInnerStream is not null)
                    {
                        break;
                    }
                }

                // 处理流不可读的情况
                if (topInnerStream is not { CanSeek: true })
                {
                    return StringUtility.FormatKeyValuesSummary(
                        [
                            new KeyValuePair<string, IEnumerable<string>>(string.Empty,
                            [
                                $"\e[36m\e[1m[Skipped: {skipReasonForwardOnly}]\e[0m"
                            ])
                        ], $"{summary} ({httpContent.GetType().Name}, Skipped: forward-only stream)");
                }
            }

            // 处理流式内容
            if (isResponse &&
                httpResponseMessage?.RequestMessage?.Options.TryGetValue(
                    new HttpRequestOptionsKey<HttpCompletionOption>(Constants.HTTP_COMPLETION_OPTION_KEY),
                    out var completionOption) == true && completionOption == HttpCompletionOption.ResponseHeadersRead)
            {
                return StringUtility.FormatKeyValuesSummary(
                    [
                        new KeyValuePair<string, IEnumerable<string>>(string.Empty,
                        [
                            "\e[36m\e[1m[Skipped: ResponseHeadersRead mode, content not buffered]\e[0m"
                        ])
                    ], $"{summary} ({httpContent.GetType().Name}, Skipped: ResponseHeadersRead)");
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
                // 一旦发生异常，流将变为不可读状态，之后所有读取操作均会失败，所以这种情况应该禁用请求分析工具
                throw new InvalidOperationException(
                    $"The {(isResponse ? "response body" : "request body")} for request '{(httpRequestMessage ?? httpResponseMessage?.RequestMessage)?.RequestUri?.OriginalString}' exceeds the maximum allowed size of 5 MB for profiling and cannot be printed. To resolve this, disable profiling by calling `HttpRequestBuilder.Profiler(false)`, applying the `[Profiler(false)]` attribute to declarative requests, or removing the global `.AddProfilerDelegatingHandler()` registration.");
            }
        }

        // 初始化最终显示的内容和实际大小
        string finalBody;
        string totalSizeInfo;

        // 处理 MultipartContent 内容
        if (httpContent is MultipartContent multipartContent)
        {
            string? boundary = null;

            // 尝试解析 boundary 参数
            if (!string.IsNullOrWhiteSpace(contentType) &&
                MediaTypeHeaderValue.TryParse(contentType, out var parsedContentType))
            {
                // 移除前后双引号
                boundary = parsedContentType.Boundary.Value?.TrimStart('"').TrimEnd('"');
            }

            // 如果解析失败，尝试直接从内容类型中获取原值
            if (string.IsNullOrEmpty(boundary))
            {
                var boundaryParam = httpContent.Headers.ContentType?.Parameters.FirstOrDefault(p =>
                    string.Equals(p.Name, "boundary", StringComparison.OrdinalIgnoreCase));

                // 移除前后双引号
                boundary = boundaryParam?.Value?.TrimStart('"').TrimEnd('"');
            }

            // 构建最终用于输出的 boundary 字符串
            var boundaryOutput = string.IsNullOrEmpty(boundary)
                ? "\e[36m\e[1m[Warning: Missing boundary in Content-Type]\e[0m"
                : $"\e[90m{boundary}\e[0m";

            // 初始化 StringBuilder 实例
            var stringBuilder = new StringBuilder();
            long totalMultipartSize = 0;

            // 遍历 Multipart 的每一个 Part
            foreach (var part in multipartContent)
            {
                // 追加 boundary
                stringBuilder.AppendLine(boundaryOutput);

                Stream? innerStream = null;
                string? skipReason = null;

                // 处理请求内容是 StreamContent 情况，尝试反射获取内部流
                if (part is StreamContent streamContent)
                {
                    try
                    {
                        // 遍历查找内部流
                        foreach (var field in StreamContentInternalFields)
                        {
                            if (!typeof(Stream).IsAssignableFrom(field.FieldType))
                            {
                                continue;
                            }

                            // 获取内部流的值
                            innerStream = field.GetValue(streamContent) as Stream;

                            // 空检查
                            if (innerStream is not null)
                            {
                                break;
                            }
                        }
                    }
                    catch
                    {
                        innerStream = null;
                    }

                    // 空检查
                    if (innerStream == null)
                    {
                        skipReason = "Unable to access internal stream of StreamContent";
                    }
                    // 检查流是否可读
                    else if (!innerStream.CanSeek)
                    {
                        skipReason = skipReasonForwardOnly;
                    }
                }

                // 遍历并追加内容头
                foreach (var header in part.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }

                // 追加空行
                stringBuilder.AppendLine();

                // 空检查
                if (skipReason is not null)
                {
                    stringBuilder.AppendLine($"\e[36m\e[1m[Skipped: {skipReason}]\e[0m");
                    totalMultipartSize += part.Headers.ContentLength ?? 0;

                    continue;
                }

                // 空检查
                if (innerStream is not null)
                {
                    // 初始化可读流原始位置
                    // ReSharper disable once RedundantAssignment
                    var originalPosition = 0L;

                    try
                    {
                        originalPosition = innerStream.Position;
                        innerStream.Position = 0;
                    }
                    catch
                    {
                        stringBuilder.AppendLine(
                            "\e[36m\e[1m[Skipped: Unable to determine or reset stream position]\e[0m");
                        totalMultipartSize += part.Headers.ContentLength ?? 0;

                        continue;
                    }

                    try
                    {
                        // 获取流内容编码
                        var streamPartEncoding = part.Headers.ContentEncoding.FirstOrDefault();

                        // 从流中按需解压并读取前 (maxBytesToDisplay + 1) 字节，用于判断是否发生截断
                        var (partialBuffer, totalRead, isTruncated) =
                            await ReadAndDecompressFirstBytesAsync(innerStream, streamPartEncoding,
                                maxBytesToDisplay + 1, cancellationToken);

                        // 计算实际需要显示的字节数和获取内容字符编码
                        var bytesToShow = isTruncated ? maxBytesToDisplay : totalRead;
                        var charset = part.Headers.ContentType?.CharSet;

                        // 将字节数组格式化为可读文本或 Hex Dump
                        var bodyString = FormatBytes(partialBuffer, bytesToShow, maxBytesToDisplay, isTruncated,
                            totalRead, isResponse, httpResponseMessage, charset);

                        stringBuilder.AppendLine(bodyString);
                    }
                    catch (Exception ex)
                    {
                        stringBuilder.AppendLine(
                            $"\e[36m\e[1m[Skipped: Failed to read stream content - {ex.Message}]\e[0m");
                    }
                    finally
                    {
                        // 恢复可读流的原始位置
                        try
                        {
                            innerStream.Position = originalPosition;
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    // 获取当前内容大小
                    var partSize = part.Headers.ContentLength ?? 0;

                    // 空检查
                    if (partSize == 0)
                    {
                        try
                        {
                            // 检查流是否可读
                            if (innerStream.CanSeek)
                            {
                                partSize = innerStream.Length;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    totalMultipartSize += partSize;

                    continue;
                }

                // 处理非 StreamContent 内容
                try
                {
                    // 尝试将 HttpContent 缓冲到内存，但限制最大大小以防止内存溢出（OOM）
#if NET8_0
                    await part.LoadIntoBufferAsync(maxAllowedSize);
#else
                    await part.LoadIntoBufferAsync(maxAllowedSize, cancellationToken);
#endif
                }
                catch
                {
                    stringBuilder.AppendLine("\e[36m\e[1m[Skipped: part unreadable or exceeds size limit]\e[0m");

                    continue;
                }

                try
                {
                    // 获取内容编码
                    var partEncoding = part.Headers.ContentEncoding.FirstOrDefault();

                    // 从流中按需解压并读取前 (maxBytesToDisplay + 1) 字节，用于判断是否发生截断
                    var (partBody, partRead, _) = await FormatContentBodyAsync(part, maxBytesToDisplay, isResponse,
                        partEncoding, httpResponseMessage, cancellationToken);

                    stringBuilder.AppendLine(partBody);
                    totalMultipartSize += partRead;
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendLine(
                        $"\e[36m\e[1m[Skipped: Failed to format part content - {ex.Message}]\e[0m");
                }
            }

            // 移除末尾换行
            finalBody = stringBuilder.ToString().TrimEnd('\r', '\n');

            // 处理 304 Not Modified 状态码且响应内容为空的情况
            if (isResponse && httpResponseMessage!.StatusCode == HttpStatusCode.NotModified &&
                totalMultipartSize == 0 && string.IsNullOrWhiteSpace(finalBody))
            {
                finalBody =
                    $"\e[36m\e[1m[Empty: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode}, no content returned by server]\e[0m";
            }

            totalSizeInfo = totalMultipartSize.ToString();
        }
        // 处理非 MultipartContent 内容
        else
        {
            // 获取响应内容 Content-Encoding 标头
            string? contentEncoding = null;

            // 检查是否是响应内容
            if (isResponse)
            {
                contentEncoding = httpResponseMessage!.Content.Headers.ContentEncoding.FirstOrDefault();
            }

            try
            {
                // 从流中按需解压并读取前 (maxBytesToDisplay + 1) 字节，用于判断是否发生截断
                var (body, totalRead, isTruncated) = await FormatContentBodyAsync(httpContent, maxBytesToDisplay,
                    isResponse, contentEncoding, httpResponseMessage, cancellationToken);

                // 处理 304 Not Modified 状态码且响应内容为空的情况
                if (isResponse && httpResponseMessage!.StatusCode == HttpStatusCode.NotModified && totalRead == 0 &&
                    string.IsNullOrWhiteSpace(body))
                {
                    body =
                        $"\e[36m\e[1m[Empty: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode}, no content returned by server]\e[0m";
                }

                finalBody = body;
                totalSizeInfo = isTruncated ? $"> {maxBytesToDisplay}" : totalRead.ToString();
            }
            catch (Exception ex)
            {
                finalBody = $"\e[36m\e[1m[Skipped: Failed to format content - {ex.Message}]\e[0m";
                totalSizeInfo = "0";
            }
        }

        return StringUtility.FormatKeyValuesSummary(
            [new KeyValuePair<string, IEnumerable<string>>(string.Empty, [finalBody])],
            $"{summary} ({httpContent.GetType().Name}, total: {totalSizeInfo} bytes)");
    }

    /// <summary>
    ///     将字节数组格式化为可读文本或 Hex Dump
    /// </summary>
    /// <param name="buffer">字节缓冲区</param>
    /// <param name="bytesToShow">实际显示的字节数</param>
    /// <param name="maxBytesLimit">配置的最大预览字节数</param>
    /// <param name="isTruncated">是否被截断</param>
    /// <param name="totalRead">实际读取的总字节数</param>
    /// <param name="isResponse">是否为响应内容</param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="charset">内容字符编码</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string FormatBytes(byte[] buffer, int bytesToShow, int maxBytesLimit, bool isTruncated,
        int totalRead, bool isResponse, HttpResponseMessage? httpResponseMessage, string? charset = null)
    {
        // 空检查
        if (bytesToShow == 0)
        {
            return string.Empty;
        }

        var isBinary = false;
        var nonPrintableCount = 0;

        // 实现二进制检测
        // 如果包含 0x00 (Null 字符)，或者非打印控制字符比例超过 10%，则判定为二进制文件
        for (var i = 0; i < bytesToShow; i++)
        {
            var b = buffer[i];
            if (b == 0)
            {
                isBinary = true;
                break;
            }

            if (b < 32 && b != 9 && b != 10 && b != 13)
            {
                nonPrintableCount++;
            }
        }

        if (!isBinary && nonPrintableCount > bytesToShow / 10)
        {
            isBinary = true;
        }

        // 二进制内容处理
        if (isBinary)
        {
            // 限制 Hex Dump 只打印 0.5KB 内容
            const int maxHexDumpBytes = 512; // 0.5KB
            var hexBytesToShow = Math.Min(bytesToShow, maxHexDumpBytes);

            // 生成 Hex Dump 格式内容
            var bodyString = GetHexDump(buffer, hexBytesToShow);

            // 如果内容被截断，或者实际大小超过了 Hex 显示限制，则追加提示信息
            if (isTruncated || bytesToShow > maxHexDumpBytes)
            {
                bodyString +=
                    $"\e[36m\e[1m... [Binary content, showing first {hexBytesToShow} bytes of {totalRead} total bytes]\e[0m";
            }

            return bodyString.TrimEnd('\r', '\n');
        }

        // 注册 CodePagesEncodingProvider，使得程序能够识别并使用 Windows 代码页中的各种编码
        EncodingUtility.Initialize();

        // 获取 Charset 编码，如果无效则回退到 UTF-8
        Encoding encoding;
        try
        {
            encoding = Encoding.GetEncoding(charset ?? "utf-8");
        }
        catch
        {
            encoding = Encoding.UTF8;
        }

        // 将字节数组解码为字符串
        var partialContent = encoding.GetString(buffer, 0, bytesToShow);

        // 过滤掉不可见的 ASCII 控制字符，保留换行、回车和制表符，防止控制台排版错乱
        partialContent =
            new string(partialContent.Where(c => c >= 32 || c == '\n' || c == '\r' || c == '\t').ToArray());

        // 尝试反转义 Unicode 字符 (如 \u003c 转为 <)
        if (!isTruncated && UnicodeEscapeRegex().IsMatch(partialContent))
        {
            try
            {
                partialContent = Regex.Unescape(partialContent);
            }
            catch
            {
                // ignored
            }
        }

        // 如果是响应内容且不为空，则根据状态码进行终端颜色高亮
        if (isResponse && httpResponseMessage is not null)
        {
            partialContent = httpResponseMessage.GetColoredText(partialContent, false);
        }
        else
        {
            partialContent = $"\e[36m{partialContent}\e[0m";
        }

        // 如果内容超长被截断，追加省略号提示
        return !isTruncated
            ? partialContent
            : partialContent + $"\e[36m\e[1m ... [truncated, > {maxBytesLimit} bytes]\e[0m";
    }

    /// <summary>
    ///     格式化 <see cref="HttpContent" />
    /// </summary>
    /// <param name="content">
    ///     <see cref="HttpContent" />
    /// </param>
    /// <param name="maxBytesToDisplay">最大显示字节数</param>
    /// <param name="isResponse">是否为响应内容</param>
    /// <param name="contentEncoding">内容编码（gzip, deflate, br, zstd 等）</param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <para>
    ///         <see cref="Tuple{T1,T2,T3}" />
    ///     </para>
    ///     <para>包含格式化后的内容、实际读取字节数以及是否截断的值元组</para>
    /// </returns>
    internal static async Task<(string Body, int TotalRead, bool IsTruncated)> FormatContentBodyAsync(
        HttpContent content, int maxBytesToDisplay, bool isResponse, string? contentEncoding,
        HttpResponseMessage? httpResponseMessage, CancellationToken cancellationToken)
    {
        // 获取已缓冲的内部流
        var stream = await content.ReadAsStreamAsync(cancellationToken);

        // 检查流是否可读
        if (stream.CanSeek)
        {
            // 重置流指针至起始位置
            stream.Seek(0, SeekOrigin.Begin);
        }

        // 从流中按需解压并读取前 (maxBytesToDisplay + 1) 字节
        var (partialBuffer, totalRead, isTruncated) =
            await ReadAndDecompressFirstBytesAsync(stream, contentEncoding, maxBytesToDisplay + 1, cancellationToken);

        // 检查流是否可读
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        // 计算实际需要显示的字节数和获取内容字符编码
        var bytesToShow = isTruncated ? maxBytesToDisplay : totalRead;
        var charset = content.Headers.ContentType?.CharSet;

        // 将字节数组格式化为可读文本或 Hex Dump
        var bodyString = FormatBytes(partialBuffer, bytesToShow, maxBytesToDisplay, isTruncated, totalRead, isResponse,
            httpResponseMessage, charset);

        return (bodyString, totalRead, isTruncated);
    }

    /// <summary>
    ///     将字节数组格式化为 Hex Dump 格式
    /// </summary>
    /// <param name="buffer">字节数组</param>
    /// <param name="length">需要格式化的长度</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GetHexDump(byte[] buffer, int length)
    {
        // 初始化 StringBuilder 实例
        var stringBuilder = new StringBuilder();

        // 每次处理 16 个字节 (标准 Hex Dump 行宽)
        for (var i = 0; i < length; i += 16)
        {
            // 输出偏移量 (8位十六进制)
            stringBuilder.Append($"{i:X8}  ");

            // 输出十六进制数值
            for (var j = 0; j < 16; j++)
            {
                if (i + j < length)
                {
                    stringBuilder.Append($"{buffer[i + j]:X2} ");
                }
                else
                {
                    // 不足 16 字节时补空格
                    stringBuilder.Append("   ");
                }

                // 8 字节一断
                if (j == 7)
                {
                    stringBuilder.Append(' ');
                }
            }

            // 输出 ASCII 可打印字符
            stringBuilder.Append(" |");

            for (var j = 0; j < 16; j++)
            {
                if (i + j < length)
                {
                    var bytes = buffer[i + j];

                    // 仅显示标准 ASCII 可打印字符，其余显示为 '.'
                    stringBuilder.Append(bytes is >= 32 and <= 126 ? (char)bytes : '.');
                }
                else
                {
                    stringBuilder.Append(' ');
                }
            }

            stringBuilder.AppendLine("|");
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     从流中读取最多指定数量的解压后字节
    /// </summary>
    /// <param name="rawStream">压缩数据流</param>
    /// <param name="contentEncoding">内容编码（gzip, deflate, br, zstd 等）</param>
    /// <param name="maxBytes">最多读取的字节数（解压后）</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <para>
    ///         <see cref="Tuple{T1,T2,T3}" />
    ///     </para>
    ///     <para>包含解压后字节数组、实际读取字节数以及是否截断的值元组</para>
    /// </returns>
    internal static async Task<(byte[] Buffer, int TotalRead, bool IsTruncated)> ReadAndDecompressFirstBytesAsync(
        Stream rawStream, string? contentEncoding, int maxBytes, CancellationToken cancellationToken)
    {
        // 根据 Content-Encoding 自动包装解压流
        var decompressedStream = Helpers.WrapDecompressionStream(rawStream, contentEncoding);

        try
        {
            // 初始化最大读取字节数的缓冲区
            var buffer = new byte[maxBytes];
            var totalRead = 0;

            // 循环读取直到填满缓冲区或流已结束
            while (totalRead < buffer.Length)
            {
                // 从流中读取数据并写入缓冲区
                var read = await decompressedStream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead),
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
            // 确保解压流被正确释放
            if (decompressedStream != rawStream)
            {
                await decompressedStream.DisposeAsync();
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

        // 获取响应状态码
        var statusCode = (int)httpResponseMessage.StatusCode;

        switch (statusCode)
        {
            // 1xx 信息性状态码呈现蓝色
            case >= 100 and < 200:
                stringBuilder.Append("\e[34m");
                break;
            // 2xx 成功 和 304 未修改呈现绿色
            case >= 200 and < 300 or 304:
                stringBuilder.Append("\e[32m");
                break;
            // 3xx 重定向呈现黄色
            case >= 300 and < 400:
                stringBuilder.Append("\e[33m");
                break;
            // 4xx 客户端错误 和 5xx 服务端错误呈现红色
            case >= 400 and < 600:
                stringBuilder.Append("\e[31m");
                break;
            // 未知或自定义状态码呈现灰色
            default:
                stringBuilder.Append("\e[90m");
                break;
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
    /// <exception cref="ArgumentNullException"></exception>
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
            clonedHttpRequestMessage.Options.AddOrUpdate(key, value);
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

        // 去掉引号、分号等多余字符，并规范化空白
        var normalized = charset.Trim().Trim('"', '\'', ';', ',').Trim();

        // 去掉所有空白后忽略大小写比较 "utf8"
        var noWhitespace = Regex.Replace(normalized, @"\s+", "");

        if (noWhitespace.Equals("utf-8", StringComparison.OrdinalIgnoreCase) ||
            noWhitespace.Equals("utf8", StringComparison.OrdinalIgnoreCase) ||
            noWhitespace.Equals("utf", StringComparison.OrdinalIgnoreCase))
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
    ///     获取与异常关联的 <see cref="HttpResponseMessage" />
    /// </summary>
    /// <param name="exception">
    ///     <see cref="Exception" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HttpResponseMessage? GetResponseMessage(this Exception exception)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(exception);

        return exception.Data[nameof(HttpResponseMessage)] as HttpResponseMessage;
    }

    /// <summary>
    ///     获取与异常关联的请求耗时（毫秒）
    /// </summary>
    /// <param name="exception">
    ///     <see cref="Exception" />
    /// </param>
    /// <returns>
    ///     <see cref="long" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static long? GetRequestDuration(this Exception exception)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(exception);

        return (long?)exception.Data[nameof(HttpRequestPipelineContext.RequestDuration)];
    }

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
        return HttpRemoteUtility.ResolveHttpClientOptions(serviceProvider, httpResponseMessage?.ResolveHttpClientName())
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
                out var enableValue) == true)
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