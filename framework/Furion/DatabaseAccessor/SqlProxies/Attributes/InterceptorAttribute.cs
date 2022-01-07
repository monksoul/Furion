// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// Sql 代理拦截器
    /// </summary>
    /// <remarks>如果贴在静态方法中且 InterceptorId/MethodName 为空，则为全局拦截</remarks>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Method)]
    public class InterceptorAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public InterceptorAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="interceptorIds"></param>
        public InterceptorAttribute(params string[] interceptorIds)
        {
            InterceptorIds = interceptorIds;
        }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string[] InterceptorIds { get; set; }
    }
}