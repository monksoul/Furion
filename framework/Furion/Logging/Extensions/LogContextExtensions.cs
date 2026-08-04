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

namespace Furion.Logging;

/// <summary>
/// LogContext 扩展
/// </summary>
public static class LogContextExtensions
{
    /// <summary>
    /// 设置上下文数据
    /// </summary>
    /// <param name="logContext"></param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static LogContext Set(this LogContext logContext, object key, object value)
    {
        if (logContext == null || key == null) return logContext;

        logContext.Properties ??= new ConcurrentDictionary<object, object>();
        logContext.Properties[key] = value;
        return logContext;
    }

    /// <summary>
    /// 批量设置上下文数据
    /// </summary>
    /// <param name="logContext"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static LogContext SetRange(this LogContext logContext, IDictionary<object, object> properties)
    {
        if (logContext == null
            || properties == null
            || properties.Count == 0) return logContext;

        foreach (var (key, value) in properties)
        {
            logContext.Set(key, value);
        }

        return logContext;
    }

    /// <summary>
    /// 获取上下文数据
    /// </summary>
    /// <param name="logContext"></param>
    /// <param name="key">键</param>
    /// <returns></returns>
    public static object Get(this LogContext logContext, object key)
    {
        if (logContext == null
            || key == null
            || logContext.Properties == null
            || logContext.Properties.Count == 0) return default;

        var isExists = logContext.Properties.TryGetValue(key, out var value);
        return isExists ? value : null;
    }

    /// <summary>
    /// 获取上下文数据
    /// </summary>
    /// <param name="logContext"></param>
    /// <param name="key">键</param>
    /// <returns></returns>
    public static T Get<T>(this LogContext logContext, object key)
    {
        var value = logContext.Get(key);
        return value.ChangeType<T>();
    }
}