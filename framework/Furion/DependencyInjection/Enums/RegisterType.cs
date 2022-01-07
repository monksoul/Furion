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
    /// 注册类型
    /// </summary>
    [SuppressSniffer]
    public enum RegisterType
    {
        /// <summary>
        /// 瞬时
        /// </summary>
        [Description("瞬时")]
        Transient,

        /// <summary>
        /// 作用域
        /// </summary>
        [Description("作用域")]
        Scoped,

        /// <summary>
        /// 单例
        /// </summary>
        [Description("单例")]
        Singleton
    }
}