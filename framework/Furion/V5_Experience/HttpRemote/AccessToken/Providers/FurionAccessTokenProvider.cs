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

using Furion.Utilities;
using System.Net;

namespace Furion.HttpRemote;

/// <summary>
///     Furion 框架 Access Token 提供器
/// </summary>
/// <remarks>参考文献：https://furion.net/docs/auth-control。</remarks>
/// <param name="accessTokenManager">
///     <see cref="IHttpAccessTokenManager" />
/// </param>
/// <param name="logger">
///     <see cref="IHttpRemoteLogger" />
/// </param>
public class FurionAccessTokenProvider(IHttpAccessTokenManager accessTokenManager, IHttpRemoteLogger logger)
    : IHttpAccessTokenProvider, IHttpAccessTokenConfigurator
{
    // Furion 框架相关常量定义
    internal const string XAuthorizationHeaderName = "X-Authorization";
    internal const string AccessTokenHeaderName = "access-token";
    internal const string XAccessTokenHeaderName = "x-access-token";

    /// <inheritdoc />
    public virtual void Configure(HttpRequestBuilder httpRequestBuilder, HttpAccessToken httpAccessToken)
    {
        // 设置 JWT 身份验证凭据请求授权标头
        httpRequestBuilder.AddBearerAuthentication(httpAccessToken.Value);

        // 检查 Access Token 是否过期且刷新 Token 不为空
        if (httpAccessToken.IsExpired() && httpAccessToken.RefreshToken is not null)
        {
            httpRequestBuilder.AddBearerAuthentication(XAuthorizationHeaderName, httpAccessToken.RefreshToken);
        }

        // 设置在收到 HTTP 响应之后执行的操作
        httpRequestBuilder.SetOnPostReceiveResponse(async (httpResponseMessage, cancellationToken) =>
        {
            // 获取响应标头中的 access-token 和 x-access-token
            if (!httpResponseMessage.Headers.TryGetValues(AccessTokenHeaderName, out var atValues) ||
                !httpResponseMessage.Headers.TryGetValues(XAccessTokenHeaderName, out var rtValues))
            {
                return;
            }

            var newAccessToken = atValues.FirstOrDefault();
            var newRefreshToken = rtValues.FirstOrDefault();

            // 如果任意一个值为空则跳过
            if (string.IsNullOrWhiteSpace(newAccessToken) || string.IsNullOrWhiteSpace(newRefreshToken))
            {
                logger.LogWarning("Furion access token response headers are present but empty or invalid.");

                return;
            }

            // 如果服务端返回 "invalid_token"，说明 RefreshToken 也彻底失效了
            if (newAccessToken == "invalid_token")
            {
                logger.LogWarning("Furion refresh token is invalid or expired. Server returned 'invalid_token'.");

                return;
            }

            try
            {
                // 从新的 Access Token 的 JWT 中解析过期时间
                var expiresAt = JwtTokenUtility.Parse(newAccessToken).GetExpirationTimeUtc();

                // 空检查
                if (expiresAt is null)
                {
                    logger.LogWarning("Failed to parse expiration time from the new Furion access token.");

                    return;
                }

                // 创建新的 HttpAccessToken 实例
                var updatedToken =
                    new HttpAccessToken(newAccessToken, expiresAt.Value) { RefreshToken = newRefreshToken };

                // 更新 Access Token 缓存
                await accessTokenManager.SetAsync(httpRequestBuilder.HttpClientName, updatedToken, cancellationToken);

                // 记录刷新成功及新的过期时间
                logger.LogInformation("Furion access token successfully refreshed. New expiration time: {ExpiresAt:O}.",
                    expiresAt.Value);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "An error occurred while parsing or updating the Furion access token.");
            }
        });
    }

    /// <inheritdoc />
    public virtual Task<HttpAccessToken?>
        GetAsync(HttpAccessTokenContext context, CancellationToken cancellationToken) =>
        Task.FromResult(HttpAccessToken.None);

    /// <inheritdoc />
    public virtual Task<HttpAccessToken?> RefreshAsync(HttpAccessTokenContext context, HttpAccessToken? currentToken,
        CancellationToken cancellationToken)
    {
        // 空检查
        if (currentToken is null)
        {
            logger.LogWarning("Cannot refresh Furion access token because the current token is null.");

            return Task.FromResult(HttpAccessToken.None);
        }

        // 初始化新的 HttpAccessToken 实例
        var forcedExpiredToken = new HttpAccessToken(currentToken.Value, DateTimeOffset.MinValue)
        {
            RefreshToken = currentToken.RefreshToken, Scheme = currentToken.Scheme
        };

        // 复制自定义共享数据
        foreach (var item in currentToken.Items)
        {
            forcedExpiredToken.Items[item.Key] = item.Value;
        }

        return Task.FromResult<HttpAccessToken?>(forcedExpiredToken);
    }

    /// <inheritdoc />
    public virtual Task<bool> ShouldRefreshAsync(HttpAccessTokenContext context,
        HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
    {
        // 检查响应状态码是否是 401 或 403
        var shouldRefresh = httpResponseMessage.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;

        // 判断是否需要刷新
        if (shouldRefresh)
        {
            logger.LogWarning("Furion access token refresh triggered due to HTTP 401 Unauthorized response.");
        }

        return Task.FromResult(shouldRefresh);
    }
}