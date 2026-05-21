using Microsoft.AspNetCore.Mvc;

namespace Furion.TestProject;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddInject();
        services.AddViewEngine();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        // 这里如果不改框架初始化swagger的顺序逻辑, 没办法在 minimal api 定义之前获取到分组和Ui根据分组配置终点路由
        // app.UseInject(string.Empty); 

        app.UseEndpoints(endpoints =>
        {
            var groupApi =endpoints.MapGroup("/")
                .WithGroupName("group");
        
            groupApi.MapGet("/afterInject", () => "testAfterInject")
                .WithGroupName("afterInject")
                .WithGroupName("otherGroup");
            endpoints.MapGet("withMetadata", () => "withMetadata")
                .WithMetadata(new ApiDescriptionSettingsAttribute
                {
                    Groups = ["withMetadata", "afterInject"]
                });
            endpoints.MapControllers();
        });
        // 所以需要放到后面 minimalapi的后面,
        // 除非定义一套规则, 像收集controller的action一样收集元数据的方式, 把minimal api的元数据收集起来
        // app.UseInject("api");
        app.UseInject(string.Empty);
    }
}