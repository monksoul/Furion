// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;

namespace Furion.EventBus
{
    /// <summary>
    /// 事件处理程序特性
    /// </summary>
    /// <remarks>
    /// <para>作用于 <see cref="IEventSubscriber"/> 实现类实例方法</para>
    /// <para>支持多个事件 Id 触发同一个事件处理程序</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class EventSubscribeAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventId">事件 Id</param>
        public EventSubscribeAttribute(string eventId)
        {
            EventId = eventId;
        }

        /// <summary>
        /// 事件 Id
        /// </summary>
        public string EventId { get; set; }
    }
}