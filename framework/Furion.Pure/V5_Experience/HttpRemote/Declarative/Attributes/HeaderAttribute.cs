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
///     HTTP 声明式请求标头特性
/// </summary>
/// <remarks>支持多次指定。</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Parameter,
    AllowMultiple = true)]
public sealed class HeaderAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="HeaderAttribute" />
    /// </summary>
    /// <remarks>特性作用于参数时有效。</remarks>
    public HeaderAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="HeaderAttribute" />
    /// </summary>
    /// <remarks>
    ///     <para>当特性作用于方法或接口时，若配置字符串不含冒号（:），则表示移除指定请求标头；若含冒号，则以第一个冒号为分隔，左侧为键，右侧为值。</para>
    ///     <para>当特性作用于参数时，则表示添加请求标头，同时设置请求标头键为 <c>name</c> 的值。</para>
    /// </remarks>
    /// <param name="name">请求标头键</param>
    public HeaderAttribute(string name)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    /// <summary>
    ///     <inheritdoc cref="HeaderAttribute" />
    /// </summary>
    /// <param name="name">请求标头键</param>
    /// <param name="value">请求标头的值</param>
    public HeaderAttribute(string name, object? value)
        : this(name) =>
        Value = value;

    /// <summary>
    ///     请求标头键
    /// </summary>
    /// <remarks>该属性优先级低于 <see cref="AliasAs" /> 属性设置的值。</remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     请求标头的值
    /// </summary>
    /// <remarks>当特性作用于参数时，表示默认值。</remarks>
    public object? Value
    {
        get;
        set
        {
            field = value;
            HasSetValue = true;
        }
    }

    /// <summary>
    ///     别名
    /// </summary>
    /// <remarks>
    ///     <para>特性用于参数时有效。</para>
    ///     <para>该属性优先级高于 <see cref="Name" /> 属性设置的值。</para>
    /// </remarks>
    public string? AliasAs { get; set; }

    /// <summary>
    ///     是否转义
    /// </summary>
    public bool Escape { get; set; }

    /// <summary>
    ///     是否替换已存在的请求标头。默认值为 <c>false</c>
    /// </summary>
    public bool Replace { get; set; }

    /// <summary>
    ///     是否设置了值
    /// </summary>
    internal bool HasSetValue { get; private set; }
}