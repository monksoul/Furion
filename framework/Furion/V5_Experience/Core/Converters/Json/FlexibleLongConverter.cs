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

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Furion.Converters.Json;

/// <summary>
///     <see cref="long" /> JSON 序列化转换器
/// </summary>
public sealed class FlexibleLongConverter : JsonConverter<long>
{
    /// <inheritdoc />
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            // 处理字符串类型
            case JsonTokenType.String:
                var stringValue = reader.GetString();

                // 空检查
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    throw new JsonException("Cannot convert empty string to Int64.");
                }

                // 移除前后空白
                stringValue = stringValue.Trim();

                // 尝试转换为 long 类型
                if (long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedLong))
                {
                    return parsedLong;
                }

                // 尝试解析为 decimal（处理科学计数法和浮点数），之后转换为 long 类型
                return decimal.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var parsedDecimal)
                    ? ConvertDecimalToLong(parsedDecimal)
                    : throw new JsonException($"Unable to convert string '{stringValue}' to Int64.");

            // 处理数值类型
            case JsonTokenType.Number:
                // 尝试转换为 long 类型
                if (reader.TryGetInt64(out var longValue))
                {
                    return longValue;
                }

                // 尝试解析为 decimal（处理科学计数法和浮点数），之后转换为 long 类型
                return reader.TryGetDecimal(out var decimalValue)
                    ? ConvertDecimalToLong(decimalValue)
                    : throw new JsonException("The JSON number is out of range or cannot be converted to Int64.");

            // 处理空值
            case JsonTokenType.Null:
                throw new JsonException("Cannot convert null to Int64.");

            // 处理其他类型
            case JsonTokenType.None:
            case JsonTokenType.StartObject:
            case JsonTokenType.EndObject:
            case JsonTokenType.StartArray:
            case JsonTokenType.EndArray:
            case JsonTokenType.PropertyName:
            case JsonTokenType.Comment:
            case JsonTokenType.True:
            case JsonTokenType.False:
            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} when converting to Int64.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);

    /// <summary>
    ///     将 <see cref="decimal" /> 转换为 <see cref="long" />
    /// </summary>
    /// <param name="decimalValue">
    ///     <see cref="decimal" />
    /// </param>
    /// <returns>
    ///     <see cref="long" />
    /// </returns>
    /// <exception cref="JsonException"></exception>
    internal static long ConvertDecimalToLong(decimal decimalValue)
    {
        // 检查是否在 long 范围内
        if (decimalValue is < long.MinValue or > long.MaxValue)
        {
            throw new JsonException($"Value {decimalValue} is out of range for Int64.");
        }

        // 检查是否有小数部分
        if (decimalValue != decimal.Truncate(decimalValue))
        {
            throw new JsonException(
                $"Value {decimalValue} has a fractional part and cannot be converted to Int64 without loss.");
        }

        return (long)decimalValue;
    }
}

/// <summary>
///     <see cref="long" />? JSON 序列化转换器
/// </summary>
public sealed class FlexibleNullableLongConverter : JsonConverter<long?>
{
    /// <inheritdoc cref="FlexibleLongConverter" />
    internal static readonly FlexibleLongConverter InnerConverter = new();

    /// <inheritdoc />
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 处理空值
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return InnerConverter.Read(ref reader, typeof(long), options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        // 空检查
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}