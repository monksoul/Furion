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
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Furion.Converters.Json;

/// <summary>
///     <see cref="ExpandoObject" /> 类型 JSON 序列化转换器
/// </summary>
public class ExpandoObjectJsonConverter : JsonConverter<ExpandoObject>
{
    /// <inheritdoc />
    public override ExpandoObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        reader.TokenType switch
        {
            // 根据当前的 JSON 令牌类型，决定是读取对象还是数组
            JsonTokenType.StartObject => ReadObject(ref reader, options),
            JsonTokenType.StartArray => ReadArrayAsExpandoObject(ref reader, options),
            _ => throw new JsonException("Unexpected token type.")
        };

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ExpandoObject value, JsonSerializerOptions options)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // 检查 ExpandoObject 是否包含 "Items" 属性
        if (((IDictionary<string, object?>)value).TryGetValue("Items", out var items) &&
            items is List<object?> itemList)
        {
            // 写出数组
            writer.WriteStartArray();
            foreach (var item in itemList)
            {
                WriteValue(writer, item, options);
            }

            writer.WriteEndArray();
        }
        else
        {
            // 写出对象
            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);
                WriteValue(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }
    }

    /// <summary>
    ///     读取 JSON 对象并将其转换为 <see cref="ExpandoObject" />
    /// </summary>
    /// <param name="reader">
    ///     <see cref="Utf8JsonReader" />
    /// </param>
    /// <param name="options">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="ExpandoObject" />
    /// </returns>
    /// <exception cref="JsonException"></exception>
    internal ExpandoObject ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var expandoObject = new ExpandoObject();
        IDictionary<string, object?> dictionary = expandoObject;

        // 遍历 JSON 对象的每个属性
        while (reader.Read())
        {
            // 结束对象读取
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name.");
            }

            // 获取属性名
            var propertyName = reader.GetString()!;

            if (!reader.Read())
            {
                throw new JsonException("Failed to read property value.");
            }

            // 递归读取属性值
            var propertyValue = ReadValue(ref reader, options);

            // 将属性名和值添加到 ExpandoObject 中
            dictionary[propertyName] = propertyValue;
        }

        return expandoObject;
    }

    /// <summary>
    ///     读取 JSON 值并根据其类型返回相应的对象
    /// </summary>
    /// <param name="reader">
    ///     <see cref="Utf8JsonReader" />
    /// </param>
    /// <param name="options">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    /// <exception cref="JsonException"></exception>
    internal object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (reader.TokenType)
        {
            // 读取字符串值
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                // 读取整数值
                if (reader.TryGetInt64(out var intValue))
                {
                    return intValue;
                }

                // 读取浮点数值
                return reader.GetDouble();
            // 读取布尔值 true
            case JsonTokenType.True:
                return true;
            // 读取布尔值 false
            case JsonTokenType.False:
                return false;
            // 读取空值
            case JsonTokenType.Null:
                return null;
            // 递归读取嵌套对象
            case JsonTokenType.StartObject:
                return ReadObject(ref reader, options);
            // 读取数组
            case JsonTokenType.StartArray:
                return ReadArrayAsList(ref reader, options);
            default:
                throw new JsonException("Unexpected token type.");
        }
    }

    /// <summary>
    ///     读取 JSON 数组并将其转换为 <see cref="List{T}" />
    /// </summary>
    /// <param name="reader">
    ///     <see cref="Utf8JsonReader" />
    /// </param>
    /// <param name="options">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="List{T}" />
    /// </returns>
    internal List<object?> ReadArrayAsList(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var list = new List<object?>();

        // 遍历数组中的每个元素
        while (reader.Read())
        {
            // 结束数组读取
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            // 递归读取数组元素
            var item = ReadValue(ref reader, options);

            // 将元素添加到列表中
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    ///     读取 JSON 数组并将其转换为包含 "Items" 属性的 <see cref="ExpandoObject" />
    /// </summary>
    /// <param name="reader">
    ///     <see cref="Utf8JsonReader" />
    /// </param>
    /// <param name="options">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="ExpandoObject" />
    /// </returns>
    internal ExpandoObject ReadArrayAsExpandoObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var expandoObject = new ExpandoObject();
        IDictionary<string, object?> dictionary = expandoObject;

        // 读取数组
        var list = ReadArrayAsList(ref reader, options);

        // 将数组添加到 ExpandoObject 中
        dictionary["Items"] = list;

        return expandoObject;
    }

    /// <summary>
    ///     写出 JSON 值
    /// </summary>
    /// <param name="writer">
    ///     <see cref="Utf8JsonWriter" />
    /// </param>
    /// <param name="value">
    ///     要写出的值
    /// </param>
    /// <param name="options">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    private void WriteValue(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string stringValue:
                writer.WriteStringValue(stringValue);
                break;
            case int intValue:
                writer.WriteNumberValue(intValue);
                break;
            case long longValue:
                writer.WriteNumberValue(longValue);
                break;
            case float floatValue:
                writer.WriteNumberValue(floatValue);
                break;
            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                break;
            case decimal decimalValue:
                writer.WriteNumberValue(decimalValue);
                break;
            case bool boolValue:
                writer.WriteBooleanValue(boolValue);
                break;
            case DateTime dateTimeValue:
                // ISO 8601 格式
                writer.WriteStringValue(dateTimeValue.ToString("O", CultureInfo.InvariantCulture));
                break;
            case ExpandoObject expandoValue:
                Write(writer, expandoValue, options);
                break;
            case IEnumerable enumerableValue:
                writer.WriteStartArray();
                foreach (var item in enumerableValue)
                {
                    WriteValue(writer, item, options);
                }

                writer.WriteEndArray();
                break;
            default:
                throw new JsonException($"Unsupported value type: {value.GetType().FullName}.");
        }
    }
}