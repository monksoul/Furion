// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Furion.TaskScheduler
{
    /// <summary>
    /// Cron 表达式的解析器和调度程序
    /// <para>代码参考自：https://github.com/HangfireIO/Cronos </para>
    /// </summary>
    [SuppressSniffer]
    public sealed class CronExpression : IEquatable<CronExpression>
    {
        private const long NotFound = 0;

        private const int MinNthDayOfWeek = 1;
        private const int MaxNthDayOfWeek = 5;
        private const int SundayBits = 0b1000_0001;

        private const int MaxYear = 2099;

        private static readonly TimeZoneInfo UtcTimeZone = TimeZoneInfo.Utc;

        private static readonly CronExpression Yearly = Parse("0 0 1 1 *");
        private static readonly CronExpression Weekly = Parse("0 0 * * 0");
        private static readonly CronExpression Monthly = Parse("0 0 1 * *");
        private static readonly CronExpression Daily = Parse("0 0 * * *");
        private static readonly CronExpression Hourly = Parse("0 * * * *");
        private static readonly CronExpression Minutely = Parse("* * * * *");
        private static readonly CronExpression Secondly = Parse("* * * * * *", CronFormat.IncludeSeconds);

        private static readonly int[] DeBruijnPositions =
        {
            0, 1, 2, 53, 3, 7, 54, 27,
            4, 38, 41, 8, 34, 55, 48, 28,
            62, 5, 39, 46, 44, 42, 22, 9,
            24, 35, 59, 56, 49, 18, 29, 11,
            63, 52, 6, 26, 37, 40, 33, 47,
            61, 45, 43, 21, 23, 58, 17, 10,
            51, 25, 36, 32, 60, 20, 57, 16,
            50, 31, 19, 15, 30, 14, 13, 12
        };

        private long _second;     // 60 bits -> from 0 bit to 59 bit
        private long _minute;     // 60 bits -> from 0 bit to 59 bit
        private int _hour;       // 24 bits -> from 0 bit to 23 bit
        private int _dayOfMonth; // 31 bits -> from 1 bit to 31 bit
        private short _month;      // 12 bits -> from 1 bit to 12 bit
        private byte _dayOfWeek;  // 8 bits  -> from 0 bit to 7 bit

        private byte _nthDayOfWeek;
        private byte _lastMonthOffset;

        private CronExpressionFlag _flags;

        private CronExpression()
        {
        }

        ///<summary>
        /// Constructs a new <see cref="CronExpression"/> based on the specified
        /// cron expression. It's supported expressions consisting of 5 fields:
        /// minute, hour, day of month, month, day of week.
        /// If you want to parse non-standard cron expressions use <see cref="Parse(string, CronFormat)"/> with specified CronFields argument.
        /// See more: <a href="https://github.com/HangfireIO/Cronos">https://github.com/HangfireIO/Cronos</a>
        /// </summary>
        public static CronExpression Parse(string expression)
        {
            return Parse(expression, CronFormat.Standard);
        }

        ///<summary>
        /// Constructs a new <see cref="CronExpression"/> based on the specified
        /// cron expression. It's supported expressions consisting of 5 or 6 fields:
        /// second (optional), minute, hour, day of month, month, day of week.
        /// See more: <a href="https://github.com/HangfireIO/Cronos">https://github.com/HangfireIO/Cronos</a>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe CronExpression Parse(string expression, CronFormat format)
        {
            if (string.IsNullOrEmpty(expression)) throw new ArgumentNullException(nameof(expression));

            fixed (char* value = expression)
            {
                var pointer = value;

                SkipWhiteSpaces(ref pointer);

                CronExpression cronExpression;

                if (Accept(ref pointer, '@'))
                {
                    cronExpression = ParseMacro(ref pointer);
                    SkipWhiteSpaces(ref pointer);

                    if (cronExpression == null || !IsEndOfString(*pointer)) ThrowFormatException("Macro: Unexpected character '{0}' on position {1}.", *pointer, pointer - value);

                    return cronExpression;
                }

                cronExpression = new CronExpression();

                if (format == CronFormat.IncludeSeconds)
                {
                    cronExpression._second = ParseField(CronField.Seconds, ref pointer, ref cronExpression._flags);
                    ParseWhiteSpace(CronField.Seconds, ref pointer);
                }
                else
                {
                    SetBit(ref cronExpression._second, CronField.Seconds.First);
                }

                cronExpression._minute = ParseField(CronField.Minutes, ref pointer, ref cronExpression._flags);
                ParseWhiteSpace(CronField.Minutes, ref pointer);

                cronExpression._hour = (int)ParseField(CronField.Hours, ref pointer, ref cronExpression._flags);
                ParseWhiteSpace(CronField.Hours, ref pointer);

                cronExpression._dayOfMonth = (int)ParseDayOfMonth(ref pointer, ref cronExpression._flags, ref cronExpression._lastMonthOffset);
                ParseWhiteSpace(CronField.DaysOfMonth, ref pointer);

                cronExpression._month = (short)ParseField(CronField.Months, ref pointer, ref cronExpression._flags);
                ParseWhiteSpace(CronField.Months, ref pointer);

                cronExpression._dayOfWeek = (byte)ParseDayOfWeek(ref pointer, ref cronExpression._flags, ref cronExpression._nthDayOfWeek);
                ParseEndOfString(ref pointer);

                // Make sundays equivalent.
                if ((cronExpression._dayOfWeek & SundayBits) != 0)
                {
                    cronExpression._dayOfWeek |= SundayBits;
                }

                return cronExpression;
            }
        }

        /// <summary>
        /// Calculates next occurrence starting with <paramref name="fromUtc"/> (optionally <paramref name="inclusive"/>) in UTC time zone.
        /// </summary>
        public DateTime? GetNextOccurrence(DateTime fromUtc, bool inclusive = false)
        {
            if (fromUtc.Kind != DateTimeKind.Utc) ThrowWrongDateTimeKindException(nameof(fromUtc));

            var found = FindOccurrence(fromUtc.Ticks, inclusive);
            if (found == NotFound) return null;

            return new DateTime(found, DateTimeKind.Utc);
        }

        /// <summary>
        /// Returns the list of next occurrences within the given date/time range,
        /// including <paramref name="fromUtc"/> and excluding <paramref name="toUtc"/>
        /// by default, and UTC time zone. When none of the occurrences found, an
        /// empty list is returned.
        /// </summary>
        public IEnumerable<DateTime> GetOccurrences(
            DateTime fromUtc,
            DateTime toUtc,
            bool fromInclusive = true,
            bool toInclusive = false)
        {
            if (fromUtc > toUtc) ThrowFromShouldBeLessThanToException(nameof(fromUtc), nameof(toUtc));

            for (var occurrence = GetNextOccurrence(fromUtc, fromInclusive);
                occurrence < toUtc || occurrence == toUtc && toInclusive;
                // ReSharper disable once RedundantArgumentDefaultValue
                // ReSharper disable once ArgumentsStyleLiteral
                occurrence = GetNextOccurrence(occurrence.Value, inclusive: false))
            {
                yield return occurrence.Value;
            }
        }

        /// <summary>
        /// Calculates next occurrence starting with <paramref name="fromUtc"/> (optionally <paramref name="inclusive"/>) in given <paramref name="zone"/>
        /// </summary>
        public DateTime? GetNextOccurrence(DateTime fromUtc, TimeZoneInfo zone, bool inclusive = false)
        {
            if (fromUtc.Kind != DateTimeKind.Utc) ThrowWrongDateTimeKindException(nameof(fromUtc));

            if (ReferenceEquals(zone, UtcTimeZone))
            {
                var found = FindOccurrence(fromUtc.Ticks, inclusive);
                if (found == NotFound) return null;

                return new DateTime(found, DateTimeKind.Utc);
            }

            var fromOffset = new DateTimeOffset(fromUtc);

            var occurrence = GetOccurrenceConsideringTimeZone(fromOffset, zone, inclusive);

            return occurrence?.UtcDateTime;
        }

        /// <summary>
        /// Returns the list of next occurrences within the given date/time range, including
        /// <paramref name="fromUtc"/> and excluding <paramref name="toUtc"/> by default, and
        /// specified time zone. When none of the occurrences found, an empty list is returned.
        /// </summary>
        public IEnumerable<DateTime> GetOccurrences(
            DateTime fromUtc,
            DateTime toUtc,
            TimeZoneInfo zone,
            bool fromInclusive = true,
            bool toInclusive = false)
        {
            if (fromUtc > toUtc) ThrowFromShouldBeLessThanToException(nameof(fromUtc), nameof(toUtc));

            for (var occurrence = GetNextOccurrence(fromUtc, zone, fromInclusive);
                occurrence < toUtc || occurrence == toUtc && toInclusive;
                // ReSharper disable once RedundantArgumentDefaultValue
                // ReSharper disable once ArgumentsStyleLiteral
                occurrence = GetNextOccurrence(occurrence.Value, zone, inclusive: false))
            {
                yield return occurrence.Value;
            }
        }

        /// <summary>
        /// Calculates next occurrence starting with <paramref name="from"/> (optionally <paramref name="inclusive"/>) in given <paramref name="zone"/>
        /// </summary>
        public DateTimeOffset? GetNextOccurrence(DateTimeOffset from, TimeZoneInfo zone, bool inclusive = false)
        {
            if (ReferenceEquals(zone, UtcTimeZone))
            {
                var found = FindOccurrence(from.UtcTicks, inclusive);
                if (found == NotFound) return null;

                return new DateTimeOffset(found, TimeSpan.Zero);
            }

            return GetOccurrenceConsideringTimeZone(from, zone, inclusive);
        }

        /// <summary>
        /// Returns the list of occurrences within the given date/time offset range,
        /// including <paramref name="from"/> and excluding <paramref name="to"/> by
        /// default. When none of the occurrences found, an empty list is returned.
        /// </summary>
        public IEnumerable<DateTimeOffset> GetOccurrences(
            DateTimeOffset from,
            DateTimeOffset to,
            TimeZoneInfo zone,
            bool fromInclusive = true,
            bool toInclusive = false)
        {
            if (from > to) ThrowFromShouldBeLessThanToException(nameof(from), nameof(to));

            for (var occurrence = GetNextOccurrence(from, zone, fromInclusive);
                occurrence < to || occurrence == to && toInclusive;
                // ReSharper disable once RedundantArgumentDefaultValue
                // ReSharper disable once ArgumentsStyleLiteral
                occurrence = GetNextOccurrence(occurrence.Value, zone, inclusive: false))
            {
                yield return occurrence.Value;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var expressionBuilder = new StringBuilder();

            AppendFieldValue(expressionBuilder, CronField.Seconds, _second).Append(' ');
            AppendFieldValue(expressionBuilder, CronField.Minutes, _minute).Append(' ');
            AppendFieldValue(expressionBuilder, CronField.Hours, _hour).Append(' ');
            AppendDayOfMonth(expressionBuilder, _dayOfMonth).Append(' ');
            AppendFieldValue(expressionBuilder, CronField.Months, _month).Append(' ');
            AppendDayOfWeek(expressionBuilder, _dayOfWeek);

            return expressionBuilder.ToString();
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="object"/>.
        /// </summary>
        /// <param name="other">The <see cref="object"/> to compare with the current <see cref="object"/>.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="object"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CronExpression other)
        {
            if (other == null) return false;

            return _second == other._second &&
                   _minute == other._minute &&
                   _hour == other._hour &&
                   _dayOfMonth == other._dayOfMonth &&
                   _month == other._month &&
                   _dayOfWeek == other._dayOfWeek &&
                   _nthDayOfWeek == other._nthDayOfWeek &&
                   _lastMonthOffset == other._lastMonthOffset &&
                   _flags == other._flags;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object" /> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as CronExpression);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(_second);
            hash.Add(_minute);
            hash.Add(_hour);
            hash.Add(_dayOfMonth);
            hash.Add(_month);
            hash.Add(_dayOfWeek);
            hash.Add(_nthDayOfWeek);
            hash.Add(_lastMonthOffset);
            hash.Add(_flags);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(CronExpression left, CronExpression right) => Equals(left, right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(CronExpression left, CronExpression right) => !Equals(left, right);

        private DateTimeOffset? GetOccurrenceConsideringTimeZone(DateTimeOffset fromUtc, TimeZoneInfo zone, bool inclusive)
        {
            if (!DateTimeHelper.IsRound(fromUtc))
            {
                // Rarely, if fromUtc is very close to DST transition, `TimeZoneInfo.ConvertTime` may not convert it correctly on Windows.
                // E.g., In Jordan Time DST started 2017-03-31 00:00 local time. Clocks jump forward from `2017-03-31 00:00 +02:00` to `2017-03-31 01:00 +3:00`.
                // But `2017-03-30 23:59:59.9999000 +02:00` will be converted to `2017-03-31 00:59:59.9999000 +03:00` instead of `2017-03-30 23:59:59.9999000 +02:00` on Windows.
                // It can lead to skipped occurrences. To avoid such errors we floor fromUtc to seconds:
                // `2017-03-30 23:59:59.9999000 +02:00` will be floored to `2017-03-30 23:59:59.0000000 +02:00` and will be converted to `2017-03-30 23:59:59.0000000 +02:00`.
                fromUtc = DateTimeHelper.FloorToSeconds(fromUtc);
                inclusive = false;
            }

            var from = TimeZoneInfo.ConvertTime(fromUtc, zone);

            var fromLocal = from.DateTime;

            if (TimeZoneHelper.IsAmbiguousTime(zone, fromLocal))
            {
                var currentOffset = from.Offset;
                var standardOffset = zone.BaseUtcOffset;

                if (standardOffset != currentOffset)
                {
                    var daylightOffset = TimeZoneHelper.GetDaylightOffset(zone, fromLocal);
                    var daylightTimeLocalEnd = TimeZoneHelper.GetDaylightTimeEnd(zone, fromLocal, daylightOffset).DateTime;

                    // Early period, try to find anything here.
                    var foundInDaylightOffset = FindOccurrence(fromLocal.Ticks, daylightTimeLocalEnd.Ticks, inclusive);
                    if (foundInDaylightOffset != NotFound) return new DateTimeOffset(foundInDaylightOffset, daylightOffset);

                    fromLocal = TimeZoneHelper.GetStandardTimeStart(zone, fromLocal, daylightOffset).DateTime;
                    inclusive = true;
                }

                // Skip late ambiguous interval.
                var ambiguousIntervalLocalEnd = TimeZoneHelper.GetAmbiguousIntervalEnd(zone, fromLocal).DateTime;

                if (HasFlag(CronExpressionFlag.Interval))
                {
                    var foundInStandardOffset = FindOccurrence(fromLocal.Ticks, ambiguousIntervalLocalEnd.Ticks - 1, inclusive);
                    if (foundInStandardOffset != NotFound) return new DateTimeOffset(foundInStandardOffset, standardOffset);
                }

                fromLocal = ambiguousIntervalLocalEnd;
                inclusive = true;
            }

            var occurrenceTicks = FindOccurrence(fromLocal.Ticks, inclusive);
            if (occurrenceTicks == NotFound) return null;

            var occurrence = new DateTime(occurrenceTicks);

            if (zone.IsInvalidTime(occurrence))
            {
                var nextValidTime = TimeZoneHelper.GetDaylightTimeStart(zone, occurrence);
                return nextValidTime;
            }

            if (TimeZoneHelper.IsAmbiguousTime(zone, occurrence))
            {
                var daylightOffset = TimeZoneHelper.GetDaylightOffset(zone, occurrence);
                return new DateTimeOffset(occurrence, daylightOffset);
            }

            return new DateTimeOffset(occurrence, zone.GetUtcOffset(occurrence));
        }

        private long FindOccurrence(long startTimeTicks, long endTimeTicks, bool startInclusive)
        {
            var found = FindOccurrence(startTimeTicks, startInclusive);

            if (found == NotFound || found > endTimeTicks) return NotFound;
            return found;
        }

        private long FindOccurrence(long ticks, bool startInclusive)
        {
            if (!startInclusive) ticks++;

            CalendarHelper.FillDateTimeParts(
                ticks,
                out var startSecond,
                out var startMinute,
                out var startHour,
                out var startDay,
                out var startMonth,
                out var startYear);

            var minMatchedDay = GetFirstSet(_dayOfMonth);

            var second = startSecond;
            var minute = startMinute;
            var hour = startHour;
            var day = startDay;
            var month = startMonth;
            var year = startYear;

            if (!GetBit(_second, second) && !Move(_second, ref second)) minute++;
            if (!GetBit(_minute, minute) && !Move(_minute, ref minute)) hour++;
            if (!GetBit(_hour, hour) && !Move(_hour, ref hour)) day++;

            // If NearestWeekday flag is set it's possible forward shift.
            if (HasFlag(CronExpressionFlag.NearestWeekday)) day = CronField.DaysOfMonth.First;

            if (!GetBit(_dayOfMonth, day) && !Move(_dayOfMonth, ref day)) goto RetryMonth;
            if (!GetBit(_month, month)) goto RetryMonth;

            Retry:

            if (day > GetLastDayOfMonth(year, month)) goto RetryMonth;

            if (HasFlag(CronExpressionFlag.DayOfMonthLast)) day = GetLastDayOfMonth(year, month);

            var lastCheckedDay = day;

            if (HasFlag(CronExpressionFlag.NearestWeekday)) day = CalendarHelper.MoveToNearestWeekDay(year, month, day);

            if (IsDayOfWeekMatch(year, month, day))
            {
                if (CalendarHelper.IsGreaterThan(year, month, day, startYear, startMonth, startDay)) goto RolloverDay;
                if (hour > startHour) goto RolloverHour;
                if (minute > startMinute) goto RolloverMinute;
                goto ReturnResult;

            RolloverDay: hour = GetFirstSet(_hour);
            RolloverHour: minute = GetFirstSet(_minute);
            RolloverMinute: second = GetFirstSet(_second);

            ReturnResult:

                var found = CalendarHelper.DateTimeToTicks(year, month, day, hour, minute, second);
                if (found >= ticks) return found;
            }

            day = lastCheckedDay;
            if (Move(_dayOfMonth, ref day)) goto Retry;

            RetryMonth:

            if (!Move(_month, ref month) && ++year >= MaxYear) return NotFound;
            day = minMatchedDay;

            goto Retry;
        }

        private static bool Move(long fieldBits, ref int fieldValue)
        {
            if (fieldBits >> ++fieldValue == 0)
            {
                fieldValue = GetFirstSet(fieldBits);
                return false;
            }

            fieldValue += GetFirstSet(fieldBits >> fieldValue);
            return true;
        }

        private int GetLastDayOfMonth(int year, int month)
        {
            return CalendarHelper.GetDaysInMonth(year, month) - _lastMonthOffset;
        }

        private bool IsDayOfWeekMatch(int year, int month, int day)
        {
            if (HasFlag(CronExpressionFlag.DayOfWeekLast) && !CalendarHelper.IsLastDayOfWeek(year, month, day) ||
                HasFlag(CronExpressionFlag.NthDayOfWeek) && !CalendarHelper.IsNthDayOfWeek(day, _nthDayOfWeek))
            {
                return false;
            }

            if (_dayOfWeek == CronField.DaysOfWeek.AllBits) return true;

            var dayOfWeek = CalendarHelper.GetDayOfWeek(year, month, day);

            return ((_dayOfWeek >> (int)dayOfWeek) & 1) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetFirstSet(long value)
        {
            // TODO: Add description and source
            var res = unchecked((ulong)(value & -value) * 0x022fdd63cc95386d) >> 58;
            return DeBruijnPositions[res];
        }

        private bool HasFlag(CronExpressionFlag value)
        {
            return (_flags & value) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void SkipWhiteSpaces(ref char* pointer)
        {
            while (IsWhiteSpace(*pointer)) { pointer++; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ParseWhiteSpace(CronField prevField, ref char* pointer)
        {
            if (!IsWhiteSpace(*pointer)) ThrowFormatException(prevField, "Unexpected character '{0}'.", *pointer);
            SkipWhiteSpaces(ref pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ParseEndOfString(ref char* pointer)
        {
            if (!IsWhiteSpace(*pointer) && !IsEndOfString(*pointer)) ThrowFormatException(CronField.DaysOfWeek, "Unexpected character '{0}'.", *pointer);

            SkipWhiteSpaces(ref pointer);
            if (!IsEndOfString(*pointer)) ThrowFormatException("Unexpected character '{0}'.", *pointer);
        }

        private static unsafe CronExpression ParseMacro(ref char* pointer)
        {
            switch (ToUpper(*pointer++))
            {
                case 'A':
                    if (AcceptCharacter(ref pointer, 'N') &&
                        AcceptCharacter(ref pointer, 'N') &&
                        AcceptCharacter(ref pointer, 'U') &&
                        AcceptCharacter(ref pointer, 'A') &&
                        AcceptCharacter(ref pointer, 'L') &&
                        AcceptCharacter(ref pointer, 'L') &&
                        AcceptCharacter(ref pointer, 'Y'))
                        return Yearly;
                    return null;

                case 'D':
                    if (AcceptCharacter(ref pointer, 'A') &&
                        AcceptCharacter(ref pointer, 'I') &&
                        AcceptCharacter(ref pointer, 'L') &&
                        AcceptCharacter(ref pointer, 'Y'))
                        return Daily;
                    return null;

                case 'E':
                    if (AcceptCharacter(ref pointer, 'V') &&
                        AcceptCharacter(ref pointer, 'E') &&
                        AcceptCharacter(ref pointer, 'R') &&
                        AcceptCharacter(ref pointer, 'Y') &&
                        Accept(ref pointer, '_'))
                    {
                        if (AcceptCharacter(ref pointer, 'M') &&
                            AcceptCharacter(ref pointer, 'I') &&
                            AcceptCharacter(ref pointer, 'N') &&
                            AcceptCharacter(ref pointer, 'U') &&
                            AcceptCharacter(ref pointer, 'T') &&
                            AcceptCharacter(ref pointer, 'E'))
                            return Minutely;

                        if (*(pointer - 1) != '_') return null;

                        if (AcceptCharacter(ref pointer, 'S') &&
                            AcceptCharacter(ref pointer, 'E') &&
                            AcceptCharacter(ref pointer, 'C') &&
                            AcceptCharacter(ref pointer, 'O') &&
                            AcceptCharacter(ref pointer, 'N') &&
                            AcceptCharacter(ref pointer, 'D'))
                            return Secondly;
                    }

                    return null;

                case 'H':
                    if (AcceptCharacter(ref pointer, 'O') &&
                        AcceptCharacter(ref pointer, 'U') &&
                        AcceptCharacter(ref pointer, 'R') &&
                        AcceptCharacter(ref pointer, 'L') &&
                        AcceptCharacter(ref pointer, 'Y'))
                        return Hourly;
                    return null;

                case 'M':
                    if (AcceptCharacter(ref pointer, 'O') &&
                        AcceptCharacter(ref pointer, 'N') &&
                        AcceptCharacter(ref pointer, 'T') &&
                        AcceptCharacter(ref pointer, 'H') &&
                        AcceptCharacter(ref pointer, 'L') &&
                        AcceptCharacter(ref pointer, 'Y'))
                        return Monthly;

                    if (ToUpper(*(pointer - 1)) == 'M' &&
                        AcceptCharacter(ref pointer, 'I') &&
                        AcceptCharacter(ref pointer, 'D') &&
                        AcceptCharacter(ref pointer, 'N') &&
                        AcceptCharacter(ref pointer, 'I') &&
                        AcceptCharacter(ref pointer, 'G') &&
                        AcceptCharacter(ref pointer, 'H') &&
                        AcceptCharacter(ref pointer, 'T'))
                        return Daily;

                    return null;

                case 'W':
                    if (AcceptCharacter(ref pointer, 'E') &&
                        AcceptCharacter(ref pointer, 'E') &&
                        AcceptCharacter(ref pointer, 'K') &&
                        AcceptCharacter(ref pointer, 'L') &&
                        AcceptCharacter(ref pointer, 'Y'))
                        return Weekly;
                    return null;

                case 'Y':
                    if (AcceptCharacter(ref pointer, 'E') &&
                        AcceptCharacter(ref pointer, 'A') &&
                        AcceptCharacter(ref pointer, 'R') &&
                        AcceptCharacter(ref pointer, 'L') &&
                        AcceptCharacter(ref pointer, 'Y'))
                        return Yearly;
                    return null;

                default:
                    pointer--;
                    return null;
            }
        }

        private static unsafe long ParseField(CronField field, ref char* pointer, ref CronExpressionFlag flags)
        {
            if (Accept(ref pointer, '*') || Accept(ref pointer, '?'))
            {
                if (field.CanDefineInterval) flags |= CronExpressionFlag.Interval;
                return ParseStar(field, ref pointer);
            }

            var num = ParseValue(field, ref pointer);

            var bits = ParseRange(field, ref pointer, num, ref flags);
            if (Accept(ref pointer, ',')) bits |= ParseList(field, ref pointer, ref flags);

            return bits;
        }

        private static unsafe long ParseDayOfMonth(ref char* pointer, ref CronExpressionFlag flags, ref byte lastDayOffset)
        {
            var field = CronField.DaysOfMonth;

            if (Accept(ref pointer, '*') || Accept(ref pointer, '?')) return ParseStar(field, ref pointer);

            if (AcceptCharacter(ref pointer, 'L')) return ParseLastDayOfMonth(field, ref pointer, ref flags, ref lastDayOffset);

            var dayOfMonth = ParseValue(field, ref pointer);

            if (AcceptCharacter(ref pointer, 'W'))
            {
                flags |= CronExpressionFlag.NearestWeekday;
                return GetBit(dayOfMonth);
            }

            var bits = ParseRange(field, ref pointer, dayOfMonth, ref flags);
            if (Accept(ref pointer, ',')) bits |= ParseList(field, ref pointer, ref flags);

            return bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe long ParseDayOfWeek(ref char* pointer, ref CronExpressionFlag flags, ref byte nthWeekDay)
        {
            var field = CronField.DaysOfWeek;
            if (Accept(ref pointer, '*') || Accept(ref pointer, '?')) return ParseStar(field, ref pointer);

            var dayOfWeek = ParseValue(field, ref pointer);

            if (AcceptCharacter(ref pointer, 'L')) return ParseLastWeekDay(dayOfWeek, ref flags);
            if (Accept(ref pointer, '#')) return ParseNthWeekDay(field, ref pointer, dayOfWeek, ref flags, out nthWeekDay);

            var bits = ParseRange(field, ref pointer, dayOfWeek, ref flags);
            if (Accept(ref pointer, ',')) bits |= ParseList(field, ref pointer, ref flags);

            return bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe long ParseStar(CronField field, ref char* pointer)
        {
            return Accept(ref pointer, '/')
                ? ParseStep(field, ref pointer, field.First, field.Last)
                : field.AllBits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe long ParseList(CronField field, ref char* pointer, ref CronExpressionFlag flags)
        {
            var num = ParseValue(field, ref pointer);
            var bits = ParseRange(field, ref pointer, num, ref flags);

            do
            {
                if (!Accept(ref pointer, ',')) return bits;

                bits |= ParseList(field, ref pointer, ref flags);
            } while (true);
        }

        private static unsafe long ParseRange(CronField field, ref char* pointer, int low, ref CronExpressionFlag flags)
        {
            if (!Accept(ref pointer, '-'))
            {
                if (!Accept(ref pointer, '/')) return GetBit(low);

                if (field.CanDefineInterval) flags |= CronExpressionFlag.Interval;
                return ParseStep(field, ref pointer, low, field.Last);
            }

            if (field.CanDefineInterval) flags |= CronExpressionFlag.Interval;

            var high = ParseValue(field, ref pointer);
            if (Accept(ref pointer, '/')) return ParseStep(field, ref pointer, low, high);
            return GetBits(field, low, high, 1);
        }

        private static unsafe long ParseStep(CronField field, ref char* pointer, int low, int high)
        {
            // Get the step size -- note: we don't pass the
            // names here, because the number is not an
            // element id, it's a step size.  'low' is
            // sent as a 0 since there is no offset either.
            var step = ParseNumber(field, ref pointer, 1, field.Last);
            return GetBits(field, low, high, step);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe long ParseLastDayOfMonth(CronField field, ref char* pointer, ref CronExpressionFlag flags, ref byte lastMonthOffset)
        {
            flags |= CronExpressionFlag.DayOfMonthLast;

            if (Accept(ref pointer, '-')) lastMonthOffset = (byte)ParseNumber(field, ref pointer, 0, field.Last - 1);
            if (AcceptCharacter(ref pointer, 'W')) flags |= CronExpressionFlag.NearestWeekday;
            return field.AllBits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe long ParseNthWeekDay(CronField field, ref char* pointer, int dayOfWeek, ref CronExpressionFlag flags, out byte nthDayOfWeek)
        {
            nthDayOfWeek = (byte)ParseNumber(field, ref pointer, MinNthDayOfWeek, MaxNthDayOfWeek);
            flags |= CronExpressionFlag.NthDayOfWeek;
            return GetBit(dayOfWeek);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ParseLastWeekDay(int dayOfWeek, ref CronExpressionFlag flags)
        {
            flags |= CronExpressionFlag.DayOfWeekLast;
            return GetBit(dayOfWeek);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool Accept(ref char* pointer, char character)
        {
            if (*pointer == character)
            {
                pointer++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool AcceptCharacter(ref char* pointer, char character)
        {
            if (ToUpper(*pointer) == character)
            {
                pointer++;
                return true;
            }

            return false;
        }

        private static unsafe int ParseNumber(CronField field, ref char* pointer, int low, int high)
        {
            var num = GetNumber(ref pointer, null);
            if (num == -1 || num < low || num > high)
            {
                ThrowFormatException(field, "Value must be a number between {0} and {1} (all inclusive).", low, high);
            }
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int ParseValue(CronField field, ref char* pointer)
        {
            var num = GetNumber(ref pointer, field.Names);
            if (num == -1 || num < field.First || num > field.Last)
            {
                ThrowFormatException(field, "Value must be a number between {0} and {1} (all inclusive).", field.First, field.Last);
            }
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StringBuilder AppendFieldValue(StringBuilder expressionBuilder, CronField field, long fieldValue)
        {
            if (field.AllBits == fieldValue) return expressionBuilder.Append('*');

            // Unset 7 bit for Day of week field because both 0 and 7 stand for Sunday.
            if (field == CronField.DaysOfWeek) fieldValue &= ~(1 << field.Last);

            for (var i = GetFirstSet(fieldValue); ; i = GetFirstSet(fieldValue >> i << i))
            {
                expressionBuilder.Append(i);
                if (fieldValue >> ++i == 0) break;
                expressionBuilder.Append(',');
            }

            return expressionBuilder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder AppendDayOfMonth(StringBuilder expressionBuilder, int domValue)
        {
            if (HasFlag(CronExpressionFlag.DayOfMonthLast))
            {
                expressionBuilder.Append('L');
                if (_lastMonthOffset != 0) expressionBuilder.Append($"-{_lastMonthOffset}");
            }
            else
            {
                AppendFieldValue(expressionBuilder, CronField.DaysOfMonth, (uint)domValue);
            }

            if (HasFlag(CronExpressionFlag.NearestWeekday)) expressionBuilder.Append('W');

            return expressionBuilder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendDayOfWeek(StringBuilder expressionBuilder, int dowValue)
        {
            AppendFieldValue(expressionBuilder, CronField.DaysOfWeek, dowValue);

            if (HasFlag(CronExpressionFlag.DayOfWeekLast)) expressionBuilder.Append('L');
            else if (HasFlag(CronExpressionFlag.NthDayOfWeek)) expressionBuilder.Append($"#{_nthDayOfWeek}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetBits(CronField field, int num1, int num2, int step)
        {
            if (num2 < num1) return GetReversedRangeBits(field, num1, num2, step);
            if (step == 1) return (1L << (num2 + 1)) - (1L << num1);

            return GetRangeBits(num1, num2, step);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetRangeBits(int low, int high, int step)
        {
            var bits = 0L;
            for (var i = low; i <= high; i += step)
            {
                SetBit(ref bits, i);
            }
            return bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetReversedRangeBits(CronField field, int num1, int num2, int step)
        {
            var high = field.Last;
            // Skip one of sundays.
            if (field == CronField.DaysOfWeek) high--;

            var bits = GetRangeBits(num1, high, step);

            num1 = field.First + step - (high - num1) % step - 1;
            return bits | GetRangeBits(num1, num2, step);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetBit(int num1)
        {
            return 1L << num1;
        }

        private static unsafe int GetNumber(ref char* pointer, int[] names)
        {
            if (IsDigit(*pointer))
            {
                var num = GetNumeric(*pointer++);

                if (!IsDigit(*pointer)) return num;

                num = num * 10 + GetNumeric(*pointer++);

                if (!IsDigit(*pointer)) return num;
                return -1;
            }

            if (names == null) return -1;

            if (!IsLetter(*pointer)) return -1;
            var buffer = ToUpper(*pointer++);

            if (!IsLetter(*pointer)) return -1;
            buffer |= ToUpper(*pointer++) << 8;

            if (!IsLetter(*pointer)) return -1;
            buffer |= ToUpper(*pointer++) << 16;

            var length = names.Length;

            for (var i = 0; i < length; i++)
            {
                if (buffer == names[i])
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFormatException(CronField field, string format, params object[] args)
        {
            throw new CronFormatException(field, string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFormatException(string format, params object[] args)
        {
            throw new CronFormatException(string.Format(format, args));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFromShouldBeLessThanToException(string fromName, string toName)
        {
            throw new ArgumentException($"The value of the {fromName} argument should be less than the value of the {toName} argument.", fromName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowWrongDateTimeKindException(string paramName)
        {
            throw new ArgumentException("The supplied DateTime must have the Kind property set to Utc", paramName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetBit(long value, int index)
        {
            return (value & (1L << index)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetBit(ref long value, int index)
        {
            value |= 1L << index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEndOfString(int code)
        {
            return code == '\0';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsWhiteSpace(int code)
        {
            return code == '\t' || code == ' ';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDigit(int code)
        {
            return code >= 48 && code <= 57;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsLetter(int code)
        {
            return (code >= 65 && code <= 90) || (code >= 97 && code <= 122);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNumeric(int code)
        {
            return code - 48;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ToUpper(int code)
        {
            if (code >= 97 && code <= 122)
            {
                return code - 32;
            }

            return code;
        }
    }
}