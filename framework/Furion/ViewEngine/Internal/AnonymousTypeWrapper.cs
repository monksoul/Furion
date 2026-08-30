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
using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace Furion.ViewEngine;

/// <summary>
/// 匿名类型包装器
/// </summary>
public class AnonymousTypeWrapper : DynamicObject
{
    /// <summary>
    /// 匿名模型
    /// </summary>
    private readonly object _model;

    /// <summary>
    /// 属性缓存
    /// </summary>
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo?>> _propertyCache = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="model"></param>
    public AnonymousTypeWrapper(object model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <summary>
    /// 获取成员信息
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        var modelType = _model.GetType();

        if (_model is IDictionary<string, object> dictionary && dictionary.TryGetValue(binder.Name, out var dictValue))
        {
            result = dictValue;
            return true;
        }

        var typeProperties = _propertyCache.GetOrAdd(modelType, static type =>
            new ConcurrentDictionary<string, PropertyInfo?>(StringComparer.Ordinal));

        var propertyInfo = typeProperties.GetOrAdd(binder.Name, static (name, type) => type.GetProperty(name), modelType);

        if (propertyInfo == null)
        {
            result = null;
            return false;
        }

        result = propertyInfo.GetValue(_model, null);

        if (result == null)
        {
            return true;
        }

        if (result.IsAnonymous())
        {
            result = new AnonymousTypeWrapper(result);
            return true;
        }

        if (result is not string && result is IEnumerable enumerable)
        {
            result = ConvertEnumerable(enumerable);
        }

        return true;
    }

    /// <summary>
    /// 将集合中的匿名类型元素包装为 <see cref="AnonymousTypeWrapper"/>
    /// </summary>
    /// <param name="enumerable"></param>
    /// <returns></returns>
    private static IEnumerable ConvertEnumerable(IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            if (item == null)
            {
                yield return null;
            }
            else if (item.IsAnonymous())
            {
                yield return new AnonymousTypeWrapper(item);
            }
            else
            {
                yield return item;
            }
        }
    }
}