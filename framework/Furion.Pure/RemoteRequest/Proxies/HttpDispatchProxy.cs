// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using Furion.ClayObject.Extensions;
using Furion.DataValidation;
using Furion.DependencyInjection;
using Furion.Extensions;
using Furion.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Furion.RemoteRequest
{
    /// <summary>
    /// 远程请求实现类（以下代码还需进一步优化性能，启动时把所有扫描缓存起来）
    /// </summary>
    [SuppressSniffer]
    public class HttpDispatchProxy : AspectDispatchProxy, IDispatchProxy
    {
        /// <summary>
        /// 被代理对象
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// 服务提供器
        /// </summary>
        public IServiceProvider Services { get; set; }

        /// <summary>
        /// 拦截同步方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object Invoke(MethodInfo method, object[] args)
        {
            throw new NotSupportedException("Please use asynchronous operation mode.");
        }

        /// <summary>
        /// 拦截异步无返回方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async override Task InvokeAsync(MethodInfo method, object[] args)
        {
            var httpRequestPart = BuildHttpRequestPart(method, args);
            _ = await httpRequestPart.SendAsync();
        }

        /// <summary>
        /// 拦截异步带返回方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
        {
            var httpRequestPart = BuildHttpRequestPart(method, args);
            var result = httpRequestPart.SendAsAsync<T>();
            return result;
        }

        /// <summary>
        /// 构建 HttpClient 请求部件
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private HttpRequestPart BuildHttpRequestPart(MethodInfo method, object[] args)
        {
            // 判断方法是否是远程代理请求方法
            if (!method.IsDefined(typeof(HttpMethodBaseAttribute), true)) throw new InvalidOperationException($"{method.Name} is not a valid request proxy method.");

            // 解析方法参数及参数值
            var parameters = method.GetParameters().Select((u, i) => new MethodParameterInfo
            {
                Parameter = u,
                Name = u.Name,
                Value = args[i]
            });

            // 获取请求配置
            var httpMethodBase = method.GetCustomAttribute<HttpMethodBaseAttribute>(true);

            // 创建请求配置对象
            var httpRequestPart = new HttpRequestPart();
            httpRequestPart.SetRequestUrl(httpMethodBase.RequestUrl)
                          .SetHttpMethod(httpMethodBase.Method)
                          .SetTemplates(parameters.ToDictionary(u => u.Name, u => u.Value))
                          .SetRequestScoped(Services);

            // 设置请求客户端
            var clientAttribute = method.GetFoundAttribute<ClientAttribute>(true);
            if (clientAttribute != null) httpRequestPart.SetClient(clientAttribute.Name);

            // 设置请求超时时间
            var timeout = method.GetFoundAttribute<TimeoutAttribute>(true)?.Seconds;
            if (timeout != null && timeout.Value > 0) httpRequestPart.SetClientTimeout(timeout.Value);

            // 设置请求报文头
            SetHeaders(method, parameters, httpRequestPart);

            // 设置 Url 地址参数
            SetQueries(parameters, httpRequestPart);

            // 设置 Body 信息
            SetBody(parameters, httpRequestPart);

            // 设置验证
            SetValidation(parameters);

            // 设置序列化提供器
            SetJsonSerialization(method, parameters, httpRequestPart);

            // 配置全局拦截
            CallGlobalInterceptors(httpRequestPart, method.DeclaringType);

            // 设置请求拦截
            SetInterceptors(parameters, httpRequestPart);

            // 设置重试
            var retryPolicyAttribute = method.GetFoundAttribute<RetryPolicyAttribute>(true);
            if (retryPolicyAttribute != null) httpRequestPart.SetRetryPolicy(retryPolicyAttribute.NumRetries, retryPolicyAttribute.RetryTimeout);

            return httpRequestPart;
        }

        /// <summary>
        /// 设置 Url 地址参数
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="httpRequestPart"></param>
        private static void SetQueries(IEnumerable<MethodParameterInfo> parameters, HttpRequestPart httpRequestPart)
        {
            // 配置 Url 地址参数
            var queryParameters = parameters.Where(u => u.Parameter.IsDefined(typeof(QueryStringAttribute), true));
            var parameterQueries = new Dictionary<string, object>();
            foreach (var item in queryParameters)
            {
                var queryStringAttribute = item.Parameter.GetCustomAttribute<QueryStringAttribute>();
                if (item.Value != null)
                {
                    // 处理基元类型
                    if (item.Value.GetType().IsRichPrimitive())
                    {
                        parameterQueries.Add(queryStringAttribute.Alias ?? item.Name, item.Value);
                    }
                    // 处理类类型
                    else
                    {
                        parameterQueries.AddOrUpdate(item.Value.ToDictionary());
                    }
                }
            }
            httpRequestPart.SetQueries(parameterQueries);
        }

        /// <summary>
        /// 设置 Body 参数
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="httpRequestPart"></param>
        private static void SetBody(IEnumerable<MethodParameterInfo> parameters, HttpRequestPart httpRequestPart)
        {
            // 配置 Body 参数，只取第一个
            var bodyParameter = parameters.FirstOrDefault(u => u.Parameter.IsDefined(typeof(BodyAttribute), true));
            if (bodyParameter != null)
            {
                var bodyAttribute = bodyParameter.Parameter.GetCustomAttribute<BodyAttribute>(true);
                httpRequestPart.SetBody(bodyParameter.Value, bodyAttribute.ContentType, Encoding.GetEncoding(bodyAttribute.Encoding));
            }

            // 查找所有贴了 [BodyBytes] 特性的参数
            var bodyBytesParameters = parameters.Where(u => u.Parameter.IsDefined(typeof(BodyBytesAttribute), true));
            if (bodyBytesParameters != null)
            {
                var bodyBytes = new List<(string Name, byte[] Bytes, string FileName)>();
                foreach (var item in bodyBytesParameters)
                {
                    var bodyBytesAttribute = item.Parameter.GetCustomAttribute<BodyBytesAttribute>();
                    if (item.Value != null && item.Value.GetType() == typeof(byte[])) bodyBytes.Add((bodyBytesAttribute.Alias ?? item.Name, (byte[])item.Value, bodyBytesAttribute.FileName));
                }

                httpRequestPart.SetBodyBytes(bodyBytes.ToArray());
            }
        }

        /// <summary>
        /// 设置验证
        /// </summary>
        /// <param name="parameters"></param>
        private static void SetValidation(IEnumerable<MethodParameterInfo> parameters)
        {
            // 验证参数，查询所有配置验证特性的参数，排除 Body 验证
            var validateParameters = parameters.Where(u => u.Parameter.IsDefined(typeof(ValidationAttribute), true) && !u.Parameter.IsDefined(typeof(BodyAttribute), true));
            foreach (var item in validateParameters)
            {
                // 处理空值
                var isRequired = item.Parameter.IsDefined(typeof(RequiredAttribute), true);
                if (isRequired && item.Value == null) throw new InvalidOperationException($"{item.Name} can not be null.");

                // 判断是否是基元类型
                if (item.Parameter.ParameterType.IsRichPrimitive())
                {
                    var validationAttributes = item.Parameter.GetCustomAttributes<ValidationAttribute>(true);
                    item.Value?.Validate(validationAttributes.ToArray());
                }
                else item.Value?.Validate();
            }
        }

        /// <summary>
        /// 设置序列化
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="httpRequestPart"></param>
        private static void SetJsonSerialization(MethodInfo method, IEnumerable<MethodParameterInfo> parameters, HttpRequestPart httpRequestPart)
        {
            // 判断方法是否自定义序列化选项
            var jsonSerializerOptions = parameters.FirstOrDefault(u => u.Parameter.IsDefined(typeof(JsonSerializerOptionsAttribute), true))?.Value
                                                        // 获取静态方法且贴有 [JsonSerializerOptions] 特性的缺省配置
                                                        ?? method.DeclaringType.GetMethods()
                                                                               .FirstOrDefault(u => u.IsDefined(typeof(JsonSerializerOptionsAttribute), true))
                                                                               ?.Invoke(null, null);

            // 查询自定义序列化提供器，如果没找到，默认 SystemTextJsonSerializerProvider
            var jsonSerializerProvider = method.GetFoundAttribute<JsonSerializationAttribute>(true)?.ProviderType;
            httpRequestPart.SetJsonSerialization(jsonSerializerProvider, jsonSerializerOptions);
        }

        /// <summary>
        /// 调用全局拦截
        /// </summary>
        /// <param name="httpRequestPart"></param>
        /// <param name="declaringType"></param>
        private static void CallGlobalInterceptors(HttpRequestPart httpRequestPart, Type declaringType)
        {
            // 获取所有静态方法且贴有 [Interceptor] 特性
            var interceptorMethods = declaringType.GetMethods()
                                                                  .Where(u => u.IsDefined(typeof(InterceptorAttribute), true));

            foreach (var method in interceptorMethods)
            {
                // 获取拦截器类型
                var interceptor = method.GetCustomAttributes<InterceptorAttribute>().First();
                switch (interceptor.Type)
                {
                    // 加载请求拦截
                    case InterceptorTypes.Request:
                        var onRequesting = (Action<HttpRequestMessage>)Delegate.CreateDelegate(typeof(Action<HttpRequestMessage>), method);
                        httpRequestPart.OnRequesting(onRequesting);
                        break;
                    // 加载响应拦截
                    case InterceptorTypes.Response:
                        var onResponsing = (Action<HttpResponseMessage>)Delegate.CreateDelegate(typeof(Action<HttpResponseMessage>), method);
                        httpRequestPart.OnResponsing(onResponsing);
                        break;
                    // 加载 Client 配置拦截
                    case InterceptorTypes.Client:
                        var onClientCreating = (Action<HttpClient>)Delegate.CreateDelegate(typeof(Action<HttpClient>), method);
                        httpRequestPart.OnClientCreating(onClientCreating);
                        break;
                    // 加载异常拦截
                    case InterceptorTypes.Exception:
                        var onException = (Action<HttpResponseMessage, string>)Delegate.CreateDelegate(typeof(Action<HttpResponseMessage, string>), method);
                        httpRequestPart.OnException(onException);
                        break;

                    default: break;
                }
            }
        }

        /// <summary>
        /// 设置请求拦截
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="httpRequestPart"></param>
        private static void SetInterceptors(IEnumerable<MethodParameterInfo> parameters, HttpRequestPart httpRequestPart)
        {
            // 添加方法拦截器
            var Interceptors = parameters.Where(u => u.Parameter.IsDefined(typeof(InterceptorAttribute), true));
            foreach (var item in Interceptors)
            {
                // 获取拦截器类型
                var interceptor = item.Parameter.GetCustomAttribute<InterceptorAttribute>();
                switch (interceptor.Type)
                {
                    // 加载请求拦截
                    case InterceptorTypes.Request:
                        if (item.Value is Action<HttpRequestMessage> onRequesting)
                        {
                            httpRequestPart.OnRequesting(onRequesting);
                        }
                        break;
                    // 加载响应拦截
                    case InterceptorTypes.Response:
                        if (item.Value is Action<HttpResponseMessage> onResponsing)
                        {
                            httpRequestPart.OnResponsing(onResponsing);
                        }
                        break;
                    // 加载 Client 配置拦截
                    case InterceptorTypes.Client:
                        if (item.Value is Action<HttpClient> onClientCreating)
                        {
                            httpRequestPart.OnClientCreating(onClientCreating);
                        }
                        break;
                    // 加载异常拦截
                    case InterceptorTypes.Exception:
                        if (item.Value is Action<HttpResponseMessage, string> onException)
                        {
                            httpRequestPart.OnException(onException);
                        }
                        break;

                    default: break;
                }
            }
        }

        /// <summary>
        /// 设置请求报文头
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="httpRequestPart"></param>
        private static void SetHeaders(MethodInfo method, IEnumerable<MethodParameterInfo> parameters, HttpRequestPart httpRequestPart)
        {
            var declaringType = method.DeclaringType;

            // 获取声明类请求报文头
            var declaringTypeHeaders = (declaringType.IsDefined(typeof(HeadersAttribute), true)
                ? declaringType.GetCustomAttributes<HeadersAttribute>(true)
                : Array.Empty<HeadersAttribute>()).ToDictionary(u => u.Key, u => u.Value);

            // 获取方法请求报文头
            var methodHeaders = (method.IsDefined(typeof(HeadersAttribute), true)
                ? method.GetCustomAttributes<HeadersAttribute>(true)
                : Array.Empty<HeadersAttribute>()).ToDictionary(u => u.Key, u => u.Value);

            // 获取参数请求报文头
            var headerParameters = parameters.Where(u => u.Parameter.IsDefined(typeof(HeadersAttribute), true));
            var parameterHeaders = new Dictionary<string, object>();
            foreach (var item in headerParameters)
            {
                var headersAttribute = item.Parameter.GetCustomAttribute<HeadersAttribute>(true);
                if (item.Value != null)
                {
                    // 处理参数传入 [Headers] IDictionary<string, object>  情况
                    if (item.Value is IDictionary<string, object> dicHeaders)
                    {
                        foreach (var (key, value) in dicHeaders)
                        {
                            parameterHeaders.Add(key, value);
                        }
                    }
                    else
                    {
                        parameterHeaders.Add(headersAttribute.Key ?? item.Name, item.Value);
                    }
                }
            }

            // 合并所有请求报文头
            var headers = declaringTypeHeaders.AddOrUpdate(methodHeaders)
                                                                  .AddOrUpdate(parameterHeaders);
            httpRequestPart.SetHeaders(headers);
        }
    }
}