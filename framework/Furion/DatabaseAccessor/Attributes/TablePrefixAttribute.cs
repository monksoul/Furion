// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;

namespace System.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// 配置表名称前缀
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TablePrefixAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="prefix"></param>
        public TablePrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// 前缀
        /// </summary>
        public string Prefix { get; set; }
    }
}