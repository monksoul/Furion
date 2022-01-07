// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 接口参数约束
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Parameter)]
    public class RouteConstraintAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="constraint"></param>
        public RouteConstraintAttribute(string constraint)
        {
            Constraint = constraint;
        }

        /// <summary>
        /// 参数位置
        /// </summary>
        public string Constraint { get; set; }
    }
}