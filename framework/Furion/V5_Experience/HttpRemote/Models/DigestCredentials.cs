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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Furion.HttpRemote;

/// <summary>
///     摘要认证
/// </summary>
public sealed class DigestCredentials
{
    /// <summary>
    ///     用户名
    /// </summary>
    public string? Username { get; private init; }

    /// <summary>
    ///     密码
    /// </summary>
    public string? Password { get; private init; }

    /// <summary>
    ///     服务器提供的认证领域
    /// </summary>
    /// <remarks>服务器通过 <c>WWW-Authenticate</c> 响应标头返回。</remarks>
    public string? Realm { get; private init; }

    /// <summary>
    ///     服务器提供的随机数
    /// </summary>
    /// <remarks>服务器通过 <c>WWW-Authenticate</c> 响应标头返回。</remarks>
    public string? Nonce { get; private init; }

    /// <summary>
    ///     保护质量
    /// </summary>
    /// <remarks>服务器通过 <c>WWW-Authenticate</c> 响应标头返回。</remarks>
    public string? Qop { get; private init; }

    /// <summary>
    ///     摘要算法
    /// </summary>
    /// <remarks>支持 <c>MD5</c> 和 <c>MD5-sess</c>。</remarks>
    public string? Algorithm { get; private init; }

    /// <summary>
    ///     非一次性计数器
    /// </summary>
    public int Nc { get; private init; }

    /// <summary>
    ///     客户端提供的随机数
    /// </summary>
    public string? CNonce { get; private init; }

    /// <summary>
    ///     服务器提供的不透明数据
    /// </summary>
    /// <remarks>服务器通过 <c>WWW-Authenticate</c> 响应标头返回，客户端需原样返回。</remarks>
    public string? Opaque { get; private init; }

    /// <summary>
    ///     获取 Digest 摘要认证授权凭证
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <param name="httpMethod">
    ///     <see cref="HttpMethod" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetDigestCredentials(string? requestUri, string username, string password,
        HttpMethod httpMethod)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNull(httpMethod);

        // 初始化 HttpClient 实例
        using var httpClient = new HttpClient();

        // 设置默认 User-Agent
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, UserAgents.Edge.PC);

        // 为 HttpClient 启用标准请求标头
        httpClient.UseStandardRequestHeaders();

        try
        {
            // 发送 HTTP 远程请求（默认 HEAD 请求）
            using var httpResponseMessage = httpClient.Send(new HttpRequestMessage(HttpMethod.Head, requestUri),
                HttpCompletionOption.ResponseHeadersRead);

            // 检查响应状态码是否是 401 且响应标头是否包含 WWW-Authenticate 
            if (httpResponseMessage is not
                { StatusCode: HttpStatusCode.Unauthorized, Headers.WwwAuthenticate.Count: > 0 })
            {
                throw new InvalidOperationException(
                    "Unable to initiate digest authentication: The server did not return a 401 Unauthorized status or the `WWW-Authenticate` header is missing.");
            }

            // 从 WWW-Authenticate 标头中筛选出 Digest 方案
            var digestChallenge = httpResponseMessage.Headers.WwwAuthenticate.FirstOrDefault(h =>
                h.Scheme.Equals(Constants.DIGEST_AUTHENTICATION_SCHEME, StringComparison.OrdinalIgnoreCase))?.Parameter;

            // 空检查
            if (string.IsNullOrWhiteSpace(digestChallenge))
            {
                throw new InvalidOperationException(
                    "The `WWW-Authenticate` header does not contain a Digest challenge.");
            }

            // 创建 DigestCredentials 实例并生成授权凭证
            var digestCredentials = Create(username, password, digestChallenge)
                .GenerateCredentials(httpResponseMessage.RequestMessage?.RequestUri?.PathAndQuery, httpMethod);

            return digestCredentials;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to obtain digest credentials.", e);
        }
    }

    /// <summary>
    ///     创建 <see cref="DigestCredentials" /> 实例
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <param name="wwwAuthenticateValue">服务器响应标头 <c>WWW-Authenticate</c> 的值</param>
    /// <param name="nc">非一次性计数器；默认值为：1</param>
    /// <returns>
    ///     <see cref="DigestCredentials" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal static DigestCredentials Create(string username, string password, string wwwAuthenticateValue, int nc = 1)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(wwwAuthenticateValue);

        // 从响应标头 WWW-Authenticate 的值中解析各个参数
        var realm = ExtractParameterValueFromHeader("realm", wwwAuthenticateValue);
        var nonce = ExtractParameterValueFromHeader("nonce", wwwAuthenticateValue);
        var qop = ExtractParameterValueFromHeader("qop", wwwAuthenticateValue);
        var opaque = ExtractParameterValueFromHeader("opaque", wwwAuthenticateValue);
        var algorithm = ExtractParameterValueFromHeader("algorithm", wwwAuthenticateValue) ?? "MD5";

        // 根据 RFC 7616 规范，realm 和 nonce 是服务器挑战中绝对必需的参数
        if (string.IsNullOrWhiteSpace(realm) || string.IsNullOrWhiteSpace(nonce))
        {
            throw new InvalidOperationException("Missing required 'realm' or 'nonce' in WWW-Authenticate header.");
        }

        // 检查是否是 MD5 和 MD5-sess 算法
        if (!algorithm.Equals("MD5", StringComparison.OrdinalIgnoreCase) &&
            !algorithm.Equals("MD5-sess", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unsupported digest algorithm '{algorithm}'. Only MD5 and MD5-sess are supported.");
        }

        // 处理 qop 多值情况
        string? selectedQop = null;

        // 空检查 
        if (!string.IsNullOrWhiteSpace(qop))
        {
            var qopOptions = qop.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            selectedQop = qopOptions.FirstOrDefault(o => o.Equals("auth", StringComparison.OrdinalIgnoreCase));

            // 空检查
            if (selectedQop is null)
            {
                throw new InvalidOperationException($"Server requested qop '{qop}', but only 'auth' is supported.");
            }
        }

        // 生成随机值
        var cNonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

        // 初始化 DigestCredentials 实例
        return new DigestCredentials
        {
            Username = username,
            Password = password,
            Realm = realm,
            Nonce = nonce,
            Qop = selectedQop,
            Algorithm = algorithm,
            Nc = nc > 0 ? nc : 1,
            CNonce = cNonce,
            Opaque = opaque
        };
    }

    /// <summary>
    ///     生成摘要认证授权凭证
    /// </summary>
    /// <param name="digestUri">请求相对地址（不包含主机地址）</param>
    /// <param name="httpMethod">
    ///     <see cref="HttpMethod" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    internal string GenerateCredentials(string? digestUri, HttpMethod httpMethod)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(digestUri);
        ArgumentNullException.ThrowIfNull(httpMethod);

        // 计算基础 HA1
        var ha1Base = GenerateMd5Hash($"{Username}:{Realm}:{Password}");

        // 根据算法计算最终 HA1（MD5-sess 需要再散列一次）
        var ha1 = Algorithm?.Equals("MD5-sess", StringComparison.OrdinalIgnoreCase) == true
            ? GenerateMd5Hash($"{ha1Base}:{Nonce}:{CNonce}")
            : ha1Base;

        var ha2 = GenerateMd5Hash($"{httpMethod}:{digestUri}");

        string digestResponse;
        var parts = new List<string>
        {
            $"username=\"{Username}\"",
            $"realm=\"{Realm}\"",
            $"nonce=\"{Nonce}\"",
            $"uri=\"{digestUri}\"",
            $"algorithm={Algorithm ?? "MD5"}"
        };

        // 空检查
        if (!string.IsNullOrWhiteSpace(Qop))
        {
            digestResponse = GenerateMd5Hash($"{ha1}:{Nonce}:{Nc:x8}:{CNonce}:{Qop}:{ha2}");
            parts.Add($"qop={Qop}");
            parts.Add($"nc={Nc:x8}");
            parts.Add($"cnonce=\"{CNonce}\"");
        }
        else
        {
            digestResponse = GenerateMd5Hash($"{ha1}:{Nonce}:{ha2}");
        }

        parts.Add($"response=\"{digestResponse}\"");

        // 空检查
        if (!string.IsNullOrWhiteSpace(Opaque))
        {
            parts.Add($"opaque=\"{Opaque}\"");
        }

        return string.Join(", ", parts);
    }

    /// <summary>
    ///     从服务器响应标头 <c>WWW-Authenticate</c> 的值中提取参数值
    /// </summary>
    /// <param name="name">参数名</param>
    /// <param name="wwwAuthenticateValue">服务器响应标头 <c>WWW-Authenticate</c> 的值</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static string? ExtractParameterValueFromHeader(string name, string wwwAuthenticateValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(wwwAuthenticateValue);

        var match = new Regex($"""
                               {name}=(?:"([^"]*)"|([^,\s]+))
                               """).Match(wwwAuthenticateValue);

        return match.Success ? match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value : null;
    }

    /// <summary>
    ///     生成 MD5 哈希
    /// </summary>
    /// <param name="input">值</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static string GenerateMd5Hash(string input)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(input);

        return string.Concat(MD5.HashData(Encoding.UTF8.GetBytes(input)).Select(x => x.ToString("x2")));
    }
}