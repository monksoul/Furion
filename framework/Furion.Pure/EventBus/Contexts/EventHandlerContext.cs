// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Collections.Generic;

namespace Furion.EventBus
{
    /// <summary>
    /// 事件处理程序上下文
    /// </summary>
    public abstract class EventHandlerContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventSource">事件源（事件承载对象）</param>
        /// <param name="properties">共享上下文数据</param>
        public EventHandlerContext(IEventSource eventSource, IDictionary<object, object> properties)
        {
            Source = eventSource;
            Properties = properties;
        }

        /// <summary>
        /// 事件源（事件承载对象）
        /// </summary>
        public IEventSource Source { get; }

        /// <summary>
        /// 共享上下文数据
        /// </summary>
        public IDictionary<object, object> Properties { get; }
    }
}