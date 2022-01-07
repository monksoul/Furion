// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Threading.Tasks;

namespace Furion.IPCChannel
{
    /// <summary>
    /// 进程管道内通信处理程序
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class ChannelHandler<TMessage>
    {
        /// <summary>
        /// 管道执行器
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task InvokeAsync(TMessage message);
    }
}