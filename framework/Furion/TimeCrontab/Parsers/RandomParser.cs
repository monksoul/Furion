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

namespace Furion.TimeCrontab;

/// <summary>
/// Cron 字段值含 R 或 H 字符解析器
/// </summary>
/// <remarks>
/// <para>R 或 H 表示随机生成的时刻，两者完全等价，仅在 <see cref="CrontabFieldKind.Second"/>、<see cref="CrontabFieldKind.Minute"/> 或 <see cref="CrontabFieldKind.Hour"/> 字段域中使用。</para>
/// <para>支持区间随机：R(min-max) 或 H(min-max)，例如 R(30-59) 表示在 30 到 59 之间随机。</para>
/// <para>支持带步长的区间随机：R(min-max)/step 或 H(min-max)/step，例如 R(1-5)/2 表示在 1,3,5 中随机。</para>
/// <para>支持全范围带步长随机：R/step 或 H/step，例如 R/5 表示在字段全范围内每 5 个值取一个随机。</para>
/// <para>支持离散值随机：R(1,5,10,12) 或 H(1,5,10,12) 表示在 1、5、10、12 中随机。</para>
/// <para>参考文献：https://help.eset.com/protect_admin/13.0/zh-CN/cron_expression.html。</para>
/// </remarks>
internal sealed class RandomParser : ICronParser, ITimeParser
{
    /// <summary>
    /// 全局随机种子生成器（线程安全锁）
    /// </summary>
    private static readonly Random _globalRandom = new();

    /// <summary>
    /// 线程独立的随机实例
    /// </summary>
    [ThreadStatic]
    private static Random _localRandom;

    /// <summary>
    /// 获取当前线程的随机实例
    /// </summary>
    private static Random GetRandom()
    {
        if (_localRandom == null)
        {
            int seed;
            lock (_globalRandom)
            {
                seed = _globalRandom.Next();
            }
            _localRandom = new Random(seed);
        }
        return _localRandom;
    }

    /// <summary>
    /// 候选值集合
    /// </summary>
    /// <remarks>
    /// 当指定步长或离散值时，该集合预先存储所有允许的值；
    /// 若无步长且非离散值，则为 null，表示区间内任意值随机。
    /// </remarks>
    private readonly List<int> _candidates;

    /// <summary>
    /// 随机范围最小值（包含）
    /// </summary>
    private readonly int _minValue;

    /// <summary>
    /// 随机范围最大值（包含）
    /// </summary>
    private readonly int _maxValue;

    /// <summary>
    /// 步长
    /// </summary>
    /// <remarks>可为 null，表示无步长限制，此时直接从整个区间随机。</remarks>
    private readonly int? _step;

    /// <summary>
    /// 是否为离散值模式（如 R(1,5,10)）
    /// </summary>
    private readonly bool _isDiscrete;

    /// <summary>
    /// 是否使用简洁步长格式（即 R/step，而非 R(min-max)/step）
    /// </summary>
    private readonly bool _useShortStepFormat;

    /// <summary>
    /// 随机标识符前缀（R 或 H）
    /// </summary>
    private readonly char _prefix;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <remarks>全范围随机。</remarks>
    /// <param name="kind">Cron 字段种类</param>
    /// <param name="prefix">随机标识符前缀，默认为 'R'</param>
    public RandomParser(CrontabFieldKind kind, char prefix = 'R')
        : this(kind, Constants.MinimumDateTimeValues[kind], Constants.MaximumDateTimeValues[kind], null, false, prefix)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <remarks>指定随机区间，无步长。区间内的每个值都有相同的被选中概率。</remarks>
    /// <param name="kind">Cron 字段种类</param>
    /// <param name="minValue">最小值（包含）</param>
    /// <param name="maxValue">最大值（包含）</param>
    /// <param name="prefix">随机标识符前缀，默认为 'R'</param>
    public RandomParser(CrontabFieldKind kind, int minValue, int maxValue, char prefix = 'R')
        : this(kind, minValue, maxValue, null, false, prefix)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <remarks>
    /// 指定随机区间及步长。当提供步长时，会预先生成候选值集合，后续随机只会从这些候选值中抽取。
    /// 例如 minValue=1, maxValue=10, step=3 会生成候选集 {1,4,7,10}。
    /// </remarks>
    /// <param name="kind">Cron 字段种类</param>
    /// <param name="minValue">最小值（包含）</param>
    /// <param name="maxValue">最大值（包含）</param>
    /// <param name="step">步长，可为 null 表示无步长限制</param>
    /// <param name="useShortStepFormat">是否使用简洁步长格式 R/step，默认为 false（使用 R(min-max)/step）</param>
    /// <param name="prefix">随机标识符前缀，默认为 'R'</param>
    /// <exception cref="TimeCrontabException"></exception>
    public RandomParser(CrontabFieldKind kind, int minValue, int maxValue, int? step, bool useShortStepFormat = false, char prefix = 'R')
    {
        // 验证 R 或 H 字符是否在 Second、Minute 或 Hour 字段域中使用
        if (kind != CrontabFieldKind.Second &&
            kind != CrontabFieldKind.Minute &&
            kind != CrontabFieldKind.Hour)
        {
            throw new TimeCrontabException("The <R> or <H> parser can only be used with the Second, Minute, or Hour fields.");
        }

        var fieldMin = Constants.MinimumDateTimeValues[kind];
        var fieldMax = Constants.MaximumDateTimeValues[kind];

        // 验证区间有效性
        if (minValue < fieldMin || minValue > fieldMax)
        {
            throw new TimeCrontabException($"The minimum value {minValue} is out of bounds for the {kind} field.");
        }

        if (maxValue < fieldMin || maxValue > fieldMax)
        {
            throw new TimeCrontabException($"The maximum value {maxValue} is out of bounds for the {kind} field.");
        }

        if (minValue > maxValue)
        {
            throw new TimeCrontabException($"The minimum value {minValue} cannot be greater than the maximum value {maxValue}.");
        }

        // 验证步长有效性（步长必须为正整数且不能超过字段上限）
        if (step.HasValue && (step.Value <= 0 || step.Value > fieldMax))
        {
            throw new TimeCrontabException($"Steps = {step} is out of bounds for <{kind}> field.");
        }

        Kind = kind;
        _minValue = minValue;
        _maxValue = maxValue;
        _step = step;
        _isDiscrete = false;
        _useShortStepFormat = useShortStepFormat;
        _prefix = prefix;

        // 如果提供了步长，则预先生成所有符合步长条件的候选值
        // 生成规则：从 minValue 开始，每次增加 step，直到超过 maxValue
        if (_step.HasValue)
        {
            _candidates = [];

            for (var val = _minValue; val <= _maxValue; val++)
            {
                // 使用 (val - _minValue) % step == 0 来判断是否在步长序列上
                if ((val - _minValue) % _step.Value == 0)
                {
                    _candidates.Add(val);
                }
            }

            // 必须至少有一个候选值，否则抛出异常（例如区间内没有任何值满足步长）
            if (_candidates.Count == 0)
            {
                throw new TimeCrontabException($"The random range ({minValue}-{maxValue})/{step} produces no valid values.");
            }
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="kind">Cron 字段种类</param>
    /// <param name="values">允许的离散值集合，不允许为空</param>
    /// <param name="prefix">随机标识符前缀，默认为 'R'</param>
    /// <exception cref="TimeCrontabException"></exception>
    public RandomParser(CrontabFieldKind kind, IEnumerable<int> values, char prefix = 'R')
    {
        // 验证 R 或 H 字符是否在 Second、Minute 或 Hour 字段域中使用
        if (kind != CrontabFieldKind.Second &&
            kind != CrontabFieldKind.Minute &&
            kind != CrontabFieldKind.Hour)
        {
            throw new TimeCrontabException("The <R> or <H> parser can only be used with the Second, Minute, or Hour fields.");
        }

        // 去重并排序，确保后续输出时顺序一致
        var valueList = values?.Distinct().OrderBy(v => v).ToList();
        if (valueList == null || valueList.Count == 0)
        {
            throw new TimeCrontabException("Discrete random values must not be empty.");
        }

        var fieldMin = Constants.MinimumDateTimeValues[kind];
        var fieldMax = Constants.MaximumDateTimeValues[kind];

        // 检查每个值是否在字段范围内
        foreach (var val in valueList)
        {
            if (val < fieldMin || val > fieldMax)
            {
                throw new TimeCrontabException($"Value {val} is out of bounds for the {kind} field.");
            }
        }

        Kind = kind;
        _candidates = valueList;
        _minValue = valueList.Min();
        _maxValue = valueList.Max();
        _step = null;
        _isDiscrete = true;
        _useShortStepFormat = false;
        _prefix = prefix;
    }

    /// <summary>
    /// Cron 字段种类
    /// </summary>
    public CrontabFieldKind Kind { get; }

    /// <summary>
    /// 判断当前时间是否符合 Cron 字段种类解析规则
    /// </summary>
    /// <param name="datetime">当前时间</param>
    /// <returns><see cref="bool"/></returns>
    public bool IsMatch(DateTime datetime)
    {
        // 获取当前时间在该字段上的值
        var currentValue = Kind switch
        {
            CrontabFieldKind.Second => datetime.Second,
            CrontabFieldKind.Minute => datetime.Minute,
            CrontabFieldKind.Hour => datetime.Hour,
            _ => throw new InvalidOperationException("RandomParser can only be used for Second, Minute or Hour fields.")
        };

        // 如果有候选集（步长或离散），则检查是否在候选集中
        if (_candidates != null)
        {
            return _candidates.Contains(currentValue);
        }

        // 否则检查是否在区间内
        return currentValue >= _minValue && currentValue <= _maxValue;
    }

    /// <summary>
    /// 获取 Cron 字段种类当前值的下一个发生值
    /// </summary>
    /// <param name="currentValue">时间值</param>
    /// <returns><see cref="int"/></returns>
    public int? Next(int currentValue)
    {
        return GetRandomValue();
    }

    /// <summary>
    /// 获取 Cron 字段种类当前值的上一个发生值
    /// </summary>
    /// <param name="currentValue">时间值</param>
    /// <returns><see cref="int"/></returns>
    public int? Previous(int currentValue)
    {
        return GetRandomValue();
    }

    /// <summary>
    /// 获取 Cron 字段种类字段起始值
    /// </summary>
    /// <remarks>
    /// 若存在候选集，则返回候选集的最小值；否则返回区间最小值。
    /// 该值主要用于调度算法在需要回退到字段起始时使用。
    /// </remarks>
    /// <returns><see cref="int"/></returns>
    public int First()
    {
        return _candidates != null ? _candidates.Min() : _minValue;
    }

    /// <summary>
    /// 获取 Cron 字段种类字段末尾值
    /// </summary>
    /// <remarks>
    /// 若存在候选集，则返回候选集的最大值；否则返回区间最大值。
    /// 该值主要用于调度算法在需要前推到字段末尾时使用。
    /// </remarks>
    /// <returns><see cref="int"/></returns>
    public int Last()
    {
        return _candidates != null ? _candidates.Max() : _maxValue;
    }

    /// <summary>
    /// 将解析器转换成字符串输出
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        // 离散值模式：R(1,5,10) 或 H(1,5,10)
        if (_isDiscrete)
        {
            return _prefix + "(" + string.Join(",", _candidates.Select(v => v.ToString()).ToArray()) + ")";
        }

        var fieldMin = Constants.MinimumDateTimeValues[Kind];
        var fieldMax = Constants.MaximumDateTimeValues[Kind];

        // 无步长情况
        if (!_step.HasValue)
        {
            // 如果区间等于字段全范围，简化为 "R" 或 "H"；否则输出 "R(min-max)" 或 "H(min-max)"
            return (_minValue == fieldMin && _maxValue == fieldMax) ? _prefix.ToString() : $"{_prefix}({_minValue}-{_maxValue})";
        }

        // 带步长情况
        if (_useShortStepFormat)
        {
            // 简洁格式：R/step 或 H/step
            return $"{_prefix}/{_step.Value}";
        }
        else
        {
            // 完整格式：R(min-max)/step 或 H(min-max)/step（无论区间是否全范围）
            return $"{_prefix}({_minValue}-{_maxValue})/{_step.Value}";
        }
    }

    /// <summary>
    /// 生成一个完全随机的值
    /// </summary>
    /// <remarks>
    /// 用于溢出重置时获取新随机值，或当需要全随机场景时使用。
    /// 如果存在候选集合（步长或离散），则随机选择一个索引返回对应的值；
    /// 否则在 [_minValue, _maxValue] 区间内直接随机生成一个整数。
    /// </remarks>
    /// <returns><see cref="int"/></returns>
    private int GetRandomValue()
    {
        if (_candidates != null)
        {
            // 从候选集合中均匀随机选取
            var index = GetRandom().Next(_candidates.Count);
            return _candidates[index];
        }

        // 区间内完全随机（含两端）
        return GetRandom().Next(_minValue, _maxValue + 1);
    }

    /// <summary>
    /// 外部使用的全随机接口
    /// </summary>
    /// <remarks>实际调用 <see cref="GetRandomValue"/>。</remarks>
    /// <returns><see cref="int"/></returns>
    internal int GetNextRandom()
    {
        return GetRandomValue();
    }
}