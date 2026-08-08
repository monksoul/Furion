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

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
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
    /// <exception cref="ArgumentNullException"></exception>
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
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpRequestMessage Build(HttpRemoteOptions httpRemoteOptions,
        IHttpContentProcessorFactory httpContentProcessorFactory, Uri? clientBaseAddress)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);
        ArgumentNullException.ThrowIfNull(httpContentProcessorFactory);
        ArgumentNullException.ThrowIfNull(HttpMethod);

        // 调用 HttpRequestBuilder 统一配置器
        httpRemoteOptions.HttpRequestBuilderConfigurator?.Configure(this);

        // 构建最终的请求地址
        var finalRequestUri = BuildFinalRequestUri(clientBaseAddress, httpRemoteOptions);

        // 初始化 HttpRequestMessage 实例
        var httpRequestMessage = new HttpRequestMessage(HttpMethod, finalRequestUri);

        // 设置 HTTP 版本
        if (Version is not null)
        {
            httpRequestMessage.Version = Version;
        }

        // 启用标准请求标头
        EnableStandardRequestHeaders(httpRequestMessage);

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
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal string BuildFinalRequestUri(Uri? clientBaseAddress, HttpRemoteOptions httpRemoteOptions)
    {
        // 获取配置
        var configuration = httpRemoteOptions.Configuration;

        // 替换路径或配置参数，处理非标准 HTTP URI 的应用场景（如 {url}）
        var processedRequestUri = RequestUri is null or { OriginalString: null }
            ? RequestUri
            : new Uri(ReplacePlaceholders(RequestUri.OriginalString, configuration)!, UriKind.RelativeOrAbsolute);

        // 替换 BaseAddress 路径或配置参数，处理非标准 HTTP URI 的应用场景（如 {url}）
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

        // 初始化带全局（客户端）BaseAddress 的请求地址
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
        OnUriBuilding?.Invoke(uriBuilder);

        // 替换路径或配置参数
        var finalRequestUri = ReplacePlaceholders(uriBuilder.Uri.ToString(), configuration)!;

        // 处理末尾斜杠
        return RemoveTrailingSlashEnabled && finalRequestUri.EndsWith('/')
            ? finalRequestUri.TrimEnd('/')
            : finalRequestUri;
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
        if (PathSegments is null or { Count: 0 } && PathSegmentsToRemove is null or { Count: 0 })
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
    /// <param name="urlParameterFormatter">
    ///     <see cref="IUrlParameterFormatter" />
    /// </param>
    internal void AppendQueryParameters(UriBuilder uriBuilder, IUrlParameterFormatter? urlParameterFormatter)
    {
        // 空检查
        if (QueryParameters is null or { Count: 0 } && QueryParametersToRemove is null or { Count: 0 })
        {
            return;
        }

        // 解析 URL 中的查询字符串为键值对列表
        var originalPairs = uriBuilder.Query.ParseFormatKeyValueString(['&'], '?');

        // 从原始 URL 参数中移除需要排除的键
        if (QueryParametersToRemove is { Count: > 0 })
        {
            originalPairs.RemoveAll(u => QueryParametersToRemove.Contains(u.Key));
        }

        // 处理需要替换原始 URL 地址上的键
        if (ReplacedQueryParameterKeys is { Count: > 0 })
        {
            var handledKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var newOriginalPairs = new List<KeyValuePair<string, string?>>();

            // 遍历所有原始参数的键
            foreach (var pair in originalPairs)
            {
                // 检查是否需要替换（保留原有顺序）
                if (ReplacedQueryParameterKeys.Contains(pair.Key))
                {
                    // 检查是否已替换过
                    if (!handledKeys.Add(pair.Key))
                    {
                        continue;
                    }

                    // 对每个替换键只保留第一次出现，并用新值替换
                    if (QueryParameters is not null && QueryParameters.TryGetValue(pair.Key, out var vals) &&
                        vals.Count > 0)
                    {
                        newOriginalPairs.Add(new KeyValuePair<string, string?>(pair.Key, vals[0]?.ToString()));
                    }
                    else
                    {
                        newOriginalPairs.Add(pair);
                    }
                }
                else
                {
                    newOriginalPairs.Add(pair);
                }
            }

            originalPairs = newOriginalPairs;

            // 双重检查
            foreach (var key in handledKeys)
            {
                QueryParameters?.Remove(key);
            }
        }

        // 初始化 URL 参数格式化程序实例
        var formatter = urlParameterFormatter ?? new UrlParameterFormatter();

        // 将原始 URL 参数按参数名分组
        var originalGroups = originalPairs.GroupBy(u => u.Key).Select(g => (g.Key, g.Select(object? (u) => u.Value)));

        // 初始化收集所有最终键值对实例
        var allPairs = new List<KeyValuePair<string, string?>>();

        /*
         * 原始 URL 参数可能以 key[]、key[index]、key[][] 等格式存在，
         * 合并原始参数与用户参数会破坏原始分组结构，导致最终 URL 不正确，
         * 分开两个 foreach 循环，分别处理原始参数和用户参数，确保各自结构独立。
         */

        // 处理原始 URL 参数
        foreach (var (key, values) in originalGroups)
        {
            // 格式化后追加
            allPairs.AddRange(formatter.Format(new UrlFormattingContext(key, HttpClientName) { IsOriginal = true }, key,
                values) ?? []);
        }

        // 追加用户设置的查询参数
        foreach (var (key, values) in QueryParameters.ConcatIgnoreNull([]))
        {
            // 跳过需要移除的键
            if (QueryParametersToRemove?.Contains(key) == true)
            {
                continue;
            }

            // 格式化后追加
            allPairs.AddRange(formatter.Format(new UrlFormattingContext(key, HttpClientName), key, values) ?? []);
        }

        // 调用查询参数排序委托
        var sortedPairs = QueryParametersSorter?.Invoke(allPairs) ?? allPairs;

        // 构建查询字符串
        var queryString = string.Join("&", sortedPairs.Select(u => $"{u.Key}={u.Value}"));

        // 将查询字符串赋值给 UriBuilder 的 Query 属性
        uriBuilder.Query = queryString;
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
        if (PathParameters is not null)
        {
            newString = newString.ReplacePlaceholders(PathParameters);
        }

        // 空检查
        if (ObjectPathParameters is not null)
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
        if (Headers is null or { Count: 0 })
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
        if (HeadersToRemove is null or { Count: 0 })
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
    ///     启用标准请求标头
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void EnableStandardRequestHeaders(HttpRequestMessage httpRequestMessage)
    {
        // 检查是否启用标准请求标头
        if (!StandardRequestHeadersEnabled)
        {
            return;
        }

        // 设置 Accept 头 (避免被 WAF 拦截)
        if (httpRequestMessage.Headers.Accept.Count == 0)
        {
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain", 0.9));
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
        }

        // 显式声明 Keep-Alive
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
        if (Cookies is null or { Count: 0 })
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
        if (CookiesToRemove is null or { Count: 0 })
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

        // 是否在请求结束后自动释放资源
        if (processorContext.CompletionDisposables is { } disposables)
        {
            AddDisposables(disposables);
            processorContext.CompletionDisposables.Clear();
        }

        switch (httpContent)
        {
            // 空检查
            case null:
                return;
            // 检查是否是 CompositeHttpContent 实例
            case CompositeHttpContent compositeHttpContent:
                {
                    // 获取 HttpContent 集合
                    var httpContents = compositeHttpContent.Contents;

                    switch (httpContents.Count)
                    {
                        case 0:
                            return;
                        case 1:
                            httpContent = httpContents[0];
                            break;
                        default:
                            throw new InvalidOperationException(
                                "Multiple content items are not allowed as top-level request content.");
                    }

                    break;
                }
        }

        // 追加请求内容标头
        if (ContentHeaders is { Count: > 0 })
        {
            // 遍历请求内容标头集合并追加到 HttpContent.Headers 中
            foreach (var (key, values) in ContentHeaders)
            {
                // 避免重复添加 Content-Type
                if (key.Equals(HeaderNames.ContentType, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // 替换配置参数并追加
                httpContent.Headers.TryAddWithoutValidation(key,
                    values.Select(v => ReplacePlaceholders(v, httpRemoteOptions.Configuration)));
            }
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
        if (Properties is { Count: > 0 })
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

        // 尝试解析请求内容标头集合中的 "Content-Type"  键
        var contentType = ContentHeaders?[HeaderNames.ContentType].FirstOrDefault();

        // 空检查
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            ContentType = contentType;

            return;
        }

        ContentType = Helpers.GetContentTypeOrDefault(RawContent, defaultContentType);
    }
}