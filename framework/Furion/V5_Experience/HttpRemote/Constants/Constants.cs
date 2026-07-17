// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求模块常量配置
/// </summary>
internal static class Constants
{
    /// <summary>
    ///     请求跟踪标识标头
    /// </summary>
    internal const string X_TRACE_ID_HEADER = "X-Trace-ID";

    /// <summary>
    ///     未知 <c>User Agent</c> 版本
    /// </summary>
    internal const string UNKNOWN_USER_AGENT_VERSION = "unknown";

    /// <summary>
    ///     Basic 授权标识
    /// </summary>
    internal const string BASIC_AUTHENTICATION_SCHEME = "Basic";

    /// <summary>
    ///     JWT (JSON Web Token) 授权标识
    /// </summary>
    internal const string JWT_BEARER_AUTHENTICATION_SCHEME = "Bearer";

    /// <summary>
    ///     Digest 授权标识
    /// </summary>
    internal const string DIGEST_AUTHENTICATION_SCHEME = "Digest";

    /// <summary>
    ///     响应结束符标头
    /// </summary>
    internal const string X_END_OF_STREAM_HEADER = "X-End-Of-Stream";

    /// <summary>
    ///     请求原始地址标头
    /// </summary>
    internal const string X_ORIGINAL_URL_HEADER = "X-Original-URL";

    /// <summary>
    ///     请求转发目标地址标头
    /// </summary>
    internal const string X_FORWARD_TO_HEADER = "X-Forward-To";

    /// <summary>
    ///     压力测试标头
    /// </summary>
    internal const string X_STRESS_TEST_HEADER = "X-Stress-Test";

    /// <summary>
    ///     压力测试标头值
    /// </summary>
    internal const string X_STRESS_TEST_VALUE = "Harness";

    /// <summary>
    ///     禁用请求分析工具键
    /// </summary>
    /// <remarks>被用于从 <see cref="HttpRequestMessage" /> 的 <c>Options</c> 属性中读取。</remarks>
    internal const string DISABLE_PROFILER_KEY = "__DISABLE_PROFILER__";

    /// <summary>
    ///     请求分析工具打印标识键
    /// </summary>
    /// <remarks>解决重复打印问题。被用于从 <see cref="HttpRequestMessage" /> 的 <c>Options</c> 属性中读取。</remarks>
    internal const string PROFILER_PRINTED_KEY = "__PROFILER_PRINTED__";

    /// <summary>
    ///     启用 JSON 响应反序列化包装器键
    /// </summary>
    /// <remarks>被用于从 <see cref="HttpRequestMessage" /> 的 <c>Options</c> 属性中读取。</remarks>
    internal const string ENABLE_JSON_RESPONSE_WRAPPER_KEY = "__ENABLE_JSON_RESPONSE_WRAPPER__";

    /// <summary>
    ///     启用 JSON 响应内容字符串的解包处理（双重序列化）
    /// </summary>
    /// <remarks>被用于从 <see cref="HttpRequestMessage" /> 的 <c>Options</c> 属性中读取。</remarks>
    internal const string ENABLE_JSON_RESPONSE_STRING_UNWRAP_KEY = "__ENABLE_JSON_RESPONSE_STRING_UNWRAP__";

    /// <summary>
    ///     HTTP 声明式请求方法签名键
    /// </summary>
    /// <remarks>被用于从 <see cref="HttpRequestMessage" /> 的 <c>Options</c> 属性中读取。</remarks>
    internal const string DECLARATIVE_METHOD_KEY = "__DECLARATIVE_METHOD__";

    /// <summary>
    ///     HTTP 请求 <see cref="HttpClient" /> 实例的配置名称键
    /// </summary>
    /// <remarks>被用于从 <see cref="HttpRequestMessage" /> 的 <c>Options</c> 属性中读取。</remarks>
    internal const string HTTP_CLIENT_NAME = "__HTTP_CLIENT_NAME__";

    /// <summary>
    ///     <c>Referer</c> 标头请求基地址模板
    /// </summary>
    internal const string REFERER_HEADER_BASE_ADDRESS_TEMPLATE = "{BASE_ADDRESS}";

    /// <summary>
    ///     请求管道上下文中请求分析工具的键
    /// </summary>
    /// <remarks>用于在 <see cref="HttpRequestPipelineContext.Items" /> 中存储或获取 <see cref="HttpRemoteAnalyzer" /> 实例。</remarks>
    internal const string PROFILER_ANALYZER_KEY = "ProfilerAnalyzer";

    /// <summary>
    ///     请求管道上下文中请求事件处理程序的键
    /// </summary>
    /// <remarks>用于在 <see cref="HttpRequestPipelineContext.Items" /> 中存储或获取 <see cref="IHttpRequestEventHandler" /> 实例。</remarks>
    internal const string REQUEST_EVENT_HANDLER_KEY = "RequestEventHandler";
}