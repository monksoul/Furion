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

using System.Net;
using System.Text;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求断言上下文
/// </summary>
public sealed class HttpAssertionContext
{
    /// <summary>
    ///     请求内容字符串缓存
    /// </summary>
    internal string? _cachedRequestContent;

    /// <summary>
    ///     响应内容字符串缓存
    /// </summary>
    internal string? _cachedResponseContent;

    /// <summary>
    ///     <inheritdoc cref="HttpAssertionContext" />
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />，可为 <c>null</c>（用于请求断言阶段）
    /// </param>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />，可选，用于断言请求内容
    /// </param>
    /// <param name="requestDuration">请求耗时（毫秒）</param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpAssertionContext(HttpResponseMessage? httpResponseMessage,
        HttpRequestMessage? httpRequestMessage, long requestDuration, IServiceProvider serviceProvider)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serviceProvider);

        ResponseMessage = httpResponseMessage;
        RequestMessage = httpRequestMessage;
        RequestDuration = requestDuration;
        ServiceProvider = serviceProvider;

        StatusCode = httpResponseMessage?.StatusCode ?? default;
        IsSuccessStatusCode = httpResponseMessage?.IsSuccessStatusCode ?? false;
    }

    /// <inheritdoc cref="HttpResponseMessage" />
    public HttpResponseMessage? ResponseMessage { get; }

    /// <inheritdoc cref="HttpRequestMessage" />
    public HttpRequestMessage? RequestMessage { get; }

    /// <summary>
    ///     请求耗时（毫秒）
    /// </summary>
    public long RequestDuration { get; }

    /// <summary>
    ///     <inheritdoc cref="IServiceProvider" />
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     响应状态码
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    ///     是否请求成功
    /// </summary>
    public bool IsSuccessStatusCode { get; }

    /// <summary>
    ///     读取响应内容字符串
    /// </summary>
    /// <remarks>支持多次读取。</remarks>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string?> ReadResponseAsStringAsync(CancellationToken cancellationToken = default)
    {
        // 空检查
        if (ResponseMessage is null)
        {
            throw new InvalidOperationException(
                $"{nameof(ResponseMessage)} is null, cannot read response content.");
        }

        // 空检查
        if (_cachedResponseContent is not null)
        {
            return _cachedResponseContent;
        }

        // 启用缓冲，可重复读取
#if NET8_0
        await ResponseMessage.Content.LoadIntoBufferAsync();
#else
        await ResponseMessage.Content.LoadIntoBufferAsync(cancellationToken);
#endif

        _cachedResponseContent = await ResponseMessage.Content.ReadAsStringAsync(cancellationToken);

        return _cachedResponseContent;
    }

    /// <summary>
    ///     读取请求内容字符串
    /// </summary>
    /// <remarks>支持多次读取。</remarks>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public async Task<string?> ReadRequestAsStringAsync(CancellationToken cancellationToken = default)
    {
        // 空检查
        if (RequestMessage?.Content is null)
        {
            return null;
        }

        // 空检查
        if (_cachedRequestContent is not null)
        {
            return _cachedRequestContent;
        }

        // 启用缓冲，可重复读取
        try
        {
#if NET8_0
            await RequestMessage.Content.LoadIntoBufferAsync();
#else
            await RequestMessage.Content.LoadIntoBufferAsync(cancellationToken);
#endif
        }
        catch
        {
            // ignored
        }

        // 读取流内容
        var stream = await RequestMessage.Content.ReadAsStreamAsync(cancellationToken);

        // 检查流是否可读
        if (stream.CanSeek)
        {
            // 重置流指针至起始位置
            stream.Position = 0;
        }

        // 初始化 StreamReader 实例
        using var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);

        // 读取完整内容
        _cachedRequestContent = await streamReader.ReadToEndAsync(cancellationToken);

        return _cachedRequestContent;
    }
}