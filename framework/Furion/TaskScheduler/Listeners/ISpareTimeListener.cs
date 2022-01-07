// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Threading.Tasks;

namespace Furion.TaskScheduler
{
    /// <summary>
    /// 定时器监听接口（注册为单例）
    /// </summary>
    public interface ISpareTimeListener
    {
        /// <summary>
        /// 监听器
        /// </summary>
        /// <param name="executer"></param>
        Task OnListener(SpareTimerExecuter executer);
    }
}