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
using Furion.Utilities;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Furion.HttpRemote;

/// <summary>
///     文件传输的进度信息
/// </summary>
public sealed partial class FileTransferProgress
{
    /// <summary>
    ///     使用一个小的正值来防止除零错误
    /// </summary>
    internal const double _epsilon = double.Epsilon;

    /// <summary>
    ///     速率计算的时间窗口（毫秒）
    /// </summary>
    internal const int SpeedCalculationWindowMs = 2000;

    /// <summary>
    ///     控制台输出的全局锁
    /// </summary>
    internal static readonly object _consoleLock = new();

    /// <summary>
    ///     当前活跃的文件传输数量
    /// </summary>
    internal static int _activeDownloadCount;

    /// <summary>
    ///     更新进度临时锁对象
    /// </summary>
    internal readonly object _historyLock = new();

    /// <summary>
    ///     最近传输记录（时间点 + 已传输字节数）
    /// </summary>
    internal readonly Queue<(DateTimeOffset Timestamp, long Bytes)> _transferHistory = new();

    /// <summary>
    ///     标记是否已打印文件头
    /// </summary>
    /// <remarks>仅适用于 <see cref="UpdateConsoleProgress" /> 方法。</remarks>
    internal bool _hasPrintedHeader;

    /// <summary>
    ///     上一次输出文本的显示长度
    /// </summary>
    internal int _lastDisplayLength;

    /// <summary>
    ///     <inheritdoc cref="FileTransferProgress" />
    /// </summary>
    /// <param name="filePath">文件的路径</param>
    /// <param name="fileSize">文件的大小</param>
    /// <param name="fileName">文件的名称</param>
    internal FileTransferProgress(string filePath, long fileSize, string? fileName = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        FileSize = fileSize;

        FilePath = filePath;
        FileName = fileName ?? Path.GetFileName(filePath);
    }

    /// <summary>
    ///     文件的路径
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    ///     文件的名称
    /// </summary>
    public string FileName { get; }

    /// <summary>
    ///     文件的大小
    /// </summary>
    /// <remarks>以字节为单位。</remarks>
    public long FileSize { get; internal set; }

    /// <summary>
    ///     已传输的数据量
    /// </summary>
    /// <remarks>以字节为单位。</remarks>
    public long Transferred { get; private set; }

    /// <summary>
    ///     已完成的传输百分比
    /// </summary>
    public double PercentageComplete { get; private set; }

    /// <summary>
    ///     当前的传输速率
    /// </summary>
    /// <remarks>以字节/秒为单位。</remarks>
    public double TransferRate { get; private set; }

    /// <summary>
    ///     从开始传输到现在的持续时间
    /// </summary>
    public TimeSpan TimeElapsed { get; private set; }

    /// <summary>
    ///     预估剩余传输时间
    /// </summary>
    public TimeSpan EstimatedTimeRemaining { get; private set; }

    /// <summary>
    ///     递增活跃传输计数
    /// </summary>
    internal static void IncrementActiveCount() => Interlocked.Increment(ref _activeDownloadCount);

    /// <summary>
    ///     递减活跃传输计数
    /// </summary>
    internal static void DecrementActiveCount() => Interlocked.Decrement(ref _activeDownloadCount);

    /// <inheritdoc />
    public override string ToString() =>
        StringUtility.FormatKeyValuesSummary([
            new KeyValuePair<string, IEnumerable<string>>("File Name", [FileName]),
            new KeyValuePair<string, IEnumerable<string>>("File Path", [FilePath]),
            new KeyValuePair<string, IEnumerable<string>>("File Size",
                [FileSize > 0 ? $"{FileSize.ToSizeUnits("MB"):F2}MB" : "Unknown"]),
            new KeyValuePair<string, IEnumerable<string>>("Transferred", [$"{Transferred.ToSizeUnits("MB"):F2}MB"]),
            new KeyValuePair<string, IEnumerable<string>>("Percentage Complete",
                [FileSize > 0 ? $"{PercentageComplete:F2}%" : "N/A"]),
            new KeyValuePair<string, IEnumerable<string>>("Transfer Rate",
                [$"{TransferRate.ToSizeUnits("MB"):F2}MB/s"]),
            new KeyValuePair<string, IEnumerable<string>>("Time Elapsed (s)", [$"{TimeElapsed.TotalSeconds:F2}"]),
            new KeyValuePair<string, IEnumerable<string>>("Estimated Time Remaining (s)",
            [
                FileSize > 0 && EstimatedTimeRemaining != TimeSpan.MaxValue
                    ? $"{EstimatedTimeRemaining.TotalSeconds:F2}"
                    : "Unknown"
            ])
        ], "Transfer Progress")!;

    /// <inheritdoc cref="ToString" />
    public Task<string> ToStringAsync() => Task.FromResult(ToString());

    /// <summary>
    ///     输出简要进度字符串
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string ToSummaryString()
    {
        // 判断是否已完成传输
        var isComplete = FileSize > 0 && PercentageComplete >= 100.0;

        // 初始化已传输的数据文本
        var sizeText = FileSize > 0
            ? $"{Transferred.ToSizeUnits("MB"):F2}MB of {FileSize.ToSizeUnits("MB"):F2}MB ({PercentageComplete:F2}% complete)"
            : $"{Transferred.ToSizeUnits("MB"):F2}MB (Unknown size)";

        // 初始化预估剩余传输时间文本
        var etaPart = isComplete
            ? string.Empty
            : FileSize > 0 && EstimatedTimeRemaining != TimeSpan.MaxValue
                ? $", ETA: {EstimatedTimeRemaining.TotalSeconds:F2}s"
                : ", ETA: Unknown";

        // 初始化完成状态文本
        var donePart = isComplete ? " Done!" : string.Empty;

        return
            $"Transferred {sizeText}, Speed: {TransferRate.ToSizeUnits("MB"):F2}MB/s, Time: {TimeElapsed.TotalSeconds:F2}s{etaPart}.{donePart} File: {FileName}, Path: {FilePath}.";
    }

    /// <inheritdoc cref="ToSummaryStringAsync" />
    public Task<string> ToSummaryStringAsync() => Task.FromResult(ToSummaryString());

    /// <summary>
    ///     在控制台中更新（打印）文件传输进度条
    /// </summary>
    /// <remarks>需确保应用项目支持 <see cref="Console" /> 输出。</remarks>
    public Task UpdateConsoleProgressAsync()
    {
        UpdateConsoleProgress();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     在控制台中更新（打印）文件传输进度条
    /// </summary>
    /// <remarks>需确保应用项目支持 <see cref="Console" /> 输出。</remarks>
    public void UpdateConsoleProgress()
    {
        // 确保多文件传输时控制台进度输出串行化
        lock (_consoleLock)
        {
            // 判断是否已完成传输
            var isComplete = FileSize > 0 && PercentageComplete >= 100.0;

            // 判断是否存在多个文件同时传输
            if (_activeDownloadCount > 1 && !isComplete)
            {
                return;
            }

            // 检查是否已打印文件头
            if (!_hasPrintedHeader)
            {
                // 构造跨平台的 file:// URL
                var fileUrl = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "file:///" : "file://") +
                              FilePath.Replace("\\", "/");

                // 实现控制台可点击的路径，参考文献：https://learn.microsoft.com/zh-cn/windows/console/console-virtual-terminal-sequences
                Console.WriteLine($"File: {FileName}, Path: \e]8;;{fileUrl}\a{FilePath}\e]8;;\a");
                _hasPrintedHeader = true;
            }

            // 初始化控制台宽度
            var windowWidth = 120;

            // 获取控制台实际宽度
            try
            {
                windowWidth = Console.WindowWidth;
            }
            catch
            {
                try
                {
                    windowWidth = Console.BufferWidth;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }
            }

            // 计算自适应进度条宽度，10-20 字符最合适
            var barWidth = isComplete ? 20 : (int)Math.Clamp(windowWidth * 0.3, 10, 20);

            // 初始化进度文本
            string progressText;

            // 初始化传输完成显示的 Done! 字符串
            const string done = " \e[32mDone!\e[0m";

            // 已知文件大小则显示标准进度条
            if (FileSize > 0)
            {
                var progress = (int)Math.Clamp(PercentageComplete, 0, 100);
                var filledLength = (int)(progress / 100.0 * barWidth);
                var progressBar = new string('#', filledLength) + new string('.', barWidth - filledLength);
                var statusSuffix = isComplete ? done : string.Empty;
                var etaPart = isComplete
                    ? string.Empty
                    : $", ETA: {EstimatedTimeRemaining.TotalMilliseconds.FormatDuration()}";

                progressText =
                    $"[{progressBar}] {PercentageComplete:F2}% ({Transferred.ToSizeUnits("MB"):F2}MB/{FileSize.ToSizeUnits("MB"):F2}MB) Speed: {TransferRate.ToSizeUnits("MB"):F2}MB/s, Time: {TimeElapsed.TotalMilliseconds.FormatDuration()}{etaPart}.{statusSuffix}";
            }
            // 未知文件大小则显示动态动画进度条
            else
            {
                const string animatedChars = "|/-\\";
                var animatedChar = animatedChars[(int)(TimeElapsed.TotalMilliseconds / 100) % animatedChars.Length];
                var halfWidth = barWidth / 2;
                var progressBar = new string('#', halfWidth) + animatedChar + new string('.', barWidth - halfWidth - 1);

                progressText =
                    $"[{progressBar}] {Transferred.ToSizeUnits("MB"):F2}MB (Unknown size) Speed: {TransferRate.ToSizeUnits("MB"):F2}MB/s, Time: {TimeElapsed.TotalMilliseconds.FormatDuration()}, ETA: Unknown.";
            }

            // 处理窗口过小问题（截断处理）
            if (GetDisplayLength(progressText) >= windowWidth)
            {
                progressText = !isComplete
                    ? progressText[..(windowWidth - 4)] + "..."
                    : progressText[..(windowWidth - 4 - GetDisplayLength(done))] + "..." + done;
            }

            // 清除当前行并写入新进度
            try
            {
                Console.CursorLeft = 0;
                Console.Write(progressText);

                // 获取文本的显示长度和是否需要填充
                var displayLen = GetDisplayLength(progressText);
                var padding = windowWidth - 1 - displayLen;

                // 填充空格
                if (padding > 0)
                {
                    Console.Write(new string(' ', padding));
                }

                // 检查是否已完成传输
                if (isComplete)
                {
                    Console.WriteLine();
                    _hasPrintedHeader = false;
                }
                else
                {
                    Console.CursorLeft = 0;
                }

                // 更新历史显示长度
                _lastDisplayLength = displayLen;

                return;
            }
            // 控制台不可用
            catch (IOException) { }
            // 某些平台不支持
            catch (PlatformNotSupportedException) { }

            // 发生异常时回退输出
            FallbackWrite(progressText, isComplete);
        }
    }

    /// <summary>
    ///     更新文件传输进度
    /// </summary>
    /// <param name="transferred">已传输的数据量</param>
    /// <param name="timeElapsed">从开始传输到现在的持续时间</param>
    internal void UpdateProgress(long transferred, TimeSpan timeElapsed)
    {
        // 获取当前 UTC 时间
        var now = DateTimeOffset.UtcNow;

        lock (_historyLock)
        {
            // 记录当前传输点
            _transferHistory.Enqueue((now, transferred));

            // 清理过期记录（早于当前时间 - 窗口大小）
            while (_transferHistory.Count > 1 &&
                   (now - _transferHistory.Peek().Timestamp).TotalMilliseconds > SpeedCalculationWindowMs)
            {
                _transferHistory.Dequeue();
            }
        }

        // 计算瞬时速率：最近窗口内的字节数变化 / 时间变化
        double transferRate;
        lock (_historyLock)
        {
            // 数据不足，使用总平均速率
            if (_transferHistory.Count < 2)
            {
                transferRate = timeElapsed.TotalSeconds > _epsilon ? transferred / timeElapsed.TotalSeconds : 0;
            }
            else
            {
                var first = _transferHistory.Peek();
                var last = _transferHistory.ElementAt(_transferHistory.Count - 1);

                // 计算窗口期内的字节增量和时间增量
                var bytesDelta = last.Bytes - first.Bytes;
                var timeDelta = (last.Timestamp - first.Timestamp).TotalSeconds;

                // 计算瞬时速率（字节/秒）
                transferRate = timeDelta > _epsilon ? bytesDelta / timeDelta : 0;
            }
        }

        // 计算已完成的传输百分比
        var percentageComplete = FileSize > 0 ? 100.0 * transferred / FileSize : -1;

        // 更新内部进度状态
        Transferred = transferred;
        TimeElapsed = timeElapsed;
        PercentageComplete = percentageComplete;
        TransferRate = transferRate;

        // 计算预估剩余传输时间
        EstimatedTimeRemaining = CalculateEstimatedTimeRemaining();
    }

    /// <summary>
    ///     计算预估剩余传输时间
    /// </summary>
    /// <returns>
    ///     <see cref="TimeSpan" />
    /// </returns>
    internal TimeSpan CalculateEstimatedTimeRemaining()
    {
        // 如果文件大小小于等于 0 或传输速率为 0 或接近 0，则认为无法预估
        if (FileSize <= 0 || TransferRate <= _epsilon)
        {
            return TimeSpan.MaxValue;
        }

        // 计算剩余时间
        var secondsRemaining = (FileSize - Transferred) / TransferRate;

        // 如果剩余时间超过最大值，则返回最大值
        return secondsRemaining > TimeSpan.MaxValue.TotalSeconds
            ? TimeSpan.MaxValue
            : TimeSpan.FromSeconds(secondsRemaining);
    }

    /// <summary>
    ///     回退输出
    /// </summary>
    /// <param name="text">要输出的进度文本</param>
    /// <param name="isComplete">是否已完成传输</param>
    internal void FallbackWrite(string text, bool isComplete)
    {
        // 获取文本的显示长度
        var displayLen = GetDisplayLength(text);

        // 初始化需要填充的最长显示长度
        var fillLen = Math.Max(_lastDisplayLength, displayLen);
        var padding = fillLen - displayLen;

        // 输出文本
        Console.Write("\r" + text);

        // 填充空格
        if (padding > 0)
        {
            Console.Write(new string(' ', padding));
        }

        // 完成时换行并重置头标记
        if (isComplete)
        {
            Console.WriteLine();
            _hasPrintedHeader = false;
        }

        _lastDisplayLength = fillLen;
    }

    /// <summary>
    ///     获取字符串的显示长度
    /// </summary>
    /// <param name="text">字符串</param>
    /// <returns>
    ///     <see cref="int" />
    /// </returns>
    internal static int GetDisplayLength(string text) => StripAnsi(text).Length;

    /// <summary>
    ///     移除 ANSI 转义序列
    /// </summary>
    /// <param name="text">字符串</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string StripAnsi(string text) => AnsiRegex().Replace(text, string.Empty);

    /// <summary>
    ///     ANSI 转义序列正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(@"\e\[[\d;]*[a-zA-Z]")]
    private static partial Regex AnsiRegex();
}