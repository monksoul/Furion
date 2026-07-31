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

using Furion.HttpRemote.Extensions;
using Furion.Utilities;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求分析工具处理委托
/// </summary>
/// <remarks>参考文献：https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/http-requests?view=aspnetcore-8.0#outgoing-request-middleware</remarks>
/// <param name="logger">
///     <see cref="IHttpRemoteLogger" />
/// </param>
/// <param name="httpRemoteOptions">
///     <see cref="IOptions{TOptions}" />
/// </param>
public sealed class ProfilerDelegatingHandler(
    IHttpRemoteLogger logger,
    IOptionsMonitor<HttpRemoteOptions> httpRemoteOptions)
    : DelegatingHandler
{
    /// <summary>
    ///     是否启用请求分析工具
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsEnabled(HttpRequestMessage httpRequestMessage)
    {
        // 检查是否已打印过
        if (httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<string>(Constants.PROFILER_PRINTED_KEY),
                out _))
        {
            return false;
        }

        return !(httpRequestMessage.Options.TryGetValue(
            new HttpRequestOptionsKey<string>(Constants.DISABLE_PROFILER_KEY), out var value) && value == "TRUE");
    }

    /// <inheritdoc />
    protected override HttpResponseMessage Send(HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken)
    {
        // 检查是否启用请求分析工具
        if (!IsEnabled(httpRequestMessage))
        {
            return base.Send(httpRequestMessage, cancellationToken);
        }

        // 记录请求信息
        AsyncUtility.RunSync(() =>
            LogRequestAsync(logger, httpRemoteOptions.CurrentValue, httpRequestMessage, null, null, cancellationToken));

        // 初始化 Stopwatch 实例并开启计时操作
        var stopwatch = Stopwatch.StartNew();

        // 发送 HTTP 远程请求
        var httpResponseMessage = base.Send(httpRequestMessage, cancellationToken);

        // 获取请求耗时
        var requestDuration = stopwatch.ElapsedMilliseconds;

        // 停止计时
        stopwatch.Stop();

        // 记录响应信息
        AsyncUtility.RunSync(() => LogResponseAsync(logger, httpRemoteOptions.CurrentValue, httpResponseMessage,
            requestDuration, null, cancellationToken));

        // 打印 CookieContainer 内容
        LogCookieContainer(logger, httpRemoteOptions.CurrentValue, httpRequestMessage, ExtractCookieContainer());

        return httpResponseMessage;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken)
    {
        // 检查是否启用请求分析工具
        if (!IsEnabled(httpRequestMessage))
        {
            return await base.SendAsync(httpRequestMessage, cancellationToken);
        }

        // 记录请求信息
        await LogRequestAsync(logger, httpRemoteOptions.CurrentValue, httpRequestMessage, null, null,
            cancellationToken);

        // 初始化 Stopwatch 实例并开启计时操作
        var stopwatch = Stopwatch.StartNew();

        // 发送 HTTP 远程请求
        var httpResponseMessage = await base.SendAsync(httpRequestMessage, cancellationToken);

        // 获取请求耗时
        var requestDuration = stopwatch.ElapsedMilliseconds;

        // 停止计时
        stopwatch.Stop();

        // 记录响应信息
        await LogResponseAsync(logger, httpRemoteOptions.CurrentValue, httpResponseMessage, requestDuration,
            null, cancellationToken);

        // 打印 CookieContainer 内容
        LogCookieContainer(logger, httpRemoteOptions.CurrentValue, httpRequestMessage, ExtractCookieContainer());

        return httpResponseMessage;
    }

    /// <summary>
    ///     记录请求信息
    /// </summary>
    /// <param name="logger">
    ///     <see cref="IHttpRemoteLogger" />
    /// </param>
    /// <param name="remoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="httpRemoteAnalyzer">
    ///     <see cref="HttpRemoteAnalyzer" />
    /// </param>
    /// <param name="httpClient">
    ///     <see cref="HttpClient" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task LogRequestAsync(IHttpRemoteLogger logger, HttpRemoteOptions remoteOptions,
        HttpRequestMessage httpRequestMessage, HttpRemoteAnalyzer? httpRemoteAnalyzer = null,
        HttpClient? httpClient = null, CancellationToken cancellationToken = default)
    {
        // 解析请求标头和内容信息
        var requestSummary = httpRequestMessage.ProfilerHeaders(httpClient);
        var requestContentSummary =
            await httpRequestMessage.Content.ProfilerAsync(httpRequestMessage: httpRequestMessage,
                cancellationToken: cancellationToken);

        Log(logger, remoteOptions, Helpers.JoinNonEmptyLines(requestSummary, requestContentSummary),
            httpRemoteAnalyzer);
    }

    /// <summary>
    ///     记录响应信息
    /// </summary>
    /// <param name="logger">
    ///     <see cref="IHttpRemoteLogger" />
    /// </param>
    /// <param name="remoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="requestDuration">请求耗时（毫秒）</param>
    /// <param name="httpRemoteAnalyzer">
    ///     <see cref="HttpRemoteAnalyzer" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task LogResponseAsync(IHttpRemoteLogger logger, HttpRemoteOptions remoteOptions,
        HttpResponseMessage httpResponseMessage, long requestDuration, HttpRemoteAnalyzer? httpRemoteAnalyzer = null,
        CancellationToken cancellationToken = default)
    {
        // 解析常规和响应内容信息
        var generalSummary = httpResponseMessage.ProfilerGeneralAndHeaders(generalCustomKeyValues:
            [new KeyValuePair<string, IEnumerable<string>>("Request Duration (ms)", [$"{requestDuration:N2}"])]);
        var responseContentSummary =
            await httpResponseMessage.Content.ProfilerAsync("Response Body", httpResponseMessage,
                cancellationToken: cancellationToken);

        Log(logger, remoteOptions, Helpers.JoinNonEmptyLines(generalSummary, responseContentSummary),
            httpRemoteAnalyzer);
    }

    /// <summary>
    ///     打印 <see cref="CookieContainer" /> 内容
    /// </summary>
    /// <param name="logger">
    ///     <see cref="IHttpRemoteLogger" />
    /// </param>
    /// <param name="remoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="request">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="cookieContainer">
    ///     <see cref="CookieContainer" />
    /// </param>
    /// <param name="httpRemoteAnalyzer">
    ///     <see cref="HttpRemoteAnalyzer" />
    /// </param>
    internal static void LogCookieContainer(IHttpRemoteLogger logger, HttpRemoteOptions remoteOptions,
        HttpRequestMessage request, CookieContainer? cookieContainer, HttpRemoteAnalyzer? httpRemoteAnalyzer = null)
    {
        // 空检查
        if (request.RequestUri is null || cookieContainer is null)
        {
            return;
        }

        // 获取 Cookie 集合
        var cookies = cookieContainer.GetCookies(request.RequestUri);

        // 空检查
        if (cookies is { Count: 0 })
        {
            return;
        }

        // 打印日志
        Log(logger, remoteOptions, StringUtility.FormatKeyValuesSummary(
            cookies.ToDictionary(u => u.Name, u => Enumerable.Empty<string>().Concat([u.Value])),
            "Cookie Container"), httpRemoteAnalyzer);
    }

    /// <summary>
    ///     打印日志
    /// </summary>
    /// <param name="logger">
    ///     <see cref="IHttpRemoteLogger" />
    /// </param>
    /// <param name="remoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="message">日志消息</param>
    /// <param name="httpRemoteAnalyzer">
    ///     <see cref="HttpRemoteAnalyzer" />
    /// </param>
    internal static void Log(IHttpRemoteLogger logger, HttpRemoteOptions remoteOptions, string? message,
        HttpRemoteAnalyzer? httpRemoteAnalyzer = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(logger);

        // 空检查
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        // 追加分析数据
        httpRemoteAnalyzer?.AppendData(message);

        // 记录日志
        logger.Log(remoteOptions.ProfilerLogLevel, null, message);
    }

    /// <summary>
    ///     提取 <see cref="CookieContainer" /> 实例
    /// </summary>
    /// <returns>
    ///     <see cref="CookieContainer" />
    /// </returns>
    internal CookieContainer? ExtractCookieContainer()
    {
        // 检查是否是 WebAssembly 应用，如果是则跳过
        if (OperatingSystem.IsBrowser())
        {
            return null;
        }

        return InnerHandler switch
        {
            LoggingHttpMessageHandler loggingHttpMessageHandler => loggingHttpMessageHandler.InnerHandler switch
            {
                SocketsHttpHandler socketsHttpHandler => socketsHttpHandler.CookieContainer,
                HttpClientHandler httpClientHandler => httpClientHandler.CookieContainer,
                _ => null
            },
            LoggingScopeHttpMessageHandler loggingScopeHttpMessageHandler => loggingScopeHttpMessageHandler.InnerHandler
                switch
                {
                    SocketsHttpHandler socketsHttpHandler => socketsHttpHandler.CookieContainer,
                    HttpClientHandler httpClientHandler => httpClientHandler.CookieContainer,
                    _ => null
                },
            SocketsHttpHandler socketsHttpHandler => socketsHttpHandler.CookieContainer,
            HttpClientHandler httpClientHandler => httpClientHandler.CookieContainer,
            _ => null
        };
    }
}