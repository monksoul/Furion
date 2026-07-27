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

using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Furion.Shapeless;

/// <summary>
///     流变对象
/// </summary>
public partial class Clay
{
    /// <inheritdoc />
    public bool Equals(Clay? other)
    {
        // 检查是否是相同的实例
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // 空检查及基础类型检查
        if (other is null || Type != other.Type)
        {
            return false;
        }

        return JsonNode.DeepEquals(JsonCanvas, other.JsonCanvas);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || Equals(obj as Clay);

    /// <summary>
    ///     重载 == 运算符
    /// </summary>
    /// <param name="left">
    ///     <see cref="Clay" />
    /// </param>
    /// <param name="right">
    ///     <see cref="Clay" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool operator ==(Clay? left, Clay? right) => Equals(left, right);

    /// <summary>
    ///     重载 != 运算符
    /// </summary>
    /// <param name="left">
    ///     <see cref="Clay" />
    /// </param>
    /// <param name="right">
    ///     <see cref="Clay" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool operator !=(Clay? left, Clay? right) => !(left == right);

    /// <summary>
    ///     重载 + 运算符
    /// </summary>
    /// <param name="left">
    ///     <see cref="Clay" />
    /// </param>
    /// <param name="right">
    ///     <see cref="Clay" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay operator +(Clay left, Clay right) => left.Combine(right);

    /// <summary>
    ///     支持 <see cref="Clay" /> 类型隐式转换为 <see cref="string" />
    /// </summary>
    /// <param name="clay">
    ///     <see cref="Clay" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static implicit operator string(in Clay clay) => clay.JsonCanvas.ToJsonString();

    /// <summary>
    ///     支持 <see cref="Clay" /> 类型隐式转换为 <see cref="Dictionary{TKey,TValue}" />
    /// </summary>
    /// <param name="clay">
    ///     <see cref="Clay" />
    /// </param>
    /// <returns>
    ///     <see cref="Dictionary{TKey,TValue}" />
    /// </returns>
    public static implicit operator Dictionary<string, object?>(in Clay clay) =>
        clay.As<Dictionary<string, object?>>()!;

    /// <summary>
    ///     支持 <see cref="string" /> 类型隐式转换为 <see cref="Clay" />
    /// </summary>
    /// <param name="str">
    ///     <see cref="string" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static implicit operator Clay(string? str) => Parse(str);

    /// <summary>
    ///     支持 <see cref="Stream" /> 类型隐式转换为 <see cref="Clay" />
    /// </summary>
    /// <param name="stream">
    ///     <see cref="Stream" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static implicit operator Clay(Stream? stream) => Parse(stream);

    /// <summary>
    ///     支持 <see cref="byte" />[] 类型隐式转换为 <see cref="Clay" />
    /// </summary>
    /// <param name="bytes">
    ///     <see cref="byte" />[]
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static implicit operator Clay(byte[]? bytes) => Parse(bytes);

    /// <summary>
    ///     支持 <see cref="JsonNode" /> 类型隐式转换为 <see cref="Clay" />
    /// </summary>
    /// <param name="jsonNode">
    ///     <see cref="JsonNode" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static implicit operator Clay(JsonNode? jsonNode) => Parse(jsonNode);

    /// <summary>
    ///     支持 <see cref="Dictionary{TKey,TValue}" /> 类型隐式转换为 <see cref="Clay" />
    /// </summary>
    /// <param name="dic">
    ///     <see cref="Dictionary{TKey,TValue}" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static implicit operator Clay(Dictionary<string, object?>? dic) => Parse(dic);

    /// <summary>
    ///     支持 <see cref="ExpandoObject" /> 类型隐式转换为 <see cref="Clay" />
    /// </summary>
    /// <param name="expandoObject">
    ///     <see cref="ExpandoObject" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static implicit operator Clay(ExpandoObject? expandoObject) => Parse(expandoObject);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // 初始化 HashCode 实例
        var hashCode = new HashCode();

        // 递归计算 JsonCanvas 哈希值
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        ComputeHash(JsonCanvas, ref hashCode);

        return hashCode.ToHashCode();

        // 递归计算 JsonNode 哈希值
        static void ComputeHash(JsonNode? jsonNode, ref HashCode hash)
        {
            // 空检查
            if (jsonNode is null)
            {
                hash.Add(0);
                return;
            }

            // 根据 JSON 值的种类分别计算哈希值
            switch (jsonNode.GetValueKind())
            {
                // 对象
                case JsonValueKind.Object:
                    // 预处理键值对（按键名排序）
                    var sortedProperties = jsonNode.AsObject().OrderBy(p => p.Key, StringComparer.Ordinal);

                    // 遍历所有键值对，递归计算哈希
                    foreach (var (key, value) in sortedProperties)
                    {
                        hash.Add(key);
                        ComputeHash(value, ref hash);
                    }

                    break;
                // 数组
                case JsonValueKind.Array:
                    // 遍历数组每一项，按顺序递归计算
                    foreach (var item in jsonNode.AsArray())
                    {
                        ComputeHash(item, ref hash);
                    }

                    break;
                // 字符串
                case JsonValueKind.String:
                    hash.Add(jsonNode.GetValue<string>());
                    break;
                // 数值
                case JsonValueKind.Number:
                    hash.Add(jsonNode.GetValue<decimal>());
                    break;
                // True
                case JsonValueKind.True:
                    hash.Add(true);
                    break;
                // False
                case JsonValueKind.False:
                    hash.Add(false);
                    break;
                // 其他类型
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    hash.Add(0);
                    break;
            }
        }
    }
}