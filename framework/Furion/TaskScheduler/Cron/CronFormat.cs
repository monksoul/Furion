// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.ComponentModel;

namespace Furion.TaskScheduler
{
    /// <summary>
    /// Cron 表达式支持类型
    /// </summary>
    [SuppressSniffer, Flags]
    public enum CronFormat
    {
        /// <summary>
        /// 只有 5 个字符：分钟，小时，月/天，天，周/天
        /// </summary>
        [Description("只有 5 个字符：分钟，小时，月/天，天，周/天")]
        Standard = 0,

        /// <summary>
        /// 支持秒解析，也就是 6 个字符
        /// </summary>
        [Description("支持秒解析，也就是 6 个字符")]
        IncludeSeconds = 1
    }
}