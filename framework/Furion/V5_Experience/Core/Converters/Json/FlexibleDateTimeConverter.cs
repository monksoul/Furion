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
using System.Text.RegularExpressions;

namespace Furion.Converters.Json;

/// <summary>
///     <see cref="DateTime" /> JSON 序列化转换器
/// </summary>
public partial class FlexibleDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly DateTime s_epoch = new(1970, 1, 1, 0, 0, 0);
    private static readonly Regex s_regex = Regex();

    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var formatted = reader.GetString()!;
        var match = s_regex.Match(formatted);

        // 尝试获取 Unix epoch 日期格式
        // 参考文献：https://learn.microsoft.com/zh-cn/dotnet/standard/datetime/system-text-json-support#use-unix-epoch-date-format
        if (match.Success && long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out var unixTime))
        {
            return s_epoch.AddMilliseconds(unixTime);
        }

        // 尝试获取日期
        if (reader.TryGetDateTime(out var value))
        {
            return value;
        }

        // 尝试获取 ISO 8601-1:2019 日期格式
        return !DateTime.TryParse(formatted, out value) ? throw new JsonException() : value;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("O"));

    [GeneratedRegex(@"^/Date\(([+-]*\d+)\)/$", RegexOptions.CultureInvariant)]
    private static partial Regex Regex();
}