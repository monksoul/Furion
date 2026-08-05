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

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求服务
/// </summary>
public partial interface IHttpRemoteService
{
    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Get(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Get(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> GetAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> GetAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? GetAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? GetAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> GetAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> GetAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Get<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Get<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> GetAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> GetAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? GetAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? GetAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? GetAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? GetAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? GetAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? GetAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> GetAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> GetAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> GetAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> GetAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> GetAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP GET 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> GetAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Put(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Put(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> PutAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> PutAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? PutAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? PutAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> PutAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> PutAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Put<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Put<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> PutAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> PutAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? PutAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? PutAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? PutAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? PutAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? PutAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? PutAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> PutAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> PutAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> PutAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> PutAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> PutAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PUT 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> PutAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Post(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Post(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> PostAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> PostAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? PostAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? PostAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> PostAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> PostAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Post<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Post<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> PostAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> PostAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? PostAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? PostAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? PostAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? PostAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? PostAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? PostAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> PostAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> PostAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> PostAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> PostAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> PostAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP POST 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> PostAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Delete(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Delete(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> DeleteAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> DeleteAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? DeleteAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? DeleteAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> DeleteAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> DeleteAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Delete<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Delete<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> DeleteAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> DeleteAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? DeleteAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? DeleteAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? DeleteAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? DeleteAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? DeleteAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? DeleteAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> DeleteAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> DeleteAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> DeleteAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> DeleteAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> DeleteAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP DELETE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> DeleteAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Head(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Head(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> HeadAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> HeadAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? HeadAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? HeadAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> HeadAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> HeadAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Head<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Head<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> HeadAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> HeadAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? HeadAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? HeadAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? HeadAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? HeadAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? HeadAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? HeadAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> HeadAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> HeadAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> HeadAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> HeadAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> HeadAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP HEAD 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> HeadAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Options(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Options(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> OptionsAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> OptionsAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? OptionsAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? OptionsAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> OptionsAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> OptionsAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Options<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Options<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> OptionsAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> OptionsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? OptionsAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? OptionsAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? OptionsAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? OptionsAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? OptionsAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? OptionsAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> OptionsAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> OptionsAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> OptionsAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> OptionsAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> OptionsAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP OPTIONS 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> OptionsAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Trace(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Trace(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> TraceAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> TraceAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? TraceAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? TraceAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> TraceAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> TraceAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Trace<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Trace<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> TraceAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> TraceAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? TraceAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? TraceAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? TraceAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? TraceAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? TraceAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? TraceAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> TraceAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> TraceAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> TraceAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> TraceAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> TraceAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP TRACE 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> TraceAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Patch(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Patch(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> PatchAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> PatchAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? PatchAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? PatchAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> PatchAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> PatchAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Patch<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Patch<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> PatchAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> PatchAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? PatchAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? PatchAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? PatchAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? PatchAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? PatchAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? PatchAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> PatchAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> PatchAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> PatchAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> PatchAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> PatchAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP PATCH 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> PatchAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Query(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Query(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> QueryAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    Task<HttpResponseMessage?> QueryAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? QueryAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    TResult? QueryAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> QueryAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<TResult?> QueryAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Query<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    HttpRemoteResult<TResult>? Query<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> QueryAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRemoteResult{TResult}" />
    /// </returns>
    Task<HttpRemoteResult<TResult>?> QueryAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? QueryAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? QueryAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? QueryAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    string? QueryAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Stream? QueryAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    byte[]? QueryAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> QueryAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> QueryAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> QueryAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    Task<string?> QueryAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    Task<Stream?> QueryAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP QUERY 远程请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <c>byte[]</c>
    /// </returns>
    Task<byte[]?> QueryAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default);
}