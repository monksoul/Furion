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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Furion.Extensions;

/// <summary>
///     <see cref="MethodInfo" /> 扩展类
/// </summary>
internal static class MethodInfoExtensions
{
    /// <summary>
    ///     检查是否定义了指定特性
    /// </summary>
    /// <param name="method">
    ///     <see cref="MethodInfo" />
    /// </param>
    /// <param name="attribute">
    ///     <typeparamref name="TAttribute" />
    /// </param>
    /// <param name="inherit">是否在基类中搜索</param>
    /// <typeparam name="TAttribute">
    ///     <see cref="Attribute" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsDefined<TAttribute>(this MethodInfo method, [NotNullWhen(true)] out TAttribute? attribute,
        bool inherit = false)
        where TAttribute : Attribute
    {
        // 获取指定特性实例
        attribute = method.GetCustomAttribute<TAttribute>(inherit);

        // 检查是否定义了指定特性
        var isDefined = attribute != null || method.IsDefined(typeof(TAttribute), inherit);
        if (isDefined || !inherit)
        {
            return isDefined;
        }

        // 尝试查找所在声明类是否定义了指定特性
        attribute = method.DeclaringType?.GetCustomAttribute<TAttribute>(inherit);
        isDefined = attribute != null || method.DeclaringType?.IsDefined(typeof(TAttribute), inherit) == true;

        return isDefined;
    }

    /// <summary>
    ///     获取指定特性的所有实例
    /// </summary>
    /// <param name="method">
    ///     <see cref="MethodInfo" />
    /// </param>
    /// <param name="inherit">是否在基类中搜索</param>
    /// <param name="methodScanFirst">是否优先查找 <see cref="MethodInfo" /> 的特性。默认值为：<c>true</c>。</param>
    /// <typeparam name="TAttribute">
    ///     <see cref="Attribute" />
    /// </typeparam>
    /// <returns>
    ///     <typeparamref name="TAttribute" /><c>[]</c>
    /// </returns>
    internal static TAttribute[]? GetDefinedCustomAttributes<TAttribute>(this MethodInfo method, bool inherit = false,
        bool methodScanFirst = true)
        where TAttribute : Attribute
    {
        // 初始化指定特性集合
        var attributes = new List<TAttribute>();

        // 获取指定特性集合
        attributes.AddRange(method.GetCustomAttributes<TAttribute>(inherit));

        // 尝试获取所在声明类上指定特性集合
        // ReSharper disable once InvertIf
        if (inherit && method.DeclaringType is not null)
        {
            var declaringAttributes = method.DeclaringType.GetCustomAttributes<TAttribute>(inherit);

            // 是否优先查找方法特性
            if (methodScanFirst)
            {
                attributes.AddRange(declaringAttributes);
            }
            // 否则添加到头部
            else
            {
                attributes.InsertRange(0, declaringAttributes);
            }
        }

        return attributes.Count > 0 ? attributes.ToArray() : null;
    }

    /// <summary>
    ///     输出方法签名的友好字符串
    /// </summary>
    /// <param name="method">
    ///     <see cref="MethodBase" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ToFriendlyString(this MethodBase? method)
    {
        // 空检查
        if (method is null)
        {
            return null;
        }

        // 获取方法名
        var methodName = method.Name;

        // 获取方法返回值
        var returnType = method is MethodInfo methodInfo
            ? methodInfo.ReturnType.ToFriendlyString()
            : "System.Void"; // 构造函数无返回值

        var skipCount = 0;
        var declaringType = method.DeclaringType;

        // 检查方法的声明类型是否是泛型且是开放类型
        if (declaringType is { IsGenericType: true, ContainsGenericParameters: true })
        {
            skipCount = declaringType.GetGenericArguments().Length;
        }

        // 处理泛型方法
        var genericArguments = method.IsGenericMethod
            ? method.GetGenericArguments().Skip(skipCount).Select(t => t.ToFriendlyString()).ToArray()
            : [];

        // 获取参数列表
        var parameters = method.GetParameters().Select(p => p.ParameterType.ToFriendlyString());

        // 组合字符串
        var genericPart = genericArguments.Length != 0 ? $"<{string.Join(", ", genericArguments)}>" : string.Empty;

        return $"{returnType} {methodName}{genericPart}({string.Join(", ", parameters)})";
    }
}