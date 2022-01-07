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
    /// 事件处理程序执行器依赖接口
    /// </summary>
    public interface IEventHandlerExecutor
    {
        /// <summary>
        /// 执行事件处理程序
        /// </summary>
        /// <remarks>在这里可以实现超时控制，失败重试控制等等</remarks>
        /// <param name="context">事件处理程序执行前上下文</param>
        /// <param name="handler">事件处理程序</param>
        /// <returns><see cref="Task"/> 实例</returns>
        Task ExecuteAsync(EventHandlerExecutingContext context, Func<EventHandlerExecutingContext, Task> handler);
    }
}
