// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Furion.EventBus
{
    /// <summary>
    /// 内存通道事件源存储器（默认实现）
    /// </summary>
    /// <remarks>
    /// <para>顾名思义，这里指的是事件消息存储中心，提供读写能力</para>
    /// <para>默认实现为内存中的 <see cref="System.Threading.Channels.Channel"/>，可自由更换存储介质，如 Kafka，SQL Server 等</para>
    /// </remarks>
    internal sealed partial class ChannelEventSourceStorer : IEventSourceStorer
    {
        /// <summary>
        /// 内存通道事件源存储器
        /// </summary>
        private readonly Channel<IEventSource> _channel;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">存储器最多能够处理多少消息，超过该容量进入等待写入</param>
        public ChannelEventSourceStorer(int capacity)
        {
            // 配置通道，设置超出默认容量后进入等待
            var boundedChannelOptions = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };

            // 创建有限容量通道
            _channel = Channel.CreateBounded<IEventSource>(boundedChannelOptions);
        }

        /// <summary>
        /// 将事件源写入存储器
        /// </summary>
        /// <param name="eventSource">事件源对象</param>
        /// <param name="cancellationToken">取消任务 Token</param>
        /// <returns><see cref="ValueTask"/></returns>
        public async ValueTask WriteAsync(IEventSource eventSource, CancellationToken cancellationToken)
        {
            // 空检查
            if (eventSource == default)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            // 写入存储器
            await _channel.Writer.WriteAsync(eventSource, cancellationToken);
        }

        /// <summary>
        /// 从存储器中读取一条事件源
        /// </summary>
        /// <param name="cancellationToken">取消任务 Token</param>
        /// <returns>事件源对象</returns>
        public async ValueTask<IEventSource> ReadAsync(CancellationToken cancellationToken)
        {
            // 读取一条事件源
            var eventSource = await _channel.Reader.ReadAsync(cancellationToken);
            return eventSource;
        }
    }
}