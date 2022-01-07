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
    /// 远程请求参数拦截器
    /// </summary>
    /// <remarks>如果贴在静态方法中，则为全局拦截</remarks>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public class InterceptorAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type"></param>
        public InterceptorAttribute(InterceptorTypes type)
        {
            Type = type;
        }

        /// <summary>
        /// 拦截类型
        /// </summary>
        public InterceptorTypes Type { get; set; }
    }
}