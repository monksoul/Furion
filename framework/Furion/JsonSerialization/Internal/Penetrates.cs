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

namespace Furion.JsonSerialization;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 将 JSON 中的值转换为 DateTime
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="format"></param>
    /// <param name="localized"></param>
    /// <returns></returns>
    internal static DateTime ConvertToDateTime(ref Utf8JsonReader reader, string format, bool localized)
    {
        // 处理 JSON 数字时间戳
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var longValue))
        {
            return ConvertFromUnixTimestampToDateTime(longValue, localized);
        }

        var stringValue = reader.GetString();

        // 处理纯数字字符串时间戳
        if (long.TryParse(stringValue, out var longValue2))
        {
            return ConvertFromUnixTimestampToDateTime(longValue2, localized);
        }

        // 处理日期字符串
        return ParseDateTimeString(stringValue, format);
    }

    /// <summary>
    /// 将 JSON 中的值转换为 DateTimeOffset
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="format"></param>
    /// <param name="localized"></param>
    /// <returns></returns>
    internal static DateTimeOffset ConvertToDateTimeOffset(ref Utf8JsonReader reader, string format, bool localized)
    {
        // 处理 JSON 数字时间戳
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var longValue))
        {
            return ConvertFromUnixTimestampToDateTimeOffset(longValue, localized);
        }

        var stringValue = reader.GetString();

        // 处理纯数字字符串时间戳
        if (long.TryParse(stringValue, out var longValue2))
        {
            return ConvertFromUnixTimestampToDateTimeOffset(longValue2, localized);
        }

        // 处理日期字符串
        return ParseDateTimeOffsetString(stringValue, format, localized);
    }

    /// <summary>
    /// 将 Unix 时间戳（秒或毫秒）转换为 DateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="localized"></param>
    /// <returns></returns>
    private static DateTime ConvertFromUnixTimestampToDateTime(long timestamp, bool localized)
    {
        DateTimeOffset dto;

        // 判断是秒还是毫秒
        if (Math.Abs(timestamp) > 100000000000)
        {
            dto = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        }
        else
        {
            dto = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        }

        return localized ? dto.LocalDateTime : dto.UtcDateTime;
    }

    /// <summary>
    /// 将 Unix 时间戳（秒或毫秒）转换为 DateTimeOffset
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="localized"></param>
    /// <returns></returns>
    private static DateTimeOffset ConvertFromUnixTimestampToDateTimeOffset(long timestamp, bool localized)
    {
        DateTimeOffset dto;

        // 判断是秒还是毫秒
        if (Math.Abs(timestamp) > 100000000000)
        {
            dto = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        }
        else
        {
            dto = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        }

        return localized ? dto.ToLocalTime() : dto.ToUniversalTime();
    }

    /// <summary>
    /// 解析日期时间字符串为 DateTime
    /// </summary>
    /// <param name="stringValue"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    private static DateTime ParseDateTimeString(string stringValue, string format)
    {
        if (string.IsNullOrEmpty(stringValue))
        {
            throw new JsonException("Cannot parse an empty string to DateTime.");
        }

        // 尝试按指定格式解析
        if (!string.IsNullOrEmpty(format) &&
            DateTime.TryParseExact(stringValue, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDateTime))
        {
            return exactDateTime;
        }

        // 回退到通用解析
        if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return dateTime;
        }

        throw new JsonException($"Cannot parse string '{stringValue}' to DateTime.");
    }

    /// <summary>
    /// 解析日期时间字符串为 DateTimeOffset
    /// </summary>
    /// <param name="stringValue"></param>
    /// <param name="format"></param>
    /// <param name="localized"></param>
    /// <returns></returns>
    private static DateTimeOffset ParseDateTimeOffsetString(string stringValue, string format, bool localized)
    {
        if (string.IsNullOrEmpty(stringValue))
        {
            throw new JsonException("Cannot parse an empty string to DateTimeOffset.");
        }

        var dateTimeStyles = localized ? DateTimeStyles.AssumeLocal : DateTimeStyles.AssumeUniversal;

        // 尝试按指定格式解析
        if (!string.IsNullOrEmpty(format) &&
            DateTimeOffset.TryParseExact(stringValue, format, CultureInfo.InvariantCulture, dateTimeStyles, out var dtoExact))
        {
            return localized ? dtoExact.ToLocalTime() : dtoExact.ToUniversalTime();
        }

        // 回退到通用解析
        if (DateTimeOffset.TryParse(stringValue, CultureInfo.InvariantCulture, dateTimeStyles, out var dto))
        {
            return localized ? dto.ToLocalTime() : dto.ToUniversalTime();
        }

        throw new JsonException($"Cannot parse string '{stringValue}' to DateTimeOffset.");
    }
}