using Fur.DependencyInjection;

namespace Fur.RemoteRequest
{
    /// <summary>
    /// 远程客户端配置
    /// </summary>
    [SkipScan]
    public sealed class HttpClientConfigure
    {
        /// <summary>
        /// 基础地址
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// 默认请求头
        /// </summary>
        public string[] DefaultRequestHeaders { get; set; }
    }
}