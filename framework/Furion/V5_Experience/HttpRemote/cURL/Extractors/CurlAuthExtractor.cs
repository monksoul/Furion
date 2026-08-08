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

namespace Furion.HttpRemote;

/// <summary>
///     cURL 身份认证提取器
/// </summary>
internal sealed class CurlAuthExtractor : IHttpCurlExtractor
{
    /// <summary>
    ///     需要携带参数的认证标志集合
    /// </summary>
    internal static readonly string[] _flagsWithArgument = ["-u", "--user", "--bearer"];

    /// <summary>
    ///     不需要携带参数的认证方案标志集合
    /// </summary>
    internal static readonly string[] _flagsWithoutArgument = ["--basic", "--digest", "--any", "--negotiate", "--ntlm"];

    /// <inheritdoc />
    public bool TryExtract(HttpRequestBuilder httpRequestBuilder, HttpCurlTokenExtractorContext context)
    {
        // 检查是否匹配带参数的认证标志
        if (context.CurrentTokenMatches(_flagsWithArgument))
        {
            // 预览下一个 Token
            var argument = context.PeekNext();

            // 空检查
            if (!string.IsNullOrWhiteSpace(argument))
            {
                // 处理带参数的认证
                ProcessAuthWithArgument(httpRequestBuilder, context.CurrentToken, argument);

                // 推进游标
                context.Advance(2);
            }
            else
            {
                // 推进游标
                context.Advance();
            }

            return true;
        }

        // 检查是否匹配不带参数的认证标志
        // ReSharper disable once InvertIf
        if (context.CurrentTokenMatches(_flagsWithoutArgument))
        {
            // 处理认证方案切换
            ProcessAuthScheme(httpRequestBuilder, context.CurrentToken);

            // 推进游标
            context.Advance();

            return true;
        }

        return false;
    }

    /// <summary>
    ///     处理带参数的认证标志
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="flag">当前匹配的命令标志</param>
    /// <param name="argument">携带的参数值</param>
    internal static void ProcessAuthWithArgument(HttpRequestBuilder httpRequestBuilder, string flag, string argument)
    {
        // 处理 Bearer Token
        if (string.Equals(flag, "--bearer", StringComparison.OrdinalIgnoreCase))
        {
            // 设置 Bearer 身份验证凭据请求授权标头
            httpRequestBuilder.AddBearerAuthentication(argument);

            return;
        }

        string username;
        string password;

        // 处理 -u 或 --user
        var colonIndex = argument.IndexOf(':');
        if (colonIndex > 0)
        {
            username = argument[..colonIndex];
            password = argument[(colonIndex + 1)..];
        }
        else
        {
            username = argument;
            password = string.Empty;
        }

        // 设置 Basic 身份验证凭据请求授权标头
        httpRequestBuilder.AddBasicAuthentication(username, password);
    }

    /// <summary>
    ///     处理不带参数的认证方案标志
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="flag">当前匹配的命令标志</param>
    internal static void ProcessAuthScheme(HttpRequestBuilder httpRequestBuilder, string flag)
    {
        // 获取当前的身份验证凭据请求授权标头
        var currentAuth = httpRequestBuilder.AuthenticationHeader;

        // 空检查
        if (currentAuth is null || string.IsNullOrWhiteSpace(currentAuth.Parameter))
        {
            return;
        }

        // 处理当前已设置了 Basic 认证
        if (string.Equals(currentAuth.Scheme, Constants.BASIC_AUTHENTICATION_SCHEME,
                StringComparison.OrdinalIgnoreCase))
        {
            // 解码 Base64 获取 username:password
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(currentAuth.Parameter));
            var colonIndex = decoded.IndexOf(':');

            // 解析出用户名和密码
            var username = colonIndex > 0 ? decoded[..colonIndex] : decoded;
            var password = colonIndex > 0 ? decoded[(colonIndex + 1)..] : string.Empty;

            // 根据最新的 flag 切换方案
            if (string.Equals(flag, "--digest", StringComparison.OrdinalIgnoreCase))
            {
                // 设置 Digest 摘要身份验证凭据请求授权标头
                httpRequestBuilder.AddDigestAuthentication(username, password);
            }
            else if (string.Equals(flag, "--basic", StringComparison.OrdinalIgnoreCase))
            {
                // 设置 Basic 身份验证凭据请求授权标头
                httpRequestBuilder.AddBasicAuthentication(username, password);
            }
        }
        // 处理当前已设置了 Digest 认证
        else if (string.Equals(currentAuth.Scheme, Constants.DIGEST_AUTHENTICATION_SCHEME,
                     StringComparison.OrdinalIgnoreCase))
        {
            // 使用 |:| 进行分割出用户名和密码
            var parts = currentAuth.Parameter.Split("|:|");

            // ReSharper disable once InvertIf
            if (parts.Length == 2)
            {
                // 根据最新的 flag 切换方案
                if (string.Equals(flag, "--basic", StringComparison.OrdinalIgnoreCase))
                {
                    // 设置 Basic 身份验证凭据请求授权标头
                    httpRequestBuilder.AddBasicAuthentication(parts[0], parts[1]);
                }
                else if (string.Equals(flag, "--digest", StringComparison.OrdinalIgnoreCase))
                {
                    // 设置 Digest 摘要身份验证凭据请求授权标头
                    httpRequestBuilder.AddDigestAuthentication(parts[0], parts[1]);
                }
            }
        }
    }
}