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
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 声明式 <see cref="QueryParamAttribute" /> 特性提取器
/// </summary>
internal sealed class QueryParamDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeParsingContext context)
    {
        /* 情况一：当特性作用于方法或接口时 */

        // 获取 QueryParamAttribute 特性集合
        var queryAttributes = context.GetMethodDefinedCustomAttributes<QueryParamAttribute>(true, false)?.ToArray();

        // 空检查
        if (queryAttributes is { Length: > 0 })
        {
            // 遍历所有 [QueryParam] 特性并添加到 HttpRequestBuilder 中
            foreach (var queryAttribute in queryAttributes)
            {
                // 获取查询参数键
                var queryName = queryAttribute.Name;

                // 空检查
                ArgumentException.ThrowIfNullOrEmpty(queryName);

                // 设置查询参数
                if (queryAttribute.HasSetValue)
                {
                    httpRequestBuilder.WithQueryParameter(queryName, queryAttribute.Value, queryAttribute.Replace,
                        queryAttribute.IgnoreNullValues, queryAttribute.Format);
                }
                // 移除查询参数
                else
                {
                    httpRequestBuilder.RemoveQueryParameters(queryName);
                }
            }
        }

        /* 情况二：当特性作用于参数时 */

        // 查找所有贴有 [QueryParam] 特性的参数集合
        var queryParameters = context.UnFrozenParameters.Where(u => u.Key.IsDefined(typeof(QueryParamAttribute), true))
            .ToArray();

        // 空检查
        if (queryParameters.Length == 0)
        {
            return;
        }

        // 遍历所有贴有 [QueryParam] 特性的参数
        foreach (var (parameter, value) in queryParameters)
        {
            // 获取 QueryParamAttribute 特性集合
            var parameterQueryAttributes = parameter.GetCustomAttributes<QueryParamAttribute>(true);

            // 获取参数名
            var parameterName = AliasAsUtility.GetParameterName(parameter, out var aliasAsDefined);

            // 遍历所有 [QueryParam] 特性并添加到 HttpRequestBuilder 中
            foreach (var queryAttribute in parameterQueryAttributes)
            {
                // 检查参数是否贴了 [AliasAs] 特性
                if (!aliasAsDefined)
                {
                    parameterName = string.IsNullOrWhiteSpace(queryAttribute.AliasAs)
                        ? string.IsNullOrWhiteSpace(queryAttribute.Name) ? parameterName : queryAttribute.Name.Trim()
                        : queryAttribute.AliasAs.Trim();
                }

                // 检查类型是否是基本类型或枚举类型或由它们组成的数组或集合类型
                if (parameter.ParameterType.IsBaseTypeOrEnumOrCollection())
                {
                    httpRequestBuilder.WithQueryParameter(parameterName, value ?? queryAttribute.Value,
                        queryAttribute.Replace, queryAttribute.IgnoreNullValues, queryAttribute.Format);

                    continue;
                }

                // 空检查
                if (value is not null)
                {
                    httpRequestBuilder.WithQueryParameters(value, queryAttribute.Prefix, queryAttribute.Replace,
                        queryAttribute.IgnoreNullValues);
                }
            }
        }
    }
}