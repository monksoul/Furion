// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.Extensions;
using Furion.Templates.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Furion.DataValidation
{
    /// <summary>
    /// 数据验证器
    /// </summary>
    [SuppressSniffer]
    public static class DataValidator
    {
        /// <summary>
        /// 所有验证类型
        /// </summary>
        private static readonly IEnumerable<Type> ValidationTypes;

        /// <summary>
        /// 所有验证类型
        /// </summary>
        private static readonly IEnumerable<Type> ValidationMessageTypes;

        /// <summary>
        /// 验证类型正则表达式
        /// </summary>
        private static readonly ConcurrentDictionary<string, ValidationItemMetadataAttribute> ValidationItemMetadatas;

        /// <summary>
        /// 构造函数
        /// </summary>
        static DataValidator()
        {
            // 获取所有验证类型
            ValidationTypes = GetValidationTypes();

            // 获取所有验证消息类型
            ValidationMessageTypes = GetValidationMessageTypes();

            // 获取所有验证类型正则表达式
            ValidationItemMetadatas = GetValidationValidationItemMetadatas();

            // 缓存所有正则表达式
            GetValidationTypeValidationItemMetadataCached = new ConcurrentDictionary<object, (string, ValidationItemMetadataAttribute)>();
        }

        /// <summary>
        /// 验证类类型对象
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <param name="validateAllProperties">是否验证所有属性</param>
        /// <returns>验证结果</returns>
        public static DataValidationResult TryValidateObject(object obj, bool validateAllProperties = true)
        {
            // 如果该类型贴有 [NonValidate] 特性，则跳过验证
            if (obj.GetType().IsDefined(typeof(NonValidationAttribute), true))
                return new DataValidationResult
                {
                    IsValid = true,
                    MemberOrValue = obj
                };

            // 存储验证结果
            ICollection<ValidationResult> results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, new ValidationContext(obj), results, validateAllProperties);

            // 返回验证结果
            return new DataValidationResult
            {
                IsValid = isValid,
                ValidationResults = results,
                MemberOrValue = obj
            };
        }

        /// <summary>
        /// 验证单个值
        /// </summary>
        /// <param name="value">单个值</param>
        /// <param name="validationAttributes">验证特性</param>
        /// <returns></returns>
        public static DataValidationResult TryValidateValue(object value, params ValidationAttribute[] validationAttributes)
        {
            // 存储验证结果
            ICollection<ValidationResult> results = new List<ValidationResult>();
            var isValid = Validator.TryValidateValue(value, new ValidationContext(value), results, validationAttributes);

            // 返回验证结果
            return new DataValidationResult
            {
                IsValid = isValid,
                ValidationResults = results,
                MemberOrValue = value
            };
        }

        /// <summary>
        /// 正则表达式验证
        /// </summary>
        /// <param name="value"></param>
        /// <param name="regexPattern"></param>
        /// <param name="regexOptions">正则表达式选项</param>
        /// <returns></returns>
        public static bool TryValidateValue(object value, string regexPattern, RegexOptions regexOptions = RegexOptions.None)
        {
            return value == null
                ? throw new ArgumentNullException(nameof(value))
                : Regex.IsMatch(value.ToString(), regexPattern, regexOptions);
        }

        /// <summary>
        /// 验证类型验证
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationTypes"></param>
        /// <returns></returns>
        public static DataValidationResult TryValidateValue(object value, params object[] validationTypes)
        {
            return TryValidateValue(value, ValidationPattern.AllOfThem, validationTypes);
        }

        /// <summary>
        /// 验证类型验证
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationOptionss">验证方式</param>
        /// <param name="validationTypes"></param>
        /// <returns></returns>
        public static DataValidationResult TryValidateValue(object value, ValidationPattern validationOptionss, params object[] validationTypes)
        {
            // 存储验证结果
            ICollection<ValidationResult> results = new List<ValidationResult>();

            // 如果值未null，验证失败
            if (value == null)
            {
                results.Add(new ValidationResult("The value is required"));

                // 返回验证结果
                return new DataValidationResult
                {
                    IsValid = false,
                    ValidationResults = results,
                    MemberOrValue = value
                };
            }

            // 验证标识
            bool? isValid = null;
            foreach (var validationType in validationTypes)
            {
                // 解析名称和正则表达式
                var (validationName, validationItemMetadata) = GetValidationTypeValidationItemMetadata(validationType);

                // 验证结果
                var validResult = TryValidateValue(value, validationItemMetadata.RegularExpression, validationItemMetadata.RegexOptions);

                // 判断是否需要同时验证通过才通过
                if (validationOptionss == ValidationPattern.AtLeastOne)
                {
                    // 只要有一个验证通过，则跳出
                    if (validResult)
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!validResult)
                {
                    if (isValid != false) isValid = false;
                    // 添加错误消息
                    results.Add(new ValidationResult(
                        string.Format(validationItemMetadata.DefaultErrorMessage.Render(), value, validationName)));
                }
            }

            // 返回验证结果
            return new DataValidationResult
            {
                IsValid = isValid ?? true,
                ValidationResults = results,
                MemberOrValue = value
            };
        }

        /// <summary>
        /// 获取验证类型验证Item集合
        /// </summary>
        private static readonly ConcurrentDictionary<object, (string, ValidationItemMetadataAttribute)> GetValidationTypeValidationItemMetadataCached;

        /// <summary>
        /// 获取验证类型正则表达式（需要缓存）
        /// </summary>
        /// <param name="validationType"></param>
        /// <returns></returns>
        private static (string ValidationName, ValidationItemMetadataAttribute ValidationItemMetadata) GetValidationTypeValidationItemMetadata(object validationType)
        {
            return GetValidationTypeValidationItemMetadataCached.GetOrAdd(validationType, Function);

            // 本地函数
            static (string, ValidationItemMetadataAttribute) Function(object validationType)
            {
                // 获取验证类型
                var type = validationType.GetType();

                // 判断是否是有效的验证类型
                if (!ValidationTypes.Any(u => u == type))
                    throw new InvalidOperationException($"{type.Name} is not a valid validation type.");

                // 获取对应的枚举名称
                var validationName = Enum.GetName(type, validationType);

                // 判断是否配置验证正则表达式
                if (!ValidationItemMetadatas.ContainsKey(validationName))
                    throw new InvalidOperationException($"No ${validationName} validation type metadata exists.");

                // 获取对应的验证选项
                var validationItemMetadataAttribute = ValidationItemMetadatas[validationName];

                return (validationName, validationItemMetadataAttribute);
            }
        }

        /// <summary>
        /// 获取所有验证类型
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetValidationTypes()
        {
            // 扫描所有公开的枚举且贴有 [ValidationType] 特性
            var validationTypes = App.EffectiveTypes.Where(u => u.IsDefined(typeof(ValidationTypeAttribute), true) && u.IsEnum);
            return validationTypes;
        }

        /// <summary>
        /// 获取所有验证消息类型
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetValidationMessageTypes()
        {
            // 扫描所有公开的的枚举且贴有 [ValidationMessageType] 特性
            var validationMessageTypes = App.EffectiveTypes
                .Where(u => u.IsDefined(typeof(ValidationMessageTypeAttribute), true) && u.IsEnum);

            // 加载自定义验证消息类型提供器
            var validationMessageTypeProvider = App.GetService<IValidationMessageTypeProvider>(App.RootServices);
            if (validationMessageTypeProvider is { Definitions: not null }) validationMessageTypes = validationMessageTypes.Concat(validationMessageTypeProvider.Definitions);

            return validationMessageTypes.Distinct();
        }

        /// <summary>
        /// 获取验证类型所有有效的正则表达式
        /// </summary>
        /// <returns></returns>
        private static ConcurrentDictionary<string, ValidationItemMetadataAttribute> GetValidationValidationItemMetadatas()
        {
            var vaidationItems = new ConcurrentDictionary<string, ValidationItemMetadataAttribute>();

            // 查找所有 [ValidationMessageType] 类型中的 [ValidationMessage] 消息定义
            var customErrorMessages = ValidationMessageTypes.SelectMany(u => u.GetFields()
                    .Where(u => u.IsDefined(typeof(ValidationMessageAttribute))))
                .ToDictionary(u => u.Name, u => u.GetCustomAttribute<ValidationMessageAttribute>().ErrorMessage.Render());

            // 加载配置文件配置
            var validationTypeMessageSettings = App.GetConfig<ValidationTypeMessageSettingsOptions>("ValidationTypeMessageSettings", true);
            if (validationTypeMessageSettings is { Definitions: not null })
            {
                // 获取所有参数大于1的配置
                var settingsErrorMessages = validationTypeMessageSettings.Definitions
                    .Where(u => u.Length > 1)
                    .ToDictionary(u => u[0].ToString(), u => u[1].ToString());

                customErrorMessages = customErrorMessages.AddOrUpdate(settingsErrorMessages);
            }

            // 获取所有验证属性
            var validationFields = ValidationTypes.SelectMany(u => u.GetFields()
                .Where(u => u.IsDefined(typeof(ValidationItemMetadataAttribute))))
                .ToDictionary(u => u.Name, u => ReplaceValidateErrorMessage(u.Name, u, customErrorMessages));

            vaidationItems.AddOrUpdate(validationFields);

            return vaidationItems;
        }

        /// <summary>
        /// 替换默认验证失败消息
        /// </summary>
        /// <param name="name">验证唯一名称</param>
        /// <param name="field"></param>
        /// <param name="customErrorMessages"></param>
        private static ValidationItemMetadataAttribute ReplaceValidateErrorMessage(string name, FieldInfo field, Dictionary<string, string> customErrorMessages)
        {
            var validationValidationItemMetadata = field.GetCustomAttribute<ValidationItemMetadataAttribute>();
            if (customErrorMessages.ContainsKey(name))
            {
                validationValidationItemMetadata.DefaultErrorMessage = customErrorMessages[name];
            }

            return validationValidationItemMetadata;
        }
    }
}