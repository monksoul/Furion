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

using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Furion.Logging;

/// <summary>
/// 数据库记录器配置选项
/// </summary>
public sealed class DatabaseLoggerOptions
{
    /// <summary>
    /// 自定义日志筛选器
    /// </summary>
    public Func<LogMessage, bool> WriteFilter { get; set; }

    /// <summary>
    /// 最低日志记录级别
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

    /// <summary>
    /// 自定义日志消息格式化程序
    /// </summary>
    public Func<LogMessage, string> MessageFormat { get; set; }

    /// <summary>
    /// 自定义数据库日志写入错误程序
    /// </summary>
    /// <remarks>
    /// 主要解决日志在写入过程出现异常问题。
    /// <code>
    /// options.HandleWriteError = (err) => {
    ///     // do anything
    /// };
    /// </code>
    /// </remarks>
    public Action<DatabaseWriteError> HandleWriteError { get; set; }

    /// <summary>
    /// 当发生递归日志写入时的备用处理动作
    /// </summary>
    /// <remarks>例如输出到控制台或独立文件。</remarks>
    public Action<LogMessage> FallbackLogAction { get; set; }

    /// <summary>
    /// 是否使用 UTC 时间戳
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool UseUtcTimestamp { get; set; }

    /// <summary>
    /// 日期格式化
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fffffff zzz dddd";

    /// <summary>
    /// 是否启用日志上下文
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// 忽略日志循环输出
    /// </summary>
    /// <remarks>对性能有些许影响。</remarks>
    public bool IgnoreReferenceLoop { get; set; } = true;

    /// <summary>
    /// 显示跟踪/请求 Id
    /// </summary>
    public bool WithTraceId { get; set; } = false;

    /// <summary>
    /// 显示堆栈框架
    /// </summary>
    /// <remarks>程序集和方法签名。</remarks>
    public bool WithStackFrame { get; set; } = false;

    /// <summary>
    /// 日志消息内容转换
    /// </summary>
    public Func<string, string> MessageProcess { get; set; }

    /// <summary>
    /// 格式化提供器
    /// </summary>
    public IFormatProvider? FormatProvider { get; set; } = CultureInfo.InvariantCulture;
}