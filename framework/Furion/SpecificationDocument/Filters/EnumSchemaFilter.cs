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

using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Furion.SpecificationDocument;

/// <summary>
/// 修正 规范化文档 Enum 提示
/// </summary>
public partial class EnumSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// 中文正则表达式
    /// </summary>
    private const string CHINESE_PATTERN = @"[\u4e00-\u9fa5]";

    /// <summary>
    /// 枚举缓存
    /// </summary>
    private static readonly ConcurrentDictionary<Type, (List<EnumEntry> Entries, bool ConvertToNumber)> EnumCache = new();

    /// <summary>
    /// 实现过滤器方法
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="context"></param>
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema model) return;

        var type = context.Type;

        // 排除非枚举类型以及非本应用程序集的枚举
        if (!type.IsEnum || !App.Assemblies.Contains(type.Assembly)) return;

        // 尝试从缓存获取枚举元数据
        if (!EnumCache.TryGetValue(type, out var cache))
        {
            // 确定是否转为数字
            bool convertToNumber;
            var enumToNumberAttr = type.GetCustomAttribute<EnumToNumberAttribute>(false);

            if (enumToNumberAttr != null)
            {
                convertToNumber = enumToNumberAttr.Enabled;
            }
            else
            {
                convertToNumber = App.Configuration.GetValue("SpecificationDocumentSettings:EnumToNumber", false);
            }

            // 如果枚举名称包含中文，强制使用数字
            if (Enum.GetNames(type).Any(v => ChineseRegex().IsMatch(v)))
            {
                convertToNumber = true;
            }

            // 获取枚举底层数值类型
            var enumValueType = Enum.GetUnderlyingType(type);

            var entries = new List<EnumEntry>();

            foreach (var value in Enum.GetValues(type))
            {
                var name = Enum.GetName(type, value);
                var fieldInfo = type.GetField(name!);
                var description = fieldInfo?.GetCustomAttribute<DescriptionAttribute>(true)?.Description;
                var numValue = Convert.ChangeType(value, enumValueType);

                entries.Add(new EnumEntry(value, name!, description, numValue));
            }

            cache = (entries, convertToNumber);
            EnumCache.TryAdd(type, cache);
        }

        model.Enum.Clear();

        var stringBuilder = new StringBuilder();

        // 保留原有描述并换行
        stringBuilder.Append(model.Description ?? string.Empty).Append("<br />");

        foreach (var entry in cache.Entries)
        {
            model.Enum.Add(!cache.ConvertToNumber
                ? JsonNode.Parse($"\"{entry.Value}\"")
                : JsonNode.Parse($"{entry.NumValue}"));

            stringBuilder.Append("&nbsp;")
                         .Append(entry.Description)
                         .Append(' ')
                         .Append(entry.Name)
                         .Append(" = ")
                         .Append(entry.NumValue)
                         .Append("<br />");
        }
        model.Description = stringBuilder.ToString();

        // 如果不是数字模式，则设置类型为字符串
        if (!cache.ConvertToNumber)
        {
            model.Type = JsonSchemaType.String;
            model.Format = null;
        }

        // 修复启用 UseAllOfToExtendReferenceSchemas 导致的冗余 allOf 引用
        model.AllOf = null;
    }

    /// <summary>
    /// 缓存的枚举条目信息
    /// </summary>
    private record EnumEntry(object Value, string Name, string? Description, object NumValue);

    /// <summary>
    /// 中文正则表达式
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(CHINESE_PATTERN)]
    private static partial Regex ChineseRegex();
}