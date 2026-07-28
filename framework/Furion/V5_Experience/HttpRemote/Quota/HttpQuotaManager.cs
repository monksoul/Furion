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

using System.Collections.Concurrent;

namespace Furion.HttpRemote;

/// <summary>
///     基于内存缓存的配额管理器
/// </summary>
internal sealed class HttpQuotaManager : IHttpQuotaManager
{
    /// <summary>
    ///     计数器缓存字典
    /// </summary>
    /// <remarks>键格式：{httpClientName}:{quotaKey}。</remarks>
    internal readonly ConcurrentDictionary<string, HttpQuotaCounter> _counters = new();

    /// <summary>
    ///     策略名称到策略实例的映射（忽略大小写）
    /// </summary>
    internal readonly Dictionary<string, IHttpQuotaStrategy> _strategies;

    /// <summary>
    ///     <inheritdoc cref="HttpQuotaManager" />
    /// </summary>
    /// <param name="strategies"><see cref="IHttpQuotaStrategy" /> 集合</param>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpQuotaManager(IEnumerable<IHttpQuotaStrategy> strategies)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(strategies);

        _strategies = new Dictionary<string, IHttpQuotaStrategy>(StringComparer.OrdinalIgnoreCase);

        // 构建策略名称到策略实例的映射
        foreach (var strategy in strategies)
        {
            _strategies[strategy.Name] = strategy;
        }
    }

    /// <inheritdoc />
    public bool TryIncrement(string? httpClientName, string quotaKey, HttpQuotaLimit quotaLimit, out int current)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(quotaKey);
        ArgumentNullException.ThrowIfNull(quotaLimit);

        // 空检查
        if (string.IsNullOrWhiteSpace(quotaLimit.Strategy))
        {
            throw new InvalidOperationException(
                $"Quota limit for key '{quotaKey}' has no strategy specified. Please set {nameof(HttpQuotaLimit.Strategy)} to a registered IHttpQuotaStrategy name (e.g., \"daily\").");
        }

        // 尝试从映射表中获取配额策略实例
        if (!_strategies.TryGetValue(quotaLimit.Strategy, out var quotaStrategy))
        {
            throw new InvalidOperationException(
                $"No quota strategy registered with name '{quotaLimit.Strategy}' (required by quota key '{quotaKey}'). Please use `AddDefaultQuotaStrategies()` or `AddQuotaStrategy<T>()` on the HttpRemoteBuilder to register the strategy.");
        }

        // 小于或等于 0 检查
        if (quotaLimit.MaxCount <= 0)
        {
            throw new InvalidOperationException(
                $"Invalid MaxCount ({quotaLimit.MaxCount}) for quota key '{quotaKey}'. It must be greater than zero.");
        }

        // 初始化计数器缓存键
        var key = $"{httpClientName ?? string.Empty}:{quotaKey}";

        // 从缓存字典中获取或创建配额计数器
        var quotaCounter = _counters.GetOrAdd(key, _ => new HttpQuotaCounter());

        // 尝试获取一个配额，并更新 quotaCounter 的计数和窗口标识
        lock (quotaCounter)
        {
            return quotaStrategy.TryAcquire(quotaCounter, quotaLimit.MaxCount, out current);
        }
    }
}