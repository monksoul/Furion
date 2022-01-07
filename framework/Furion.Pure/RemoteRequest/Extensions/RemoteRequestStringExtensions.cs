// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.JsonSerialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.RemoteRequest.Extensions
{
    /// <summary>
    /// 远程请求字符串拓展
    /// </summary>
    [SuppressSniffer]
    public static class RemoteRequestStringExtensions
    {
        /// <summary>
        /// 设置 URL 模板
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        public static HttpRequestPart SetTemplates(this string requestUrl, IDictionary<string, object> templates)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetTemplates(templates);
        }

        /// <summary>
        /// 设置 URL 模板
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        public static HttpRequestPart SetTemplates(this string requestUrl, object templates)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetTemplates(templates);
        }

        /// <summary>
        /// 设置请求方法
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static HttpRequestPart SetHttpMethod(this string requestUrl, HttpMethod httpMethod)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetHttpMethod(httpMethod);
        }

        /// <summary>
        /// 设置请求报文头
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static HttpRequestPart SetHeaders(this string requestUrl, IDictionary<string, object> headers)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetHeaders(headers);
        }

        /// <summary>
        /// 设置请求报文头
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static HttpRequestPart SetHeaders(this string requestUrl, object headers)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetHeaders(headers);
        }

        /// <summary>
        /// 设置 URL 参数
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="queries"></param>
        /// <returns></returns>
        public static HttpRequestPart SetQueries(this string requestUrl, IDictionary<string, object> queries)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetQueries(queries);
        }

        /// <summary>
        /// 设置 URL 参数
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="queries"></param>
        /// <returns></returns>
        public static HttpRequestPart SetQueries(this string requestUrl, object queries)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetQueries(queries);
        }

        /// <summary>
        /// 设置客户端分类名
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static HttpRequestPart SetClient(this string requestUrl, string name)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetClient(name);
        }

        /// <summary>
        /// 设置 Body 内容
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static HttpRequestPart SetBody(this string requestUrl, object body, string contentType = default, Encoding encoding = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetBody(body, contentType, encoding);
        }

        /// <summary>
        /// 设置内容类型
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpRequestPart SetContentType(this string requestUrl, string contentType)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetContentType(contentType);
        }

        /// <summary>
        /// 设置内容编码
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static HttpRequestPart SetContentEncoding(this string requestUrl, Encoding encoding)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetContentEncoding(encoding);
        }

        /// <summary>
        /// 设置 Body  Bytes
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="bytesData"></param>
        /// <returns></returns>
        public static HttpRequestPart SetBodyBytes(this string requestUrl, params (string Name, byte[] Bytes, string FileName)[] bytesData)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetBodyBytes(bytesData);
        }

        /// <summary>
        /// 设置超时时间（秒）
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static HttpRequestPart SetClientTimeout(this string requestUrl, long timeout)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetClientTimeout(timeout);
        }

        /// <summary>
        /// 设置 JSON 序列化提供器
        /// </summary>
        /// <typeparam name="TJsonSerializationProvider"></typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        public static HttpRequestPart SetJsonSerialization<TJsonSerializationProvider>(this string requestUrl, object jsonSerializerOptions = default)
            where TJsonSerializationProvider : IJsonSerializerProvider
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetJsonSerialization<TJsonSerializationProvider>(jsonSerializerOptions);
        }

        /// <summary>
        /// 设置 JSON 序列化提供器
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="providerType"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        public static HttpRequestPart SetJsonSerialization(this string requestUrl, Type providerType, object jsonSerializerOptions = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetJsonSerialization(providerType, jsonSerializerOptions);
        }

        /// <summary>
        /// 是否启用验证状态
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="enabled"></param>
        /// <param name="includeNull"></param>
        /// <returns></returns>
        public static HttpRequestPart SetValidationState(this string requestUrl, bool enabled, bool includeNull = true)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetValidationState(enabled, includeNull);
        }

        /// <summary>
        /// 设置请求作用域
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static HttpRequestPart SetRequestScoped(this string requestUrl, IServiceProvider serviceProvider)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetRequestScoped(serviceProvider);
        }

        /// <summary>
        /// 构建请求对象拦截器
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HttpRequestPart OnRequesting(this string requestUrl, Action<HttpRequestMessage> action)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).OnRequesting(action);
        }

        /// <summary>
        /// 创建客户端对象拦截器
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HttpRequestPart OnClientCreating(this string requestUrl, Action<HttpClient> action)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).OnClientCreating(action);
        }

        /// <summary>
        /// 请求成功拦截器
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HttpRequestPart OnResponsing(this string requestUrl, Action<HttpResponseMessage> action)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).OnResponsing(action);
        }

        /// <summary>
        /// 请求异常拦截器
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HttpRequestPart OnException(this string requestUrl, Action<HttpResponseMessage, string> action)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).OnException(action);
        }

        /// <summary>
        /// 配置重试策略
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="numRetries"></param>
        /// <param name="retryTimeout">每次延迟时间（毫秒）</param>
        /// <returns></returns>
        public static HttpRequestPart SetRetryPolicy(this string requestUrl, int numRetries, int retryTimeout = 1000)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SetRetryPolicy(numRetries, retryTimeout);
        }

        /// <summary>
        /// 发送 GET 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<T> GetAsAsync<T>(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).GetAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求返回 Stream
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> GetAsStreamAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).GetAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求返回 String
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> GetAsStringAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).GetAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求返回 ByteArray
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> GetAsByteArrayAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).GetAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> GetAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).GetAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<T> PostAsAsync<T>(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PostAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求返回 Stream
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> PostAsStreamAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PostAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求返回 String
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> PostAsStringAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PostAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求返回 ByteArray
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> PostAsByteArrayAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PostAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PostAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<T> PutAsAsync<T>(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PutAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求返回 Stream
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> PutAsStreamAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PutAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求返回 String
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> PutAsStringAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PutAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求返回 ByteArray
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> PutAsByteArrayAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PutAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PutAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PutAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<T> DeleteAsAsync<T>(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).DeleteAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求返回 Stream
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> DeleteAsStreamAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).DeleteAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求返回 String
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> DeleteAsStringAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).DeleteAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求返回 ByteArray
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> DeleteAsByteArrayAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).DeleteAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> DeleteAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).DeleteAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<T> PatchAsAsync<T>(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PatchAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求返回 Stream
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> PatchAsStreamAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PatchAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求返回 String
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> PatchAsStringAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PatchAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求返回 ByteArray
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> PatchAsByteArrayAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PatchAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PatchAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).PatchAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 HEAD 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<T> HeadAsAsync<T>(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).HeadAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 HEAD 请求返回 Stream
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> HeadAsStreamAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).HeadAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 HEAD 请求返回 String
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> HeadAsStringAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).HeadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 HEAD 请求返回 ByteArray
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> HeadAsByteArrayAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).HeadAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 HEAD 请求
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> HeadAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).HeadAsync(cancellationToken);
        }

        /// <summary>
        /// 发送请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<T> SendAsAsync<T>(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SendAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送请求返回 Stream
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> SendAsStreamAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SendAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送请求返回 String
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> SendAsStringAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SendAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送请求返回 ByteArray
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> SendAsByteArrayAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SendAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> SendAsync(this string requestUrl, CancellationToken cancellationToken = default)
        {
            return new HttpRequestPart().SetRequestUrl(requestUrl).SendAsync(cancellationToken);
        }
    }
}