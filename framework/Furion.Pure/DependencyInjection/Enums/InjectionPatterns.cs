// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.ComponentModel;

namespace Furion.DependencyInjection
{
    /// <summary>
    /// 注册范围
    /// </summary>
    [SuppressSniffer]
    public enum InjectionPatterns
    {
        /// <summary>
        /// 只注册自己
        /// </summary>
        [Description("只注册自己")]
        Self,

        /// <summary>
        /// 第一个接口
        /// </summary>
        [Description("只注册第一个接口")]
        FirstInterface,

        /// <summary>
        /// 自己和第一个接口，默认值
        /// </summary>
        [Description("自己和第一个接口")]
        SelfWithFirstInterface,

        /// <summary>
        /// 所有接口
        /// </summary>
        [Description("所有接口")]
        ImplementedInterfaces,

        /// <summary>
        /// 注册自己包括所有接口
        /// </summary>
        [Description("自己包括所有接口")]
        All
    }
}