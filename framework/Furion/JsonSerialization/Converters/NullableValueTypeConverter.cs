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
/// 空字符串转换为 null 转换器
/// </summary>
public class EmptyStringToNullConverter : JsonConverter<string?>
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 处理 JSON null
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // 处理字符串令牌
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return reader.GetString();
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}

/// <summary>
/// 可空值类型转换器
/// </summary>
/// <remarks>将 JSON 空字符串或空白字符串视为 null。非 JSON 可尝试使用 <c>[DisplayFormat(ConvertEmptyStringToNull = true)]</c>。</remarks>
/// <typeparam name="T"></typeparam>
public class NullableValueTypeConverter<T> : JsonConverter<T?> where T : struct
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 处理 JSON null
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // 处理字符串令牌
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }
        }

        return JsonSerializer.Deserialize<T>(ref reader, options);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

/// <summary>
/// 可空值类型转换器工厂
/// </summary>
public class NullableConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// 判断是否可以转换指定类型
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// 创建转换器实例
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // 获取可空类型实际类型
        var valueType = typeToConvert.GetGenericArguments()[0];

        // 创建 NullableValueTypeConverter<T> 的泛型类型
        var converterType = typeof(NullableValueTypeConverter<>).MakeGenericType(valueType);

        return (JsonConverter)Activator.CreateInstance(converterType);
    }
}