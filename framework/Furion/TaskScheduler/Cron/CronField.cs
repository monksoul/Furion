// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

namespace Furion.TaskScheduler
{
    /// <summary>
    /// Cron 表达式内置字段
    /// </summary>
    internal sealed class CronField
    {
        private static readonly string[] MonthNames =
        {
            null, "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
        };

        private static readonly string[] DayOfWeekNames =
        {
            "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"
        };

        private static readonly int[] MonthNamesArray = new int[MonthNames.Length];
        private static readonly int[] DayOfWeekNamesArray = new int[DayOfWeekNames.Length];

        // 0 and 7 are both Sunday, for compatibility reasons.
        public static readonly CronField DaysOfWeek = new("Days of week", 0, 7, DayOfWeekNamesArray, false);

        public static readonly CronField Months = new("Months", 1, 12, MonthNamesArray, false);
        public static readonly CronField DaysOfMonth = new("Days of month", 1, 31, null, false);
        public static readonly CronField Hours = new("Hours", 0, 23, null, true);
        public static readonly CronField Minutes = new("Minutes", 0, 59, null, true);
        public static readonly CronField Seconds = new("Seconds", 0, 59, null, true);

        static CronField()
        {
            for (var i = 1; i < MonthNames.Length; i++)
            {
                var name = MonthNames[i].ToUpperInvariant();
                var array = new char[3];
                array[0] = name[0];
                array[1] = name[1];
                array[2] = name[2];

                var combined = name[0] | (name[1] << 8) | (name[2] << 16);

                MonthNamesArray[i] = combined;
            }

            for (var i = 0; i < DayOfWeekNames.Length; i++)
            {
                var name = DayOfWeekNames[i].ToUpperInvariant();
                var array = new char[3];
                array[0] = name[0];
                array[1] = name[1];
                array[2] = name[2];

                var combined = name[0] | (name[1] << 8) | (name[2] << 16);

                DayOfWeekNamesArray[i] = combined;
            }
        }

        public readonly string Name;
        public readonly int First;
        public readonly int Last;
        public readonly int[] Names;
        public readonly bool CanDefineInterval;
        public readonly long AllBits;

        private CronField(string name, int first, int last, int[] names, bool canDefineInterval)
        {
            Name = name;
            First = first;
            Last = last;
            Names = names;
            CanDefineInterval = canDefineInterval;
            for (var i = First; i <= Last; i++)
            {
                AllBits |= (1L << i);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}