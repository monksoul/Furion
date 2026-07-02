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

using Furion.Utilities;
using System.Xml.Serialization;

namespace Furion.HttpRemote;

/// <summary>
///     XML 对象内容转换器
/// </summary>
public class XmlObjectContentConverter : IHttpContentConverter
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
        CancellationToken cancellationToken = default) =>
        DeserializeXml(resultType, await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken));

    /// <summary>
    ///     将 XML 字符串反序列化为转换的目标类型
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="xmlString">XML 字符串</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    protected virtual object? DeserializeXml(Type resultType, string xmlString)
    {
        // 初始化 XmlSerializer 实例
        var xmlSerializer = new XmlSerializer(resultType);

        // 初始化 StringReader 实例
        using var stringReader = new StringReader(xmlString);

        // XML 反序列化为对象
        return xmlSerializer.Deserialize(stringReader);
    }
}

/// <inheritdoc cref="XmlObjectContentConverter" />
/// <typeparam name="TResult">转换的目标类型</typeparam>
public class XmlObjectContentConverter<TResult> : XmlObjectContentConverter, IHttpContentConverter<TResult>
{
    /// <inheritdoc />
    public virtual TResult? Read(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default) =>
        (TResult?)DeserializeXml(typeof(TResult),
            AsyncUtility.RunSync(() => httpResponseMessage.Content.ReadAsStringAsync(cancellationToken)));

    /// <inheritdoc />
    public virtual async Task<TResult?> ReadAsync(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default) =>
        (TResult?)DeserializeXml(typeof(TResult),
            await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken));
}