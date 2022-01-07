// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.Text.RegularExpressions;

namespace Furion.DataValidation
{
    /// <summary>
    /// 验证项元数据
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Field)]
    public class ValidationItemMetadataAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regularExpression">正则表达式</param>
        /// <param name="defaultErrorMessage">失败提示默认消息</param>
        /// <param name="regexOptions">正则表达式匹配选项</param>
        public ValidationItemMetadataAttribute(string regularExpression, string defaultErrorMessage, RegexOptions regexOptions = RegexOptions.None)
        {
            RegularExpression = regularExpression;
            DefaultErrorMessage = defaultErrorMessage;
            RegexOptions = regexOptions;
        }

        /// <summary>
        /// 正则表达式
        /// </summary>
        public string RegularExpression { get; set; }

        /// <summary>
        /// 默认验证失败类型
        /// </summary>
        public string DefaultErrorMessage { get; set; }

        /// <summary>
        /// 正则表达式选项
        /// </summary>
        public RegexOptions RegexOptions { get; set; }
    }
}