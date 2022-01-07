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
    /// 内存通道事件源（事件承载对象）
    /// </summary>
    public sealed class ChannelEventSource : IEventSource
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventId">事件 Id</param>
        public ChannelEventSource(string eventId)
        {
            EventId = eventId;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventId">事件 Id</param>
        /// <param name="payload">事件承载（携带）数据</param>
        public ChannelEventSource(string eventId, object payload)
            : this(eventId)
        {
            Payload = payload;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventId">事件 Id</param>
        /// <param name="payload">事件承载（携带）数据</param>
        /// <param name="cancellationToken">取消任务 Token</param>
        public ChannelEventSource(string eventId, object payload, CancellationToken cancellationToken)
            : this(eventId, payload)
        {
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// 事件 Id
        /// </summary>
        public string EventId { get; }

        /// <summary>
        /// 事件承载（携带）数据
        /// </summary>
        public object Payload { get; }

        /// <summary>
        /// 取消任务 Token
        /// </summary>
        /// <remarks>用于取消本次消息处理</remarks>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// 事件创建时间
        /// </summary>
        public DateTime CreatedTime { get; } = DateTime.UtcNow;
    }
}