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
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.Net;
using System.Threading.Channels;

namespace Furion.HttpRemote;

/// <summary>
///     文件下载管理器
/// </summary>
internal sealed class FileDownloadManager
{
    /// <inheritdoc cref="HttpFileDownloadBuilder" />
    internal readonly HttpFileDownloadBuilder _httpFileDownloadBuilder;

    /// <inheritdoc cref="IHttpRemoteService" />
    internal readonly IHttpRemoteService _httpRemoteService;

    /// <summary>
    ///     文件传输进度信息的通道
    /// </summary>
    internal readonly Channel<FileTransferProgress> _progressChannel;

    /// <inheritdoc cref="Throttler" />
    internal readonly Throttler _throttler;

    /// <summary>
    ///     全局已接收字节数
    /// </summary>
    /// <remarks>用于多线程分块下载进度计算。</remarks>
    internal long _totalBytesReceived;

    /// <summary>
    ///     <inheritdoc cref="FileDownloadManager" />
    /// </summary>
    /// <param name="httpRemoteService">
    ///     <see cref="IHttpRemoteService" />
    /// </param>
    /// <param name="httpFileDownloadBuilder">
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    internal FileDownloadManager(IHttpRemoteService httpRemoteService, HttpFileDownloadBuilder httpFileDownloadBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteService);
        ArgumentNullException.ThrowIfNull(httpFileDownloadBuilder);

        _httpRemoteService = httpRemoteService;
        _httpFileDownloadBuilder = httpFileDownloadBuilder;

        // 初始化文件传输进度信息的通道
        _progressChannel = Channel.CreateUnbounded<FileTransferProgress>(new UnboundedChannelOptions
        {
            SingleReader = true, SingleWriter = false, AllowSynchronousContinuations = true
        });

        // 初始化节流器实例
        _throttler = new Throttler(_httpFileDownloadBuilder.ProgressInterval);

        // 解析 IHttpFileTransferEventHandler 事件处理程序
        FileTransferEventHandler = (httpFileDownloadBuilder.FileTransferEventHandlerType is not null
            ? httpRemoteService.For(httpFileDownloadBuilder.FileTransferEventHandlerType)
            : null) as IHttpFileTransferEventHandler;

        // 构建 HttpRequestBuilder 实例
        RequestBuilder =
            httpFileDownloadBuilder.Build(httpRemoteService.For<IOptionsMonitor<HttpRemoteOptions>>().CurrentValue);
    }

    /// <summary>
    ///     <inheritdoc cref="HttpRequestBuilder" />
    /// </summary>
    internal HttpRequestBuilder RequestBuilder { get; }

    /// <summary>
    ///     <inheritdoc cref="IHttpFileTransferEventHandler" />
    /// </summary>
    internal IHttpFileTransferEventHandler? FileTransferEventHandler { get; }

    /// <summary>
    ///     开始下载
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="FileTransferResult" />
    /// </returns>
    internal FileTransferResult Start(CancellationToken cancellationToken = default)
        => AsyncUtility.RunSync(() => StartAsync(cancellationToken));

    /// <summary>
    ///     开始下载
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="FileTransferResult" />
    /// </returns>
    internal async Task<FileTransferResult> StartAsync(CancellationToken cancellationToken = default)
    {
        // 递增活跃传输计数，用于并发进度条显示控制
        FileTransferProgress.IncrementActiveCount();

        // 初始化 FileTransferResult 实例
        var fileTransferResult = new FileTransferResult();

        // 创建进度报告任务取消标识
        using var progressCancellationTokenSource = new CancellationTokenSource();

        // 初始化进度报告任务
        var reportProgressTask = ReportProgressAsync(progressCancellationTokenSource.Token, cancellationToken);

        // 处理文件传输开始
        HandleTransferStarted();

        // 获取临时文件路径
        var tempDestinationPath = Path.GetTempFileName();

        // 声明 FileStream 变量
        FileStream? fileStream = null;

        // 初始化 Stopwatch 实例并开启计时操作
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 发送 HTTP 远程请求
            using var httpResponseMessage = await _httpRemoteService.SendAsync(RequestBuilder,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // 设置文件下载地址和响应状态码
            fileTransferResult.RequestUri =
                httpResponseMessage?.RequestMessage?.RequestUri ?? _httpFileDownloadBuilder.RequestUri;
            fileTransferResult.StatusCode = httpResponseMessage?.StatusCode;

            // 空检查
            if (httpResponseMessage is null)
            {
                // 设置文件传输结果信息
                fileTransferResult.IsSuccess = false;
                return fileTransferResult;
            }

            // 根据文件是否存在及配置的行为来决定是否应继续进行文件下载
            if (!ShouldContinueWithDownload(httpResponseMessage, out var destinationPath))
            {
                // 处理文件存在且配置为跳过时的操作
                HandleFileExistAndSkip();

                // 设置文件传输结果信息
                fileTransferResult.FilePath = destinationPath;
                fileTransferResult.FileSize = new FileInfo(destinationPath).Length;
                fileTransferResult.IsSuccess = true; // 因文件存在而跳过也被视为成功
                return fileTransferResult;
            }

            // 获取文件总大小和服务器是否支持 Range 请求
            var contentLength = httpResponseMessage.Content.Headers.ContentLength ?? -1;
            var acceptRanges = httpResponseMessage.Headers.AcceptRanges;
            var supportsRange = acceptRanges.Count > 0 && acceptRanges.Contains("bytes");

            // 判断是否启用了多线程下载，且服务器支持 Range 请求、文件大小有效
            var isMultiThreaded = _httpFileDownloadBuilder.MaxThreads > 1 && supportsRange && contentLength > 0;

            // 初始化 FileTransferProgress 实例
            var fileTransferProgress = new FileTransferProgress(destinationPath, contentLength);

            // 初始化 FileStream 实例，使用文件流创建文件，设置写入模式，并允许其他进程同时读取文件
            fileStream = new FileStream(tempDestinationPath, FileMode.Create, FileAccess.Write, FileShare.Read,
                _httpFileDownloadBuilder.BufferSize, true);

            // 实际接收到的字节大小
            long actualBytesReceived;

            // 检查是否启用了多线程下载
            if (isMultiThreaded)
            {
                // 多线程分块下载
                await DownloadInChunksAsync(contentLength, fileStream, fileTransferProgress, stopwatch,
                    cancellationToken);

                // 同步实际接收到的字节大小
                actualBytesReceived = _totalBytesReceived;
            }
            else
            {
                // 单线程下载
                actualBytesReceived = await DownloadSingleThreadedAsync(httpResponseMessage, fileStream,
                    fileTransferProgress, stopwatch, contentLength, cancellationToken);
            }

            // 移动临时文件至文件保存的目标路径
            MoveTempFileToDestinationPath(fileStream, tempDestinationPath, destinationPath);

            // 计算文件传输总花费时间
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            // 处理文件传输完成
            HandleTransferCompleted(elapsedMilliseconds);

            // 设置文件传输结果信息
            fileTransferResult.FilePath = destinationPath;
            fileTransferResult.FileSize = actualBytesReceived;
            fileTransferResult.ElapsedMilliseconds = elapsedMilliseconds;
            fileTransferResult.IsSuccess = true;
            return fileTransferResult;
        }
        catch (Exception e)
        {
            // 清理临时文件
            fileStream?.Close();
            if (File.Exists(tempDestinationPath))
            {
                File.Delete(tempDestinationPath);
            }

            // 处理文件传输失败
            HandleTransferFailed(e);

            throw;
        }
        finally
        {
            // 递减活跃传输计数，用于并发进度条显示控制
            FileTransferProgress.DecrementActiveCount();

            // 释放 FileStream 实例
            if (fileStream is not null)
            {
                await fileStream.DisposeAsync();
            }

            // 停止计时
            stopwatch.Stop();

            // 关闭通道
            _progressChannel.Writer.Complete();

            // 等待进度报告任务完成
            await progressCancellationTokenSource.CancelAsync();
            await reportProgressTask;
        }
    }

    /// <summary>
    ///     多线程分块下载
    /// </summary>
    /// <param name="contentLength">文件总大小</param>
    /// <param name="fileStream">
    ///     <see cref="FileStream" />
    /// </param>
    /// <param name="fileTransferProgress">
    ///     <see cref="FileTransferProgress" />
    /// </param>
    /// <param name="stopwatch">
    ///     <see cref="Stopwatch" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task DownloadInChunksAsync(long contentLength, FileStream fileStream,
        FileTransferProgress fileTransferProgress, Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        // 获取配置的下载最大线程数并计算每个分块的大小
        var maxThreads = _httpFileDownloadBuilder.MaxThreads;
        var chunkSize = (contentLength + maxThreads - 1) / maxThreads;

        // 重置全局已接收字节数
        _totalBytesReceived = 0;

        // 创建联动取消令牌，防止某个分块彻底失败导致其他分块继续运行
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 初始化任务列表和每个分块创建的临时文件列表
        var tasks = new List<Task>(maxThreads);
        var chunkTempFiles = new string[maxThreads];

        // 根据配置的最大线程数创建分块下载任务
        for (var i = 0; i < maxThreads; i++)
        {
            // 计算当前分块的起始和结束位置
            var start = i * chunkSize;
            var end = Math.Min(((i + 1) * chunkSize) - 1, contentLength - 1);

            // 为每个分块创建独立的临时文件
            chunkTempFiles[i] = Path.GetTempFileName();

            // 启动分块下载任务
            tasks.Add(DownloadChunkWithFailFastAsync(start, end, chunkTempFiles[i], linkedCts));
        }

        try
        {
            // 等待所有分块下载任务完成
            await Task.WhenAll(tasks);

            // 重置文件流指针至起始位置
            fileStream.Seek(0, SeekOrigin.Begin);

            var mergeBuffer = new byte[_httpFileDownloadBuilder.BufferSize];

            // 将所有分块文件按顺序合并到主文件中
            foreach (var chunkFile in chunkTempFiles)
            {
                // 检查分块文件是否存在
                if (!File.Exists(chunkFile))
                {
                    continue;
                }

                // 读取分块文件
                await using var chunkStream = new FileStream(chunkFile, FileMode.Open, FileAccess.Read,
                    FileShare.Read, _httpFileDownloadBuilder.BufferSize, true);

                // 合并所有分块文件到主文件
                int bytesRead;
                while ((bytesRead = await chunkStream.ReadAsync(mergeBuffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(mergeBuffer.AsMemory(0, bytesRead), cancellationToken);
                }
            }
        }
        finally
        {
            // 清理掉所有临时分块文件
            foreach (var chunkFile in chunkTempFiles)
            {
                // 检查分块文件是否存在
                if (!File.Exists(chunkFile))
                {
                    continue;
                }

                try
                {
                    File.Delete(chunkFile);
                }
                catch
                {
                    // ignored
                }
            }
        }

        // 更新下载完成时的传输进度
        fileTransferProgress.FileSize = _totalBytesReceived;
        fileTransferProgress.UpdateProgress(_totalBytesReceived, stopwatch.Elapsed);

        // 发送文件传输进度到通道
        await _progressChannel.Writer.WriteAsync(fileTransferProgress, cancellationToken);
        return;

        // 包装分块下载任务，实现异常时的联动取消
        async Task DownloadChunkWithFailFastAsync(long start, long end, string chunkTempFilePath,
            CancellationTokenSource cts)
        {
            try
            {
                await DownloadChunkAsync(start, end, chunkTempFilePath, fileTransferProgress, stopwatch, cts.Token);
            }
            catch
            {
                // 只要有一个分块彻底失败，立即取消其他所有分块
                await cts.CancelAsync();

                throw;
            }
        }
    }

    /// <summary>
    ///     下载单个分块
    /// </summary>
    /// <param name="start">分块起始字节位置</param>
    /// <param name="end">分块结束字节位置</param>
    /// <param name="chunkTempFilePath">分块专属的临时文件路径</param>
    /// <param name="fileTransferProgress">
    ///     <see cref="FileTransferProgress" />
    /// </param>
    /// <param name="stopwatch">
    ///     <see cref="Stopwatch" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task DownloadChunkAsync(long start, long end, string chunkTempFilePath,
        FileTransferProgress fileTransferProgress, Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        // 初始化读取数据的缓冲区和记录进度所需的变量
        var buffer = new byte[_httpFileDownloadBuilder.BufferSize];
        var bytesReceived = 0L;

        //  获取分块下载的最大重试次数和单次读取数据的最大空闲等待时间以及当前重试次数
        var maxRetries = _httpFileDownloadBuilder.ChunkMaxRetries;
        var chunkTimeout = _httpFileDownloadBuilder.ChunkTimeout;
        var currentRetry = 0;

        while (currentRetry <= maxRetries)
        {
            // 处理用户主动取消
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // 根据已成功接收的字节数调整 Range 请求标头
                var currentStart = start + bytesReceived;

                // 检查分块是否已下载完成
                if (currentStart > end)
                {
                    break;
                }

                // 克隆 HttpRequestBuilder 并设置 Range 头
                var clonedBuilder = RequestBuilder
                    .Clone(nameof(HttpRequestBuilder.Disposables), nameof(HttpRequestBuilder.HttpClientPooling))
                    .WithoutTimeout().SetRetry(0)
                    .WithHeader(HeaderNames.Range, $"bytes={currentStart}-{end}", replace: true);

                // 初始化当前重试的超时取消令牌
                using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // 若未配置永不超时，则在规定时间内取消操作
                if (chunkTimeout != Timeout.InfiniteTimeSpan)
                {
                    attemptCts.CancelAfter(chunkTimeout);
                }

                // 发送 HTTP 远程请求
                using var httpResponseMessage = await _httpRemoteService.SendAsync(clonedBuilder,
                    HttpCompletionOption.ResponseHeadersRead, attemptCts.Token);

                // 空检查
                if (httpResponseMessage is null)
                {
                    throw new InvalidOperationException("HTTP response is null.");
                }

                // 检查服务器是否返回了部分内容（HTTP 206）
                if (httpResponseMessage.StatusCode is not HttpStatusCode.PartialContent)
                {
                    throw new InvalidOperationException(
                        $"Server did not return partial content for range {currentStart}-{end}. Status code: {httpResponseMessage.StatusCode}.");
                }

                // 获取 HTTP 响应内容中的内容流
                await using var stream = await httpResponseMessage.Content.ReadAsStreamAsync(attemptCts.Token);

                // 初始化分块文件流
                await using var chunkFileStream = new FileStream(chunkTempFilePath, FileMode.OpenOrCreate,
                    FileAccess.Write, FileShare.None, _httpFileDownloadBuilder.BufferSize, true);

                // 定位分块文件流指针至末尾位置
                chunkFileStream.SetLength(bytesReceived);
                chunkFileStream.Seek(0, SeekOrigin.End);

                // 循环读取数据直到取消请求或分块完成
                while (true)
                {
                    // 处理用户主动取消
                    cancellationToken.ThrowIfCancellationRequested();

                    // 初始化读取分块文件流超时取消令牌
                    using var readCts = CancellationTokenSource.CreateLinkedTokenSource(attemptCts.Token);

                    // 如果不是永不超时，那么就在规定时间内取消操作
                    if (chunkTimeout != Timeout.InfiniteTimeSpan)
                    {
                        readCts.CancelAfter(chunkTimeout);
                    }

                    int numBytesRead;
                    try
                    {
                        // 读取分块流
                        numBytesRead = await stream.ReadAsync(buffer, readCts.Token);
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested &&
                                                             readCts.IsCancellationRequested)
                    {
                        throw new TimeoutException($"Chunk read idle timeout after {chunkTimeout.TotalSeconds}s.");
                    }

                    // 检查是否存在读取到的内容
                    if (numBytesRead == 0)
                    {
                        break;
                    }

                    // 写入当前分块专属的临时文件
                    await chunkFileStream.WriteAsync(buffer.AsMemory(0, numBytesRead), cancellationToken);

                    // 更新已接收字节数
                    bytesReceived += numBytesRead;

                    // 同步实际接收到的字节大小
                    var newTotalBytesReceived = Interlocked.Add(ref _totalBytesReceived, numBytesRead);

                    // 发送文件传输进度到通道
                    // ReSharper disable once InvertIf
                    if (_throttler.TryEnter())
                    {
                        fileTransferProgress.UpdateProgress(newTotalBytesReceived, stopwatch.Elapsed);
                        await _progressChannel.Writer.WriteAsync(fileTransferProgress, cancellationToken);
                    }
                }

                break;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // 下载失败递增重试次数
                currentRetry++;

                // 检查最大重试次数
                if (currentRetry > maxRetries)
                {
                    throw new InvalidOperationException(
                        $"Chunk download failed after {maxRetries} retries. Target range: bytes={start}-{end}, Downloaded before failure: {bytesReceived} bytes.",
                        ex);
                }

                // 指数退避重试
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, currentRetry)), cancellationToken);
            }
        }
    }

    /// <summary>
    ///     单线程下载
    /// </summary>
    /// <remarks>仅在单线程下载或服务器不支持 <c>Range</c> 请求时使用。</remarks>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="fileStream">
    ///     <see cref="FileStream" />
    /// </param>
    /// <param name="fileTransferProgress">
    ///     <see cref="FileTransferProgress" />
    /// </param>
    /// <param name="stopwatch">
    ///     <see cref="Stopwatch" />
    /// </param>
    /// <param name="expectedContentLength">预期的文件总大小</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="long" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task<long> DownloadSingleThreadedAsync(HttpResponseMessage httpResponseMessage,
        FileStream fileStream,
        FileTransferProgress fileTransferProgress, Stopwatch stopwatch,
        long expectedContentLength,
        CancellationToken cancellationToken)
    {
        // 初始化读取数据的缓冲区和记录进度所需的变量
        var buffer = new byte[_httpFileDownloadBuilder.BufferSize];
        var bytesReceived = 0L;

        // 获取单次读取数据的最大空闲等待时间
        var chunkTimeout = _httpFileDownloadBuilder.ChunkTimeout;

        // 获取 HTTP 响应内容中的内容流
        await using var stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

        // 尝试解压内容流，解决部分内容流被压缩的情况
        await using var decompressedStream = Helpers.WrapDecompressionStream(stream, httpResponseMessage);

        // 循环读取数据直到取消请求或读取完毕
        while (!cancellationToken.IsCancellationRequested)
        {
            // 初始化单次读取网络响应流的超时取消令牌
            using var readCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // 若未配置永不超时，则在规定时间内取消操作
            if (chunkTimeout != Timeout.InfiniteTimeSpan)
            {
                readCts.CancelAfter(chunkTimeout);
            }

            int numBytesRead;
            try
            {
                // 读取流
                numBytesRead = await decompressedStream.ReadAsync(buffer, readCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested &&
                                                     readCts.IsCancellationRequested)
            {
                throw new TimeoutException($"Single-thread read idle timeout after {chunkTimeout.TotalSeconds}s.");
            }

            // 检查是否存在读取到的内容
            if (numBytesRead == 0)
            {
                break;
            }

            // 将读取的数据写入文件
            await fileStream.WriteAsync(buffer.AsMemory(0, numBytesRead), cancellationToken);

            // 更新文件传输进度
            bytesReceived += numBytesRead;

            // 判断当前是否允许执行操作
            // ReSharper disable once InvertIf
            if (_throttler.TryEnter())
            {
                // 发送文件传输进度到通道
                fileTransferProgress.UpdateProgress(bytesReceived, stopwatch.Elapsed);
                await _progressChannel.Writer.WriteAsync(fileTransferProgress, cancellationToken);
            }
        }

        // 最终完整性校验
        if (expectedContentLength > 0 && stream == decompressedStream && bytesReceived != expectedContentLength)
        {
            throw new InvalidOperationException(
                $"Download incomplete. Expected {expectedContentLength} bytes, but received {bytesReceived} bytes.");
        }

        // 更新下载完成时的传输进度
        fileTransferProgress.FileSize = bytesReceived;

        // 发送文件传输进度到通道
        fileTransferProgress.UpdateProgress(bytesReceived, stopwatch.Elapsed);
        await _progressChannel.Writer.WriteAsync(fileTransferProgress, cancellationToken);

        return bytesReceived;
    }

    /// <summary>
    ///     文件传输进度报告任务
    /// </summary>
    /// <param name="progressCancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal async Task ReportProgressAsync(CancellationToken progressCancellationToken,
        CancellationToken cancellationToken)
    {
        // 空检查
        if (_httpFileDownloadBuilder.OnProgressChanged is null && FileTransferEventHandler is null)
        {
            return;
        }

        try
        {
            // 从进度通道中读取所有的进度信息
            await foreach (var fileTransferProgress in _progressChannel.Reader.ReadAllAsync(cancellationToken))
            {
                // 如果请求了取消，则抛出 OperationCanceledException
                cancellationToken.ThrowIfCancellationRequested();

                // 检查是否已完成文件传输（确保最后一次进度能够被订阅）
                if (progressCancellationToken.IsCancellationRequested && fileTransferProgress.PercentageComplete >= 1.0)
                {
                    // 处理文件传输进度变化
                    await HandleProgressChangedAsync(fileTransferProgress);

                    break;
                }

                // 处理文件传输进度变化
                await HandleProgressChangedAsync(fileTransferProgress);

                // 根据配置的进度更新（通知）的间隔时间延迟进度报告
                await Task.Delay(_httpFileDownloadBuilder.ProgressInterval, cancellationToken);
            }
        }
        catch (Exception e) when (cancellationToken.IsCancellationRequested || e is OperationCanceledException)
        {
            // 任务被取消
        }
        catch (Exception)
        {
            // ignored
        }
    }

    /// <summary>
    ///     处理文件传输开始
    /// </summary>
    internal void HandleTransferStarted()
    {
        // 空检查
        if (FileTransferEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(FileTransferEventHandler.OnTransferStarted);
        }

        _httpFileDownloadBuilder.OnTransferStarted.TryInvoke();
    }

    /// <summary>
    ///     处理文件传输完成
    /// </summary>
    /// <param name="duration">文件传输总花费时间</param>
    internal void HandleTransferCompleted(long duration)
    {
        // 空检查
        if (FileTransferEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(FileTransferEventHandler.OnTransferCompleted, duration);
        }

        _httpFileDownloadBuilder.OnTransferCompleted.TryInvoke(duration);
    }

    /// <summary>
    ///     处理文件传输失败
    /// </summary>
    /// <param name="e">
    ///     <see cref="Exception" />
    /// </param>
    internal void HandleTransferFailed(Exception e)
    {
        // 空检查
        if (FileTransferEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(FileTransferEventHandler.OnTransferFailed, e);
        }

        _httpFileDownloadBuilder.OnTransferFailed.TryInvoke(e);
    }

    /// <summary>
    ///     处理文件存在且配置为跳过时的操作
    /// </summary>
    internal void HandleFileExistAndSkip() => _httpFileDownloadBuilder.OnFileExistAndSkip.TryInvoke();

    /// <summary>
    ///     处理文件传输进度变化
    /// </summary>
    /// <param name="fileTransferProgress">
    ///     <see cref="FileTransferProgress" />
    /// </param>
    internal async Task HandleProgressChangedAsync(FileTransferProgress fileTransferProgress)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(fileTransferProgress);

        // 空检查
        if (FileTransferEventHandler is not null)
        {
            await DelegateExtensions.TryInvokeAsync(FileTransferEventHandler.OnProgressChangedAsync,
                fileTransferProgress);
        }

        await _httpFileDownloadBuilder.OnProgressChanged.TryInvokeAsync(fileTransferProgress);
    }

    /// <summary>
    ///     根据文件是否存在及配置的行为来决定是否应继续进行文件下载
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal bool ShouldContinueWithDownload(HttpResponseMessage httpResponseMessage, out string destinationPath)
    {
        // 获取 DestinationPath 配置
        var destPath = _httpFileDownloadBuilder.DestinationPath;

        // 初始化用于判断传入的是目录还是文件路径
        bool isDirectory;

        // 空检查
        if (string.IsNullOrWhiteSpace(destPath))
        {
            isDirectory = true;
        }
        // 以目录分隔符结尾视为目录
        else if (destPath.EndsWith(Path.DirectorySeparatorChar) || destPath.EndsWith(Path.AltDirectorySeparatorChar))
        {
            isDirectory = true;
        }
        // 路径是一个已存在的目录视为目录
        else if (Directory.Exists(destPath))
        {
            isDirectory = true;
        }
        // 路径是一个已存在的文件，视为完整文件路径
        else if (File.Exists(destPath))
        {
            isDirectory = false;
        }
        // 路径不存在视为文件路径，通过路径末尾 \ 或 / 来判断是否是目录
        else
        {
            isDirectory = false;
        }

        string? destinationDir;
        string fileName;

        // 检查是否是目录
        if (isDirectory)
        {
            // 解析目录和文件下载名
            destinationDir = destPath;
            fileName = ExtractFileName(httpResponseMessage);
        }
        else
        {
            // 解析目录和文件下载名
            destinationDir = Path.GetDirectoryName(destPath);
            fileName = Path.GetFileName(destPath) ?? string.Empty;

            // 空检查
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // 解析文件下载名
                fileName = ExtractFileName(httpResponseMessage);
            }
        }

        // 获取无效的文件名字符数组
        var invalidChars = Path.GetInvalidFileNameChars();

        // 替换文件名中所有非法字符，默认替换为 '_'，避免因非法字符中断下载程序
        fileName = new string(fileName.Select(c => Array.IndexOf(invalidChars, c) >= 0 ? '_' : c).ToArray());

        // 生成完整的文件存储路径
        destinationPath = Path.GetFullPath(Path.Combine(destinationDir ?? string.Empty, fileName));

        // 检查最终路径不能是一个已存在的目录
        if (Directory.Exists(destinationPath))
        {
            throw new InvalidOperationException(
                $"The destination path '{destinationPath}' is an existing directory, not a file path.");
        }

        // 检查文件是否存在
        if (!File.Exists(destinationPath))
        {
            return true;
        }

        // 检查文件存在时的行为
        switch (_httpFileDownloadBuilder.FileExistsBehavior)
        {
            case FileExistsBehavior.CreateNew:
                throw new InvalidOperationException($"The destination path `{destinationPath}` already exists.");
            case FileExistsBehavior.Skip:
                return false;
            case FileExistsBehavior.Overwrite:
            default:
                break;
        }

        return true;
    }

    /// <summary>
    ///     解析文件下载名
    /// </summary>
    /// <remarks>从 HTTP 响应标头 <c>Content-Disposition</c> 或请求地址中解析文件名。</remarks>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    internal static string ExtractFileName(HttpResponseMessage httpResponseMessage)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 尝试从响应标头 Content-Disposition 中解析文件名
        var fileName = Helpers.ExtractFileNameFromContentDisposition(httpResponseMessage.Content.Headers
            .ContentDisposition);

        // 空检查
        if (string.IsNullOrWhiteSpace(fileName))
        {
            // 尝试从原始的请求地址中解析
            fileName = Helpers.GetFileNameFromUri(httpResponseMessage.RequestMessage?.RequestUri);
        }

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return fileName;
    }

    /// <summary>
    ///     移动临时文件至文件保存的目标路径
    /// </summary>
    /// <param name="fileStream">
    ///     <see cref="FileStream" />
    /// </param>
    /// <param name="tempDestinationPath">临时文件路径</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    internal static void MoveTempFileToDestinationPath(FileStream fileStream, string tempDestinationPath,
        string destinationPath)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(tempDestinationPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        // 检查临时文件是否存在
        if (!File.Exists(tempDestinationPath))
        {
            throw new FileNotFoundException($"The temp destination path `{tempDestinationPath}` does not exist.");
        }

        // 获取文件保存的目标目录
        var destinationDirectory = Path.GetDirectoryName(destinationPath);

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);

        // 如果目录不存在则创建
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        // 如果下载成功，则移动临时文件到文件保存的目标路径（文件存在则替换）
        fileStream.Close();
        File.Move(tempDestinationPath, destinationPath, true);
    }
}