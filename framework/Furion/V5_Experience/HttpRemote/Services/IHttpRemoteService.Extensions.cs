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

using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求服务
/// </summary>
public partial interface IHttpRemoteService
{
    /// <summary>
    ///     下载文件
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="FileTransferResult" />
    /// </returns>
    FileTransferResult DownloadFile(string? requestUri, string? destinationPath = null,
        Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     下载文件
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="FileTransferResult" />
    /// </returns>
    Task<FileTransferResult> DownloadFileAsync(string? requestUri, string? destinationPath = null,
        Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     下载文件（带控制台进度打印）
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="FileTransferResult" />
    /// </returns>
    FileTransferResult DownloadFileWithConsoleProgress(string? requestUri, string? destinationPath = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     下载文件（带控制台进度打印）
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="FileTransferResult" />
    /// </returns>
    Task<FileTransferResult> DownloadFileWithConsoleProgressAsync(string? requestUri, string? destinationPath = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     下载文件
    /// </summary>
    /// <param name="httpFileDownloadBuilder">
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="FileTransferResult" />
    /// </returns>
    FileTransferResult Send(HttpFileDownloadBuilder httpFileDownloadBuilder,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     下载文件
    /// </summary>
    /// <param name="httpFileDownloadBuilder">
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="FileTransferResult" />
    /// </returns>
    Task<FileTransferResult> SendAsync(HttpFileDownloadBuilder httpFileDownloadBuilder,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     上传文件
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称；默认值为 <c>file</c>。</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? UploadFile(string? requestUri, string filePath, string name = "file",
        Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     上传文件
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称；默认值为 <c>file</c>。</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task<HttpResponseMessage?> UploadFileAsync(string? requestUri, string filePath, string name = "file",
        Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     上传文件（带控制台进度打印）
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称；默认值为 <c>file</c>。</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? UploadFileWithConsoleProgress(string? requestUri, string filePath, string name = "file",
        string? fileName = null, Action<HttpFileUploadBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     上传文件（带控制台进度打印）
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称；默认值为 <c>file</c>。</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task<HttpResponseMessage?> UploadFileWithConsoleProgressAsync(string? requestUri, string filePath,
        string name = "file", string? fileName = null, Action<HttpFileUploadBuilder>? configure = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     上传文件
    /// </summary>
    /// <param name="httpFileUploadBuilder">
    ///     <see cref="HttpFileUploadBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    HttpResponseMessage? Send(HttpFileUploadBuilder httpFileUploadBuilder,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     上传文件
    /// </summary>
    /// <param name="httpFileUploadBuilder">
    ///     <see cref="HttpFileUploadBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task<HttpResponseMessage?> SendAsync(HttpFileUploadBuilder httpFileUploadBuilder,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 Server-Sent Events 请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onMessage">用于在从事件源接收到数据时的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    void ServerSentEvents(string? requestUri, Func<ServerSentEventsData, CancellationToken, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 Server-Sent Events 请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onMessage">用于在从事件源接收到数据时的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    Task ServerSentEventsAsync(string? requestUri, Func<ServerSentEventsData, CancellationToken, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 Server-Sent Events 请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{T}" />
    /// </returns>
    IAsyncEnumerable<ServerSentEventsData> ServerSentEventsAsAsyncEnumerable(string? requestUri,
        Action<HttpServerSentEventsBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 Server-Sent Events 请求
    /// </summary>
    /// <param name="httpServerSentEventsBuilder">
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    void Send(HttpServerSentEventsBuilder httpServerSentEventsBuilder, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 Server-Sent Events 请求
    /// </summary>
    /// <param name="httpServerSentEventsBuilder">
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    Task SendAsync(HttpServerSentEventsBuilder httpServerSentEventsBuilder,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 Server-Sent Events 请求
    /// </summary>
    /// <param name="httpServerSentEventsBuilder">
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{T}" />
    /// </returns>
    IAsyncEnumerable<ServerSentEventsData> SendAsAsyncEnumerable(
        HttpServerSentEventsBuilder httpServerSentEventsBuilder,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     压力测试
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="numberOfRequests">并发请求数量，默认值为：100。</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="StressTestHarnessResult" />
    /// </returns>
    StressTestHarnessResult StressTestHarness(string? requestUri, int numberOfRequests = 100,
        Action<HttpStressTestHarnessBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     压力测试
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="numberOfRequests">并发请求数量，默认值为：100。</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task<StressTestHarnessResult> StressTestHarnessAsync(string? requestUri, int numberOfRequests = 100,
        Action<HttpStressTestHarnessBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     压力测试
    /// </summary>
    /// <param name="httpStressTestHarnessBuilder">
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="StressTestHarnessResult" />
    /// </returns>
    StressTestHarnessResult Send(HttpStressTestHarnessBuilder httpStressTestHarnessBuilder,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     压力测试
    /// </summary>
    /// <param name="httpStressTestHarnessBuilder">
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task<StressTestHarnessResult> SendAsync(HttpStressTestHarnessBuilder httpStressTestHarnessBuilder,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送长轮询请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onDataReceived">用于接收服务器返回 <c>200~299</c> 状态码的数据的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    void LongPolling(string? requestUri, Func<HttpResponseMessage, CancellationToken, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送长轮询请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onDataReceived">用于接收服务器返回 <c>200~299</c> 状态码的数据的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    Task LongPollingAsync(string? requestUri, Func<HttpResponseMessage, CancellationToken, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送长轮询请求
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{T}" />
    /// </returns>
    IAsyncEnumerable<HttpResponseMessage> LongPollingAsAsyncEnumerable(string? requestUri,
        Action<HttpLongPollingBuilder>? configure = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送长轮询请求
    /// </summary>
    /// <param name="httpLongPollingBuilder">
    ///     <see cref="HttpLongPollingBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    void Send(HttpLongPollingBuilder httpLongPollingBuilder, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送长轮询请求
    /// </summary>
    /// <param name="httpLongPollingBuilder">
    ///     <see cref="HttpLongPollingBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    Task SendAsync(HttpLongPollingBuilder httpLongPollingBuilder, CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送长轮询请求
    /// </summary>
    /// <param name="httpLongPollingBuilder">
    ///     <see cref="HttpLongPollingBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{T}" />
    /// </returns>
    IAsyncEnumerable<HttpResponseMessage> SendAsAsyncEnumerable(HttpLongPollingBuilder httpLongPollingBuilder,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发送 HTTP 声明式请求
    /// </summary>
    /// <remarks>仅支持同步方法。</remarks>
    /// <param name="method">被调用方法</param>
    /// <param name="args">被调用方法的参数值数组</param>
    /// <param name="interfaceType">实际被代理的接口类型</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    object? Declarative(MethodInfo method, object[] args, Type? interfaceType = null);

    /// <summary>
    ///     发送 HTTP 声明式请求
    /// </summary>
    /// <remarks>仅支持异步方法。若无返回值则泛型传入 <see cref="VoidContent" /> 类型。</remarks>
    /// <param name="method">被调用方法</param>
    /// <param name="args">被调用方法的参数值数组</param>
    /// <param name="interfaceType">实际被代理的接口类型</param>
    /// <typeparam name="T">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    Task<T?> DeclarativeAsync<T>(MethodInfo method, object[] args, Type? interfaceType = null);

    /// <summary>
    ///     发送 HTTP 声明式请求
    /// </summary>
    /// <remarks>仅支持同步方法。</remarks>
    /// <param name="httpDeclarativeBuilder">
    ///     <see cref="HttpDeclarativeBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    object? SendAs(HttpDeclarativeBuilder httpDeclarativeBuilder);

    /// <summary>
    ///     发送 HTTP 声明式请求
    /// </summary>
    /// <remarks>仅支持异步方法。若无返回值则泛型传入 <see cref="VoidContent" /> 类型。</remarks>
    /// <param name="httpDeclarativeBuilder">
    ///     <see cref="HttpDeclarativeBuilder" />
    /// </param>
    /// <typeparam name="T">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    Task<T?> SendAsAsync<T>(HttpDeclarativeBuilder httpDeclarativeBuilder);

    /// <summary>
    ///     解析服务
    /// </summary>
    /// <typeparam name="T">服务的类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    T For<T>() where T : notnull;

    /// <summary>
    ///     解析服务
    /// </summary>
    /// <param name="required">要求服务必须注册，默认值为：<c>true</c></param>
    /// <typeparam name="T">服务的类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    T? For<T>(bool required) where T : notnull;

    /// <summary>
    ///     解析服务
    /// </summary>
    /// <param name="serviceType">服务的类型</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    object? For(Type serviceType);
}