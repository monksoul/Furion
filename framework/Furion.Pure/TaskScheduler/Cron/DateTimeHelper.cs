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
    /// 日期时间帮助类
    /// </summary>
    internal static class DateTimeHelper
    {
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        public static DateTimeOffset FloorToSeconds(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.AddTicks(-GetExtraTicks(dateTimeOffset.Ticks));
        }

        public static bool IsRound(DateTimeOffset dateTimeOffset)
        {
            return GetExtraTicks(dateTimeOffset.Ticks) == 0;
        }

        private static long GetExtraTicks(long ticks)
        {
            return ticks % OneSecond.Ticks;
        }
    }
}