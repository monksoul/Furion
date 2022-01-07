// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System.ComponentModel;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 数据库上下文模式
    /// </summary>
    [SuppressSniffer]
    public enum DbContextMode
    {
        /// <summary>
        /// 缓存模型数据库上下文
        /// <para>
        /// OnModelCreating 只会初始化一次
        /// </para>
        /// </summary>
        [Description("缓存模型数据库上下文")]
        Cached,

        /// <summary>
        /// 动态模型数据库上下文
        /// <para>
        /// OnModelCreating 每次都会调用
        /// </para>
        /// </summary>
        [Description("动态模型数据库上下文")]
        Dynamic
    }
}