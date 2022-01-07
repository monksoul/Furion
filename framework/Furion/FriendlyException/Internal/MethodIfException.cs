// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Collections.Generic;
using System.Reflection;

namespace Furion.FriendlyException
{
    /// <summary>
    /// 方法异常类
    /// </summary>
    internal sealed class MethodIfException
    {
        /// <summary>
        /// 出异常的方法
        /// </summary>
        public MethodBase ErrorMethod { get; set; }

        /// <summary>
        /// 异常特性
        /// </summary>
        public IEnumerable<IfExceptionAttribute> IfExceptionAttributes { get; set; }
    }
}