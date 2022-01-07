// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

namespace Furion.DataValidation
{
    /// <summary>
    /// 数据验证服务配置选项
    /// </summary>
    public sealed class DataValidationServiceOptions
    {
        /// <summary>
        /// 启用全局数据验证
        /// </summary>
        public bool EnableGlobalDataValidation { get; set; } = true;

        /// <summary>
        /// 禁止C# 8.0 验证非可空引用类型
        /// </summary>
        public bool SuppressImplicitRequiredAttributeForNonNullableReferenceTypes { get; set; } = true;
    }
}