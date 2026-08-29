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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Furion.HttpRemote.Extensions;

/// <summary>
///     <see cref="HttpContext" /> 扩展类
/// </summary>
public static partial class HttpContextExtensions
{
    /// <summary>
    ///     忽略在转发时需要跳过的响应标头列表
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             <term>Content-Type: </term>
    ///             <description>
    ///                 非标准的 <c>Content-Type</c> 值（例如 <c>text/plain; charset=utf-8</c>
    ///                 ）可能会导致“No output formatter was found for content types 'text/plain; charset=utf-8, text/plain;
    ///                 charset=utf-8' to write the response.”错误。忽略此标头以防止此类错误。
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Transfer-Encoding: </term>
    ///             <description>当响应标头包含 <c>Transfer-Encoding: chunked</c> 时，可能导致响应处理过程无限期挂起。忽略此标头可避免该问题。</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    internal static readonly HashSet<string> _ignoreResponseHeaders =
    [
        "Content-Type", "Connection", "Transfer-Encoding", "Keep-Alive", "Upgrade", "Proxy-Connection"
    ];

    /// <summary>
    ///     解析 <see cref="HttpContextForwardOptions" /> 实例
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpContextForwardOptions" />
    /// </returns>
    internal static HttpContextForwardOptions
        ResolveForwardOptions(this HttpContext httpContext, HttpContextForwardOptions? forwardOptions) =>
        forwardOptions ??
        httpContext.RequestServices.GetService<IOptionsMonitor<HttpContextForwardOptions>>()?.CurrentValue ??
        new HttpContextForwardOptions();

    /// <summary>
    ///     将 <see cref="HttpResponse" /> 配置为 Server-Sent Events 标准流式响应格式
    /// </summary>
    /// <param name="httpResponse">
    ///     <see cref="HttpResponse" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void EnableServerSentEvents(this HttpResponse httpResponse)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponse);

        // 设置标准的 SSE Content-Type
        httpResponse.ContentType = "text/event-stream; charset=utf-8";

        // 禁用缓存，确保客户端每次都能拿到最新数据
        httpResponse.Headers.CacheControl = "no-cache";

        // 禁用 Nginx 等反向代理的缓冲，确保数据实时推送到客户端
        httpResponse.Headers["X-Accel-Buffering"] = "no";
    }

    /// <summary>
    ///     向客户端写入一条消息并立即刷新响应流
    /// </summary>
    /// <param name="httpResponse">
    ///     <see cref="HttpResponse" />
    /// </param>
    /// <param name="text">消息文本</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task WriteAndFlushAsync(this HttpResponse httpResponse, string? text,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponse);

        // 空检查
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        await httpResponse.WriteAsync(text, cancellationToken);
        await httpResponse.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    public static HttpResponseMessage? Forward(this HttpContext? httpContext, string? requestUri = null,
        Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.Forward(Helpers.ParseHttpMethod(httpContext?.Request.Method),
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure,
            completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    public static HttpResponseMessage? Forward(this HttpContext? httpContext, HttpMethod httpMethod,
        string? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.Forward(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure,
            completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    public static HttpResponseMessage? Forward(this HttpContext? httpContext, Uri? requestUri = null,
        Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.Forward(Helpers.ParseHttpMethod(httpContext?.Request.Method), requestUri,
            configure, completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    public static HttpResponseMessage? Forward(this HttpContext? httpContext, HttpMethod httpMethod,
        Uri? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(httpMethod);

        // 构建 HttpRequestBuilder 实例
        var httpRequestBuilder = new HttpContextForwardBuilder(httpMethod, requestUri)
            .Build(httpContext, configure, forwardOptions);

        // 获取 IHttpRemoteService 实例
        var httpRemoteService = httpContext.RequestServices.GetRequiredService<IHttpRemoteService>();

        // 发送 HTTP 远程请求
        var httpResponseMessage =
            httpRemoteService.Send(httpRequestBuilder, completionOption, httpContext.RequestAborted);

        // 根据配置选项将 HttpResponseMessage 信息转发到 HttpContext 中
        ForwardResponseMessage(httpContext, httpResponseMessage, httpContext.ResolveForwardOptions(forwardOptions));

        return httpResponseMessage;
    }

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    public static Task<HttpResponseMessage?> ForwardAsync(this HttpContext? httpContext, string? requestUri = null,
        Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.ForwardAsync(Helpers.ParseHttpMethod(httpContext?.Request.Method),
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure,
            completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    public static Task<HttpResponseMessage?> ForwardAsync(this HttpContext? httpContext, HttpMethod httpMethod,
        string? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.ForwardAsync(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure,
            completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    public static Task<HttpResponseMessage?> ForwardAsync(this HttpContext? httpContext, Uri? requestUri = null,
        Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.ForwardAsync(Helpers.ParseHttpMethod(httpContext?.Request.Method), requestUri,
            configure, completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    public static async Task<HttpResponseMessage?> ForwardAsync(this HttpContext? httpContext, HttpMethod httpMethod,
        Uri? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(httpMethod);

        // 构建 HttpRequestBuilder 实例
        var httpRequestBuilder = await new HttpContextForwardBuilder(httpMethod, requestUri)
            .BuildAsync(httpContext, configure, forwardOptions);

        // 获取 IHttpRemoteService 实例
        var httpRemoteService = httpContext.RequestServices.GetRequiredService<IHttpRemoteService>();

        // 发送 HTTP 远程请求
        var httpResponseMessage = await httpRemoteService.SendAsync(httpRequestBuilder, completionOption,
            httpContext.RequestAborted);

        // 根据配置选项将 HttpResponseMessage 信息转发到 HttpContext 中
        ForwardResponseMessage(httpContext, httpResponseMessage, httpContext.ResolveForwardOptions(forwardOptions));

        return httpResponseMessage;
    }

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    public static HttpRemoteResult<TResult>? Forward<TResult>(this HttpContext? httpContext, string? requestUri = null,
        Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.Forward<TResult>(Helpers.ParseHttpMethod(httpContext?.Request.Method),
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure,
            completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    public static HttpRemoteResult<TResult>? Forward<TResult>(this HttpContext? httpContext, HttpMethod httpMethod,
        string? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.Forward<TResult>(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure,
            completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    public static HttpRemoteResult<TResult>? Forward<TResult>(this HttpContext? httpContext, Uri? requestUri = null,
        Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.Forward<TResult>(Helpers.ParseHttpMethod(httpContext?.Request.Method), requestUri,
            configure, completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    public static HttpRemoteResult<TResult>? Forward<TResult>(this HttpContext? httpContext, HttpMethod httpMethod,
        Uri? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(httpMethod);

        // 构建 HttpRequestBuilder 实例
        var httpRequestBuilder = new HttpContextForwardBuilder(httpMethod, requestUri)
            .Build(httpContext, configure, forwardOptions);

        // 获取 IHttpRemoteService 实例
        var httpRemoteService = httpContext.RequestServices.GetRequiredService<IHttpRemoteService>();

        // 发送 HTTP 远程请求
        var httpRemoteResult =
            httpRemoteService.Send<TResult>(httpRequestBuilder, completionOption, httpContext.RequestAborted);

        // 根据配置选项将 HttpResponseMessage 信息转发到 HttpContext 中
        ForwardResponseMessage(httpContext, httpRemoteResult?.ResponseMessage,
            httpContext.ResolveForwardOptions(forwardOptions));

        return httpRemoteResult;
    }

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    public static Task<HttpRemoteResult<TResult>?> ForwardAsync<TResult>(this HttpContext? httpContext,
        string? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.ForwardAsync<TResult>(Helpers.ParseHttpMethod(httpContext?.Request.Method),
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure,
            completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    public static Task<HttpRemoteResult<TResult>?> ForwardAsync<TResult>(this HttpContext? httpContext,
        HttpMethod httpMethod, string? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.ForwardAsync<TResult>(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure,
            completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    public static Task<HttpRemoteResult<TResult>?> ForwardAsync<TResult>(this HttpContext? httpContext,
        Uri? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null) =>
        httpContext.ForwardAsync<TResult>(Helpers.ParseHttpMethod(httpContext?.Request.Method), requestUri,
            configure, completionOption, forwardOptions);

    /// <summary>
    ///     转发 <see cref="HttpContext" /> 到新的 HTTP 远程地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    public static async Task<HttpRemoteResult<TResult>?> ForwardAsync<TResult>(this HttpContext? httpContext,
        HttpMethod httpMethod, Uri? requestUri = null, Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        HttpContextForwardOptions? forwardOptions = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(httpMethod);

        // 构建 HttpRequestBuilder 实例
        var httpRequestBuilder = await new HttpContextForwardBuilder(httpMethod, requestUri)
            .BuildAsync(httpContext, configure, forwardOptions);

        // 获取 IHttpRemoteService 实例
        var httpRemoteService = httpContext.RequestServices.GetRequiredService<IHttpRemoteService>();

        // 发送 HTTP 远程请求
        var httpRemoteResult = await httpRemoteService.SendAsync<TResult>(httpRequestBuilder, completionOption,
            httpContext.RequestAborted);

        // 根据配置选项将 HttpResponseMessage 信息转发到 HttpContext 中
        ForwardResponseMessage(httpContext, httpRemoteResult?.ResponseMessage,
            httpContext.ResolveForwardOptions(forwardOptions));

        return httpRemoteResult;
    }

    /// <summary>
    ///     根据配置选项将 <see cref="HttpResponseMessage" /> 信息转发到 <see cref="HttpContext" /> 中
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    internal static void ForwardResponseMessage(HttpContext httpContext, HttpResponseMessage? httpResponseMessage,
        HttpContextForwardOptions forwardOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(forwardOptions);

        // 空检查
        if (httpResponseMessage is null)
        {
            return;
        }

        // 获取 HttpResponse 实例
        var httpResponse = httpContext.Response;

        // 检查是否配置了响应状态码转发
        if (forwardOptions.WithResponseStatusCode)
        {
            httpResponse.StatusCode = (int)httpResponseMessage.StatusCode;
        }

        // 检查是否配置了响应标头转发
        if (forwardOptions.WithResponseHeaders)
        {
            ForwardHttpHeaders(httpResponse, httpResponseMessage.Headers, forwardOptions);
        }

        // 检查是否配置了响应内容标头转发
        if (forwardOptions.WithResponseContentHeaders)
        {
            ForwardHttpHeaders(httpResponse, httpResponseMessage.Content.Headers, forwardOptions);
        }

        // 调用用于在转发响应之前执行自定义操作
        forwardOptions.OnForward?.Invoke(httpContext, httpResponseMessage);
    }

    /// <summary>
    ///     转发 HTTP 标头
    /// </summary>
    /// <param name="httpResponse">
    ///     <see cref="HttpResponse" />
    /// </param>
    /// <param name="httpHeaders">
    ///     <see cref="HttpHeaders" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    internal static void ForwardHttpHeaders(HttpResponse httpResponse, HttpHeaders httpHeaders,
        HttpContextForwardOptions forwardOptions)
    {
        // 初始化忽略在转发时需要跳过的响应标头列表
        var ignoreResponseHeaders = _ignoreResponseHeaders.ConcatIgnoreNull(forwardOptions.IgnoreResponseHeaders)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        // 逐条更新响应标头
        foreach (var (key, values) in httpHeaders)
        {
            // 忽略特定响应标头
            if (key.IsIn(ignoreResponseHeaders, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            httpResponse.Headers[key] = values.ToArray();
        }
    }
}