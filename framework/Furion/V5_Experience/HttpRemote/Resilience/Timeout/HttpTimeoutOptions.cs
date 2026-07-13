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
///     HTTP 请求超时选项
/// </summary>
public sealed class HttpTimeoutOptions
{
    /// <summary>
    ///     超时时间
    /// </summary>
    /// <remarks>设置为 <c>null</c> 表示使用默认超时（通常为 100 秒），设置为 <see cref="System.Threading.Timeout.InfiniteTimeSpan" /> 表示永不超时。</remarks>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    ///     超时发生时要执行的操作
    /// </summary>
    public Action? OnTimeout { get; set; }

    /// <summary>
    ///     设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间，<c>null</c> 表示使用默认值</param>
    /// <returns>
    ///     <see cref="HttpTimeoutOptions" />
    /// </returns>
    public HttpTimeoutOptions SetTimeout(TimeSpan? timeout)
    {
        Timeout = timeout;

        return this;
    }

    /// <summary>
    ///     设置超时时间
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>设置为 <c>-1</c> 表示无超时</description>
    ///         </item>
    ///         <item>
    ///             <description>设置为 <c>0</c> 将立即取消请求</description>
    ///         </item>
    ///         <item>
    ///             <description>负值（除 <c>-1</c> 毫秒外）将引发 <see cref="ArgumentOutOfRangeException" /></description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="milliseconds">超时时间（毫秒）</param>
    /// <returns>
    ///     <see cref="HttpTimeoutOptions" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public HttpTimeoutOptions SetTimeout(double milliseconds)
    {
        const double epsilon = 1e-9;
        var isInfinite = Math.Abs(milliseconds + 1) < epsilon;

        // 检查超时时间是否小于 0 且不等于 -1
        if (milliseconds < 0 && !isInfinite)
        {
            throw new ArgumentOutOfRangeException(nameof(milliseconds),
                "Timeout value must be greater than or equal to -1. Use -1 for infinite timeout, 0 for immediate cancellation.");
        }

        Timeout = isInfinite ? System.Threading.Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(milliseconds);

        return this;
    }

    /// <summary>
    ///     设置超时发生时的回调操作
    /// </summary>
    /// <param name="onTimeout">超时发生时要执行的操作</param>
    /// <returns>
    ///     <see cref="HttpTimeoutOptions" />
    /// </returns>
    public HttpTimeoutOptions SetOnTimeout(Action? onTimeout)
    {
        OnTimeout = onTimeout;

        return this;
    }
}