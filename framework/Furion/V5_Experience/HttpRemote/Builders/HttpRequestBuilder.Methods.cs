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
using Microsoft.Net.Http.Headers;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using ContentDispositionHeaderValue = System.Net.Http.Headers.ContentDispositionHeaderValue;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     表示是否已添加了 <see cref="StringContentForFormUrlEncodedContentProcessor" /> 处理器
    /// </summary>
    internal bool _formUrlEncodedStringContentProcessorAdded;

    /// <summary>
    ///     设置跟踪标识
    /// </summary>
    /// <param name="traceIdentifier">设置跟踪标识</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetTraceIdentifier(string traceIdentifier)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(traceIdentifier);

        TraceIdentifier = traceIdentifier;

        return this;
    }

    /// <summary>
    ///     设置内容类型
    /// </summary>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetContentType(string contentType)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        // 解析内容类型字符串
        var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);

        ContentType = mediaTypeHeaderValue.MediaType;

        // 检查是否包含 charset 设置
        if (!string.IsNullOrWhiteSpace(mediaTypeHeaderValue.CharSet))
        {
            SetContentEncoding(mediaTypeHeaderValue.CharSet);
        }

        return this;
    }

    /// <summary>
    ///     设置内容编码
    /// </summary>
    /// <param name="encoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetContentEncoding(Encoding encoding)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(encoding);

        ContentEncoding = encoding;

        return this;
    }

    /// <summary>
    ///     设置内容编码
    /// </summary>
    /// <param name="encodingName">内容编码名</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetContentEncoding(string encodingName)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(encodingName);

        SetContentEncoding(Encoding.GetEncoding(encodingName));

        return this;
    }

    /// <summary>
    ///     设置 JSON 内容
    /// </summary>
    /// <param name="rawJson">JSON 字符串/原始对象</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetJsonContent(object? rawJson, Encoding? contentEncoding = null,
        string? contentType = null, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 检查是否是字符串类型
        if (rawJson is not string rawString)
        {
            // 当传入 jsonSerializerOptions 参数时，直接进行序列化后再传入
            var extractedJson = jsonSerializerOptions is null
                ? rawJson
                : JsonSerializer.Serialize(rawJson, jsonSerializerOptions);

            return SetContent(extractedJson, contentType ?? MediaTypeNames.Application.Json, contentEncoding);
        }

        // 尝试验证并获取 JsonDocument 实例
        using var jsonDocument = JsonDocument.Parse(rawString);

        return SetContent(jsonDocument.RootElement.Clone(), contentType ?? MediaTypeNames.Application.Json,
            contentEncoding);
    }

    /// <summary>
    ///     设置 JSON 内容（宽松模式）
    /// </summary>
    /// <remarks>跳过对 JSON 字符串校验。</remarks>
    /// <param name="rawJson">JSON 字符串</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetJsonContentWithoutValidation(string? rawJson, Encoding? contentEncoding = null,
        string? contentType = null) =>
        SetContent(rawJson, contentType ?? MediaTypeNames.Application.Json, contentEncoding);

    /// <summary>
    ///     设置 HTML 内容
    /// </summary>
    /// <param name="htmlString">HTML 字符串</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetHtmlContent(string? htmlString, Encoding? contentEncoding = null,
        string? contentType = null) =>
        SetContent(htmlString, contentType ?? MediaTypeNames.Text.Html, contentEncoding);

    /// <summary>
    ///     设置 XML 内容
    /// </summary>
    /// <param name="xmlString">XML 字符串</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetXmlContent(string? xmlString, Encoding? contentEncoding = null,
        string? contentType = null) => SetContent(xmlString, contentType ?? MediaTypeNames.Text.Xml, contentEncoding);

    /// <summary>
    ///     设置文本内容
    /// </summary>
    /// <param name="text">文本</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder
        SetTextContent(string? text, Encoding? contentEncoding = null, string? contentType = null) =>
        SetContent(text, contentType ?? MediaTypeNames.Text.Plain, contentEncoding);

    /// <summary>
    ///     设置原始字符串内容
    /// </summary>
    /// <remarks>字符串内容将被双引号包围并发送，格式如下：<c>"内容"</c>。</remarks>
    /// <param name="text">文本</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetRawStringContent(string text, string? contentType = null,
        Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(text);

        return SetContent(text.AddQuotes(), contentType, contentEncoding);
    }

    /// <summary>
    ///     设置 URL 编码表单内容
    /// </summary>
    /// <param name="rawObject">原始对象</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="useStringContent">
    ///     是否使用 <see cref="StringContent" /> 构建
    ///     <see cref="FormUrlEncodedContent" />。默认值为： <c>false</c>
    /// </param>
    /// <param name="urlEncode">是否对表单数据进行 URL 编码，默认值为： <c>true</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetFormUrlEncodedContent(object? rawObject, Encoding? contentEncoding = null,
        bool useStringContent = false, bool urlEncode = true)
    {
        SetContent(rawObject, MediaTypeNames.Application.FormUrlEncoded, contentEncoding);

        // 检查是否启用 StringContent 方式构建 application/x-www-form-urlencoded 请求内容
        if (useStringContent || !urlEncode)
        {
            AddStringContentForFormUrlEncodedContentProcessor(urlEncode);
        }

        return this;
    }

    /// <summary>
    ///     从本地路径或互联网地址中设置文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="httpMethod"><see cref="HttpMethod" />，默认值为：<see cref="HttpMethod.Get" /></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetFileContent(string filePath, string? fileName = null, string? contentType = null,
        Encoding? contentEncoding = null, Action<HttpRequestMessage>? configure = null, HttpMethod? httpMethod = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        Stream stream;
        string? effectiveFileName;

        // 检查是否是网络地址
        if (Uri.TryCreate(filePath, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            // 尝试从 Uri 地址中解析文件的名称
            effectiveFileName = fileName ?? Helpers.GetFileNameFromUri(uri);

            // 尝试从互联网 URL 地址中加载流
            stream = Helpers.GetStreamFromRemote(filePath, configure, httpMethod);
        }
        else
        {
            // 获取文件的名称
            effectiveFileName = fileName ?? Path.GetFileName(filePath);

            // 读取文件流（没有 using）
            stream = File.OpenRead(filePath);
        }

        return SetStreamContent(stream, effectiveFileName, contentType, contentEncoding);
    }

    /// <summary>
    ///     设置二进制流内容
    /// </summary>
    /// <param name="stream">
    ///     <see cref="Stream" />
    /// </param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="disposeResourcesOnRequestCompletion">是否在请求结束后自动释放资源。默认值为：<c>true</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetStreamContent(Stream stream, string? fileName = null, string? contentType = null,
        Encoding? contentEncoding = null, bool disposeResourcesOnRequestCompletion = true)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(stream);

        // 空检查
        if (!string.IsNullOrWhiteSpace(fileName) &&
            (ContentHeaders is null || !ContentHeaders.ContainsKey(HeaderNames.ContentDisposition)))
        {
            // 添加 Content-Disposition 请求内容标头
            WithHeader(HeaderNames.ContentDisposition,
                new ContentDispositionHeaderValue("attachment") { FileName = fileName.AddQuotes() });
        }

        return SetContent(stream, contentType ?? FileTypeMapper.GetContentType(fileName!), contentEncoding,
            disposeResourcesOnRequestCompletion);
    }

    /// <summary>
    ///     设置 MCP (Model Context Protocol) 2.0 消息内容
    /// </summary>
    /// <param name="clientName">客户端名称</param>
    /// <param name="method">方法名称</param>
    /// <param name="params">方法参数</param>
    /// <param name="protocolVersion"> MCP (Model Context Protocol) 协议版本，默认为 "2026-07-28"</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetMcpContent(string clientName, string method, object? @params,
        string protocolVersion = "2026-07-28") =>
        SetMcpContent(clientName, new McpMessageData(method, @params), protocolVersion);

    /// <summary>
    ///     设置 MCP (Model Context Protocol) 2.0 消息内容
    /// </summary>
    /// <param name="clientName">客户端名称</param>
    /// <param name="data">
    ///     <see cref="McpMessageData" />
    /// </param>
    /// <param name="protocolVersion"> MCP (Model Context Protocol) 协议版本，默认为 "2026-07-28"</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetMcpContent(string clientName, McpMessageData data,
        string protocolVersion = "2026-07-28")
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        ArgumentException.ThrowIfNullOrWhiteSpace(data?.Method);

        // 设置请求头
        WithHeaders(
            new Dictionary<string, object?>
            {
                ["Mcp-Name"] = clientName,
                ["MCP-Protocol-Version"] = protocolVersion,
                ["Mcp-Method"] = data.Method,
                ["Accept"] = "application/json, text/event-stream"
            }, replace: true);

        // 设置 JSON 内容
        SetJsonContent(new
        {
            jsonrpc = data.JsonRpc ?? "2.0",
            id = data.Id ?? Interlocked.Increment(ref _globalRequestId),
            method = data.Method,
            @params = data.Params
        });

        return this;
    }

    /// <summary>
    ///     设置请求内容
    /// </summary>
    /// <param name="rawContent">原始请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="disposeResourcesOnRequestCompletion">是否在请求结束后自动释放资源。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    public HttpRequestBuilder SetContent(object? rawContent, string? contentType = null,
        Encoding? contentEncoding = null, bool disposeResourcesOnRequestCompletion = false)
    {
        // 空检查
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            // 解析内容类型字符串
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);

            // 禁止使用该方法设置 multipart/form-data 类型内容
            if (mediaTypeHeaderValue.MediaType == MediaTypeNames.Multipart.FormData &&
                rawContent is not MultipartContent)
            {
                throw new NotSupportedException(
                    $"The method does not support setting the request content type to `{MediaTypeNames.Multipart.FormData}`. Please use the `{nameof(SetMultipartContent)}` method instead. If you are using an HTTP declarative requests, define the parameter with the `Action<HttpMultipartFormDataBuilder>` type or annotate the parameter with the `MultipartAttribute`.");
            }
        }

        RawContent = rawContent;

        // 空检查
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            SetContentType(contentType);
        }

        // 空检查
        if (contentEncoding is not null)
        {
            SetContentEncoding(contentEncoding);
        }

        // 是否在请求结束后自动释放资源
        if (disposeResourcesOnRequestCompletion && rawContent is IDisposable disposable)
        {
            AddDisposable(disposable);
        }

        return this;
    }

    /// <summary>
    ///     清除请求内容
    /// </summary>
    /// <remarks>
    ///     如果 <see cref="RawContent" /> 实现了 <see cref="IDisposable" />，并且此前已通过 <see cref="AddDisposable" />
    ///     将其添加到可释放对象列表中，则该方法会将其从列表中移除。
    /// </remarks>
    /// <param name="disposeRawContent">是否需要是否原始内容（若实现 <see cref="IDisposable" /> 接口）。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder RemoveContent(bool disposeRawContent = false)
    {
        // 获取原始请求内容
        var rawContent = RawContent;

        // 移除需要释放的原始请求内容
        if (rawContent is IDisposable disposable)
        {
            // 检查是否需要是否原始内容
            if (disposeRawContent)
            {
                disposable.Dispose();
            }

            Disposables?.Remove(disposable);
        }

        ContentType = null;
        ContentEncoding = null;
        RawContent = null;
        MultipartFormDataBuilder = null;
        OmitContentType = false;

        return this;
    }

    /// <summary>
    ///     追加请求内容
    /// </summary>
    /// <remarks>
    ///     <para>仅当 <see cref="RawContent" /> 不为空时有效。</para>
    ///     <para>该方法仅负责合并内容，不会改变已有的 <see cref="ContentType" /> 和 <see cref="ContentEncoding" />。</para>
    ///     <para>支持多次调用，根据 <see cref="RawContent" /> 与传入内容的类型进行智能合并：</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>字符串 + 字符串：使用 <c>&amp;</c> 拼接</description>
    ///         </item>
    ///         <item>
    ///             <description><see cref="StringBuilder" /> + 字符串：追加字符串</description>
    ///         </item>
    ///         <item>
    ///             <description><see cref="NameValueCollection" /> + <see cref="NameValueCollection" /> 或字典：合并键值对（同名键保留多个值）</description>
    ///         </item>
    ///         <item>
    ///             <description>字典 + 字典：合并键值对（已存在的键更新值）</description>
    ///         </item>
    ///         <item>
    ///             <description>列表 + 集合：追加元素</description>
    ///         </item>
    ///         <item>
    ///             <description>其他情况：覆盖原有内容</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="rawContent">原始请求内容</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AppendContent(object? rawContent)
    {
        // 空检查
        if (rawContent is null)
        {
            return this;
        }

        // 空检查
        return RawContent is null
            ? this
            : SetContent(MergeContent(RawContent, rawContent));
    }

    /// <summary>
    ///     设置多部分表单内容，请求类型为 <c>multipart/form-data</c>
    /// </summary>
    /// <remarks>
    ///     该操作将强制覆盖 <see cref="SetContent" />、<see cref="SetContentEncoding(System.Text.Encoding)" /> 和
    ///     <see cref="SetContentType" /> 设置的内容。
    /// </remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetMultipartContent(Action<HttpMultipartFormDataBuilder> configure) =>
        SetMultipartContent(configure, true);

    /// <summary>
    ///     设置多部分表单内容，请求类型为 <c>multipart/form-data</c>
    /// </summary>
    /// <remarks>
    ///     该操作将强制覆盖 <see cref="SetContent" />、<see cref="SetContentEncoding(System.Text.Encoding)" /> 和
    ///     <see cref="SetContentType" /> 设置的内容。
    /// </remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="omitContentType">是否移除默认的多部分内容的 <c>Content-Type</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetMultipartContent(Action<HttpMultipartFormDataBuilder> configure, bool omitContentType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 初始化 HttpMultipartFormDataBuilder 实例
        var httpMultipartFormDataBuilder = new HttpMultipartFormDataBuilder(this) { OmitContentType = omitContentType };

        // 调用自定义配置委托
        configure.Invoke(httpMultipartFormDataBuilder);

        MultipartFormDataBuilder = httpMultipartFormDataBuilder;

        return this;
    }

    /// <summary>
    ///     设置多部分表单内容，请求类型为 <c>multipart/form-data</c>
    /// </summary>
    /// <remarks>
    ///     该操作将强制覆盖 <see cref="SetContent" />、<see cref="SetContentEncoding(System.Text.Encoding)" /> 和
    ///     <see cref="SetContentType" /> 设置的内容。
    /// </remarks>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpRequestBuilder SetMultipartContent(HttpMultipartFormDataBuilder httpMultipartFormDataBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMultipartFormDataBuilder);

        MultipartFormDataBuilder = httpMultipartFormDataBuilder;

        return this;
    }

    /// <summary>
    ///     追加多部分表单内容
    /// </summary>
    /// <remarks>仅当 <see cref="HttpMultipartFormDataBuilder" /> 不为空时有效。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithMultipart(Action<HttpMultipartFormDataBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 空检查
        if (MultipartFormDataBuilder is not null)
        {
            // 调用自定义配置委托
            configure.Invoke(MultipartFormDataBuilder);
        }

        return this;
    }

    /// <summary>
    ///     设置请求标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="headerValue">请求标头值格式化字符串</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="replace">是否替换已存在的请求标头。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithHeader(string headerValue, bool escape = false, bool replace = false)
    {
        // 尝试将字符串按第一个冒号拆分为键值对
        if (Helpers.TrySplitHeader(headerValue, out var key, out var value))
        {
            WithHeader(key, value, escape, replace);
        }

        return this;
    }

    /// <summary>
    ///     设置请求标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="replace">是否替换已存在的请求标头。默认值为：<c>false</c></param>
    /// <param name="format">要使用的格式，仅当 <paramref name="value" /> 实现 <see cref="IFormattable" /> 时有效</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithHeader(string key, object? value, bool escape = false, bool replace = false,
        string? format = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        // 尝试使用 format 格式化值
        var formattedValue = !string.IsNullOrWhiteSpace(format) && value is IFormattable formattable
            ? formattable.ToString(format, null)
            : value;

        return WithHeaders(
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { key, formattedValue } }, escape,
            replace);
    }

    /// <summary>
    ///     设置请求标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="headers">请求标头集合</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="replace">是否替换已存在的请求标头。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithHeaders(IDictionary<string, object?> headers, bool escape = false,
        bool replace = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(headers);

        // 初始化请求标头和请求内容标头字典
        var requestHeaders = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var contentHeaders = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        // 对不同的标头类型进行归类
        foreach (var (key, value) in headers)
        {
            if (KnownContentHeaders.Contains(key))
            {
                contentHeaders[key] = value;
            }
            else
            {
                requestHeaders[key] = value;
            }
        }

        // 合并请求标头
        Headers = MergeHeaders(Headers, requestHeaders, escape, replace);

        // 合并请求内容标头
        ContentHeaders = MergeHeaders(ContentHeaders, contentHeaders, escape, replace);

        return this;
    }

    /// <summary>
    ///     设置请求标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="headerSource">请求标头源对象</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="replace">是否替换已存在的请求标头。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithHeaders(object headerSource, bool escape = false, bool replace = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(headerSource);

        return WithHeaders(
            headerSource.ObjectToDictionary()!.ToDictionary(u => u.Key.ToInvariantCultureString()!, u => u.Value,
                StringComparer.OrdinalIgnoreCase), escape, replace);
    }

    /// <summary>
    ///     设置需要从请求中移除的标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="headerNames">请求标头名集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder RemoveHeaders(params string[] headerNames)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(headerNames);

        // 检查是否为空元素数组
        if (headerNames.Length == 0)
        {
            return this;
        }

        HeadersToRemove ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 逐条添加到集合中
        foreach (var headerName in headerNames)
        {
            // 空检查
            if (string.IsNullOrWhiteSpace(headerName))
            {
                continue;
            }

            // 移除请求标头
            HeadersToRemove.Add(headerName);

            // 移除请求内容标头
            if (KnownContentHeaders.Contains(headerName))
            {
                ContentHeaders?.Remove(headerName);
            }
        }

        return this;
    }

    /// <summary>
    ///     设置片段标识符
    /// </summary>
    /// <param name="fragment">片段标识符</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetFragment(string fragment)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(fragment);

        Fragment = fragment;

        return this;
    }

    /// <summary>
    ///     设置路径片段
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="segment">路径片段</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithPathSegment(string segment) => WithPathSegments([segment]);

    /// <summary>
    ///     设置路径片段
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="segments">路径片段集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithPathSegments(IEnumerable<string> segments)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(segments);

        PathSegments ??= [];
        PathSegments.AddRange(segments);

        return this;
    }

    /// <summary>
    ///     设置需要从 URL 地址中移除的路径片段
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="segments">路径片段集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder RemovePathSegments(params string[] segments)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(segments);

        // 检查是否为空元素数组
        if (segments.Length == 0)
        {
            return this;
        }

        PathSegmentsToRemove ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 逐条添加到集合中
        foreach (var segment in segments)
        {
            if (!string.IsNullOrWhiteSpace(segment))
            {
                PathSegmentsToRemove.Add(segment);
            }
        }

        return this;
    }

    /// <summary>
    ///     设置查询参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="replace">是否替换已存在的查询参数。默认值为：<c>false</c></param>
    /// <param name="ignoreNullValues">是否忽略空值。默认值为：<c>false</c></param>
    /// <param name="format">要使用的格式，仅当 <paramref name="value" /> 实现 <see cref="IFormattable" /> 时有效</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithQueryParameter(string key, object? value, bool replace = false,
        bool ignoreNullValues = false, string? format = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        // 尝试使用 format 格式化值
        var formattedValue = !string.IsNullOrWhiteSpace(format) && value is IFormattable formattable
            ? formattable.ToString(format, null)
            : value;

        return WithQueryParameters(
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { key, formattedValue } }, replace,
            ignoreNullValues);
    }

    /// <summary>
    ///     设置查询参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="valueProvider">值提供器</param>
    /// <param name="replace">是否替换已存在的查询参数。默认值为：<c>false</c></param>
    /// <param name="ignoreNullValues">是否忽略空值。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithQueryParameter(string key, Func<object?> valueProvider, bool replace = false,
        bool ignoreNullValues = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(valueProvider);

        return WithQueryParameter(key, (object?)valueProvider, replace, ignoreNullValues);
    }

    /// <summary>
    ///     设置查询参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="valueProvider">值提供器</param>
    /// <param name="replace">是否替换已存在的查询参数。默认值为：<c>false</c></param>
    /// <param name="ignoreNullValues">是否忽略空值。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithQueryParameter(string key, Func<UrlFormattingContext, object?> valueProvider,
        bool replace = false, bool ignoreNullValues = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(valueProvider);

        return WithQueryParameter(key, (object?)valueProvider, replace, ignoreNullValues);
    }

    /// <summary>
    ///     设置查询参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameters">查询参数集合</param>
    /// <param name="replace">是否替换已存在的查询参数。默认值为：<c>false</c></param>
    /// <param name="ignoreNullValues">是否忽略空值。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithQueryParameters(IDictionary<string, object?> parameters, bool replace = false,
        bool ignoreNullValues = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameters);

        // 初始化查询参数
        QueryParameters ??= new Dictionary<string, List<object?>>(StringComparer.OrdinalIgnoreCase);

        // 存在则合并否则添加
        QueryParameters.AddOrUpdate(
            parameters.WhereIf(ignoreNullValues, u => u.Value is not null)
                .ToDictionary(StringComparer.OrdinalIgnoreCase), false, replace);

        // 检查是否替换已存在的查询参数
        if (!replace)
        {
            return this;
        }

        ReplacedQueryParameterKeys ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 记录这些需要替换的参数名
        foreach (var key in parameters.Keys)
        {
            ReplacedQueryParameterKeys.Add(key);
        }

        return this;
    }

    /// <summary>
    ///     设置查询参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameterSource">查询参数集合</param>
    /// <param name="prefix">参数前缀。对于对象类型可生成如 <c>prefix.Name=furion</c> 与 <c>prefix.Age=30</c> 参数格式</param>
    /// <param name="replace">是否替换已存在的查询参数。默认值为：<c>false</c></param>
    /// <param name="ignoreNullValues">是否忽略空值。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithQueryParameters(object parameterSource, string? prefix = null, bool replace = false,
        bool ignoreNullValues = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameterSource);

        return WithQueryParameters(
            parameterSource.ObjectToDictionary()!.ToDictionary(
                u => $"{(string.IsNullOrWhiteSpace(prefix) ? null : $"{prefix}.")}{u.Key.ToInvariantCultureString()}",
                u => u.Value, StringComparer.OrdinalIgnoreCase), replace, ignoreNullValues);
    }

    /// <summary>
    ///     设置查询参数排序规则
    /// </summary>
    /// <param name="sorter">接收所有查询键值对（包括原始 URL 参数和用户追加的参数），返回排序后的键值对序列。为 <c>null</c> 时不排序（原始添加顺序）</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetQueryParametersSorter(
        Func<IEnumerable<KeyValuePair<string, string?>>, IEnumerable<KeyValuePair<string, string?>>>? sorter)
    {
        QueryParametersSorter = sorter;

        return this;
    }

    /// <summary>
    ///     设置需要从 URL 中移除的查询参数集合
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameterNames">查询参数键集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder RemoveQueryParameters(params string[] parameterNames)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameterNames);

        // 检查是否为空元素数组
        if (parameterNames.Length == 0)
        {
            return this;
        }

        QueryParametersToRemove ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 逐条添加到集合中
        foreach (var parameterName in parameterNames)
        {
            // 空检查
            if (!string.IsNullOrWhiteSpace(parameterName))
            {
                QueryParametersToRemove.Add(parameterName);
            }
        }

        return this;
    }

    /// <summary>
    ///     设置路径参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithPathParameter(string key, object? value)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return WithPathParameters(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { key, value } });
    }

    /// <summary>
    ///     设置路径参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameters">路径参数集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithPathParameters(IDictionary<string, object?> parameters)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameters);

        PathParameters ??= new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        // 存在则更新否则添加
        PathParameters.AddOrUpdate(parameters.ToDictionary(u => u.Key, u => u.Value?.ToInvariantCultureString(),
            StringComparer.OrdinalIgnoreCase));

        return this;
    }

    /// <summary>
    ///     设置路径参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameterSource">路径参数源对象</param>
    /// <param name="prefix">模板字符串前缀。若该参数值不为空，则支持 <c>{prefix.Prop.SubProp}</c> 对象路径方式</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithPathParameters(object? parameterSource, string? prefix = null)
    {
        // 检查是否设置了模板字符串前缀
        if (string.IsNullOrWhiteSpace(prefix))
        {
            // 空检查
            ArgumentNullException.ThrowIfNull(parameterSource);

            return WithPathParameters(
                parameterSource.ObjectToDictionary()!.ToDictionary(u => u.Key.ToInvariantCultureString()!,
                    u => u.Value, StringComparer.OrdinalIgnoreCase));
        }

        ObjectPathParameters ??= new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        // 存在则更新否则添加
        ObjectPathParameters[prefix] = parameterSource;

        return this;
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookieHeaderValue">Cookie 标头值格式化字符串</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithCookie(string cookieHeaderValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(cookieHeaderValue);

        return WithCookies(cookieHeaderValue);
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="format">要使用的格式，仅当 <paramref name="value" /> 实现 <see cref="IFormattable" /> 时有效</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithCookie(string key, object? value, string? format = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        // 尝试使用 format 格式化值
        var formattedValue = !string.IsNullOrWhiteSpace(format) && value is IFormattable formattable
            ? formattable.ToString(format, null)
            : value;

        return WithCookies(
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { key, formattedValue } });
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookieHeaderValue">Cookie 标头值格式化字符串</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithCookies(string cookieHeaderValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(cookieHeaderValue);

        return WithCookies(cookieHeaderValue.ParseFormatKeyValueString([';']));
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookies">Cookies 集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithCookies(IDictionary<string, object?> cookies)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(cookies);

        Cookies ??= new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        // 存在则更新否则添加
        Cookies.AddOrUpdate(cookies.ToDictionary(u => u.Key, u => u.Value?.ToInvariantCultureString(),
            StringComparer.OrdinalIgnoreCase));

        return this;
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookieSource">Cookie 参数源对象</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithCookies(object cookieSource)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(cookieSource);

        // 存在则更新否则添加
        return WithCookies(
            cookieSource.ObjectToDictionary()!.ToDictionary(u => u.Key.ToInvariantCultureString()!, u => u.Value,
                StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     需要从请求中移除的 Cookie 集合
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookieNames">Cookie 键集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder RemoveCookies(params string[] cookieNames)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(cookieNames);

        // 检查是否为空元素数组
        if (cookieNames.Length == 0)
        {
            return this;
        }

        CookiesToRemove ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 逐条添加到集合中
        foreach (var cookieName in cookieNames)
        {
            // 空检查
            if (!string.IsNullOrWhiteSpace(cookieName))
            {
                CookiesToRemove.Add(cookieName);
            }
        }

        return this;
    }

    /// <summary>
    ///     设置 <see cref="HttpClient" /> 实例的配置名称
    /// </summary>
    /// <param name="httpClientName"><see cref="HttpClient" /> 实例的配置名称</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetHttpClientName(string? httpClientName)
    {
        HttpClientName = httpClientName;

        return this;
    }

    /// <summary>
    ///     设置响应内容最大缓存字节数
    /// </summary>
    /// <param name="maxResponseContentBufferSize">响应内容最大缓存字节数</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetMaxResponseContentBufferSize(long maxResponseContentBufferSize)
    {
        // 小于或等于 0 检查
        if (maxResponseContentBufferSize <= 0)
        {
            throw new ArgumentException("Max response content buffer size must be greater than 0.",
                nameof(maxResponseContentBufferSize));
        }

        MaxResponseContentBufferSize = maxResponseContentBufferSize;

        return this;
    }

    /// <summary>
    ///     设置 <see cref="HttpClient" /> 实例提供器
    /// </summary>
    /// <param name="configure"><inheritdoc cref="HttpClient" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetHttpClientProvider(Func<(HttpClient Instance, Action<HttpClient>? Release)> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        HttpClientProvider = configure;

        return this;
    }

    /// <summary>
    ///     添加 <see cref="IHttpContentProcessor" /> 请求内容处理器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure"><see cref="IHttpContentProcessor" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder AddHttpContentProcessors(Func<IEnumerable<IHttpContentProcessor>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        HttpContentProcessorProviders ??= new List<Func<IEnumerable<IHttpContentProcessor>>>();

        HttpContentProcessorProviders.Add(configure);

        return this;
    }

    /// <summary>
    ///     添加 <see cref="IHttpContentConverter" /> 响应内容转换器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure"><see cref="IHttpContentConverter" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder AddHttpContentConverters(Func<IEnumerable<IHttpContentConverter>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        HttpContentConverterProviders ??= new List<Func<IEnumerable<IHttpContentConverter>>>();

        HttpContentConverterProviders.Add(configure);

        return this;
    }

    /// <summary>
    ///     设置用于处理在设置 <see cref="HttpRequestMessage" /> 的 <c>Content</c> 时的操作
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnPreSetContent(Action<HttpContent?> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnPreSetContent += configure;

        return this;
    }

    /// <summary>
    ///     设置在发送 HTTP 请求之前执行的操作
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnPreSendRequest(Action<HttpRequestMessage> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnPreSendRequest += configure;

        return this;
    }

    /// <summary>
    ///     设置在收到 HTTP 响应之后执行的操作
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnPostReceiveResponse(Func<HttpResponseMessage, CancellationToken, Task> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnPostReceiveResponse += configure;

        return this;
    }

    /// <summary>
    ///     设置在发送 HTTP 请求发生异常时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetOnRequestFailed(Action<Exception, HttpResponseMessage?> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnRequestFailed = configure;

        return this;
    }

    /// <summary>
    ///     如果 HTTP 响应的 IsSuccessStatusCode 属性是 <c>false</c>，则引发异常
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder EnsureSuccessStatusCode() => EnsureSuccessStatusCode(true);

    /// <summary>
    ///     设置是否如果 HTTP 响应的 IsSuccessStatusCode 属性是 <c>false</c>，则引发异常
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder EnsureSuccessStatusCode(bool enabled)
    {
        EnsureSuccessStatusCodeEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置 Basic 身份认证凭据请求授权标头
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder AddBasicAuthentication(string username, string? password)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        // 将用户名和密码存入自定义数据中，供内部组件切换方案时使用
        WithData(Constants.INTERNAL_AUTH_SCHEME_KEY, Constants.BASIC_AUTHENTICATION_SCHEME);
        WithData(Constants.INTERNAL_AUTH_USERNAME_KEY, username);
        WithData(Constants.INTERNAL_AUTH_PASSWORD_KEY, password);

        // 将用户名和密码转换为 Base64 字符串
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

        AddAuthentication(new AuthenticationHeaderValue(Constants.BASIC_AUTHENTICATION_SCHEME, base64Credentials));

        return this;
    }

    /// <summary>
    ///     设置 Bearer 身份认证凭据请求授权标头
    /// </summary>
    /// <param name="token">令牌</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder AddBearerAuthentication(string token)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        AddAuthentication(new AuthenticationHeaderValue(Constants.BEARER_AUTHENTICATION_SCHEME, token));

        return this;
    }

    /// <summary>
    ///     设置 Bearer 身份认证凭据请求授权标头
    /// </summary>
    /// <param name="headerName">自定义标头</param>
    /// <param name="token">令牌</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder AddBearerAuthentication(string headerName, string token)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return WithHeader(headerName, $"Bearer {token}", replace: true);
    }

    /// <summary>
    ///     设置 Digest（摘要）身份认证凭据请求授权标头
    /// </summary>
    /// <remarks>建议先使用 <see cref="DigestCredentials.GetDigestCredentials" /> 获取凭证并缓存，再直接设置 <c>Authorization</c> 标头，避免每次请求触发探测。</remarks>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder AddDigestAuthentication(string username, string password)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // 将用户名和密码存入自定义数据中，供内部组件切换方案时使用
        WithData(Constants.INTERNAL_AUTH_SCHEME_KEY, Constants.DIGEST_AUTHENTICATION_SCHEME);
        WithData(Constants.INTERNAL_AUTH_USERNAME_KEY, username);
        WithData(Constants.INTERNAL_AUTH_PASSWORD_KEY, password);

        // 清除可能存在的认证信息
        AuthenticationHeader = null;

        return this;
    }

    /// <summary>
    ///     设置身份认证凭据请求授权标头
    /// </summary>
    /// <param name="scheme">身份认证的方案</param>
    /// <param name="parameter">身份认证的凭证</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AddAuthentication(string scheme, string? parameter) =>
        AddAuthentication(new AuthenticationHeaderValue(scheme, parameter));

    /// <summary>
    ///     设置身份认证凭据请求授权标头
    /// </summary>
    /// <param name="authenticationHeader">
    ///     <see cref="AuthenticationHeaderValue" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder AddAuthentication(AuthenticationHeaderValue authenticationHeader)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(authenticationHeader);

        AuthenticationHeader = authenticationHeader;

        return this;
    }

    /// <summary>
    ///     设置禁用 HTTP 缓存
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder DisableCache() => DisableCache(true);

    /// <summary>
    ///     设置禁用 HTTP 缓存
    /// </summary>
    /// <param name="disabled">是否禁用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder DisableCache(bool disabled)
    {
        DisableCacheEnabled = disabled;

        return this;
    }

    /// <summary>
    ///     设置 HTTP 远程请求事件处理程序
    /// </summary>
    /// <param name="requestEventHandlerType">实现 <see cref="IHttpRequestEventHandler" /> 接口的类型</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetEventHandler(Type requestEventHandlerType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(requestEventHandlerType);

        // 检查类型是否实现了 IHttpRequestEventHandler 接口
        if (!typeof(IHttpRequestEventHandler).IsAssignableFrom(requestEventHandlerType))
        {
            throw new ArgumentException(
                $"`{requestEventHandlerType}` type is not assignable from `{typeof(IHttpRequestEventHandler)}`.",
                nameof(requestEventHandlerType));
        }

        RequestEventHandlerType = requestEventHandlerType;

        return this;
    }

    /// <summary>
    ///     设置 HTTP 远程请求事件处理程序
    /// </summary>
    /// <typeparam name="TRequestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetEventHandler<TRequestEventHandler>()
        where TRequestEventHandler : IHttpRequestEventHandler =>
        SetEventHandler(typeof(TRequestEventHandler));

    /// <summary>
    ///     设置是否启用 <see cref="HttpClient" /> 的池化管理
    /// </summary>
    /// <remarks>
    ///     <para>用于在并发请求中复用同一个 <see cref="HttpClient" /> 实例。</para>
    ///     <para>注意：启用池化管理后，在请求完成之后需手动调用 <see cref="ReleaseResources" /> 方法释放资源。</para>
    /// </remarks>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseHttpClientPool()
    {
        HttpClientPoolingEnabled = true;

        return this;
    }

    /// <summary>
    ///     添加请求结束时需要释放的对象
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="disposable">
    ///     <see cref="IDisposable" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder AddDisposable(IDisposable disposable)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(disposable);

        return AddDisposables(disposable);
    }

    /// <summary>
    ///     批量添加请求结束时需要释放的对象
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="disposables">
    ///     <see cref="IDisposable" /> 集合
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder AddDisposables(params IEnumerable<IDisposable> disposables)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(disposables);

        Disposables ??= [];
        Disposables.AddRange(disposables);

        return this;
    }

    /// <summary>
    ///     释放资源集合
    /// </summary>
    /// <remarks>包含自定义 <see cref="HttpClient" /> 实例和其他可释放对象集合。</remarks>
    public void ReleaseResources()
    {
        // 释放通过自定义提供器创建的 HttpClient 实例
        HttpClientPooling?.Release?.Invoke(HttpClientPooling.Instance);
        HttpClientPooling = null;

        // 释放可释放的对象集合
        ReleaseDisposables();
    }

    /// <summary>
    ///     设置请求来源地址
    /// </summary>
    /// <remarks>设置此配置后，将在单次请求标头中添加 <c>Referer</c> 标头。</remarks>
    /// <param name="referer">请求来源地址，当设置为 <c>"{BASE_ADDRESS}"</c> 时将替换为基地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetReferer(string? referer) =>
        WithHeader(HeaderNames.Referer, referer, replace: true);

    /// <summary>
    ///     设置请求标头的 <c>User-Agent</c> 值
    /// </summary>
    /// <remarks>推荐使用 <see cref="UserAgents" /> 静态类。</remarks>
    /// <param name="userAgent"><c>User-Agent</c> 值</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetUserAgent(string? userAgent) =>
        WithHeader(HeaderNames.UserAgent, userAgent, replace: true);

    /// <summary>
    ///     设置模拟浏览器环境
    /// </summary>
    /// <remarks>支持桌面端或移动端切换。设置此配置后，将在单次请求标头中添加 <c>User-Agent</c> 值。</remarks>
    /// <param name="isMobile">是否为移动端，默认值为：<c>false</c>（即桌面端）</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SimulateBrowser(bool isMobile = false) =>
        SetUserAgent(!isMobile ? UserAgents.Edge.PC : UserAgents.Edge.Mobile);

    /// <summary>
    ///     添加状态码处理程序
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <param name="handler">自定义处理程序</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithStatusCodeHandler(object statusCode,
        Func<HttpResponseMessage, CancellationToken, Task> handler) =>
        WithStatusCodeHandler([statusCode], handler);

    /// <summary>
    ///     添加任何状态码处理程序
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="handler">自定义处理程序</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithAnyStatusCodeHandler(Func<HttpResponseMessage, CancellationToken, Task> handler) =>
        WithStatusCodeHandler(["*"], handler);

    /// <summary>
    ///     添加请求成功（200-299）状态码处理程序
    /// </summary>
    /// <param name="handler">自定义处理程序</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder
        WithSuccessStatusCodeHandler(Func<HttpResponseMessage, CancellationToken, Task> handler) =>
        WithStatusCodeHandler("200-299", handler);

    /// <summary>
    ///     添加状态码处理程序
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="statusCodes">HTTP 状态码集合</param>
    /// <param name="handler">自定义处理程序</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithStatusCodeHandler(IEnumerable<object> statusCodes,
        Func<HttpResponseMessage, CancellationToken, Task> handler)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(statusCodes);
        ArgumentNullException.ThrowIfNull(handler);

        // 防止多次枚举
        var codesArray = statusCodes.ToArray();

        // 检查数量是否为空
        if (codesArray is { Length: 0 })
        {
            throw new ArgumentException(
                "The status codes array cannot be empty. At least one status code must be provided.",
                nameof(statusCodes));
        }

        StatusCodeHandlers ??=
            new Dictionary<IEnumerable<object>, Func<HttpResponseMessage, CancellationToken, Task>>();

        StatusCodeHandlers[codesArray] = handler;

        return this;
    }

    /// <summary>
    ///     启用请求分析工具（调试）
    /// </summary>
    /// <remarks>建议在生产环境中禁用或关闭。</remarks>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder Debugger() => Profiler(true);

    /// <summary>
    ///     禁用请求分析工具（调试）
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder DisableDebugger() => Profiler(false);

    /// <summary>
    ///     设置是否启用请求分析工具（调试）
    /// </summary>
    /// <remarks>建议在生产环境中禁用或关闭。</remarks>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder Debugger(bool enabled) => Profiler(enabled);

    /// <summary>
    ///     启用请求分析工具
    /// </summary>
    /// <remarks>建议在生产环境中禁用或关闭。</remarks>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder Profiler() => Profiler(true);

    /// <summary>
    ///     禁用请求分析工具
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder DisableProfiler() => Profiler(false);

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <remarks>建议在生产环境中禁用或关闭。</remarks>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder Profiler(bool enabled)
    {
        ProfilerEnabled = enabled;
        ProfilerDisabled = !enabled;

        return this;
    }

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <remarks>建议在生产环境中禁用或关闭。</remarks>
    /// <param name="predicate">自定义处理委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder Profiler(Action<HttpRemoteAnalyzer> predicate) => Profiler(predicate, true);

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <remarks>建议在生产环境中禁用或关闭。</remarks>
    /// <param name="predicate">自定义处理委托</param>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder Profiler(Action<HttpRemoteAnalyzer> predicate, bool enabled)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(predicate);

        ProfilerPredicate = enabled ? predicate : null;

        return Profiler(enabled);
    }

    /// <summary>
    ///     设置客户端所偏好的自然语言和区域设置
    /// </summary>
    /// <remarks>设置此配置后，将在单次请求标头中添加 <c>Accept-Language</c> 值。</remarks>
    /// <param name="language">自然语言和区域设置</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AcceptLanguage(string? language) =>
        WithHeader(HeaderNames.AcceptLanguage, language, replace: true);

    /// <summary>
    ///     设置 <see cref="HttpRequestMessage" /> 请求属性
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithProperty(string key, object? value)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(key);

        return WithProperties(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { key, value } });
    }

    /// <summary>
    ///     设置 <see cref="HttpRequestMessage" /> 请求属性集合
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="properties">请求的属性集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithProperties(IDictionary<string, object?> properties)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(properties);

        Properties.AddOrUpdate(properties);

        return this;
    }

    /// <summary>
    ///     设置 <see cref="HttpRequestMessage" /> 请求属性集合
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="propertySource"><see cref="HttpRequestMessage" /> 请求的属性源对象</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithProperties(object? propertySource)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(propertySource);

        return WithProperties(
            propertySource.ObjectToDictionary()!.ToDictionary(u => u.Key.ToInvariantCultureString()!, u => u.Value,
                StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     设置启用标准请求标头
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseStandardRequestHeaders() => UseStandardRequestHeaders(true);

    /// <summary>
    ///     设置是否启用标准请求标头
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseStandardRequestHeaders(bool enabled)
    {
        StandardRequestHeadersEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置是否自动设置 <c>Host</c> 标头
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AutoSetHostHeader() => AutoSetHostHeader(true);

    /// <summary>
    ///     设置是否自动设置 <c>Host</c> 标头
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AutoSetHostHeader(bool enabled)
    {
        AutoSetHostHeaderEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置请求基地址
    /// </summary>
    /// <param name="baseAddress">基地址。支持相对路径</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetBaseAddress(Uri? baseAddress)
    {
        BaseAddress = baseAddress;

        return this;
    }

    /// <summary>
    ///     设置请求基地址
    /// </summary>
    /// <remarks>可充当前缀使用。</remarks>
    /// <param name="baseAddress">基地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetBaseAddress(string? baseAddress) =>
        SetBaseAddress(string.IsNullOrWhiteSpace(baseAddress)
            ? null
            : new Uri(baseAddress, UriKind.RelativeOrAbsolute));

    /// <summary>
    ///     设置 HTTP 版本
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetVersion(string? version) =>
        SetVersion(string.IsNullOrWhiteSpace(version) ? null : new Version(version));

    /// <summary>
    ///     设置 HTTP 版本
    /// </summary>
    /// <param name="version">
    ///     <see cref="Version" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetVersion(Version? version)
    {
        Version = version;

        return this;
    }

    /// <summary>
    ///     设置异常抑制
    /// </summary>
    /// <remarks>抑制所有异常。重复调用仅最后一次调用生效。</remarks>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SuppressExceptions() => SuppressExceptions(true);

    /// <summary>
    ///     设置异常抑制
    /// </summary>
    /// <remarks>重复调用仅最后一次调用生效。</remarks>
    /// <param name="enable">是否启用异常抑制。当设置为 <c>false</c> 时，将禁用异常抑制机制</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SuppressExceptions(bool enable) => SuppressExceptions(enable ? [typeof(Exception)] : []);

    /// <summary>
    ///     设置是否移除默认的内容的 <c>Content-Type</c>
    /// </summary>
    /// <param name="omit">如果为 <c>true</c> 则移除，默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOmitContentType(bool omit)
    {
        OmitContentType = omit;

        return this;
    }

    /// <summary>
    ///     设置异常抑制
    /// </summary>
    /// <remarks>重复调用仅最后一次调用生效。</remarks>
    /// <param name="exceptionTypes">异常抑制类型集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SuppressExceptions(Type[] exceptionTypes)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(exceptionTypes);

        // 检查是否包含 null 或者不是 Exception 类型的元素
        if (exceptionTypes.Any(u => (Type?)u is null || !typeof(Exception).IsAssignableFrom(u)))
        {
            throw new ArgumentException(
                "All elements in exceptionTypes must be non-null and assignable to System.Exception.");
        }

        // 释放引用
        SuppressExceptionTypes = null;

        // 空检查
        if (exceptionTypes.Length == 0)
        {
            return this;
        }

        // 确保每次都能覆盖
        SuppressExceptionTypes = [];

        // 遍历异常抑制类型集合逐条追加
        foreach (var exceptionType in exceptionTypes)
        {
            SuppressExceptionTypes.Add(exceptionType);
        }

        return this;
    }

    /// <summary>
    ///     当指定条件为 <c>true</c> 时，对构建器执行配置操作
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="condition">布尔条件值</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder When(bool condition, Action<HttpRequestBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 检查条件是否成立
        if (condition)
        {
            configure(this);
        }

        return this;
    }

    /// <summary>
    ///     克隆并返回新的 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <param name="excludePropertyNames">要排除复制的属性名称集合</param>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder Clone(params string[]? excludePropertyNames)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(HttpMethod);

        // 初始化新的 HttpRequestBuilder 实例
        var httpRequestBuilder = new HttpRequestBuilder();

        // 将所有配置从当前构建器复制到目标 httpRequestBuilder
        CopyTo(httpRequestBuilder, excludePropertyNames);

        return httpRequestBuilder;
    }

    /// <summary>
    ///     将所有配置从当前构建器复制到目标 <see cref="HttpRequestBuilder" />
    /// </summary>
    /// <param name="target">目标 <see cref="HttpRequestBuilder" /></param>
    /// <param name="excludePropertyNames">要排除复制的属性名称集合</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void CopyTo(HttpRequestBuilder target, params string[]? excludePropertyNames)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(target);

        // 构建排除属性哈希集
        var excludeSet = excludePropertyNames?.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 遍历所有属性并设置给 target 实例
        foreach (var property in CachedProperties.Value)
        {
            // 跳过需要排除的属性
            if (excludeSet is not null && excludeSet.Contains(property.Name))
            {
                continue;
            }

            property.SetValue(target, property.GetValue(this));
        }
    }

    /// <summary>
    ///     启用断言功能
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseAssertions() => UseAssertions(true);

    /// <summary>
    ///     设置是否开启断言功能
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseAssertions(bool enabled)
    {
        AssertionsEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     配置断言逻辑
    /// </summary>
    /// <remarks>
    ///     <para>当 <see cref="UseAssertions()" /> 处于启用状态有效。</para>
    ///     <para>支持多次调用。</para>
    /// </remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder Asserts(Action<HttpAssertionBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 检查断言是否开启
        if (!AssertionsEnabled)
        {
            return this;
        }

        // 初始化 HttpAssertionBuilder 实例
        var httpAssertionBuilder = new HttpAssertionBuilder();

        // 调用自定义配置委托
        configure(httpAssertionBuilder);

        // 获取请求断言和响应断言集合
        var requestAssertions = httpAssertionBuilder.GetRequestAssertions();
        var responseAssertions = httpAssertionBuilder.GetResponseAssertions();

        // 添加请求断言委托集合
        if (requestAssertions.Count > 0)
        {
            RequestAssertions ??= [];
            RequestAssertions.AddRange(requestAssertions);
        }

        // 添加响应断言委托集合
        // ReSharper disable once InvertIf
        if (responseAssertions.Count > 0)
        {
            ResponseAssertions ??= [];
            ResponseAssertions.AddRange(responseAssertions);
        }

        return this;
    }

    /// <summary>
    ///     设置启用 JSON 响应反序列化包装器
    /// </summary>
    /// <remarks>当配置了 <see cref="HttpClientOptions.JsonResponseWrapper" /> 时有效。</remarks>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseJsonResponseWrapper() => UseJsonResponseWrapper(true);

    /// <summary>
    ///     设置是否启用 JSON 响应反序列化包装器
    /// </summary>
    /// <remarks>当配置了 <see cref="HttpClientOptions.JsonResponseWrapper" /> 时有效。</remarks>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseJsonResponseWrapper(bool enabled)
    {
        JsonResponseWrapperEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置启用 JSON 响应内容字符串的解包处理（双重序列化）
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseJsonResponseStringUnwrap() => UseJsonResponseStringUnwrap(true);

    /// <summary>
    ///     设置是否启用 JSON 响应内容字符串的解包处理（双重序列化）
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseJsonResponseStringUnwrap(bool enabled)
    {
        JsonResponseStringUnwrapEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置在构建最终请求 URL 的操作
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnUriBuilding(Action<UriBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnUriBuilding += configure;

        return this;
    }

    /// <summary>
    ///     设置在创建重定向构建器时进行额外配置的委托
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">自定义配置委托。第一个参数是当前（原始）构建器，第二个参数是即将用于重定向的克隆构建器。可以在此处移除敏感的 Header、修改 URL 等</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnRedirect(Action<HttpRequestBuilder, HttpRequestBuilder>? configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnRedirect += configure;

        return this;
    }

    /// <summary>
    ///     设置禁用框架的 Access Token 自动管理
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithoutTokenManagement()
    {
        SuppressTokenManagement = true;

        return this;
    }

    /// <summary>
    ///     设置获取 Access Token 时传递的自定义数据
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithAccessTokenData(string key, object? value)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return WithAccessTokenData(
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { key, value } });
    }

    /// <summary>
    ///     批量设置获取 Access Token 时传递的自定义数据
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="data">数据字典</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithAccessTokenData(IDictionary<string, object?> data)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(data);

        AccessTokenData ??= new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        // 遍历数据字典并添加或更新
        foreach (var (key, value) in data)
        {
            AccessTokenData[key] = value;
        }

        return this;
    }

    /// <summary>
    ///     设置 HTTP 完成选项
    /// </summary>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetCompletionOption(HttpCompletionOption? completionOption)
    {
        CompletionOption = completionOption;

        return this;
    }

    /// <summary>
    ///     移除 URL 地址末尾的 "/"
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder RemoveTrailingSlash() => RemoveTrailingSlash(true);

    /// <summary>
    ///     设置是否移除 URL 地址末尾的 "/"
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder RemoveTrailingSlash(bool enabled)
    {
        RemoveTrailingSlashEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置配额键
    /// </summary>
    /// <param name="key">配额标识，通常为接口路径或自定义名称</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetQuotaKey(string? key)
    {
        QuotaKey = key;

        return this;
    }

    /// <summary>
    ///     启用 ETag 缓存处理
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseETag() => UseETag(true);

    /// <summary>
    ///     设置是否启用 ETag 缓存处理
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseETag(bool enabled)
    {
        ETagEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置 SOAP 请求地址
    /// </summary>
    /// <param name="soapAction">SOAPAction 头的值</param>
    /// <param name="addQuotes">是否自动为该值添加双引号（"）包裹。SOAP 1.1 规范建议将非空值用双引号括起，但并非强制。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    // ReSharper disable once InconsistentNaming
    public HttpRequestBuilder SetSOAPAction(string soapAction, bool addQuotes = false)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(soapAction);

        return WithHeader("SOAPAction", addQuotes ? soapAction.AddQuotes() : soapAction, replace: true);
    }

    /// <summary>
    ///     设置自定义数据
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder WithData(string key, object? value)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return WithData(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { key, value } });
    }

    /// <summary>
    ///     批量设置自定义数据
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="data">数据字典</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithData(IDictionary<string, object?> data)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(data);

        Items ??= new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        // 遍历数据字典并添加或更新
        foreach (var (key, value) in data)
        {
            Items[key] = value;
        }

        return this;
    }


    /// <summary>
    ///     将一组标头合并到现有标头字典中
    /// </summary>
    /// <param name="existing">当前标头字典</param>
    /// <param name="newHeaders">待合并的新标头字典</param>
    /// <param name="escape">是否转义字符串</param>
    /// <param name="replace">是否替换已存在的请求标头</param>
    /// <returns>
    ///     <see cref="IDictionary{TKey,TValue}" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static IDictionary<string, List<string?>>? MergeHeaders(
        [NotNullIfNotNull(nameof(existing))] IDictionary<string, List<string?>>? existing,
        IDictionary<string, object?> newHeaders, bool escape, bool replace)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(newHeaders);

        // 空检查
        if (newHeaders is { Count: 0 })
        {
            return existing;
        }

        // 初始化字典
        existing ??= new Dictionary<string, List<string?>>(StringComparer.OrdinalIgnoreCase);
        var objectHeaders = new Dictionary<string, List<object?>>(StringComparer.OrdinalIgnoreCase);

        // 存在则合并否则添加
        objectHeaders.AddOrUpdate(
            existing.ToDictionary(u => u.Key, object? (u) => u.Value, StringComparer.OrdinalIgnoreCase), false);
        objectHeaders.AddOrUpdate(newHeaders, false, replace);

        return objectHeaders.ToDictionary(kvp => kvp.Key,
            kvp => kvp.Value.Select(u => u.ToInvariantCultureString()?.EscapeDataString(escape)).ToList(),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     合并请求内容
    /// </summary>
    /// <param name="existing">当前请求内容</param>
    /// <param name="incoming">传入请求内容</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    internal static object? MergeContent(object? existing, object? incoming)
    {
        switch (existing)
        {
            // 字符串 + 字符串：使用 & 拼接
            case string existingString when incoming is string incomingString:
                return $"{existingString}&{incomingString}";
            // StringBuilder + 字符串：追加字符串
            case StringBuilder existingSb when incoming is string s:
                return existingSb.Append(s);
            // NameValueCollection + NameValueCollection：追加元素
            case NameValueCollection existingNvc when incoming is NameValueCollection incomingNvc:
                foreach (string? key in incomingNvc)
                {
                    // 空检查
                    if (key is null)
                    {
                        continue;
                    }

                    foreach (var value in incomingNvc.GetValues(key)!)
                    {
                        existingNvc.Add(key, value);
                    }
                }

                return existingNvc;
            // NameValueCollection + IDictionary<string, object?>：追加元素
            case NameValueCollection existingNvc when incoming is IDictionary<string, object?> dict:
                foreach (var kvp in dict)
                {
                    existingNvc.Add(kvp.Key, kvp.Value?.ToString());
                }

                return existingNvc;
            // 字典 + 字典：合并键值对
            case IDictionary<string, object?> existingDictionary when
                incoming is IDictionary<string, object?> incomingDictionary:
                existingDictionary.AddOrUpdate(incomingDictionary);
                return existingDictionary;
            // 列表 + 集合：追加元素
            case IList existingList when incoming is IEnumerable incomingEnumerable and not string:
                {
                    foreach (var item in incomingEnumerable)
                    {
                        existingList.Add(item);
                    }

                    return existingList;
                }
            default:
                // 其他情况：覆盖
                return incoming;
        }
    }

    /// <summary>
    ///     释放可释放的对象集合
    /// </summary>
    internal void ReleaseDisposables()
    {
        // 空检查
        if (Disposables is null or { Count: 0 })
        {
            return;
        }

        // 逐条遍历进行释放
        foreach (var disposable in Disposables)
        {
            disposable.Dispose();
        }

        // 清空集合
        Disposables.Clear();
    }

    /// <summary>
    ///     添加 <see cref="StringContentForFormUrlEncodedContentProcessor" /> 处理器
    /// </summary>
    /// <remarks>仅首次调用时有效。</remarks>
    /// <param name="urlEncode">是否对表单数据进行 URL 编码，默认值为： <c>true</c></param>
    internal void AddStringContentForFormUrlEncodedContentProcessor(bool urlEncode = true)
    {
        // 检查是否已添加 StringContentForFormUrlEncodedContentProcessor 处理器
        if (_formUrlEncodedStringContentProcessorAdded)
        {
            return;
        }

        _formUrlEncodedStringContentProcessorAdded = true;
        AddHttpContentProcessors(() =>
            [new StringContentForFormUrlEncodedContentProcessor { UrlEncode = urlEncode }]);
    }

    /// <summary>
    ///     基于当前构建器创建一个用于重定向的新构建器
    /// </summary>
    /// <param name="redirectUri">重定向地址</param>
    /// <param name="redirectMethod">重定向方法</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpRequestBuilder CreateRedirectBuilder(Uri? redirectUri, HttpMethod redirectMethod)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(redirectMethod);

        // 克隆时排除不应继承的属性
        var redirectBuilder = Clone(nameof(Disposables), nameof(HttpClientPooling), nameof(PathParameters),
            nameof(ObjectPathParameters), nameof(QueryParameters), nameof(QueryParametersToRemove),
            nameof(ReplacedQueryParameterKeys), nameof(PathSegments), nameof(PathSegmentsToRemove));

        // 修改重定向特有的部分
        redirectBuilder.RequestUri = redirectUri;
        redirectBuilder.HttpMethod = redirectMethod;

        // 重定向时若请求方法为 GET 或 HEAD，则不应设置请求内容
        // Query 允许携带请求内容，参考文献：https://www.rfc-editor.org/info/rfc10008
        if (redirectMethod == HttpMethod.Get || redirectMethod == HttpMethod.Head)
        {
            redirectBuilder.RawContent = null;
            redirectBuilder.MultipartFormDataBuilder = null;
            redirectBuilder.ContentType = null;
            redirectBuilder.ContentEncoding = null;
            redirectBuilder.OmitContentType = false;
        }

        // 调用自定义的重定向配置委托
        OnRedirect?.Invoke(this, redirectBuilder);

        return redirectBuilder;
    }

    /// <summary>
    ///     设置请求方式
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpRequestBuilder SetHttpMethod(HttpMethod httpMethod)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);

        HttpMethod = httpMethod;

        return this;
    }

    /// <summary>
    ///     设置请求地址
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    internal HttpRequestBuilder SetRequestUri(Uri? requestUri)
    {
        RequestUri = requestUri;

        return this;
    }
}