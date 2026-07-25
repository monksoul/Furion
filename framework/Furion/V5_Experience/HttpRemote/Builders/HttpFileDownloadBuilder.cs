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

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 文件下载构建器
/// </summary>
/// <remarks>使用 <c>HttpRequestBuilder.DownloadFile(requestUri, destinationPath)</c> 静态方法创建。</remarks>
public sealed class HttpFileDownloadBuilder
{
    /// <summary>
    ///     <see cref="HttpRequestBuilder" /> 配置委托
    /// </summary>
    internal Action<HttpRequestBuilder>? _configureRequest;

    /// <summary>
    ///     <inheritdoc cref="HttpFileDownloadBuilder" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpFileDownloadBuilder(HttpMethod httpMethod, Uri? requestUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);

        HttpMethod = httpMethod;
        RequestUri = requestUri;
    }

    /// <summary>
    ///     请求地址
    /// </summary>
    public Uri? RequestUri { get; }

    /// <summary>
    ///     请求方式
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    ///     用于传输操作的缓冲区大小
    /// </summary>
    /// <remarks>
    ///     <para>以字节为单位，默认值为 <c>80 KB</c>。</para>
    ///     <para>建议下载小于 <c>10M</c> 的文件使用默认配置，若文件较大，建议设置为 <c>1M</c>（即 <c>1024 * 1024</c>）。</para>
    /// </remarks>
    public int BufferSize { get; private set; } = 80 * 1024;

    /// <summary>
    ///     文件保存的目标路径
    /// </summary>
    public string? DestinationPath { get; private set; }

    /// <summary>
    ///     当目标文件已存在时的行为
    /// </summary>
    /// <remarks>默认值为创建新文件，如果文件已存在则抛出异常。</remarks>
    public FileExistsBehavior FileExistsBehavior { get; private set; } = FileExistsBehavior.CreateNew;

    /// <summary>
    ///     进度更新（通知）的间隔时间
    /// </summary>
    /// <remarks>默认值为 250 毫秒。</remarks>
    public TimeSpan ProgressInterval { get; private set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    ///     下载最大线程数
    /// </summary>
    /// <remarks>推荐值为 4 或 8。默认值为：1。</remarks>
    public int MaxThreads { get; private set; } = 1;

    /// <summary>
    ///     用于处理在文件开始传输时的操作
    /// </summary>
    public Action? OnTransferStarted { get; private set; }

    /// <summary>
    ///     用于处理在文件传输完成时的操作
    /// </summary>
    public Action<long>? OnTransferCompleted { get; private set; }

    /// <summary>
    ///     用于处理在文件传输发生异常时的操作
    /// </summary>
    public Action<Exception>? OnTransferFailed { get; private set; }

    /// <summary>
    ///     用于处理在文件存在且配置为跳过时的操作
    /// </summary>
    public Action? OnFileExistAndSkip { get; private set; }

    /// <summary>
    ///     用于传输进度发生变化时的操作
    /// </summary>
    public Func<FileTransferProgress, Task>? OnProgressChanged { get; private set; }

    /// <summary>
    ///     实现 <see cref="IHttpFileTransferEventHandler" /> 的类型
    /// </summary>
    internal Type? FileTransferEventHandlerType { get; private set; }

    /// <summary>
    ///     设置用于传输操作的缓冲区大小
    /// </summary>
    /// <param name="bufferSize">用于传输操作的缓冲区大小</param>
    /// <remarks>
    ///     <para>以字节为单位，默认值为 <c>80 KB</c>。</para>
    ///     <para>建议下载小于 <c>10M</c> 的文件使用默认配置，若文件较大，建议设置为 <c>1M</c>（即 <c>1024 * 1024</c>）。</para>
    /// </remarks>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileDownloadBuilder SetBufferSize(int bufferSize)
    {
        // 小于或等于 0 检查
        if (bufferSize <= 0)
        {
            throw new ArgumentException("Buffer size must be greater than 0.", nameof(bufferSize));
        }

        BufferSize = bufferSize;

        return this;
    }

    /// <summary>
    ///     设置文件保存的目标路径
    /// </summary>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <remarks>
    ///     如果设置为 <c>null</c>，则尝试获取 HTTP 模块的 <see cref="HttpRemoteBuilder" /> 构建器的 <c>DefaultFileDownloadDirectory</c>
    ///     的属性配置。
    /// </remarks>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileDownloadBuilder SetDestinationPath(string? destinationPath)
    {
        // 跳过空检查
        if (destinationPath is null)
        {
            return this;
        }

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        DestinationPath = destinationPath;

        return this;
    }

    /// <summary>
    ///     设置当目标文件已存在时的行为
    /// </summary>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public HttpFileDownloadBuilder SetFileExistsBehavior(FileExistsBehavior fileExistsBehavior)
    {
        FileExistsBehavior = fileExistsBehavior;

        return this;
    }

    /// <summary>
    ///     设置文件传输进度（通知）的间隔时间
    /// </summary>
    /// <param name="progressInterval">进度更新（通知）的间隔时间</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileDownloadBuilder SetProgressInterval(TimeSpan progressInterval)
    {
        // 小于或等于 0 检查
        if (progressInterval <= TimeSpan.Zero)
        {
            throw new ArgumentException("Progress interval must be greater than 0.", nameof(progressInterval));
        }

        ProgressInterval = progressInterval;

        return this;
    }

    /// <summary>
    ///     设置下载最大线程数
    /// </summary>
    /// <param name="maxThreads">下载最大线程数，推荐值为 4 或 8</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileDownloadBuilder SetMaxThreads(int maxThreads)
    {
        // 小于或等于 0 检查
        if (maxThreads <= 0)
        {
            throw new ArgumentException("Max Threads must be greater than 0.", nameof(maxThreads));
        }

        MaxThreads = maxThreads;

        return this;
    }

    /// <summary>
    ///     设置在文件开始传输时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileDownloadBuilder SetOnTransferStarted(Action configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnTransferStarted = configure;

        return this;
    }

    /// <summary>
    ///     设置用于传输进度发生变化时执行的委托
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileDownloadBuilder SetOnProgressChanged(Func<FileTransferProgress, Task> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnProgressChanged = configure;

        return this;
    }

    /// <summary>
    ///     设置在文件传输完成时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托；委托参数为文件传输总花费时间（毫秒）</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileDownloadBuilder SetOnTransferCompleted(Action<long> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnTransferCompleted = configure;

        return this;
    }

    /// <summary>
    ///     设置在文件传输发生异常时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileDownloadBuilder SetOnTransferFailed(Action<Exception> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnTransferFailed = configure;

        return this;
    }

    /// <summary>
    ///     设置在文件存在且配置为跳过时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileDownloadBuilder SetOnFileExistAndSkip(Action configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnFileExistAndSkip = configure;

        return this;
    }

    /// <summary>
    ///     设置 HTTP 文件传输事件处理程序
    /// </summary>
    /// <param name="fileTransferEventHandlerType">实现 <see cref="IHttpFileTransferEventHandler" /> 接口的类型</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileDownloadBuilder SetEventHandler(Type fileTransferEventHandlerType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(fileTransferEventHandlerType);

        // 检查类型是否实现了 IHttpFileTransferEventHandler 接口
        if (!typeof(IHttpFileTransferEventHandler).IsAssignableFrom(fileTransferEventHandlerType))
        {
            throw new ArgumentException(
                $"`{fileTransferEventHandlerType}` type is not assignable from `{typeof(IHttpFileTransferEventHandler)}`.",
                nameof(fileTransferEventHandlerType));
        }

        FileTransferEventHandlerType = fileTransferEventHandlerType;

        return this;
    }

    /// <summary>
    ///     设置 HTTP 文件传输事件处理程序
    /// </summary>
    /// <typeparam name="TFileTransferEventHandler">
    ///     <see cref="IHttpFileTransferEventHandler" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public HttpFileDownloadBuilder SetEventHandler<TFileTransferEventHandler>()
        where TFileTransferEventHandler : IHttpFileTransferEventHandler =>
        SetEventHandler(typeof(TFileTransferEventHandler));

    /// <summary>
    ///     配置 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileDownloadBuilder WithRequest(Action<HttpRequestBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        _configureRequest += configure;

        return this;
    }

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public HttpFileDownloadBuilder Profiler() => WithRequest(builder => builder.Profiler(true));

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public HttpFileDownloadBuilder Profiler(bool enabled) => WithRequest(builder => builder.Profiler(enabled));

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <param name="predicate">自定义处理委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public HttpFileDownloadBuilder Profiler(Action<HttpRemoteAnalyzer> predicate) =>
        WithRequest(builder => builder.Profiler(predicate));

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    internal HttpRequestBuilder Build(HttpRemoteOptions httpRemoteOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);

        // 检查是否设置了文件保存的目标路径，如果没有则设置为默认文件下载保存目录
        DestinationPath ??= httpRemoteOptions.DefaultFileDownloadDirectory;

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(DestinationPath);

        // 初始化 HttpRequestBuilder 实例；如果请求失败，则应抛出异常。
        var httpRequestBuilder = HttpRequestBuilder.Create(HttpMethod, RequestUri).PerformanceOptimization()
            .EnsureSuccessStatusCode();

        // 检查是否设置了事件处理程序且该处理程序实现了 IHttpRequestEventHandler 接口，如果有则设置给 httpRequestBuilder
        if (FileTransferEventHandlerType is not null &&
            typeof(IHttpRequestEventHandler).IsAssignableFrom(FileTransferEventHandlerType))
        {
            httpRequestBuilder.SetEventHandler(FileTransferEventHandlerType);
        }

        // 调用自定义配置委托
        _configureRequest?.Invoke(httpRequestBuilder);

        return httpRequestBuilder;
    }
}