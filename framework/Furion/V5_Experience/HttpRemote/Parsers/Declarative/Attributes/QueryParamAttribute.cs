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
///     HTTP 声明式查询参数特性
/// </summary>
/// <remarks>支持多次指定。</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Parameter,
    AllowMultiple = true)]
public sealed class QueryParamAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="QueryParamAttribute" />
    /// </summary>
    /// <remarks>特性作用于参数时有效。</remarks>
    public QueryParamAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="QueryParamAttribute" />
    /// </summary>
    /// <remarks>
    ///     <para>当特性作用于方法或接口时，则表示移除指定查询参数操作。</para>
    ///     <para>当特性作用于参数时，则表示添加查询参数，同时设置查询参数键为 <c>name</c> 的值。</para>
    /// </remarks>
    /// <param name="name">查询参数键</param>
    public QueryParamAttribute(string name)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    /// <summary>
    ///     <inheritdoc cref="QueryParamAttribute" />
    /// </summary>
    /// <param name="name">查询参数键</param>
    /// <param name="value">查询参数的值</param>
    public QueryParamAttribute(string name, object? value)
        : this(name) =>
        Value = value;

    /// <summary>
    ///     查询参数键
    /// </summary>
    /// <remarks>该属性优先级低于 <see cref="AliasAs" /> 属性设置的值。</remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     查询参数的值
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
    ///     参数前缀
    /// </summary>
    /// <remarks>作用于对象类型时有效。</remarks>
    public string? Prefix { get; set; }

    /// <summary>
    ///     是否替换已存在的查询参数。默认值为：<c>false</c>
    /// </summary>
    public bool Replace { get; set; }

    /// <summary>
    ///     是否忽略空值
    /// </summary>
    /// <remarks>设置为 <c>true</c> 之后，当参数值为 <c>null</c> 时将被忽略。默认值为：<c>false</c>。</remarks>
    public bool IgnoreNullValues { get; set; }

    /// <summary>
    ///     要使用的格式
    /// </summary>
    /// <remarks>仅当 <see cref="Value" /> 实现 <see cref="IFormattable" /> 时有效。</remarks>
    public string? Format { get; set; }

    /// <summary>
    ///     是否设置了值
    /// </summary>
    internal bool HasSetValue { get; private set; }
}