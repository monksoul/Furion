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
    /// 解析 Cron 表达式字段并存储其 所有发生值 字符解析器
    /// </summary>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型</param>
    /// <returns><see cref="Dictionary{TKey, TValue}"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    private static Dictionary<CrontabFieldKind, List<ICronParser>> ParseToDictionary(string expression, CronStringFormat format)
    {
        // Cron 表达式空检查
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new TimeCrontabException("The provided cron string is null, empty or contains only whitespace.");
        }

        // 通过空白符切割 Cron 表达式每个字段域
        var instructions = expression.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        // 验证当前 Cron 格式化类型字段数量和表达式字段数量是否一致
        var expectedCount = Constants.ExpectedFieldCounts[format];
        if (instructions.Length > expectedCount)
        {
            throw new TimeCrontabException(string.Format("The provided cron string <{0}> has too many parameters.", expression));
        }
        if (instructions.Length < expectedCount)
        {
            throw new TimeCrontabException(string.Format("The provided cron string <{0}> has too few parameters.", expression));
        }

        // 初始化字段偏移量和字段字符解析器
        var defaultFieldOffset = 0;
        var fieldParsers = new Dictionary<CrontabFieldKind, List<ICronParser>>();

        // 判断当前 Cron 格式化类型是否包含秒字段域，如果包含则优先解析秒字段域字符解析器
        if (format == CronStringFormat.WithSeconds || format == CronStringFormat.WithSecondsAndYears)
        {
            fieldParsers.Add(CrontabFieldKind.Second, ParseField(instructions[0], CrontabFieldKind.Second));
            defaultFieldOffset = 1;
        }

        // Cron 常规字段域
        fieldParsers.Add(CrontabFieldKind.Minute, ParseField(instructions[defaultFieldOffset + 0], CrontabFieldKind.Minute));   // 偏移量 1
        fieldParsers.Add(CrontabFieldKind.Hour, ParseField(instructions[defaultFieldOffset + 1], CrontabFieldKind.Hour));   // 偏移量 2
        fieldParsers.Add(CrontabFieldKind.Day, ParseField(instructions[defaultFieldOffset + 2], CrontabFieldKind.Day)); // 偏移量 3
        fieldParsers.Add(CrontabFieldKind.Month, ParseField(instructions[defaultFieldOffset + 3], CrontabFieldKind.Month)); // 偏移量 4
        fieldParsers.Add(CrontabFieldKind.DayOfWeek, ParseField(instructions[defaultFieldOffset + 4], CrontabFieldKind.DayOfWeek)); // 偏移量 5

        // 判断当前 Cron 格式化类型是否包含年字段域，如果包含则解析年字段域字符解析器
        if (format == CronStringFormat.WithYears || format == CronStringFormat.WithSecondsAndYears)
        {
            fieldParsers.Add(CrontabFieldKind.Year, ParseField(instructions[defaultFieldOffset + 5], CrontabFieldKind.Year));   // 偏移量 6
        }

        // 检查非法字符解析器，如 2 月没有 30 和 31 号
        CheckForIllegalParsers(fieldParsers);

        return fieldParsers;
    }

    /// <summary>
    /// 解析 Cron 单个字段域所有发生值 字符解析器
    /// </summary>
    /// <param name="field">字段值</param>
    /// <param name="kind">Cron 表达式格式化类型</param>
    /// <returns><see cref="List{T}"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    private static List<ICronParser> ParseField(string field, CrontabFieldKind kind)
    {
        /*
         * 在 Cron 表达式中，单个字段域值也支持定义多个值（我们称为值中值），如 1,2,3 或 SUN,FRI,SAT
         * 所以，这里需要将字段域值通过 , 进行切割后独立处理
         * 但特殊地，如果字段以 R 或 H 开头且包含括号，说明是 R 或 H 的范围或离散值模式（如 R(30-59) 或 H(1,5,10)），应整体解析，不分割。
         */

        try
        {
            // 处理 R 或 H 的范围/离散值模式：以 R 或 H 开头且包含 '('，直接整体解析
            if ((field.Trim().StartsWith("R", StringComparison.OrdinalIgnoreCase) || field.Trim().StartsWith("H", StringComparison.OrdinalIgnoreCase)) && field.Contains('('))
            {
                var parser = ParseParser(field, kind);
                return [parser];
            }

            var parsers = field.Split(',').Select(parser => ParseParser(parser, kind)).ToList();

            // 禁止 R 或 H 字符与其他值在同一字段内混用（例如 R,30 或 H(10-20),30 是非法的）
            if (parsers.Any(p => p is RandomParser) && parsers.Count > 1)
            {
                throw new TimeCrontabException(
                    string.Format("The 'R' or 'H' random parser cannot be combined with other values in the {0} field.",
                    Enum.GetName(typeof(CrontabFieldKind), kind)));
            }

            return parsers;
        }
        catch (Exception ex)
        {
            throw new TimeCrontabException(
                string.Format("There was an error parsing '{0}' for the {1} field.", field, Enum.GetName(typeof(CrontabFieldKind), kind))
                , ex);
        }
    }

    /// <summary>
    /// 解析 Cron 字段域值中值
    /// </summary>
    /// <param name="parser">字段值中值</param>
    /// <param name="kind">Cron 表达式格式化类型</param>
    /// <returns><see cref="ICronParser"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    private static ICronParser ParseParser(string parser, CrontabFieldKind kind)
    {
        // Cron 字段中所有字母均采用大写方式，所以需要转换所有为大写再操作
        var newParser = parser.ToUpperInvariant();

        try
        {
            // 判断值是否以 * 字符开头
            if (newParser.StartsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                // 继续往后解析
                newParser = newParser[1..];

                // 判断是否以 / 字符开头，如果是，则该值为带步长的 Cron 值
                if (newParser.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    // 继续往后解析
                    newParser = newParser[1..];

                    // 解析 Cron 值步长并创建 StepParser 解析器
                    var steps = GetValue(ref newParser, kind);
                    return new StepParser(0, steps, kind);
                }

                // 处理 * 携带意外值
                if (newParser != string.Empty)
                {
                    throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
                }

                // 否则，创建 AnyParser 解析器
                return new AnyParser(kind);
            }

            // 判断值是否以 L 字符开头
            if (newParser.StartsWith("L") && kind == CrontabFieldKind.Day)
            {
                // 继续往后解析
                newParser = newParser[1..];

                // 是否是 LW 字符，如果是，创建 LastWeekdayOfMonthParser 解析器
                if (newParser == "W")
                {
                    return new LastWeekdayOfMonthParser(kind);
                }
                // 否则创建 LastDayOfMonthParser 解析器
                else
                {
                    return new LastDayOfMonthParser(kind);
                }
            }

            // 判断值是否以 R 或 H 开头（支持全范围、区间、带步长、离散值、第 N 个随机星期）
            if (newParser.StartsWith("R") || newParser.StartsWith("H"))
            {
                var prefix = newParser[0]; // 'R' 或 'H'
                var remaining = newParser[1..];

                // 纯 "R" 或 "H"，全范围随机
                if (string.IsNullOrWhiteSpace(remaining))
                {
                    // 创建 RandomParser 解析器
                    return new RandomParser(kind, prefix);
                }

                // R#N 或 H#N 语法，仅 DayOfWeek 字段有效
                if (remaining.StartsWith("#"))
                {
                    if (kind != CrontabFieldKind.DayOfWeek)
                    {
                        throw new TimeCrontabException("The 'R#' or 'H#' parser can only be used in the Day of Week field.");
                    }

                    var weekNumberPart = remaining.Substring(1);
                    if (!int.TryParse(weekNumberPart, out var weekNumber) || weekNumber < 1 || weekNumber > 5)
                    {
                        throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
                    }

                    // 随机选定一个星期几（0-7，0=周日，7=周六）
                    var dayOfWeek = GetRandomDayOfWeek();
                    return new SpecificDayOfWeekInMonthParser(dayOfWeek, weekNumber, kind);
                }

                // 如果 remaining 以 '(' 开头，则包含范围或离散值
                if (remaining.StartsWith("("))
                {
                    var closingIndex = remaining.IndexOf(')');
                    if (closingIndex == -1)
                    {
                        throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
                    }

                    var inside = remaining.Substring(1, closingIndex - 1);  // 括号内的内容
                    var after = remaining.Substring(closingIndex + 1);  // 括号后的部分，如 /step

                    int? step = null;

                    // 处理括号后的步长部分
                    if (after.StartsWith("/"))
                    {
                        var stepPart = after.Substring(1);

                        // 步长必须是有效整数
                        if (!int.TryParse(stepPart, out var stepVal))
                        {
                            throw new TimeCrontabException(string.Format("Invalid step value in parser '{0}'.", parser));
                        }

                        step = stepVal;
                    }
                    else if (after != string.Empty)
                    {
                        throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
                    }

                    // 括号内如果包含逗号，则为离散值模式
                    if (inside.Contains(","))
                    {
                        // 尝试使用逗号分割剩余部分
                        var parts = inside.Split(',');
                        var values = new List<int>();

                        foreach (var part in parts)
                        {
                            // 尝试将每个部分解析为整数值
                            if (!int.TryParse(part, out var val))
                            {
                                throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
                            }
                            values.Add(val);
                        }

                        return new RandomParser(kind, values, prefix);
                    }
                    // 括号内如果包含 '-'，则为区间模式
                    else if (inside.Contains('-'))
                    {
                        // 找到 "-" 的位置，分割出 min 和 后续部分（max 可能含有 /step）
                        var dashIndex = inside.IndexOf('-');
                        var minPart = inside[..dashIndex];
                        var maxPart = inside[(dashIndex + 1)..];

                        // 解析 min 和 max，必须都是整数
                        if (!int.TryParse(minPart, out var minValue) || !int.TryParse(maxPart, out var maxValue))
                        {
                            throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
                        }

                        // 创建 RandomParser 解析器
                        return new RandomParser(kind, minValue, maxValue, step, false, prefix);
                    }
                    else
                    {
                        throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
                    }
                }
                // 以 "/" 开头说明是 R/step 或 H/step 形式，全范围带步长
                else if (remaining.StartsWith("/"))
                {
                    var stepPart = remaining[1..];
                    if (!int.TryParse(stepPart, out var stepVal))
                    {
                        throw new TimeCrontabException(string.Format("Invalid step value in parser '{0}'.", parser));
                    }

                    // 创建 RandomParser 解析器
                    return new RandomParser(kind,
                        Constants.MinimumDateTimeValues[kind],
                        Constants.MaximumDateTimeValues[kind],
                        stepVal,
                        useShortStepFormat: true,
                        prefix: prefix);
                }
                else
                {
                    // 无法识别的 R 或 H 格式，如 "Rabc"
                    throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
                }
            }

            // 判断值是否等于 ?
            if (newParser == "?")
            {
                // 创建 BlankDayOfMonthOrWeekParser 解析器
                return new BlankDayOfMonthOrWeekParser(kind);
            }

            /*
             * 如果上面均不匹配，那么该值类似取值有：2，1/2，1-10，1-10/2，SUN，SUNDAY，SUNL，JAN，3W，3L，2#5 等
             */

            // 继续推进解析
            var firstValue = GetValue(ref newParser, kind);

            // 如果没有返回新的待解析字符，则认为这是一个具体值
            if (string.IsNullOrWhiteSpace(newParser))
            {
                // 对年份进行特别处理
                if (kind == CrontabFieldKind.Year)
                {
                    return new SpecificYearParser(firstValue, kind);
                }
                else
                {
                    // 创建 SpecificParser 解析器
                    return new SpecificParser(firstValue, kind);
                }
            }

            // 如果存在待解析字符，如 - / # L W 值，则进一步解析
            switch (newParser[0])
            {
                // 判断值是否以 / 字符开头
                case '/':
                    {
                        // 继续往后解析
                        newParser = newParser[1..];

                        // 解析 Cron 值步长并创建 StepParser 解析器
                        var steps = GetValue(ref newParser, kind);
                        return new StepParser(firstValue, steps, kind);
                    }
                // 判断值是否以 - 字符开头
                case '-':
                    {
                        // 继续往后解析
                        newParser = newParser[1..];

                        // 获取范围结束值
                        var endValue = GetValue(ref newParser, kind);
                        int? steps = null;

                        // 继续推进解析，判断是否以 / 开头，如果是，则获取步长
                        if (newParser.StartsWith("/"))
                        {
                            newParser = newParser[1..];
                            steps = GetValue(ref newParser, kind);
                        }

                        // 创建 RangeParser 解析器
                        return new RangeParser(firstValue, endValue, steps, kind);
                    }
                // 判断值是否以 # 字符开头
                case '#':
                    {
                        // 继续往后解析
                        newParser = newParser[1..];

                        // 获取第几个
                        var weekNumber = GetValue(ref newParser, kind);

                        // 继续推进解析，如果存在其他字符，则抛异常
                        if (!string.IsNullOrWhiteSpace(newParser))
                        {
                            throw new TimeCrontabException(string.Format("Invalid parser '{0}.'", parser));
                        }

                        // 创建 SpecificDayOfWeekInMonthParser 解析器
                        return new SpecificDayOfWeekInMonthParser(firstValue, weekNumber, kind);
                    }
                // 判断解析值是否等于 L 或 W
                default:
                    // 创建 LastDayOfWeekInMonthParser 解析器
                    if (newParser == "L" && kind == CrontabFieldKind.DayOfWeek)
                    {
                        return new LastDayOfWeekInMonthParser(firstValue, kind);
                    }
                    // 创建 NearestWeekdayParser 解析器
                    else if (newParser == "W" && kind == CrontabFieldKind.Day)
                    {
                        return new NearestWeekdayParser(firstValue, kind);
                    }
                    break;
            }

            throw new TimeCrontabException(string.Format("Invalid parser '{0}'.", parser));
        }
        catch (Exception ex)
        {
            throw new TimeCrontabException(string.Format("Invalid parser '{0}'. See inner exception for details.", parser), ex);
        }
    }

    /// <summary>
    /// 生成一个随机的星期几（0-6，0=周日，6=周六）
    /// </summary>
    /// <returns><see cref="int"/></returns>
    private static int GetRandomDayOfWeek()
    {
        var random = new Random(Guid.NewGuid().GetHashCode());
        return random.Next(0, 7); // 0 到 6 闭区间
    }

    /// <summary>
    /// 将 Cron 字段值中值进一步解析
    /// </summary>
    /// <param name="parser">当前解析值</param>
    /// <param name="kind">Cron 表达式格式化类型</param>
    /// <returns><see cref="int"/></returns>
    /// <exception cref="TimeCrontabException"></exception>
    private static int GetValue(ref string parser, CrontabFieldKind kind)
    {
        // 值空检查
        if (string.IsNullOrWhiteSpace(parser))
        {
            throw new TimeCrontabException("Expected number, but parser was empty.");
        }

        // 字符偏移量
        int offset;

        // 判断首个字符是数字还是字符串
        var isDigit = char.IsDigit(parser[0]);
        var isLetter = char.IsLetter(parser[0]);

        // 推进式遍历值并检查每一个字符，一旦出现类型不连贯则停止检查
        for (offset = 0; offset < parser.Length; offset++)
        {
            // 如果存在不连贯数字或字母则跳出循环
            if ((isDigit && !char.IsDigit(parser[offset])) || (isLetter && !char.IsLetter(parser[offset])))
            {
                break;
            }
        }

        var maximum = Constants.MaximumDateTimeValues[kind];

        // 前面连贯类型的值
        var valueToParse = parser[..offset];

        // 处理数字开头的连贯类型值
        if (int.TryParse(valueToParse, out var value))
        {
            // 导出下一轮待解析的值（依旧采用推进式）
            parser = parser[offset..];

            var returnValue = value;

            // 验证值范围
            if (returnValue > maximum)
            {
                throw new TimeCrontabException(string.Format("Value for {0} parser exceeded maximum value of {1}.", Enum.GetName(typeof(CrontabFieldKind), kind), maximum));
            }

            return returnValue;
        }
        // 处理字母开头的连贯类型值，通常认为这是一个单词，如SUN，JAN
        else
        {
            List<KeyValuePair<string, int>> replaceVal = null;

            // 判断当前 Cron 字段类型是否是星期，如果是，则查找该单词是否在 Constants.Days 定义之中
            if (kind == CrontabFieldKind.DayOfWeek)
            {
                replaceVal = Constants.Days.Where(x => valueToParse.StartsWith(x.Key)).ToList();
            }
            // 判断当前 Cron 字段类型是否是月份，如果是，则查找该单词是否在 Constants.Months 定义之中
            else if (kind == CrontabFieldKind.Month)
            {
                replaceVal = Constants.Months.Where(x => valueToParse.StartsWith(x.Key)).ToList();
            }

            // 如果存在且唯一，则进入下一轮判断
            // 接下来的判断是处理 SUN + L 的情况，如 SUNL == 0L == SUNDAY，它们都是合法的 Cron 值
            if (replaceVal != null && replaceVal.Count == 1)
            {
                var missingParser = "";

                // 处理带 L 和不带 L 的单词问题
                if (parser.Length == offset
                    && parser.EndsWith("L")
                    && kind == CrontabFieldKind.DayOfWeek)
                {
                    missingParser = "L";
                }
                parser = parser[offset..] + missingParser;

                // 转换成 int 值返回（SUN，JAN.....）
                var returnValue = replaceVal.First().Value;

                // 验证值范围
                if (returnValue > maximum)
                {
                    throw new TimeCrontabException(string.Format("Value for {0} parser exceeded maximum value of {1}.", Enum.GetName(typeof(CrontabFieldKind), kind), maximum));
                }

                return returnValue;
            }
        }

        throw new TimeCrontabException("Parser does not contain expected number.");
    }

    /// <summary>
    /// 检查非法字符解析器，如 2 月没有 30 和 31 号
    /// </summary>
    /// <remarks>检查 2 月份是否存在 30 和 31 天的非法数值解析器</remarks>
    /// <param name="parsers">Cron 字段解析器字典集合</param>
    /// <exception cref="TimeCrontabException"></exception>
    private static void CheckForIllegalParsers(Dictionary<CrontabFieldKind, List<ICronParser>> parsers)
    {
        // 获取当前 Cron 表达式月字段和天字段所有数值
        var monthSingle = GetSpecificParsers(parsers, CrontabFieldKind.Month);
        var daySingle = GetSpecificParsers(parsers, CrontabFieldKind.Day);

        // 如果月份为 2 月单天数出现 30 和 31 天，则是无效数值
        if (monthSingle.Count != 0 && monthSingle.All(x => x.SpecificValue == 2))
        {
            if (daySingle.Count != 0 && daySingle.All(x => (x.SpecificValue == 30) || (x.SpecificValue == 31)))
            {
                throw new TimeCrontabException("The February 30 and 31 don't exist.");
            }
        }
    }

    /// <summary>
    /// 查找 Cron 字段类型所有具体值解析器
    /// </summary>
    /// <param name="parsers">Cron 字段解析器字典集合</param>
    /// <param name="kind">Cron 字段种类</param>
    /// <returns><see cref="List{T}"/></returns>
    private static List<SpecificParser> GetSpecificParsers(Dictionary<CrontabFieldKind, List<ICronParser>> parsers, CrontabFieldKind kind)
    {
        var kindParsers = parsers[kind];

        // 查找 Cron 字段类型所有具体值解析器
        return kindParsers.Where(x => x.GetType() == typeof(SpecificParser)).Cast<SpecificParser>()
            .Union(
            kindParsers.Where(x => x.GetType() == typeof(RangeParser)).SelectMany(x => ((RangeParser)x).SpecificParsers)
            ).Union(
                kindParsers.Where(x => x.GetType() == typeof(StepParser)).SelectMany(x => ((StepParser)x).SpecificParsers)
            ).ToList();
    }

    /// <summary>
    /// 获取特定时间范围下一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns><see cref="DateTime"/></returns>
    private DateTime InternalGetNextOccurence(DateTime baseTime, DateTime endTime)
    {
        // 判断当前 Cron 格式化类型是否支持秒
        var isSecondFormat = Format == CronStringFormat.WithSeconds || Format == CronStringFormat.WithSecondsAndYears;

        // 由于 Cron 格式化类型不包含毫秒，则裁剪掉毫秒部分
        var newValue = baseTime;
        newValue = newValue.AddMilliseconds(-newValue.Millisecond);

        // 如果当前 Cron 格式化类型不支持秒，则裁剪掉秒部分
        if (!isSecondFormat)
        {
            newValue = newValue.AddSeconds(-newValue.Second);
        }

        // 初始化是否存在随机 R 标识符
        var randomSecond = false;
        var randomMinute = false;
        var randomHour = false;

        // 获取分钟、小时所有字符解析器
        var minuteParsers = Parsers[CrontabFieldKind.Minute].Where(x => x is ITimeParser).Cast<ITimeParser>().ToList();
        randomMinute = minuteParsers.OfType<RandomParser>().Any();
        var hourParsers = Parsers[CrontabFieldKind.Hour].Where(x => x is ITimeParser).Cast<ITimeParser>().ToList();
        randomHour = hourParsers.OfType<RandomParser>().Any();

        // 获取秒、分钟、小时解析器中最小起始值
        // 该值主要用来获取下一个发生值的输入参数
        var firstSecondValue = newValue.Second;
        var firstMinuteValue = minuteParsers.Select(x => x.First()).Min();
        var firstHourValue = hourParsers.Select(x => x.First()).Min();

        // 定义一个标识，标识当前时间下一个发生时间值是否进入新一轮循环
        // 如：如果当前时间的秒数为 59，那么下一个秒数应该为 00，那么当前时间分钟就应该 +1
        // 以此类推，如果 +1 后分钟数为 59，那么下一个分钟数也应该为 00，那么当前时间小时数就应该 +1
        // ....
        var overflow = true;

        // 处理 Cron 格式化类型包含秒的情况 =================================================================
        var newSeconds = newValue.Second;
        if (isSecondFormat)
        {
            // 获取秒所有字符解析器
            var secondParsers = Parsers[CrontabFieldKind.Second].Where(x => x is ITimeParser).Cast<ITimeParser>().ToList();
            randomSecond = secondParsers.OfType<RandomParser>().Any();

            // 获取秒解析器最小起始值
            firstSecondValue = secondParsers.Select(x => x.First()).Min();

            // 获取秒下一个发生值
            newSeconds = Increment(secondParsers, newValue.Second, firstSecondValue, out overflow);

            // 设置起始时间为下一个秒时间
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newValue.Minute, newSeconds);

            // 如果当前秒并没有进入下一轮循环但存在不匹配的字符过滤器
            if (!overflow && !IsMatch(newValue))
            {
                // 重置秒为起始值并标记 overflow 为 true 进入新一轮循环
                // 若秒字段为随机字段，则重新生成随机值，避免固定值导致的单一分布
                newSeconds = randomSecond ? GetRandomFieldValue(CrontabFieldKind.Second) : firstSecondValue;

                // 此时计算时间秒部分应该为起始值
                // 如 22:10:59 -> 22:10:00
                newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newValue.Minute, newSeconds);

                // 标记进入下一轮循环
                overflow = true;
            }

            // 如果秒溢出且秒字段为随机字段，需要立即重新随机秒值，确保进入下一分钟时秒值也是随机的
            if (overflow && randomSecond)
            {
                newSeconds = GetRandomFieldValue(CrontabFieldKind.Second);
                newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newValue.Minute, newSeconds);
            }

            // 如果程序到达这里，说明并没有进入上面分支，则直接返回下一秒时间
            if (!overflow)
            {
                return MinDate(newValue, endTime);
            }
        }

        // 程序到达这里，说明秒部分已经标识进入新一轮循环，那么分支就应该获取下一个分钟发生值 =================================================================
        var newMinutes = Increment(minuteParsers, newValue.Minute + (overflow ? 0 : -1), firstMinuteValue, out overflow);

        // 设置起始时间为下一个分钟时间
        newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newMinutes, overflow ? firstSecondValue : newSeconds);

        // 如果当前分钟并没有进入下一轮循环但存在不匹配的字符过滤器
        if (!overflow && !IsMatch(newValue))
        {
            // 重置秒，分钟为起始值并标记 overflow 为 true 进入新一轮循环
            newSeconds = randomSecond ? GetRandomFieldValue(CrontabFieldKind.Second) : firstSecondValue;
            newMinutes = randomMinute ? GetRandomFieldValue(CrontabFieldKind.Minute) : firstMinuteValue;

            // 此时计算时间秒和分钟部分应该为起始值
            // 如 22:59:59 -> 22:00:00
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newMinutes, newSeconds);

            // 标记进入下一轮循环
            overflow = true;
        }

        // 如果分钟溢出且分钟字段为随机字段，需要立即重新随机分钟值，确保进入下一小时时分钟值也是随机的
        if (overflow && randomMinute)
        {
            newMinutes = GetRandomFieldValue(CrontabFieldKind.Minute);
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newMinutes, newValue.Second);
        }

        // 如果程序到达这里，说明并没有进入上面分支，则直接返回下一分钟时间
        if (!overflow)
        {
            return MinDate(newValue, endTime);
        }

        // 程序到达这里，说明分钟部分已经标识进入新一轮循环，那么分支就应该获取下一个小时发生值 =================================================================
        var newHours = Increment(hourParsers, newValue.Hour + (overflow ? 0 : -1), firstHourValue, out overflow);

        // 设置起始时间为下一个小时时间
        newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newHours,
            overflow && !randomMinute ? firstMinuteValue : (randomMinute ? GetRandomFieldValue(CrontabFieldKind.Minute) : newMinutes),
            overflow && !randomSecond ? firstSecondValue : (randomSecond ? GetRandomFieldValue(CrontabFieldKind.Second) : newSeconds));

        // 如果当前小时并没有进入下一轮循环但存在不匹配的字符过滤器
        if (!overflow && !IsMatch(newValue))
        {
            // 此时计算时间秒，分钟和小时部分应该为起始值
            // 如 23:59:59 -> 23:00:00
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day,
                randomHour ? GetRandomFieldValue(CrontabFieldKind.Hour) : firstHourValue,
                randomMinute ? GetRandomFieldValue(CrontabFieldKind.Minute) : firstMinuteValue,
                randomSecond ? GetRandomFieldValue(CrontabFieldKind.Second) : firstSecondValue);

            // 标记进入下一轮循环
            overflow = true;
        }

        // 如果小时溢出且小时字段为随机字段，需要立即重新随机小时值，确保进入下一天时小时值也是随机的
        if (overflow && randomHour)
        {
            newHours = GetRandomFieldValue(CrontabFieldKind.Hour);
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newHours, newValue.Minute, newValue.Second);
        }

        // 如果程序到达这里，说明并没有进入上面分支，则直接返回下一小时时间
        if (!randomHour && !overflow)
        {
            return MinDate(newValue, endTime);
        }

        // 如果程序达到这里，说明天数变了（一旦天数变了，那么月份可能也变了，星期可能也变了，年份也可能变了）
        // 所以这里的计算最为复杂
        List<ITimeParser> yearParsers = null;

        // 首先先判断当前 Cron 格式化类型是否支持年份
        var isYearFormat = Format == CronStringFormat.WithYears || Format == CronStringFormat.WithSecondsAndYears;

        // 如果支持，读取年份字符过滤器
        if (isYearFormat)
        {
            yearParsers = Parsers[CrontabFieldKind.Year].Where(x => x is ITimeParser).Cast<ITimeParser>().ToList();
        }

        // 程序能够执行到这里，那么说明时间已经是 23:59:59，所以起始时间追加 1 天
        // 这里的代码看起来很奇怪，但是是为了处理终止时间为 12/31/9999 23:59:59.999 的情况，也就是世界末日了~~~
        try
        {
            newValue = newValue.AddDays(1);
        }
        catch
        {
            return endTime;
        }

        // 在有效的年份时间内死循环至天、周、月、年全部匹配才终止循环
        while (!(IsMatch(newValue, CrontabFieldKind.Day)
            && IsMatch(newValue, CrontabFieldKind.DayOfWeek)
            && IsMatch(newValue, CrontabFieldKind.Month)
            && (!isYearFormat || IsMatch(newValue, CrontabFieldKind.Year))))
        {
            // 如果当前匹配到的时间已经大于或等于终止时间，则直接返回
            if (newValue >= endTime)
            {
                return MinDate(newValue, endTime);
            }

            // 如果 Cron 年份字段域获取下一个发生值为 null，那么直接返回 终止时间
            // 也就是已经没有匹配项了
            if (isYearFormat && yearParsers!.Select(x => x.Next(newValue.Year - 1)).All(x => x == null))
            {
                return endTime;
            }

            // 同样防止终止时间为 12/31/9999 23:59:59.999 的情况
            try
            {
                // 不断增加 1 天直至匹配成功
                newValue = newValue.AddDays(1);
            }
            catch
            {
                return endTime;
            }
        }

        return MinDate(newValue, endTime);
    }

    /// <summary>
    /// 获取特定时间范围上一个发生时间
    /// </summary>
    /// <param name="baseTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns><see cref="DateTime"/></returns>
    private DateTime InternalGetPreviousOccurence(DateTime baseTime, DateTime endTime)
    {
        // 判断当前 Cron 格式化类型是否支持秒
        var isSecondFormat = Format == CronStringFormat.WithSeconds || Format == CronStringFormat.WithSecondsAndYears;

        // 由于 Cron 格式化类型不包含毫秒，则裁剪掉毫秒部分
        var newValue = baseTime;
        newValue = newValue.AddMilliseconds(-newValue.Millisecond);

        // 如果当前 Cron 格式化类型不支持秒，则裁剪掉秒部分
        if (!isSecondFormat)
        {
            newValue = newValue.AddSeconds(-newValue.Second);
        }

        // 获取分钟、小时所有字符解析器
        var minuteParsers = Parsers[CrontabFieldKind.Minute].Where(x => x is ITimeParser).Cast<ITimeParser>().ToList();
        var hourParsers = Parsers[CrontabFieldKind.Hour].Where(x => x is ITimeParser).Cast<ITimeParser>().ToList();
        var randomMinute = minuteParsers.OfType<RandomParser>().Any();
        var randomHour = hourParsers.OfType<RandomParser>().Any();

        // 获取秒、分钟、小时解析器中最小起始值
        // 该值主要用来获取上一个发生值的输入参数
        var lastSecondValue = newValue.Second;
        var lastMinuteValue = minuteParsers.Select(x => x.Last()).Max();
        var lastHourValue = hourParsers.Select(x => x.Last()).Max();

        // 定义一个标识，标识当前时间上一个发生时间值是否进入新一轮循环
        // 如：如果当前时间的秒数为 00，那么上一个秒数应该为 59，那么当前时间分钟就应该 -1
        // 以此类推，如果 -1 后分钟数为 00，那么上一个分钟数也应该为 59，那么当前时间小时数就应该 -1
        // ....
        var overflow = true;

        // 处理 Cron 格式化类型包含秒的情况 =================================================================
        var newSeconds = newValue.Second;
        if (isSecondFormat)
        {
            // 获取秒所有字符解析器
            var secondParsers = Parsers[CrontabFieldKind.Second].Where(x => x is ITimeParser).Cast<ITimeParser>().ToList();
            var randomSecond = secondParsers.OfType<RandomParser>().Any();

            // 获取秒解析器最大末尾值
            lastSecondValue = secondParsers.Select(x => x.Last()).Max();

            // 获取秒上一个发生值
            newSeconds = Decrement(secondParsers, newValue.Second, lastSecondValue, out overflow);

            // 设置起始时间为上一个秒时间
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newValue.Minute, newSeconds);

            // 如果当前秒并没有进入下一轮循环但存在不匹配的字符过滤器
            if (!overflow && !IsMatch(newValue))
            {
                // 重置秒为起始值并标记 overflow 为 true 进入新一轮循环
                // 若秒字段为随机字段，则重新生成随机值，避免固定值导致的单一分布
                newSeconds = randomSecond ? GetRandomFieldValue(CrontabFieldKind.Second) : lastSecondValue;

                // 此时计算时间秒部分应该为末尾值
                // 如 22:10:00 -> 22:09:59
                newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newValue.Minute, newSeconds);

                // 标记进入下一轮循环
                overflow = true;
            }

            // 如果秒溢出且秒字段为随机字段，需要立即重新随机秒值，确保进入上一分钟时秒值也是随机的
            if (overflow && randomSecond)
            {
                newSeconds = GetRandomFieldValue(CrontabFieldKind.Second);
                newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newValue.Minute, newSeconds);
            }

            // 如果程序到达这里，说明并没有进入上面分支，则直接返回上一秒时间
            if (!overflow)
            {
                return MaxDate(newValue, endTime);
            }
        }

        // 程序到达这里，说明秒部分已经标识进入新一轮循环，那么分支就应该获取上一个分钟发生值 =================================================================
        var newMinutes = Decrement(minuteParsers, newValue.Minute + (overflow ? 0 : 1), lastMinuteValue, out overflow);

        // 设置起始时间为上一个分钟时间
        newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newMinutes, overflow ? lastSecondValue : newSeconds);

        // 如果当前分钟并没有进入下一轮循环但存在不匹配的字符过滤器
        if (!overflow && !IsMatch(newValue))
        {
            // 重置秒，分钟为起始值并标记 overflow 为 true 进入新一轮循环
            newSeconds = isSecondFormat && Parsers[CrontabFieldKind.Second].Any(p => p is RandomParser) ? GetRandomFieldValue(CrontabFieldKind.Second) : lastSecondValue;
            newMinutes = randomMinute ? GetRandomFieldValue(CrontabFieldKind.Minute) : lastMinuteValue;

            // 此时计算时间秒和分钟部分应该为起始值
            // 如 22:00:00 -> 21:59:59
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newMinutes, newSeconds);

            // 标记进入下一轮循环
            overflow = true;
        }

        // 如果分钟溢出且分钟字段为随机字段，需要立即重新随机分钟值，确保进入上一小时时分钟值也是随机的
        if (overflow && randomMinute)
        {
            newMinutes = GetRandomFieldValue(CrontabFieldKind.Minute);
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newValue.Hour, newMinutes, newValue.Second);
        }

        // 如果程序到达这里，说明并没有进入上面分支，则直接返回上一分钟时间
        if (!overflow)
        {
            return MaxDate(newValue, endTime);
        }

        // 程序到达这里，说明分钟部分已经标识进入新一轮循环，那么分支就应该获取上一个小时发生值 =================================================================
        var newHours = Decrement(hourParsers, newValue.Hour + (overflow ? 0 : 1), lastHourValue, out overflow);

        // 设置起始时间为上一个小时时间
        newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newHours,
            overflow ? (randomMinute ? GetRandomFieldValue(CrontabFieldKind.Minute) : lastMinuteValue) : newMinutes,
            overflow ? (isSecondFormat && Parsers[CrontabFieldKind.Second].Any(p => p is RandomParser) ? GetRandomFieldValue(CrontabFieldKind.Second) : lastSecondValue) : newSeconds);

        // 如果当前小时并没有进入下一轮循环但存在不匹配的字符过滤器
        if (!overflow && !IsMatch(newValue))
        {
            // 此时计算时间秒，分钟和小时部分应该为末尾值
            // 如 24:00:00 -> 23:59:59
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day,
                randomHour ? GetRandomFieldValue(CrontabFieldKind.Hour) : lastHourValue,
                randomMinute ? GetRandomFieldValue(CrontabFieldKind.Minute) : lastMinuteValue,
                isSecondFormat && Parsers[CrontabFieldKind.Second].Any(p => p is RandomParser) ? GetRandomFieldValue(CrontabFieldKind.Second) : lastSecondValue);

            // 标记进入下一轮循环
            overflow = true;
        }

        // 如果小时溢出且小时字段为随机字段，需要立即重新随机小时值，确保进入上一天时小时值也是随机的
        if (overflow && randomHour)
        {
            newHours = GetRandomFieldValue(CrontabFieldKind.Hour);
            newValue = new DateTime(newValue.Year, newValue.Month, newValue.Day, newHours, newValue.Minute, newValue.Second);
        }

        // 如果程序到达这里，说明并没有进入上面分支，则直接返回上一小时时间
        if (!overflow)
        {
            return MaxDate(newValue, endTime);
        }

        // 如果程序达到这里，说明天数变了（一旦天数变了，那么月份可能也变了，星期可能也变了，年份也可能变了）
        // 所以这里的计算最为复杂
        List<ITimeParser> yearParsers = null;

        // 首先先判断当前 Cron 格式化类型是否支持年份
        var isYearFormat = Format == CronStringFormat.WithYears || Format == CronStringFormat.WithSecondsAndYears;

        // 如果支持，读取年份字符过滤器
        if (isYearFormat)
        {
            yearParsers = Parsers[CrontabFieldKind.Year].Where(x => x is ITimeParser).Cast<ITimeParser>().ToList();
        }

        // 程序能够执行到这里，那么说明时间已经是 24:00:00，所以起始时间减 1 天
        // 这里的代码看起来很奇怪，但是是为了处理终止时间为 12/31/9999 23:59:59.999 的情况，也就是世界末日了~~~
        try
        {
            newValue = newValue.AddDays(-1);
        }
        catch
        {
            return endTime;
        }

        // 在有效的年份时间内死循环至天、周、月、年全部匹配才终止循环
        while (!(IsMatch(newValue, CrontabFieldKind.Day)
            && IsMatch(newValue, CrontabFieldKind.DayOfWeek)
            && IsMatch(newValue, CrontabFieldKind.Month)
            && (!isYearFormat || IsMatch(newValue, CrontabFieldKind.Year))))
        {
            // 如果当前匹配到的时间已经大于或等于终止时间，则直接返回
            if (newValue <= endTime)
            {
                return MaxDate(newValue, endTime);
            }

            // 如果 Cron 年份字段域获取下一个发生值为 null，那么直接返回 终止时间
            // 也就是已经没有匹配项了
            if (isYearFormat && yearParsers.Select(x => x.Previous(newValue.Year + 1)).All(x => x == null))
            {
                return endTime;
            }

            // 同样防止终止时间为 12/31/9999 23:59:59.999 的情况
            try
            {
                newValue = newValue.AddDays(-1);
            }
            catch
            {
                return endTime;
            }
        }

        return MaxDate(newValue, endTime);
    }

    /// <summary>
    /// 获取当前时间解析器下一个发生值
    /// </summary>
    /// <param name="parsers">解析器</param>
    /// <param name="value">当前值</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="overflow">控制秒、分钟、小时到达59秒/分和23小时开关</param>
    /// <returns><see cref="int"/></returns>
    private static int Increment(List<ITimeParser> parsers, int value, int defaultValue, out bool overflow)
    {
        // 检查是否是随机 R 字符解析器
        if (parsers.Count == 1 && parsers.First() is RandomParser randomParser)
        {
            // 获取区间最小值（或候选集最小值）
            var minValue = randomParser.First();

            // 如果当前值小于最小值，说明本时间单位内尚未触发，可以留在本字段
            if (value < minValue)
            {
                // 返回一个完全随机的值（从完整区间或候选集随机），本时间单位只触发这一次
                overflow = false;
                return randomParser.GetNextRandom();
            }
            else
            {
                // 否则本时间单位已经触发过，必须进位到下一时间单位
                overflow = true;
                return defaultValue;
            }
        }

        var nextValue = parsers.Select(x => x.Next(value))
            .Where(x => x > value)
            .Min()
            ?? defaultValue;

        // 如果此时秒或分钟或23到达最大值，则应该返回起始值
        overflow = nextValue <= value;
        return nextValue;
    }

    /// <summary>
    /// 获取当前时间解析器上一个发生值
    /// </summary>
    /// <param name="parsers">解析器</param>
    /// <param name="value">当前值</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="overflow">控制秒、分钟、小时到达59秒/分和23小时开关</param>
    /// <returns><see cref="int"/></returns>
    private int Decrement(List<ITimeParser> parsers, int value, int defaultValue, out bool overflow)
    {
        // 检查是否是随机 R 字符解析器
        if (parsers.Count == 1 && parsers.First() is RandomParser randomParser)
        {
            // 获取区间最大值（或候选集最大值）
            var maxValue = randomParser.Last();

            // 如果当前值大于最大值，说明本时间单位内尚未触发，可以留在本字段
            if (value > maxValue)
            {
                // 返回一个完全随机的值（从完整区间或候选集随机），本时间单位只触发这一次
                overflow = false;
                return randomParser.GetNextRandom();
            }
            else
            {
                // 否则本时间单位已经触发过，必须进位到上一时间单位
                overflow = true;
                return defaultValue;
            }
        }

        var previousValue = parsers.Select(x => x.Previous(value))
            .Where(x => x < value)
            .Max()
            ?? defaultValue;

        // 如果此时秒或分钟或00到达最小值，则应该返回末尾值
        overflow = previousValue >= value;
        return previousValue;
    }

    /// <summary>
    /// 处理下一个发生时间边界值
    /// </summary>
    /// <remarks>如果发生时间大于终止时间，则返回终止时间，否则返回发生时间</remarks>
    /// <param name="newTime">下一个发生时间</param>
    /// <param name="endTime">终止时间</param>
    /// <returns><see cref="DateTime"/></returns>
    private static DateTime MinDate(DateTime newTime, DateTime endTime)
    {
        return newTime >= endTime ? endTime : newTime;
    }

    /// <summary>
    /// 处理上一个发生时间边界值
    /// </summary>
    /// <remarks>如果发生时间小于终止时间，则返回终止时间，否则返回发生时间</remarks>
    /// <param name="newTime">下一个发生时间</param>
    /// <param name="endTime">终止时间</param>
    /// <returns><see cref="DateTime"/></returns>
    private DateTime MaxDate(DateTime newTime, DateTime endTime)
    {
        return newTime <= endTime ? endTime : newTime;
    }

    /// <summary>
    /// 判断 Cron 所有字段字符解析器是否都能匹配当前时间各个部分
    /// </summary>
    /// <param name="datetime">当前时间</param>
    /// <returns><see cref="bool"/></returns>
    private bool IsMatch(DateTime datetime)
    {
        return Parsers.All(fieldKind =>
            fieldKind.Value.Any(parser => parser.IsMatch(datetime))
        );
    }

    /// <summary>
    /// 判断当前 Cron 字段类型字符解析器和当前时间至少存在一种匹配
    /// </summary>
    /// <param name="datetime">当前时间</param>
    /// <param name="kind">Cron 字段种类</param>
    /// <returns></returns>
    private bool IsMatch(DateTime datetime, CrontabFieldKind kind)
    {
        return Parsers.Where(x => x.Key == kind)
            .SelectMany(x => x.Value)
            .Any(parser => parser.IsMatch(datetime));
    }

    /// <summary>
    /// 将 Cron 字段解析器转换成字符串
    /// </summary>
    /// <param name="paramList">Cron 字段字符串集合</param>
    /// <param name="kind">Cron 字段种类</param>
    private void JoinParsers(List<string> paramList, CrontabFieldKind kind)
    {
        paramList.Add(
            string.Join(",", Parsers
                .Where(x => x.Key == kind)
                .SelectMany(x => x.Value.Select(y => y.ToString())).ToArray()
            )
        );
    }

    /// <summary>
    /// 获取指定随机字段的一个新随机值
    /// </summary>
    /// <remarks>
    /// 当溢出重置字段时，若该字段为随机字段，则调用此方法重新获取一个随机值，
    /// 确保每次重置都能得到真正的随机分布，而非始终使用固定边界值。
    /// </remarks>
    /// <param name="kind">Cron 字段种类</param>
    /// <returns><see cref="int"/></returns>
    private int GetRandomFieldValue(CrontabFieldKind kind)
    {
        var randomParser = Parsers[kind].OfType<RandomParser>().FirstOrDefault();

        return randomParser?.GetNextRandom() ?? 0;
    }
}