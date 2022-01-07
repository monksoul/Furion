// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.RemoteRequest
{
    /// <summary>
    /// 配置请求失败重试策略
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class RetryPolicyAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="numRetries"></param>
        /// <param name="retryTimeout">每次延迟时间（毫秒）</param>
        public RetryPolicyAttribute(int numRetries, int retryTimeout = 1000)
        {
            NumRetries = numRetries;
            RetryTimeout = retryTimeout;
        }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int NumRetries { get; set; }

        /// <summary>
        /// 每次延迟时间（毫秒）
        /// </summary>
        public int RetryTimeout { get; set; } = 1000;
    }
}