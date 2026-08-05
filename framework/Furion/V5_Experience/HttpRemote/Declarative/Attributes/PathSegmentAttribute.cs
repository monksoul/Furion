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
///     HTTP 声明式路径片段特性
/// </summary>
/// <remarks>支持多次指定。</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Parameter,
    AllowMultiple = true)]
public sealed class PathSegmentAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="PathSegmentAttribute" />
    /// </summary>
    /// <remarks>特性作用于参数时有效。</remarks>
    public PathSegmentAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="PathSegmentAttribute" />
    /// </summary>
    /// <remarks>
    ///     <para>当特性作用于方法或接口时，则表示添加指定路径片段操作。</para>
    ///     <para>当特性作用于参数且参数值为 <c>null</c> 时，表示将路径片段设置为 <c>segment</c> 的值。</para>
    /// </remarks>
    /// <param name="segment">路径片段</param>
    public PathSegmentAttribute(string segment)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);

        Segment = segment;
    }

    /// <summary>
    ///     路径片段
    /// </summary>
    public string? Segment { get; set; }

    /// <summary>
    ///     是否标记为待删除
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。设置为 <c>true</c> 表示移除路径片段。</remarks>
    public bool Remove { get; set; }
}