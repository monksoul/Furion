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

using System.Reflection;

namespace Furion.Reflection.Extensions;

/// <summary>
/// Method Info 扩展
/// </summary>
public static class MethodInfoExtensions
{
    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static IEnumerable<Attribute> GetActualCustomAttributes(this MethodInfo method, object target)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes();
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static object[] GetActualCustomAttributes(this MethodInfo method, object target, bool inherit)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes(inherit);
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="attributeType"></param>
    /// <returns></returns>
    public static IEnumerable<Attribute> GetActualCustomAttributes(this MethodInfo method, object target, Type attributeType)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes(attributeType);
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="attributeType"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static object[] GetActualCustomAttributes(this MethodInfo method, object target, Type attributeType, bool inherit)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes(attributeType, inherit);
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static IEnumerable<TAttribute> GetActualCustomAttributes<TAttribute>(this MethodInfo method, object target)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes<TAttribute>();
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static IEnumerable<TAttribute> GetActualCustomAttributes<TAttribute>(this MethodInfo method, object target, bool inherit)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes<TAttribute>(inherit);
    }

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="attributeType"></param>
    /// <returns></returns>
    public static Attribute GetActualCustomAttribute(this MethodInfo method, object target, Type attributeType)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttribute(attributeType);
    }

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="attributeType"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static Attribute GetActualCustomAttribute(this MethodInfo method, object target, Type attributeType, bool inherit)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttribute(attributeType, inherit);
    }

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static TAttribute GetActualCustomAttribute<TAttribute>(this MethodInfo method, object target)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttribute<TAttribute>();
    }

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static TAttribute GetActualCustomAttribute<TAttribute>(this MethodInfo method, object target, bool inherit)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttribute<TAttribute>(inherit);
    }

    /// <summary>
    /// 获取实际方法对象
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private static MethodInfo GetActualMethodInfo(MethodInfo method, object target)
    {
        if (target == null) return default;

        var targetType = target.GetType();

        var methodString = method.ToString();
        var actualMethod = targetType.GetMethods()
                                             .FirstOrDefault(u => u.ToString() == methodString);

        if (actualMethod == null) return default;

        return actualMethod;
    }
}