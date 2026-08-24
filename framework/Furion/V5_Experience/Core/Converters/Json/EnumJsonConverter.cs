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

namespace Furion.Converters.Json;

/// <summary>
///     枚举 JSON 序列化转换器
/// </summary>
/// <remarks>支持将 JSON 数字、字符串数字（如 "1"）或枚举名称（忽略大小写）反序列化为枚举。</remarks>
public class EnumJsonConverter : JsonConverter<object>
{
    /// <summary>
    ///     控制序列化时是否将枚举输出为字符串（枚举名称）
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    public bool WriteAsString { get; set; }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 处理 JSON 数字 token
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (reader.TokenType == JsonTokenType.Number)
        {
            object numValue;

            if (reader.TryGetInt64(out var longValue))
            {
                numValue = longValue;
            }
            else if (reader.TryGetUInt64(out var ulongValue))
            {
                numValue = ulongValue;
            }
            else
            {
                throw new JsonException($"The JSON number could not be converted to enum {typeToConvert.Name}.");
            }

            // 将指定的整数值转换为枚举成员
            return Enum.ToObject(typeToConvert, numValue);
        }

        // 处理 JSON 字符串 token
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            // 尝试按枚举名称解析（忽略大小写）
            if (Enum.TryParse(typeToConvert, stringValue, true, out var nameResult))
            {
                return nameResult;
            }

            // 获取枚举的底层类型
            var underlyingType = Enum.GetUnderlyingType(typeToConvert);

            try
            {
                // 尝试将字符串解析为底层类型的数字
                var numericValue = Convert.ChangeType(stringValue, underlyingType);

                // 将指定的整数值转换为枚举成员
                return Enum.ToObject(typeToConvert, numericValue!);
            }
            catch
            {
                // ignored
            }

            throw new JsonException(
                $"The JSON string \"{stringValue}\" could not be converted to enum {typeToConvert.Name}.");
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing enum {typeToConvert.Name}.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // 检查是否将枚举输出为字符串（枚举名称）
        if (WriteAsString)
        {
            writer.WriteStringValue(value.ToString());
        }
        else
        {
            writer.WriteNumberValue(Convert.ToDecimal(value));
        }
    }
}