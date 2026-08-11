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

using Furion.Utilities;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Furion.Logging;

/// <summary>
/// 文件日志写入器
/// </summary>
internal class FileLoggingWriter
{
    /// <summary>
    /// 文件日志记录器提供程序
    /// </summary>
    private readonly FileLoggerProvider _fileLoggerProvider;

    /// <summary>
    /// 日志配置选项
    /// </summary>
    private readonly FileLoggerOptions _options;

    /// <summary>
    /// 日志文件名
    /// </summary>
    private string _fileName;

    /// <summary>
    /// 文件流
    /// </summary>
    private FileStream _fileStream;

    /// <summary>
    /// 文本写入器
    /// </summary>
    private StreamWriter _textWriter;

    /// <summary>
    /// 缓存上次返回的基本日志文件名，避免重复解析
    /// </summary>
    private string _lastBaseFileName;

    /// <summary>
    /// 缓存上次扫描的日志文件列表
    /// </summary>
    private List<FileInfo> _cachedLogFiles;

    /// <summary>
    /// 缓存最后扫描时间戳
    /// </summary>
    private long _lastScanTimestamp;

    /// <summary>
    /// 目录扫描缓存有效期（毫秒），默认 5 秒
    /// </summary>
    private const long DirectoryScanCacheTtlMs = 5000;

    /// <summary>
    /// 判断是否启动滚动日志功能
    /// </summary>
    private readonly bool _isEnabledRollingFiles;

    /// <summary>
    /// 是否启用兼容模式
    /// </summary>
    /// <remarks>调试状态下自动启用，解决 VS 双击打开文件被占用的问题</remarks>
    private readonly bool _isCompatibleMode;

    /// <summary>
    /// UTF-8 编码
    /// </summary>
    /// <remarks>不带 BOM，避免外部编辑乱码</remarks>
    private static readonly UTF8Encoding _utf8Encoding = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// 写入计数器，用于周期性检查文件存在性
    /// </summary>
    private int _writeCount = 0;

    /// <summary>
    /// 周期性检查文件的间隔（写入次数）
    /// </summary>
    /// <remarks>默认 100 次。</remarks>
    private const int PeriodicCheckInterval = 100;

    /// <summary>
    /// 重连锁，避免多线程同时重连
    /// </summary>
    private int _reconnecting = 0;

    /// <summary>
    /// 是否已释放标志
    /// </summary>
    private volatile bool _isDisposed = false;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileLoggerProvider">文件日志记录器提供程序</param>
    internal FileLoggingWriter(FileLoggerProvider fileLoggerProvider)
    {
        _fileLoggerProvider = fileLoggerProvider;
        _options = fileLoggerProvider.LoggerOptions;
        _isEnabledRollingFiles = _options.MaxRollingFiles > 0 && _options.FileSizeLimitBytes > 0;

        // 根据调试状态自动判断是否启用兼容模式
        // 注意：兼容模式下性能较差但避免文件占用问题，适合调试环境使用！！！
        _isCompatibleMode = EnvironmentUtility.IsDevelopment;

        // 解析当前写入日志的文件名
        GetCurrentFileName();

        // 如果启用了滚动日志，重建滚动文件列表
        if (_isEnabledRollingFiles)
        {
            RebuildRollingFileNames();
        }

        // 兼容模式下不需要预打开长连接文件流
        if (!_isCompatibleMode)
        {
            AsyncUtility.RunSync(() => OpenFileAsync(_options.Append));
        }
    }

    /// <summary>
    /// 获取日志基础文件名
    /// </summary>
    /// <returns>日志文件名</returns>
    private string GetBaseFileName()
    {
        var fileName = _fileLoggerProvider.FileName;

        // 如果配置了日志文件名格式化程序，则先处理再返回
        if (_options.FileNameRule != null)
            fileName = _options.FileNameRule(fileName);

        return fileName;
    }

    /// <summary>
    /// 根据当前环境获取默认日志目录
    /// </summary>
    /// <returns></returns>
    private static string GetSafeDefaultLogDirectory()
    {
        return EnvironmentUtility.IsDevelopment
            ? Directory.GetCurrentDirectory()
            : AppContext.BaseDirectory;
    }

    /// <summary>
    /// 确保文件名是绝对路径
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="baseDirectory"></param>
    /// <returns></returns>
    private static string EnsureAbsolutePath(string fileName, string baseDirectory)
    {
        if (Path.IsPathRooted(fileName)) return fileName;
        return Path.Combine(baseDirectory, fileName);
    }

    /// <summary>
    /// 解析当前写入日志的文件名
    /// </summary>
    private void GetCurrentFileName()
    {
        var originalBaseFileName = GetBaseFileName();
        var baseNameChanged = originalBaseFileName != _lastBaseFileName;

        // 总是更新缓存，确保下次能检测到变化
        _lastBaseFileName = originalBaseFileName;

        var baseFileName = EnsureAbsolutePath(originalBaseFileName, GetSafeDefaultLogDirectory());

        // 是否配置了日志文件最大存储大小
        if (_options.FileSizeLimitBytes <= 0)
        {
            _fileName = baseFileName;
            return;
        }

        // 检查基础文件名是否变化
        if (baseNameChanged)
        {
            _fileName = baseFileName;

            // 滚动日志文件名规则变更后重建列表
            if (_isEnabledRollingFiles)
            {
                RebuildRollingFileNames();
            }
            return;
        }

        // 定义文件查找通配符
        var logFileMask = Path.GetFileNameWithoutExtension(baseFileName) + "*" + Path.GetExtension(baseFileName);

        // 获取文件路径
        var logDirName = Path.GetDirectoryName(baseFileName);

        // 如果目录变化，清空缓存
        if (_cachedLogFiles != null && _cachedLogFiles.Count > 0)
        {
            var cachedDir = Path.GetDirectoryName(_cachedLogFiles[0]?.FullName);
            if (!string.Equals(cachedDir, logDirName, StringComparison.OrdinalIgnoreCase))
            {
                _cachedLogFiles = null; // 强制重建缓存
            }
        }

        var now = Stopwatch.GetTimestamp();
        var elapsedMs = (now - _lastScanTimestamp) * 1000L / Stopwatch.Frequency;
        // 在当前目录下根据文件通配符查找所有匹配的文件
        if (_cachedLogFiles == null || elapsedMs >= DirectoryScanCacheTtlMs)
        {
            _cachedLogFiles = [];

            if (Directory.Exists(logDirName))
            {
                foreach (var file in Directory.EnumerateFiles(logDirName, logFileMask, SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        _cachedLogFiles.Add(new FileInfo(file));
                    }
                    catch { }
                }
            }

            _lastScanTimestamp = now;
        }

        // 处理已有日志文件存在情况
        if (_cachedLogFiles.Count > 0)
        {
            FileInfo lastFileInfo = null;
            long maxTicks = -1;

            foreach (var fInfo in _cachedLogFiles)
            {
                var ticks = fInfo.LastWriteTimeUtc.Ticks;
                // 优先按时间，时间相同按文件名降序
                if (ticks > maxTicks || (ticks == maxTicks && (lastFileInfo == null || string.CompareOrdinal(fInfo.Name, lastFileInfo.Name) > 0)))
                {
                    maxTicks = ticks;
                    lastFileInfo = fInfo;
                }
            }
            // 没有任何匹配的日志文件直接使用当前基础文件名
            _fileName = lastFileInfo?.FullName ?? baseFileName;
        }
        else _fileName = baseFileName;
    }

    /// <summary>
    /// 重建滚动日志文件列表（从磁盘扫描）
    /// </summary>
    private void RebuildRollingFileNames()
    {
        var baseFileName = EnsureAbsolutePath(_lastBaseFileName, GetSafeDefaultLogDirectory());
        var logFileMask = Path.GetFileNameWithoutExtension(baseFileName) + "*" + Path.GetExtension(baseFileName);
        var logDirName = Path.GetDirectoryName(baseFileName);

        if (!Directory.Exists(logDirName)) return;

        // 清空当前记录
        _fileLoggerProvider._rollingFileNames.Clear();

        // 将所有匹配的文件加入滚动列表
        foreach (var file in Directory.EnumerateFiles(logDirName, logFileMask, SearchOption.TopDirectoryOnly))
        {
            try
            {
                var fileInfo = new FileInfo(file);
                // 处理 Windows 和 Linux 路径分隔符不一致问题
                var key = fileInfo.FullName.Replace('\\', '/');
                _fileLoggerProvider._rollingFileNames.TryAdd(key, fileInfo);
            }
            catch { }
        }
    }

    /// <summary>
    /// 获取下一个匹配的日志文件名
    /// </summary>
    /// <remarks>只有配置了 <see cref="FileLoggerOptions.FileSizeLimitBytes"/> 或 <see cref="FileLoggerOptions.FileNameRule"/> 或 <see cref="FileLoggerOptions.MaxRollingFiles"/> 有效</remarks>
    /// <returns></returns>
    private string GetNextFileName()
    {
        // 获取日志基础文件名
        var baseFileName = EnsureAbsolutePath(GetBaseFileName(), GetSafeDefaultLogDirectory());

        // 如果基础文件不存在，或者没有配置大小限制，或者基础文件还没达到大小限制，直接返回新的基础文件名
        if (!File.Exists(baseFileName) || _options.FileSizeLimitBytes <= 0 || new FileInfo(baseFileName).Length < _options.FileSizeLimitBytes)
        {
            return baseFileName;
        }

        var currentFileIndex = 0;
        var baseFileNameOnly = Path.GetFileNameWithoutExtension(baseFileName);
        var currentFileNameOnly = Path.GetFileNameWithoutExtension(_fileName);

        // 解析日志文件名【递增】部分
        var suffix = string.Empty;
        if (currentFileNameOnly.Length > baseFileNameOnly.Length &&
            currentFileNameOnly.StartsWith(baseFileNameOnly, StringComparison.OrdinalIgnoreCase))
        {
            suffix = currentFileNameOnly[baseFileNameOnly.Length..];
        }

        if (suffix.StartsWith('_'))
        {
            suffix = suffix[1..];
        }

        if (suffix.Length > 0 && int.TryParse(suffix, out var parsedIndex))
        {
            currentFileIndex = parsedIndex;
        }

        // 【递增】部分 +1
        var nextFileIndex = currentFileIndex + 1;

        // 如果配置了最大【递增】数，则超出自动从头开始（覆盖写入）
        if (_options.MaxRollingFiles > 0)
        {
            nextFileIndex %= _options.MaxRollingFiles;
        }

        // 返回下一个匹配的日志文件名（完整路径）
        var nextFileName = baseFileNameOnly + (nextFileIndex > 0 ? "_" + nextFileIndex.ToString() : "") + Path.GetExtension(baseFileName);
        var directory = Path.GetDirectoryName(baseFileName);
        return Path.Combine(directory, nextFileName);
    }

    /// <summary>
    /// 打开文件（仅长连接模式使用）
    /// </summary>
    /// <param name="append"></param>
    /// <returns><see cref="Task"/></returns>
    private Task OpenFileAsync(bool append)
    {
        try
        {
            CreateFileStream();
        }
        catch (Exception ex)
        {
            // 处理文件写入错误
            if (_options.HandleWriteError != null)
            {
                var fileWriteError = new FileWriteError(_fileName, ex);
                _options.HandleWriteError(fileWriteError);

                // 如果配置了备用文件名，则重新写入
                if (fileWriteError.RollbackFileName != null)
                {
                    _fileLoggerProvider.FileName = fileWriteError.RollbackFileName;

                    // 递归操作，直到应用程序停止
                    GetCurrentFileName();
                    RebuildRollingFileNames(); // 同步更新滚动列表
                    CreateFileStream();
                }
            }
            // 其他直接抛出异常
            else throw;
        }

        // 初始化文本写入器
        _textWriter = new StreamWriter(_fileStream, _utf8Encoding, 4096, leaveOpen: true)
        {
            AutoFlush = false,
            NewLine = Environment.NewLine
        };

        // 创建文件流
        void CreateFileStream()
        {
            var fileInfo = new FileInfo(_fileName);

            // 判断文件目录是否存在，不存在则自动创建
            fileInfo.Directory?.Create();

            _fileStream = new FileStream(
                _fileName,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.ReadWrite | FileShare.Delete,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            // 删除超出滚动日志限制的文件
            DropFilesIfOverLimit(fileInfo);

            // 判断是否追加还是覆盖
            if (append)
            {
                if (_fileStream.Length > 0) _fileStream.Seek(0, SeekOrigin.End);
            }
            else
            {
                _fileStream.SetLength(0);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 兼容模式下的短连接写入方法（每次写入独立打开/关闭文件）
    /// </summary>
    private async Task WriteWithCompatibleModeAsync(string message)
    {
        // 解析当前写入日志的文件名
        GetCurrentFileName();

        var directory = Path.GetDirectoryName(_fileName);

        // 空检查
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 处理文件滚动逻辑
        if (_isEnabledRollingFiles)
        {
            var fileInfo = new FileInfo(_fileName);
            if (fileInfo.Exists && fileInfo.Length >= _options.FileSizeLimitBytes)
            {
                // 滚动到新文件
                _fileName = GetNextFileName();

                // 清理超出数量的旧文件
                DropFilesIfOverLimit(fileInfo);
            }
        }

        try
        {
            // 每次写入独立打开/关闭文件，避免文件占用问题
            using var fileStream = new FileStream(
                _fileName,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite | FileShare.Delete,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            var bytes = _utf8Encoding.GetBytes(message + Environment.NewLine);
            await fileStream.WriteAsync(bytes);
            await fileStream.FlushAsync();
        }
        catch (Exception ex)
        {
            if (_options.HandleWriteError != null)
            {
                var fileWriteError = new FileWriteError(_fileName, ex);
                _options.HandleWriteError(fileWriteError);
            }
        }
    }

    /// <summary>
    /// 判断是否需要创建新文件写入（仅长连接模式使用）
    /// </summary>
    /// <returns><see cref="Task"/></returns>
    private async Task CheckForNewLogFileAsync()
    {
        // 兼容模式下跳过此方法
        if (_isCompatibleMode) return;

        var openNewFile = false;
        if (isMaxFileSizeThresholdReached() || isBaseFileNameChanged())
            openNewFile = true;

        // 重新创建新文件并写入
        if (openNewFile)
        {
            await CloseAsync();

            // 计算新文件名
            _fileName = GetNextFileName();

            // 打开新文件并写入
            await OpenFileAsync(false);
        }

        // 是否超出限制的最大大小
        bool isMaxFileSizeThresholdReached() => _options.FileSizeLimitBytes > 0
            && _fileStream?.Length > _options.FileSizeLimitBytes;

        // 是否重新自定义了文件名
        bool isBaseFileNameChanged()
        {
            if (_options.FileNameRule != null)
            {
                var baseFileName = GetBaseFileName();

                if (baseFileName != _lastBaseFileName)
                {
                    _lastBaseFileName = baseFileName;

                    // 滚动日志文件名规则变更后重建列表
                    if (_isEnabledRollingFiles)
                    {
                        RebuildRollingFileNames();
                    }
                    return true;
                }

                return false;
            }

            return false;
        }
    }

    /// <summary>
    /// 删除超出滚动日志限制的文件
    /// </summary>
    /// <param name="fileInfo"></param>
    private void DropFilesIfOverLimit(FileInfo fileInfo)
    {
        // 判断是否启用滚动文件功能
        if (!_isEnabledRollingFiles) return;

        // 处理 Windows 和 Linux 路径分隔符不一致问题
        var fName = fileInfo.FullName.Replace('\\', '/');

        // 将当前文件名存储到集合中
        _fileLoggerProvider._rollingFileNames.AddOrUpdate(fName, fileInfo, (key, oldValue) => fileInfo);

        // 判断超出限制的文件自动删除
        if (_fileLoggerProvider._rollingFileNames.Count > _options.MaxRollingFiles)
        {
            // 收集需要删除的文件
            var filesToDelete = new List<string>(_fileLoggerProvider._rollingFileNames.Count - _options.MaxRollingFiles);

            foreach (var item in _fileLoggerProvider._rollingFileNames.OrderBy(u => u.Value.LastWriteTimeUtc)
                .Take(_fileLoggerProvider._rollingFileNames.Count - _options.MaxRollingFiles))
            {
                if (_fileLoggerProvider._rollingFileNames.TryRemove(item.Key, out _))
                {
                    filesToDelete.Add(item.Key);
                }
            }

            // 批量异步删除
            if (filesToDelete.Count > 0)
            {
                Task.Run(() =>
                {
                    foreach (var filePath in filesToDelete)
                    {
                        try
                        {
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        }
                        catch { }
                    }
                });
            }
        }
    }

    /// <summary>
    /// 处理当文件被外部删除时重新建立连接
    /// </summary>
    private async Task ReconnectFileAsync()
    {
        // 检查是否已释放
        if (_isDisposed) return;

        // 轻量锁：避免多线程同时重连
        if (Interlocked.Exchange(ref _reconnecting, 1) == 1)
            return; // 已有线程在重连，直接返回

        try
        {
            await CloseAsync();
            GetCurrentFileName();
            RebuildRollingFileNames();
            await OpenFileAsync(_options.Append);
        }
        finally
        {
            Interlocked.Exchange(ref _reconnecting, 0);
        }
    }

    /// <summary>
    /// 判断异常是否表示文件不存在/句柄无效
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFileMissingException(Exception ex)
    {
        return ex switch
        {
            FileNotFoundException or DirectoryNotFoundException or ObjectDisposedException => true,
            // IOException：检查 HResult（低 16 位为 Win32 错误码）
            // 2=FILE_NOT_FOUND, 3=PATH_NOT_FOUND, 6=INVALID_HANDLE, 32=SHARING_VIOLATION
            IOException ioEx => (ioEx.HResult & 0xFFFF) is 2 or 3 or 6 or 32,
            // UnauthorizedAccessException：文件被删除后权限检查可能报此错
            UnauthorizedAccessException => true,
            _ => false
        };
    }

    /// <summary>
    /// 检查文件是否仍存在于文件系统中
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsFileStillExists() => File.Exists(_fileName);

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="logMsg">日志消息</param>
    /// <param name="flush"></param>
    /// <returns><see cref="Task"/></returns>
    internal async Task WriteAsync(LogMessage logMsg, bool flush)
    {
        // 检查是否已释放
        if (_isDisposed) return;

        // 检查是否是兼容模式
        if (_isCompatibleMode)
        {
            try
            {
                await WriteWithCompatibleModeAsync(logMsg.Message);
            }
            catch (Exception ex)
            {
                // 检查是否已释放
                if (_options.HandleWriteError != null)
                {
                    var fileWriteError = new FileWriteError(_fileName, ex);
                    _options.HandleWriteError(fileWriteError);
                }
            }
            return;
        }

        // 长连接模式
        if (_textWriter == null) return;

        try
        {
            await CheckForNewLogFileAsync();

            var retry = 0;
            const int maxRetries = 3;
            const int baseDelayMs = 100;

            while (retry < maxRetries)
            {
                // 检查是否已释放
                if (_isDisposed) return;

                try
                {
                    // 直接写入
                    await _textWriter.WriteLineAsync(logMsg.Message);
                    if (flush) await _textWriter.FlushAsync();

                    // 显式 flush 时或每 100 次写入时检查文件是否存在
                    if (flush || Interlocked.Increment(ref _writeCount) >= PeriodicCheckInterval)
                    {
                        // 重置计数器
                        _writeCount = 0;

                        // 如果文件不存在，说明被外部删除，需要重连
                        if (!IsFileStillExists())
                        {
                            await ReconnectFileAsync();
                        }
                    }
                    break;
                }
                // 处理临时性 IO 错误（如文件被短暂锁定）
                catch (IOException) when (retry < maxRetries - 1)
                {
                    retry++;
                    await Task.Delay(baseDelayMs * retry);
                }
                // 捕获文件不存在/句柄无效等异常，自动重连
                catch (Exception ex) when (IsFileMissingException(ex) && retry < maxRetries - 1)
                {
                    await ReconnectFileAsync();
                    retry++;
                }
            }
        }
        // 处理文件写入错误
        catch (Exception ex)
        {
            if (_options.HandleWriteError != null)
            {
                var fileWriteError = new FileWriteError(_fileName, ex);
                _options.HandleWriteError(fileWriteError);
            }
        }
    }

    /// <summary>
    /// 关闭文本写入器并释放（用于滚动文件或重连时关闭当前流）
    /// </summary>
    /// <returns><see cref="Task"/></returns>
    internal async Task CloseAsync()
    {
        // 兼容模式下无需关闭长连接资源
        if (_isCompatibleMode) return;
        if (_textWriter == null) return;

        try
        {
            await _textWriter.FlushAsync();
        }
        finally
        {
            await _textWriter.DisposeAsync();

            if (_fileStream != null)
            {
                await _fileStream.DisposeAsync();
                _fileStream = null;
            }

            _textWriter = null;
        }
    }

    /// <summary>
    /// 释放写入器资源
    /// </summary>
    /// <returns><see cref="Task"/></returns>
    internal async Task DisposeAsync()
    {
        // 检查是否已释放
        if (_isDisposed) return;

        _isDisposed = true;

        await CloseAsync();
    }
}