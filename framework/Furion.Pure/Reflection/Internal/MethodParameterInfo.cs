// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Reflection;

namespace Furion.Reflection
{
    /// <summary>
    /// 方法参数信息
    /// </summary>
    internal class MethodParameterInfo
    {
        /// <summary>
        /// 参数
        /// </summary>
        internal ParameterInfo Parameter { get; set; }

        /// <summary>
        /// 参数名
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        internal object Value { get; set; }
    }
}