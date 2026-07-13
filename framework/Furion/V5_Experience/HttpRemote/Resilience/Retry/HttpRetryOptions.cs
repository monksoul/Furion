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

using System.Net;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 请求重试选项
/// </summary>
public sealed class HttpRetryOptions
{
    /// <summary>
    ///     最大重试次数
    /// </summary>
    /// <remarks>默认值为 0，表示不重试。如果设置了 <see cref="RetryIntervals" />，此值将自动被覆盖为数组长度。</remarks>
    public int MaxRetries { get; set; }

    /// <summary>
    ///     重试间隔基准时间
    /// </summary>
    /// <remarks>默认值为 1 秒。仅在未设置 <see cref="RetryIntervals" /> 时生效。</remarks>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     是否采用指数退避重试
    /// </summary>
    /// <remarks>
    ///     默认值为：<c>false</c>。当设置为 <c>true</c> 时，每次重试间隔 = <see cref="RetryInterval" /> * 2^(retry-1)。仅在未设置
    ///     <see cref="RetryIntervals" /> 时生效。
    /// </remarks>
    public bool UseExponentialBackoff { get; set; }

    /// <summary>
    ///     自定义重试间隔数组
    /// </summary>
    /// <remarks>
    ///     如果设置了此属性，则重试次数将等于数组长度，<see cref="MaxRetries" /> 和 <see cref="UseExponentialBackoff" /> 将被忽略。每次重试将按顺序使用数组中对应索引的间隔时间。
    /// </remarks>
    public IList<TimeSpan>? RetryIntervals { get; set; }

    /// <summary>
    ///     需要重试的 HTTP 状态码集合
    /// </summary>
    /// <remarks>若为空，则仅重试因异常引发的失败。</remarks>
    public HashSet<HttpStatusCode>? RetryStatusCodes { get; set; }

    /// <summary>
    ///     需要重试的异常类型集合
    /// </summary>
    /// <remarks>若为空，则对所有 <see cref="Exception" /> 进行重试（受 <see cref="MaxRetries" /> 限制）。</remarks>
    public HashSet<Type>? RetryExceptionTypes { get; set; }

    /// <summary>
    ///     每次重试前的回调委托
    /// </summary>
    /// <remarks>可用于记录日志、发送通知等，参数为 <see cref="HttpRetryContext" />。</remarks>
    public Action<HttpRetryContext>? OnRetry { get; set; }

    /// <summary>
    ///     是否无限重试，直到成功
    /// </summary>
    /// <remarks>
    ///     默认值为：<c>false</c>。当设置为 <c>true</c> 时，<see cref="MaxRetries" /> 和 <see cref="RetryIntervals" />
    ///     的长度将被忽略，一直重试直到成功或发生不可重试的异常。
    /// </remarks>
    public bool RetryIndefinitely { get; set; }

    /// <summary>
    ///     设置最大重试次数
    /// </summary>
    /// <param name="maxRetries">最大重试次数</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public HttpRetryOptions SetMaxRetries(int maxRetries)
    {
        // 小于 0 检查
        if (maxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries must be non-negative.");
        }

        MaxRetries = maxRetries;

        return this;
    }

    /// <summary>
    ///     设置重试间隔基准时间
    /// </summary>
    /// <param name="interval">重试间隔基准时间</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public HttpRetryOptions SetRetryInterval(TimeSpan interval)
    {
        // 小于 0 检查
        if (interval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Retry interval must be non-negative.");
        }

        RetryInterval = interval;

        return this;
    }

    /// <summary>
    ///     设置重试间隔基准时间
    /// </summary>
    /// <param name="milliseconds">重试间隔基准时间（毫秒）</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public HttpRetryOptions SetRetryInterval(double milliseconds)
    {
        // 小于 0 检查
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (milliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(milliseconds), "Retry interval must be non-negative.");
        }

        return SetRetryInterval(TimeSpan.FromMilliseconds(milliseconds));
    }

    /// <summary>
    ///     设置是否使用指数退避
    /// </summary>
    /// <param name="use">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    public HttpRetryOptions SetUseExponentialBackoff(bool use)
    {
        UseExponentialBackoff = use;

        return this;
    }

    /// <summary>
    ///     设置自定义重试间隔数组
    /// </summary>
    /// <param name="intervals">间隔时间数组</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public HttpRetryOptions SetRetryIntervals(params TimeSpan[] intervals)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(intervals);

        // 检查每个时间间隔是否为负值
        for (var i = 0; i < intervals.Length; i++)
        {
            // 小于 0 检查
            if (intervals[i] < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(intervals),
                    $"Retry interval at index {i} must be non-negative.");
            }
        }

        RetryIntervals = intervals.ToList();

        return this;
    }

    /// <summary>
    ///     设置自定义重试间隔数组
    /// </summary>
    /// <param name="milliseconds">间隔毫秒数组（毫秒）</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public HttpRetryOptions SetRetryIntervals(params double[] milliseconds)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(milliseconds);

        return SetRetryIntervals(milliseconds.Select(TimeSpan.FromMilliseconds).ToArray());
    }

    /// <summary>
    ///     添加一个需要重试的 HTTP 状态码
    /// </summary>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    public HttpRetryOptions AddRetryStatusCode(HttpStatusCode statusCode)
    {
        RetryStatusCodes ??= [];
        RetryStatusCodes.Add(statusCode);

        return this;
    }

    /// <summary>
    ///     添加一个需要重试的 HTTP 状态码
    /// </summary>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    public HttpRetryOptions AddRetryStatusCode(int statusCode) => AddRetryStatusCode((HttpStatusCode)statusCode);

    /// <summary>
    ///     添加多个需要重试的 HTTP 状态码
    /// </summary>
    /// <param name="statusCodes">HTTP 状态码集合</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRetryOptions AddRetryStatusCodes(params IEnumerable<HttpStatusCode> statusCodes)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(statusCodes);

        RetryStatusCodes ??= [];

        // 遍历 HTTP 状态码集合
        foreach (var statusCode in statusCodes)
        {
            RetryStatusCodes.Add(statusCode);
        }

        return this;
    }

    /// <summary>
    ///     添加多个需要重试的 HTTP 状态码
    /// </summary>
    /// <param name="statusCodes">HTTP 状态码集合</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRetryOptions AddRetryStatusCodes(params IEnumerable<int> statusCodes)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(statusCodes);

        return AddRetryStatusCodes(statusCodes.Select(code => (HttpStatusCode)code));
    }

    /// <summary>
    ///     添加一个需要重试的异常类型
    /// </summary>
    /// <typeparam name="TException">异常类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    public HttpRetryOptions AddRetryException<TException>() where TException : Exception =>
        AddRetryExceptions(typeof(TException));

    /// <summary>
    ///     添加一个需要重试的异常类型
    /// </summary>
    /// <param name="exceptionType">异常类型</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    public HttpRetryOptions AddRetryException(Type exceptionType) => AddRetryExceptions(exceptionType);

    /// <summary>
    ///     添加多个需要重试的异常类型
    /// </summary>
    /// <param name="exceptionTypes">异常类型集合</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public HttpRetryOptions AddRetryExceptions(params IEnumerable<Type> exceptionTypes)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(exceptionTypes);

        RetryExceptionTypes ??= [];

        // 遍历异常类型集合
        foreach (var exceptionType in exceptionTypes)
        {
            // 检查类型是否是 Exception 类型
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException($"The type '{exceptionType}' must be derived from {typeof(Exception)}.",
                    nameof(exceptionTypes));
            }

            RetryExceptionTypes.Add(exceptionType);
        }

        return this;
    }

    /// <summary>
    ///     设置每次重试前的回调委托
    /// </summary>
    /// <param name="onRetry">回调委托</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    public HttpRetryOptions SetOnRetry(Action<HttpRetryContext>? onRetry)
    {
        OnRetry = onRetry;

        return this;
    }

    /// <summary>
    ///     设置是否无限重试
    /// </summary>
    /// <param name="retryIndefinitely">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRetryOptions" />
    /// </returns>
    public HttpRetryOptions SetRetryIndefinitely(bool retryIndefinitely)
    {
        RetryIndefinitely = retryIndefinitely;

        return this;
    }
}