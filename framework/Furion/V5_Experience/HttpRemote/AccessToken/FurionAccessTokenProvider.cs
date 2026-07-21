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

namespace Furion.HttpRemote;

/// <summary>
///     Furion 框架 Access Token 提供器
/// </summary>
public class FurionAccessTokenProvider : IHttpAccessTokenProvider, IHttpAccessTokenConfigurator
{
    /// <inheritdoc />
    public virtual void Configure(HttpRequestBuilder httpRequestBuilder, HttpAccessToken httpAccessToken)
    {
        // 设置 JWT 身份验证凭据请求授权标头
        httpRequestBuilder.AddJwtBearerAuthentication(httpAccessToken.Value);

        // 检查 Access Token 是否过期且刷新 Token 不为空
        if (httpAccessToken.IsExpired() && httpAccessToken.RefreshToken is not null)
        {
            httpRequestBuilder.WithHeader("X-Authorization", $"Bearer {httpAccessToken.RefreshToken}", replace: true);
        }

        //  设置在收到 HTTP 响应之后执行的操作
        httpRequestBuilder.SetOnPostReceiveResponse(httpResponseMessage =>
        {
            // 获取响应标头中的 access-token 和 x-access-token
            var newAccessToken = httpResponseMessage.Headers.GetValues("access-token").FirstOrDefault();
            var newRefreshToken = httpResponseMessage.Headers.GetValues("x-access-token").FirstOrDefault();

            // 空检查
            // ReSharper disable once InvertIf
            if (!string.IsNullOrWhiteSpace(newAccessToken) && !string.IsNullOrWhiteSpace(newRefreshToken))
            {
                httpAccessToken.Value = newAccessToken;
                httpAccessToken.RefreshToken = newRefreshToken;

                httpAccessToken.ExpiresAt = JwtTokenUtility.Parse(newRefreshToken).GetExpirationTimeUtc()!.Value;
            }
        });
    }

    /// <inheritdoc />
    public virtual Task<HttpAccessToken?> GetTokenAsync(HttpAccessTokenContext context,
        CancellationToken cancellationToken) => Task.FromResult<HttpAccessToken?>(null);

    /// <inheritdoc />
    public virtual Task<HttpAccessToken?> RefreshTokenAsync(HttpAccessTokenContext context,
        HttpAccessToken? currentToken, CancellationToken cancellationToken) => Task.FromResult(currentToken);
}