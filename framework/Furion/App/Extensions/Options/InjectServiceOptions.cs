// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DataValidation;
using Furion.FriendlyException;
using Furion.SpecificationDocument;
using System;

namespace Furion
{
    /// <summary>
    /// Inject 服务配置选项
    /// </summary>
    public sealed class InjectServiceOptions
    {
        /// <summary>
        /// 规范化结果配置
        /// </summary>
        public Action<SpecificationDocumentServiceOptions> SpecificationDocumentConfigure { get; set; }

        /// <summary>
        /// 数据校验配置
        /// </summary>
        public Action<DataValidationServiceOptions> DataValidationConfigure { get; set; }

        /// <summary>
        /// 友好异常配置
        /// </summary>
        public Action<FriendlyExceptionServiceOptions> FriendlyExceptionConfigure { get; set; }
    }
}