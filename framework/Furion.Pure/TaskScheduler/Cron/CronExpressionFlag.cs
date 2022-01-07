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
    /// Cron 表达式标识
    /// </summary>
    [Flags]
    internal enum CronExpressionFlag : byte
    {
        DayOfMonthLast = 0b00001,
        DayOfWeekLast = 0b00010,
        Interval = 0b00100,
        NearestWeekday = 0b01000,
        NthDayOfWeek = 0b10000
    }
}