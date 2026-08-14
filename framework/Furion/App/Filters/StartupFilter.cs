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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Furion;

/// <summary>
/// 应用启动时自动注册中间件
/// </summary>
/// <remarks>
/// </remarks>
public class StartupFilter : IStartupFilter
{
    /// <summary>
    /// 配置中间件
    /// </summary>
    /// <param name="next"></param>
    /// <returns></returns>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // 存储根服务
            InternalApp.RootServices ??= app.ApplicationServices;

            // 环境名
            var envName = App.HostEnvironment?.EnvironmentName ?? "Unknown";
            var version = $"{GetType().Assembly.GetName().Version}";

            // 设置响应报文头信息
            app.Use(async (context, next) =>
            {
                // 处理 WebSocket 请求
                if (context.IsWebSocketRequest()) await next.Invoke();
                else
                {
                    // 输出当前环境标识
                    context.Response.Headers["Environment"] = envName;

                    // 输出框架版本
                    context.Response.Headers[nameof(Furion)] = version;

                    // 执行下一个中间件
                    await next.Invoke();

                    // 解决刷新 Token 时间和 Token 时间相近问题
                    if (!context.Response.HasStarted
                        && context.Response.StatusCode == StatusCodes.Status401Unauthorized
                        && context.Response.Headers.ContainsKey("access-token")
                        && context.Response.Headers.ContainsKey("x-access-token"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    }

                    // 释放所有未托管的服务提供器
                    App.DisposeUnmanagedObjects();
                }
            });

            // 调用默认中间件
            app.UseApp();

            // 配置所有 Startup Configure
            UseStartups(app);

            // 调用启动层的 Startup
            next(app);
        };
    }

    /// <summary>
    /// 配置 Startup 的 Configure
    /// </summary>
    /// <param name="app">应用构建器</param>
    private static void UseStartups(IApplicationBuilder app)
    {
        // 反转，处理排序
        var startups = App.AppStartups.Reverse();
        if (!startups.Any()) return;

        // 处理【部署】二级虚拟目录
        var virtualPath = App.Settings.VirtualPath;
        if (!string.IsNullOrWhiteSpace(virtualPath) && virtualPath.StartsWith('/'))
        {
            app.Map(virtualPath, _app => UseStartups(startups, _app));
            return;
        }

        UseStartups(startups, app);
    }

    /// <summary>
    /// 批量将自定义 AppStartup 添加到 Startup.cs 的 Configure 中
    /// </summary>
    /// <param name="startups"></param>
    /// <param name="app"></param>
    private static void UseStartups(IEnumerable<object> startups, IApplicationBuilder app)
    {
        // 遍历所有
        foreach (var startup in startups)
        {
            var type = startup.GetType();

            // 获取所有符合依赖注入格式的方法，如返回值 void，且第一个参数是 IApplicationBuilder 类型
            var configureMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(u => u.ReturnType == typeof(void)
                    && u.GetParameters().Length > 0
                    && u.GetParameters()[0].ParameterType == typeof(IApplicationBuilder))
                .ToList();

            if (configureMethods.Count == 0) continue;

            foreach (var method in configureMethods)
            {
                var target = method.IsStatic ? null : startup;
                method.Invoke(target, ResolveMethodParameterInstances(app, method));
            }
        }

        // 释放内存
        App.AppStartups.Clear();
    }

    /// <summary>
    /// 解析方法参数实例
    /// </summary>
    /// <param name="app"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    private static object[] ResolveMethodParameterInstances(IApplicationBuilder app, MethodInfo method)
    {
        // 获取方法所有参数
        var parameters = method.GetParameters();
        var parameterInstances = new object[parameters.Length];
        parameterInstances[0] = app;

        var serviceProvider = app.ApplicationServices;

        // 解析服务
        for (var i = 1; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            parameterInstances[i] = serviceProvider.GetRequiredService(parameter.ParameterType);
        }

        return parameterInstances;
    }
}