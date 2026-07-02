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

using Furion.HttpRemote.Extensions;
using Furion.Utilities;
using System.Net.Http.Json;

namespace Furion.HttpRemote;

/// <summary>
///     对象内容转换器
/// </summary>
/// <remarks>默认作为 JSON 对象内容转换器。</remarks>
public class ObjectContentConverter : IHttpContentConverter
{
    /// <inheritdoc />
    public virtual bool KeepsResponseAlive => false;

    /// <inheritdoc />
    public IServiceProvider? ServiceProvider { get; set; }

    /// <inheritdoc />
    public virtual object? Read(Type resultType, HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default) =>
        AsyncUtility.RunSync(() => ReadAsync(resultType, httpResponseMessage, cancellationToken));

    /// <inheritdoc />
    public virtual async Task<object?> ReadAsync(Type resultType, HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default)
    {
        // 解析 HttpClient 客户端对应的 JSON 序列化上下文信息
        var jsonSerializationContext =
            HttpRemoteUtility.ResolveJsonSerializationContext(resultType, httpResponseMessage, ServiceProvider);

        // 获取 JSON 反序列化的值
        var deserializedValue = !httpResponseMessage.ShouldJsonResponseStringUnwrap()
            ? await httpResponseMessage.Content.ReadFromJsonAsync(jsonSerializationContext.ResultType,
                jsonSerializationContext.JsonSerializerOptions, cancellationToken)
            // 解析经过双重序列化的 JSON 字符串，并将其反序列化为指定类型
            : await httpResponseMessage.Content.ReadAndUnwrapFromJsonAsync(jsonSerializationContext.ResultType,
                jsonSerializationContext.JsonSerializerOptions, cancellationToken);

        // 获取转换的目标类型值
        return jsonSerializationContext.GetResultValue(deserializedValue, httpResponseMessage);
    }
}

/// <inheritdoc cref="ObjectContentConverter" />
/// <typeparam name="TResult">转换的目标类型</typeparam>
public class ObjectContentConverter<TResult> : ObjectContentConverter, IHttpContentConverter<TResult>
{
    /// <inheritdoc />
    public virtual TResult? Read(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default) =>
        (TResult?)base.Read(typeof(TResult), httpResponseMessage, cancellationToken);

    /// <inheritdoc />
    public virtual async Task<TResult?> ReadAsync(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default) =>
        (TResult?)await base.ReadAsync(typeof(TResult), httpResponseMessage, cancellationToken);
}