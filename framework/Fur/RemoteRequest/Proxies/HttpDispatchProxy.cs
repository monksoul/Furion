using Fur.DependencyInjection;
using Fur.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Fur.RemoteRequest
{
    /// <summary>
    /// 请求拦截代理
    /// </summary>
    [SkipScan]
    public class HttpDispatchProxy : DispatchProxy, IDispatchProxy
    {
        /// <summary>
        /// MiniProfiler 分类名
        /// </summary>
        private const string MiniProfilerCategory = "httpRequest";

        /// <summary>
        /// 实例对象
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// 服务提供器
        /// </summary>
        public IServiceProvider Services { get; set; }

        /// <summary>
        /// 拦截
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            // 获取方法真实返回值
            var returnType = targetMethod.GetMethodRealReturnType();

            // 判断是否贴了 [HttpMethod] 特性
            if (!targetMethod.IsDefined(typeof(HttpMethodAttribute), true)) throw new InvalidOperationException("The method is missing the [HttpMethod] annotation");

            // 获取请求方式
            var httpMethodAttribute = targetMethod.GetCustomAttribute<HttpMethodAttribute>(true);

            // 处理参数
            var methodParameters = targetMethod.GetParameters();
            var realUrl = httpMethodAttribute.Url;
            for (int i = 0; i < methodParameters.Length; i++)
            {
                var parameter = methodParameters[i];
                var value = args[i];
                if (value != null)
                {
                    realUrl = realUrl.Replace($"{{{parameter.Name}}}", args[i].ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }

            // 创建请求消息对象
            var request = new HttpRequestMessage(httpMethodAttribute.Method, realUrl);

            // 获取请求报文头
            var headerAttributes = targetMethod.GetCustomAttributes<HeaderAttribute>(true);
            foreach (var headerAttribute in headerAttributes)
            {
                request.Headers.Add(headerAttribute.Key, headerAttribute.Value);
            }

            // 打印请求地址
            App.PrintToMiniProfiler(MiniProfilerCategory, "Beginning", $"{request.Method.Method} {request.RequestUri}");

            // 创建 HttpClient 对象
            var clientFactory = Services.GetService<IHttpClientFactory>();
            var httpClient = clientFactory.CreateClient();

            // 发送请求
            var response = httpClient.SendAsync(request).GetAwaiter().GetResult();

            // 判断是否请求成功
            if (response.IsSuccessStatusCode)
            {
                // 打印成功消息
                App.PrintToMiniProfiler(MiniProfilerCategory, "Successed");

                // 处理无返回值情况
                if (returnType == typeof(void)) return default;
                else
                {
                    // 读取数据并序列化返回
                    using var responseStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                    var result = JsonSerializer.DeserializeAsync(responseStream
                        , returnType, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        }).GetAwaiter().GetResult();

                    return !targetMethod.IsAsync() ? result : result.ToTaskResult(returnType);
                }
            }
            else
            {
                // 打印成功消息
                App.PrintToMiniProfiler(MiniProfilerCategory, "Failed", isError: true);

                return default;
            }
        }
    }
}