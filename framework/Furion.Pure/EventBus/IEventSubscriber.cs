// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

namespace Furion.EventBus
{
    /// <summary>
    /// 事件订阅者依赖接口
    /// </summary>
    /// <remarks>
    /// <para>可自定义事件处理方法，但须符合 Func{EventSubscribeExecutingContext, Task} 签名</para>
    /// <para>通常只做依赖查找，不做服务调用</para>
    /// </remarks>
    public interface IEventSubscriber
    {
        /*
         * // 事件处理程序定义规范
         * [EventSubscribe(YourEventID)]
         * public Task YourHandler(EventHandlerExecutingContext context)
         * {
         *     // To Do...
         * }
         */
    }
}