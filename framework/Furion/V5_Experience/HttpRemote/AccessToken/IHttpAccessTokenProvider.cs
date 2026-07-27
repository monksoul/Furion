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

using System.Net;

namespace Furion.HttpRemote;

/// <summary>
///     Access Token 提供器
/// </summary>
/// <remarks>
///     <para>负责获取新的 Access Token 并告知过期时间。</para>
///     <para>实现该接口的类型也可以同时实现 <see cref="IHttpAccessTokenConfigurator" />。</para>
/// </remarks>
public interface IHttpAccessTokenProvider
{
    /// <summary>
    ///     获取 Access Token
    /// </summary>
    /// <remarks>若使用 <see cref="IHttpAccessTokenManager.SetAsync" /> 设置 Access Token，那么该方法可以空实现或抛出异常。</remarks>
    /// <param name="context">
    ///     <see cref="HttpAccessTokenContext" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpAccessToken" />
    /// </returns>
    Task<HttpAccessToken?> GetAsync(HttpAccessTokenContext context, CancellationToken cancellationToken);

    /// <summary>
    ///     刷新 Access Token
    /// </summary>
    /// <remarks>默认实现直接调用 <see cref="GetAsync" />。若认证流程中“首次获取 Token”与“刷新 Token”使用不同的接口（如登录接口 vs 刷新专用接口），可重写此方法并提供专用的刷新逻辑。</remarks>
    /// <param name="context">
    ///     <see cref="HttpAccessTokenContext" />
    /// </param>
    /// <param name="currentToken">当前已缓存的 <see cref="HttpAccessToken" /></param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpAccessToken" />
    /// </returns>
    Task<HttpAccessToken?> RefreshAsync(HttpAccessTokenContext context, HttpAccessToken? currentToken,
        CancellationToken cancellationToken) => GetAsync(context, cancellationToken);

    /// <summary>
    ///     指示是否需要强制刷新 Access Token 并重试请求
    /// </summary>
    /// <remarks>
    ///     默认实现仅在状态码为 <see cref="HttpStatusCode.Unauthorized" />（401）时返回 <c>true</c>。可重写此方法以自定义刷新策略（例如检查 403、响应头、响应体等）。
    /// </remarks>
    /// <param name="context">
    ///     <see cref="HttpAccessTokenContext" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    Task<bool> ShouldRefreshAsync(HttpAccessTokenContext context, HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken) =>
        Task.FromResult(httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized);
}