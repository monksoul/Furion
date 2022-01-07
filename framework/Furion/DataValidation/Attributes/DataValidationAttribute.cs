// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DataValidation;
using Furion.DependencyInjection;
using Furion.Localization;
using System.Linq;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// 数据类型验证特性
    /// </summary>
    [SuppressSniffer]
    public class DataValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="validationPattern">验证逻辑</param>
        /// <param name="validationTypes"></param>
        public DataValidationAttribute(ValidationPattern validationPattern, params object[] validationTypes)
        {
            ValidationPattern = validationPattern;
            ValidationTypes = validationTypes;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="validationTypes"></param>
        public DataValidationAttribute(params object[] validationTypes)
        {
            ValidationPattern = ValidationPattern.AllOfThem;
            ValidationTypes = validationTypes;
        }

        /// <summary>
        /// 验证逻辑
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // 判断是否允许 空值
            if (AllowNullValue && value == null) return ValidationResult.Success;

            // 是否忽略空字符串
            if (AllowEmptyStrings && value is string && value.Equals(string.Empty)) return ValidationResult.Success;

            // 执行值验证
            var dataValidationResult = value.TryValidate(ValidationPattern, ValidationTypes);
            dataValidationResult.MemberOrValue = validationContext.MemberName;

            // 验证失败
            if (!dataValidationResult.IsValid)
            {
                var resultMessage = dataValidationResult.ValidationResults.FirstOrDefault().ErrorMessage;

                // 进行多语言处理
                var errorMessage = !string.IsNullOrWhiteSpace(ErrorMessage) ? ErrorMessage : resultMessage;

                return new ValidationResult(string.Format(L.Text == null ? errorMessage : L.Text[errorMessage], validationContext.MemberName));
            }

            // 验证成功
            return ValidationResult.Success;
        }

        /// <summary>
        /// 验证类型
        /// </summary>
        public object[] ValidationTypes { get; set; }

        /// <summary>
        /// 验证逻辑
        /// </summary>
        public ValidationPattern ValidationPattern { get; set; }

        /// <summary>
        ///是否允许空字符串
        /// </summary>
        public bool AllowEmptyStrings { get; set; } = false;

        /// <summary>
        /// 允许空值，有值才验证，默认 false
        /// </summary>
        public bool AllowNullValue { get; set; } = false;
    }
}