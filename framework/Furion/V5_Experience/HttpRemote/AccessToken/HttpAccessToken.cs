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
///     Access Token 信息
/// </summary>
public sealed class HttpAccessToken
{
    /// <summary>
    ///     刷新 Token 常量
    /// </summary>
    internal const string RefreshTokenKey = "refresh_token";

    /// <summary>
    ///     <inheritdoc cref="HttpAccessToken" />
    /// </summary>
    public HttpAccessToken()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="HttpAccessToken" />
    /// </summary>
    /// <param name="value">Access Token 值</param>
    /// <param name="expiresAt">Access Token 的绝对过期时间</param>
    /// <exception cref="ArgumentException"></exception>
    public HttpAccessToken(string value, DateTimeOffset expiresAt)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    ///     <inheritdoc cref="HttpAccessToken" />
    /// </summary>
    /// <param name="value">Access Token 值</param>
    /// <param name="expiresAt">Access Token 的绝对过期时间（Unix 秒）</param>
    public HttpAccessToken(string value, long expiresAt)
        : this(value, DateTimeOffset.FromUnixTimeSeconds(expiresAt))
    {
    }

    /// <summary>
    ///     <inheritdoc cref="HttpAccessToken" />
    /// </summary>
    /// <param name="jwtToken">完整 JWT Token 字符串</param>
    /// <exception cref="ArgumentException"></exception>
    public HttpAccessToken(string jwtToken)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtToken);

        Value = jwtToken;
        ExpiresAt = JwtTokenUtility.Parse(jwtToken).GetExpirationTimeUtc()!.Value;
    }

    /// <summary>
    ///     Access Token 值
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    ///     Access Token 的绝对过期时间
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    ///     HTTP 认证方案
    /// </summary>
    public string? Scheme { get; set; }

    /// <summary>
    ///     刷新 Token（可选）
    /// </summary>
    /// <remarks>内部是 <c>Items["refresh_token"]</c> 的便捷访问器。如果不需要，可保持为 <c>null</c>。</remarks>
    public string? RefreshToken
    {
        get => Items.TryGetValue(RefreshTokenKey, out var refreshToken) ? refreshToken?.ToString() : null;
        set
        {
            // 空检查
            if (value is null)
            {
                Items.Remove(RefreshTokenKey);
            }
            else
            {
                Items[RefreshTokenKey] = value;
            }
        }
    }

    /// <summary>
    ///     共享数据字典
    /// </summary>
    /// <remarks>用于存储与 Access Token 相关的自定义数据。</remarks>
    public IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

    /// <summary>
    ///     设置 Access Token 的绝对过期时间
    /// </summary>
    /// <param name="expiresAt">Access Token 的绝对过期时间</param>
    /// <returns>
    ///     <see cref="HttpAccessToken" />
    /// </returns>
    public HttpAccessToken SetExpiresAt(DateTimeOffset expiresAt)
    {
        ExpiresAt = expiresAt;

        return this;
    }

    /// <summary>
    ///     设置 Access Token 的绝对过期时间
    /// </summary>
    /// <param name="expiresAt">Access Token 的绝对过期时间（Unix 秒）</param>
    /// <returns>
    ///     <see cref="HttpAccessToken" />
    /// </returns>
    public HttpAccessToken SetExpiresAt(long expiresAt) => SetExpiresAt(DateTimeOffset.FromUnixTimeSeconds(expiresAt));

    /// <summary>
    ///     检查 Access Token 是否过期
    /// </summary>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool IsExpired() => DateTimeOffset.UtcNow >= ExpiresAt;
}