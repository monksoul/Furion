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
    /// 任务执行类型
    /// </summary>
    [SuppressSniffer]
    public enum SpareTimeExecuteTypes
    {
        /// <summary>
        /// 并行执行（默认方式）
        /// <para>无需等待上一个完成</para>
        /// </summary>
        [Description("并行执行")]
        Parallel,

        /// <summary>
        /// 串行执行
        /// <para>需等待上一个完成</para>
        /// </summary>
        [Description("串行执行")]
        Serial
    }
}