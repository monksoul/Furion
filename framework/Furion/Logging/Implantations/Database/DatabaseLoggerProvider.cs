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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Furion.Logging;

/// <summary>
/// 数据库日志记录器提供程序
/// </summary>
/// <remarks>https://docs.microsoft.com/zh-cn/dotnet/core/extensions/custom-logging-provider</remarks>
[ProviderAlias("Database")]
public sealed class DatabaseLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    /// <summary>
    /// 存储多日志分类日志记录器
    /// </summary>
    private readonly ConcurrentDictionary<string, DatabaseLogger> _databaseLoggers = new();

    /// <summary>
    /// 日志消息队列（线程安全）
    /// </summary>
    private readonly Channel<LogMessage> _logMessageChannel;

    /// <summary>
    /// 日志作用域提供器
    /// </summary>
    private IExternalScopeProvider _scopeProvider;

    /// <summary>
    /// 数据库日志写入器作用域范围
    /// </summary>
    internal IServiceScope _serviceScope;

    /// <summary>
    /// 服务提供器
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 写入器类型
    /// </summary>
    private readonly Type _writerType;

    /// <summary>
    /// 数据库日志写入器实例
    /// </summary>
    private IDatabaseLoggingWriter _databaseLoggingWriter;

    /// <summary>
    /// 写入器初始化锁
    /// </summary>
    private readonly object _writerLock = new object();

    /// <summary>
    /// 长时间运行的后台任务
    /// </summary>
    /// <remarks>实现不间断写入</remarks>
    private readonly Task _processQueueTask;

    /// <summary>
    /// 是否正在解析日志写入器
    /// </summary>
    /// <remarks>用于防止构造函数循环依赖。</remarks>
    [ThreadStatic]
    private static bool _isResolvingWriter;

    /// <summary>
    /// 用于检测当前正在执行写入的写入器类型
    /// </summary>
    /// <remarks>用于丢弃该写入器自身发出的日志。</remarks>
    internal static readonly AsyncLocal<Type> CurrentWritingWriterType = new();

    /// <summary>
    /// 是否已释放标志
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="databaseLoggerOptions">数据库日志记录器配置选项</param>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="writerType">实现 <see cref="IDatabaseLoggingWriter"/> 的类型</param>
    public DatabaseLoggerProvider(DatabaseLoggerOptions databaseLoggerOptions, IServiceProvider serviceProvider, Type writerType)
    {
        LoggerOptions = databaseLoggerOptions;
        _serviceProvider = serviceProvider;
        _writerType = writerType;

        _logMessageChannel = Channel.CreateBounded<LogMessage>(new BoundedChannelOptions(12000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = true
        });

        // 创建长时间运行的后台任务，并将日志消息队列中数据写入存储中
        _processQueueTask = Task.Factory.StartNew(ProcessQueueAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }

    /// <summary>
    /// 数据库日志记录器配置选项
    /// </summary>
    internal DatabaseLoggerOptions LoggerOptions { get; private set; }

    /// <summary>
    /// 日志作用域提供器
    /// </summary>
    internal IExternalScopeProvider ScopeProvider => _scopeProvider;

    /// <summary>
    /// 创建数据库日志记录器
    /// </summary>
    /// <param name="categoryName">日志分类名</param>
    /// <returns><see cref="ILogger"/></returns>
    public ILogger CreateLogger(string categoryName)
    {
        // 解决日志死循环问题
        if (_isResolvingWriter) return NullLogger.Instance;

        return _databaseLoggers.GetOrAdd(categoryName, name => new DatabaseLogger(name, this));
    }

    /// <summary>
    /// 设置作用域提供器
    /// </summary>
    /// <param name="scopeProvider"></param>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    /// 释放非托管资源
    /// </summary>
    /// <remarks>控制日志消息队列</remarks>
    public void Dispose()
    {
        // 标识已释放
        _isDisposed = true;

        // 标记通道已完成写入
        _logMessageChannel.Writer.Complete();

        try
        {
            // 设置 1.5 秒的缓冲时间，避免还有日志消息没有完成写入数据库中
            _processQueueTask?.Wait(1500);
        }
        catch (TaskCanceledException) { }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        catch { }

        // 清空数据库日志记录器
        _databaseLoggers.Clear();

        // 释放数据库写入器作用域范围
        _serviceScope?.Dispose();
    }

    /// <summary>
    /// 将日志消息写入队列中等待后台任务出队写入数据库
    /// </summary>
    /// <param name="logMsg">结构化日志消息</param>
    internal void WriteToQueue(LogMessage logMsg)
    {
        // 非阻塞写入
        _logMessageChannel.Writer.TryWrite(logMsg);
    }

    /// <summary>
    /// 将日志消息批量写入数据库中
    /// </summary>
    private async Task ProcessQueueAsync()
    {
        // 持续读取通道中的消息，直到通道关闭
        while (await _logMessageChannel.Reader.WaitToReadAsync())
        {
            // 检查是否已释放
            if (_isDisposed) break;

            // 读取一批消息（最多 100 条）
            var batch = new List<LogMessage>(100);
            while (_logMessageChannel.Reader.TryRead(out var logMsg) && batch.Count < 100)
            {
                batch.Add(logMsg);
            }

            // 如果本次没有读取到任何消息
            if (batch.Count == 0) continue;

            // 判断通道中是否还有更多消息
            var hasMore = _logMessageChannel.Reader.Count > 0;

            IDatabaseLoggingWriter databaseLoggingWriter = null;
            try
            {
                databaseLoggingWriter = GetWriter();
            }
            catch (Exception ex)
            {
                LoggerOptions.HandleWriteError?.Invoke(new DatabaseWriteError(ex));

                foreach (var msg in batch)
                {
                    msg.Context?.Dispose();
                }

                continue;
            }

            // 记录当前正在执行的写入器类型
            CurrentWritingWriterType.Value = databaseLoggingWriter?.GetType();

            try
            {
                // 检查是否已释放
                if (!_isDisposed)
                {
                    // 调用数据库写入器的批量写入方法
                    await databaseLoggingWriter.WriteAsync(batch, !hasMore);
                }
            }
            catch (Exception ex)
            {
                // 处理数据库写入错误
                if (LoggerOptions.HandleWriteError != null)
                {
                    var databaseWriteError = new DatabaseWriteError(ex);
                    LoggerOptions.HandleWriteError(databaseWriteError);
                }
            }
            finally
            {
                CurrentWritingWriterType.Value = null;

                // 释放批次中每条日志的作用域上下文
                foreach (var logMsg in batch)
                {
                    logMsg.Context?.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// 获取数据库日志写入器
    /// </summary>
    private IDatabaseLoggingWriter GetWriter()
    {
        if (_databaseLoggingWriter == null)
        {
            lock (_writerLock)
            {
                if (_databaseLoggingWriter == null)
                {
                    _isResolvingWriter = true;
                    try
                    {
                        // 解析服务作用域工厂服务
                        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

                        // 创建服务作用域
                        _serviceScope = scopeFactory.CreateScope();

                        // 基于当前作用域创建数据库日志写入器
                        _databaseLoggingWriter = _serviceScope.ServiceProvider.GetRequiredService(_writerType) as IDatabaseLoggingWriter;
                    }
                    finally
                    {
                        _isResolvingWriter = false;
                    }
                }
            }
        }

        return _databaseLoggingWriter;
    }
}