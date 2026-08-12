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

using Furion;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// HostApplication 扩展
/// </summary>
public static class AppHostApplicationBuilderExtensions
{
    /// <summary>
    /// Host 应用注入
    /// </summary>
    /// <param name="hostApplicationBuilder">Host 应用构建器</param>
    /// <param name="autoRegisterBackgroundService"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder Inject(this IHostApplicationBuilder hostApplicationBuilder, bool autoRegisterBackgroundService = true)
    {
        // 初始化配置
        InternalApp.ConfigureApplication(hostApplicationBuilder, autoRegisterBackgroundService);

        return hostApplicationBuilder;
    }

    /// <summary>
    /// Host 应用注入
    /// </summary>
    /// <param name="hostApplicationBuilder">Host 应用构建器</param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder Inject(this IHostApplicationBuilder hostApplicationBuilder, Action<IHostApplicationBuilder, InjectOptions> configure = default)
    {
        // 载入服务配置选项
        var configureOptions = new InjectOptions();
        configure?.Invoke(hostApplicationBuilder, configureOptions);

        InternalApp.ConfigureApplication(hostApplicationBuilder, configureOptions.AutoRegisterBackgroundService);

        return hostApplicationBuilder;
    }

    /// <summary>
    /// 注册依赖组件
    /// </summary>
    /// <typeparam name="TComponent">派生自 <see cref="IServiceComponent"/></typeparam>
    /// <param name="hostApplicationBuilder">Host 应用构建器</param>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddComponent<TComponent>(this IHostApplicationBuilder hostApplicationBuilder, object options = default)
        where TComponent : class, IServiceComponent, new()
    {
        hostApplicationBuilder.Services.AddComponent<TComponent>(options);

        return hostApplicationBuilder;
    }

    /// <summary>
    /// 注册依赖组件
    /// </summary>
    /// <typeparam name="TComponent">派生自 <see cref="IServiceComponent"/></typeparam>
    /// <typeparam name="TComponentOptions">组件参数</typeparam>
    /// <param name="hostApplicationBuilder">Host 应用构建器</param>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddComponent<TComponent, TComponentOptions>(this IHostApplicationBuilder hostApplicationBuilder, TComponentOptions options = default)
        where TComponent : class, IServiceComponent, new()
    {
        hostApplicationBuilder.Services.AddComponent<TComponent, TComponentOptions>(options);

        return hostApplicationBuilder;
    }

    /// <summary>
    /// 注册依赖组件
    /// </summary>
    /// <param name="hostApplicationBuilder">Host 应用构建器</param>
    /// <param name="componentType">组件类型</param>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddComponent(this IHostApplicationBuilder hostApplicationBuilder, Type componentType, object options = default)
    {
        hostApplicationBuilder.Services.AddComponent(componentType, options);

        return hostApplicationBuilder;
    }
}