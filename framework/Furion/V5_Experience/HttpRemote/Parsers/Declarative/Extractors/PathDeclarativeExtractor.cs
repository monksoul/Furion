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
using Furion.Utilities;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 声明式路径参数提取器
/// </summary>
internal sealed class PathDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeParsingContext context)
    {
        /* 情况一：当特性作用于方法或接口时 */

        // 获取 PathAttribute 特性集合
        var pathAttributes = context.GetMethodDefinedCustomAttributes<PathAttribute>(true, false)?.ToArray();

        // 空检查
        if (pathAttributes is { Length: > 0 })
        {
            // 遍历所有 [Path] 特性并添加到 HttpRequestBuilder 中
            foreach (var pathAttribute in pathAttributes)
            {
                // 设置路径参数
                httpRequestBuilder.WithPathParameter(pathAttribute.Name, pathAttribute.Value);
            }
        }

        /* 情况二：将所有非冻结类型参数添加到路径参数中 */

        // 遍历所有路径参数
        foreach (var (parameter, value) in context.UnFrozenParameters)
        {
            // 获取参数名
            var parameterName = AliasAsUtility.GetParameterName(parameter, out _);

            // 检查类型是否是基本类型或枚举类型或由它们组成的数组或集合类型
            if (parameter.ParameterType.IsBaseTypeOrEnumOrCollection())
            {
                // 设置路径参数
                httpRequestBuilder.WithPathParameter(parameterName, value);

                continue;
            }

            // 设置路径参数
            httpRequestBuilder.WithPathParameters(value, parameterName);
        }
    }
}