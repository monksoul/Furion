// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.TaskScheduler
{
    /// <summary>
    /// 解析 Cron 表达式出错异常类
    /// </summary>
    [SuppressSniffer, Serializable]
    public class CronFormatException : FormatException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public CronFormatException(string message) : base(message)
        {
        }

        /// <summary>
        /// 内部构造函数
        /// </summary>
        /// <param name="field"></param>
        /// <param name="message"></param>
        internal CronFormatException(CronField field, string message) : this($"{field}: {message}")
        {
        }
    }
}