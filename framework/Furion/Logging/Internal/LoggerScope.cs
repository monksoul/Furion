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
using System.Collections.Concurrent;

namespace Furion.Logging;

/// <summary>
/// 日志范围
/// </summary>
/// <remarks>用于提供日志上下文编写</remarks>
public sealed class LoggerScope : IDisposable
{
    /// <summary>
    /// 待释放的日志范围集合
    /// </summary>
    private readonly ConcurrentBag<IDisposable> _disposables;

    /// <summary>
    /// <see cref="ILogger"/>
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 释放标识
    /// </summary>
    private bool disposedValue;

    /// <summary>
    /// 构造函数
    /// </summary>
    internal LoggerScope(ILogger logger)
    {
        _disposables = [];
        _logger = logger;
    }

    /// <summary>
    /// 设置日志上下文
    /// </summary>
    /// <param name="properties">建议使用 ConcurrentDictionary 类型</param>
    public void WithContext(IDictionary<object, object> properties)
    {
        _disposables.Add(_logger.BeginScope(new LogContext { Properties = properties }));
    }

    /// <summary>
    /// 设置日志上下文
    /// </summary>
    /// <param name="configure"></param>
    public void WithContext(Action<LogContext> configure)
    {
        var logContext = new LogContext();
        configure?.Invoke(logContext);

        _disposables.Add(_logger.BeginScope(logContext));
    }

    /// <summary>
    /// 设置日志上下文
    /// </summary>
    /// <param name="context"></param>
    public void WithContext(LogContext context)
    {
        _disposables.Add(_logger.BeginScope(context));
    }

    /// <summary>
    /// 释放托管资源
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                List<Exception> exceptions = null;

                while (_disposables.TryTake(out var disposable))
                {
                    try
                    {
                        disposable?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= [];
                        exceptions.Add(ex);
                    }
                }

                if (exceptions != null && exceptions.Count > 0)
                {
                    throw new AggregateException("One or more errors occurred while disposing logger scopes.", exceptions);
                }
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}