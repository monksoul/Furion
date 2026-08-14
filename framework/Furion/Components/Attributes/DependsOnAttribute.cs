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

using Furion.Components;
using Furion.Reflection;

namespace System;

/// <summary>
/// 组件依赖配置特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class DependsOnAttribute : Attribute
{
    /// <summary>
    /// 依赖组件列表
    /// </summary>
    private Type[] _dependComponents = [];

    /// <summary>
    /// 链接组件列表
    /// </summary>
    private Type[] _links = [];

    /// <summary>
    /// 构造函数
    /// </summary>
    public DependsOnAttribute()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dependComponents">依赖组件列表</param>
    /// <remarks>支持字符串类型程序集/类型配置</remarks>
    public DependsOnAttribute(params object[] dependComponents)
    {
        DependComponents = ParseTypes(dependComponents);
    }

    /// <summary>
    /// 依赖组件列表
    /// </summary>
    public Type[] DependComponents
    {
        get => _dependComponents;
        set
        {
            var components = value ?? [];
            ValidateComponents(components);
            _dependComponents = components;
        }
    }

    /// <summary>
    /// 链接组件列表
    /// </summary>
    public Type[] Links
    {
        get => _links;
        set
        {
            var components = value ?? [];
            ValidateComponents(components);
            _links = components;
        }
    }

    /// <summary>
    /// 将 object 数组转换为 Type 数组，支持字符串
    /// </summary>
    /// <param name="components">原始组件集合</param>
    /// <returns></returns>
    private static Type[] ParseTypes(object[] components)
    {
        if (components == null || components.Length == 0) return [];

        var types = new Type[components.Length];
        var count = 0;

        // 遍历所有依赖组件
        foreach (var component in components)
        {
            // 如果是类型自动载入
            if (component is Type componentType)
            {
                types[count++] = componentType;
            }
            // 处理字符串配置模式
            else if (component is string typeString)
            {
                types[count++] = Reflect.GetStringType(typeString);
            }
            else throw new InvalidOperationException("Component type can only be `Type` or `String` type of specific format.");
        }

        if (count == types.Length) return types;

        Array.Resize(ref types, count);
        return types;
    }

    /// <summary>
    /// 验证组件类型合法性
    /// </summary>
    /// <param name="types">待验证类型数组</param>
    private static void ValidateComponents(Type[] types)
    {
        foreach (var type in types)
        {
            // 检查类型是否实现 IComponent 接口
            if (!typeof(IComponent).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"The type of `{type.FullName}` must be assignable from `{nameof(IComponent)}`.");
            }
        }
    }
}