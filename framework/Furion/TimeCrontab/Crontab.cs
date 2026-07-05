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
/// Cron 表达式抽象类
/// </summary>
/// <remarks>主要将 Cron 表达式转换成 OOP 类进行操作</remarks>
public sealed partial class Crontab
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <remarks>禁止外部 new 实例化</remarks>
    private Crontab()
    {
        Parsers = [];
        Format = CronStringFormat.Default;
    }

    /// <summary>
    /// Cron 字段解析器字典集合
    /// </summary>
    private Dictionary<CrontabFieldKind, List<ICronParser>> Parsers { get; set; }

    /// <summary>
    /// Cron 表达式格式化类型
    /// </summary>
    /// <remarks>禁止运行时更改</remarks>
    public CronStringFormat Format { get; private set; }

    /// <summary>
    /// 解析 Cron 表达式并转换成 <see cref="Crontab"/> 对象
    /// </summary>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型</param>
    /// <returns><see cref="Crontab"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    public static Crontab Parse(string expression, CronStringFormat format = CronStringFormat.Default)
    {
        // 处理 Macro 表达式
        if (expression.StartsWith('@'))
        {
            return expression.ToLowerInvariant() switch
            {
                "@secondly" => Secondly,
                "@minutely" => Minutely,
                "@hourly" => Hourly,
                "@daily" => Daily,
                "@monthly" => Monthly,
                "@weekly" => Weekly,
                "@yearly" => Yearly,
                "@weekdays" => Weekdays,
                "@weekends" => Weekends,
                _ => throw new NotImplementedException(),
            };
        }

        return new Crontab
        {
            Format = format,
            Parsers = ParseToDictionary(expression, format)
        };
    }

    /// <summary>
    /// 解析 Cron Macro 符号并转换成 <see cref="Crontab"/> 对象
    /// </summary>
    /// <param name="macro">Macro 符号</param>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="Crontab"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    public static Crontab ParseAt(string macro, params object[] fields)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(macro)) throw new ArgumentNullException(nameof(macro));

        return macro.ToLowerInvariant() switch
        {
            "@secondly" => SecondlyAt(fields),
            "@minutely" => MinutelyAt(fields),
            "@hourly" => HourlyAt(fields),
            "@daily" => DailyAt(fields),
            "@monthly" => MonthlyAt(fields),
            "@weekly" => WeeklyAt(fields),
            "@yearly" => YearlyAt(fields),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// 解析 Cron 表达式并转换成 <see cref="Crontab"/> 对象
    /// </summary>
    /// <remarks>解析失败返回 default</remarks>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型</param>
    /// <returns><see cref="Crontab"/></returns>
    public static Crontab TryParse(string expression, CronStringFormat format = CronStringFormat.Default)
    {
        try
        {
            return Parse(expression, format);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 判断 Cron 表达式是否有效
    /// </summary>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型</param>
    /// <returns><see cref="Crontab"/></returns>
    public static bool IsValid(string expression, CronStringFormat format = CronStringFormat.Default)
    {
        return TryParse(expression, format) != null;
    }

    /// <summary>
    /// 获取起始时间下一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <returns><see cref="DateTime"/></returns>
    public DateTime GetNextOccurrence(DateTime baseTime)
    {
        return GetNextOccurrence(baseTime, DateTime.MaxValue);
    }

    /// <summary>
    /// 获取起始时间上一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <returns><see cref="DateTime"/></returns>
    public DateTime GetPreviousOccurrence(DateTime baseTime)
    {
        return GetPreviousOccurrence(baseTime, DateTime.MinValue);
    }

    /// <summary>
    /// 获取特定时间范围下一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns><see cref="DateTime"/></returns>
    public DateTime GetNextOccurrence(DateTime baseTime, DateTime endTime)
    {
        return InternalGetNextOccurence(baseTime, endTime);
    }

    /// <summary>
    /// 获取特定时间范围上一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    public DateTime GetPreviousOccurrence(DateTime baseTime, DateTime endTime)
    {
        return InternalGetPreviousOccurence(baseTime, endTime);
    }

    /// <summary>
    /// 获取特定时间范围所有发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns><see cref="IEnumerable{T}"/></returns>
    public IEnumerable<DateTime> GetNextOccurrences(DateTime baseTime, DateTime endTime)
    {
        for (var occurrence = GetNextOccurrence(baseTime, endTime);
             occurrence < endTime;
             occurrence = GetNextOccurrence(occurrence, endTime))
        {
            yield return occurrence;
        }
    }

    /// <summary>
    /// 获取特定时间范围所有发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns><see cref="IEnumerable{T}"/></returns>
    public IEnumerable<DateTime> GetPreviousOccurrences(DateTime baseTime, DateTime endTime)
    {
        for (var occurrence = GetPreviousOccurrence(baseTime, endTime);
            occurrence > endTime;
            occurrence = GetPreviousOccurrence(occurrence, endTime))
        {
            yield return occurrence;
        }
    }

    /// <summary>
    /// 获取从起始时间开始指定数量的下一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="count">需要获取的发生时间数量（必须大于 0）</param>
    /// <returns>包含指定数量下一次发生时间的集合</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IEnumerable<DateTime> GetNextOccurrences(DateTime baseTime, int count)
    {
        // 检查数量是否大于 0
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        var next = baseTime;

        for (var i = 0; i < count; i++)
        {
            next = GetNextOccurrence(next);
            yield return next;
        }
    }

    /// <summary>
    /// 获取从起始时间开始指定数量的上一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="count">需要获取的发生时间数量（必须大于 0）</param>
    /// <returns>包含指定数量上一次发生时间的集合</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IEnumerable<DateTime> GetPreviousOccurrences(DateTime baseTime, int count)
    {
        // 检查数量是否大于 0
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");
        }

        var previous = baseTime;

        for (var i = 0; i < count; i++)
        {
            previous = GetPreviousOccurrence(previous);
            yield return previous;
        }
    }

    /// <summary>
    /// 计算距离下一个发生时间相差毫秒数
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <returns><see cref="double"/></returns>
    public double GetSleepMilliseconds(DateTime baseTime)
    {
        // 采用 DateTimeKind.Unspecified 转换当前时间并忽略毫秒之后部分
        var startAt = new DateTime(baseTime.Year
            , baseTime.Month
            , baseTime.Day
            , baseTime.Hour
            , baseTime.Minute
            , baseTime.Second
            , baseTime.Millisecond);

        // 计算总休眠时间
        return (GetNextOccurrence(startAt) - startAt).TotalMilliseconds;
    }

    /// <summary>
    /// 计算距离下一个发生时间相差时间戳
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <returns><see cref="TimeSpan"/></returns>
    public TimeSpan GetSleepTimeSpan(DateTime baseTime)
    {
        return TimeSpan.FromMilliseconds(GetSleepMilliseconds(baseTime));
    }

    /// <summary>
    /// 将 <see cref="Crontab"/> 对象转换成 Cron 表达式字符串
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        var paramList = new List<string>();

        // 判断当前 Cron 格式化类型是否包含秒字段域
        if (Format == CronStringFormat.WithSeconds || Format == CronStringFormat.WithSecondsAndYears)
        {
            JoinParsers(paramList, CrontabFieldKind.Second);
        }

        // Cron 常规字段域
        JoinParsers(paramList, CrontabFieldKind.Minute);
        JoinParsers(paramList, CrontabFieldKind.Hour);
        JoinParsers(paramList, CrontabFieldKind.Day);
        JoinParsers(paramList, CrontabFieldKind.Month);
        JoinParsers(paramList, CrontabFieldKind.DayOfWeek);

        // 判断当前 Cron 格式化类型是否包含年字段域
        if (Format == CronStringFormat.WithYears || Format == CronStringFormat.WithSecondsAndYears)
        {
            JoinParsers(paramList, CrontabFieldKind.Year);
        }

        // 空格分割并输出
        return string.Join(" ", paramList.ToArray());
    }
}