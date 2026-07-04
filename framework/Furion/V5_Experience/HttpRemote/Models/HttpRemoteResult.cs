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

using Furion.HttpRemote.Extensions;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Net.Http.Headers;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求结果
/// </summary>
/// <remarks>用于将原始的 <see cref="HttpResponseMessage" /> 进行包装转换。</remarks>
/// <typeparam name="TResult">转换的目标类型</typeparam>
public sealed class HttpRemoteResult<TResult> : IDisposable
{
    /// <summary>
    ///     <inheritdoc cref="HttpRemoteResult{TResult}" />
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    public HttpRemoteResult(HttpResponseMessage httpResponseMessage)
    {
        ResponseMessage = httpResponseMessage;

        // 初始化
        Initialize();
    }

    /// <inheritdoc cref="HttpResponseMessage" />
    public HttpResponseMessage ResponseMessage { get; }

    /// <summary>
    ///     内容类型
    /// </summary>
    public string? ContentType { get; private set; }

    /// <summary>
    ///     字符集
    /// </summary>
    public string? CharSet { get; private set; }

    /// <summary>
    ///     内容编码
    /// </summary>
    public ICollection<string> ContentEncoding { get; private set; } = null!;

    /// <summary>
    ///     内容大小
    /// </summary>
    public long? ContentLength { get; private set; }

    /// <summary>
    ///     响应 <c>Server</c> 标头
    /// </summary>
    public HttpHeaderValueCollection<ProductInfoHeaderValue> Server { get; private set; } = null!;

    /// <summary>
    ///     原始响应标头 <c>Set-Cookie</c> 集合
    /// </summary>
    public List<string>? RawSetCookies { get; private set; }

    /// <summary>
    ///     <see cref="Microsoft.Net.Http.Headers.SetCookieHeaderValue" /> 集合
    /// </summary>
    public IList<SetCookieHeaderValue>? SetCookies { get; private set; }

    /// <summary>
    ///     响应状态码
    /// </summary>
    public HttpStatusCode StatusCode { get; private set; }

    /// <summary>
    ///     是否请求成功
    /// </summary>
    public bool IsSuccessStatusCode { get; private set; }

    /// <summary>
    ///     <typeparamref name="TResult" />
    /// </summary>
    /// <remarks>注意 <c>HEAD</c> 请求不包含响应体。</remarks>
    public TResult? Result { get; internal init; }

    /// <summary>
    ///     请求耗时（毫秒）
    /// </summary>
    public long RequestDuration { get; internal init; }

    /// <summary>
    ///     响应标头
    /// </summary>
    public HttpResponseHeaders Headers { get; private set; } = null!;

    /// <summary>
    ///     响应体标头
    /// </summary>
    public HttpContentHeaders ContentHeaders { get; private set; } = null!;

    /// <summary>
    ///     HTTP 版本
    /// </summary>
    public Version Version { get; private set; } = null!;

    /// <summary>
    ///     <see cref="HttpClient" /> 实例的配置名称
    /// </summary>
    public string? HttpClientName { get; private set; }

    /// <inheritdoc />
    public void Dispose() => ResponseMessage.Dispose();

    // /// <summary>
    // ///     解构函数（至少包含两个 out 参数！！！）
    // /// </summary>
    // /// <param name="result">
    // ///     <typeparamref name="TResult" />
    // /// </param>
    // public void Deconstruct(out TResult? result)
    // {
    //     result = Result;
    // }

    /// <summary>
    ///     解构函数
    /// </summary>
    /// <param name="result">
    ///     <typeparamref name="TResult" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <inheritdoc cref="HttpResponseMessage" />
    /// </param>
    public void Deconstruct(out TResult? result, out HttpResponseMessage httpResponseMessage)
    {
        result = Result;
        httpResponseMessage = ResponseMessage;
    }

    /// <summary>
    ///     解构函数
    /// </summary>
    /// <param name="result">
    ///     <typeparamref name="TResult" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <inheritdoc cref="HttpResponseMessage" />
    /// </param>
    /// <param name="isSuccessStatusCode">是否请求成功</param>
    public void Deconstruct(out TResult? result, out HttpResponseMessage httpResponseMessage,
        out bool isSuccessStatusCode)
    {
        result = Result;
        httpResponseMessage = ResponseMessage;
        isSuccessStatusCode = IsSuccessStatusCode;
    }

    /// <summary>
    ///     解构函数
    /// </summary>
    /// <param name="result">
    ///     <typeparamref name="TResult" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <inheritdoc cref="HttpResponseMessage" />
    /// </param>
    /// <param name="isSuccessStatusCode">是否请求成功</param>
    /// <param name="statusCode">响应状态码</param>
    public void Deconstruct(out TResult? result, out HttpResponseMessage httpResponseMessage,
        out bool isSuccessStatusCode, out HttpStatusCode statusCode)
    {
        result = Result;
        httpResponseMessage = ResponseMessage;
        isSuccessStatusCode = IsSuccessStatusCode;
        statusCode = StatusCode;
    }

    /// <summary>
    ///     初始化
    /// </summary>
    internal void Initialize()
    {
        // 解析响应状态码
        ParseStatusCode();

        // 解析响应标头
        ParseHeaders();

        // 解析响应内容标头部分信息
        ParseContentMetadata(ResponseMessage.Content.Headers);

        // 解析响应标头 Set-Cookie 集合
        ParseSetCookies(ResponseMessage.Headers);

        // 获取 HTTP 版本
        Version = ResponseMessage.Version;

        // 根据 HTTP 响应消息解析出 HttpClient 实例的配置名称
        HttpClientName = ResponseMessage.ResolveHttpClientName();
    }

    /// <summary>
    ///     解析响应状态码
    /// </summary>
    internal void ParseStatusCode()
    {
        StatusCode = ResponseMessage.StatusCode;
        IsSuccessStatusCode = ResponseMessage.IsSuccessStatusCode;
    }

    /// <summary>
    ///     解析响应标头
    /// </summary>
    internal void ParseHeaders()
    {
        Headers = ResponseMessage.Headers;
        ContentHeaders = ResponseMessage.Content.Headers;
        Server = ResponseMessage.Headers.Server;
    }

    /// <summary>
    ///     解析响应体标头元数据
    /// </summary>
    /// <param name="contentHeaders">
    ///     <see cref="HttpContentHeaders" />
    /// </param>
    internal void ParseContentMetadata(HttpContentHeaders contentHeaders)
    {
        ContentLength = contentHeaders.ContentLength;
        ContentType = contentHeaders.ContentType?.MediaType;
        CharSet = contentHeaders.ContentType?.CharSet;
        ContentEncoding = contentHeaders.ContentEncoding;
    }

    /// <summary>
    ///     解析响应标头 <c>Set-Cookie</c> 集合
    /// </summary>
    /// <param name="responseHeaders">
    ///     <see cref="HttpResponseHeaders" />
    /// </param>
    internal void ParseSetCookies(HttpResponseHeaders responseHeaders)
    {
        // 尝试获取响应标头 Set-Cookie 集合
        if (!responseHeaders.TryGetSetCookies(out var setCookies, out var rawSetCookies))
        {
            return;
        }

        SetCookies = setCookies;
        RawSetCookies = rawSetCookies;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        // 格式化请求条目
        var requestEntry = ResponseMessage.RequestMessage?.ProfilerHeaders();

        // 格式化常规和响应条目
        var generalAndResponseEntry = ResponseMessage.ProfilerGeneralAndHeaders(generalCustomKeyValues:
            [new KeyValuePair<string, IEnumerable<string>>("Request Duration (ms)", [$"{RequestDuration:N2}"])]);

        return Helpers.JoinNonEmptyLines(requestEntry, generalAndResponseEntry);
    }
}