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
///     HTTP 声明式多部分表单项内容特性
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class MultipartAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="MultipartAttribute" />
    /// </summary>
    public MultipartAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="MultipartAttribute" />
    /// </summary>
    /// <param name="name">表单名称</param>
    public MultipartAttribute(string name) => Name = name;

    /// <summary>
    ///     表单名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     文件的名称
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    ///     内容类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///     内容编码
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    ///     表示将字符串作为多部分表单文件的来源
    /// </summary>
    /// <remarks>用于设置多部分表单文件内容。当参数类型为 <see cref="string" /> 时有效。</remarks>
    public FileSourceType AsFileFrom { get; set; }

    /// <summary>
    ///     表示是否作为表单的一项
    /// </summary>
    /// <remarks>
    ///     <para>当参数类型为对象类型时有效。</para>
    ///     <para>该属性值为 <c>true</c> 时作为表单的一项。否则将遍历对象类型的每一个公开属性作为表单的项。默认值为：<c>true</c>。</para>
    /// </remarks>
    public bool AsFormItem { get; set; } = true;
}