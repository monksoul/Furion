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

using System.Net.Http.Headers;
using System.Text.Json;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="IHttpContentProcessor" /> 内容处理器基类
/// </summary>
public abstract class HttpContentProcessorBase : IHttpContentProcessor, IServiceProvider
{
    /// <inheritdoc />
    public IServiceProvider? ServiceProvider { get; set; }

    /// <inheritdoc />
    public abstract bool CanProcess(HttpContentProcessorContext context);

    /// <inheritdoc />
    public abstract HttpContent? Process(HttpContentProcessorContext context);

    /// <inheritdoc />
    public object? GetService(Type serviceType) => ServiceProvider?.GetService(serviceType);

    /// <summary>
    ///     尝试解析 <see cref="HttpContent" /> 类型
    /// </summary>
    /// <param name="context">
    ///     <see cref="HttpContentProcessorContext" />
    /// </param>
    /// <param name="httpContent">
    ///     <see cref="HttpContent" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpContent" />
    /// </returns>
    public virtual bool TryProcess(HttpContentProcessorContext context, out HttpContent? httpContent)
    {
        switch (context.RawContent)
        {
            case null:
                httpContent = null;
                return true;
            case HttpContent content:
                // 设置 Content-Type
                content.Headers.ContentType ??=
                    new MediaTypeHeaderValue(context.ContentType) { CharSet = context.Encoding?.WebName };

                httpContent = content;
                return true;
            default:
                httpContent = null;
                return false;
        }
    }

    /// <summary>
    ///     解析 JSON 序列化配置
    /// </summary>
    /// <param name="httpClientName"><see cref="HttpClient" /> 实例的配置名称</param>
    /// <returns>
    ///     <see cref="JsonSerializerOptions" />
    /// </returns>
    public virtual JsonSerializerOptions ResolveJsonSerializerOptions(string? httpClientName) =>
        HttpRemoteUtility.ResolveJsonSerializerOptions(this, httpClientName, out _);
}