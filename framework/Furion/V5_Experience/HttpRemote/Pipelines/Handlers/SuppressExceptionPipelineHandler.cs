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

namespace Furion.HttpRemote;

/// <summary>
///     异常抑制管道处理器
/// </summary>
/// <remarks>确保该处理器位于管道最外层。</remarks>
internal sealed class SuppressExceptionPipelineHandler : IHttpRequestPipelineHandler
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage?> HandleAsync(HttpRequestPipelineContext context,
        Func<Task<HttpResponseMessage?>> next)
    {
        try
        {
            // 调用下一个处理器的委托
            return await next();
        }
        // 检查是否启用异常抑制机制
        catch (Exception e) when (ShouldSuppressException(context.Builder.SuppressExceptionTypes, e))
        {
            return context.ResponseMessage;
        }
    }

    /// <summary>
    ///     检查是否启用异常抑制机制
    /// </summary>
    /// <param name="suppressExceptionTypes">受抑制的异常类型列表</param>
    /// <param name="exception">
    ///     <see cref="Exception" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool ShouldSuppressException(HashSet<Type>? suppressExceptionTypes, Exception? exception)
    {
        // 空检查
        if (suppressExceptionTypes is null or { Count: 0 } || exception is null)
        {
            return false;
        }

        return suppressExceptionTypes.Any(u => u.IsInstanceOfType(exception));
    }
}