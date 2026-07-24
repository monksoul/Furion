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
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     <inheritdoc cref="IHttpRemoteService" />
/// </summary>
internal sealed partial class HttpRemoteService
{
    /// <inheritdoc />
    public FileTransferResult DownloadFile(string? requestUri, string? destinationPath,
        Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        Send(
            HttpRequestBuilder.DownloadFile(requestUri, destinationPath, onProgressChanged, fileExistsBehavior,
                configure), cancellationToken);

    /// <inheritdoc />
    public Task<FileTransferResult> DownloadFileAsync(string? requestUri, string? destinationPath,
        Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync(
            HttpRequestBuilder.DownloadFile(requestUri, destinationPath, onProgressChanged, fileExistsBehavior,
                configure), cancellationToken);

    /// <inheritdoc />
    public FileTransferResult Send(HttpFileDownloadBuilder httpFileDownloadBuilder,
        CancellationToken cancellationToken = default) =>
        new FileDownloadManager(this, httpFileDownloadBuilder).Start(cancellationToken);

    /// <inheritdoc />
    public Task<FileTransferResult> SendAsync(HttpFileDownloadBuilder httpFileDownloadBuilder,
        CancellationToken cancellationToken = default) =>
        new FileDownloadManager(this, httpFileDownloadBuilder).StartAsync(cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage? UploadFile(string? requestUri, string filePath, string name = "file",
        Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        Send(HttpRequestBuilder.UploadFile(requestUri, filePath, name, onProgressChanged, fileName, configure),
            cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage?> UploadFileAsync(string? requestUri, string filePath, string name = "file",
        Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync(HttpRequestBuilder.UploadFile(requestUri, filePath, name, onProgressChanged, fileName, configure),
            cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage? Send(HttpFileUploadBuilder httpFileUploadBuilder,
        CancellationToken cancellationToken = default) =>
        new FileUploadManager(this, httpFileUploadBuilder).Start(cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage?> SendAsync(HttpFileUploadBuilder httpFileUploadBuilder,
        CancellationToken cancellationToken = default) =>
        new FileUploadManager(this, httpFileUploadBuilder).StartAsync(cancellationToken);

    /// <inheritdoc />
    public void ServerSentEvents(string? requestUri, Func<ServerSentEventsData, CancellationToken, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        Send(HttpRequestBuilder.ServerSentEvents(requestUri, onMessage, configure), cancellationToken);

    /// <inheritdoc />
    public Task ServerSentEventsAsync(string? requestUri, Func<ServerSentEventsData, CancellationToken, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync(HttpRequestBuilder.ServerSentEvents(requestUri, onMessage, configure), cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<ServerSentEventsData> ServerSentEventsAsAsyncEnumerable(string? requestUri,
        Action<HttpServerSentEventsBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        SendAsAsyncEnumerable(HttpRequestBuilder.ServerSentEvents(requestUri, configure), cancellationToken);

    /// <inheritdoc />
    public void Send(HttpServerSentEventsBuilder httpServerSentEventsBuilder,
        CancellationToken cancellationToken = default) =>
        new ServerSentEventsManager(this, httpServerSentEventsBuilder).Start(cancellationToken);

    /// <inheritdoc />
    public Task SendAsync(HttpServerSentEventsBuilder httpServerSentEventsBuilder,
        CancellationToken cancellationToken = default) =>
        new ServerSentEventsManager(this, httpServerSentEventsBuilder).StartAsync(cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<ServerSentEventsData> SendAsAsyncEnumerable(
        HttpServerSentEventsBuilder httpServerSentEventsBuilder, CancellationToken cancellationToken = default) =>
        new ServerSentEventsManager(this, httpServerSentEventsBuilder).StartAsAsyncEnumerable(cancellationToken);

    /// <inheritdoc />
    public StressTestHarnessResult StressTestHarness(string? requestUri, int numberOfRequests = 100,
        Action<HttpStressTestHarnessBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) =>
        Send(HttpRequestBuilder.StressTestHarness(requestUri, numberOfRequests, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public Task<StressTestHarnessResult> StressTestHarnessAsync(string? requestUri, int numberOfRequests = 100,
        Action<HttpStressTestHarnessBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpRequestBuilder.StressTestHarness(requestUri, numberOfRequests, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public StressTestHarnessResult Send(HttpStressTestHarnessBuilder httpStressTestHarnessBuilder,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) =>
        new StressTestHarnessManager(this, httpStressTestHarnessBuilder).Start(completionOption,
            cancellationToken);

    /// <inheritdoc />
    public Task<StressTestHarnessResult> SendAsync(HttpStressTestHarnessBuilder httpStressTestHarnessBuilder,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) =>
        new StressTestHarnessManager(this, httpStressTestHarnessBuilder).StartAsync(completionOption,
            cancellationToken);

    /// <inheritdoc />
    public void LongPolling(string? requestUri, Func<HttpResponseMessage, CancellationToken, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        Send(HttpRequestBuilder.LongPolling(requestUri, onDataReceived, configure), cancellationToken);

    /// <inheritdoc />
    public Task LongPollingAsync(string? requestUri, Func<HttpResponseMessage, CancellationToken, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync(HttpRequestBuilder.LongPolling(requestUri, onDataReceived, configure), cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<HttpResponseMessage> LongPollingAsAsyncEnumerable(string? requestUri,
        Action<HttpLongPollingBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        SendAsAsyncEnumerable(HttpRequestBuilder.LongPolling(requestUri, configure), cancellationToken);

    /// <inheritdoc />
    public void Send(HttpLongPollingBuilder httpLongPollingBuilder, CancellationToken cancellationToken = default) =>
        new LongPollingManager(this, httpLongPollingBuilder).Start(cancellationToken);

    /// <inheritdoc />
    public Task SendAsync(HttpLongPollingBuilder httpLongPollingBuilder,
        CancellationToken cancellationToken = default) =>
        new LongPollingManager(this, httpLongPollingBuilder).StartAsync(cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<HttpResponseMessage> SendAsAsyncEnumerable(HttpLongPollingBuilder httpLongPollingBuilder,
        CancellationToken cancellationToken = default) =>
        new LongPollingManager(this, httpLongPollingBuilder).StartAsAsyncEnumerable(cancellationToken);

    /// <inheritdoc />
    public object? Declarative(MethodInfo method, object[] args, Type? interfaceType = null) =>
        SendAs(HttpRequestBuilder.Declarative(method, args, interfaceType));

    /// <inheritdoc />
    public Task<T?> DeclarativeAsync<T>(MethodInfo method, object[] args, Type? interfaceType = null) =>
        SendAsAsync<T>(HttpRequestBuilder.Declarative(method, args, interfaceType));

    /// <inheritdoc />
    public object? SendAs(HttpDeclarativeBuilder httpDeclarativeBuilder) =>
        new DeclarativeManager(this, httpDeclarativeBuilder).Start();

    /// <inheritdoc />
    public Task<T?> SendAsAsync<T>(HttpDeclarativeBuilder httpDeclarativeBuilder) =>
        new DeclarativeManager(this, httpDeclarativeBuilder).StartAsync<T>();

    /// <inheritdoc />
    public T For<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();
}