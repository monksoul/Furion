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
using System.Diagnostics;
using System.Threading.Channels;

namespace Furion.HttpRemote;

/// <summary>
///     文件上传管理器
/// </summary>
internal sealed class FileUploadManager
{
    /// <inheritdoc cref="HttpFileUploadBuilder" />
    internal readonly HttpFileUploadBuilder _httpFileUploadBuilder;

    /// <inheritdoc cref="IHttpRemoteService" />
    internal readonly IHttpRemoteService _httpRemoteService;

    /// <summary>
    ///     文件传输进度信息的通道
    /// </summary>
    internal readonly Channel<FileTransferProgress> _progressChannel;

    /// <summary>
    ///     <inheritdoc cref="FileUploadManager" />
    /// </summary>
    /// <param name="httpRemoteService">
    ///     <see cref="IHttpRemoteService" />
    /// </param>
    /// <param name="httpFileUploadBuilder">
    ///     <see cref="HttpFileUploadBuilder" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    internal FileUploadManager(IHttpRemoteService httpRemoteService, HttpFileUploadBuilder httpFileUploadBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteService);
        ArgumentNullException.ThrowIfNull(httpFileUploadBuilder);

        _httpRemoteService = httpRemoteService;
        _httpFileUploadBuilder = httpFileUploadBuilder;

        // 初始化文件传输进度信息的通道
        _progressChannel = Channel.CreateBounded<FileTransferProgress>(new BoundedChannelOptions(1)
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        // 解析 IHttpFileTransferEventHandler 事件处理程序
        FileTransferEventHandler = (httpFileUploadBuilder.FileTransferEventHandlerType is not null
            ? httpRemoteService.For(httpFileUploadBuilder.FileTransferEventHandlerType)
            : null) as IHttpFileTransferEventHandler;

        // 构建 HttpRequestBuilder 实例
        RequestBuilder =
            httpFileUploadBuilder.Build(httpRemoteService.For<IOptionsMonitor<HttpRemoteOptions>>().CurrentValue,
                _progressChannel);
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
    ///     开始上传
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    /// <exception cref="NotImplementedException"></exception>
    internal HttpResponseMessage? Start(CancellationToken cancellationToken = default)
        => AsyncUtility.RunSync(() => StartAsync(cancellationToken));

    /// <summary>
    ///     开始上传
    /// </summary>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    internal async Task<HttpResponseMessage?> StartAsync(CancellationToken cancellationToken = default)
    {
        // 递增活跃传输计数，用于并发进度条显示控制
        FileTransferProgress.IncrementActiveCount();

        // 创建进度报告任务取消标识
        using var progressCancellationTokenSource = new CancellationTokenSource();

        // 初始化进度报告任务
        var reportProgressTask = ReportProgressAsync(progressCancellationTokenSource.Token, cancellationToken);

        // 处理文件传输开始
        HandleTransferStarted();

        // 初始化 Stopwatch 实例并开启计时操作
        var stopwatch = Stopwatch.StartNew();

        HttpResponseMessage? httpResponseMessage;

        try
        {
            // 发送 HTTP 远程请求
            httpResponseMessage = await _httpRemoteService.SendAsync(RequestBuilder, cancellationToken);

            // 计算文件传输总花费时间并处理文件传输完成
            HandleTransferCompleted(stopwatch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            // 处理文件传输失败
            HandleTransferFailed(e);

            throw;
        }
        finally
        {
            // 递减活跃传输计数，用于并发进度条显示控制
            FileTransferProgress.DecrementActiveCount();

            // 停止计时
            stopwatch.Stop();

            // 关闭通道
            _progressChannel.Writer.Complete();

            // 等待进度报告任务完成
            await progressCancellationTokenSource.CancelAsync();
            await reportProgressTask;
        }

        return httpResponseMessage;
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
        if (_httpFileUploadBuilder.OnProgressChanged is null && FileTransferEventHandler is null)
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

        _httpFileUploadBuilder.OnTransferStarted.TryInvoke();
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

        _httpFileUploadBuilder.OnTransferCompleted.TryInvoke(duration);
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

        _httpFileUploadBuilder.OnTransferFailed.TryInvoke(e);
    }

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

        await _httpFileUploadBuilder.OnProgressChanged.TryInvokeAsync(fileTransferProgress);
    }
}