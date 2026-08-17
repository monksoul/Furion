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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Furion.JsonSerialization;

/// <summary>
/// 解决 long 精度问题
/// </summary>
public class SystemTextJsonLongToStringJsonConverter : JsonConverter<long>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SystemTextJsonLongToStringJsonConverter()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="overMaxLengthOf17"></param>
    public SystemTextJsonLongToStringJsonConverter(bool overMaxLengthOf17 = false)
    {
        OverMaxLengthOf17 = overMaxLengthOf17;
    }

    /// <summary>
    /// 是否超过最大长度 17 再处理
    /// </summary>
    public bool OverMaxLengthOf17 { get; set; }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return long.Parse(reader.GetString());

            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var longValue))
                {
                    return longValue;
                }

                var doubleValue = reader.GetDouble();
                if (doubleValue < long.MinValue || doubleValue > long.MaxValue)
                {
                    throw new JsonException($"Value {doubleValue} is out of range for Int64.");
                }
                return (long)doubleValue;

            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} when converting to Int64.");
        }
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        if (OverMaxLengthOf17)
        {
            if (value.ToString().Length <= 17) writer.WriteNumberValue(value);
            else writer.WriteStringValue(value.ToString());
        }
        else writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// 解决 long? 精度问题
/// </summary>
public class SystemTextJsonNullableLongToStringJsonConverter : JsonConverter<long?>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SystemTextJsonNullableLongToStringJsonConverter()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="overMaxLengthOf17"></param>
    public SystemTextJsonNullableLongToStringJsonConverter(bool overMaxLengthOf17 = false)
    {
        OverMaxLengthOf17 = overMaxLengthOf17;
    }

    /// <summary>
    /// 是否超过最大长度 17 再处理
    /// </summary>
    public bool OverMaxLengthOf17 { get; set; }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return long.Parse(reader.GetString());

            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var longValue))
                {
                    return longValue;
                }

                var doubleValue = reader.GetDouble();
                if (doubleValue < long.MinValue || doubleValue > long.MaxValue)
                {
                    throw new JsonException($"Value {doubleValue} is out of range for Int64.");
                }
                return (long)doubleValue;

            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} when converting to Int64?.");
        }
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            var newValue = value.Value;
            if (OverMaxLengthOf17)
            {
                if (newValue.ToString().Length <= 17) writer.WriteNumberValue(newValue);
                else writer.WriteStringValue(newValue.ToString());
            }
            else
            {
                writer.WriteStringValue(newValue.ToString());
            }
        }
    }
}