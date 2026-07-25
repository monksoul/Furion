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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Furion.HttpRemote;

/// <summary>
///     构建 <see cref="HttpRequestMessage" /> 管道处理器
/// </summary>
/// <param name="serviceProvider">
///     <see cref="IServiceProvider" />
/// </param>
/// <param name="httpContentProcessorFactory">
///     <see cref="IHttpContentProcessorFactory" />
/// </param>
/// <param name="httpRemoteOptions">
///     <see cref="HttpRemoteOptions" />
/// </param>
internal sealed class RequestBuilderPipelineHandler(
    IServiceProvider serviceProvider,
    IHttpContentProcessorFactory httpContentProcessorFactory,
    IOptions<HttpRemoteOptions> httpRemoteOptions) : IHttpRequestPipelineHandler
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage?> HandleAsync(HttpRequestPipelineContext context,
        Func<Task<HttpResponseMessage?>> next)
    {
        // 获取当前 HttpRequestBuilder 实例
        var httpRequestBuilder = context.Builder;

        // 构建 HttpRequestMessage 实例
        var httpRequestMessage = httpRequestBuilder.Build(httpRemoteOptions.Value, httpContentProcessorFactory,
            context.HttpClient.BaseAddress ?? httpRemoteOptions.Value.FallbackBaseAddress);

        // 将 HttpCompletionOption 写入请求选项，供请求分析工具使用
        httpRequestMessage.Options.Set(
            new HttpRequestOptionsKey<HttpCompletionOption>(Constants.HTTP_COMPLETION_OPTION_KEY),
            context.CompletionOption);

        // 更新上下文
        context.RequestMessage = httpRequestMessage;

        // 获取当前 HttpClient 实例的配置名称的配置选项
        var httpClientOptions = serviceProvider.GetService<IOptionsMonitor<HttpClientOptions>>()
            ?.Get(httpRequestBuilder.HttpClientName);

        // 获取全局的 IHttpRequestEventHandler 事件处理程序
        var globalEventHandler = httpClientOptions?.HttpRequestEventHandler;

        // 解析 IHttpRequestEventHandler 事件处理程序
        var requestEventHandler = context.Items.TryGetValue(Constants.REQUEST_EVENT_HANDLER_KEY, out var eventHandler)
            ? eventHandler as IHttpRequestEventHandler
            : null;

        // 处理发送 HTTP 请求之前
        HandlePreSendRequest(httpRequestBuilder, globalEventHandler, requestEventHandler, httpRequestMessage);

        // 调用下一个处理器的委托
        return await next();
    }

    /// <summary>
    ///     处理发送 HTTP 请求之前
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="globalEventHandler"><see cref="HttpClientOptions" /> 配置 <see cref="IHttpRequestEventHandler" /></param>
    /// <param name="requestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </param>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal static void HandlePreSendRequest(HttpRequestBuilder httpRequestBuilder,
        IHttpRequestEventHandler? globalEventHandler, IHttpRequestEventHandler? requestEventHandler,
        HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (globalEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(globalEventHandler.OnPreSendRequest, httpRequestMessage);
        }

        // 空检查
        if (requestEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(requestEventHandler.OnPreSendRequest, httpRequestMessage);
        }

        httpRequestBuilder.OnPreSendRequest.TryInvoke(httpRequestMessage);
    }
}