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
using System.Threading.Channels;

namespace Furion.Logging;

/// <summary>
/// 文件日志记录器提供程序
/// </summary>
/// <remarks>https://docs.microsoft.com/zh-cn/dotnet/core/extensions/custom-logging-provider</remarks>
[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    /// <summary>
    /// 存储多日志分类日志记录器
    /// </summary>
    private readonly ConcurrentDictionary<string, FileLogger> _fileLoggers = new();

    /// <summary>
    /// 日志消息队列
    /// </summary>
    private readonly Channel<LogMessage> _logMessageChannel;

    /// <summary>
    /// 日志作用域提供器
    /// </summary>
    private IExternalScopeProvider _scopeProvider;

    /// <summary>
    /// 记录日志所有滚动文件名
    /// </summary>
    /// <remarks>只有 MaxRollingFiles 和 FileSizeLimitBytes 大于 0 有效</remarks>
    internal readonly ConcurrentDictionary<string, FileInfo> _rollingFileNames = new();

    /// <summary>
    /// 文件日志写入器
    /// </summary>
    private readonly FileLoggingWriter _fileLoggingWriter;

    /// <summary>
    /// 长时间运行的后台任务
    /// </summary>
    /// <remarks>实现不间断写入</remarks>
    private readonly Task _processQueueTask;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileName">日志文件名</param>
    public FileLoggerProvider(string fileName)
        : this(fileName, true)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileName">日志文件名</param>
    /// <param name="append">追加到已存在日志文件或覆盖它们</param>
    public FileLoggerProvider(string fileName, bool append)
        : this(fileName, new FileLoggerOptions() { Append = append })
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileName">日志文件名</param>
    /// <param name="fileLoggerOptions">文件日志记录器配置选项</param>
    public FileLoggerProvider(string fileName, FileLoggerOptions fileLoggerOptions)
    {
        // 支持文件名嵌入系统环境变量，格式为：%SystemDrive%，%SystemRoot%，处理 Windows 和 Linux 路径分隔符不一致问题
        FileName = Environment.ExpandEnvironmentVariables(fileName).Replace('\\', '/');
        LoggerOptions = fileLoggerOptions;

        // 创建文件日志写入器
        _fileLoggingWriter = new FileLoggingWriter(this);

        _logMessageChannel = Channel.CreateBounded<LogMessage>(new BoundedChannelOptions(12000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = true
        });

        // 创建长时间运行的后台任务，并将日志消息队列中数据写入文件中
        _processQueueTask = Task.Factory.StartNew(ProcessQueueAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }

    /// <summary>
    /// 文件名
    /// </summary>
    internal string FileName;

    /// <summary>
    /// 文件日志记录器配置选项
    /// </summary>
    internal FileLoggerOptions LoggerOptions { get; private set; }

    /// <summary>
    /// 日志作用域提供器
    /// </summary>
    internal IExternalScopeProvider ScopeProvider => _scopeProvider;

    /// <summary>
    /// 创建文件日志记录器
    /// </summary>
    /// <param name="categoryName">日志分类名</param>
    /// <returns><see cref="ILogger"/></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return _fileLoggers.GetOrAdd(categoryName, name => new FileLogger(name, this));
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
        // 标记通道已完成写入
        _logMessageChannel.Writer.Complete();

        try
        {
            // 设置 1.5 秒的缓冲时间，避免还有日志消息没有完成写入文件中
            _processQueueTask?.Wait(1500);
        }
        catch (TaskCanceledException) { }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        catch { }

        // 清空文件日志记录器
        _fileLoggers.Clear();

        // 清空滚动文件名记录器
        _rollingFileNames.Clear();

        // 释放内部文件写入器
        try
        {
            _fileLoggingWriter?.DisposeAsync().Wait(500);
        }
        catch { }
    }

    /// <summary>
    /// 将日志消息写入队列中等待后台任务出队写入文件
    /// </summary>
    /// <param name="logMsg">日志消息</param>
    internal void WriteToQueue(LogMessage logMsg)
    {
        // 非阻塞写入
        _logMessageChannel.Writer.TryWrite(logMsg);
    }

    /// <summary>
    /// 将日志消息写入文件中
    /// </summary>
    /// <remarks>使用批处理读取以提高文件写入吞吐量</remarks>
    private async Task ProcessQueueAsync()
    {
        // 持续读取通道中的消息，直到通道关闭
        while (await _logMessageChannel.Reader.WaitToReadAsync())
        {
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

            for (var i = 0; i < batch.Count; i++)
            {
                var item = batch[i];
                try
                {
                    // 调用文件日志写入器的写入方法
                    await _fileLoggingWriter.WriteAsync(item, !hasMore && i == batch.Count - 1);
                }
                catch
                {
                }
                finally
                {
                    item.Context?.Dispose();
                }
            }
        }
    }
}