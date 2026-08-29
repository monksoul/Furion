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
using System.Net.Http.Headers;
using System.Threading.Channels;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 文件上传构建器
/// </summary>
/// <remarks>使用 <c>HttpRequestBuilder.UploadFile(requestUri, filePath, name)</c> 静态方法创建。</remarks>
public sealed class HttpFileUploadBuilder : HttpRequestBuilderConfigurator<HttpFileUploadBuilder>
{
    /// <summary>
    ///     <inheritdoc cref="HttpFileUploadBuilder" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    internal HttpFileUploadBuilder(HttpMethod httpMethod, Uri? requestUri, string filePath, string name,
        string? fileName = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        HttpMethod = httpMethod;
        RequestUri = requestUri;

        FilePath = filePath;
        Name = name;
        FileName = fileName;
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
    ///     文件路径
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    ///     文件的名称
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    ///     表单名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     内容类型
    /// </summary>
    public string? ContentType { get; private set; }

    /// <summary>
    ///     允许的文件扩展名
    /// </summary>
    public string[]? AllowedFileExtensions { get; private set; }

    /// <summary>
    ///     允许的文件大小。以字节为单位
    /// </summary>
    public long? MaxFileSizeInBytes { get; private set; }

    /// <summary>
    ///     进度更新（通知）的间隔时间
    /// </summary>
    /// <remarks>默认值为 250 毫秒。</remarks>
    public TimeSpan ProgressInterval { get; private set; } = TimeSpan.FromMilliseconds(250);

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
    ///     用于传输进度发生变化时的操作
    /// </summary>
    public Func<FileTransferProgress, Task>? OnProgressChanged { get; private set; }

    /// <summary>
    ///     实现 <see cref="IHttpFileTransferEventHandler" /> 的类型
    /// </summary>
    internal Type? FileTransferEventHandlerType { get; private set; }

    /// <summary>
    ///     设置内容类型（文件类型）
    /// </summary>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileUploadBuilder SetContentType(string contentType)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        // 解析内容类型字符串
        var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);

        ContentType = mediaTypeHeaderValue.MediaType;

        return this;
    }

    /// <summary>
    ///     设置允许的文件扩展名
    /// </summary>
    /// <param name="allowedFileExtensions">允许的文件扩展名</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileUploadBuilder SetAllowedFileExtensions(string[] allowedFileExtensions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(allowedFileExtensions);

        AllowedFileExtensions = allowedFileExtensions;

        return this;
    }

    /// <summary>
    ///     设置允许的文件扩展名
    /// </summary>
    /// <param name="allowedFileExtensions">允许的文件扩展名字符串，用分号分隔</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileUploadBuilder SetAllowedFileExtensions(string allowedFileExtensions)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(allowedFileExtensions);

        AllowedFileExtensions = allowedFileExtensions.Split(';', StringSplitOptions.RemoveEmptyEntries);

        return this;
    }

    /// <summary>
    ///     设置允许的文件大小
    /// </summary>
    /// <param name="maxFileSizeInBytes">允许的文件大小。以字节为单位</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public HttpFileUploadBuilder SetMaxFileSizeInBytes(long maxFileSizeInBytes)
    {
        // 小于或等于 0 检查
        if (maxFileSizeInBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFileSizeInBytes),
                "Max file size in bytes must be greater than zero.");
        }

        MaxFileSizeInBytes = maxFileSizeInBytes;

        return this;
    }

    /// <summary>
    ///     设置文件传输进度（通知）的间隔时间
    /// </summary>
    /// <param name="progressInterval">进度更新（通知）的间隔时间</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileUploadBuilder SetProgressInterval(TimeSpan progressInterval)
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
    ///     设置在文件开始传输时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileUploadBuilder SetOnTransferStarted(Action configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnTransferStarted = configure;

        return this;
    }

    /// <summary>
    ///     设置用于上传进度发生变化时执行的委托
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileUploadBuilder SetOnProgressChanged(Func<FileTransferProgress, Task> configure)
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
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileUploadBuilder SetOnTransferCompleted(Action<long> configure)
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
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpFileUploadBuilder SetOnTransferFailed(Action<Exception> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnTransferFailed = configure;

        return this;
    }

    /// <summary>
    ///     设置 HTTP 文件传输事件处理程序
    /// </summary>
    /// <param name="fileTransferEventHandlerType">实现 <see cref="IHttpFileTransferEventHandler" /> 接口的类型</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public HttpFileUploadBuilder SetEventHandler(Type fileTransferEventHandlerType)
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
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    public HttpFileUploadBuilder SetEventHandler<TFileTransferEventHandler>()
        where TFileTransferEventHandler : IHttpFileTransferEventHandler =>
        SetEventHandler(typeof(TFileTransferEventHandler));

    /// <summary>
    ///     追加多部分表单内容
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    public HttpFileUploadBuilder WithMultipart(Action<HttpMultipartFormDataBuilder> configure) =>
        With(builder => builder.WithMultipart(configure));

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="progressChannel">文件传输进度信息的通道</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpRequestBuilder Build(HttpRemoteOptions httpRemoteOptions,
        Channel<FileTransferProgress> progressChannel)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);
        ArgumentNullException.ThrowIfNull(progressChannel);

        // 检查文件扩展名和大小合法性
        EnsureLegalData(FilePath, AllowedFileExtensions, MaxFileSizeInBytes);

        // 初始化 HttpRequestBuilder 实例，如果请求失败，则应抛出异常
        var httpRequestBuilder = HttpRequestBuilder.Create(HttpMethod, RequestUri).SetMultipartContent(builder =>
        {
            builder.AddFileWithProgressAsStream(FilePath, progressChannel, Name, FileName, ContentType,
                progressInterval: ProgressInterval);
        }).EnsureSuccessStatusCode();

        // 检查是否设置了事件处理程序且该处理程序实现了 IHttpRequestEventHandler 接口，如果有则设置给 httpRequestBuilder
        if (FileTransferEventHandlerType is not null &&
            typeof(IHttpRequestEventHandler).IsAssignableFrom(FileTransferEventHandlerType))
        {
            httpRequestBuilder.SetEventHandler(FileTransferEventHandlerType);
        }

        // 调用自定义配置委托
        Configure?.Invoke(httpRequestBuilder);

        return httpRequestBuilder;
    }

    /// <summary>
    ///     检查文件扩展名和大小合法性
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="allowedFileExtensions">允许的文件扩展名</param>
    /// <param name="maxFileSizeInBytes">允许的文件大小。以字节为单位</param>
    /// <exception cref="ArgumentException"></exception>
    internal static void EnsureLegalData(string filePath, string[]? allowedFileExtensions, long? maxFileSizeInBytes)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        // 空检查
        if (allowedFileExtensions is { Length: > 0 })
        {
            FileUtility.ValidateExtension(filePath, allowedFileExtensions);
        }

        // 空检查
        if (maxFileSizeInBytes is not null)
        {
            FileUtility.ValidateSize(filePath, maxFileSizeInBytes.Value);
        }
    }
}