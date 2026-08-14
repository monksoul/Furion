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

using Furion.Components;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 组件应用中间件扩展类
/// </summary>
public static class ComponentApplicationBuilderExtensions
{
    /// <summary>
    /// 已执行过中间件注册的根组件类型
    /// </summary>
    private static readonly ConcurrentDictionary<Type, byte> _loadedApplicationComponents = new();

    /// <summary>
    /// 注册依赖组件
    /// </summary>
    /// <typeparam name="TComponent">派生自 <see cref="IApplicationComponent"/></typeparam>
    /// <param name="app"><see cref="IApplicationBuilder"/></param>
    /// <param name="env"><see cref="IWebHostEnvironment"/></param>
    /// <param name="options">组件参数</param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder UseComponent<TComponent>(this IApplicationBuilder app, IWebHostEnvironment env, object options = default)
        where TComponent : class, IApplicationComponent, new()
    {
        return app.UseComponent<TComponent, object>(env, options);
    }

    /// <summary>
    /// 注册依赖组件
    /// </summary>
    /// <typeparam name="TComponent">派生自 <see cref="IApplicationComponent"/></typeparam>
    /// <typeparam name="TComponentOptions">组件参数</typeparam>
    /// <param name="app"><see cref="IApplicationBuilder"/></param>
    /// <param name="env"><see cref="IWebHostEnvironment"/></param>
    /// <param name="options">组件参数</param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder UseComponent<TComponent, TComponentOptions>(this IApplicationBuilder app, IWebHostEnvironment env, TComponentOptions options = default)
        where TComponent : class, IApplicationComponent, new()
    {
        return app.UseComponent(env, typeof(TComponent), options);
    }

    /// <summary>
    /// 注册依赖组件
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/></param>
    /// <param name="env"><see cref="IWebHostEnvironment"/></param>
    /// <param name="componentType">组件类型</param>
    /// <param name="options">组件参数</param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder UseComponent(this IApplicationBuilder app, IWebHostEnvironment env, Type componentType, object options = default)
    {
        // 防止重复注册同一个根组件
        if (!_loadedApplicationComponents.TryAdd(componentType, 0))
        {
            return app;
        }

        // 创建组件依赖链
        var componentContextLinkList = Penetrates.CreateDependLinkList(componentType, options);

        // 逐条创建组件实例并调用
        foreach (var componentContext in componentContextLinkList)
        {
            // 检查当前组件是否实现了 IApplicationComponent 接口
            if (!typeof(IApplicationComponent).IsAssignableFrom(componentContext.ComponentType))
            {
                continue;
            }

            // 创建组件实例
            var component = Activator.CreateInstance(componentContext.ComponentType) as IApplicationComponent;

            // 调用
            component?.Load(app, env, componentContext);

            // 释放资源
            if (component is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        return app;
    }
}