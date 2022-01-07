// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Threading;
using System.Threading.Tasks;

namespace Furion.EventBus
{
    /// <summary>
    /// 事件源存储器
    /// </summary>
    /// <remarks>
    /// <para>顾名思义，这里指的是事件消息存储中心，提供读写能力</para>
    /// <para>默认实现为内存中的 <see cref="System.Threading.Channels.Channel"/>，可自由更换存储介质，如 Kafka，SQL Server 等</para>
    /// </remarks>
    public interface IEventSourceStorer
    {
        /// <summary>
        /// 将事件源写入存储器
        /// </summary>
        /// <param name="eventSource">事件源对象</param>
        /// <param name="cancellationToken">取消任务 Token</param>
        /// <returns><see cref="ValueTask"/></returns>
        ValueTask WriteAsync(IEventSource eventSource, CancellationToken cancellationToken);

        /// <summary>
        /// 从存储器中读取一条事件源
        /// </summary>
        /// <param name="cancellationToken">取消任务 Token</param>
        /// <returns>事件源对象</returns>
        ValueTask<IEventSource> ReadAsync(CancellationToken cancellationToken);
    }
}