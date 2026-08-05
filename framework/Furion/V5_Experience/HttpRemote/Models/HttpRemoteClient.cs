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

namespace Furion.HttpRemote;

/// <summary>
///     提供静态访问 <see cref="IHttpRemoteService" /> 服务的方式
/// </summary>
/// <remarks>
///     <para>支持服务的延迟初始化、配置更新以及资源释放。</para>
///     <para>
///         注意：可通过 <see cref="SetServiceProvider" /> 方法注入应用程序的主 <see cref="IServiceProvider" />，此时将优先从该容器解析
///         <see cref="IHttpRemoteService" />，与应用的 DI 体系保持共享；若未注入外部容器，则本类维护一套独立的 DI 容器。
///     </para>
/// </remarks>
public static class HttpRemoteClient
{
    /// <summary>
    ///     标记服务是否已释放
    /// </summary>
    internal static volatile bool _isDisposed;

    /// <summary>
    ///     当前 <see cref="IHttpRemoteService" /> 实例
    /// </summary>
    internal static volatile IHttpRemoteService? _serviceInstance;

    /// <summary>
    ///     自行构建的内部 <see cref="IServiceProvider" />
    /// </summary>
    internal static IServiceProvider? _serviceProvider;

    /// <summary>
    ///     外部注入的应用程序主 <see cref="IServiceProvider" />
    /// </summary>
    internal static IServiceProvider? _externalServiceProvider;

    /// <summary>
    ///     并发锁对象
    /// </summary>
    internal static readonly object _lock = new();

    /// <summary>
    ///     自定义服务注册逻辑的委托
    /// </summary>
    internal static Action<IServiceCollection> _configure = services => services.AddHttpRemote();

    /// <summary>
    ///     获取当前配置下的 <see cref="IHttpRemoteService" /> 实例
    /// </summary>
    public static IHttpRemoteService Service
    {
        get
        {
            // 释放检查
            ObjectDisposedException.ThrowIf(_isDisposed, typeof(HttpRemoteClient));

            // 双重检查锁定
            // ReSharper disable once InvertIf
            if (_serviceInstance is null)
            {
                lock (_lock)
                {
                    // 释放检查
                    ObjectDisposedException.ThrowIf(_isDisposed, typeof(HttpRemoteClient));

                    // 空检查
                    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                    if (_serviceInstance is null)
                    {
                        _serviceInstance = CreateService();
                    }
                }
            }

            return _serviceInstance;
        }
    }

    /// <summary>
    ///     设置应用程序的主 <see cref="IServiceProvider" />
    /// </summary>
    /// <remarks>
    ///     <para>优先使用该容器解析服务。</para>
    ///     <para>调用此方法后，后续访问 <see cref="Service" /> 将直接从外部容器获取实例，而非构建独立容器。外部容器的生命周期由调用方管理，本类不会释放它。</para>
    ///     <para>
    ///         注意：传入的 <paramref name="serviceProvider" /> 必须已完成 <see cref="IHttpRemoteService" /> 的注册，否则后续访问
    ///         <see cref="Service" /> 时将抛出异常。
    ///     </para>
    /// </remarks>
    /// <param name="serviceProvider">外部注入的应用程序主 <see cref="IServiceProvider" /></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serviceProvider);

        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, typeof(HttpRemoteClient));

            // 释放自行构建的内部服务提供器
            ReleaseInternalServiceProvider();
            _serviceInstance = null;

            _externalServiceProvider = serviceProvider;
        }
    }

    /// <summary>
    ///     自定义服务注册逻辑
    /// </summary>
    /// <remarks>仅影响自行构建的内部容器。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public static void Configure(Action<IServiceCollection> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, typeof(HttpRemoteClient));

            // 更新配置委托
            var previousConfigure = _configure;
            _configure = services =>
            {
                // 调用历史配置委托
                previousConfigure(services);

                // 调用当前自定义配置委托
                configure(services);

                // 检查 HTTP 远程请求服务是否已注册，若未注册则自动完成注册 
                // ReSharper disable once SimplifyLinqExpressionUseAll
                if (!services.Any(u => u.ServiceType == typeof(IHttpRemoteService)))
                {
                    services.AddHttpRemote();
                }
            };

            // 重新初始化服务
            Reinitialize();
        }
    }

    /// <summary>
    ///     释放服务提供器及相关资源
    /// </summary>
    /// <remarks>
    ///     <para>通常在应用程序关闭或不再需要 HTTP 远程请求服务时调用。</para>
    ///     <para>注意：释放自行构建的内部容器及相关资源，外部注入的容器不受影响。</para>
    /// </remarks>
    public static void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                return;
            }

            // 释放自行构建的内部服务提供器
            ReleaseInternalServiceProvider();
            _serviceInstance = null;
            _externalServiceProvider = null;

            // 标记为已释放状态
            _isDisposed = true;
        }
    }

    /// <summary>
    ///     创建 <see cref="IHttpRemoteService" /> 实例
    /// </summary>
    /// <returns>
    ///     <see cref="IHttpRemoteService" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static IHttpRemoteService CreateService()
    {
        try
        {
            // 优先使用外部容器
            if (_externalServiceProvider is not null)
            {
                return _externalServiceProvider.GetRequiredService<IHttpRemoteService>();
            }

            // 回退处理：初始化 ServiceCollection 实例
            var services = new ServiceCollection();

            // 调用自定义服务注册逻辑的委托
            _configure(services);

            // 构建服务提供器
            var provider = services.BuildServiceProvider();
            _serviceProvider = provider;

            // 解析并返回
            return provider.GetRequiredService<IHttpRemoteService>();
        }
        catch (Exception ex)
        {
            // 释放自行构建的内部服务提供器
            ReleaseInternalServiceProvider();

            throw new InvalidOperationException("Failed to initialize IHttpRemoteService.", ex);
        }
    }

    /// <summary>
    ///     使用最新的配置重新初始化服务
    /// </summary>
    internal static void Reinitialize()
    {
        // 释放自行构建的内部服务提供器
        ReleaseInternalServiceProvider();
        _serviceInstance = null;
    }

    /// <summary>
    ///     释放自行构建的内部服务提供器
    /// </summary>
    internal static void ReleaseInternalServiceProvider()
    {
        // 如果服务提供器支持释放资源，则执行释放操作
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _serviceProvider = null;
    }
}