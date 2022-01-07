// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.Extensions;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 规范化结果配置
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class UnifyResultAttribute : ProducesResponseTypeAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="statusCode"></param>
        public UnifyResultAttribute(int statusCode) : base(statusCode)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type"></param>
        public UnifyResultAttribute(Type type) : base(type, StatusCodes.Status200OK)
        {
            WrapType(type);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="statusCode"></param>
        public UnifyResultAttribute(Type type, int statusCode) : base(type, statusCode)
        {
            WrapType(type);
        }

        /// <summary>
        /// 包装类型
        /// </summary>
        /// <param name="type"></param>
        private void WrapType(Type type)
        {
            if (type != null)
            {
                if (!type.HasImplementedRawGeneric(UnifyContext.RESTfulResultType))
                {
                    Type = UnifyContext.RESTfulResultType.MakeGenericType(type);
                }
                else Type = default;
            }
        }
    }
}