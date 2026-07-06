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

using System.Net;

namespace Furion.HttpRemote;

/// <summary>
///     复合 <see cref="HttpContent" />
/// </summary>
/// <remarks>内部包含多个 <see cref="HttpContent" />，该类型本身不产生网络数据，仅作为容器使用，框架在发送前会自动展开其内容。</remarks>
public sealed class CompositeHttpContent : HttpContent
{
    /// <summary>
    ///     <see cref="HttpContent" /> 集合
    /// </summary>
    internal readonly List<HttpContent> _contents = [];

    /// <summary>
    ///     <inheritdoc cref="CompositeHttpContent" />
    /// </summary>
    public CompositeHttpContent()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="CompositeHttpContent" />
    /// </summary>
    /// <param name="contents"><see cref="HttpContent" /> 集合</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CompositeHttpContent(params IEnumerable<HttpContent> contents)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(contents);

        AddRange(contents);
    }

    /// <summary>
    ///     <see cref="HttpContent" /> 集合
    /// </summary>
    public IReadOnlyList<HttpContent> Contents => _contents.AsReadOnly();

    /// <summary>
    ///     添加单个 <see cref="HttpContent" />
    /// </summary>
    /// <param name="content">
    ///     <see cref="HttpContent" />
    /// </param>
    /// <returns>
    ///     <see cref="CompositeHttpContent" />
    /// </returns>
    public CompositeHttpContent Add(HttpContent content)
    {
        // 空检查
        if ((HttpContent?)content is null)
        {
            return this;
        }

        // 检查是否是 CompositeHttpContent 实例
        if (content is CompositeHttpContent compositeHttpContent)
        {
            _contents.AddRange(compositeHttpContent._contents);
        }
        else
        {
            _contents.Add(content);
        }

        return this;
    }

    /// <summary>
    ///     批量添加 <see cref="HttpContent" />
    /// </summary>
    /// <param name="contents"><see cref="HttpContent" /> 集合</param>
    /// <returns>
    ///     <see cref="CompositeHttpContent" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CompositeHttpContent AddRange(params IEnumerable<HttpContent> contents)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(contents);

        // 遍历添加
        foreach (var content in contents)
        {
            Add(content);
        }

        return this;
    }

    /// <inheritdoc />
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        throw new NotSupportedException($"{nameof(CompositeHttpContent)} is not serializable.");

    /// <inheritdoc />
    protected override bool TryComputeLength(out long length) =>
        throw new NotSupportedException("Length computation is not supported.");
}