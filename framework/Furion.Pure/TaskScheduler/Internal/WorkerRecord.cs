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
    /// 记录任务数据
    /// </summary>
    internal sealed class WorkerRecord
    {
        /// <summary>
        /// 定时器
        /// </summary>
        internal SpareTimer Timer { get; set; }

        /// <summary>
        /// 任务执行计数
        /// </summary>
        internal long Tally { get; set; } = 0;

        /// <summary>
        /// 解决定时器重入问题
        /// </summary>
        internal long Interlocked { get; set; } = 0;

        /// <summary>
        /// Cron 表达式实际执行计数
        /// </summary>
        internal long CronActualTally { get; set; } = 0;

        /// <summary>
        /// 是否上一个已完成
        /// </summary>
        internal bool IsCompleteOfPrev { get; set; } = true;
    }
}