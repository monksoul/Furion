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
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Furion.HttpRemote;

/// <summary>
///     微信开放平台 Access Token 提供器
/// </summary>
/// <remarks>参考文献：https://developers.weixin.qq.com/doc/service/api/base/api_getaccesstoken.html。</remarks>
/// <param name="httpRemoteService">
///     <see cref="IHttpRemoteService" />
/// </param>
/// <param name="logger">
///     <see cref="IHttpRemoteLogger" />
/// </param>
/// <param name="appId">唯一凭证</param>
/// <param name="appSecret">唯一凭证密钥</param>
public class WeChatAccessTokenProvider(
    IHttpRemoteService httpRemoteService,
    IHttpRemoteLogger logger,
    string appId,
    string appSecret)
    : IHttpAccessTokenProvider, IHttpAccessTokenConfigurator
{
    // 微信相关常量定义
    internal const string AccessTokenKey = "access_token";
    internal const string ExpiresInKey = "expires_in";

    /// <inheritdoc />
    public virtual void Configure(HttpRequestBuilder httpRequestBuilder, HttpAccessToken httpAccessToken) =>
        // 设置凭证查询参数
        httpRequestBuilder.WithQueryParameter("access_token", httpAccessToken.Value, true);

    /// <inheritdoc />
    public virtual async Task<HttpAccessToken?> GetAsync(HttpAccessTokenContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // 发送 HTTP 远程请求获取接口调用凭据
            var body = await httpRemoteService.SendAsStringAsync(
                HttpRequestBuilder.Get("https://api.weixin.qq.com/cgi-bin/token")
                    .WithQueryParameters(new { appid = appId, secret = appSecret, grant_type = "client_credential" })
                    // 跳过 Token 管理，避免递归调用
                    .WithoutTokenManagement(), cancellationToken);

            // 空检查
            if (string.IsNullOrWhiteSpace(body))
            {
                logger.LogWarning("WeChat access token response is empty.");

                return null;
            }

            // 解析 JSON
            var json = JsonNode.Parse(body);

            // 空检查
            if (json is null)
            {
                logger.LogWarning("Failed to parse WeChat access token response. Response: {Body}", body);

                return null;
            }

            // 尝试解析微信业务错误码
            var errCode = json["errcode"]?.GetValue<int>();

            // 空检查
            if (errCode is not null && errCode != 0)
            {
                // 获取错误码信息
                var errMsg = json["errmsg"]?.GetValue<string>();

                logger.LogWarning("Failed to fetch WeChat access token. ErrCode: {ErrCode}, ErrMsg: {ErrMsg}", errCode,
                    errMsg);

                return null;
            }

            // 获取 access_token 和 expires_in
            var accessToken = json[AccessTokenKey]?.GetValue<string>();
            var expiresIn = json[ExpiresInKey]?.GetValue<int>() ?? 0;

            // 校验凭证有效性
            if (string.IsNullOrWhiteSpace(accessToken) || expiresIn <= 0)
            {
                logger.LogWarning("WeChat access token response is invalid. Response: {Body}", body);

                return null;
            }

            // 默认提前 5 秒过期，避免因网络延迟导致使用失效 access_token
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0, expiresIn - 5));

            // 记录成功获取及新的过期时间
            logger.LogInformation("WeChat access token successfully fetched. Expiration time: {ExpiresAt:O}.",
                expiresAt);

            return new HttpAccessToken(accessToken, expiresAt);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogWarning(e, "Failed to fetch WeChat access token from remote server.");
        }

        return null;
    }

    /// <inheritdoc />
    public virtual async Task<bool> ShouldRefreshAsync(HttpAccessTokenContext context,
        HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken)
    {
        // 检查响应状态码是否是 401 或 403
        if (httpResponseMessage.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            logger.LogWarning("WeChat access token refresh triggered due to HTTP {StatusCode} response.",
                (int)httpResponseMessage.StatusCode);

            return true;
        }

        // 检查是否请求成功
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return false;
        }

        // 启用缓冲，可重复读取
        try
        {
#if NET8_0
            await httpResponseMessage.Content.LoadIntoBufferAsync();
#else
            await httpResponseMessage.Content.LoadIntoBufferAsync(cancellationToken);
#endif
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return false;
        }

        // 读取响应内容字符串
        var body = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

        // 空检查
        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        try
        {
            // 尝试解析 errcode 值
            var json = JsonNode.Parse(body);
            var errCode = json?["errcode"]?.GetValue<int>();

            // 40001: invalid credential, 40014: invalid access_token, 42001: access_token expired
            // ReSharper disable once InvertIf
            if (errCode is 40001 or 40014 or 42001)
            {
                logger.LogWarning("WeChat access token refresh triggered due to WeChat error code: {ErrCode}.",
                    errCode);

                return true;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}