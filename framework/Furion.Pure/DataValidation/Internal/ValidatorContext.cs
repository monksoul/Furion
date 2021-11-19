// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Furion.DataValidation
{
    /// <summary>
    /// 验证上下文
    /// </summary>
    internal static class ValidatorContext
    {
        /// <summary>
        /// 获取验证错误信息
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        internal static ValidationMetadata GetValidationMetadata(object errors)
        {
            ModelStateDictionary _modelState = null;
            Dictionary<string, string[]> validationResults = null;

            // 如果是模型验证字典类型
            if (errors is ModelStateDictionary modelState)
            {
                _modelState = modelState;
                // 将验证错误信息转换成字典并序列化成 Json
                validationResults = modelState.Where(u => modelState[u.Key].ValidationState == ModelValidationState.Invalid)
                        .ToDictionary(u => u.Key, u => modelState[u.Key].Errors.Select(c => c.ErrorMessage).ToArray());
            }
            // 如果是 ValidationProblemDetails 特殊类型
            else if (errors is ValidationProblemDetails validation)
            {
                validationResults = validation.Errors
                    .ToDictionary(u => u.Key, u => u.Value.ToArray());
            }
            // 如果是字典类型
            else if (errors is Dictionary<string, string[]> dicResults)
            {
                validationResults = dicResults;
            }
            // 其他类型
            else validationResults = new Dictionary<string, string[]>
            {
                {string.Empty, new[]{errors?.ToString()}}
            };

            return new ValidationMetadata
            {
                ValidationResult = validationResults,
                Message = JsonSerializer.Serialize(validationResults, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                }),
                ModelState = _modelState
            };
        }
    }
}