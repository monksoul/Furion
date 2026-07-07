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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization.Metadata;

namespace Furion.Components;

/// <summary>
/// Serve 组件应用服务组件
/// </summary>
public sealed class ServeServiceComponent : IServiceComponent
{
    /// <summary>
    /// 装载服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="componentContext"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Load(IServiceCollection services, ComponentContext componentContext)
    {
        // 控制台日志美化
        services.AddConsoleFormatter();

        // 配置跨域
        services.AddCorsAccessor();

        // 控制器和规范化结果
        services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                })
                .AddInjectWithUnifyResult();
    }
}

/// <summary>
/// Serve 组件应用中间件组件
/// </summary>
public sealed class ServeApplicationComponent : IApplicationComponent
{
    /// <summary>
    /// 装载中间件
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    /// <param name="componentContext"></param>
    public void Load(IApplicationBuilder app, IWebHostEnvironment env, ComponentContext componentContext)
    {
        // 配置错误页
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // 401，403 规范化结果
        app.UseUnifyResultStatusCodes();

        // 配置静态
        app.UseStaticFiles();

        // 注册定时任务 UI
        app.UseScheduleUI();

        // 配置路由
        app.UseRouting();

        // 配置跨域
        app.UseCorsAccessor();

        // 配置授权
        app.UseAuthentication();
        app.UseAuthorization();

        // 框架基础配置
        app.UseInject(string.Empty);

        // 配置路由
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}