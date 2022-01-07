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
using System.Timers;

namespace Furion.TaskScheduler
{
    /// <summary>
    /// 内置时间调度器
    /// </summary>
    [SuppressSniffer]
    public sealed class SpareTimer : Timer
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="workerName"></param>
        internal SpareTimer(string workerName = default) : base()
        {
            WorkerName = workerName ?? Guid.NewGuid().ToString("N");

            // 记录当前定时器
            if (!SpareTime.WorkerRecords.TryAdd(WorkerName, new WorkerRecord
            {
                Timer = this
            })) throw new InvalidOperationException($"The worker name `{WorkerName}` is exist.");
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="workerName"></param>
        internal SpareTimer(double interval, string workerName = default) : base(interval)
        {
            WorkerName = workerName ?? Guid.NewGuid().ToString("N");

            // 记录当前定时器
            if (!SpareTime.WorkerRecords.TryAdd(WorkerName, new WorkerRecord
            {
                Interlocked = 0,
                Tally = 0,
                Timer = this
            })) throw new InvalidOperationException($"The worker name `{WorkerName}` is exist.");
        }

        /// <summary>
        /// 当前任务名
        /// </summary>
        public string WorkerName { get; private set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public SpareTimeTypes Type { get; internal set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public SpareTimeStatus Status { get; internal set; }

        /// <summary>
        /// 执行类型
        /// </summary>
        public SpareTimeExecuteTypes ExecuteType { get; internal set; } = SpareTimeExecuteTypes.Parallel;

        /// <summary>
        /// 异常信息
        /// </summary>
        public Dictionary<long, Exception> Exception { get; internal set; } = new Dictionary<long, Exception>();

        /// <summary>
        /// 任务执行计数
        /// </summary>
        public long Tally { get; internal set; } = 0;
    }
}