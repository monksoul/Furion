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
    /// <remarks>服务器通过 <c>WWW-Authenticate</c> 响应标头返回，客户端需原样回去。</remarks>
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

        // 启用性能优化
        httpClient.PerformanceOptimization();

        try
        {
            // 发送 HTTP 远程请求
            using var httpResponseMessage = httpClient.Send(new HttpRequestMessage(httpMethod, requestUri),
                HttpCompletionOption.ResponseHeadersRead);

            // 检查响应状态码是否是 401 且响应标头是否包含 WWW-Authenticate 
            if (httpResponseMessage is not
                { StatusCode: HttpStatusCode.Unauthorized, Headers.WwwAuthenticate.Count: > 0 })
            {
                throw new InvalidOperationException(
                    "Unable to initiate digest authentication: The server did not return a 401 Unauthorized status or the `WWW-Authenticate` header is missing.");
            }

            // 创建 DigestCredentials 实例并生成授权凭证
            var digestCredentials =
                Create(username, password, httpResponseMessage.Headers.WwwAuthenticate.First().ToString())
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
    /// <returns>
    ///     <see cref="DigestCredentials" />
    /// </returns>
    internal static DigestCredentials Create(string username, string password, string wwwAuthenticateValue)
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
        var cNonce = RandomNumberGenerator.GetInt32(123400, 9999999).ToString();

        // 初始化 DigestCredentials 实例
        return new DigestCredentials
        {
            Username = username,
            Password = password,
            Realm = realm,
            Nonce = nonce,
            Qop = qop,
            Nc = 1, // 注意
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
    internal string GenerateCredentials(string? digestUri, HttpMethod httpMethod)
    {
        var ha1 = GenerateMd5Hash($"{Username}:{Realm}:{Password}");
        var ha2 = GenerateMd5Hash($"{httpMethod}:{digestUri}");

        var digestResponse =
            GenerateMd5Hash(
                $"{ha1}:{Nonce}:{Nc:00000000}:{CNonce}:{Qop}:{ha2}");

        var credentials =
            $"username=\"{Username}\", realm=\"{Realm}\", nonce=\"{Nonce}\", uri=\"{digestUri}\", " +
            $"algorithm=MD5, qop={Qop}, nc={Nc:00000000}, cnonce=\"{CNonce}\", " +
            $"response=\"{digestResponse}\", opaque=\"{Opaque}\"";

        return credentials;
    }

    /// <summary>
    ///     从服务器响应标头 <c>WWW-Authenticate</c> 的值中提取参数值
    /// </summary>
    /// <param name="name">参数名</param>
    /// <param name="wwwAuthenticateValue">服务器响应标头 <c>WWW-Authenticate</c> 的值</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ExtractParameterValueFromHeader(string name, string wwwAuthenticateValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(wwwAuthenticateValue);

        var match = new Regex($"""
                               {name}="([^"]*)"
                               """).Match(wwwAuthenticateValue);

        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    ///     生成 MD5 哈希
    /// </summary>
    /// <param name="input">值</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GenerateMd5Hash(string input)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(input);

        return string.Concat(MD5.HashData(Encoding.UTF8.GetBytes(input)).Select(x => x.ToString("x2")));
    }
}