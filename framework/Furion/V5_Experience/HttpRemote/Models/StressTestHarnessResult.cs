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

using Furion.Utilities;

namespace Furion.HttpRemote;

/// <summary>
///     压力测试结果
/// </summary>
public sealed class StressTestHarnessResult
{
    /// <summary>
    ///     <inheritdoc cref="StressTestHarnessResult" />
    /// </summary>
    /// <param name="totalRequests">总请求次数</param>
    /// <param name="totalTimeInSeconds">总用时（秒）</param>
    /// <param name="successfulRequests">成功请求次数</param>
    /// <param name="failedRequests">失败请求次数</param>
    /// <param name="responseTimes">请求的响应时间数组</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public StressTestHarnessResult(long totalRequests, double totalTimeInSeconds, long successfulRequests,
        long failedRequests, double[] responseTimes)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(responseTimes);

        // 检查请求的响应时间数组长度是否等于总请求次数
        if (responseTimes.Length != totalRequests)
        {
            throw new ArgumentException(
                $"The number of response times ({responseTimes.Length}) does not match the total number of requests ({totalRequests}).",
                nameof(responseTimes)
            );
        }

        TotalRequests = totalRequests;
        TotalTimeInSeconds = totalTimeInSeconds;
        SuccessfulRequests = successfulRequests;
        FailedRequests = failedRequests;

        // 计算每秒查询率 (QPS)
        CalculateQueriesPerSecond(totalRequests, totalTimeInSeconds);

        // 计算最小、最大和平均响应时间（毫秒）
        CalculateMinMaxAvgResponseTime(responseTimes);

        // 初始化排序后的响应时间数组
        var sortedResponseTimes = (double[])responseTimes.Clone();
        Array.Sort(sortedResponseTimes);

        // 计算各个百分位的响应时间（毫秒）
        CalculatePercentiles(sortedResponseTimes);
    }

    /// <summary>
    ///     总请求次数
    /// </summary>
    public long TotalRequests { get; }

    /// <summary>
    ///     总用时（秒）
    /// </summary>
    public double TotalTimeInSeconds { get; }

    /// <summary>
    ///     成功请求次数
    /// </summary>
    public long SuccessfulRequests { get; }

    /// <summary>
    ///     失败请求次数
    /// </summary>
    public long FailedRequests { get; }

    /// <summary>
    ///     每秒查询率 (QPS)
    /// </summary>
    public double QueriesPerSecond { get; private set; }

    /// <summary>
    ///     最小响应时间（毫秒）
    /// </summary>
    public double MinResponseTime { get; private set; }

    /// <summary>
    ///     最大响应时间（毫秒）
    /// </summary>
    public double MaxResponseTime { get; private set; }

    /// <summary>
    ///     平均响应时间（毫秒）
    /// </summary>
    public double AverageResponseTime { get; private set; }

    /// <summary>
    ///     P10 响应时间（毫秒）
    /// </summary>
    public double Percentile10ResponseTime { get; private set; }

    /// <summary>
    ///     P25 响应时间（毫秒）
    /// </summary>
    public double Percentile25ResponseTime { get; private set; }

    /// <summary>
    ///     P50 响应时间（毫秒）
    /// </summary>
    public double Percentile50ResponseTime { get; private set; }

    /// <summary>
    ///     P75 响应时间（毫秒）
    /// </summary>
    public double Percentile75ResponseTime { get; private set; }

    /// <summary>
    ///     P90 响应时间（毫秒）
    /// </summary>
    public double Percentile90ResponseTime { get; private set; }

    /// <summary>
    ///     P95 响应时间（毫秒）
    /// </summary>
    public double Percentile95ResponseTime { get; private set; }

    /// <summary>
    ///     P99 响应时间（毫秒）
    /// </summary>
    public double Percentile99ResponseTime { get; private set; }

    /// <summary>
    ///     P99.99 响应时间（毫秒）
    /// </summary>
    public double Percentile9999ResponseTime { get; private set; }

    /// <inheritdoc />
    public override string ToString() =>
        StringUtility.FormatKeyValuesSummary([
            new KeyValuePair<string, IEnumerable<string>>("Total Requests", [$"{TotalRequests}"]),
            new KeyValuePair<string, IEnumerable<string>>("Total Time (s)", [$"{TotalTimeInSeconds:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("Successful Requests", [$"{SuccessfulRequests}"]),
            new KeyValuePair<string, IEnumerable<string>>("Failed Requests", [$"{FailedRequests}"]),
            new KeyValuePair<string, IEnumerable<string>>("QPS",
                [double.IsPositiveInfinity(QueriesPerSecond) ? "∞" : $"{QueriesPerSecond:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("Min RT (ms)", [$"{MinResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("Max RT (ms)", [$"{MaxResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("Avg RT (ms)", [$"{AverageResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("P10 RT (ms)", [$"{Percentile10ResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("P25 RT (ms)", [$"{Percentile25ResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("P50 RT (ms)", [$"{Percentile50ResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("P75 RT (ms)", [$"{Percentile75ResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("P90 RT (ms)", [$"{Percentile90ResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("P95 RT (ms)", [$"{Percentile95ResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("P99 RT (ms)", [$"{Percentile99ResponseTime:N2}"]),
            new KeyValuePair<string, IEnumerable<string>>("P99.99 RT (ms)", [$"{Percentile9999ResponseTime:N2}"])
        ], "Stress Test Harness Result")!;

    /// <summary>
    ///     计算每秒查询率 (QPS)
    /// </summary>
    /// <param name="totalRequests">总请求次数</param>
    /// <param name="totalTimeInSeconds">总用时（秒）</param>
    internal void CalculateQueriesPerSecond(long totalRequests, double totalTimeInSeconds) =>
        QueriesPerSecond = totalTimeInSeconds > 0 ? totalRequests / totalTimeInSeconds : double.PositiveInfinity;

    /// <summary>
    ///     计算最小、最大和平均响应时间（毫秒）
    /// </summary>
    /// <param name="responseTimes">每个请求的响应时间数组</param>
    internal void CalculateMinMaxAvgResponseTime(double[] responseTimes)
    {
        // 检查请求的响应时间数组是否为空
        if (responseTimes.Length == 0)
        {
            MinResponseTime = MaxResponseTime = AverageResponseTime = 0;
            return;
        }

        // 初始化计算所需变量
        var min = responseTimes[0];
        var max = responseTimes[0];
        var sum = 0.0;

        // 计算总响应时间，同时更新最小和最大值（毫秒）
        foreach (var t in responseTimes)
        {
            if (t < min)
            {
                min = t;
            }

            if (t > max)
            {
                max = t;
            }

            sum += t;
        }

        // 赋值最小响应时间和最大响应时间（毫秒）
        MinResponseTime = min;
        MaxResponseTime = max;

        // 计算平均响应时间（毫秒）
        AverageResponseTime = sum / responseTimes.Length;
    }

    /// <summary>
    ///     计算各个百分位的响应时间（毫秒）
    /// </summary>
    /// <param name="sortedResponseTimes">已排序的请求响应时间数组</param>
    internal void CalculatePercentiles(double[] sortedResponseTimes)
    {
        // 检查数组是否为空
        if (sortedResponseTimes.Length == 0)
        {
            return;
        }

        // 计算百分位数的响应时间（毫秒）
        Percentile10ResponseTime = CalculatePercentile(sortedResponseTimes, 0.1);
        Percentile25ResponseTime = CalculatePercentile(sortedResponseTimes, 0.25);
        Percentile50ResponseTime = CalculatePercentile(sortedResponseTimes, 0.5);
        Percentile75ResponseTime = CalculatePercentile(sortedResponseTimes, 0.75);
        Percentile90ResponseTime = CalculatePercentile(sortedResponseTimes, 0.9);
        Percentile95ResponseTime = CalculatePercentile(sortedResponseTimes, 0.95);
        Percentile99ResponseTime = CalculatePercentile(sortedResponseTimes, 0.99);
        Percentile9999ResponseTime = CalculatePercentile(sortedResponseTimes, 0.9999);
    }

    /// <summary>
    ///     计算百分位数（毫秒）
    /// </summary>
    /// <param name="sortedResponseTimes">排序后的请求的响应时间数组</param>
    /// <param name="percentile">百分位数</param>
    /// <returns>
    ///     <see cref="double" />
    /// </returns>
    internal static double CalculatePercentile(double[] sortedResponseTimes, double percentile)
    {
        var index = (int)Math.Ceiling((percentile * sortedResponseTimes.Length) - 1e-9) - 1;
        if (index >= sortedResponseTimes.Length)
        {
            index = sortedResponseTimes.Length - 1;
        }

        if (index < 0)
        {
            index = 0;
        }

        return sortedResponseTimes[index];
    }
}