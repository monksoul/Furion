// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.RemoteRequest
{
    /// <summary>
    /// 配置Body参数
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Parameter)]
    public class BodyAttribute : ParameterBaseAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public BodyAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="contentType"></param>
        public BodyAttribute(string contentType)
        {
            ContentType = contentType;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="encoding"></param>
        public BodyAttribute(string contentType, string encoding)
        {
            ContentType = contentType;
            Encoding = encoding;
        }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// 内容编码
        /// </summary>
        public string Encoding { get; set; } = "UTF-8";
    }
}