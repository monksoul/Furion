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

using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Furion.SpecificationDocument;

/// <summary>
/// 标签文档排序/注释拦截器
/// </summary>
public class TagsOrderDocumentFilter : IDocumentFilter
{
    /// <summary>
    /// 配置拦截
    /// </summary>
    /// <param name="swaggerDoc"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // 获取原先的标签描述
        var tagDescriptionMap = swaggerDoc.Tags?
            .ToDictionary(t => t.Name, t => t.Description)
            ?? [];

        var orderedTags = Penetrates.ControllerOrderCollection
            .Where(u => SpecificationDocumentBuilder.GetControllerGroups(u.Value.Item3).Any(c => c.Group == context.DocumentName))
            .OrderByDescending(u => u.Value.Item2)
            .ThenBy(u => u.Key)
            .Select(u =>
            {
                var description = tagDescriptionMap.TryGetValue(u.Value.Item1, out var desc) ? desc : null;

                // 如果没有标签描述，尝试从控制器的 [ApiDescriptionSettings] 特性中获取
                if (string.IsNullOrEmpty(description))
                {
                    description = u.Value.Item3.GetCustomAttribute<ApiDescriptionSettingsAttribute>(inherit: true)?.Description;
                }

                return new OpenApiTag
                {
                    Name = u.Value.Item1,
                    Description = description
                };
            })
            .ToHashSet();

        swaggerDoc.Tags = orderedTags;
    }
}