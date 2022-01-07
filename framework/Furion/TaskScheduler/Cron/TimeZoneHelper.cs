// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;

namespace Furion.TaskScheduler
{
    /// <summary>
    /// 处理不同平台时区的区别
    /// </summary>
    internal static class TimeZoneHelper
    {
        public static bool IsAmbiguousTime(TimeZoneInfo zone, DateTime ambiguousTime)
        {
            return zone.IsAmbiguousTime(ambiguousTime.AddTicks(1));
        }

        public static TimeSpan GetDaylightOffset(TimeZoneInfo zone, DateTime ambiguousDateTime)
        {
            var offsets = GetAmbiguousOffsets(zone, ambiguousDateTime);
            var baseOffset = zone.BaseUtcOffset;

            if (offsets[0] != baseOffset) return offsets[0];

            return offsets[1];
        }

        public static DateTimeOffset GetDaylightTimeStart(TimeZoneInfo zone, DateTime invalidDateTime)
        {
            var dstTransitionDateTime = new DateTime(invalidDateTime.Year, invalidDateTime.Month, invalidDateTime.Day,
                invalidDateTime.Hour, invalidDateTime.Minute, 0, 0, invalidDateTime.Kind);

            while (zone.IsInvalidTime(dstTransitionDateTime))
            {
                dstTransitionDateTime = dstTransitionDateTime.AddMinutes(1);
            }

            var dstOffset = zone.GetUtcOffset(dstTransitionDateTime);

            return new DateTimeOffset(dstTransitionDateTime, dstOffset);
        }

        public static DateTimeOffset GetStandardTimeStart(TimeZoneInfo zone, DateTime ambiguousTime, TimeSpan daylightOffset)
        {
            var dstTransitionEnd = GetDstTransitionEndDateTime(zone, ambiguousTime);

            return new DateTimeOffset(dstTransitionEnd, daylightOffset).ToOffset(zone.BaseUtcOffset);
        }

        public static DateTimeOffset GetAmbiguousIntervalEnd(TimeZoneInfo zone, DateTime ambiguousTime)
        {
            var dstTransitionEnd = GetDstTransitionEndDateTime(zone, ambiguousTime);

            return new DateTimeOffset(dstTransitionEnd, zone.BaseUtcOffset);
        }

        public static DateTimeOffset GetDaylightTimeEnd(TimeZoneInfo zone, DateTime ambiguousTime, TimeSpan daylightOffset)
        {
            var daylightTransitionEnd = GetDstTransitionEndDateTime(zone, ambiguousTime);

            return new DateTimeOffset(daylightTransitionEnd.AddTicks(-1), daylightOffset);
        }

        private static TimeSpan[] GetAmbiguousOffsets(TimeZoneInfo zone, DateTime ambiguousTime)
        {
            return zone.GetAmbiguousTimeOffsets(ambiguousTime.AddTicks(1));
        }

        private static DateTime GetDstTransitionEndDateTime(TimeZoneInfo zone, DateTime ambiguousDateTime)
        {
            var dstTransitionDateTime = new DateTime(ambiguousDateTime.Year, ambiguousDateTime.Month, ambiguousDateTime.Day,
                ambiguousDateTime.Hour, ambiguousDateTime.Minute, 0, 0, ambiguousDateTime.Kind);

            while (zone.IsAmbiguousTime(dstTransitionDateTime))
            {
                dstTransitionDateTime = dstTransitionDateTime.AddMinutes(1);
            }

            return dstTransitionDateTime;
        }
    }
}