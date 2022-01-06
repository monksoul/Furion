// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using Furion.ClayObject.Extensions;
using Furion.DataValidation;
using Furion.FriendlyException;
using Furion.JsonSerialization;
using Furion.Templates.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.RemoteRequest
{
    /// <summary>
    /// HttpClient 对象组装部件
    /// </summary>
    public sealed partial class HttpRequestPart
    {
        /// <summary>
        /// 请求失败事件
        /// </summary>
        public event EventHandler<HttpRequestFaildedEventArgs> OnRequestFailded;

        /// <summary>
        /// MiniProfiler 分类名
        /// </summary>
        private const string MiniProfilerCategory = "httpclient";

        /// <summary>
        /// 发送 GET 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> GetAsAsync<T>(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Get).SendAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求返回 Stream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Stream> GetAsStreamAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Get).SendAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求返回 String
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetAsStringAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Get).SendAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求返回 ByteArray
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> GetAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Get).SendAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 GET 请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> GetAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Get).SendAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> PostAsAsync<T>(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Post).SendAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求返回 Stream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Stream> PostAsStreamAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Post).SendAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求返回 String
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> PostAsStringAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Post).SendAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求返回 ByteArray
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> PostAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Post).SendAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 POST 请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> PostAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Post).SendAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> PutAsAsync<T>(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Put).SendAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求返回 Stream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Stream> PutAsStreamAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Put).SendAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求返回 String
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> PutAsStringAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Put).SendAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求返回 ByteArray
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> PutAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Put).SendAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PUT 请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> PutAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Put).SendAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> DeleteAsAsync<T>(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Delete).SendAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求返回 Stream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Stream> DeleteAsStreamAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Delete).SendAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求返回 String
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> DeleteAsStringAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Delete).SendAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求返回 ByteArray
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> DeleteAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Delete).SendAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 DELETE 请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> DeleteAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Delete).SendAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> PatchAsAsync<T>(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Patch).SendAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求返回 Stream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Stream> PatchAsStreamAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Patch).SendAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 Patch 请求返回 String
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> PatchAsStringAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Patch).SendAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 Patch 请求返回 ByteArray
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> PatchAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Patch).SendAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 PATCH 请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> PatchAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Patch).SendAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 HEAD 请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> HeadAsAsync<T>(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Head).SendAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// 发送 HEAD 请求返回 Stream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Stream> HeadAsStreamAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Head).SendAsStreamAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 Head 请求返回 String
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> HeadAsStringAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Head).SendAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 Head 请求返回 ByteArray
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> HeadAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Head).SendAsByteArrayAsync(cancellationToken);
        }

        /// <summary>
        /// 发送 HEAD 请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> HeadAsync(CancellationToken cancellationToken = default)
        {
            return SetHttpMethod(HttpMethod.Head).SendAsync(cancellationToken);
        }

        /// <summary>
        /// 发送请求返回 T 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> SendAsAsync<T>(CancellationToken cancellationToken = default)
        {
            // 如果 T 是 HttpResponseMessage 类型，则返回
            if (typeof(T) == typeof(HttpResponseMessage))
            {
                var httpResponseMessage = await SendAsync(cancellationToken);
                return (T)(object)httpResponseMessage;
            }
            if (typeof(T) == typeof(string))
            {
                var str = await SendAsStringAsync(cancellationToken);
                return (T)(object)str;
            }

            // 读取流内容
            var stream = await SendAsStreamAsync(cancellationToken);
            if (stream == null) return default;

            // 如果 T 是 Stream 类型，则返回
            if (typeof(T) == typeof(Stream)) return (T)(object)stream;

            using var streamReader = new StreamReader(stream);
            var text = await streamReader.ReadToEndAsync();
            // 释放流
            await stream.DisposeAsync();

            // 如果字符串为空，则返回默认值
            if (string.IsNullOrWhiteSpace(text)) return default;

            // 解析 Json 序列化提供器
            var jsonSerializer = App.GetService(JsonSerializerProvider, RequestScoped ?? App.RootServices) as IJsonSerializerProvider;

            // 反序列化流
            var result = jsonSerializer.Deserialize<T>(text, JsonSerializerOptions);
            return result;
        }

        /// <summary>
        /// 发送请求返回 Stream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Stream> SendAsStreamAsync(CancellationToken cancellationToken = default)
        {
            var response = await SendAsync(cancellationToken);
            if (response == null || response.Content == null) return default;

            // 读取响应流
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return stream;
        }

        /// <summary>
        /// 发送请求返回 String
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> SendAsStringAsync(CancellationToken cancellationToken = default)
        {
            var response = await SendAsync(cancellationToken);
            if (response == null || response.Content == null) return default;

            // 读取响应报文
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return content;
        }

        /// <summary>
        /// 发送请求返回 ByteArray
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> SendAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            var response = await SendAsync(cancellationToken);
            if (response == null || response.Content == null) return default;

            // 读取响应报文
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return content;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendAsync(CancellationToken cancellationToken = default)
        {
            // 检查是否配置了请求方法
            if (Method == null) throw new NullReferenceException(nameof(Method));

            // 检查请求地址，只有 Client 不配置才检查空
            if (string.IsNullOrWhiteSpace(ClientName) && string.IsNullOrWhiteSpace(RequestUrl)) throw new NullReferenceException(RequestUrl);

            // 处理模板问题
            RequestUrl = RequestUrl.Render(Templates, true);

            // 构建请求对象
            var request = new HttpRequestMessage(Method, RequestUrl);
            request.AppendQueries(Queries);

            // 设置请求报文头
            if (Headers != null)
            {
                foreach (var header in Headers)
                {
                    if (header.Value != null) request.Headers.Add(header.Key, header.Value.ToString());
                }
            }

            // 验证模型参数（只作用于 body 类型）
            if (ValidationState.Enabled)
            {
                // 判断是否启用 Null 验证且 body 值为 null
                if (ValidationState.IncludeNull && Body == null) throw new InvalidOperationException($"{nameof(Body)} can not be null.");

                // 验证模型
                Body?.Validate();
            }

            // 设置 HttpContent
            SetHttpContent(request);

            // 配置请求拦截
            RequestInterceptors.ForEach(u =>
            {
                u?.Invoke(request);
            });

            // 创建客户端请求工厂
            var clientFactory = App.GetService<IHttpClientFactory>(RequestScoped ?? App.RootServices);
            if (clientFactory == null) throw new InvalidOperationException("please add `services.AddRemoteRequest()` in Startup.cs.");

            // 创建 HttpClient 对象
            using var httpClient = string.IsNullOrWhiteSpace(ClientName)
                                         ? clientFactory.CreateClient()
                                         : clientFactory.CreateClient(ClientName);

            // 只有大于 0 才设置超时时间
            if (Timeout > 0)
            {
                // 设置请求超时时间
                httpClient.Timeout = TimeSpan.FromSeconds(Timeout);
            }

            // 判断命名客户端是否配置了 BaseAddress，且必须以 / 结尾
            var httpClientOriginalString = httpClient.BaseAddress?.OriginalString;
            if (!string.IsNullOrWhiteSpace(httpClientOriginalString) && !httpClientOriginalString.EndsWith("/"))
                throw new InvalidOperationException($"The `{ClientName}` of HttpClient BaseAddress must be end with '/'.");

            // 添加默认 User-Agent
            if (!httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                                 "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.101 Safari/537.36 Edg/91.0.864.48");
            }

            // 配置 HttpClient 拦截
            HttpClientInterceptors.ForEach(u =>
            {
                u?.Invoke(httpClient);
            });

            // 打印发送请求
            App.PrintToMiniProfiler(MiniProfilerCategory, "Sending", $"[{Method}] {httpClientOriginalString}{request.RequestUri?.OriginalString}");

            // 捕获异常
            Exception exception = default;
            HttpResponseMessage response = default;

            try
            {
                if (RetryPolicy == null) response = await httpClient.SendAsync(request, cancellationToken);
                else
                {
                    // 失败重试
                    await Retry.Invoke(async () =>
                    {
                        // 发送请求
                        response = await httpClient.SendAsync(request, cancellationToken);
                    }, RetryPolicy.Value.NumRetries, RetryPolicy.Value.RetryTimeout);
                }
            }
            catch (Exception ex)
            {
                // 触发自定义事件
                if (response != null) OnRequestFailded(this, new HttpRequestFaildedEventArgs(request, response, ex));

                exception = ex;
            }

            // 请求成功
            if (response?.IsSuccessStatusCode == true && exception == default)
            {
                // 打印成功请求
                App.PrintToMiniProfiler(MiniProfilerCategory, "Succeeded", $"[StatusCode: {response.StatusCode}] Succeeded");

                // 调用成功拦截器
                ResponseInterceptors.ForEach(u =>
                {
                    u?.Invoke(response);
                });
            }
            // 请求异常
            else
            {
                // 读取错误消息
                var errors = exception == null && response != null
                    ? await response.Content.ReadAsStringAsync(cancellationToken)
                    : exception?.Message;

                // 打印失败请求
                App.PrintToMiniProfiler(MiniProfilerCategory, "Failed", $"[StatusCode: {response?.StatusCode}] {errors}", true);

                // 抛出异常
                if (ExceptionInterceptors == null || ExceptionInterceptors.Count == 0) throw new HttpRequestException(errors);
                // 调用异常拦截器
                else ExceptionInterceptors.ForEach(u =>
                {
                    u?.Invoke(response, errors);
                });
            }

            return response;
        }

        /// <summary>
        /// 设置 HttpContent
        /// </summary>
        /// <param name="request"></param>
        private void SetHttpContent(HttpRequestMessage request)
        {
            // GET/HEAD 请求不支持设置 Body 请求
            if (Method == HttpMethod.Get || Method == HttpMethod.Head) return;

            HttpContent httpContent = null;

            // 处理各种 Body 类型
            switch (ContentType)
            {
                case "multipart/form-data":
                    var multipartFormDataContent = new MultipartFormDataContent();

                    // 添加 Bytes 类型
                    foreach (var (Name, Bytes, FileName) in BodyBytes)
                    {
                        if (string.IsNullOrWhiteSpace(FileName))
                            multipartFormDataContent.Add(new ByteArrayContent(Bytes), Name);
                        else
                            multipartFormDataContent.Add(new ByteArrayContent(Bytes), Name, FileName);
                    }

                    // 处理其他类型
                    var dic = ConvertBodyToDictionary();
                    if (dic != null && dic.Count > 0)
                    {
                        foreach (var (key, value) in dic)
                        {
                            multipartFormDataContent.Add(new StringContent(value, ContentEncoding), string.Format("\"{0}\"", key));
                        }
                    }

                    // 设置内容类型
                    //multipartFormDataContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
                    httpContent = multipartFormDataContent;
                    break;

                case "application/octet-stream":
                    if (BodyBytes.Count > 0 && BodyBytes[0].Bytes.Length > 0)
                    {
                        httpContent = new ByteArrayContent(BodyBytes[0].Bytes);

                        // 设置内容类型
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
                    }
                    break;

                case "application/json":
                case "text/json":
                case "application/*+json":
                    if (Body != null)
                    {
                        httpContent = new StringContent(SerializerObject(Body), ContentEncoding);

                        // 设置内容类型
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
                    }
                    break;

                case "application/x-www-form-urlencoded":
                    // 解析字典
                    var keyValues = ConvertBodyToDictionary();

                    if (keyValues == null || keyValues.Count == 0) return;

                    // 设置内容类型
                    httpContent = new FormUrlEncodedContent(keyValues);
                    break;

                case "application/xml":
                case "text/xml":
                    if (Body != null) httpContent = new StringContent(Body.ToString(), ContentEncoding, ContentType);
                    break;

                default:
                    // 其他类型可通过 `HttpRequestMessage` 拦截器设置
                    break;
            }

            // 设置 HttpContent
            if (httpContent != null) request.Content = httpContent;
        }

        /// <summary>
        /// 转换 Body 为 字典类型
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, string> ConvertBodyToDictionary()
        {
            IDictionary<string, string> keyValues = null;
            if (Body == null) return default;

            // 处理各种情况
            if (Body is IDictionary<string, string> dic) keyValues = dic;
            else if (Body is IDictionary<string, object> dicObj) keyValues = dicObj.ToDictionary(u => u.Key, u => SerializerObject(u.Value));
            else keyValues = Body.ToDictionary().ToDictionary(u => u.Key, u => SerializerObject(u.Value));
            return keyValues;
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private string SerializerObject(object body)
        {
            if (body == null) return default;
            if (body is string) return body as string;
            if (body.GetType().IsValueType) return body.ToString();

            // 解析序列化工具
            var jsonSerializer = App.GetService(JsonSerializerProvider, RequestScoped ?? App.RootServices) as IJsonSerializerProvider;
            return jsonSerializer.Serialize(body, JsonSerializerOptions);
        }
    }
}