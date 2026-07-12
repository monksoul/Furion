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

namespace Furion.HttpRemote;

/// <summary>
///     响应内容长度验证管道处理器
/// </summary>
internal sealed class ContentLengthValidationPipelineHandler : IHttpRequestPipelineHandler
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage?> HandleAsync(HttpRequestPipelineContext context,
        Func<Task<HttpResponseMessage?>> next)
    {
        // 调用下一个处理器的委托
        var httpResponseMessage = await next();

        // 空检查
        if (httpResponseMessage is null)
        {
            return null;
        }

        // 获取当前 HttpRequestBuilder 实例
        var httpRequestBuilder = context.Builder;

        // 检查 HTTP 响应内容长度是否在设定的最大缓冲区大小限制内
        CheckContentLengthWithinLimit(httpRequestBuilder, httpResponseMessage);

        return httpResponseMessage;
    }

    /// <summary>
    ///     检查 HTTP 响应内容长度是否在设定的最大缓冲区大小限制内
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <exception cref="HttpRequestException"></exception>
    internal static void CheckContentLengthWithinLimit(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage)
    {
        // 空检查
        if (httpRequestBuilder.MaxResponseContentBufferSize is null)
        {
            return;
        }

        // 检查响应内容长度
        if (httpResponseMessage.Content.Headers.ContentLength is { } contentLength &&
            contentLength > httpRequestBuilder.MaxResponseContentBufferSize)
        {
            throw new HttpRequestException(
                $"Cannot write more bytes to the buffer than the configured maximum buffer size: `{httpRequestBuilder.MaxResponseContentBufferSize}`.");
        }
    }
}