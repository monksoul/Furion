// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// 安全定义特性
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SecurityDefineAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SecurityDefineAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resourceId"></param>
        public SecurityDefineAttribute(string resourceId)
        {
            ResourceId = resourceId;
        }

        /// <summary>
        /// 资源Id，必须是唯一的
        /// </summary>
        public string ResourceId { get; set; }
    }
}