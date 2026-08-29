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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Hosting;

namespace Furion;

/// <summary>
/// 内部 App 副本
/// </summary>
internal static class InternalApp
{
    /// <summary>
    /// 应用服务
    /// </summary>
    internal static IServiceCollection InternalServices;

    /// <summary>
    /// 根服务
    /// </summary>
    internal static IServiceProvider RootServices;

    /// <summary>
    /// 配置对象
    /// </summary>
    internal static IConfiguration Configuration;

    /// <summary>
    /// 获取Web主机环境
    /// </summary>
    internal static IWebHostEnvironment WebHostEnvironment;

    /// <summary>
    /// 获取泛型主机环境
    /// </summary>
    internal static IHostEnvironment HostEnvironment;

    /// <summary>
    /// 配置 Furion 框架（Web）
    /// </summary>
    /// <remarks>此次添加 <see cref="HostBuilder"/> 参数是为了兼容 .NET 5 直接升级到 .NET 6 问题</remarks>
    /// <param name="builder"></param>
    /// <param name="hostBuilder"></param>
    internal static void ConfigureApplication(IWebHostBuilder builder, IHostBuilder hostBuilder = default)
    {
        // 自动装载配置
        if (hostBuilder == default)
        {
            builder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
            {
                // 存储环境对象
                HostEnvironment = WebHostEnvironment = hostContext.HostingEnvironment;

                // 加载配置
                AddJsonFiles(configurationBuilder, hostContext.HostingEnvironment);

                // 加载自定义配置
                InjectOptions.WebAppConfigurationConfigure?.Invoke(hostContext, configurationBuilder);
            });
        }
        // 自动装载配置
        else ConfigureHostAppConfiguration(hostBuilder);

        // 应用初始化服务
        builder.ConfigureServices((hostContext, services) =>
        {
            // 存储配置对象
            Configuration = hostContext.Configuration;

            // 存储服务提供器
            InternalServices = services;

            // 存储根服务（解决 Web 主机还未启动时在 HostedService 中使用 App.GetService 问题
            services.AddHostedService<GenericHostLifetimeEventsHostedService>();

            // 注册 Startup 过滤器
            services.AddTransient<IStartupFilter, StartupFilter>();

            // 注册 HttpContextAccessor 服务
            services.AddHttpContextAccessor();

            // 初始化应用服务
            services.AddApp(hostContext.Configuration);

            // 加载自定义配置
            InjectOptions.WebServicesConfigure?.Invoke(hostContext, services);
        });
    }

    /// <summary>
    /// 配置 Furion 框架（非 Web）
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="autoRegisterBackgroundService"></param>
    internal static void ConfigureApplication(IHostBuilder builder, bool autoRegisterBackgroundService = true)
    {
        // 自动装载配置
        ConfigureHostAppConfiguration(builder);

        // 自动注入 AddApp() 服务
        builder.ConfigureServices((hostContext, services) =>
        {
            // 存储配置对象
            Configuration = hostContext.Configuration;

            // 存储服务提供器
            InternalServices = services;

            // 存储根服务
            services.AddHostedService<GenericHostLifetimeEventsHostedService>();

            // 初始化应用服务
            services.AddApp(hostContext.Configuration);

            // 自动注册 BackgroundService
            if (autoRegisterBackgroundService) services.AddAppHostedService();

            // 加载自定义配置
            InjectOptions.ServicesConfigure?.Invoke(hostContext, services);
        });
    }

    /// <summary>
    /// 配置 Furion 框架（非 Web）
    /// </summary>
    /// <param name="hostApplicationBuilder"></param>
    /// <param name="autoRegisterBackgroundService"></param>
    internal static void ConfigureApplication(IHostApplicationBuilder hostApplicationBuilder, bool autoRegisterBackgroundService = true)
    {
        // 存储环境对象
        HostEnvironment = hostApplicationBuilder.Environment;

        // 加载配置
        AddJsonFiles(hostApplicationBuilder.Configuration, hostApplicationBuilder.Environment);

        // 存储配置对象
        Configuration = hostApplicationBuilder.Configuration;

        // 存储服务提供器
        InternalServices = hostApplicationBuilder.Services;

        // 存储根服务
        hostApplicationBuilder.Services.AddHostedService<GenericHostLifetimeEventsHostedService>();

        // 初始化应用服务
        hostApplicationBuilder.Services.AddApp(hostApplicationBuilder.Configuration);

        // 自动注册 BackgroundService
        if (autoRegisterBackgroundService) hostApplicationBuilder.Services.AddAppHostedService();
    }

    /// <summary>
    /// 自动装载主机配置
    /// </summary>
    /// <param name="builder"></param>
    private static void ConfigureHostAppConfiguration(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
        {
            // 存储环境对象
            HostEnvironment = hostContext.HostingEnvironment;

            // 加载配置
            AddJsonFiles(configurationBuilder, hostContext.HostingEnvironment);

            // 加载自定义配置
            InjectOptions.AppConfigurationConfigure?.Invoke(hostContext, configurationBuilder);
        });
    }

    /// <summary>
    /// 加载自定义 .json 配置文件
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="hostEnvironment"></param>
    internal static void AddJsonFiles(IConfigurationBuilder configurationBuilder, IHostEnvironment hostEnvironment)
    {
        // 获取根配置
        var configuration = configurationBuilder is ConfigurationManager configurationManager
            ? configurationManager
            : configurationBuilder.Build();

        // 获取程序执行目录
        var executeDirectory = AppContext.BaseDirectory;

        // 获取自定义配置扫描目录
        var configurationScanDirectories = configuration.GetSection("ConfigurationScanDirectories")
            .Get<string[]>() ?? [];

        // 聚合所有扫描目录，并过滤掉不存在的目录
        var directories = new[] { executeDirectory }
            .Concat(configurationScanDirectories.Select(u => Path.Combine(executeDirectory, u)))
            .Concat(InjectOptions.InternalConfigurationScanDirectories)
            .Where(Directory.Exists);

        // 扫描目录下所有 *.json 文件
        var allJsonFiles = directories
            .SelectMany(dir => Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly))
            .ToList();

        // 如果没有配置文件，中止执行
        if (allJsonFiles.Count == 0) return;

        // 获取 JSON 文件扫描配置（2025.07.25），修复 docker 中挂载大文件数据卷导致启动缓慢的问题
        var jsonFileScanner = configuration.GetSection("AppSettings:JsonFileScanner")
            .Get<JsonFileScanner>() ?? new JsonFileScanner();

        // 获取环境名称
        var envName = hostEnvironment?.EnvironmentName
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Production";

        // 读取忽略的配置文件列表
        var ignoreConfigurationFiles = (configuration.GetSection("IgnoreConfigurationFiles")
                .Get<string[]>()
            ?? []).Concat(InjectOptions.InternalIgnoreConfigurationFiles);

        // 将忽略列表拆分为精确文件名和 glob 模式
        var ignoreFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ignoreMatchers = new List<Matcher>();

        foreach (var ignorePattern in ignoreConfigurationFiles)
        {
            if (ignorePattern.Contains('*') || ignorePattern.Contains('?'))
            {
                var matcher = new Matcher();
                matcher.AddInclude(ignorePattern);
                ignoreMatchers.Add(matcher);
            }
            else
            {
                ignoreFileNames.Add(ignorePattern);
            }
        }

        // 先剔除运行时后缀、忽略文件，再进行分组
        var filteredFiles = allJsonFiles.Where(file =>
        {
            var fileName = Path.GetFileName(file);

            // 排除运行时生成的文件
            if (IsRuntimeJsonFile(fileName))
                return false;

            // 排除忽略配置中的精确文件名
            if (ignoreFileNames.Contains(fileName)) return false;

            // 排除忽略配置中的 glob 模式
            if (ignoreMatchers.Any(m => m.Match(fileName).HasMatches)) return false;

            return true;
        }).ToList();

        // 将所有有效文件进行分组
        var jsonFilesGroups = SplitConfigFileNameToGroups(filteredFiles)
            .Where(g => !excludeJsonPrefixs.Contains(g.Key));

        // 遍历所有配置分组
        foreach (var group in jsonFilesGroups)
        {
            // 限制查找的 json 文件组
            var baseFileName = $"{group.Key}.json";
            var envFileName = $"{group.Key}.{envName}.json";

            // 查找默认配置和环境配置
            var files = group
                .Where(u =>
                {
                    var name = Path.GetFileName(u);
                    return string.Equals(name, baseFileName, StringComparison.OrdinalIgnoreCase) || string.Equals(name, envFileName, StringComparison.OrdinalIgnoreCase);
                })
                .OrderBy(u => Path.GetFileName(u).Length);

            // 循环加载
            foreach (var jsonFile in files)
            {
                configurationBuilder.AddJsonFile(jsonFile, optional: jsonFileScanner.Optional, reloadOnChange: jsonFileScanner.ReloadOnChange);
            }
        }
    }

    /// <summary>
    /// 判断是否为运行时 Json 文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns></returns>
    private static bool IsRuntimeJsonFile(string fileName)
    {
        foreach (var suffix in runtimeJsonSuffixs)
        {
            if (fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 排除的配置文件前缀
    /// </summary>
    private static readonly HashSet<string> excludeJsonPrefixs = new(StringComparer.OrdinalIgnoreCase)
    {
        "appsettings", "bundleconfig", "compilerconfig", "ReSharperTestRunner"
    };

    /// <summary>
    /// 排除运行时 Json 后缀
    /// </summary>
    private static readonly string[] runtimeJsonSuffixs =
    [
        "deps.json",
        "runtimeconfig.dev.json",
        "runtimeconfig.prod.json",
        "runtimeconfig.json",
        "staticwebassets.runtime.json",
        "staticwebassets.endpoints.json",
        "nuget.dgspec.json",
        "project.assets.json",
        "MvcTestingAppManifest.json"
    ];

    /// <summary>
    /// 对配置文件名进行分组
    /// </summary>
    /// <param name="configFiles"></param>
    /// <returns></returns>
    private static IEnumerable<IGrouping<string, string>> SplitConfigFileNameToGroups(IEnumerable<string> configFiles)
    {
        // 分组
        return configFiles.GroupBy(Function);

        // 本地函数
        static string Function(string file)
        {
            // 根据 . 分隔
            var fileNameParts = Path.GetFileName(file).Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (fileNameParts.Length == 2) return fileNameParts[0];

            return string.Join('.', fileNameParts.Take(fileNameParts.Length - 2));
        }
    }
}