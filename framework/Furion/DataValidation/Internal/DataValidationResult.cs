// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Furion.DataValidation
{
    /// <summary>
    /// 数据验证结果
    /// </summary>
    [SuppressSniffer]
    public sealed class DataValidationResult
    {
        /// <summary>
        /// 验证状态
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 验证结果
        /// </summary>
        public ICollection<ValidationResult> ValidationResults { get; set; }

        /// <summary>
        /// 成员或值
        /// </summary>
        public object MemberOrValue { get; set; }
    }
}