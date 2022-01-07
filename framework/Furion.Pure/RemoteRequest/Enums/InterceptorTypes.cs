// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System.ComponentModel;

namespace Furion.RemoteRequest
{
    /// <summary>
    /// 拦截类型
    /// </summary>
    [SuppressSniffer]
    public enum InterceptorTypes
    {
        /// <summary>
        /// HttpClient 拦截
        /// </summary>
        [Description("HttpClient 拦截")]
        Client,

        /// <summary>
        /// HttpRequestMessage 拦截
        /// </summary>
        [Description("HttpRequestMessage 拦截")]
        Request,

        /// <summary>
        /// HttpResponseMessage 拦截
        /// </summary>
        [Description("HttpResponseMessage 拦截")]
        Response,

        /// <summary>
        /// 异常拦截
        /// </summary>
        [Description("异常拦截")]
        Exception
    }
}