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

using System.Collections.Concurrent;

namespace Furion.HttpRemote;

/// <summary>
///     Access Token 管理器
/// </summary>
internal sealed class HttpAccessTokenManager : IHttpAccessTokenManager
{
    /// <summary>
    ///     <see cref="HttpClient" /> 实例的配置名称的 Access Token 缓存字典
    /// </summary>
    internal readonly ConcurrentDictionary<string, AccessTokenCache> _httpClientNameCaches = new();

    /// <inheritdoc />
    public async Task SetTokenAsync(string? httpClientName, HttpAccessToken httpAccessToken,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpAccessToken);

        // 获取或创建与 HttpClient 实例的配置名称对应的 Access Token 缓存项
        var accessTokenCache =
            _httpClientNameCaches.GetOrAdd(httpClientName ?? string.Empty, _ => new AccessTokenCache());

        await accessTokenCache.SetAsync(httpAccessToken, cancellationToken);
    }

    /// <inheritdoc />
    public Task<HttpAccessToken?> GetTokenAsync(string? httpClientName, CancellationToken cancellationToken = default)
    {
        // 检查 HttpClient 实例的配置名称是否存在 Access Token 缓存项
        if (!_httpClientNameCaches.TryGetValue(httpClientName ?? string.Empty, out var accessTokenCache))
        {
            return Task.FromResult<HttpAccessToken?>(null);
        }

        // 获取当前缓存的 Access Token
        var current = accessTokenCache.Current;

        // 检查缓存项目是否存在且未过期
        if (current is not null && !current.IsExpired())
        {
            return Task.FromResult<HttpAccessToken?>(current);
        }

        return Task.FromResult<HttpAccessToken?>(null);
    }

    /// <summary>
    ///     获取或刷新指定 <see cref="HttpClient" /> 实例的配置名称的 Access Token
    /// </summary>
    /// <param name="context">
    ///     <see cref="HttpAccessTokenContext" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpAccessToken" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<HttpAccessToken?> GetOrRefreshAsync(HttpAccessTokenContext context,
        CancellationToken cancellationToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(context);

        // 获取或创建与 HttpClient 实例的配置名称对应的 Access Token 缓存项
        var accessTokenCache = _httpClientNameCaches.GetOrAdd(context.HttpClientName, _ => new AccessTokenCache());

        // 检查 Access Token 是否过期
        if (accessTokenCache.Current?.IsExpired() == false)
        {
            return accessTokenCache.Current;
        }

        return await accessTokenCache.GetOrRefreshAsync(context, cancellationToken);
    }

    /// <summary>
    ///     强制刷新指定 <see cref="HttpClient" /> 实例的配置名称的 Access Token
    /// </summary>
    /// <param name="context">
    ///     <see cref="HttpAccessTokenContext" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpAccessToken" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<HttpAccessToken?> ForceRefreshAsync(HttpAccessTokenContext context,
        CancellationToken cancellationToken)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(context);

        // 获取或创建与 HttpClient 实例的配置名称对应的 Access Token 缓存项
        var accessTokenCache = _httpClientNameCaches.GetOrAdd(context.HttpClientName, _ => new AccessTokenCache());

        return await accessTokenCache.ForceRefreshAsync(context, cancellationToken);
    }

    /// <summary>
    ///     Access Token 缓存项
    /// </summary>
    internal sealed class AccessTokenCache
    {
        /// <summary>
        ///     <see cref="SemaphoreSlim" /> 刷新锁
        /// </summary>
        /// <remarks>确保同一时间只有一个刷新 Access Token 操作。</remarks>
        internal readonly SemaphoreSlim _refreshLock = new(1, 1);

        /// <summary>
        ///     当前有效的 <see cref="HttpAccessToken" /> 实例
        /// </summary>
        internal volatile HttpAccessToken? _current;

        /// <summary>
        ///     当前有效的 <see cref="HttpAccessToken" /> 实例
        /// </summary>
        internal HttpAccessToken? Current => _current;

        /// <summary>
        ///     设置 Access Token
        /// </summary>
        /// <param name="httpAccessToken">
        ///     <see cref="HttpAccessToken" />
        /// </param>
        /// <param name="cancellationToken">
        ///     <see cref="CancellationToken" />
        /// </param>
        internal async Task SetAsync(HttpAccessToken httpAccessToken,
            CancellationToken cancellationToken)
        {
            // 等待进入互斥区
            await _refreshLock.WaitAsync(cancellationToken);

            try
            {
                // 更新缓存
                _current = httpAccessToken;
            }
            finally
            {
                // 释放锁
                _refreshLock.Release();
            }
        }

        /// <summary>
        ///     获取或刷新 Access Token
        /// </summary>
        /// <param name="context">
        ///     <see cref="HttpAccessTokenContext" />
        /// </param>
        /// <param name="cancellationToken">
        ///     <see cref="CancellationToken" />
        /// </param>
        /// <returns>
        ///     <see cref="HttpAccessToken" />
        /// </returns>
        internal async Task<HttpAccessToken?> GetOrRefreshAsync(HttpAccessTokenContext context,
            CancellationToken cancellationToken)
        {
            // 等待进入互斥区
            await _refreshLock.WaitAsync(cancellationToken);

            try
            {
                // 检查 Access Token 是否过期
                if (_current?.IsExpired() == false)
                {
                    return _current;
                }

                // 获取新的 Access Token
                var httpAccessToken =
                    await context.HttpAccessTokenProvider.RefreshTokenAsync(context, _current, cancellationToken);

                // 更新缓存
                _current = httpAccessToken;

                return httpAccessToken;
            }
            finally
            {
                // 释放锁
                _refreshLock.Release();
            }
        }

        /// <summary>
        ///     强制刷新 Access Token
        /// </summary>
        /// <param name="context">
        ///     <see cref="HttpAccessTokenContext" />
        /// </param>
        /// <param name="cancellationToken">
        ///     <see cref="CancellationToken" />
        /// </param>
        /// <returns>
        ///     <see cref="HttpAccessToken" />
        /// </returns>
        internal async Task<HttpAccessToken?> ForceRefreshAsync(HttpAccessTokenContext context,
            CancellationToken cancellationToken)
        {
            // 等待进入互斥区
            await _refreshLock.WaitAsync(cancellationToken);

            try
            {
                // 刷新 Access Token
                var httpAccessToken =
                    await context.HttpAccessTokenProvider.RefreshTokenAsync(context, _current, cancellationToken);

                // 更新缓存
                _current = httpAccessToken;

                return httpAccessToken;
            }
            finally
            {
                // 释放锁
                _refreshLock.Release();
            }
        }
    }
}