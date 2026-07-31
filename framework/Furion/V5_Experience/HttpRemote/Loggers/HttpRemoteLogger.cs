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
using Microsoft.Extensions.Options;

namespace Furion.HttpRemote;

/// <inheritdoc />
/// <param name="logger">
///     <see cref="Logger{T}" />
/// </param>
/// <param name="httpRemoteOptions">
///     <see cref="IOptions{TOptions}" />
/// </param>
/// <param name="isLoggingRegistered">是否配置（注册）了日志程序</param>
internal sealed class HttpRemoteLogger(
    ILogger<Logging> logger,
    IOptionsMonitor<HttpRemoteOptions> httpRemoteOptions,
    bool isLoggingRegistered) : IHttpRemoteLogger
{
    /// <summary>
    ///     日志消息格式化器
    /// </summary>
    /// <remarks>用于在未注册 <see cref="ILogger" /> 时通过 <see cref="HttpRemoteOptions.FallbackLogger" /> 输出结构化日志。</remarks>
    internal Lazy<Func<string?, object?[], string?>> _logMessageFormatter = new(() =>
    {
        try
        {
            // 获取内部的 Microsoft.Extensions.Logging.FormattedLogValues 类型
            if (Type.GetType(
                    "Microsoft.Extensions.Logging.FormattedLogValues, Microsoft.Extensions.Logging.Abstractions") is
                { } formattedLogValuesType)
            {
                return (message, args) =>
                {
                    try
                    {
                        // 初始化 FormattedLogValues 实例
                        var instance = Activator.CreateInstance(formattedLogValuesType, message, args);
                        return instance?.ToString();
                    }
                    catch
                    {
                        return message;
                    }
                };
            }
        }
        catch
        {
            // ignored
        }

        return (message, _) => message;
    });

    /// <inheritdoc />
    public void LogInformation(string? message, params object?[] args) =>
        Log(LogLevel.Information, null, message, args);

    /// <inheritdoc />
    public void LogTrace(string? message, params object?[] args) => Log(LogLevel.Trace, null, message, args);

    /// <inheritdoc />
    public void LogDebug(string? message, params object?[] args) => Log(LogLevel.Debug, null, message, args);

    /// <inheritdoc />
    public void LogWarning(string? message, params object?[] args) => Log(LogLevel.Warning, null, message, args);

    /// <inheritdoc />
    public void LogWarning(Exception? exception, string? message, params object?[] args) =>
        Log(LogLevel.Warning, exception, message, args);

    /// <inheritdoc />
    public void LogCritical(string? message, params object?[] args) => Log(LogLevel.Critical, null, message, args);

    /// <inheritdoc />
    public void LogError(Exception? exception, string? message, params object?[] args) =>
        Log(LogLevel.Error, exception, message, args);

    /// <inheritdoc />
    public void Log(LogLevel logLevel, Exception? exception, string? message, params object?[] args)
    {
        // 检查是否注册了日志输出程序
        if (isLoggingRegistered)
        {
            logger.Log(logLevel, exception, message, args);
        }
        else
        {
            // 调用备用日志输出委托
            httpRemoteOptions.CurrentValue.FallbackLogger?.Invoke(_logMessageFormatter.Value(message, args));
        }
    }
}