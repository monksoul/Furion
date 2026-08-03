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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Furion.HttpRemote;

/// <summary>
///     Access Token 自动管理管道处理器
/// </summary>
/// <param name="serviceProvider">
///     <see cref="IServiceProvider" />
/// </param>
/// <param name="logger">
///     <see cref="IHttpRemoteLogger" />
/// </param>
/// <param name="accessTokenManager">
///     <see cref="IHttpAccessTokenManager" />
/// </param>
internal sealed class TokenManagementPipelineHandler(
    IServiceProvider serviceProvider,
    IHttpRemoteLogger logger,
    IHttpAccessTokenManager accessTokenManager) : IHttpRequestPipelineHandler
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage?> HandleAsync(HttpRequestPipelineContext context,
        Func<Task<HttpResponseMessage?>> next)
    {
        // 获取当前 HttpRequestBuilder 实例
        var httpRequestBuilder = context.Builder;

        // 检查是否跳过框架的 Access Token 自动管理
        if (httpRequestBuilder.SuppressTokenManagement)
        {
            // 调用下一个处理器的委托
            return await next();
        }

        // 获取当前 HttpClient 实例的配置名称
        var httpClientName = httpRequestBuilder.HttpClientName;

        // 获取当前 HttpClient 实例的配置名称的配置选项
        var httpClientOptions = HttpRemoteUtility.ResolveHttpClientOptions(serviceProvider, httpClientName);

        // 检查是否配置了 Access Token 提供器
        if (httpClientOptions?.HttpAccessTokenProvider is not { } httpAccessTokenProvider)
        {
            // 调用下一个处理器的委托
            return await next();
        }

        // 初始化 HttpAccessTokenContext 实例
        var httpAccessTokenContext = new HttpAccessTokenContext(httpClientName, httpAccessTokenProvider);

        // 将 HttpRequestBuilder 中携带的 Access Token 自定义数据复制到上下文中
        if (httpRequestBuilder.AccessTokenData is { Count: > 0 })
        {
            foreach (var (key, value) in httpRequestBuilder.AccessTokenData)
            {
                httpAccessTokenContext.Items[key] = value;
            }
        }

        // 获取或刷新指定 HttpClient 实例的配置名称的 Access Token
        var httpAccessToken =
            await accessTokenManager.GetOrRefreshAsync(httpAccessTokenContext, context.CancellationToken);

        // 空检查
        if (httpAccessToken is not null)
        {
            // 申请将 Access Token 添加到 HttpRequestBuilder 中
            ApplyAccessToken(httpRequestBuilder, httpAccessTokenProvider, httpAccessToken);
        }

        // 调用下一个处理器的委托
        var httpResponseMessage = await next();

        // 检查是否需要强制刷新 Token 并重试（由提供器决定，默认 401）
        // ReSharper disable once InvertIf
        if (httpResponseMessage is not null && httpAccessToken is not null &&
            await httpAccessTokenProvider.ShouldRefreshAsync(httpAccessTokenContext, httpResponseMessage,
                context.CancellationToken))
        {
            // 输出重试日志
            logger.LogWarning(
                "Access token refresh triggered due to HTTP {StatusCode}. Refreshing token and retrying request for HttpClient '{HttpClientName}'.",
                (int)httpResponseMessage.StatusCode, httpClientName);

            // 释放前一个 HttpResponseMessage 实例
            httpResponseMessage.Dispose();

            // 强制刷新指定 HttpClient 实例的配置名称的 Access Token
            var newHttpAccessTokenToken =
                await accessTokenManager.ForceRefreshAsync(httpAccessTokenContext, context.CancellationToken);

            // 空检查
            if (newHttpAccessTokenToken is not null)
            {
                // 申请将 Access Token 添加到 HttpRequestBuilder 中
                ApplyAccessToken(httpRequestBuilder, httpAccessTokenProvider, newHttpAccessTokenToken);
            }

            // 调用下一个处理器的委托
            httpResponseMessage = await next();
        }

        return httpResponseMessage;
    }

    /// <summary>
    ///     申请将 Access Token 添加到 <see cref="HttpRequestBuilder" /> 中
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpAccessTokenProvider">
    ///     <see cref="IHttpAccessTokenProvider" />
    /// </param>
    /// <param name="httpAccessToken">
    ///     <see cref="HttpAccessToken" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    internal void ApplyAccessToken(HttpRequestBuilder httpRequestBuilder,
        IHttpAccessTokenProvider httpAccessTokenProvider, HttpAccessToken httpAccessToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);
        ArgumentNullException.ThrowIfNull(httpAccessTokenProvider);
        ArgumentNullException.ThrowIfNull(httpAccessToken);

        // 获取或解析 IHttpAccessTokenConfigurator 实例
        // ReSharper disable once SuspiciousTypeConversion.Global
        var httpAccessTokenConfigurator = httpAccessTokenProvider as IHttpAccessTokenConfigurator ??
                                          serviceProvider.GetService<IHttpAccessTokenConfigurator>();

        // 空检查
        if (httpAccessTokenConfigurator is not null)
        {
            httpAccessTokenConfigurator.Configure(httpRequestBuilder, httpAccessToken);
        }
        // 检查是否配置了 HTTP 认证方案
        else if (httpAccessToken.Scheme is { } scheme)
        {
            httpRequestBuilder.AddAuthentication(scheme, httpAccessToken.Value);
        }
        else
        {
            // 无自定义配置时，默认将 Access Token 值作为 Authorization 请求头发送
            httpRequestBuilder.WithHeader(HeaderNames.Authorization, httpAccessToken.Value, replace: true);
        }
    }
}