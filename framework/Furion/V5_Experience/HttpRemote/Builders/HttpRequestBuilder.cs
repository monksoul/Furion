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
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Net.Mime;
using CacheControlHeaderValue = System.Net.Http.Headers.CacheControlHeaderValue;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     <see cref="StringContentForFormUrlEncodedContentProcessor" /> 实例
    /// </summary>
    internal static readonly Lazy<StringContentForFormUrlEncodedContentProcessor>
        _stringContentForFormUrlEncodedContentProcessor =
            new(() => new StringContentForFormUrlEncodedContentProcessor());

    /// <summary>
    ///     <inheritdoc cref="HttpRequestBuilder" />
    /// </summary>
    private HttpRequestBuilder()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="HttpRequestBuilder" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    internal HttpRequestBuilder(HttpMethod httpMethod, Uri? requestUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);

        HttpMethod = httpMethod;
        RequestUri = requestUri;
    }

    /// <summary>
    ///     构建 <see cref="HttpRequestMessage" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="httpContentProcessorFactory">
    ///     <see cref="IHttpContentProcessorFactory" />
    /// </param>
    /// <param name="clientBaseAddress">客户端基地址</param>
    /// <returns>
    ///     <see cref="HttpRequestMessage" />
    /// </returns>
    internal HttpRequestMessage Build(HttpRemoteOptions httpRemoteOptions,
        IHttpContentProcessorFactory httpContentProcessorFactory, Uri? clientBaseAddress)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);
        ArgumentNullException.ThrowIfNull(httpContentProcessorFactory);
        ArgumentNullException.ThrowIfNull(HttpMethod);

        // 调用 HttpRequestBuilder 统一配置器
        httpRemoteOptions.HttpRequestBuilderConfigurer?.Configure(this);

        // 构建最终的请求地址
        var finalRequestUri = BuildFinalRequestUri(clientBaseAddress, httpRemoteOptions);

        // 初始化 HttpRequestMessage 实例
        var httpRequestMessage = new HttpRequestMessage(HttpMethod, finalRequestUri);

        // 设置 HTTP 版本
        if (Version is not null)
        {
            httpRequestMessage.Version = Version;
        }

        // 启用性能优化
        EnablePerformanceOptimization(httpRequestMessage);

        // 追加请求标头
        AppendHeaders(httpRequestMessage, httpRemoteOptions.Configuration);

        // 追加 Cookies
        AppendCookies(httpRequestMessage, httpRemoteOptions.Configuration);

        // 移除 Cookies
        RemoveCookies(httpRequestMessage);

        // 移除请求标头
        RemoveHeaders(httpRequestMessage);

        // 构建并设置指定的 HttpRequestMessage 请求消息的内容
        BuildAndSetContent(httpRequestMessage, httpContentProcessorFactory, httpRemoteOptions);

        // 追加 HttpRequestMessage 请求属性集合
        AppendProperties(httpRequestMessage);

        return httpRequestMessage;
    }

    /// <summary>
    ///     构建最终的请求地址
    /// </summary>
    /// <param name="clientBaseAddress">客户端基地址</param>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal string BuildFinalRequestUri(Uri? clientBaseAddress, HttpRemoteOptions httpRemoteOptions)
    {
        // 获取配置
        var configuration = httpRemoteOptions.Configuration;

        // 替换路径或配置参数，处理非标准 HTTP URI 的应用场景（如 {url}），此时需优先解决路径或配置参数问题
        var processedRequestUri = RequestUri is null or { OriginalString: null }
            ? RequestUri
            : new Uri(ReplacePlaceholders(RequestUri.OriginalString, configuration)!, UriKind.RelativeOrAbsolute);

        // 替换 BaseAddress 路径或配置参数，处理非标准 HTTP URI 的应用场景（如 {url}），此时需优先解决路径或配置参数问题
        var processedBaseAddress = BaseAddress is null or { OriginalString: null }
            ? BaseAddress
            : new Uri(ReplacePlaceholders(BaseAddress.OriginalString, configuration)!, UriKind.RelativeOrAbsolute);

        // 初始化带局部 BaseAddress 的请求地址
        var requestUriWithBaseAddress = processedBaseAddress is null
            ? processedRequestUri
            : processedRequestUri is null
                ? processedBaseAddress
                : new Uri(Helpers.CombineUrl(processedBaseAddress.OriginalString, processedRequestUri.OriginalString),
                    UriKind.RelativeOrAbsolute);

        // 初始化带全局（客户端） BaseAddress 的请求地址
        var requestUriWithClientBaseAddress = clientBaseAddress is null
            ? requestUriWithBaseAddress
            : requestUriWithBaseAddress is null
                ? clientBaseAddress
                : new Uri(clientBaseAddress, requestUriWithBaseAddress);

        // 空检查
        ArgumentNullException.ThrowIfNull(requestUriWithClientBaseAddress);

        UriBuilder uriBuilder;
        try
        {
            // 初始化 UriBuilder 实例
            uriBuilder = new UriBuilder(requestUriWithClientBaseAddress);
        }
        catch (Exception)
        {
            // 检查基地址是否是绝对路径地址
            if (processedBaseAddress is not null && !processedBaseAddress.IsAbsoluteUri)
            {
                throw new InvalidOperationException(
                    $"The base address must be absolute. Please check the `.{nameof(SetBaseAddress)}()` method or the `[BaseAddress()]` attribute.");
            }

            throw;
        }

        // 追加路径片段
        AppendPathSegments(uriBuilder);

        // 追加查询参数
        AppendQueryParameters(uriBuilder, httpRemoteOptions.UrlParameterFormatter);

        // 追加片段标识符
        AppendFragment(uriBuilder);

        // 调用构建最终请求 URL 的操作
        UriBuilderConfigure?.Invoke(uriBuilder);

        // 替换路径或配置参数
        var finalRequestUri = ReplacePlaceholders(uriBuilder.Uri.ToString(), configuration)!;

        return finalRequestUri;
    }

    /// <summary>
    ///     追加路径片段
    /// </summary>
    /// <param name="uriBuilder">
    ///     <see cref="UriBuilder" />
    /// </param>
    internal void AppendPathSegments(UriBuilder uriBuilder)
    {
        // 空检查
        if ((PathSegments == null || PathSegments.Count == 0) &&
            (PathSegmentsToRemove == null || PathSegmentsToRemove.Count == 0))
        {
            return;
        }

        // 记录原路径是否以斜杠结尾（修复核心逻辑）
        var originalPath = uriBuilder.Uri.AbsolutePath;
        var endsWithSlash = originalPath.Length > 1 && originalPath.EndsWith('/');

        // 解析 URL 中的路径片段列表
        var pathSegments = uriBuilder.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // 追加并处理新路径片段
        var newPathSegments = pathSegments.Concat(PathSegments.ConcatIgnoreNull([])
            .Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.TrimStart('/').TrimEnd('/')));

        // 过滤需要移除的路径片段
        var filteredSegments = newPathSegments.WhereIf(PathSegmentsToRemove is { Count: > 0 },
            u => PathSegmentsToRemove?.Contains(u) == false).ToArray();

        // 构建最终路径
        if (filteredSegments.Length != 0)
        {
            uriBuilder.Path = $"/{string.Join('/', filteredSegments)}";

            // 恢复原路径的结尾斜杠（当存在路径片段时）
            if (endsWithSlash)
            {
                uriBuilder.Path += "/";
            }
        }
        // 没有路径片段时设置为根路径
        else
        {
            uriBuilder.Path = "/";
        }
    }

    /// <summary>
    ///     追加查询参数
    /// </summary>
    /// <param name="uriBuilder">
    ///     <see cref="UriBuilder" />
    /// </param>
    /// <param name="formatter">
    ///     <see cref="IUrlParameterFormatter" />
    /// </param>
    internal void AppendQueryParameters(UriBuilder uriBuilder, IUrlParameterFormatter? formatter)
    {
        // 空检查
        if ((QueryParameters is null || QueryParameters.Count == 0) &&
            (QueryParametersToRemove is null || QueryParametersToRemove.Count == 0))
        {
            return;
        }

        // 解析 URL 中的查询字符串为键值对列表
        var queryParameters = uriBuilder.Query.ParseFormatKeyValueString(['&'], '?');

        // 初始化 URL 参数格式化委托
        Func<object?, UrlFormattingContext, string?> format =
            formatter is not null ? formatter.Format : UrlParameterFormatter.DefaultFormatter;

        // 追加查询参数
        foreach (var (key, values) in QueryParameters.ConcatIgnoreNull([]))
        {
            queryParameters.AddRange(values.Select(value =>
                new KeyValuePair<string, string?>(key, format(value, new UrlFormattingContext(key)))));
        }

        // 构建最终的查询参数
        var finalQueryParameters = queryParameters
            // 过滤已标记为移除的查询参数键
            .WhereIf(QueryParametersToRemove is { Count: > 0 },
                u => QueryParametersToRemove?.TryGetValue(u.Key, out _) == false).Select(u => $"{u.Key}={u.Value}")
            .ToArray();

        // 调用查询参数排序委托
        var sortedQueryParameters = QueryParametersSorter?.Invoke(finalQueryParameters) ?? finalQueryParameters;

        // 构建查询字符串赋值给 UriBuilder 的 Query 属性
        uriBuilder.Query = sortedQueryParameters.Length == 0
            ? string.Empty
            : '?' + string.Join('&', sortedQueryParameters);
    }

    /// <summary>
    ///     追加片段标识符
    /// </summary>
    /// <param name="uriBuilder">
    ///     <see cref="UriBuilder" />
    /// </param>
    internal void AppendFragment(UriBuilder uriBuilder)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(Fragment))
        {
            return;
        }

        uriBuilder.Fragment = Fragment;
    }

    /// <summary>
    ///     替换路径或配置参数
    /// </summary>
    /// <param name="originalString">源请求地址</param>
    /// <param name="configuration">
    ///     <see cref="IConfiguration" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal string? ReplacePlaceholders(string? originalString, IConfiguration? configuration)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(originalString))
        {
            return originalString;
        }

        var newString = originalString;

        // 空检查
        if (!PathParameters.IsNullOrEmpty())
        {
            newString = newString.ReplacePlaceholders(PathParameters);
        }

        // 空检查
        if (!ObjectPathParameters.IsNullOrEmpty())
        {
            newString = ObjectPathParameters.Aggregate(newString,
                (current, objectPathParameter) =>
                    current.ReplacePlaceholders(objectPathParameter.Value, objectPathParameter.Key));
        }

        // 替换配置参数
        newString = newString.ReplacePlaceholders(configuration);

        return newString;
    }

    /// <summary>
    ///     追加请求标头
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="configuration">
    ///     <see cref="IConfiguration" />
    /// </param>
    internal void AppendHeaders(HttpRequestMessage httpRequestMessage, IConfiguration? configuration)
    {
        // 添加 Host 标头
        if (AutoSetHostHeaderEnabled)
        {
            httpRequestMessage.Headers.Host =
                $"{httpRequestMessage.RequestUri?.Host}{(httpRequestMessage.RequestUri?.IsDefaultPort != true ? $":{httpRequestMessage.RequestUri?.Port}" : string.Empty)}";
        }

        // 添加跟踪标识
        if (!string.IsNullOrWhiteSpace(TraceIdentifier))
        {
            httpRequestMessage.Headers.TryAddWithoutValidation(Constants.X_TRACE_ID_HEADER, TraceIdentifier);
        }

        // 添加身份认证
        AppendAuthentication(httpRequestMessage);

        // 设置禁用 HTTP 缓存
        if (DisableCacheEnabled)
        {
            httpRequestMessage.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true, NoStore = true, MustRevalidate = true
            };

            // 解决部分服务器不能识别 Cache-Control 标头问题
            httpRequestMessage.Headers.TryAddWithoutValidation("Pragma", "no-cache");
            httpRequestMessage.Headers.TryAddWithoutValidation("If-None-Match", "\"\"");
        }

        // 空检查
        if (Headers.IsNullOrEmpty())
        {
            return;
        }

        // 遍历请求标头集合并追加到 HttpRequestMessage.Headers 中
        foreach (var (key, values) in Headers)
        {
            // 替换 Referer 标头的 "{BASE_ADDRESS}" 模板字符串
            if (key.IsIn([HeaderNames.Referer], StringComparer.OrdinalIgnoreCase) &&
                values.FirstOrDefault() == Constants.REFERER_HEADER_BASE_ADDRESS_TEMPLATE)
            {
                httpRequestMessage.Headers.Referrer = new Uri(
                    $"{httpRequestMessage.RequestUri?.Scheme}://{httpRequestMessage.RequestUri?.Host}{(httpRequestMessage.RequestUri?.IsDefaultPort != true ? $":{httpRequestMessage.RequestUri?.Port}" : string.Empty)}",
                    UriKind.RelativeOrAbsolute);
                continue;
            }

            // 替换配置参数并追加
            httpRequestMessage.Headers.TryAddWithoutValidation(key,
                values.Select(v => ReplacePlaceholders(v, configuration)));
        }
    }

    /// <summary>
    ///     添加身份认证
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void AppendAuthentication(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (AuthenticationHeader is null)
        {
            return;
        }

        // 检查是否是 Digest 摘要认证
        if (AuthenticationHeader.Scheme != Constants.DIGEST_AUTHENTICATION_SCHEME)
        {
            httpRequestMessage.Headers.Authorization = AuthenticationHeader;

            return;
        }

        // 检查参数是否包含预设的 Digest 授权凭证
        const string separator = "|:|";
        if (AuthenticationHeader.Parameter?.Contains(separator) != true)
        {
            return;
        }

        // 分割预设的用户名和密码
        var parts = AuthenticationHeader.Parameter.Split(separator);

        // 获取 Digest 摘要认证授权凭证
        var digestCredentials =
            DigestCredentials.GetDigestCredentials(httpRequestMessage.RequestUri?.OriginalString, parts[0], parts[1],
                HttpMethod!);

        // 设置身份验证凭据请求授权标头
        httpRequestMessage.Headers.Authorization =
            new AuthenticationHeaderValue(Constants.DIGEST_AUTHENTICATION_SCHEME, digestCredentials);
    }

    /// <summary>
    ///     移除请求标头
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void RemoveHeaders(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (HeadersToRemove.IsNullOrEmpty())
        {
            return;
        }

        // 遍历请求标头集合并从 HttpRequestMessage.Headers 中移除
        foreach (var headerName in HeadersToRemove)
        {
            httpRequestMessage.Headers.Remove(headerName);
        }
    }

    /// <summary>
    ///     启用性能优化
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void EnablePerformanceOptimization(HttpRequestMessage httpRequestMessage)
    {
        if (!PerformanceOptimizationEnabled)
        {
            return;
        }

        // 设置 Accept 头，表示可以接受任何类型的内容
        httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        // 添加 Accept-Encoding 头，支持 gzip、deflate 以及 Brotli 压缩算法
        // 这样服务器可以根据情况选择最合适的压缩方式发送响应，从而减少传输的数据量
        httpRequestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        httpRequestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        httpRequestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

        // 设置 Connection 头为 keep-alive，允许重用 TCP 连接，避免每次请求都重新建立连接带来的开销
        httpRequestMessage.Headers.ConnectionClose = false;
    }

    /// <summary>
    ///     追加 Cookies
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="configuration">
    ///     <see cref="IConfiguration" />
    /// </param>
    internal void AppendCookies(HttpRequestMessage httpRequestMessage, IConfiguration? configuration)
    {
        // 空检查
        if (Cookies.IsNullOrEmpty())
        {
            return;
        }

        // 解决重复设置 Cookie 头问题（单元测试）
        // httpRequestMessage.Headers.Remove(HeaderNames.Cookie);

        // 替换配置参数并追加
        httpRequestMessage.Headers.TryAddWithoutValidation(HeaderNames.Cookie,
            string.Join("; ",
                Cookies.Select(u => $"{u.Key}={ReplacePlaceholders(u.Value, configuration)?.EscapeDataString(true)}")));
    }

    /// <summary>
    ///     移除 Cookies
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void RemoveCookies(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (CookiesToRemove.IsNullOrEmpty())
        {
            return;
        }

        // 获取已经设置的 Cookies
        if (!httpRequestMessage.Headers.TryGetValues(HeaderNames.Cookie, out var cookies))
        {
            return;
        }

        // 解析 Cookies 标头值
        var cookieList = CookieHeaderValue.ParseList(cookies.ToList());

        // 空检查
        if (cookieList.Count == 0)
        {
            return;
        }

        // 重新设置 Cookies
        httpRequestMessage.Headers.Remove(HeaderNames.Cookie);
        httpRequestMessage.Headers.TryAddWithoutValidation(HeaderNames.Cookie,
            // 过滤已标记为移除的 Cookie 键
            string.Join("; ", cookieList.WhereIf(CookiesToRemove is { Count: > 0 },
                    u => CookiesToRemove?.TryGetValue(u.Name.ToString(), out _) == false)
                .Select(u => $"{u.Name}={u.Value}")));
    }

    /// <summary>
    ///     构建并设置指定的 <see cref="HttpRequestMessage" /> 请求消息的内容
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="httpContentProcessorFactory">
    ///     <see cref="IHttpContentProcessorFactory" />
    /// </param>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    internal void BuildAndSetContent(HttpRequestMessage httpRequestMessage,
        IHttpContentProcessorFactory httpContentProcessorFactory, HttpRemoteOptions httpRemoteOptions)
    {
        // 获取自定义的 IHttpContentProcessor 集合
        var processors = HttpContentProcessorProviders?.SelectMany(u => u.Invoke()).ToArray();

        // 构建 MultipartFormDataContent 请求内容
        if (MultipartFormDataBuilder is not null)
        {
            ContentType = MediaTypeNames.Multipart.FormData;
            RawContent = MultipartFormDataBuilder.Build(httpRemoteOptions, httpContentProcessorFactory, processors);
        }

        // 检查是否设置了内容
        if (RawContent is null)
        {
            return;
        }

        // 设置默认的内容类型
        SetDefaultContentType(httpRemoteOptions.DefaultContentType);

        // 初始化 HttpContentProcessorContext 实例
        var processorContext = new HttpContentProcessorContext(RawContent, ContentType!, ContentEncoding)
        {
            HttpClientName = HttpClientName
        };

        // 构建 HttpContent 实例
        var httpContent = httpContentProcessorFactory.Build(processorContext, processors);

        // 是否在请求结束后自动释放流
        if (processorContext.CompletionDisposable is { } disposable)
        {
            AddDisposable(disposable);
        }

        // 空检查
        if (httpContent is null)
        {
            return;
        }

        // 检查是否移除默认的内容的 Content-Type，解决对接 Java 程序时可能出现失败问题
        if (OmitContentType)
        {
            httpContent.Headers.ContentType = null;
        }

        // 调用用于处理在设置请求消息的内容时的操作
        OnPreSetContent?.Invoke(httpContent);

        // 设置 HttpRequestMessage 请求消息的内容
        httpRequestMessage.Content = httpContent;
    }

    /// <summary>
    ///     追加 <see cref="HttpRequestMessage" /> 请求属性集合
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void AppendProperties(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (Properties.Count > 0)
        {
            // 注意：httpRequestMessage.Properties 已过时，使用 Options 替代
            httpRequestMessage.Options.AddOrUpdate(Properties);
        }

        // 检查是否禁用全局请求分析工具
        if (ProfilerDisabled)
        {
            httpRequestMessage.Options.AddOrUpdate(Constants.DISABLE_PROFILER_KEY, "TRUE");
        }

        // 检查是否显式启用或禁用 JSON 响应反序列化包装器
        if (JsonResponseWrapperEnabled is not null)
        {
            httpRequestMessage.Options.AddOrUpdate(Constants.ENABLE_JSON_RESPONSE_WRAPPER_KEY,
                JsonResponseWrapperEnabled.Value ? "TRUE" : "FALSE");
        }

        // 检查是否显式启用或禁用 JSON 响应内容字符串的解包处理（双重序列化）
        if (JsonResponseStringUnwrapEnabled is not null)
        {
            httpRequestMessage.Options.AddOrUpdate(Constants.ENABLE_JSON_RESPONSE_STRING_UNWRAP_KEY,
                JsonResponseStringUnwrapEnabled.Value ? "TRUE" : "FALSE");
        }

        // 检查是否配置了 HttpClient 实例名称
        if (HttpClientName is not null)
        {
            httpRequestMessage.Options.AddOrUpdate(Constants.HTTP_CLIENT_NAME, HttpClientName);
        }
    }

    /// <summary>
    ///     设置默认的内容类型
    /// </summary>
    /// <param name="defaultContentType">默认请求内容类型</param>
    internal void SetDefaultContentType(string? defaultContentType)
    {
        // 空检查
        if (!string.IsNullOrWhiteSpace(ContentType))
        {
            return;
        }

        ContentType = Helpers.GetContentTypeOrDefault(RawContent, defaultContentType);
    }
}