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

using System.Text;
using System.Text.Json;

namespace Furion.Utilities;

/// <summary>
///     JWT 实用方法
/// </summary>
public static class JwtTokenUtility
{
    /// <summary>
    ///     获取 JWT Payload 部分的 JSON 字符串
    /// </summary>
    /// <param name="token">完整 JWT 或单独的 Payload 片段</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetPayloadJson(string token)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        // 如果是完整的三段式 JWT，提取中间部分
        var parts = token.Split('.');
        var payload = parts.Length == 3 ? parts[1] : token;

        // 将 Payload 部分从 Base64Url 转换为标准 Base64
        var base64 = payload.Replace('-', '+').Replace('_', '/');

        // 补齐 =
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }

    /// <summary>
    ///     解析 JWT 的 Payload
    /// </summary>
    /// <param name="token">完整 JWT 或单独的 Payload 片段</param>
    /// <returns>
    ///     <see cref="JwtPayload" />
    /// </returns>
    public static JwtPayload Parse(string token)
    {
        // 获取 JWT Payload 部分的 JSON 字符串
        var payloadJson = GetPayloadJson(token);

        // 将 JSON 字符串转换为 JsonDocument
        using var jsonDocument = JsonDocument.Parse(payloadJson);

        // 初始化 JwtPayload 实例并返回
        return new JwtPayload(jsonDocument.RootElement.Clone(), payloadJson);
    }

    /// <summary>
    ///     JWT Payload 内容
    /// </summary>
    public class JwtPayload
    {
        /// <summary>
        ///     根 <see cref="JsonElement" />
        /// </summary>
        internal readonly JsonElement _root;

        /// <summary>
        ///     <inheritdoc cref="JwtPayload" />
        /// </summary>
        /// <param name="root">根 <see cref="JsonElement" /></param>
        /// <param name="rawJson">原始 JSON 字符串</param>
        internal JwtPayload(JsonElement root, string rawJson)
        {
            _root = root;
            RawJson = rawJson;
        }

        /// <summary>
        ///     原始 JSON 字符串
        /// </summary>
        public string RawJson { get; }

        /// <summary>
        ///     检查是否包含指定声明
        /// </summary>
        /// <param name="claimName">声明的键名</param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool Contains(string claimName) => _root.TryGetProperty(claimName, out _);

        /// <summary>
        ///     获取指定声明
        /// </summary>
        /// <param name="claimName">声明的键名</param>
        /// <returns>
        ///     <see cref="JsonElement" />
        /// </returns>
        public JsonElement? GetProperty(string claimName) =>
            _root.TryGetProperty(claimName, out var element) ? element : null;

        /// <summary>
        ///     获取签发者
        /// </summary>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public string? GetIssuer()
        {
            var property = GetProperty("iss");

            return property?.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
        }

        /// <summary>
        ///     获取主题
        /// </summary>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public string? GetSubject()
        {
            var property = GetProperty("sub");

            return property?.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
        }

        /// <summary>
        ///     获取接收方
        /// </summary>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public string? GetAudience()
        {
            var property = GetProperty("aud");

            return property?.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
        }

        /// <summary>
        ///     获取过期时间（Unix 秒）
        /// </summary>
        /// <remarks>可通过 <c>DateTimeOffset.FromUnixTimeSeconds(exp)</c> 转换为 <see cref="DateTimeOffset" />。</remarks>
        /// <returns>
        ///     <see cref="long" />
        /// </returns>
        public long? GetExpiration()
        {
            var property = GetProperty("exp");

            return property?.ValueKind == JsonValueKind.Number && property.Value.TryGetInt64(out var value)
                ? value
                : null;
        }

        /// <summary>
        ///     获取签发时间（Unix 秒）
        /// </summary>
        /// <returns>
        ///     <see cref="long" />
        /// </returns>
        public long? GetIssuedAt()
        {
            var property = GetProperty("iat");

            return property?.ValueKind == JsonValueKind.Number && property.Value.TryGetInt64(out var value)
                ? value
                : null;
        }

        /// <summary>
        ///     获取生效时间（Unix 秒）
        /// </summary>
        /// <returns>
        ///     <see cref="long" />
        /// </returns>
        public long? GetNotBefore()
        {
            var property = GetProperty("nbf");

            return property?.ValueKind == JsonValueKind.Number && property.Value.TryGetInt64(out var value)
                ? value
                : null;
        }

        /// <summary>
        ///     获取唯一标识
        /// </summary>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public string? GetJwtId()
        {
            var property = GetProperty("jti");

            return property?.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
        }

        /// <summary>
        ///     获取过期时间（UTC 时间）
        /// </summary>
        /// <returns>
        ///     <see cref="DateTimeOffset" />
        /// </returns>
        public DateTimeOffset? GetExpirationTimeUtc()
        {
            // 获取过期时间（Unix 秒）
            var exp = GetExpiration();

            return exp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(exp.Value) : null;
        }

        /// <summary>
        ///     获取过期时间（本地时间）
        /// </summary>
        /// <returns>
        ///     <see cref="DateTime" />
        /// </returns>
        public DateTime? GetExpirationTime() => GetExpirationTimeUtc()?.LocalDateTime;

        /// <summary>
        ///     获取签发时间（UTC 时间）
        /// </summary>
        /// <returns>
        ///     <see cref="DateTimeOffset" />
        /// </returns>
        public DateTimeOffset? GetIssuedAtTimeUtc()
        {
            // 获取签发时间（Unix 秒）
            var iat = GetIssuedAt();

            return iat.HasValue ? DateTimeOffset.FromUnixTimeSeconds(iat.Value) : null;
        }

        /// <summary>
        ///     获取签发时间（本地时间）
        /// </summary>
        /// <returns>
        ///     <see cref="DateTime" />
        /// </returns>
        public DateTime? GetIssuedAtTime() => GetIssuedAtTimeUtc()?.LocalDateTime;

        /// <summary>
        ///     获取生效时间（UTC 时间）
        /// </summary>
        /// <returns>
        ///     <see cref="DateTimeOffset" />
        /// </returns>
        public DateTimeOffset? GetNotBeforeTimeUtc()
        {
            // 获取生效时间（Unix 秒）
            var nbf = GetNotBefore();

            return nbf.HasValue ? DateTimeOffset.FromUnixTimeSeconds(nbf.Value) : null;
        }

        /// <summary>
        ///     获取生效时间（本地时间）
        /// </summary>
        /// <returns>
        ///     <see cref="DateTime" />
        /// </returns>
        public DateTime? GetNotBeforeTime() => GetNotBeforeTimeUtc()?.LocalDateTime;

        /// <summary>
        ///     获取指定声明的字符串值
        /// </summary>
        /// <param name="claimName">声明的键名</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public string? GetString(string claimName)
        {
            var prop = GetProperty(claimName);

            return prop?.ValueKind == JsonValueKind.String ? prop.Value.GetString() : null;
        }

        /// <summary>
        ///     获取指定声明的 32 位整数值
        /// </summary>
        /// <param name="claimName">声明的键名</param>
        /// <returns>
        ///     <see cref="int" />
        /// </returns>
        public int? GetInt32(string claimName)
        {
            var prop = GetProperty(claimName);

            return prop?.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var v) ? v : null;
        }

        /// <summary>
        ///     获取指定声明的 64 位整数值
        /// </summary>
        /// <param name="claimName">声明的键名</param>
        /// <returns>
        ///     <see cref="long" />
        /// </returns>
        public long? GetInt64(string claimName)
        {
            var prop = GetProperty(claimName);

            return prop?.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt64(out var v) ? v : null;
        }

        /// <summary>
        ///     判断是否已过期
        /// </summary>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool IsExpired()
        {
            // 获取过期时间（UTC 时间）
            var expirationTime = GetExpirationTimeUtc();

            // 无过期时间视为永不过期
            if (expirationTime is null)
            {
                return false;
            }

            return expirationTime.Value <= DateTimeOffset.UtcNow;
        }

        /// <summary>
        ///     判断 Token 当前是否有效（已生效且未过期）
        /// </summary>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool IsActive()
        {
            // 获取当前时间（UTC 时间）
            var now = DateTimeOffset.UtcNow;

            // 检查生效时间
            var nbfTime = GetNotBeforeTimeUtc();
            if (nbfTime.HasValue && nbfTime.Value > now)
            {
                return false;
            }

            // 检查过期时间
            var expTime = GetExpirationTimeUtc();
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (expTime.HasValue && expTime.Value <= now)
            {
                return false;
            }

            return true;
        }
    }
}