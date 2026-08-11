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

namespace Furion.HttpRemote.Extensions;

/// <summary>
///     HTTP 远程服务 Server Sent Events 扩展类
/// </summary>
public static class HttpRemoteServerSentEventsExtensions
{
    /// <summary>
    ///     将 <see cref="ServerSentEventsData" /> 解析为 <see cref="McpMessageData" />
    /// </summary>
    /// <param name="serverSentEventsData">
    ///     <see cref="ServerSentEventsData" />
    /// </param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="McpMessageData" />
    /// </returns>
    public static McpMessageData? ToMcpMessage(this ServerSentEventsData serverSentEventsData,
        JsonSerializerOptions? jsonSerializerOptions = null) =>
        string.IsNullOrWhiteSpace(serverSentEventsData?.Data)
            ? null
            : JsonSerializer.Deserialize<McpMessageData>(serverSentEventsData.Data,
                jsonSerializerOptions ?? HttpRemoteOptions.JsonSerializerOptionsDefault);

    /// <summary>
    ///     将 <see cref="McpMessageData.Result" /> 转换为指定类型
    /// </summary>
    /// <param name="mcpMessageData">
    ///     <see cref="McpMessageData" />
    /// </param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    public static T? GetResult<T>(this McpMessageData mcpMessageData,
        JsonSerializerOptions? jsonSerializerOptions = null) =>
        mcpMessageData?.Result is null
            ? default
            : mcpMessageData.Result.Value.Deserialize<T>(jsonSerializerOptions ??
                                                         HttpRemoteOptions.JsonSerializerOptionsDefault);

    /// <summary>
    ///     将 <see cref="McpError.Data" /> 转换为指定类型
    /// </summary>
    /// <param name="mcpError">
    ///     <see cref="McpError" />
    /// </param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    public static T? GetData<T>(this McpError mcpError, JsonSerializerOptions? jsonSerializerOptions = null) =>
        mcpError?.Data is null
            ? default
            : mcpError.Data.Value.Deserialize<T>(
                jsonSerializerOptions ?? HttpRemoteOptions.JsonSerializerOptionsDefault);
}