// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System.ComponentModel;

namespace Furion.TaskScheduler
{
    /// <summary>
    /// 任务状态
    /// </summary>
    [SuppressSniffer]
    public enum SpareTimeStatus
    {
        /// <summary>
        /// 运行中
        /// </summary>
        [Description("运行中")]
        Running,

        /// <summary>
        /// 已停止或未启动
        /// </summary>
        [Description("已停止或未启动")]
        Stopped,

        /// <summary>
        /// 单次执行失败
        /// </summary>
        [Description("任务停止并失败")]
        Failed,

        /// <summary>
        /// 任务已取消或没有该任务
        /// </summary>
        [Description("任务已取消或没有该任务")]
        CanceledOrNone
    }
}