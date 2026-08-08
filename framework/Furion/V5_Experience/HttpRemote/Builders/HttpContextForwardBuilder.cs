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

using Furion.AspNetCore.Extensions;
using Furion.Extensions;
using Furion.HttpRemote.Extensions;
using Furion.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Mime;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpContextForwardBuilder" /> 构建上下文
/// </summary>
/// <param name="HttpContext">
///     <see cref="HttpContext" />
/// </param>
/// <param name="Builder">
///     <see cref="HttpRequestBuilder" />
/// </param>
/// <param name="ForwardOptions">
///     <see cref="HttpContextForwardOptions" />
/// </param>
public sealed record HttpForwardBuildContext(
    HttpContext HttpContext,
    HttpRequestBuilder Builder,
    HttpContextForwardOptions ForwardOptions);

/// <summary>
///     <see cref="HttpContext" /> 转发构建器
/// </summary>
public sealed class HttpContextForwardBuilder
{
    /// <summary>
    ///     <see cref="IActionResultContentConverter" /> 实例
    /// </summary>
    internal static readonly Lazy<IActionResultContentConverter> _actionResultContentConverterInstance =
        new(() => new IActionResultContentConverter());

    /// <summary>
    ///     忽略在转发时需要跳过的请求标头列表
    /// </summary>
    internal static readonly HashSet<string> _ignoreRequestHeaders =
    [
        Constants.X_FORWARD_TO_HEADER, "Host", "Content-Length"
    ];

    /// <summary>
    ///     <inheritdoc cref="HttpContextForwardBuilder" />
    /// </summary>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址。</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpContextForwardBuilder(HttpMethod httpMethod, Uri? requestUri = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);

        HttpMethod = httpMethod;
        RequestUri = requestUri;
    }

    /// <summary>
    ///     转发地址
    /// </summary>
    public Uri? RequestUri { get; }

    /// <summary>
    ///     转发方式
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="httpContextForwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpRequestBuilder Build(HttpContext httpContext, Action<HttpRequestBuilder>? configure = null,
        HttpContextForwardOptions? httpContextForwardOptions = null) =>
        AsyncUtility.RunSync(() => BuildAsync(httpContext, configure, httpContextForwardOptions));

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="httpContextForwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal async Task<HttpRequestBuilder> BuildAsync(HttpContext httpContext,
        Action<HttpRequestBuilder>? configure = null,
        HttpContextForwardOptions? httpContextForwardOptions = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpContext);

        // 解析 HttpContextForwardOptions 实例
        var forwardOptions = httpContext.ResolveForwardOptions(httpContextForwardOptions);

        // 解析目标地址
        var targetUri = ResolveTargetUri(httpContext, forwardOptions);

        // 初始化 HttpRequestBuilder 实例
        var httpRequestBuilder = HttpRequestBuilder.Create(HttpMethod, targetUri)
            .AddHttpContentConverters(() => [_actionResultContentConverterInstance.Value]).DisableCache();

        // 初始化 HttpForwardBuildContext 实例
        var httpForwardBuildContext = new HttpForwardBuildContext(httpContext, httpRequestBuilder, forwardOptions);

        // 复制查询参数和路由参数
        CopyQueryAndRouteValues(httpForwardBuildContext);

        // 复制请求标头
        CopyHeaders(httpForwardBuildContext);

        // 复制请求内容
        await CopyBodyAsync(httpForwardBuildContext);

        // 调用自定义配置委托
        configure?.Invoke(httpRequestBuilder);

        return httpRequestBuilder;
    }

    /// <summary>
    ///     获取目标地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Uri" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal Uri? ResolveTargetUri(HttpContext httpContext, HttpContextForwardOptions forwardOptions)
    {
        // 空检查
        if (RequestUri is not null)
        {
            // 验证目标 URI 的主机是否在允许的白名单中
            ValidateHost(RequestUri, forwardOptions);

            return RequestUri;
        }

        // 尝试从请求标头 X-Forward-To 中获取目标地址
        var targetUrl = httpContext.Request.Headers[Constants.X_FORWARD_TO_HEADER].ToString();

        // 空检查
        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            return null;
        }

        // 初始化 Uri 实例
        var uri = new Uri(targetUrl, UriKind.RelativeOrAbsolute);

        // 检查是否是绝对 URI 地址
        if (!uri.IsAbsoluteUri)
        {
            throw new InvalidOperationException(
                "The target URL must be an absolute URI (e.g., https://baiqian.com/about). Relative paths are not supported.");
        }

        // 验证目标 URI 的主机是否在允许的白名单中
        ValidateHost(uri, forwardOptions);

        return uri;
    }

    /// <summary>
    ///     复制查询参数和路由参数
    /// </summary>
    /// <param name="buildContext">
    ///     <see cref="HttpForwardBuildContext" />
    /// </param>
    internal static void CopyQueryAndRouteValues(HttpForwardBuildContext buildContext)
    {
        // 解构上下文信息
        var (httpContext, httpRequestBuilder, forwardOptions) = buildContext;

        // 获取查询参数集合
        var queryValues = httpContext.Request.Query.ToArray();

        // 空检查
        if (queryValues.Length > 0)
        {
            // 检查是否转发查询参数（URL 参数）
            if (forwardOptions.WithQueryParameters)
            {
                // 初始化忽略在转发时需要跳过的查询参数（URL 参数）列表
                var ignoreQueryParameters = forwardOptions.IgnoreQueryParameters ?? [];

                // 将查询参数添加到查询参数集合中
                httpRequestBuilder.WithQueryParameters(queryValues.Where(u =>
                    !u.Key.IsIn(ignoreQueryParameters, StringComparer.OrdinalIgnoreCase)));
            }

            // 将查询参数添加到路径参数集合中
            httpRequestBuilder.WithPathParameters(queryValues);
        }

        // 获取路由参数（[FromRoute]）集合
        var routeValues = httpContext.Request.RouteValues;

        // 空检查
        if (routeValues.Count > 0)
        {
            // 将路由参数添加到路径参数集合中
            httpRequestBuilder.WithPathParameters(routeValues);
        }
    }

    /// <summary>
    ///     复制请求标头
    /// </summary>
    /// <param name="buildContext">
    ///     <see cref="HttpForwardBuildContext" />
    /// </param>
    internal static void CopyHeaders(HttpForwardBuildContext buildContext)
    {
        // 解构上下文信息
        var (httpContext, httpRequestBuilder, forwardOptions) = buildContext;

        // 获取 HttpRequest 实例
        var httpRequest = httpContext.Request;

        // 添加原始请求地址标头
        httpRequestBuilder.WithHeader(Constants.X_ORIGINAL_URL_HEADER, httpRequest.GetFullRequestUrl(), replace: true);

        // 检查是否转发请求标头
        if (!forwardOptions.WithRequestHeaders)
        {
            return;
        }

        // 初始化忽略在转发时需要跳过的请求标头列表
        var ignoreRequestHeaders = _ignoreRequestHeaders.ConcatIgnoreNull(forwardOptions.IgnoreRequestHeaders)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        // 忽略特定请求标头列表
        httpRequestBuilder.WithHeaders(
            httpRequest.Headers.Where(u => !u.Key.IsIn(ignoreRequestHeaders, StringComparer.OrdinalIgnoreCase)),
            replace: true);

        // 检查是否需要重新设置 Host 请求标头
        httpRequestBuilder.AutoSetHostHeader(forwardOptions.ResetHostRequestHeader);
    }

    /// <summary>
    ///     复制请求内容
    /// </summary>
    /// <param name="buildContext">
    ///     <see cref="HttpForwardBuildContext" />
    /// </param>
    internal static async Task CopyBodyAsync(HttpForwardBuildContext buildContext)
    {
        // 解构上下文信息
        var (httpContext, httpRequestBuilder, _) = buildContext;

        // 获取 HttpRequest 实例
        var httpRequest = httpContext.Request;

        // 检查是否包含请求内容或不可读
        if (httpRequest.ContentLength == 0 || !httpRequest.Body.CanRead)
        {
            return;
        }

        // 获取原始内容类型
        var rawContentType = httpRequest.ContentType;

        // 检查是否包含请求内容
        if (httpRequest.ContentLength is null && string.IsNullOrEmpty(rawContentType))
        {
            return;
        }

        // 空检查
        if (string.IsNullOrEmpty(rawContentType))
        {
            rawContentType = MediaTypeNames.Application.Octet;
        }

        // 解析原始内容类型
        var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(rawContentType);

        // 获取内容类型
        var contentType = mediaTypeHeaderValue.MediaType;

        // 空检查
        ArgumentNullException.ThrowIfNull(contentType);

        // 读取 HttpContext 请求内容流
        var bodyStream = ReadBody(httpContext);

        // 检查请求内容类型是否为 multipart/form-data
        if (!contentType.IsIn([MediaTypeNames.Multipart.FormData], StringComparer.OrdinalIgnoreCase))
        {
            // 复制非多部分表单内容
            CopyNonMultipartFormData(bodyStream, contentType, httpRequestBuilder);
        }
        else
        {
            // 复制多部分表单内容
            await CopyMultipartFormDataAsync(bodyStream, rawContentType, httpRequestBuilder,
                httpContext.RequestAborted);
        }

        // 将请求内容流的位置重置回起始位置
        if (bodyStream.CanSeek)
        {
            bodyStream.Position = 0;
        }
    }

    /// <summary>
    ///     复制非多部分表单内容
    /// </summary>
    /// <param name="bodyStream">
    ///     <see cref="Stream" />
    /// </param>
    /// <param name="contentType">内容类型</param>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    internal static void CopyNonMultipartFormData(Stream bodyStream, string contentType,
        HttpRequestBuilder httpRequestBuilder)
    {
        // 初始化 StreamContent 实例
        var streamContent = new StreamContent(bodyStream);

        // 设置请求内容
        httpRequestBuilder.SetContent(streamContent, contentType);
    }

    /// <summary>
    ///     复制多部分表单内容
    /// </summary>
    /// <param name="bodyStream">
    ///     <see cref="Stream" />
    /// </param>
    /// <param name="rawContentType">原始内容类型</param>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static async Task CopyMultipartFormDataAsync(Stream bodyStream, string rawContentType,
        HttpRequestBuilder httpRequestBuilder, CancellationToken cancellationToken)
    {
        // 获取多部分表单内容的边界；注意：这里可能出现前后双引号问题
        var contentTypeHeader = Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(rawContentType);
        var boundary = contentTypeHeader.Boundary.Value?.TrimStart('"').TrimEnd('"');

        // 空检查
        if (string.IsNullOrEmpty(boundary))
        {
            throw new InvalidOperationException("Multipart boundary is missing or invalid in the Content-Type header.");
        }

        // 初始化 HttpMultipartFormDataBuilder 实例
        var httpMultipartFormDataBuilder =
            new HttpMultipartFormDataBuilder(httpRequestBuilder)
            {
                Boundary = boundary,
                // 同步 HttpRequestBuilder.OmitContentType 属性，解决请求转发时无法控制 Content-Type 头部传递的问题
                OmitContentType = httpRequestBuilder.OmitContentType
            };

        // 初始化 MultipartReader 实例
        var multipartReader = new MultipartReader(boundary, bodyStream);

        // 读取下一个 MultipartSection
        while (await multipartReader.ReadNextSectionAsync(cancellationToken) is { } multipartSection)
        {
            // 检查当前节是否为文件节
            var fileMultipartSection = multipartSection.AsFileSection();
            if (fileMultipartSection is not null)
            {
                // 复制多部分表单内容文件节内容
                await CopyFileMultipartSectionAsync(fileMultipartSection, httpMultipartFormDataBuilder,
                    cancellationToken);
            }
            else
            {
                // 复制多部分表单内容文本节内容
                await CopyTextMultipartSectionAsync(multipartSection, httpMultipartFormDataBuilder, cancellationToken);
            }
        }

        // 设置多部分表单内容
        httpRequestBuilder.SetMultipartContent(httpMultipartFormDataBuilder);
    }

    /// <summary>
    ///     复制多部分表单内容文本节内容
    /// </summary>
    /// <param name="multipartSection">
    ///     <see cref="MultipartSection" />
    /// </param>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task CopyTextMultipartSectionAsync(MultipartSection multipartSection,
        HttpMultipartFormDataBuilder httpMultipartFormDataBuilder, CancellationToken cancellationToken)
    {
        // 获取 ContentDispositionHeaderValue 实例
        var contentDispositionHeaderValue = multipartSection.GetContentDispositionHeader();

        // 获取表单名称
        var name = contentDispositionHeaderValue?.Name.Value;

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 读取文本
        var text = await multipartSection.ReadAsStringAsync(cancellationToken);

        // 添加文本
        httpMultipartFormDataBuilder.AddText(text, name);
    }

    /// <summary>
    ///     复制多部分表单内容文件节内容
    /// </summary>
    /// <param name="fileMultipartSection">
    ///     <see cref="FileMultipartSection" />
    /// </param>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task CopyFileMultipartSectionAsync(FileMultipartSection fileMultipartSection,
        HttpMultipartFormDataBuilder httpMultipartFormDataBuilder, CancellationToken cancellationToken)
    {
        // 使用文件缓冲流，自动在内存和磁盘间切换
        var bufferingStream =
            new FileBufferingReadStream(fileMultipartSection.Section.Body, 64 * 1024);

        // 将节内容读入缓冲流
        await bufferingStream.DrainAsync(cancellationToken);

        // 将缓冲流的位置重置回起始位置
        bufferingStream.Position = 0;

        // 添加文件流
        httpMultipartFormDataBuilder.AddStream(bufferingStream, fileMultipartSection.Name,
            fileMultipartSection.FileName,
            fileMultipartSection.Section.ContentType, disposeResourcesOnRequestCompletion: true);
    }

    /// <summary>
    ///     读取 <see cref="HttpContext" /> 请求内容流
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static Stream ReadBody(HttpContext httpContext)
    {
        // 获取请求内容流
        var body = httpContext.Request.Body;

        // 检查请求内容流是否支持查找操作
        if (!body.CanSeek)
        {
            throw new InvalidOperationException(
                "Please ensure that the `app.UseEnableBuffering()` middleware is registered.");
        }

        // 将请求内容流的位置重置回起始位置
        body.Position = 0;

        return body;
    }

    /// <summary>
    ///     验证目标 URI 的主机是否在允许的白名单中
    /// </summary>
    /// <param name="uri">要验证的目标 URI</param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void ValidateHost(Uri uri, HttpContextForwardOptions forwardOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(uri);

        // 获取配置的允许主机列表
        // 如果白名单未配置或为空，则拒绝所有转发请求
        if (forwardOptions.AllowedHosts is not { Count: > 0 } allowedHosts)
        {
            throw new InvalidOperationException(
                "No allowed hosts have been configured for request forwarding. To enable forwarding, add target hosts to HttpContextForwardOptions.AllowedHosts, or include `*` to allow all hosts (not recommended due to SSRF risk).");
        }

        // 如果白名单中包含 "*"，表示用户明确允许转发到任意主机
        if (allowedHosts.Contains("*", StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        // 获取目标 URI 的实际主机名和端口
        var actualHost = uri.Host;
        var actualPort = uri.Port;

        // 遍历并逐一匹配白名单中的每一项
        foreach (var entry in allowedHosts)
        {
            // 分离可能的协议前缀和剩余的主机端口部分
            string? requiredScheme = null;
            string hostPortPart;

            // 检查是否包含协议
            if (entry.Contains("://", StringComparison.OrdinalIgnoreCase))
            {
                var parts = entry.Split("://", 2);

                requiredScheme = parts[0];
                hostPortPart = parts[1];
            }
            else
            {
                hostPortPart = entry;
            }

            // 如果白名单项指定了协议，则必须与目标 URI 的协议一致
            if (requiredScheme is not null && !uri.Scheme.Equals(requiredScheme, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // 从 hostPortPart 中分离主机和端口部分
            string allowedHost;
            string? allowedPortStr = null;

            // 检查是否是 IPv6 地址（以方括号包裹，如 [::1] 或 [::1]:8080）
            if (hostPortPart.StartsWith('['))
            {
                // 查找闭合方括号
                var closeBracketIndex = hostPortPart.IndexOf(']');

                // 格式不合法则跳过
                if (closeBracketIndex < 0)
                {
                    continue;
                }

                // 保留方括号
                allowedHost = hostPortPart[..(closeBracketIndex + 1)];

                // 检查闭合方括号后是否跟着端口
                if (closeBracketIndex + 1 < hostPortPart.Length && hostPortPart[closeBracketIndex + 1] == ':')
                {
                    allowedPortStr = hostPortPart[(closeBracketIndex + 2)..];
                }
            }
            // IPv4 或域名
            else
            {
                // 检查是否存在冒号
                var colonIndex = hostPortPart.LastIndexOf(':');
                if (colonIndex > 0)
                {
                    allowedHost = hostPortPart[..colonIndex];
                    allowedPortStr = hostPortPart[(colonIndex + 1)..];
                }
                else
                {
                    allowedHost = hostPortPart;
                }
            }

            // 检查主机名是否完全匹配
            if (!actualHost.Equals(allowedHost, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // 检查端口是否匹配
            switch (allowedPortStr)
            {
                case null:
                    {
                        // 检查是否是默认端口
                        if (uri.IsDefaultPort)
                        {
                            return;
                        }

                        break;
                    }
                case "*":
                    return;
                default:
                    {
                        // 解析端口并检查是否匹配
                        if (int.TryParse(allowedPortStr, out var allowedPort) && actualPort == allowedPort)
                        {
                            return;
                        }

                        break;
                    }
            }
        }

        throw new InvalidOperationException(
            $"The target host '{actualHost}:{actualPort}' is not in the allowed forwarding list.");
    }
}