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
using System.Net.Http.Headers;

namespace Furion.HttpRemote;

/// <summary>
///     ETag 缓存管道处理器
/// </summary>
/// <remarks>参考文献：https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Reference/Headers/ETag。</remarks>
/// <param name="etagCache">
///     <see cref="IHttpETagCache" />
/// </param>
internal sealed class ETagPipelineHandler(IHttpETagCache etagCache) : IHttpRequestPipelineHandler
{
    /// <inheritdoc />
    public async Task<HttpResponseMessage?> HandleAsync(HttpRequestPipelineContext context,
        Func<Task<HttpResponseMessage?>> next)
    {
        // 获取当前 HttpRequestBuilder 实例
        var httpRequestBuilder = context.Builder;

        // 检查是否禁用缓存或未启用 ETag 缓存功能
        if (httpRequestBuilder.DisableCacheEnabled || !httpRequestBuilder.ETagEnabled)
        {
            // 调用下一个处理器的委托
            return await next();
        }

        // 获取当前 HttpRequestMessage 实例
        var httpRequestMessage = context.RequestMessage;

        // 检查是否是 GET 和 HEAD 请求
        if (httpRequestMessage is null ||
            (httpRequestMessage.Method != HttpMethod.Get && httpRequestMessage.Method != HttpMethod.Head))
        {
            // 调用下一个处理器的委托
            return await next();
        }

        // 生成缓存键
        var cacheKey = GenerateCacheKey(httpRequestMessage);

        // 如果缓存中存在 ETag，则添加到请求头中
        if (etagCache.TryGet(cacheKey, out var eTagCacheItem) && eTagCacheItem?.ETag is not null)
        {
            httpRequestMessage.Headers.IfNoneMatch.Clear();
            httpRequestMessage.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{eTagCacheItem.ETag}\""));
        }

        // 调用下一个处理器的委托
        var httpResponseMessage = await next();

        // 空检查
        if (httpResponseMessage is null)
        {
            return null;
        }

        // 检查是否收到 304 Not Modified 状态码
        if (httpResponseMessage.StatusCode == HttpStatusCode.NotModified && eTagCacheItem is not null)
        {
            // 释放前一个 HttpResponseMessage 实例
            httpResponseMessage.Dispose();

            // 从缓存项重建完整响应消息
            var cachedResponseMessage = BuildResponseFromCacheItem(eTagCacheItem);

            // 更新上下文
            context.ResponseMessage = cachedResponseMessage;

            return cachedResponseMessage;
        }

        // 检查是否成功请求且包含 ETag 标头
        if (httpResponseMessage.IsSuccessStatusCode && httpResponseMessage.Headers.ETag is { } entityTagHeaderValue)
        {
            // 缓存 HttpResponseMessage 信息
            await CacheResponseAsync(cacheKey, httpResponseMessage, entityTagHeaderValue);
        }

        return httpResponseMessage;
    }

    /// <summary>
    ///     生成缓存键
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static string GenerateCacheKey(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestMessage);

        // 获取请求地址
        var requestUri = httpRequestMessage.RequestUri!;

        return $"{httpRequestMessage.Method}:{requestUri.AbsoluteUri}";
    }

    /// <summary>
    ///     缓存 <see cref="HttpResponseMessage" /> 信息
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="entityTagHeaderValue">
    ///     <see cref="EntityTagHeaderValue" />
    /// </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    internal async Task CacheResponseAsync(string cacheKey, HttpResponseMessage httpResponseMessage,
        EntityTagHeaderValue entityTagHeaderValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);
        ArgumentNullException.ThrowIfNull(httpResponseMessage);
        ArgumentNullException.ThrowIfNull(entityTagHeaderValue);

        // 初始化响应内容字节数组
        byte[]? contentBytes = null;

        // 空检查
        if (httpResponseMessage.Content is not null)
        {
            // 读取响应内容字节数组
            contentBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();
        }

        // 初始化 HttpETagCacheItem 实例
        var eTagCacheItem = new HttpETagCacheItem
        {
            // 移除前后双引号
            ETag = entityTagHeaderValue.Tag.Trim('"'),
            ContentBytes = contentBytes,
            ContentHeaders = httpResponseMessage.Content?.Headers.ToDictionary(h => h.Key, h => h.Value),
            StatusCode = httpResponseMessage.StatusCode,
            ResponseHeaders = httpResponseMessage.Headers.ToDictionary(h => h.Key, h => h.Value),
            ReasonPhrase = httpResponseMessage.ReasonPhrase
        };

        // 更新缓存
        etagCache.Set(cacheKey, eTagCacheItem);
    }

    /// <summary>
    ///     从缓存项重建 <see cref="HttpResponseMessage" /> 对象
    /// </summary>
    /// <param name="eTagCacheItem">
    ///     <see cref="HttpETagCacheItem" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpResponseMessage" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static HttpResponseMessage BuildResponseFromCacheItem(HttpETagCacheItem eTagCacheItem)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(eTagCacheItem);

        // 初始化 HttpResponseMessage 实例
        var httpResponseMessage = new HttpResponseMessage(eTagCacheItem.StatusCode);

        // 检查是否包含响应内容
        if (eTagCacheItem.ContentBytes is { Length: > 0 })
        {
            // 初始化 ByteArrayContent 实例
            var byteArrayContent = new ByteArrayContent(eTagCacheItem.ContentBytes);

            // 空检查
            if (eTagCacheItem.ContentHeaders is not null)
            {
                // 还原响应内容标头
                foreach (var header in eTagCacheItem.ContentHeaders)
                {
                    byteArrayContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // 设置响应内容
            httpResponseMessage.Content = byteArrayContent;
        }

        // 空检查
        if (eTagCacheItem.ResponseHeaders is not null)
        {
            // 还原响应标头
            foreach (var header in eTagCacheItem.ResponseHeaders)
            {
                httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // 设置原因短语
        httpResponseMessage.ReasonPhrase = eTagCacheItem.ReasonPhrase;

        return httpResponseMessage;
    }
}