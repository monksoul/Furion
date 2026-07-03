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

using Furion.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Furion.DataEncryption;

/// <summary>
/// JWT 加解密
/// </summary>
public class JWTEncryption
{
    /// <summary>
    /// 刷新 Token 身份标识
    /// </summary>
    private static readonly string[] _refreshTokenClaims = ["f", "e", "s", "l", "k"];

    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="expiredTime">过期时间（分钟），最大支持 13 年</param>
    /// <returns></returns>
    public static string Encrypt(IDictionary<string, object> payload, long? expiredTime = null)
    {
        var (Payload, JWTSettings) = CombinePayload(payload, expiredTime);
        return Encrypt(JWTSettings.IssuerSigningPrivateKey ?? JWTSettings.IssuerSigningKey, Payload, JWTSettings.Algorithm);
    }

    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="issuerSigningPrivateKey">私钥</param>
    /// <param name="payload"></param>
    /// <param name="algorithm">可使用静态类：<c>SecurityAlgorithms.HmacSha256</c></param>
    /// <returns></returns>
    public static string Encrypt(string issuerSigningPrivateKey, IDictionary<string, object> payload, string algorithm)
    {
        string stringPayload;

        // 处理 JwtPayload 序列化不一致问题
        if (payload is JwtPayload jwtPayload)
        {
            stringPayload = jwtPayload.SerializeToJson();
        }
        else
        {
            var (Payload, _) = CombinePayload(payload);
            stringPayload = JsonSerializer.Serialize(Payload, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        return Encrypt(issuerSigningPrivateKey, stringPayload, algorithm);
    }

    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="issuerSigningPrivateKey">私钥</param>
    /// <param name="payload"></param>
    /// <param name="algorithm">可使用静态类：<c>SecurityAlgorithms.HmacSha256</c></param>
    /// <returns></returns>
    public static string Encrypt(string issuerSigningPrivateKey, string payload, string algorithm)
    {
        SigningCredentials credentials = null;

        if (!string.IsNullOrWhiteSpace(issuerSigningPrivateKey))
        {
            var securityKey = CreateSecurityKey(algorithm, issuerSigningPrivateKey);
            credentials = new SigningCredentials(securityKey, algorithm);
        }

        var tokenHandler = new JsonWebTokenHandler();
        return credentials == null ? tokenHandler.CreateToken(payload) : tokenHandler.CreateToken(payload, credentials);
    }

    /// <summary>
    /// 生成刷新 Token
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="expiredTime">刷新 Token 有效期（分钟），最大支持 13 年</param>
    /// <returns></returns>
    public static string GenerateRefreshToken(string accessToken, int expiredTime = 43200)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        // 分割Token
        var segments = accessToken.Split('.', StringSplitOptions.RemoveEmptyEntries);

        // 检查 JWT 字符串是否是三部分组成
        if (segments.Length != 3)
        {
            throw new ArgumentException("Invalid JWT format. Token must consist of three parts separated by '.'.", nameof(accessToken));
        }

        // 读取 JWT 三部分字符串
        var header = segments[0];
        var payload = segments[1];
        var signature = segments[2];

        // 检查每部分是否为空
        if (header.Length == 0 || payload.Length == 0 || signature.Length == 0)
        {
            throw new ArgumentException("JWT header, payload or signature is empty.", nameof(accessToken));
        }

        const int minPayloadLength = 27;
        if (payload.Length < minPayloadLength)
        {
            throw new ArgumentException($"JWT payload length is too short. Minimum required length is {minPayloadLength} characters.", nameof(accessToken));
        }

        var s = RandomNumberGenerator.GetInt32(10, payload.Length / 2 + 2);
        var l = RandomNumberGenerator.GetInt32(3, 13);

        var refreshPayload = new Dictionary<string, object>
        {
            { "f", header },
            { "e", signature },
            { "s", s },
            { "l", l },
            { "k", payload.Substring(s, l) }
        };


        return Encrypt(refreshPayload, expiredTime);
    }

    /// <summary>
    /// 通过过期Token 和 刷新Token 换取新的 Token
    /// </summary>
    /// <param name="expiredToken"></param>
    /// <param name="refreshToken"></param>
    /// <param name="expiredTime">过期时间（分钟），最大支持 13 年</param>
    /// <param name="clockSkew">刷新token容差值，秒做单位</param>
    /// <returns></returns>
    public static string Exchange(string expiredToken, string refreshToken, long? expiredTime = null, long clockSkew = 5)
    {
        // 交换刷新Token 必须原Token 已过期
        var (_isValid, _, _) = Validate(expiredToken);
        if (_isValid) return default;

        // 判断刷新Token 是否过期
        var (isValid, refreshTokenObj, _) = Validate(refreshToken);
        if (!isValid) return default;

        // 解析 HttpContext
        var httpContext = GetCurrentHttpContext();

        // 判断这个刷新Token 是否已刷新过
        var blacklistRefreshKey = "BLACKLIST_REFRESH_TOKEN:" + refreshToken;
        var distributedCache = httpContext?.RequestServices?.GetService<IDistributedCache>();

        // 处理token并发容错问题
        var nowTime = DateTimeOffset.UtcNow;
        var cachedValue = distributedCache?.GetString(blacklistRefreshKey);
        var isRefresh = !string.IsNullOrWhiteSpace(cachedValue);    // 判断是否刷新过
        if (isRefresh)
        {
            var refreshTime = new DateTimeOffset(long.Parse(cachedValue), TimeSpan.Zero);
            // 处理并发时容差值
            if ((nowTime - refreshTime).TotalSeconds > clockSkew) return default;
        }

        // 分割过期Token
        var tokenParagraphs = expiredToken.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (tokenParagraphs.Length < 3) return default;

        // 判断各个部分是否匹配
        if (!refreshTokenObj.GetPayloadValue<string>("f").Equals(tokenParagraphs[0])) return default;
        if (!refreshTokenObj.GetPayloadValue<string>("e").Equals(tokenParagraphs[2])) return default;
        if (!tokenParagraphs[1].Substring(refreshTokenObj.GetPayloadValue<int>("s"), refreshTokenObj.GetPayloadValue<int>("l")).Equals(refreshTokenObj.GetPayloadValue<string>("k"))) return default;

        // 获取过期 Token 的存储信息
        var jwtSecurityToken = SecurityReadJwtToken(expiredToken);
        if (jwtSecurityToken is null)
        {
            return default;
        }
        var payload = jwtSecurityToken.Payload;

        // 移除 Iat，Nbf，Exp
        foreach (var innerKey in DateTypeClaimTypes)
        {
            if (!payload.ContainsKey(innerKey)) continue;

            payload.Remove(innerKey);
        }

        // 交换成功后登记刷新Token，标记失效
        if (!isRefresh)
        {
            distributedCache?.SetString(blacklistRefreshKey, nowTime.Ticks.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(refreshTokenObj.GetPayloadValue<long>(JwtRegisteredClaimNames.Exp))
            });
        }

        return Encrypt(payload, expiredTime);
    }

    /// <summary>
    /// 自动刷新 Token 信息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <param name="expiredTime">新 Token 过期时间（分钟），最大支持 13 年</param>
    /// <param name="refreshTokenExpiredTime">新刷新 Token 有效期（分钟）</param>
    /// <param name="tokenPrefix"></param>
    /// <param name="clockSkew"></param>
    /// <param name="onRefreshing">当刷新时触发</param>
    /// <returns></returns>
    public static bool AutoRefreshToken(AuthorizationHandlerContext context, DefaultHttpContext httpContext, long? expiredTime = null, int refreshTokenExpiredTime = 43200, string tokenPrefix = "Bearer ", long clockSkew = 5, Action<string, string> onRefreshing = null)
    {
        // 如果验证有效，则跳过刷新
        if (context.User.Identity.IsAuthenticated)
        {
            // 禁止使用刷新 Token 进行单独校验
            if (_refreshTokenClaims.All(k => context.User.Claims.Any(c => c.Type == k)))
            {
                return false;
            }

            return true;
        }

        // 判断是否含有匿名特性
        if (httpContext.GetEndpoint()?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null) return true;

        // 获取过期Token 和 刷新Token
        var expiredToken = GetJwtBearerToken(httpContext, tokenPrefix: tokenPrefix);
        var refreshToken = GetJwtBearerToken(httpContext, "X-Authorization", tokenPrefix: tokenPrefix);
        if (string.IsNullOrWhiteSpace(expiredToken) || string.IsNullOrWhiteSpace(refreshToken)) return false;

        // 交换新的 Token
        var accessToken = Exchange(expiredToken, refreshToken, expiredTime, clockSkew);
        if (string.IsNullOrWhiteSpace(accessToken)) return false;

        // 读取新的 Token Clamis
        var claims = ReadJwtToken(accessToken)?.Claims;
        if (claims == null) return false;

        // 创建身份信息
        var claimIdentity = new ClaimsIdentity("AuthenticationTypes.Federation");
        claimIdentity.AddClaims(claims);
        var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

        // 设置 HttpContext.User 并登录
        httpContext.User = claimsPrincipal;

        string accessTokenKey = "access-token"
             , xAccessTokenKey = "x-access-token"
             , accessControlExposeKey = "Access-Control-Expose-Headers";

        // 返回新的 Token
        httpContext.Response.Headers[accessTokenKey] = accessToken;
        // 返回新的 刷新Token
        var refreshAccessToken = GenerateRefreshToken(accessToken, refreshTokenExpiredTime);
        httpContext.Response.Headers[xAccessTokenKey] = refreshAccessToken;

        // 调用刷新后回调函数
        onRefreshing?.Invoke(accessToken, refreshAccessToken);

        // 处理 axios 问题
        httpContext.Response.Headers.TryGetValue(accessControlExposeKey, out var acehs);
        httpContext.Response.Headers[accessControlExposeKey] = string.Join(',', StringValues.Concat(acehs, new StringValues([accessTokenKey, xAccessTokenKey])).Distinct());

        return true;
    }

    /// <summary>
    /// 验证 Token
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public static (bool IsValid, JsonWebToken Token, TokenValidationResult validationResult) Validate(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || !accessToken.Contains('.'))
        {
            return default;
        }

        var jwtSettings = GetJWTSettings();
        if (jwtSettings == null) return (false, default, default);

        // 创建Token验证参数
        var tokenValidationParameters = CreateTokenValidationParameters(jwtSettings);

        // 使用公钥
        tokenValidationParameters.IssuerSigningKey ??= CreateSecurityKey(jwtSettings.Algorithm, jwtSettings.IssuerSigningKey);

        // 验证 Token
        var tokenHandler = new JsonWebTokenHandler();
        if (!tokenHandler.CanReadToken(accessToken))
        {
            return (false, default, default);
        }

        try
        {
            var tokenValidationResult = tokenHandler.ValidateToken(accessToken, tokenValidationParameters);
            if (!tokenValidationResult.IsValid) return (false, null, tokenValidationResult);

            var jsonWebToken = tokenValidationResult.SecurityToken as JsonWebToken;
            return (true, jsonWebToken, tokenValidationResult);
        }
        catch
        {
            return (false, default, default);
        }
    }

    /// <summary>
    /// 验证 Token
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="token"></param>
    /// <param name="headerKey"></param>
    /// <param name="tokenPrefix"></param>
    /// <returns></returns>
    public static bool ValidateJwtBearerToken(DefaultHttpContext httpContext, out JsonWebToken token, string headerKey = "Authorization", string tokenPrefix = "Bearer ")
    {
        // 获取 token
        var accessToken = GetJwtBearerToken(httpContext, headerKey, tokenPrefix);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            token = null;
            return false;
        }

        // 验证token
        var (IsValid, Token, _) = Validate(accessToken);
        token = IsValid ? Token : null;

        return IsValid;
    }

    /// <summary>
    /// 读取 Token，不含验证
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public static JsonWebToken ReadJwtToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || !accessToken.Contains('.'))
        {
            return default;
        }

        var tokenHandler = new JsonWebTokenHandler();
        if (!tokenHandler.CanReadToken(accessToken))
        {
            return default;
        }

        return tokenHandler.ReadJsonWebToken(accessToken);
    }

    /// <summary>
    /// 读取 Token，不含验证
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public static JwtSecurityToken SecurityReadJwtToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || !accessToken.Contains('.'))
        {
            return default;
        }

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        if (!jwtSecurityTokenHandler.CanReadToken(accessToken))
        {
            return default;
        }

        return jwtSecurityTokenHandler.ReadJwtToken(accessToken);
    }

    /// <summary>
    /// 获取 JWT Bearer Token
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="headerKey"></param>
    /// <param name="tokenPrefix"></param>
    /// <returns></returns>
    public static string GetJwtBearerToken(DefaultHttpContext httpContext, string headerKey = "Authorization", string tokenPrefix = "Bearer ")
    {
        // 判断请求报文头中是否有 "Authorization" 报文头
        var bearerToken = httpContext.Request.Headers[headerKey].ToString();
        if (string.IsNullOrWhiteSpace(bearerToken)) return default;

        var prefixLenght = tokenPrefix.Length;
        return bearerToken.StartsWith(tokenPrefix, true, null) && bearerToken.Length > prefixLenght ? bearerToken[prefixLenght..].Trim() : default;
    }

    /// <summary>
    /// 获取 JWT 配置
    /// </summary>
    /// <returns></returns>
    public static JWTSettingsOptions GetJWTSettings()
    {
        // 获取框架上下文
        _ = GetFrameworkContext();

        if (FrameworkApp == null)
        {
            Debug.WriteLine("No register the code `services.AddJwt()` on Startup.cs.");
        }

        var jwtSettingsOptions = _getJwtSettingsDelegate.Value(null);
        if (jwtSettingsOptions.Algorithm == null && jwtSettingsOptions.ExpiredTime == null)
        {
            SetDefaultJwtSettings(jwtSettingsOptions);
        }
        return jwtSettingsOptions;
    }

    /// <summary>
    /// 获取 JWT 配置方法委托
    /// </summary>
    private static Lazy<Func<IServiceProvider, JWTSettingsOptions>> _getJwtSettingsDelegate = new(() =>
    {
        var method = FrameworkApp.GetMethod("GetOptions").MakeGenericMethod(typeof(JWTSettingsOptions));
        return (Func<IServiceProvider, JWTSettingsOptions>)Delegate.CreateDelegate(typeof(Func<IServiceProvider, JWTSettingsOptions>), method);
    });

    /// <summary>
    /// 生成Token验证参数
    /// </summary>
    /// <param name="jwtSettings"></param>
    /// <returns></returns>
    public static TokenValidationParameters CreateTokenValidationParameters(JWTSettingsOptions jwtSettings)
    {
        return new TokenValidationParameters
        {
            // 验证签发方密钥
            ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey.Value,
            // 签发方密钥
            IssuerSigningKey = CreateSecurityKey(jwtSettings.Algorithm, jwtSettings.IssuerSigningKey),  // 使用公钥
            // 验证签发方
            ValidateIssuer = jwtSettings.ValidateIssuer.Value,
            // 设置签发方
            ValidIssuer = jwtSettings.ValidIssuer,
            // 验证签收方
            ValidateAudience = jwtSettings.ValidateAudience.Value,
            // 设置接收方
            ValidAudience = jwtSettings.ValidAudience,
            // 验证生存期
            ValidateLifetime = jwtSettings.ValidateLifetime.Value,
            // 过期时间容错值
            ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkew.Value),
            // 验证过期时间，设置 false 永不过期
            RequireExpirationTime = jwtSettings.RequireExpirationTime
        };
    }

    private static readonly ConcurrentDictionary<string, SecurityKey> _keyCache = new();

    /// <summary>
    /// 创建安全密钥
    /// </summary>
    /// <remarks>生成 RSA 密钥网站：https://www.ufreetools.com/zh/tool/rsa-key-pair-generator</remarks>
    /// <param name="algorithm"></param>
    /// <param name="issuerSigningKey"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>

    private static SecurityKey CreateSecurityKey(string algorithm, string issuerSigningKey)
    {
        var cacheKey = $"{algorithm.ToUpperInvariant().Trim()}:{issuerSigningKey?.GetHashCode()}";

        return _keyCache.GetOrAdd(cacheKey, _ =>
        {
            var algorithmUpper = algorithm.ToUpperInvariant();
            var keyContent = issuerSigningKey?.Trim();

            // HS* 对称加密
            if (algorithmUpper.StartsWith("HS"))
            {
                return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyContent));
            }
            // RS*/PS* RSA 非对称加密
            else if (algorithmUpper.StartsWith("RS") || algorithmUpper.StartsWith("PS"))
            {
                // 提取纯 Base64 内容
                static string ExtractBase64(string pem) => Regex.Replace(pem, @"-----BEGIN.*?-----|-----END.*?-----|\s", "");

                var rsa = RSA.Create();

                try
                {
                    // 标准 PKCS#8 格式
                    if (keyContent.Contains("BEGIN PRIVATE KEY") || keyContent.Contains("BEGIN PUBLIC KEY"))
                    {
                        rsa.ImportFromPem(keyContent);
                    }
                    // PKCS#1 RSA 私钥格式
                    else if (keyContent.Contains("BEGIN RSA PRIVATE KEY"))
                    {
                        var base64 = ExtractBase64(keyContent);
                        rsa.ImportRSAPrivateKey(Convert.FromBase64String(base64), out var _);
                    }
                    // PKCS#1 RSA 公钥格式
                    else if (keyContent.Contains("BEGIN RSA PUBLIC KEY"))
                    {
                        var base64 = ExtractBase64(keyContent);
                        rsa.ImportRSAPublicKey(Convert.FromBase64String(base64), out var _);
                    }
                    else
                    {
                        throw new CryptographicException("Unrecognized PEM format. Supported: PKCS#8, PKCS#1 RSA.");
                    }

                    return new RsaSecurityKey(rsa);
                }
                catch (Exception)
                {
                    rsa.Dispose();
                    throw;
                }
            }
            // ES* ECDSA 非对称加密
            else if (algorithmUpper.StartsWith("ES"))
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(keyContent);
                return new ECDsaSecurityKey(ecdsa);
            }
            else
            {
                throw new NotSupportedException($"The algorithm '{algorithm}' is not supported.");
            }
        });
    }

    /// <summary>
    /// 组合 Claims 负荷
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="expiredTime">过期时间，单位：分钟，最大支持 13 年</param>
    /// <returns></returns>
    private static (IDictionary<string, object> Payload, JWTSettingsOptions JWTSettings) CombinePayload(IDictionary<string, object> payload, long? expiredTime = null)
    {
        var jwtSettings = GetJWTSettings();
        var datetimeOffset = DateTimeOffset.UtcNow;

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Iat))
        {
            payload.Add(JwtRegisteredClaimNames.Iat, datetimeOffset.ToUnixTimeSeconds());
        }

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Nbf))
        {
            payload.Add(JwtRegisteredClaimNames.Nbf, datetimeOffset.ToUnixTimeSeconds());
        }

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Exp))
        {
            var minute = expiredTime ?? jwtSettings?.ExpiredTime ?? 20;
            payload.Add(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(minute).ToUnixTimeSeconds());
        }

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Iss))
        {
            payload.Add(JwtRegisteredClaimNames.Iss, jwtSettings?.ValidIssuer);
        }

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Aud))
        {
            payload.Add(JwtRegisteredClaimNames.Aud, jwtSettings?.ValidAudience);
        }

        return (payload, jwtSettings);
    }

    /// <summary>
    /// 设置默认 Jwt 配置
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    internal static JWTSettingsOptions SetDefaultJwtSettings(JWTSettingsOptions options)
    {
        options.ValidateIssuerSigningKey ??= true;
        if (options.ValidateIssuerSigningKey == true)
        {
            // 强制要求显式配置签名密钥
            if (string.IsNullOrWhiteSpace(options.IssuerSigningKey))
            {
                throw new InvalidOperationException(
                    "The JWT signing key has not been configured. Please set 'JwtSettings:IssuerSigningKey' in your configuration.");
            }
        }
        options.ValidateIssuer ??= true;
        if (options.ValidateIssuer == true)
        {
            options.ValidIssuer ??= "dotnetchina";
        }
        options.ValidateAudience ??= true;
        if (options.ValidateAudience == true)
        {
            options.ValidAudience ??= "powerby Furion";
        }
        options.ValidateLifetime ??= true;
        if (options.ValidateLifetime == true)
        {
            options.ClockSkew ??= 10;
        }
        options.ExpiredTime ??= 20;
        options.Algorithm ??= SecurityAlgorithms.HmacSha256;

        return options;
    }

    /// <summary>
    /// 获取当前的 HttpContext
    /// </summary>
    /// <returns></returns>
    private static HttpContext GetCurrentHttpContext()
    {
        return FrameworkApp.GetProperty("HttpContext").GetValue(null) as HttpContext;
    }

    /// <summary>
    /// 日期类型的 Claim 类型
    /// </summary>
    private static readonly string[] DateTypeClaimTypes = [JwtRegisteredClaimNames.Iat, JwtRegisteredClaimNames.Nbf, JwtRegisteredClaimNames.Exp];

    /// <summary>
    /// 框架 App 静态类
    /// </summary>
    internal static Type FrameworkApp { get; set; }

    /// <summary>
    /// 获取框架上下文
    /// </summary>
    /// <returns></returns>
    internal static Assembly GetFrameworkContext()
    {
        if (FrameworkApp != null) return FrameworkApp.Assembly;

        // 加载 Furion 程序集
        var furionAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("Furion"))
            ?? throw new InvalidOperationException("Unable to load Furion assembly.");

        // 获取 Furion.App 静态类
        FrameworkApp = furionAssembly.GetType("Furion.App")
            ?? throw new InvalidOperationException("Type 'Furion.App' not found in Furion assembly.");

        return furionAssembly;
    }
}