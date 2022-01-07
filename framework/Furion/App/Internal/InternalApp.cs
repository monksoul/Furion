// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Furion
{
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
        /// <param name="builder"></param>
        internal static void ConfigureApplication(IWebHostBuilder builder)
        {
            // 自动装载配置
            builder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
            {
                // 存储环境对象
                HostEnvironment = WebHostEnvironment = hostContext.HostingEnvironment;

                // 加载配置
                AddJsonFiles(configurationBuilder, hostContext.HostingEnvironment);
            });

            // 应用初始化服务
            builder.ConfigureServices((hostContext, services) =>
            {
                // 存储配置对象
                Configuration = hostContext.Configuration;

                // 存储服务提供器
                InternalServices = services;

                // 注册 Startup 过滤器
                services.AddTransient<IStartupFilter, StartupFilter>();

                // 注册 HttpContextAccessor 服务
                services.AddHttpContextAccessor();

                // 初始化应用服务
                services.AddApp();
            });
        }

        /// <summary>
        /// 配置 Furion 框架（非 Web）
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="autoRegisterBackgroundService"></param>
        internal static void ConfigureApplication(IHostBuilder builder, bool autoRegisterBackgroundService = true)
        {
            builder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
            {
                // 存储环境对象
                HostEnvironment = hostContext.HostingEnvironment;

                // 加载配置
                AddJsonFiles(configurationBuilder, hostContext.HostingEnvironment);
            });

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
                services.AddApp();

                // 自动注册 BackgroundService
                if (autoRegisterBackgroundService) services.AddAppHostedService();
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
            var configuration = configurationBuilder.Build();

            // 获取程序执行目录
            var executeDirectory = AppContext.BaseDirectory;

            // 获取自定义配置扫描目录
            var configurationScanDirectories = (configuration.GetSection("ConfigurationScanDirectories")
                    .Get<string[]>()
                ?? Array.Empty<string>()).Select(u => Path.Combine(executeDirectory, u));

            // 扫描执行目录及自定义配置目录下的 *.json 文件
            var jsonFiles = new[] { executeDirectory }.Concat(configurationScanDirectories)
                               .SelectMany(u =>
                                    Directory.GetFiles(u, "*.json", SearchOption.TopDirectoryOnly));

            // 如果没有配置文件，中止执行
            if (!jsonFiles.Any()) return;

            // 获取环境变量名，如果没找到，则读取 NETCORE_ENVIRONMENT 环境变量信息识别（用于非 Web 环境）
            var envName = hostEnvironment?.EnvironmentName ?? Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Unknown";

            // 读取忽略的配置文件
            var ignoreConfigurationFiles = configuration.GetSection("IgnoreConfigurationFiles")
                    .Get<string[]>()
                ?? Array.Empty<string>();

            // 处理控制台应用程序
            var _excludeJsonPrefixs = hostEnvironment == default ? excludeJsonPrefixs.Where(u => !u.Equals("appsettings")) : excludeJsonPrefixs;

            // 将所有文件进行分组
            var jsonFilesGroups = SplitConfigFileNameToGroups(jsonFiles)
                                                                    .Where(u => !_excludeJsonPrefixs.Contains(u.Key, StringComparer.OrdinalIgnoreCase) && !u.Any(c => runtimeJsonSuffixs.Any(z => c.EndsWith(z, StringComparison.OrdinalIgnoreCase)) || ignoreConfigurationFiles.Contains(Path.GetFileName(c), StringComparer.OrdinalIgnoreCase)));

            // 遍历所有配置分组
            foreach (var group in jsonFilesGroups)
            {
                // 限制查找的 json 文件组
                var limitFileNames = new[] { $"{group.Key}.json", $"{group.Key}.{envName}.json" };

                // 查找默认配置和环境配置
                var files = group.Where(u => limitFileNames.Contains(Path.GetFileName(u), StringComparer.OrdinalIgnoreCase))
                                                 .OrderBy(u => Path.GetFileName(u).Length);

                // 循环加载
                foreach (var jsonFile in files)
                {
                    configurationBuilder.AddJsonFile(jsonFile, optional: true, reloadOnChange: true);
                }
            }
        }

        /// <summary>
        /// 排序的配置文件前缀
        /// </summary>
        private static readonly string[] excludeJsonPrefixs = new[] { "appsettings", "bundleconfig", "compilerconfig" };

        /// <summary>
        /// 排除运行时 Json 后缀
        /// </summary>
        private static readonly string[] runtimeJsonSuffixs = new[]
        {
            "deps.json",
            "runtimeconfig.dev.json",
            "runtimeconfig.prod.json",
            "runtimeconfig.json"
        };

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
}