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

using Furion.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 声明式 <see cref="ValidationAttribute" /> 特性提取器
/// </summary>
internal sealed class ValidationDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeExtractorContext context)
    {
        // 检查方法或接口是否贴有 [SuppressValidation] 特性
        if (context.IsMethodDefined<SuppressValidationAttribute>(out var suppressValidationAttribute, true) &&
            suppressValidationAttribute.Enabled)
        {
            return;
        }

        // 遍历所有非冻结类型参数并进行验证操作
        foreach (var (parameter, value) in context.UnFrozenParameters)
        {
            ValidateParameter(parameter, value);
        }
    }

    /// <summary>
    ///     验证参数
    /// </summary>
    /// <param name="parameter">
    ///     <see cref="ParameterInfo" />
    /// </param>
    /// <param name="value">参数的值</param>
    /// <exception cref="ValidationException"></exception>
    internal static void ValidateParameter(ParameterInfo parameter, object? value)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameter);

        // 获取参数名和参数类型
        var parameterName = parameter.Name!;
        var parameterType = parameter.ParameterType;

        // 空检查
        if (value is null)
        {
            // 优先验证 [Required] 特性
            if (parameter.IsDefined(typeof(RequiredAttribute), true))
            {
                throw new ValidationException(parameter.GetCustomAttribute<RequiredAttribute>(true)
                    ?.FormatErrorMessage(parameterName));
            }

            return;
        }

        // 检查参数是否贴有验证特性
        if (parameter.IsDefined(typeof(ValidationAttribute), true))
        {
            // 验证单个值类型
            Validator.ValidateValue(value, new ValidationContext(value) { MemberName = parameterName },
                parameter.GetCustomAttributes<ValidationAttribute>(true));
        }

        // 检查类型是否是基本类型或枚举类型或由它们组成的数组或集合类型
        if (!parameterType.IsBaseTypeOrEnumOrCollection())
        {
            // 验证复杂对象类型
            Validator.ValidateObject(value, new ValidationContext(value), true);
        }
    }
}