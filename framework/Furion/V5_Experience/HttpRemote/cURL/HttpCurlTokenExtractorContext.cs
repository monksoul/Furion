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

namespace Furion.HttpRemote;

/// <summary>
///     cURL Token 提取器上下文
/// </summary>
public sealed class HttpCurlTokenExtractorContext
{
    /// <summary>
    ///     <inheritdoc cref="HttpCurlTokenExtractorContext" />
    /// </summary>
    /// <param name="tokens">Token 集合</param>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpCurlTokenExtractorContext(IReadOnlyList<string> tokens)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(tokens);

        Tokens = tokens;
        CurrentIndex = 0;
    }

    /// <summary>
    ///     Token 集合
    /// </summary>
    public IReadOnlyList<string> Tokens { get; }

    /// <summary>
    ///     当前索引位置
    /// </summary>
    public int CurrentIndex { get; private set; }

    /// <summary>
    ///     获取当前 Token
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public string CurrentToken => CurrentIndex < Tokens.Count
        ? Tokens[CurrentIndex]
        : throw new InvalidOperationException("Token index out of range.");

    /// <summary>
    ///     是否还有下一个 Token
    /// </summary>
    public bool HasNext => CurrentIndex < Tokens.Count - 1;

    /// <summary>
    ///     是否已到达末尾
    /// </summary>
    public bool IsEndOfTokens => CurrentIndex >= Tokens.Count;

    /// <summary>
    ///     预览下一个 Token（不移动指针）
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string? PeekNext() => HasNext ? Tokens[CurrentIndex + 1] : null;

    /// <summary>
    ///     前进指定步数
    /// </summary>
    /// <param name="count">前进步数，默认值为 <c>1</c>。</param>
    public void Advance(int count = 1) => CurrentIndex += count;

    /// <summary>
    ///     重置指针到起始位置
    /// </summary>
    public void Reset() => CurrentIndex = 0;

    /// <summary>
    ///     检查当前 Token 是否匹配指定的任一值（不区分大小写）
    /// </summary>
    /// <param name="values">要匹配的值集合</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool CurrentTokenMatches(params string[] values) =>
        !IsEndOfTokens && values.Any(u => string.Equals(CurrentToken, u, StringComparison.OrdinalIgnoreCase));
}