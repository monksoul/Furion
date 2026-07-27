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
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Furion.Shapeless;

/// <summary>
///     <see cref="Clay" /> 选项
/// </summary>
public sealed class ClayOptions
{
    /// <summary>
    ///     <inheritdoc cref="ClayOptions" />
    /// </summary>
    public ClayOptions() =>
        JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            PropertyNameCaseInsensitive = true,
            // 允许 String 转 Number
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            // 解决中文乱码问题
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true,
            Converters =
            {
                new ClayJsonConverter { Options = this },
                new ObjectToClayJsonConverter { Options = this },
                new FlexibleDateTimeConverter(),
                new FlexibleDateTimeOffsetConverter(),
                // 允许 Number 或 Boolean 转 String
                new StringJsonConverter()
            }
        };

    /// <summary>
    ///     默认 <see cref="ClayOptions" /> 实例
    /// </summary>
    public static ClayOptions Default => new();

    /// <summary>
    ///     允许属性名不区分大小写、访问缺失的属性或数组越界的 <see cref="ClayOptions" /> 实例
    /// </summary>
    public static ClayOptions Flexible => new()
    {
        PropertyNameCaseInsensitive = true, AllowMissingProperty = true, AllowIndexOutOfRange = true
    };

    /// <summary>
    ///     极度宽松的 <see cref="ClayOptions" /> 实例
    /// </summary>
    /// <remarks>在 <see cref="Flexible" /> 的基础上，进一步启用了自动创建嵌套对象、自动创建嵌套数组以及数组越界时自动用 <c>null</c> 补位的特性，适合对数据结构完全不固定的动态场景。</remarks>
    public static ClayOptions Lenient => new()
    {
        PropertyNameCaseInsensitive = true,
        AllowMissingProperty = true,
        AllowIndexOutOfRange = true,
        AutoCreateNestedObjects = true,
        AutoCreateNestedArrays = true,
        AutoExpandArrayWithNulls = true
    };

    /// <summary>
    ///     配置用于包裹非对象和非数组类型的键名
    /// </summary>
    /// <remarks>默认值为：<c>value</c>。</remarks>
    public string ScalarValueKey { get; set; } = "value";

    /// <summary>
    ///     是否允许访问不存在的属性
    /// </summary>
    /// <remarks>当 <see cref="Clay.IsObject" /> 为 <c>true</c> 时有效。默认值为：<c>false</c>，表示遇到缺失属性时将抛出异常。</remarks>
    public bool AllowMissingProperty { get; set; }

    /// <summary>
    ///     是否允许访问越界的数组索引
    /// </summary>
    /// <remarks>当 <see cref="Clay.IsArray" /> 为 <c>true</c> 时有效。默认值为：<c>false</c>，表示遇到无效索引时将抛出异常。</remarks>
    public bool AllowIndexOutOfRange { get; set; }

    /// <summary>
    ///     是否自动创建嵌套的对象实例
    /// </summary>
    /// <remarks>
    ///     当 <see cref="Clay.IsObject" /> 和 <see cref="AllowMissingProperty" /> 为 <c>true</c> 时有效。默认值为：<c>false</c>
    ///     ，当设置为 <c>true</c> 时，如果尝试访问或设置一个不存在的成员并且索引器带 <c>?</c> 后缀的键时，将自动创建一个新的 <see cref="Clay" /> 对象实例。
    /// </remarks>
    public bool AutoCreateNestedObjects { get; set; }

    /// <summary>
    ///     是否自动创建嵌套的数组实例
    /// </summary>
    /// <remarks>
    ///     当 <see cref="Clay.IsArray" /> 和 <see cref="AutoCreateNestedArrays" /> 为 <c>true</c> 时有效。默认值为：<c>false</c>
    ///     ，当设置为 <c>true</c> 时，如果尝试访问或设置超出数组长度的索引并且索引器带 <c>?</c> 后缀的键时，将自动创建一个新的 <see cref="Clay" /> 数组实例。
    /// </remarks>
    public bool AutoCreateNestedArrays { get; set; }

    /// <summary>
    ///     是否超出数组长度时自动补位 <c>null</c>
    /// </summary>
    /// <remarks>
    ///     当 <see cref="Clay.IsArray" /> 和 <see cref="AllowIndexOutOfRange" /> 为 <c>true</c> 时有效。默认值为：<c>false</c>，当设置为
    ///     <c>true</c> 时，如果尝试访问或设置超出数组长度的索引时，将自动进行补位 <c>null</c> 操作。
    /// </remarks>
    public bool AutoExpandArrayWithNulls { get; set; }

    /// <summary>
    ///     是否应在转换后执行数据校验
    /// </summary>
    /// <remarks>当 <see cref="Clay.IsObject" /> 为 <c>true</c> 时有效。默认值为：<c>false</c>，表示不进行转换后的数据校验。</remarks>
    public bool ValidateAfterConversion { get; set; }

    /// <summary>
    ///     控制是否将日期格式的 JSON 转换为 <see cref="DateTime" />
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool DateJsonToDateTime { get; set; }

    /// <summary>
    ///     控制是否将键值对格式的 JSON 转换为单一对象
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool KeyValueJsonToObject { get; set; }

    /// <summary>
    ///     是否属性名称不区分大小写
    /// </summary>
    /// <remarks>
    ///     <para>当 <see cref="Clay.IsObject" /> 为 <c>true</c> 时有效。默认值为：<c>false</c>，表示访问属性时使用的名称与属性名完全匹配。</para>
    ///     <para>如果在对象初始化之后更改此属性，则会触发深度克隆操作，创建一个新对象以反映更改。</para>
    /// </remarks>
    public bool PropertyNameCaseInsensitive { get; set; }

    /// <summary>
    ///     路径分隔符
    /// </summary>
    /// <remarks>默认值为：<c>:</c>。</remarks>
    public string[] PathSeparator { get; set; } = [":"];

    /// <summary>
    ///     是否是只读模式
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool ReadOnly { get; set; }

    /// <summary>
    ///     JSON 序列化配置
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; }

    /// <summary>
    ///     自定义配置 <see cref="ClayOptions" /> 实例
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="ClayOptions" />
    /// </returns>
    public ClayOptions Configure(Action<ClayOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        configure.Invoke(this);

        return this;
    }
}