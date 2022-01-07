// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;

namespace Furion.TaskScheduler
{
    /// <summary>
    /// 定时器执行状态器
    /// </summary>
    [SuppressSniffer]
    public sealed class SpareTimerExecuter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="status"></param>
        public SpareTimerExecuter(SpareTimer timer, int status)
        {
            Timer = timer;
            Status = status;
        }

        /// <summary>
        /// 定时器
        /// </summary>
        public SpareTimer Timer { get; internal set; }

        /// <summary>
        /// 状态
        /// </summary>
        /// <remarks>
        /// <para>0：任务开始</para>
        /// <para>1：执行之前</para>
        /// <para>2：执行成功</para>
        /// <para>3：执行失败</para>
        /// <para>-1：任务停止</para>
        /// <para>-2：任务取消</para>
        /// </remarks>
        public int Status { get; internal set; }
    }
}