// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// 验证逻辑
    /// </summary>
    [SuppressSniffer]
    public enum ValidationPattern
    {
        /// <summary>
        /// 全部都要验证通过
        /// </summary>
        [Description("全部验证通过才为真")]
        AllOfThem,

        /// <summary>
        /// 至少一个验证通过
        /// </summary>
        [Description("有一个通过就为真")]
        AtLeastOne
    }
}