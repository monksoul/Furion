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
using System.Net;

namespace Furion.HttpRemote;

/// <summary>
///     请求重试管道处理器
/// </summary>
internal sealed class RetryPipelineHandler : IHttpRequestPipelineHandler
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage?> HandleAsync(HttpRequestPipelineContext context,
        Func<Task<HttpResponseMessage?>> next)
    {
        // 获取当前 HttpRequestBuilder 实例
        var httpRequestBuilder = context.Builder;

        // 获取 HttpRetryOptions 实例（空检查）
        if (httpRequestBuilder.RetryOptions is not { } httpRetryOptions)
        {
            // 调用下一个处理器的委托
            return await next();
        }

        // 是否无限重试
        var retryIndefinitely = httpRetryOptions.RetryIndefinitely;

        // 计算最大重试次数（非无限时使用）
        var maxRetries = retryIndefinitely
            ? 0
            : httpRetryOptions.RetryIntervals?.Count > 0
                ? httpRetryOptions.RetryIntervals.Count
                : httpRetryOptions.MaxRetries;

        // 小于或等于 0 检查（非无限重试时）
        if (!retryIndefinitely && maxRetries <= 0)
        {
            // 调用下一个处理器的委托
            return await next();
        }

        // 获取每次重试前的回调委托
        var onRetry = httpRetryOptions.OnRetry;

        // 当前尝试次数
        var attempt = 0;

        // 循环进行重试
        while (retryIndefinitely || attempt <= maxRetries)
        {
            // 初始化 HttpRetryContext 实例
            HttpRetryContext? retryContext;

            try
            {
                // 调用下一个处理器的委托
                var httpResponseMessage = await next();

                // 判断是否需要根据状态码触发重试
                if (httpResponseMessage is not null && ShouldRetryOnStatusCode(httpRetryOptions.RetryStatusCodes,
                        httpResponseMessage.StatusCode))
                {
                    // 非无限重试时，达到最大次数后停止并返回
                    if (!retryIndefinitely && attempt == maxRetries)
                    {
                        return httpResponseMessage;
                    }

                    // 初始化 HttpRetryContext 实例
                    retryContext = new HttpRetryContext
                    {
                        Attempt = attempt + 1,
                        StatusCode = httpResponseMessage.StatusCode,
                        MaxRetries = retryIndefinitely ? -1 : maxRetries // -1 表示无限
                    };

                    // 调用每次重试前的回调委托
                    onRetry?.TryInvoke(retryContext);

                    // 释放前一个 HttpResponseMessage 实例
                    httpResponseMessage.Dispose();
                }
                else
                {
                    return httpResponseMessage;
                }
            }
            // 判断是否需要根据异常类型触发重试
            catch (Exception ex) when (ShouldRetryOnException(httpRetryOptions.RetryExceptionTypes, ex))
            {
                // 非无限重试时，达到最大次数后停止并重新抛出异常
                if (!retryIndefinitely && attempt == maxRetries)
                {
                    throw;
                }

                // 初始化 HttpRetryContext 实例
                retryContext = new HttpRetryContext
                {
                    Attempt = attempt + 1, Exception = ex, MaxRetries = retryIndefinitely ? -1 : maxRetries
                };

                // 调用每次重试前的回调委托
                onRetry?.TryInvoke(retryContext);
            }

            // 计算当前重试的间隔时间
            var retryDelay = GetRetryDelay(httpRetryOptions, attempt);

            // 小于 0 检查
            if (retryDelay < TimeSpan.Zero)
            {
                retryDelay = TimeSpan.Zero;
            }

            // 延迟指定间隔时间后再重试
            await Task.Delay(retryDelay, context.CancellationToken);

            // 尝试次数递增
            attempt++;
        }

        return null;
    }

    /// <summary>
    ///     获取当前重试应该等待的间隔时间
    /// </summary>
    /// <param name="httpRetryOptions">
    ///     <see cref="HttpRetryOptions" />
    /// </param>
    /// <param name="attempt">尝试的次数（从 0 开始）</param>
    /// <returns>
    ///     <see cref="TimeSpan" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static TimeSpan GetRetryDelay(HttpRetryOptions httpRetryOptions, int attempt)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRetryOptions);

        // 如果设置了自定义间隔数组，则循环使用
        if (httpRetryOptions.RetryIntervals?.Count > 0)
        {
            return httpRetryOptions.RetryIntervals[attempt % httpRetryOptions.RetryIntervals.Count];
        }

        // 固定间隔或指数退避
        return httpRetryOptions.UseExponentialBackoff
            ? TimeSpan.FromMilliseconds(httpRetryOptions.RetryInterval.TotalMilliseconds * Math.Pow(2, attempt))
            : httpRetryOptions.RetryInterval;
    }

    /// <summary>
    ///     判断异常类型是否应该触发重试
    /// </summary>
    /// <param name="exceptionTypes">需要重试的异常类型集合</param>
    /// <param name="exception">当前异常对象</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool ShouldRetryOnException(HashSet<Type>? exceptionTypes, Exception exception) =>
        exceptionTypes is null or { Count: 0 } || exceptionTypes.Any(type => type.IsInstanceOfType(exception));

    /// <summary>
    ///     判断 HTTP 状态码是否应该触发重试
    /// </summary>
    /// <param name="statusCodes">需要重试的 HTTP 状态码集合</param>
    /// <param name="statusCode">当前 HTTP 状态码</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool ShouldRetryOnStatusCode(HashSet<HttpStatusCode>? statusCodes, HttpStatusCode statusCode) =>
        statusCodes is not (null or { Count: 0 }) && statusCodes.Contains(statusCode);
}