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

using Furion.Extensions;
using System.Net.Mime;
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 声明式 <see cref="BodyAttribute" /> 特性提取器
/// </summary>
internal sealed class BodyDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeExtractorContext context)
    {
        // 查找单个贴有 [Body] 特性的参数
        var bodyParameter =
            context.UnFrozenParameters.SingleOrDefault(u => u.Key.IsDefined(typeof(BodyAttribute), true));

        // 解析参数信息
        var (parameter, value) = bodyParameter;

        // 空检查
        if (parameter is null)
        {
            return;
        }

        // 获取 BodyAttribute 实例
        var bodyAttribute = parameter.GetCustomAttribute<BodyAttribute>(true);

        // 空检查
        ArgumentNullException.ThrowIfNull(bodyAttribute);

        // 检查是否为原始字符串内容
        if (value is string stringValue && bodyAttribute.RawString)
        {
            // 设置原始字符串内容
            httpRequestBuilder.SetRawStringContent(stringValue, bodyAttribute.ContentType);
        }
        else
        {
            // 设置请求内容
            httpRequestBuilder.SetContent(value, bodyAttribute.ContentType,
                disposeResourcesOnRequestCompletion: bodyAttribute.DisposeResourcesOnRequestCompletion);
        }

        // 检查是否是 application/x-www-form-urlencoded 请求内容
        if (httpRequestBuilder.ContentType.IsIn([MediaTypeNames.Application.FormUrlEncoded]))
        {
            // 检查是否启用 StringContent 方式构建 application/x-www-form-urlencoded 请求内容
            if (bodyAttribute.UseStringContent || !bodyAttribute.UrlEncode)
            {
                httpRequestBuilder.AddStringContentForFormUrlEncodedContentProcessor(bodyAttribute.UrlEncode);
            }
        }

        // 设置内容编码
        if (!string.IsNullOrWhiteSpace(bodyAttribute.ContentEncoding))
        {
            httpRequestBuilder.SetContentEncoding(bodyAttribute.ContentEncoding);
        }
    }
}