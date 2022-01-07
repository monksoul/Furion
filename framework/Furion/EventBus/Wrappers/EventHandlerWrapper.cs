// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;
using System.Threading.Tasks;

namespace Furion.EventBus
{
    /// <summary>
    /// 事件处理程序包装类
    /// </summary>
    /// <remarks>主要用于主机服务启动时将所有处理程序和事件 Id 进行包装绑定</remarks>
    internal sealed class EventHandlerWrapper
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventId">事件Id</param>
        internal EventHandlerWrapper(string eventId)
        {
            EventId = eventId;
        }

        /// <summary>
        /// 事件 Id
        /// </summary>
        internal string EventId { get; set; }

        /// <summary>
        /// 事件处理程序
        /// </summary>
        internal Func<EventHandlerExecutingContext, Task> Handler { get; set; }

        /// <summary>
        /// 是否符合条件执行处理程序
        /// </summary>
        /// <param name="eventId">事件 Id</param>
        /// <returns><see cref="bool"/></returns>
        internal bool ShouldRun(string eventId)
        {
            return EventId == eventId;
        }
    }
}