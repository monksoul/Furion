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

using Furion.HttpRemote.Extensions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     获取一个用于配置当前 <see cref="HttpRequestBuilder" /> 的实例
    /// </summary>
    /// <remarks>
    ///     用于构建 <![CDATA[Action<HttpRequestBuilder>]]> 委托。
    /// </remarks>
    public static HttpRequestBuilder Setup => new();

    /// <summary>
    ///     支持 <see cref="HttpRequestBuilder" /> 隐式转换为 <![CDATA[Action<HttpRequestBuilder>]]>
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <returns><![CDATA[Action<HttpRequestBuilder>]]></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static implicit operator Action<HttpRequestBuilder>(HttpRequestBuilder httpRequestBuilder)
    {
        // 空检查
        if (httpRequestBuilder.HttpMethod is not null)
        {
            throw new InvalidOperationException(
                $"The '{nameof(Setup)}' builder must not have an HTTP method set. It is intended to be used as a configuration delegate for predicate-style methods (e.g., GetAsync, PostAsync).");
        }

        return target =>
            httpRequestBuilder.CopyTo(target, nameof(RequestUri), nameof(HttpMethod));
    }

    /// <summary>
    ///     创建 <c>GET</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Get(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Get, requestUri, configure);

    /// <summary>
    ///     创建 <c>GET</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Get(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Get, requestUri, configure);

    /// <summary>
    ///     创建 <c>PUT</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Put(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Put, requestUri, configure);

    /// <summary>
    ///     创建 <c>PUT</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Put(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Put, requestUri, configure);

    /// <summary>
    ///     创建 <c>POST</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Post(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Post, requestUri, configure);

    /// <summary>
    ///     创建 <c>POST</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Post(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Post, requestUri, configure);

    /// <summary>
    ///     创建 <c>DELETE</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Delete(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Delete, requestUri, configure);

    /// <summary>
    ///     创建 <c>DELETE</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Delete(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Delete, requestUri, configure);

    /// <summary>
    ///     创建 <c>HEAD</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Head(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Head, requestUri, configure);

    /// <summary>
    ///     创建 <c>HEAD</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Head(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Head, requestUri, configure);

    /// <summary>
    ///     创建 <c>OPTIONS</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Options(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Options, requestUri, configure);

    /// <summary>
    ///     创建 <c>OPTIONS</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Options(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Options, requestUri, configure);

    /// <summary>
    ///     创建 <c>TRACE</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Trace(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Trace, requestUri, configure);

    /// <summary>
    ///     创建 <c>TRACE</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Trace(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Trace, requestUri, configure);

    /// <summary>
    ///     创建 <c>PATCH</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Patch(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Patch, requestUri, configure);

    /// <summary>
    ///     创建 <c>PATCH</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Patch(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(HttpMethod.Patch, requestUri, configure);

    /// <summary>
    ///     创建 <c>QUERY</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Query(string? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(Helpers.HttpQuery, requestUri, configure);

    /// <summary>
    ///     创建 <c>QUERY</c> 请求的 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Query(Uri? requestUri, Action<HttpRequestBuilder>? configure = null) =>
        Create(Helpers.HttpQuery, requestUri, configure);

    /// <summary>
    ///     创建 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Create(HttpMethod httpMethod, Uri? requestUri) => new(httpMethod, requestUri);

    /// <summary>
    ///     创建 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Create(HttpMethod httpMethod, string? requestUri) =>
        Create(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute));

    /// <summary>
    ///     创建 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Create(string httpMethod, string? requestUri)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(httpMethod);

        return Create(Helpers.ParseHttpMethod(httpMethod), requestUri);
    }

    /// <summary>
    ///     创建 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Create(string httpMethod, Uri? requestUri)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(httpMethod);

        return Create(Helpers.ParseHttpMethod(httpMethod), requestUri);
    }

    /// <summary>
    ///     创建 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Create(HttpMethod httpMethod, Uri? requestUri,
        Action<HttpRequestBuilder>? configure)
    {
        // 初始化 HttpRequestBuilder 实例
        var httpRequestBuilder = Create(httpMethod, requestUri);

        // 调用自定义配置委托
        configure?.Invoke(httpRequestBuilder);

        return httpRequestBuilder;
    }

    /// <summary>
    ///     创建 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public static HttpRequestBuilder Create(HttpMethod httpMethod, string? requestUri,
        Action<HttpRequestBuilder>? configure) =>
        Create(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure);

    /// <summary>
    ///     创建 <see cref="HttpRequestBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public static HttpRequestBuilder Create(string httpMethod, string? requestUri,
        Action<HttpRequestBuilder>? configure)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(httpMethod);

        return Create(Helpers.ParseHttpMethod(httpMethod), requestUri, configure);
    }

    /// <summary>
    ///     创建 <see cref="HttpFileDownloadBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public static HttpFileDownloadBuilder DownloadFile(HttpMethod httpMethod, string? requestUri,
        string? destinationPath = null, Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null) =>
        DownloadFile(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute),
            destinationPath, onProgressChanged, fileExistsBehavior, configure);

    /// <summary>
    ///     创建 <see cref="HttpFileDownloadBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public static HttpFileDownloadBuilder DownloadFile(HttpMethod httpMethod, Uri? requestUri,
        string? destinationPath = null, Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null)
    {
        // 初始化 HttpFileDownloadBuilder 实例
        var httpFileDownloadBuilder =
            new HttpFileDownloadBuilder(httpMethod, requestUri).SetDestinationPath(destinationPath)
                .SetFileExistsBehavior(fileExistsBehavior);

        // 空检查
        if (onProgressChanged is not null)
        {
            httpFileDownloadBuilder.SetOnProgressChanged(onProgressChanged);
        }

        // 调用自定义配置委托
        configure?.Invoke(httpFileDownloadBuilder);

        return httpFileDownloadBuilder;
    }

    /// <summary>
    ///     创建 <see cref="HttpFileDownloadBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public static HttpFileDownloadBuilder DownloadFile(Uri? requestUri, string? destinationPath = null,
        Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null) =>
        DownloadFile(HttpMethod.Get, requestUri, destinationPath, onProgressChanged, fileExistsBehavior, configure);

    /// <summary>
    ///     创建 <see cref="HttpFileDownloadBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="destinationPath">文件保存的目标路径</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileExistsBehavior">
    ///     <see cref="FileExistsBehavior" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileDownloadBuilder" />
    /// </returns>
    public static HttpFileDownloadBuilder DownloadFile(string? requestUri, string? destinationPath = null,
        Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null) =>
        DownloadFile(HttpMethod.Get,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute),
            destinationPath, onProgressChanged, fileExistsBehavior, configure);

    /// <summary>
    ///     创建 <see cref="HttpFileUploadBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称；默认值为 <c>file</c>。</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    public static HttpFileUploadBuilder UploadFile(HttpMethod httpMethod, string? requestUri, string filePath,
        string name = "file", Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null) =>
        UploadFile(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), filePath,
            name, onProgressChanged, fileName, configure);

    /// <summary>
    ///     创建 <see cref="HttpFileUploadBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称；默认值为 <c>file</c>。</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    public static HttpFileUploadBuilder UploadFile(HttpMethod httpMethod, Uri? requestUri, string filePath,
        string name = "file", Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null)
    {
        // 初始化 HttpFileUploadBuilder 实例
        var httpFileUploadBuilder = new HttpFileUploadBuilder(httpMethod, requestUri, filePath, name, fileName);

        // 空检查
        if (onProgressChanged is not null)
        {
            httpFileUploadBuilder.SetOnProgressChanged(onProgressChanged);
        }

        // 调用自定义配置委托
        configure?.Invoke(httpFileUploadBuilder);

        return httpFileUploadBuilder;
    }

    /// <summary>
    ///     创建 <see cref="HttpFileUploadBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称；默认值为 <c>file</c>。</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    public static HttpFileUploadBuilder UploadFile(Uri? requestUri, string filePath, string name = "file",
        Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null) =>
        UploadFile(HttpMethod.Post, requestUri, filePath, name, onProgressChanged, fileName, configure);

    /// <summary>
    ///     创建 <see cref="HttpFileUploadBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称；默认值为 <c>file</c>。</param>
    /// <param name="onProgressChanged">用于传输进度发生变化时执行的委托</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpFileUploadBuilder" />
    /// </returns>
    public static HttpFileUploadBuilder UploadFile(string? requestUri, string filePath, string name = "file",
        Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null) =>
        UploadFile(HttpMethod.Post,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), filePath,
            name, onProgressChanged, fileName, configure);

    /// <summary>
    ///     创建 <see cref="HttpServerSentEventsBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public static HttpServerSentEventsBuilder ServerSentEvents(Uri? requestUri,
        Action<HttpServerSentEventsBuilder>? configure = null) =>
        ServerSentEvents(HttpMethod.Get, requestUri, configure);

    /// <summary>
    ///     创建 <see cref="HttpServerSentEventsBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public static HttpServerSentEventsBuilder ServerSentEvents(string? requestUri,
        Action<HttpServerSentEventsBuilder>? configure = null) =>
        ServerSentEvents(string.IsNullOrWhiteSpace(requestUri)
            ? null
            : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure);

    /// <summary>
    ///     创建 <see cref="HttpServerSentEventsBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onMessage">用于在从事件源接收到数据时的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public static HttpServerSentEventsBuilder ServerSentEvents(Uri? requestUri,
        Func<ServerSentEventsData, CancellationToken, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null) =>
        ServerSentEvents(HttpMethod.Get, requestUri, onMessage, configure);

    /// <summary>
    ///     创建 <see cref="HttpServerSentEventsBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onMessage">用于在从事件源接收到数据时的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public static HttpServerSentEventsBuilder ServerSentEvents(string? requestUri,
        Func<ServerSentEventsData, CancellationToken, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null) =>
        ServerSentEvents(string.IsNullOrWhiteSpace(requestUri)
            ? null
            : new Uri(requestUri, UriKind.RelativeOrAbsolute), onMessage, configure);

    /// <summary>
    ///     创建 <see cref="HttpServerSentEventsBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public static HttpServerSentEventsBuilder ServerSentEvents(HttpMethod httpMethod, string? requestUri,
        Action<HttpServerSentEventsBuilder>? configure = null) =>
        ServerSentEvents(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure);

    /// <summary>
    ///     创建 <see cref="HttpServerSentEventsBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onMessage">用于在从事件源接收到数据时的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public static HttpServerSentEventsBuilder ServerSentEvents(HttpMethod httpMethod, string? requestUri,
        Func<ServerSentEventsData, CancellationToken, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null) =>
        ServerSentEvents(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), onMessage,
            configure);

    /// <summary>
    ///     创建 <see cref="HttpServerSentEventsBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public static HttpServerSentEventsBuilder ServerSentEvents(HttpMethod httpMethod, Uri? requestUri,
        Action<HttpServerSentEventsBuilder>? configure = null)
    {
        // 初始化 HttpServerSentEventsBuilder 实例
        var httpServerSentEventsBuilder = new HttpServerSentEventsBuilder(httpMethod, requestUri);

        // 调用自定义配置委托
        configure?.Invoke(httpServerSentEventsBuilder);

        return httpServerSentEventsBuilder;
    }

    /// <summary>
    ///     创建 <see cref="HttpServerSentEventsBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onMessage">用于在从事件源接收到数据时的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpServerSentEventsBuilder" />
    /// </returns>
    public static HttpServerSentEventsBuilder ServerSentEvents(HttpMethod httpMethod, Uri? requestUri,
        Func<ServerSentEventsData, CancellationToken, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null)
    {
        // 初始化 HttpServerSentEventsBuilder 实例
        var httpServerSentEventsBuilder =
            new HttpServerSentEventsBuilder(httpMethod, requestUri).SetOnMessage(onMessage);

        // 调用自定义配置委托
        configure?.Invoke(httpServerSentEventsBuilder);

        return httpServerSentEventsBuilder;
    }

    /// <summary>
    ///     创建 <see cref="HttpStressTestHarnessBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="numberOfRequests">并发请求数量，默认值为：100。</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    public static HttpStressTestHarnessBuilder StressTestHarness(HttpMethod httpMethod, string? requestUri,
        int numberOfRequests = 100, Action<HttpStressTestHarnessBuilder>? configure = null) =>
        StressTestHarness(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute),
            numberOfRequests, configure);

    /// <summary>
    ///     创建 <see cref="HttpStressTestHarnessBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="numberOfRequests">并发请求数量，默认值为：100。</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    public static HttpStressTestHarnessBuilder StressTestHarness(HttpMethod httpMethod, Uri? requestUri,
        int numberOfRequests = 100, Action<HttpStressTestHarnessBuilder>? configure = null)
    {
        // 初始化 HttpStressTestHarnessBuilder 实例
        var httpStressTestHarnessBuilder =
            new HttpStressTestHarnessBuilder(httpMethod, requestUri).SetNumberOfRequests(numberOfRequests);

        // 调用自定义配置委托
        configure?.Invoke(httpStressTestHarnessBuilder);

        return httpStressTestHarnessBuilder;
    }

    /// <summary>
    ///     创建 <see cref="HttpStressTestHarnessBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="numberOfRequests">并发请求数量，默认值为：100。</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    public static HttpStressTestHarnessBuilder StressTestHarness(Uri? requestUri, int numberOfRequests = 100,
        Action<HttpStressTestHarnessBuilder>? configure = null) =>
        StressTestHarness(HttpMethod.Get, requestUri, numberOfRequests, configure);

    /// <summary>
    ///     创建 <see cref="HttpStressTestHarnessBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="numberOfRequests">并发请求数量，默认值为：100。</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    public static HttpStressTestHarnessBuilder StressTestHarness(string? requestUri, int numberOfRequests = 100,
        Action<HttpStressTestHarnessBuilder>? configure = null) =>
        StressTestHarness(HttpMethod.Get,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute),
            numberOfRequests, configure);

    /// <summary>
    ///     创建 <see cref="HttpLongPollingBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public static HttpLongPollingBuilder LongPolling(Uri? requestUri,
        Action<HttpLongPollingBuilder>? configure = null) =>
        LongPolling(HttpMethod.Get, requestUri, configure);

    /// <summary>
    ///     创建 <see cref="HttpLongPollingBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public static HttpLongPollingBuilder LongPolling(string? requestUri,
        Action<HttpLongPollingBuilder>? configure = null) =>
        LongPolling(HttpMethod.Get,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure);

    /// <summary>
    ///     创建 <see cref="HttpLongPollingBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onDataReceived">用于接收服务器返回 <c>200~299</c> 状态码的数据的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public static HttpLongPollingBuilder LongPolling(Uri? requestUri,
        Func<HttpResponseMessage, CancellationToken, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null) =>
        LongPolling(HttpMethod.Get, requestUri, onDataReceived, configure);

    /// <summary>
    ///     创建 <see cref="HttpLongPollingBuilder" /> 构建器
    /// </summary>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onDataReceived">用于接收服务器返回 <c>200~299</c> 状态码的数据的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public static HttpLongPollingBuilder LongPolling(string? requestUri,
        Func<HttpResponseMessage, CancellationToken, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null) =>
        LongPolling(HttpMethod.Get,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute),
            onDataReceived, configure);

    /// <summary>
    ///     创建 <see cref="HttpLongPollingBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public static HttpLongPollingBuilder LongPolling(HttpMethod httpMethod, string? requestUri,
        Action<HttpLongPollingBuilder>? configure = null) =>
        LongPolling(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), configure);

    /// <summary>
    ///     创建 <see cref="HttpLongPollingBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onDataReceived">用于接收服务器返回 <c>200~299</c> 状态码的数据的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public static HttpLongPollingBuilder LongPolling(HttpMethod httpMethod, string? requestUri,
        Func<HttpResponseMessage, CancellationToken, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null) =>
        LongPolling(httpMethod,
            string.IsNullOrWhiteSpace(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute),
            onDataReceived, configure);

    /// <summary>
    ///     创建 <see cref="HttpLongPollingBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public static HttpLongPollingBuilder LongPolling(HttpMethod httpMethod, Uri? requestUri,
        Action<HttpLongPollingBuilder>? configure = null)
    {
        // 初始化 HttpLongPollingBuilder 实例
        var httpLongPollingBuilder = new HttpLongPollingBuilder(httpMethod, requestUri);

        // 调用自定义配置委托
        configure?.Invoke(httpLongPollingBuilder);

        return httpLongPollingBuilder;
    }

    /// <summary>
    ///     创建 <see cref="HttpLongPollingBuilder" /> 构建器
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <param name="onDataReceived">用于接收服务器返回 <c>200~299</c> 状态码的数据的操作</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpLongPollingBuilder" />
    /// </returns>
    public static HttpLongPollingBuilder LongPolling(HttpMethod httpMethod, Uri? requestUri,
        Func<HttpResponseMessage, CancellationToken, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null)
    {
        // 初始化 HttpLongPollingBuilder 实例
        var httpLongPollingBuilder =
            new HttpLongPollingBuilder(httpMethod, requestUri).SetOnDataReceived(onDataReceived);

        // 调用自定义配置委托
        configure?.Invoke(httpLongPollingBuilder);

        return httpLongPollingBuilder;
    }

    /// <summary>
    ///     创建 <see cref="HttpDeclarativeBuilder" /> 构建器
    /// </summary>
    /// <param name="method">被调用方法</param>
    /// <param name="args">被调用方法的参数值数组</param>
    /// <param name="interfaceType">实际被代理的接口类型</param>
    /// <returns>
    ///     <see cref="HttpDeclarativeBuilder" />
    /// </returns>
    public static HttpDeclarativeBuilder Declarative(MethodInfo method, object?[] args, Type? interfaceType = null) =>
        new(method, args, interfaceType);

    /// <summary>
    ///     从 cURL 命令字符串中创建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="curlCommand">cURL 命令字符串</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static HttpRequestBuilder FromCurl(string curlCommand, Action<HttpCurlParsingOptions>? configure = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(curlCommand);

        // 初始化 HttpCurlParsingOptions 实例
        var options = new HttpCurlParsingOptions();

        // 调用自定义配置委托
        configure?.Invoke(options);

        // 将 cURL 命令字符串拆分为 Token 集合
        var tokens = HttpCurlTokenizer.Tokenize(curlCommand);

        // 检查是否以 "curl" 开头
        if (tokens.Count == 0 || !string.Equals(tokens[0], "curl", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The cURL command must start with 'curl'.");
        }

        // 移除开头的 "curl" 关键字
        tokens.RemoveAt(0);

        // 检查是否只有 "curl" 关键字
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("The cURL command is empty or contains no valid tokens.");
        }

        // 初始化 HttpRequestBuilder 实例
        var httpRequestBuilder = new HttpRequestBuilder();

        // 初始化 HttpCurlParsingContext 实例
        var parsingContext = new HttpCurlParsingContext(tokens);

        // 先将提取器进行排序
        var sortedExtractors = options.Extractors.OrderBy(e => (e as IOrderedHttpCurlExtractor)?.Order ?? 0).ToList();

        // 遍历所有的 Token
        while (!parsingContext.IsEndOfTokens)
        {
            // 尝试匹配
            if (!sortedExtractors.Any(extractor => extractor.TryExtract(httpRequestBuilder, parsingContext)))
            {
                // 推进游标
                parsingContext.Advance();
            }
        }

        // 检查是否未显式指定 HTTP 方法
        // ReSharper disable once InvertIf
        if (httpRequestBuilder.HttpMethod is null)
        {
            // 判断是否包含请求内容
            var hasBody = httpRequestBuilder.RawContent is not null ||
                          httpRequestBuilder.MultipartFormDataBuilder is not null;

            // 设置请求方式
            httpRequestBuilder.SetHttpMethod(hasBody ? HttpMethod.Post : HttpMethod.Get);
        }

        // 将原始 cURL 命令存入请求消息属性中，供请求分析工具输出
        httpRequestBuilder.WithProperty(Constants.CURL_COMMAND_KEY, curlCommand);

        return httpRequestBuilder;
    }

    /// <summary>
    ///     从 JSON 中创建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static HttpRequestBuilder FromJson(string json, Action<HttpJsonParsingOptions>? configure = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        // 手动解析 JSON 字符串
        var jsonNode = JsonNode.Parse(json, new JsonNodeOptions { PropertyNameCaseInsensitive = true },
            new JsonDocumentOptions { AllowTrailingCommas = true });

        // 检查解析结果是否为 JSON 对象
        if (jsonNode is not JsonObject jsonObject)
        {
            throw new ArgumentException("The provided JSON must be a valid JSON object.", nameof(json));
        }

        // 空检查
        ArgumentNullException.ThrowIfNull(jsonObject);

        // 初始化 HttpJsonParsingOptions 实例
        var options = new HttpJsonParsingOptions();

        // 调用自定义配置委托
        configure?.Invoke(options);

        // 初始化 HttpRequestBuilder 实例
        var httpRequestBuilder = new HttpRequestBuilder();

        // 初始化 HttpJsonParsingContext 实例
        var parsingContext = new HttpJsonParsingContext(jsonObject);

        // 遍历所有的提取器
        foreach (var extractor in options.Extractors)
        {
            extractor.Extract(httpRequestBuilder, parsingContext);
        }

        // 检查是否未显式指定 HTTP 方法
        // ReSharper disable once InvertIf
        if (httpRequestBuilder.HttpMethod is null)
        {
            // 判断是否包含请求内容
            var hasBody = httpRequestBuilder.RawContent is not null ||
                          httpRequestBuilder.MultipartFormDataBuilder is not null;

            // 设置请求方式
            httpRequestBuilder.SetHttpMethod(hasBody ? HttpMethod.Post : HttpMethod.Get);
        }

        // 将原始 JSON 字符串存入请求消息属性中，供请求分析工具输出
        httpRequestBuilder.WithProperty(Constants.JSON_COMMAND_KEY, json);

        return httpRequestBuilder;
    }
}