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
using Furion.JsonSerialization;
using Furion.Shapeless;
using System.Text.Json.Serialization;

namespace System.Text.Json;

/// <summary>
/// System.Text.Json 扩展
/// </summary>
public static class SystemTextJsonExtensions
{
    /// <summary>
    /// 添加 DateTime/DateTime?/DateTimeOffset/DateTimeOffset? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <param name="localized">自动转换 DateTime/DateTimeOffset 为当地时间</param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateTimeTypeConverters(this IList<JsonConverter> converters, string outputFormat = "yyyy-MM-dd HH:mm:ss", bool localized = false)
    {
        converters.Add(new DateTimeJsonConverter(outputFormat, localized));
        converters.Add(new NullableDateTimeJsonConverter(outputFormat, localized));

        converters.Add(new DateTimeOffsetJsonConverter(outputFormat, localized));
        converters.Add(new NullableDateTimeOffsetJsonConverter(outputFormat, localized));

        return converters;
    }

    /// <summary>
    /// 添加 long/long? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="overMaxLengthOf17">是否超过最大长度 17 再处理</param>
    /// <returns></returns>
    public static IList<JsonConverter> AddLongTypeConverters(this IList<JsonConverter> converters, bool overMaxLengthOf17 = false)
    {
        converters.Add(new LongToStringJsonConverter(overMaxLengthOf17));
        converters.Add(new NullableLongToStringJsonConverter(overMaxLengthOf17));

        return converters;
    }

    /// <summary>
    /// 添加 Clay 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <remarks>可通过 <c>JsonSerializerOptions</c> 的 <c>PropertyNamingPolicy = JsonNamingPolicy.CamelCase</c> 配置输出小写。</remarks>
    /// <returns></returns>
    public static IList<JsonConverter> AddClayConverters(this IList<JsonConverter> converters)
    {
        if (!converters.OfType<ClayJsonConverter>().Any())
        {
            converters.Add(new ClayJsonConverter());
        }

        return converters;
    }

    /// <summary>
    /// 添加 DateOnly/DateOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateOnlyConverters(this IList<JsonConverter> converters, string outputFormat = "yyyy-MM-dd")
    {
        converters.Add(new DateOnlyJsonConverter(outputFormat));
        converters.Add(new NullableDateOnlyJsonConverter(outputFormat));

        return converters;
    }

    /// <summary>
    /// 添加 TimeOnly/TimeOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddTimeOnlyConverters(this IList<JsonConverter> converters, string outputFormat = "HH:mm:ss")
    {
        converters.Add(new TimeOnlyJsonConverter(outputFormat));
        converters.Add(new NullableTimeOnlyJsonConverter(outputFormat));

        return converters;
    }

    /// <summary>
    /// 添加 DataTable 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDataTableConverters(this IList<JsonConverter> converters)
    {
        converters.Add(new DataTableJsonConverter());

        return converters;
    }

    /// <summary>
    /// 添加 DataSet 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDataSetConverters(this IList<JsonConverter> converters)
    {
        converters.Add(new DataSetJsonConverter());

        return converters;
    }

    /// <summary>
    /// 添加枚举类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="writeAsString">控制序列化时是否将枚举输出为字符串（枚举名称）</param>
    /// <returns></returns>
    public static IList<JsonConverter> AddEnumConverters(this IList<JsonConverter> converters, bool writeAsString = false)
    {
        converters.Add(new EnumJsonConverter
        {
            WriteAsString = writeAsString
        });

        return converters;
    }

    /// <summary>
    /// 添加可空类型序列化处理
    /// </summary>
    /// <remarks>将 JSON 空字符串或空白字符串视为 null。</remarks>
    /// <param name="converters"></param>
    /// <param name="enableNullableValueType">是否同时启用可空值类型处理，默认值为：<c>false</c></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddNullableConverters(this IList<JsonConverter> converters, bool enableNullableValueType = false)
    {
        converters.Add(new EmptyStringToNullConverter());

        if (enableNullableValueType)
        {
            converters.Add(new NullableConverterFactory());
        }

        return converters;
    }
}