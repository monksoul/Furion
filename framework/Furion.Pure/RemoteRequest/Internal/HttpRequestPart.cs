// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using Furion.JsonSerialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Furion.RemoteRequest
{
    /// <summary>
    /// Http 请求对象组装部件
    /// </summary>
    public sealed partial class HttpRequestPart
    {
        /// <summary>
        /// 静态缺省请求部件
        /// </summary>
        public static HttpRequestPart Default => new();

        /// <summary>
        /// 请求地址
        /// </summary>
        public string RequestUrl { get; private set; }

        /// <summary>
        /// Url 地址模板
        /// </summary>
        public IDictionary<string, object> Templates { get; private set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        public HttpMethod Method { get; private set; }

        /// <summary>
        /// 请求报文头
        /// </summary>
        public IDictionary<string, object> Headers { get; private set; }

        /// <summary>
        /// 查询参数
        /// </summary>
        public IDictionary<string, object> Queries { get; private set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ClientName { get; private set; }

        /// <summary>
        /// 请求报文 Body 参数
        /// </summary>
        public object Body { get; private set; }

        /// <summary>
        /// 请求报文 Body 内容类型
        /// </summary>
        public string ContentType { get; private set; } = "application/json";

        /// <summary>
        /// 内容编码
        /// </summary>
        public Encoding ContentEncoding { get; private set; } = Encoding.UTF8;

        /// <summary>
        /// 设置 Body Bytes 类型
        /// </summary>
        public List<(string Name, byte[] Bytes, string FileName)> BodyBytes { get; private set; } = new List<(string Name, byte[] Bytes, string FileName)>();

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public long Timeout { get; private set; }

        /// <summary>
        /// JSON 序列化提供器
        /// </summary>
        public Type JsonSerializerProvider { get; private set; } = typeof(SystemTextJsonSerializerProvider);

        /// <summary>
        /// JSON 序列化配置选项
        /// </summary>
        public object JsonSerializerOptions { get; private set; }

        /// <summary>
        /// 是否启用模型验证
        /// </summary>
        public (bool Enabled, bool IncludeNull) ValidationState { get; private set; } = (false, false);

        /// <summary>
        /// 构建请求对象拦截器
        /// </summary>
        public List<Action<HttpRequestMessage>> RequestInterceptors { get; private set; } = new List<Action<HttpRequestMessage>>();

        /// <summary>
        /// 创建客户端对象拦截器
        /// </summary>
        public List<Action<HttpClient>> HttpClientInterceptors { get; private set; } = new List<Action<HttpClient>>();

        /// <summary>
        /// 请求成功拦截器
        /// </summary>
        public List<Action<HttpResponseMessage>> ResponseInterceptors { get; private set; } = new List<Action<HttpResponseMessage>>();

        /// <summary>
        /// 请求异常拦截器
        /// </summary>
        public List<Action<HttpResponseMessage, string>> ExceptionInterceptors { get; private set; } = new List<Action<HttpResponseMessage, string>>();

        /// <summary>
        /// 设置请求作用域
        /// </summary>
        public IServiceProvider RequestScoped { get; private set; }

        /// <summary>
        /// 设置重试策略
        /// </summary>
        public (int NumRetries, int RetryTimeout)? RetryPolicy { get; private set; }
    }
}