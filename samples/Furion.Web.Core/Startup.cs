using Furion.Application;
using Furion.Schedule;
using Furion.TimeCrontab;
using Furion.VirtualFileServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Furion.Web.Core;

[AppStartup(700)]
public sealed class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddConsoleFormatter();

        // 注册 JWT 授权
        services.AddJwt<AuthHandler>();

        services.AddCorsAccessor();

        //services.ConfigureDynamicApiController(builder =>
        //{
        //    builder.ControllerFilter = (controllerModel) =>
        //    {
        //        return controllerModel.ControllerType != typeof(PersonService);
        //    };

        //    builder.ActionConfigure = (actionModel) =>
        //    {
        //        if (actionModel.ActionMethod.Name == "Insert")
        //        {
        //            actionModel.ApiExplorer.IsVisible = false;
        //        }
        //    };
        //});

        services.AddControllersWithViews()
                // 配置多语言
                .AddAppLocalization()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    options.JsonSerializerOptions.Converters.AddDateTimeTypeConverters(localized: true);
                    options.JsonSerializerOptions.Converters.AddClayConverters();

                    options.JsonSerializerOptions.Converters.AddDateOnlyConverters("yyyy-MM-dd");
                    options.JsonSerializerOptions.Converters.AddTimeOnlyConverters("HH:mm:ss");

                    options.JsonSerializerOptions.Converters.AddDataTableConverters();
                    options.JsonSerializerOptions.Converters.AddDataSetConverters();
                    options.JsonSerializerOptions.Converters.AddLongTypeConverters();
                    options.JsonSerializerOptions.Converters.AddNullableConverters();
                })
                .AddClayOptions(options =>
                {
                    options.KeyValueJsonToObject = true;
                })
                .AddInjectWithUnifyResult()
                .AddUnifyJsonOptions("special", new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                });

        services.AddUnifyProvider<SpeciallyResultProvider>("specially");

        services.AddUnifyJsonOptions("specially", new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });

        services.AddHttpRemote();

        services.AddEventBus(options =>
        {
            options.AddFallbackPolicy<EventFallbackPolicy>();
        });

        // 添加实时通讯
        services.AddSignalR();

        services.AddFileLogging();

        services.AddDatabaseLogging<DatabaseLoggingWriter>();

        services.AddMonitorLogging();

        services.AddFromConvertBinding();

        services.AddSchedule(options =>
        {
            //options.UseUtcTimestamp = true;
            //options.AddJob(JobBuilder.Create<TestJob>().SetDescription("这是定时任务包含多个作业触发器")
            //    , Triggers.Minutely(), Triggers.Period(5000).SetDescription("这是作业触发器，间隔 5 秒"));
            //options.AddJob<TestJob>(Triggers.Hourly());

            //options.AddHttpJob(request =>
            //{
            //    request.RequestUri = "https://www.chinadot.net";
            //    request.HttpMethod = HttpMethod.Get;
            //    request.Headers.Add("framework", "Furion");
            //    request.GroupName = "group";
            //    request.Description = "作业请求描述";
            //}, Triggers.PeriodSeconds(5));

            //options.AddJob((context, stoppingToken) =>
            //{
            //    context.ServiceProvider.GetLogger().LogInformation($"{context}");
            //    return Task.CompletedTask;
            //}, Triggers.PeriodSeconds(2));

            //options.AddPersistence<TestJobPersistence>();

            options.GroupSet("group1", () =>
            {
                options.AddJob<TestJob>(Triggers.PeriodHours(1).SetMaxNumberOfRuns(2), Triggers.PeriodSeconds(4));
                //options.AddJobFactory<JobFactory>();
                options.AddJob<TestCancelJob>("cancelJob", Triggers.PeriodSeconds(30));
            });

            options.GroupSet("group2", () =>
            {
                options.AddJob<TestJob>(Triggers.PeriodHours(1)
                    .SetStartTime(DateTime.Now.AddMonths(-2))
                    .SetRunOnStart(true));
            });

            options.AddJob<TestJob>(Triggers.PeriodHours(2));

            options.AddJob<TestCancelJob>(Triggers.PeriodHours(2)
               , Triggers.PeriodHours(2));

            options.AddJob<TestCancelJob>();

            options.AddJob<TestCancelJob>();
            options.AddJob(JobBuilder.Create<TestJob>().SetDescription("测试描述"), Triggers.PeriodHours(2));

            options.AddJob<TestJob>(builder => builder.SetTemporary(), Triggers.PeriodMinutes(1));

            options.AddJob<TestJob>(u => u.SetIncludeAnnotations(true), Triggers.At("2025-11-26 14:58:20"));

            options.AddJob(JobBuilder.Create<TestJob>().SetDescription("随机"), Triggers.Cron("H(30-59) * * * * *", CronStringFormat.WithSeconds));

            options.AddPersistence<TestJobPersistence>();
        });

        services.AddTaskQueue(options =>
        {
            //options.Concurrent = false;
        });

        services.AddViewEngine();

        services.AddSensitiveDetection(options =>
        {
            //options.EmbedFileName = "custom-words.txt";
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // 添加规范化结果状态码，需要在这里注册
        app.UseUnifyResultStatusCodes();

        app.UseHttpsRedirection();

        // 配置多语言，必须在 路由注册之前
        app.UseAppLocalization(options =>
        {
            // options.AddInitialRequestCultureProvider(new CustomizeQueryStringRequestCultureProvider("my-culture"));
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = FS.GetFileExtensionContentTypeProvider()
        });

        app.UseRouting();

        app.UseCorsAccessor();

        app.UseAuthentication();
        app.UseAuthorization();

        app.EnableBuffering();

        app.UseScheduleUI(options =>
        {
            options.Title = "定时任务";

            options.LoginConfig.DefaultUsername = "furion";
            options.LoginConfig.DefaultPassword = "";
            options.LoginConfig.AppSecret = "3f2d0ea0ef4df562719e70e41413658e";

            options.LoginConfig.OnLoging = async (username, password, httpContext) =>
            {
                return await Task.FromResult(username == "furion" && string.IsNullOrWhiteSpace(password));
            };
        });

        app.UseInject();

        app.UseEndpoints(endpoints =>
        {
            // 批量注册集线器
            endpoints.MapHubs();

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            endpoints.MapGet("/mini", () => "Hello, World!")
                     .WithTags("miniapi 分组")
                     .WithSummary("这是描述")
                     .WithGroupName("minimal");
        });
    }
}