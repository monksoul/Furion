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
    /// 配置 Body Bytes 参数
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Parameter)]
    public class BodyBytesAttribute : ParameterBaseAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="alias"></param>
        public BodyBytesAttribute(string alias)
        {
            Alias = alias;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="fileName"></param>
        public BodyBytesAttribute(string alias, string fileName)
        {
            Alias = alias;
            FileName = fileName;
        }

        /// <summary>
        /// 参数别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
    }
}