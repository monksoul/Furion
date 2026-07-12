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
///     响应断言管道处理器
/// </summary>
/// <param name="serviceProvider">
///     <see cref="IServiceProvider" />
/// </param>
internal sealed class ResponseAssertionPipelineHandler(IServiceProvider serviceProvider) : IHttpRequestPipelineHandler
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

        // 执行断言委托操作
        await ExecuteAssertionsAsync(httpRequestBuilder, httpResponseMessage, context.RequestDuration, serviceProvider);

        return httpResponseMessage;
    }

    /// <summary>
    ///     执行断言委托操作
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="requestDuration">请求耗时（毫秒）</param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    internal static async Task ExecuteAssertionsAsync(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage, long requestDuration, IServiceProvider serviceProvider)
    {
        // 检查断言是否启用且已配置委托集合
        if (httpRequestBuilder is { AssertionsEnabled: true, Assertions.Count: > 0 })
        {
            // 初始化 HttpAssertionContext 实例
            var httpAssertionContext = new HttpAssertionContext(httpResponseMessage, requestDuration, serviceProvider);

            // 逐个调用断言委托
            foreach (var httpAssertion in httpRequestBuilder.Assertions)
            {
                await httpAssertion(httpAssertionContext);
            }
        }
    }
}