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
using Microsoft.Extensions.Options;

namespace Furion.HttpRemote;

/// <summary>
///     自动重定向管道处理器
/// </summary>
/// <param name="httpContentProcessorFactory">
///     <see cref="IHttpContentProcessorFactory" />
/// </param>
/// <param name="httpRemoteOptions">
///     <see cref="HttpRemoteOptions" />
/// </param>
internal sealed class AutoRedirectPipelineHandler(
    IHttpContentProcessorFactory httpContentProcessorFactory,
    IOptions<HttpRemoteOptions> httpRemoteOptions) : IHttpRequestPipelineHandler
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage?> HandleAsync(HttpRequestPipelineContext context,
        Func<Task<HttpResponseMessage?>> next)
    {
        // 调用下一个处理器的委托
        var httpResponseMessage = await next();

        // 获取当前 HttpRequestBuilder 实例
        var httpRequestBuilder = context.Builder;

        // 获取 HttpRemoteOptions 实例
        var remoteOptions = httpRemoteOptions.Value;

        // 初始化当前重定向次数和原始请求方法
        var redirections = 0;
        var originalHttpMethod = context.OriginalBuilder.HttpMethod!;

        // 处理请求重定向
        while (httpResponseMessage is not null &&
               Helpers.DetermineRedirectMethod(httpResponseMessage.StatusCode, originalHttpMethod,
                   out var redirectMethod) && remoteOptions.AllowAutoRedirect &&
               redirections < remoteOptions.MaximumAutomaticRedirections)
        {
            // 获取重定向地址
            var redirectUrl = httpResponseMessage.Headers.Location;

            // 空检查
            if (redirectUrl is null)
            {
                break;
            }

            // 构建新的 HttpRequestMessage 实例
            var redirectHttpRequestMessage = httpRequestBuilder
                .ConfigureForRedirect(
                    redirectUrl.IsAbsoluteUri
                        ? redirectUrl
                        : new Uri(Helpers.ParseBaseAddress(context.RequestMessage?.RequestUri), redirectUrl),
                    redirectMethod).Build(remoteOptions, httpContentProcessorFactory,
                    context.HttpClient.BaseAddress ?? remoteOptions.FallbackBaseAddress);

            // 释放前一个 HttpResponseMessage 实例
            httpResponseMessage.Dispose();

            // 重新调用发送 HTTP 请求委托
            httpResponseMessage = await context.SendAsync(context.HttpClient, redirectHttpRequestMessage,
                context.CompletionOption, context.CancellationToken);

            // 修复无效的响应内容字符编码
            httpResponseMessage.FixInvalidCharset();

            // 递增重定向次数
            redirections++;
        }

        // 更新上下文
        context.ResponseMessage = httpResponseMessage;

        return httpResponseMessage;
    }
}