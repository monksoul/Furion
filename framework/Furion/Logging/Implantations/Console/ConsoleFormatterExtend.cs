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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Furion.Logging;

/// <summary>
/// 控制台默认格式化程序扩展
/// </summary>
public sealed class ConsoleFormatterExtend : ConsoleFormatter, IDisposable
{
    /// <summary>
    /// 日志格式化选项刷新 Token
    /// </summary>
    private readonly IDisposable _formatOptionsReloadToken;

    /// <summary>
    /// 日志格式化配置选项
    /// </summary>
    private ConsoleFormatterExtendOptions _formatterOptions;

    /// <summary>
    /// 是否启用控制台颜色
    /// </summary>
    private bool _disableColors;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="formatterOptions"></param>
    public ConsoleFormatterExtend(IOptionsMonitor<ConsoleFormatterExtendOptions> formatterOptions)
        : base("console-format")
    {
        (_formatOptionsReloadToken, _formatterOptions) = (formatterOptions.OnChange(ReloadFormatterOptions), formatterOptions.CurrentValue);
        _disableColors = _formatterOptions.ColorBehavior == LoggerColorBehavior.Disabled || (_formatterOptions.ColorBehavior == LoggerColorBehavior.Default && Console.IsOutputRedirected);
    }

    /// <summary>
    /// 写入日志
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logEntry"></param>
    /// <param name="scopeProvider"></param>
    /// <param name="textWriter"></param>
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
    {
        // 获取当前配置快照
        var options = _formatterOptions;
        var disableColors = _disableColors;

        // 获取格式化后的消息
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        // 日志消息内容转换
        if (options.MessageProcess != null && message != null)
        {
            message = options.MessageProcess(message);
        }

        // 创建日志消息
        var logDateTime = options.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        var logMsg = new LogMessage(logEntry.Category, logEntry.LogLevel, logEntry.EventId, message, logEntry.Exception, null, logEntry.State, logDateTime, Environment.CurrentManagedThreadId, options.UseUtcTimestamp, App.GetTraceId())
        {
            // 设置日志上下文
            Context = Penetrates.SetLogContext(scopeProvider, options.IncludeScopes)
        };

        try
        {
            string standardMessage;

            // 是否自定义了自定义日志格式化程序，如果是则使用
            if (options.MessageFormat != null)
            {
                // 设置日志消息模板
                standardMessage = options.MessageFormat(logMsg);
            }
            else
            {
                // 获取标准化日志消息
                standardMessage = Penetrates.OutputStandardMessage(logMsg
                   , options.DateFormat
                   , true
                   , disableColors
                   , options.WithTraceId
                   , options.WithStackFrame
                   , options.FormatProvider);
            }

            // 判断是否自定义了日志筛选器，如果是则检查是否符合条件
            if (options.WriteFilter?.Invoke(logMsg) == false)
            {
                return;
            }

            // 空检查
            if (standardMessage is null)
            {
                return;
            }

            // 判断是否自定义了日志格式化程序
            if (options.WriteHandler != null)
            {
                options.WriteHandler?.Invoke(logMsg, scopeProvider, textWriter, standardMessage, options);
            }
            else
            {
                // 写入控制台
                textWriter.WriteLine(standardMessage);
            }
        }
        finally
        {
            logMsg.Context?.Dispose();
        }
    }

    /// <summary>
    /// 释放非托管资源
    /// </summary>
    public void Dispose()
    {
        _formatOptionsReloadToken?.Dispose();
    }

    /// <summary>
    /// 刷新日志格式化选项
    /// </summary>
    /// <param name="options"></param>
    private void ReloadFormatterOptions(ConsoleFormatterExtendOptions options)
    {
        _formatterOptions = options;
        _disableColors = options.ColorBehavior == LoggerColorBehavior.Disabled || (options.ColorBehavior == LoggerColorBehavior.Default && Console.IsOutputRedirected);
    }
}