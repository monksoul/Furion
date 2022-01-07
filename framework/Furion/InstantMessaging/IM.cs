// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.AspNetCore.SignalR;
using System;

namespace Furion.InstantMessaging
{
    /// <summary>
    /// 即时通信静态类
    /// </summary>
    public static class IM
    {
        /// <summary>
        /// 获取集线器实例
        /// </summary>
        /// <typeparam name="THub"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IHubContext<THub> GetHub<THub>(IServiceProvider serviceProvider = default)
            where THub : Hub
        {
            return App.GetService<IHubContext<THub>>(serviceProvider);
        }

        /// <summary>
        /// 获取强类型集线器实例
        /// </summary>
        /// <typeparam name="THub"></typeparam>
        /// <typeparam name="TStronglyTyped"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IHubContext<THub, TStronglyTyped> GetHub<THub, TStronglyTyped>(IServiceProvider serviceProvider = default)
            where THub : Hub<TStronglyTyped>
            where TStronglyTyped : class
        {
            return App.GetService<IHubContext<THub, TStronglyTyped>>(serviceProvider);
        }
    }
}