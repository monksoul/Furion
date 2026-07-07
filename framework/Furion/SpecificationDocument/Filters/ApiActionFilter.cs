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

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace Furion.SpecificationDocument;

/// <summary>
/// 规范化文档自定义更多功能
/// </summary>
public class ApiActionFilter : IOperationFilter
{
    /// <summary>
    /// 实现过滤器方法
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 获取方法
        var method = context.MethodInfo;

        // 处理 ApiDescriptionSettings 特性描述
        var apiDescriptionSettings = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
        if (apiDescriptionSettings != null && !string.IsNullOrWhiteSpace(apiDescriptionSettings.Description))
        {
            operation.Description = (operation.Description ?? string.Empty) + apiDescriptionSettings.Description;
        }

        // 处理定义 [DisplayName] 特性但并未注释的情况
        if (string.IsNullOrWhiteSpace(operation.Summary))
        {
            var displayName = method.GetCustomAttribute<DisplayNameAttribute>(true);
            if (displayName != null && !string.IsNullOrWhiteSpace(displayName.DisplayName))
            {
                operation.Summary = displayName.DisplayName;
            }
        }

        // 处理过时标记
        var deprecated = method.GetCustomAttribute<ObsoleteAttribute>(true);
        if (deprecated != null && !string.IsNullOrWhiteSpace(deprecated.Message))
        {
            operation.Description = $"<div>{deprecated.Message}</div>" + (operation.Description ?? string.Empty);
        }
    }
}