// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.Net.Http;

namespace Furion.RemoteRequest
{
    /// <summary>
    /// 远程请求失败事件类
    /// </summary>
    [SuppressSniffer]
    public sealed class HttpRequestFaildedEventArgs : EventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="exception"></param>
        public HttpRequestFaildedEventArgs(HttpRequestMessage request, HttpResponseMessage response, Exception exception)
        {
            Request = request;
            Response = response;
            Exception = exception;
        }

        /// <summary>
        /// 请求对象
        /// </summary>
        public HttpRequestMessage Request { get; internal set; }

        /// <summary>
        /// 响应对象
        /// </summary>
        public HttpResponseMessage Response { get; internal set; }

        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception Exception { get; internal set; }
    }
}