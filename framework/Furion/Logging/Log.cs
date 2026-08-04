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

namespace Furion.Logging;

/// <summary>
/// 全局日志静态类
/// </summary>
public static class Log
{
    /// <summary>
    /// 手动构建方式
    /// </summary>
    /// <returns></returns>
    public static StringLoggingPart Default()
    {
        return StringLoggingPart.Default();
    }

    /// <summary>
    /// 创建日志记录器
    /// </summary>
    /// <returns></returns>
    public static ILogger CreateLogger<T>()
    {
        return App.GetRequiredService<ILogger<T>>();
    }

    /// <summary>
    /// 创建日志工厂
    /// </summary>
    /// <param name="configure">日志构建器</param>
    /// <remarks><see cref="ILoggerFactory"/> 实现了 <see cref="IDisposable"/> 接口，注意使用 `using` 控制</remarks>
    /// <returns></returns>
    public static ILoggerFactory CreateLoggerFactory(Action<ILoggingBuilder> configure = default)
    {
        return LoggerFactory.Create(builder =>
        {
            // 添加默认控制台输出
            builder.AddConsoleFormatter();

            configure?.Invoke(builder);
        });
    }

    /// <summary>
    /// 配置日志上下文
    /// </summary>
    /// <param name="properties">建议使用 ConcurrentDictionary 类型</param>
    /// <returns></returns>
    public static (ILogger logger, IDisposable scope) ScopeContext(IDictionary<object, object> properties)
    {
        return GetLogger(StringLoggingPart.Default().ScopeContext(properties));
    }

    /// <summary>
    /// 配置日志上下文
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static (ILogger logger, IDisposable scope) ScopeContext(Action<LogContext> configure)
    {
        return GetLogger(StringLoggingPart.Default().ScopeContext(configure));
    }

    /// <summary>
    /// 配置日志上下文
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static (ILogger logger, IDisposable scope) ScopeContext(LogContext context)
    {
        return GetLogger(StringLoggingPart.Default().ScopeContext(context));
    }

    /// <summary>
    /// LogInformation
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Information(string message, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).LogInformation();
    }

    /// <summary>
    /// LogInformation
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Information(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).LogInformation();
    }

    /// <summary>
    /// LogInformation
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Information(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetException(exception).LogInformation();
    }

    /// <summary>
    /// LogInformation
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Information(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogInformation();
    }

    /// <summary>
    /// LogInformation
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Information<TClass>(string message, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).LogInformation();
    }

    /// <summary>
    /// LogInformation
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Information<TClass>(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).LogInformation();
    }

    /// <summary>
    /// LogInformation
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Information<TClass>(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetException(exception).LogInformation();
    }

    /// <summary>
    /// LogInformation
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Information<TClass>(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogInformation();
    }

    /// <summary>
    /// LogWarning
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Warning(string message, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).LogWarning();
    }

    /// <summary>
    /// LogWarning
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Warning(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).LogWarning();
    }

    /// <summary>
    /// LogWarning
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Warning(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetException(exception).LogWarning();
    }

    /// <summary>
    /// LogWarning
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Warning(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogWarning();
    }

    /// <summary>
    /// LogWarning
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Warning<TClass>(string message, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).LogWarning();
    }

    /// <summary>
    /// LogWarning
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Warning<TClass>(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).LogWarning();
    }

    /// <summary>
    /// LogWarning
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Warning<TClass>(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetException(exception).LogWarning();
    }

    /// <summary>
    /// LogWarning
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Warning<TClass>(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogWarning();
    }

    /// <summary>
    /// LogError
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Error(string message, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).LogError();
    }

    /// <summary>
    /// LogError
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Error(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).LogError();
    }

    /// <summary>
    /// LogError
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Error(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetException(exception).LogError();
    }

    /// <summary>
    /// LogError
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Error(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogError();
    }

    /// <summary>
    /// LogError
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Error<TClass>(string message, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).LogError();
    }

    /// <summary>
    /// LogError
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Error<TClass>(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).LogError();
    }

    /// <summary>
    /// LogError
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Error<TClass>(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetException(exception).LogError();
    }

    /// <summary>
    /// LogError
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Error<TClass>(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogError();
    }

    /// <summary>
    /// LogDebug
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Debug(string message, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).LogDebug();
    }

    /// <summary>
    /// LogDebug
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Debug(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).LogDebug();
    }

    /// <summary>
    /// LogDebug
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Debug(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetException(exception).LogDebug();
    }

    /// <summary>
    /// LogDebug
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Debug(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogDebug();
    }

    /// <summary>
    /// LogDebug
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Debug<TClass>(string message, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).LogDebug();
    }

    /// <summary>
    /// LogDebug
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Debug<TClass>(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).LogDebug();
    }

    /// <summary>
    /// LogDebug
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Debug<TClass>(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetException(exception).LogDebug();
    }

    /// <summary>
    /// LogDebug
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Debug<TClass>(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogDebug();
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Trace(string message, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).LogTrace();
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Trace(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).LogTrace();
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Trace(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetException(exception).LogTrace();
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Trace(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogTrace();
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Trace<TClass>(string message, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).LogTrace();
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Trace<TClass>(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).LogTrace();
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Trace<TClass>(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetException(exception).LogTrace();
    }

    /// <summary>
    /// LogTrace
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Trace<TClass>(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogTrace();
    }

    /// <summary>
    /// LogCritical
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Critical(string message, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).LogCritical();
    }

    /// <summary>
    /// LogCritical
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Critical(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).LogCritical();
    }

    /// <summary>
    /// LogCritical
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Critical(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetException(exception).LogCritical();
    }

    /// <summary>
    /// LogCritical
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Critical(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogCritical();
    }

    /// <summary>
    /// LogCritical
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Critical<TClass>(string message, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).LogCritical();
    }

    /// <summary>
    /// LogCritical
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    public static void Critical<TClass>(string message, EventId eventId, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).LogCritical();
    }

    /// <summary>
    /// LogCritical
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Critical<TClass>(string message, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetException(exception).LogCritical();
    }

    /// <summary>
    /// LogCritical
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="message"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    public static void Critical<TClass>(string message, EventId eventId, Exception exception, params object[] args)
    {
        StringLoggingPart.Default().SetCategory<TClass>().SetMessage(message).SetArgs(args).SetEventId(eventId).SetException(exception).LogCritical();
    }

    /// <summary>
    /// 获取日志实例
    /// </summary>
    /// <param name="loggingPart"></param>
    /// <returns></returns>
    private static (ILogger, IDisposable) GetLogger(StringLoggingPart loggingPart)
    {
        // 获取日志实例
        var (logger, _, _) = loggingPart.GetLogger();

        var scope = logger.ScopeContext(loggingPart.LogContext);

        return (logger, scope);
    }
}