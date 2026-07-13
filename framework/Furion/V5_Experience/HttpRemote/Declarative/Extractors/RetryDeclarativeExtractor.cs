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
///     HTTP 声明式 <see cref="RetryAttribute" /> 特性提取器
/// </summary>
internal sealed class RetryDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeExtractorContext context)
    {
        // 检查方法或接口是否贴有 [Retry] 特性
        if (!context.IsMethodDefined<RetryAttribute>(out var retryAttribute, true))
        {
            return;
        }

        // 初始化 HttpRetryOptions 实例
        var httpRetryOptions = new HttpRetryOptions().SetMaxRetries(retryAttribute.MaxRetries)
            .SetUseExponentialBackoff(retryAttribute.UseExponentialBackoff)
            .SetRetryIndefinitely(retryAttribute.RetryIndefinitely);

        // 检查是否配置了自定义重试间隔数组
        if (retryAttribute.RetryIntervals is { Length: > 0 })
        {
            httpRetryOptions.SetRetryIntervals(retryAttribute.RetryIntervals);
        }
        else
        {
            httpRetryOptions.SetRetryInterval(retryAttribute.RetryInterval);
        }

        // 检查是否配置了需要重试的 HTTP 状态码集合
        if (retryAttribute.RetryStatusCodes is { Length: > 0 })
        {
            httpRetryOptions.AddRetryStatusCodes(retryAttribute.RetryStatusCodes);
        }

        // 检查是否配置了需要重试的异常类型集合
        if (retryAttribute.RetryExceptionTypes is { Length: > 0 })
        {
            httpRetryOptions.AddRetryExceptions(retryAttribute.RetryExceptionTypes);
        }

        // 配置请求重试策略
        httpRequestBuilder.SetRetry(httpRetryOptions);
    }
}