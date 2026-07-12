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

using Furion.Converters.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求选项
/// </summary>
public sealed class HttpRemoteOptions
{
    /// <summary>
    ///     默认 JSON 序列化配置
    /// </summary>
    /// <remarks>参考文献：https://learn.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json/configure-options。</remarks>
    public static readonly JsonSerializerOptions JsonSerializerOptionsDefault = new(JsonSerializerOptions.Default)
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // 允许 String 转 Number
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        // 解决中文乱码问题
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        AllowTrailingCommas = true,
        Converters =
        {
            new FlexibleDateTimeConverter(),
            new FlexibleDateTimeOffsetConverter(),
            // 允许 Number 或 Boolean 转 String
            new StringJsonConverter()
        }
    };

    /// <summary>
    ///     默认请求内容类型
    /// </summary>
    public string? DefaultContentType { get; set; } = MediaTypeNames.Text.Plain;

    /// <summary>
    ///     默认文件下载保存目录
    /// </summary>
    public string? DefaultFileDownloadDirectory { get; set; }

    /// <summary>
    ///     请求分析工具日志级别
    /// </summary>
    /// <remarks>默认值为 <see cref="LogLevel.Warning" /></remarks>
    public LogLevel ProfilerLogLevel { get; set; } = LogLevel.Warning;

    /// <summary>
    ///     指示请求是否应遵循重定向响应
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool AllowAutoRedirect { get; set; } = true;

    /// <summary>
    ///     请求所遵循的最大重定向数
    /// </summary>
    /// <remarks>默认值为：50 次。</remarks>
    public int MaximumAutomaticRedirections { get; set; } = 50;

    /// <summary>
    ///     回退请求基地址
    /// </summary>
    /// <remarks>当未配置 <see cref="HttpClient" /> 的 <see cref="HttpClient.BaseAddress" /> 且请求地址为相对地址时使用。</remarks>
    public Uri? FallbackBaseAddress { get; set; }

    /// <summary>
    ///     JSON 序列化配置
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new(JsonSerializerOptionsDefault);

    /// <summary>
    ///     <inheritdoc cref="IConfiguration" />
    /// </summary>
    /// <remarks>支持作为替换 URL 地址中配置模板参数的提供源。</remarks>
    public IConfiguration? Configuration { get; set; }

    /// <summary>
    ///     URL 参数格式化程序
    /// </summary>
    public IUrlParameterFormatter? UrlParameterFormatter { get; set; } = new UrlParameterFormatter();

    /// <summary>
    ///     <see cref="HttpRequestBuilder" /> 统一配置器
    /// </summary>
    /// <remarks>用于在构建 <see cref="HttpRequestMessage" /> 时调用。</remarks>
    public IHttpRequestBuilderConfigurer? HttpRequestBuilderConfigurer { get; set; }

    /// <summary>
    ///     未注册日志服务时的备用日志输出委托
    /// </summary>
    public Action<string?>? FallbackLogger { get; set; } = Console.WriteLine;

    /// <summary>
    ///     自定义 HTTP 声明式 <see cref="IHttpDeclarativeExtractor" /> 集合提供器
    /// </summary>
    /// <value>返回多个包含实现 <see cref="IHttpDeclarativeExtractor" /> 集合的集合。</value>
    internal IReadOnlyList<Func<IEnumerable<IHttpDeclarativeExtractor>>>? HttpDeclarativeExtractors { get; set; }

    /// <summary>
    ///     自定义 <see cref="IHttpRequestPipelineHandler" /> 类型集合
    /// </summary>
    /// <remarks>
    ///     <para>该列表定义了发送 HTTP 请求时执行的处理器及其顺序。</para>
    ///     <para>顺序从外到内：列表中的第一个类型位于管道最外层（最先执行），最后一个类型位于最内层（最后执行）。</para>
    ///     <para>TODO: 未来考虑开放拓展</para>
    /// </remarks>
    internal IList<Type> PipelineHandlerTypes { get; } = new List<Type>
    {
        typeof(ResponseAssertionPipelineHandler),
        typeof(ResponseProfilerPipelineHandler),
        typeof(RequestEventPipelineHandler),
        typeof(TimeoutPipelineHandler),
        typeof(AutoRedirectPipelineHandler),
        typeof(StatusCodePipelineHandler),
        typeof(ContentLengthValidationPipelineHandler),
        typeof(RequestBuilderPipelineHandler),
        typeof(RequestProfilerPipelineHandler),
        typeof(SendCorePipelineHandler)
    };
}