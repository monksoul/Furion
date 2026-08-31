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
using Furion.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Furion.Logging;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 异常分隔符
    /// </summary>
    private const string EXCEPTION_SEPARATOR = "++++++++++++++++++++++++++++++++++++++++++++++++++++++++";

    /// <summary>
    /// 从配置文件中加载配置并创建文件日志记录器提供程序
    /// </summary>
    /// <param name="configurationKey">获取配置文件对应的 Key</param>
    /// <param name="configure">文件日志记录器配置选项委托</param>
    /// <returns><see cref="FileLoggerProvider"/></returns>
    internal static FileLoggerProvider CreateFromConfiguration(Func<string> configurationKey, Action<FileLoggerOptions> configure = default)
    {
        // 检查 Key 是否存在
        var key = configurationKey?.Invoke();
        if (string.IsNullOrWhiteSpace(key)) return new FileLoggerProvider("application.log", new FileLoggerOptions());

        // 加载配置文件中指定节点
        var fileLoggerSettings = App.GetConfig<FileLoggerSettings>(key)
            ?? new FileLoggerSettings();

        // 创建文件日志记录器配置选项
        var fileLoggerOptions = new FileLoggerOptions
        {
            Append = fileLoggerSettings.Append,
            FileSizeLimitBytes = fileLoggerSettings.FileSizeLimitBytes,
            MaxRollingFiles = fileLoggerSettings.MaxRollingFiles,
            MinimumLevel = fileLoggerSettings.MinimumLevel,
            UseUtcTimestamp = fileLoggerSettings.UseUtcTimestamp,
            DateFormat = fileLoggerSettings.DateFormat,
            IncludeScopes = fileLoggerSettings.IncludeScopes,
            WithTraceId = fileLoggerSettings.WithTraceId,
            WithStackFrame = fileLoggerSettings.WithStackFrame,
            QueueCapacity = fileLoggerSettings.QueueCapacity,
            ShutdownTimeout = fileLoggerSettings.ShutdownTimeout,
        };

        // 处理自定义配置
        configure?.Invoke(fileLoggerOptions);

        // 创建文件日志记录器提供程序
        return new FileLoggerProvider(fileLoggerSettings.FileName ?? "application.log", fileLoggerOptions);
    }

    /// <summary>
    /// 从配置文件中加载配置并创建数据库日志记录器提供程序
    /// </summary>
    /// <typeparam name="TDatabaseLoggingWriter">实现自 <see cref="IDatabaseLoggingWriter"/></typeparam>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="configurationKey">获取配置文件对应的 Key</param>
    /// <param name="configure">数据库日志记录器配置选项委托</param>
    /// <returns><see cref="DatabaseLoggerProvider"/></returns>
    internal static DatabaseLoggerProvider CreateFromConfiguration<TDatabaseLoggingWriter>(IServiceProvider serviceProvider, Func<string> configurationKey, Action<DatabaseLoggerOptions> configure = default)
        where TDatabaseLoggingWriter : class, IDatabaseLoggingWriter
    {
        // 检查 Key 是否存在
        var key = configurationKey?.Invoke();
        if (string.IsNullOrWhiteSpace(key)) return new DatabaseLoggerProvider(new DatabaseLoggerOptions(), serviceProvider, typeof(TDatabaseLoggingWriter));

        // 加载配置文件中指定节点
        var databaseLoggerSettings = App.GetConfig<DatabaseLoggerSettings>(key)
            ?? new DatabaseLoggerSettings();

        // 创建数据库日志记录器配置选项
        var databaseLoggerOptions = new DatabaseLoggerOptions
        {
            MinimumLevel = databaseLoggerSettings.MinimumLevel,
            UseUtcTimestamp = databaseLoggerSettings.UseUtcTimestamp,
            DateFormat = databaseLoggerSettings.DateFormat,
            IncludeScopes = databaseLoggerSettings.IncludeScopes,
            IgnoreReferenceLoop = databaseLoggerSettings.IgnoreReferenceLoop,
            WithTraceId = databaseLoggerSettings.WithTraceId,
            WithStackFrame = databaseLoggerSettings.WithStackFrame,
            QueueCapacity = databaseLoggerSettings.QueueCapacity,
            ShutdownTimeout = databaseLoggerSettings.ShutdownTimeout,
            UseStackTraceGuard = databaseLoggerSettings.UseStackTraceGuard
        };

        // 处理自定义配置
        configure?.Invoke(databaseLoggerOptions);

        // 创建数据库日志记录器提供程序
        return new DatabaseLoggerProvider(databaseLoggerOptions, serviceProvider, typeof(TDatabaseLoggingWriter));
    }

    /// <summary>
    /// 输出标准日志消息
    /// </summary>
    /// <param name="logMsg"></param>
    /// <param name="dateFormat"></param>
    /// <param name="disableColors"></param>
    /// <param name="isConsole"></param>
    /// <param name="withTraceId"></param>
    /// <param name="withStackFrame"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    internal static string OutputStandardMessage(LogMessage logMsg
        , string dateFormat = "yyyy-MM-dd HH:mm:ss.fffffff zzz dddd"
        , bool isConsole = false
        , bool disableColors = true
        , bool withTraceId = false
        , bool withStackFrame = false
        , IFormatProvider? provider = null)
    {
        // 空检查
        if (logMsg.Message is null) return null;

        // 创建默认日志格式化模板
        var formatString = new StringBuilder();

        // 获取日志级别对应控制台的颜色
        var disableConsoleColor = !isConsole || disableColors;
        var logLevelColors = GetLogLevelConsoleColors(logMsg.LogLevel, disableConsoleColor);

        _ = AppendWithColor(formatString, GetLogLevelString(logMsg.LogLevel), logLevelColors);
        formatString.Append(": ");
        formatString.Append(logMsg.LogDateTime.ToString(dateFormat, provider));
        formatString.Append(' ');
        formatString.Append(logMsg.UseUtcTimestamp ? "U" : "L");
        formatString.Append(' ');
        _ = AppendWithColor(formatString, logMsg.LogName, disableConsoleColor
            ? new ConsoleColors(null, null)
            : new ConsoleColors(ConsoleColor.Cyan, ConsoleColor.DarkCyan));
        formatString.Append('[');
        formatString.Append(logMsg.EventId.Id);
        formatString.Append(']');
        formatString.Append(' ');
        formatString.Append($"#{logMsg.ThreadId}");
        if (withTraceId && !string.IsNullOrWhiteSpace(logMsg.TraceId))
        {
            formatString.Append(' ');
            _ = AppendWithColor(formatString, $"'{logMsg.TraceId}'", disableConsoleColor
                ? new ConsoleColors(null, null)
                : new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black));
        }
        formatString.AppendLine();

        // 输出日志输出所在方法和类型
        if (withStackFrame)
        {
            var targetMethod = GetBusinessMethod();

            if (targetMethod != null)
            {
                formatString.Append(PadLeftAlign($"[{targetMethod.DeclaringType.ToFriendlyString()}] {targetMethod.ToFriendlyString()}"));
                formatString.AppendLine();
            }
        }

        // 对日志内容进行缩进对齐处理
        formatString.Append(PadLeftAlign(logMsg.Message));

        // 如果包含异常信息，则创建新一行写入
        if (logMsg.Exception != null)
        {
            var EXCEPTION_SEPARATOR_WITHCOLOR = AppendWithColor(default, EXCEPTION_SEPARATOR, logLevelColors).ToString();
            var exceptionMessage = $"{Environment.NewLine}{EXCEPTION_SEPARATOR_WITHCOLOR}{Environment.NewLine}{logMsg.Exception}{Environment.NewLine}{EXCEPTION_SEPARATOR_WITHCOLOR}";

            formatString.Append(PadLeftAlign(exceptionMessage));
        }

        // 返回日志消息模板
        return formatString.ToString();
    }

    /// <summary>
    /// 将日志内容进行对齐
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private static string PadLeftAlign(string message)
    {
        var newMessage = string.Join(Environment.NewLine, message.Replace("\r\n", "\n").Split('\n', StringSplitOptions.None)
                    .Select(line => string.Empty.PadLeft(6, ' ') + line));

        return newMessage;
    }

    /// <summary>
    /// 获取日志级别短名称
    /// </summary>
    /// <param name="logLevel">日志级别</param>
    /// <returns></returns>
    internal static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
        };
    }

    /// <summary>
    /// 获取日志上下文
    /// </summary>
    /// <param name="scopeProvider"></param>
    /// <param name="includeScopes"></param>
    /// <returns></returns>
    internal static LogContext SetLogContext(IExternalScopeProvider scopeProvider, bool includeScopes)
    {
        // 空检查
        if (!includeScopes || scopeProvider == null) return null;

        var contexts = new List<LogContext>();
        var scopes = new List<object>();

        // 解析日志上下文数据
        scopeProvider.ForEachScope<object>((scope, ctx) =>
        {
            if (scope != null)
            {
                if (scope is LogContext context) contexts.Add(context);
                else scopes.Add(scope);
            }
        }, null);

        if (contexts.Count == 0 && scopes.Count == 0) return null;

        // 构建日志上下文
        var logConext = new LogContext
        {
            Properties = contexts.SelectMany(p => p.Properties)
                .GroupBy(p => p.Key)
                .ToDictionary(u => u.Key, u => u.FirstOrDefault().Value),
            Scopes = scopes
        };

        return logConext;
    }

    /// <summary>
    /// 扩展 StringBuilder 增加带颜色写入
    /// </summary>
    /// <param name="message"></param>
    /// <param name="colors"></param>
    /// <param name="formatString"></param>
    /// <returns></returns>
    private static StringBuilder AppendWithColor(StringBuilder formatString, string message, ConsoleColors colors)
    {
        formatString ??= new();

        // 输出控制台前景色和背景色
        if (colors.Background.HasValue) formatString.Append(GetBackgroundColorEscapeCode(colors.Background.Value));
        if (colors.Foreground.HasValue) formatString.Append(GetForegroundColorEscapeCode(colors.Foreground.Value));

        formatString.Append(message);

        // 输出控制台前景色和背景色
        if (colors.Foreground.HasValue) formatString.Append("\u001b[39m\u001b[22m");
        if (colors.Background.HasValue) formatString.Append("\u001b[49m");

        return formatString;
    }

    /// <summary>
    /// 输出控制台字体颜色 UniCode 码
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static string GetForegroundColorEscapeCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\u001b[30m",
            ConsoleColor.DarkRed => "\u001b[31m",
            ConsoleColor.DarkGreen => "\u001b[32m",
            ConsoleColor.DarkYellow => "\u001b[33m",
            ConsoleColor.DarkBlue => "\u001b[34m",
            ConsoleColor.DarkMagenta => "\u001b[35m",
            ConsoleColor.DarkCyan => "\u001b[36m",
            ConsoleColor.Gray => "\u001b[37m",
            ConsoleColor.Red => "\u001b[1m\u001b[31m",
            ConsoleColor.Green => "\u001b[1m\u001b[32m",
            ConsoleColor.Yellow => "\u001b[1m\u001b[33m",
            ConsoleColor.Blue => "\u001b[1m\u001b[34m",
            ConsoleColor.Magenta => "\u001b[1m\u001b[35m",
            ConsoleColor.Cyan => "\u001b[1m\u001b[36m",
            ConsoleColor.White => "\u001b[1m\u001b[37m",
            _ => "\u001b[39m\u001b[22m",
        };
    }

    /// <summary>
    /// 输出控制台背景颜色 UniCode 码
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static string GetBackgroundColorEscapeCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\u001b[40m",
            ConsoleColor.Red => "\u001b[41m",
            ConsoleColor.Green => "\u001b[42m",
            ConsoleColor.Yellow => "\u001b[43m",
            ConsoleColor.Blue => "\u001b[44m",
            ConsoleColor.Magenta => "\u001b[45m",
            ConsoleColor.Cyan => "\u001b[46m",
            ConsoleColor.White => "\u001b[47m",
            _ => "\u001b[49m",
        };
    }

    /// <summary>
    /// 获取控制台日志级别对应的颜色
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="disableColors"></param>
    /// <returns></returns>
    private static ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel, bool disableColors = false)
    {
        if (disableColors)
        {
            return new ConsoleColors(null, null);
        }

        return logLevel switch
        {
            LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.Red),
            LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.Red),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
            LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            _ => new ConsoleColors(null, background: null),
        };
    }

    private const string LOGGING_ABSTRACTIONS_ASSEMBLY = "Microsoft.Extensions.Logging.Abstractions";

    /// <summary>
    /// 获取调用当前日志的业务方法
    /// </summary>
    /// <returns></returns>
    private static MethodBase GetBusinessMethod()
    {
        var stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames();

        if (frames == null || frames.Length == 0) return null;

        for (var i = frames.Length - 1; i >= 0; i--)
        {
            var method = Reflect.UnwrapStateMachine(frames[i].GetMethod());
            if (method?.DeclaringType == null) continue;

            var assemblyName = method.DeclaringType.Assembly.GetName().Name;

            if (string.Equals(assemblyName, LOGGING_ABSTRACTIONS_ASSEMBLY, StringComparison.Ordinal))
            {
                if (i + 1 < frames.Length)
                {
                    return Reflect.UnwrapStateMachine(frames[i + 1].GetMethod());
                }

                return null;
            }
        }

        return null;
    }
}