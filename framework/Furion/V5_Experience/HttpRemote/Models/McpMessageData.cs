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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Furion.HttpRemote;

/// <summary>
///     MCP (Model Context Protocol) 2.0 消息数据模型
/// </summary>
/// <remarks>用于表示 JSON-RPC 2.0 格式的请求、响应或通知消息。当作为 Server-Sent Events 事件中的 data 字段解析时，可由此类提供类型化访问。</remarks>
public sealed class McpMessageData
{
    /// <summary>
    ///     <inheritdoc cref="McpMessageData" />
    /// </summary>
    public McpMessageData()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="McpMessageData" />
    /// </summary>
    /// <param name="method">方法名称</param>
    /// <param name="params">方法参数</param>
    /// <exception cref="ArgumentException"></exception>
    public McpMessageData(string method, object? @params = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(method);

        Method = method;
        Params = @params;
    }

    /// <summary>
    ///     JSON-RPC 协议版本
    /// </summary>
    /// <remarks>固定为 "2.0"。</remarks>
    [JsonPropertyName("jsonrpc")]
    public string? JsonRpc { get; set; } = "2.0";

    /// <summary>
    ///     请求标识符
    /// </summary>
    /// <remarks>用于匹配请求与响应。若为 <c>null</c>，表示这是一个通知（Notification），无需响应。</remarks>
    [JsonPropertyName("id")]
    public object? Id { get; set; }

    /// <summary>
    ///     方法名称
    /// </summary>
    /// <remarks>对于请求或通知，此字段包含要调用的方法名（如 "tools/call"）。对于响应，此字段通常为 <c>null</c>。</remarks>
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    /// <summary>
    ///     方法参数
    /// </summary>
    /// <remarks>对于请求或通知，此字段包含调用的参数对象。对于响应，此字段通常为 <c>null</c>。</remarks>
    [JsonPropertyName("params")]
    public object? Params { get; set; }

    /// <summary>
    ///     成功响应结果
    /// </summary>
    /// <remarks>当 <see cref="Id" /> 不为 <c>null</c> 且 <see cref="Error" /> 为 <c>null</c> 时，此字段包含调用结果。</remarks>
    [JsonPropertyName("result")]
    public JsonElement? Result { get; init; }

    /// <summary>
    ///     错误信息
    /// </summary>
    /// <remarks>当 <see cref="Id" /> 不为 <c>null</c> 且 <see cref="Result" /> 为 <c>null</c> 时，此字段包含错误详情。</remarks>
    [JsonPropertyName("error")]
    public McpError? Error { get; init; }
}

/// <summary>
///     MCP JSON-RPC 错误对象
/// </summary>
/// <remarks>符合 JSON-RPC 2.0 错误对象规范。参考文献：https://www.jsonrpc.org/specification#error_object。</remarks>
public sealed class McpError
{
    /// <summary>
    ///     错误码
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; init; }

    /// <summary>
    ///     错误消息
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>
    ///     附加错误数据（可选）
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement? Data { get; init; }
}