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
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     设置无限重试，直到成功
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="retryInterval">重试间隔基准时间。默认为 1 秒，如果同时指定了 <paramref name="retryIntervals" /> 则此参数被忽略</param>
    /// <param name="retryIntervals">自定义重试间隔数组。如果指定，则循环使用该数组中的时间间隔</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetRetryIndefinitely(TimeSpan? retryInterval = null, params TimeSpan[] retryIntervals)
    {
        // 初始化 HttpRetryOptions 实例
        var httpRetryOptions = new HttpRetryOptions { RetryIndefinitely = true };

        // 检查是否配置了自定义重试间隔数组
        if (retryIntervals is { Length: > 0 })
        {
            httpRetryOptions.SetRetryIntervals(retryIntervals);
        }
        else
        {
            httpRetryOptions.SetRetryInterval(retryInterval ?? TimeSpan.FromSeconds(1));
        }

        return SetRetry(httpRetryOptions);
    }

    /// <summary>
    ///     配置请求重试策略
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="maxRetries">最大重试次数。0 表示不重试</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetRetry(int maxRetries) => SetRetry(u => u.SetMaxRetries(maxRetries));

    /// <summary>
    ///     配置请求重试策略
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="maxRetries">最大重试次数。0 表示不重试</param>
    /// <param name="onRetry">每次重试前的回调委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetRetry(int maxRetries, Action<HttpRetryContext>? onRetry) =>
        SetRetry(u => u.SetMaxRetries(maxRetries).SetOnRetry(onRetry));

    /// <summary>
    ///     配置请求重试策略
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="maxRetries">最大重试次数。0 表示不重试</param>
    /// <param name="retryInterval">重试间隔基准时间</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetRetry(int maxRetries, TimeSpan retryInterval) =>
        SetRetry(u => u.SetMaxRetries(maxRetries).SetRetryInterval(retryInterval));

    /// <summary>
    ///     配置请求重试策略
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="maxRetries">最大重试次数。0 表示不重试</param>
    /// <param name="retryInterval">重试间隔基准时间</param>
    /// <param name="onRetry">每次重试前的回调委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetRetry(int maxRetries, TimeSpan retryInterval, Action<HttpRetryContext>? onRetry) =>
        SetRetry(u => u.SetMaxRetries(maxRetries).SetRetryInterval(retryInterval).SetOnRetry(onRetry));

    /// <summary>
    ///     配置请求重试策略
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="maxRetries">最大重试次数。0 表示不重试</param>
    /// <param name="milliseconds">重试间隔基准时间</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetRetry(int maxRetries, double milliseconds) =>
        SetRetry(u => u.SetMaxRetries(maxRetries).SetRetryInterval(milliseconds));

    /// <summary>
    ///     配置请求重试策略
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="maxRetries">最大重试次数。0 表示不重试</param>
    /// <param name="milliseconds">重试间隔基准时间</param>
    /// <param name="onRetry">每次重试前的回调委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetRetry(int maxRetries, double milliseconds, Action<HttpRetryContext>? onRetry) =>
        SetRetry(u => u.SetMaxRetries(maxRetries).SetRetryInterval(milliseconds).SetOnRetry(onRetry));

    /// <summary>
    ///     配置请求重试策略
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="httpRetryOptions">
    ///     <see cref="HttpRetryOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetRetry(HttpRetryOptions httpRetryOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRetryOptions);

        RetryOptions = httpRetryOptions;

        return this;
    }

    /// <summary>
    ///     配置请求重试策略
    /// </summary>
    /// <remarks>请务必配合 <see cref="SetTimeout(TimeSpan?)" /> 等超时方法使用，否则可能导致请求无限阻塞。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetRetry(Action<HttpRetryOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 初始化 HttpRetryOptions 实例
        var httpRetryOptions = new HttpRetryOptions();

        // 调用自定义配置委托
        configure.Invoke(httpRetryOptions);

        RetryOptions = httpRetryOptions;

        return this;
    }

    /// <summary>
    ///     设置永不超时
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithoutTimeout() => SetTimeout(u => u.SetTimeout(Timeout.InfiniteTimeSpan));

    /// <summary>
    ///     设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetTimeout(TimeSpan? timeout) => SetTimeout(u => u.SetTimeout(timeout));

    /// <summary>
    ///     设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <param name="onTimeout">超时发生时要执行的操作；若 <paramref name="timeout" /> 为 null，那么 <paramref name="onTimeout" /> 将不会被调用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetTimeout(TimeSpan? timeout, Action? onTimeout) =>
        SetTimeout(u => u.SetTimeout(timeout).SetOnTimeout(onTimeout));

    /// <summary>
    ///     设置超时时间
    /// </summary>
    /// <param name="timeout">
    ///     <para>超时时间（毫秒）</para>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>设置为 <c>-1</c> 表示无超时</description>
    ///             </item>
    ///             <item>
    ///                 <description>设置为 <c>0</c> 将立即取消请求</description>
    ///             </item>
    ///             <item>
    ///                 <description>负值（除 <c>-1</c> 毫秒外）将引发 <see cref="ArgumentOutOfRangeException" /></description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetTimeout(double timeout) => SetTimeout(u => u.SetTimeout(timeout));

    /// <summary>
    ///     设置超时时间
    /// </summary>
    /// <param name="timeout">
    ///     <para>超时时间（毫秒）</para>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>设置为 <c>-1</c> 表示无超时</description>
    ///             </item>
    ///             <item>
    ///                 <description>设置为 <c>0</c> 将立即取消请求</description>
    ///             </item>
    ///             <item>
    ///                 <description>负值（除 <c>-1</c> 毫秒外）将引发 <see cref="ArgumentOutOfRangeException" /></description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </param>
    /// <param name="onTimeout">超时发生时要执行的操作</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetTimeout(double timeout, Action? onTimeout) =>
        SetTimeout(u => u.SetTimeout(timeout).SetOnTimeout(onTimeout));

    /// <summary>
    ///     配置请求超时策略
    /// </summary>
    /// <param name="httpTimeoutOptions">
    ///     <see cref="HttpTimeoutOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetTimeout(HttpTimeoutOptions httpTimeoutOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpTimeoutOptions);

        TimeoutOptions = httpTimeoutOptions;

        return this;
    }

    /// <summary>
    ///     配置请求超时策略
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder SetTimeout(Action<HttpTimeoutOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 初始化 HttpTimeoutOptions 实例
        var httpTimeoutOptions = new HttpTimeoutOptions();

        // 调用自定义配置委托
        configure.Invoke(httpTimeoutOptions);

        TimeoutOptions = httpTimeoutOptions;

        return this;
    }
}