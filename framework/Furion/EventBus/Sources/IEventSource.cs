// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;
using System.Threading;

namespace Furion.EventBus
{
    /// <summary>
    /// 事件源（事件承载对象）依赖接口
    /// </summary>
    public interface IEventSource
    {
        /// <summary>
        /// 事件 Id
        /// </summary>
        string EventId { get; }

        /// <summary>
        /// 事件承载（携带）数据
        /// </summary>
        object Payload { get; }

        /// <summary>
        /// 事件创建时间
        /// </summary>
        DateTime CreatedTime { get; }

        /// <summary>
        /// 取消任务 Token
        /// </summary>
        /// <remarks>用于取消本次消息处理</remarks>
        CancellationToken CancellationToken { get; }
    }
}