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
///     HTTP 声明式重试特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public class RetryAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="RetryAttribute" />
    /// </summary>
    public RetryAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="RetryAttribute" />
    /// </summary>
    /// <param name="maxRetries">最大重试次数。0 表示不重试</param>
    public RetryAttribute(int maxRetries) => MaxRetries = maxRetries;

    /// <summary>
    ///     <inheritdoc cref="RetryAttribute" />
    /// </summary>
    /// <param name="maxRetries">最大重试次数。0 表示不重试</param>
    /// <param name="retryInterval">重试间隔基准时间（毫秒）</param>
    public RetryAttribute(int maxRetries, double retryInterval)
        : this(maxRetries) =>
        RetryInterval = retryInterval;

    /// <summary>
    ///     最大重试次数
    /// </summary>
    /// <remarks>默认值为 0，表示不重试。如果设置了 <see cref="RetryIntervals" />，此值将自动被覆盖为数组长度。</remarks>
    public int MaxRetries { get; set; }

    /// <summary>
    ///     重试间隔基准时间（毫秒）
    /// </summary>
    /// <remarks>默认值为 1000 毫秒。仅在未设置 <see cref="RetryIntervals" /> 时生效。</remarks>
    public double RetryInterval { get; set; } = 1000;

    /// <summary>
    ///     是否采用指数退避重试
    /// </summary>
    /// <remarks>
    ///     默认值为：<c>false</c>。当设置为 <c>true</c> 时，每次重试间隔 = <see cref="RetryInterval" /> * 2^(retry-1)。仅在未设置
    ///     <see cref="RetryIntervals" /> 时生效。
    /// </remarks>
    public bool UseExponentialBackoff { get; set; }

    /// <summary>
    ///     自定义重试间隔数组（毫秒）
    /// </summary>
    /// <remarks>
    ///     如果设置了此属性，则重试次数将等于数组长度，<see cref="MaxRetries" /> 和 <see cref="UseExponentialBackoff" /> 将被忽略。每次重试将按顺序使用数组中对应索引的间隔时间。
    /// </remarks>
    public double[]? RetryIntervals { get; set; }

    /// <summary>
    ///     需要重试的 HTTP 状态码集合
    /// </summary>
    /// <remarks>若为空，则仅重试因异常引发的失败。</remarks>
    public int[]? RetryStatusCodes { get; set; }

    /// <summary>
    ///     需要重试的异常类型集合
    /// </summary>
    /// <remarks>若为空，则对所有 <see cref="Exception" /> 进行重试（受 <see cref="MaxRetries" /> 限制）。</remarks>
    public Type[]? RetryExceptionTypes { get; set; }

    /// <summary>
    ///     是否无限重试，直到成功
    /// </summary>
    /// <remarks>
    ///     默认值为：<c>false</c>。当设置为 <c>true</c> 时，<see cref="MaxRetries" /> 和 <see cref="RetryIntervals" />
    ///     的长度将被忽略，一直重试直到成功或发生不可重试的异常。
    /// </remarks>
    public bool RetryIndefinitely { get; set; }
}