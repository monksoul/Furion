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
///     超时控制管道处理器
/// </summary>
internal sealed class TimeoutPipelineHandler : IHttpRequestPipelineHandler
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage?> HandleAsync(HttpRequestPipelineContext context,
        Func<Task<HttpResponseMessage?>> next)
    {
        // 获取当前 HttpRequestBuilder 实例
        var httpRequestBuilder = context.Builder;

        // 空检查
        if (httpRequestBuilder.Timeout is null)
        {
            // 调用下一个处理器的委托
            return await next();
        }

        // 确保 HttpRequestBuilder 的 Timeout 属性值小于 HttpClient 的 Timeout 属性值（默认 100秒）
        if (httpRequestBuilder.Timeout.Value > context.HttpClient.Timeout)
        {
            throw new InvalidOperationException(
                "HttpRequestBuilder's Timeout cannot be greater than HttpClient's Timeout, which defaults to 100 seconds.");
        }

        // 创建关联的超时 Token 标识
        using var timeoutCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
        var timeoutCancellationToken = timeoutCancellationTokenSource.Token;

        // 定义标志位，用于判断是否引发了超时操作
        var isTimeoutTriggered = false;

        // 调用超时发生时要执行的操作
        if (httpRequestBuilder.TimeoutAction is not null)
        {
            timeoutCancellationToken.Register(httpRequestBuilder.TimeoutAction.TryInvoke);
        }

        // 注册回调，用于标记是否是超时触发的取消
        timeoutCancellationToken.Register(() => isTimeoutTriggered = true);

        // 延迟指定时间后取消任务
        timeoutCancellationTokenSource.CancelAfter(httpRequestBuilder.Timeout.Value);

        // 获取原始取消令牌
        var originalToken = context.CancellationToken;

        // 更新上下文（替换）
        context.CancellationToken = timeoutCancellationToken;

        try
        {
            // 调用下一个处理器的委托
            return await next();
        }
        // 检查是否是超时导致的取消，如果是则抛出 TaskCanceledException(TimeoutException) 超时异常
        catch (OperationCanceledException ex) when (isTimeoutTriggered && !originalToken.IsCancellationRequested)
        {
            throw new TaskCanceledException(
                $"The request was canceled due to the configured HttpRequestBuilder.Timeout of {httpRequestBuilder.Timeout?.TotalSeconds:0.###} seconds elapsing.",
                new TimeoutException("The operation was canceled.", ex));
        }
        finally
        {
            // 同步上下文
            context.CancellationToken = originalToken;
        }
    }
}