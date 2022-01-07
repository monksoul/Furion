// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Furion.DataValidation
{
    /// <summary>
    /// 验证信息元数据
    /// </summary>
    public sealed class ValidationMetadata
    {
        /// <summary>
        /// 验证结果
        /// </summary>
        public Dictionary<string, string[]> ValidationResult { get; internal set; }

        /// <summary>
        /// 异常消息
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// 验证状态
        /// </summary>
        public ModelStateDictionary ModelState { get; internal set; }
    }
}