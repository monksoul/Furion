// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.FriendlyException
{
    /// <summary>
    /// 异常元数据特性
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Field)]
    public sealed class ErrorCodeItemMetadataAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="errorMessage">错误消息</param>
        /// <param name="args">格式化参数</param>
        public ErrorCodeItemMetadataAttribute(string errorMessage, params object[] args)
        {
            ErrorMessage = errorMessage;
            Args = args;
        }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public object ErrorCode { get; set; }

        /// <summary>
        /// 格式化参数
        /// </summary>
        public object[] Args { get; set; }
    }
}