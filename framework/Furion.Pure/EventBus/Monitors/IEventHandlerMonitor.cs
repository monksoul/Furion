// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Threading.Tasks;

namespace Furion.EventBus
{
    /// <summary>
    /// 事件处理程序监视器
    /// </summary>
    public interface IEventHandlerMonitor
    {
        /// <summary>
        /// 事件处理程序执行前
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns><see cref="Task"/> 实例</returns>
        Task OnExecutingAsync(EventHandlerExecutingContext context);

        /// <summary>
        /// 事件处理程序执行后
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns><see cref="Task"/> 实例</returns>
        Task OnExecutedAsync(EventHandlerExecutedContext context);
    }
}