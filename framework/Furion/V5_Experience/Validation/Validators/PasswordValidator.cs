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

using Furion.Validation.Resources;
using System.Text.RegularExpressions;

namespace Furion.Validation;

/// <summary>
///     密码验证器
/// </summary>
/// <remarks>
///     支持普通和强密码两种模式：
///     普通模式：密码长度为 8-64 位，包含至少一个字母和一个数字。
///     强密码模式：密码长度为 12-64 位，必须包含大小写字母、数字、任意非空白特殊字符。
/// </remarks>
public partial class PasswordValidator : ValidatorBase
{
    /// <summary>
    ///     <inheritdoc cref="PasswordValidator" />
    /// </summary>
    public PasswordValidator() => UseResourceKey(GetResourceKey);

    /// <summary>
    ///     是否启用强密码验证模式
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool Strong { get; set; }

    /// <inheritdoc />
    public override bool IsValid(object? value, IValidationContext? validationContext) =>
        value switch
        {
            null => true,
            string text => !string.IsNullOrWhiteSpace(text) &&
                           (Strong ? StrongRegex().IsMatch(text) : Regex().IsMatch(text)),
            _ => false
        };

    /// <summary>
    ///     获取错误信息对应的资源键
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal string GetResourceKey() =>
        Strong
            ? nameof(ValidationMessages.PasswordValidator_ValidationError_Strong)
            : nameof(ValidationMessages.PasswordValidator_ValidationError);

    /// <summary>
    ///     普通密码正则表达式
    /// </summary>
    /// <remarks>至少包含一个字母和一个数字，长度 8-64 位。</remarks>
    [GeneratedRegex(@"\A(?=.*[a-zA-Z])(?=.*\d).{8,64}\z")]
    private static partial Regex Regex();

    /// <summary>
    ///     强密码正则表达式
    /// </summary>
    /// <remarks>必须包含大小写、数字、特殊字符，长度 12-64 位。</remarks>
    [GeneratedRegex(@"\A(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s]).{12,64}\z")]
    private static partial Regex StrongRegex();
}