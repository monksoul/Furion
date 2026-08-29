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

using Furion.HttpRemote;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     HTTP 远程请求模块 <see cref="IServiceCollection" /> 扩展类
/// </summary>
public static class HttpRemoteServiceCollectionExtensions
{
    /// <summary>
    ///     添加 HTTP 远程请求服务
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="IHttpRemoteBuilder" />
    /// </returns>
    public static IHttpRemoteBuilder AddHttpRemote(this IServiceCollection services
        , Action<HttpRemoteBuilder>? configure = null)
    {
        // 初始化 HTTP 远程请求构建器
        var httpRemoteBuilder = new HttpRemoteBuilder();

        // 调用自定义配置委托
        configure?.Invoke(httpRemoteBuilder);

        return services.AddHttpRemote(httpRemoteBuilder);
    }

    /// <summary>
    ///     添加 HTTP 远程请求服务
    /// </summary>
    /// <param name="services">
    ///     <see cref="IServiceCollection" />
    /// </param>
    /// <param name="httpRemoteBuilder">
    ///     <see cref="HttpRemoteBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="IHttpRemoteBuilder" />
    /// </returns>
    public static IHttpRemoteBuilder AddHttpRemote(this IServiceCollection services,
        HttpRemoteBuilder httpRemoteBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteBuilder);

        // 构建模块服务
        httpRemoteBuilder.Build(services);

        return new DefaultHttpRemoteBuilder(services);
    }

    /// <summary>
    ///     将应用程序的主 <see cref="IServiceProvider" /> 注入到 <see cref="HttpRemoteClient" />，使其优先使用该容器解析
    ///     <see cref="IHttpRemoteService" /> 服务
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         调用此方法后，<see cref="HttpRemoteClient.Service" /> 将从外部容器获取服务实例，而非自行构建独立 DI 容器。注入的容器生命周期由调用方管理，
    ///         <see cref="HttpRemoteClient.Dispose" /> 不会释放它。
    ///     </para>
    ///     <para>注意：请确保使用的是根容器（Root ServiceProvider），使用作用域容器可能导致对象生命周期异常。</para>
    /// </remarks>
    /// <param name="serviceProvider">应用程序的根服务提供器，必须已完成 <see cref="IHttpRemoteService" /> 的注册</param>
    /// <returns>
    ///     <see cref="IServiceProvider" />
    /// </returns>
    public static IServiceProvider UseHttpRemoteClient(this IServiceProvider serviceProvider)
    {
        HttpRemoteClient.SetServiceProvider(serviceProvider);

        return serviceProvider;
    }
}