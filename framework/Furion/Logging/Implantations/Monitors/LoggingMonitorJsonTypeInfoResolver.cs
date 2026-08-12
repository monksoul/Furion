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

using System.Text.Json.Serialization.Metadata;

namespace Furion.Logging;

/// <summary>
/// 支持忽略特定属性和类型的解析器
/// </summary>
internal sealed class LoggingMonitorJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    /// <summary>
    /// 被忽略的属性名称集合
    /// </summary>
    private readonly HashSet<string> _ignoreNames;

    /// <summary>
    /// 被忽略的属性类型集合
    /// </summary>
    private readonly HashSet<Type> _ignoreTypes;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="ignoreNames">忽略的属性名称</param>
    /// <param name="ignoreTypes">忽略的属性类型</param>
    public LoggingMonitorJsonTypeInfoResolver(string[] ignoreNames, Type[] ignoreTypes)
    {
        _ignoreNames = new HashSet<string>(ignoreNames ?? [], StringComparer.OrdinalIgnoreCase);
        _ignoreTypes = new HashSet<Type>(ignoreTypes ?? []);

        Modifiers.Add(typeInfo =>
        {
            if (typeInfo.Kind == JsonTypeInfoKind.Object)
            {
                for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
                {
                    var prop = typeInfo.Properties[i];
                    if (_ignoreNames.Contains(prop.Name) || _ignoreTypes.Contains(prop.PropertyType))
                    {
                        typeInfo.Properties.RemoveAt(i);
                    }
                }
            }
        });
    }
}