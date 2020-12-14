using Fur.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fur
{
    /// <summary>
    /// 内部 App 副本
    /// </summary>
    [SkipScan]
    internal static class InternalApp
    {
        /// <summary>
        /// 应用服务
        /// </summary>
        internal static IServiceCollection InternalServices;

        /// <summary>
        /// 全局配置构建器
        /// </summary>
        internal static IConfigurationBuilder ConfigurationBuilder;

        /// <summary>
        /// 添加配置文件
        /// </summary>
        /// <param name="config"></param>
        /// <param name="env"></param>
        internal static void AddConfigureFiles(IConfigurationBuilder config, IHostEnvironment env)
        {
            // 读取忽略的配置文件
            var ignoreConfigurationFiles = config.Build()
                .GetSection("IgnoreConfigurationFiles").Get<string[]>()
                ?? Array.Empty<string>();

            // 加载配置
            AutoAddJsonFiles(config, env, ignoreConfigurationFiles);
            AutoAddXmlFiles(config, env, ignoreConfigurationFiles);

            // 存储配置
            ConfigurationBuilder = config;
        }

        /// <summary>
        /// 自动加载自定义 .json 配置文件
        /// </summary>
        /// <param name="config"></param>
        /// <param name="env"></param>
        /// <param name="ignoreConfigurationFiles"></param>
        private static void AutoAddJsonFiles(IConfigurationBuilder config, IHostEnvironment env, string[] ignoreConfigurationFiles)
        {
            // 获取程序目录下的所有配置文件
            var jsonFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.json", SearchOption.TopDirectoryOnly)
                .Union(
                    Directory.GetFiles(Directory.GetCurrentDirectory(), "*.json", SearchOption.TopDirectoryOnly)
                )
                .Where(u => !excludeJsons.Contains(Path.GetFileName(u)) && !ignoreConfigurationFiles.Contains(Path.GetFileName(u)) && !runtimeJsonSuffixs.Any(j => u.EndsWith(j)));

            if (!jsonFiles.Any()) return;

            // 获取环境变量名
            var envName = env.EnvironmentName;
            var envFiles = new List<string>();

            // 自动加载配置文件
            foreach (var jsonFile in jsonFiles)
            {
                // 处理带环境的配置文件
                if (Path.GetFileNameWithoutExtension(jsonFile).EndsWith($".{envName}"))
                {
                    envFiles.Add(jsonFile);
                    continue;
                }

                config.AddJsonFile(jsonFile, optional: true, reloadOnChange: true);
            }

            // 配置带环境的配置文件
            envFiles.ForEach(u => config.AddJsonFile(u, optional: true, reloadOnChange: true));
        }

        /// <summary>
        /// 自动加载自定义 .xml 配置文件
        /// </summary>
        /// <param name="config"></param>
        /// <param name="env"></param>
        /// <param name="ignoreConfigurationFiles"></param>
        private static void AutoAddXmlFiles(IConfigurationBuilder config, IHostEnvironment env, string[] ignoreConfigurationFiles)
        {
            // 获取程序目录下的所有配置文件，必须以 .config.xml 结尾
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
                .Union(
                    Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml", SearchOption.TopDirectoryOnly)
                )
                .Where(u => !ignoreConfigurationFiles.Contains(Path.GetFileName(u)) && u.EndsWith(".config.xml", StringComparison.OrdinalIgnoreCase));

            if (!xmlFiles.Any()) return;

            // 获取环境变量名
            var envName = env.EnvironmentName;
            var envFiles = new List<string>();

            // 自动加载配置文件
            foreach (var xmlFile in xmlFiles)
            {
                // 处理带环境的配置文件
                if (Path.GetFileNameWithoutExtension(xmlFile).EndsWith($".{envName}.config"))
                {
                    envFiles.Add(xmlFile);
                    continue;
                }

                config.AddXmlFile(xmlFile, optional: true, reloadOnChange: true);
            }

            // 配置带环境的配置文件
            envFiles.ForEach(u => config.AddXmlFile(u, optional: true, reloadOnChange: true));
        }

        /// <summary>
        /// 默认排除配置项
        /// </summary>
        private static readonly string[] excludeJsons = new[] {
            "appsettings.json",
            "appsettings.Development.json",
            "appsettings.Production.json",
            "bundleconfig.json",
            "bundleconfig.Development.json",
            "bundleconfig.Production.json",
            "compilerconfig.json",
            "compilerconfig.Development.json",
            "compilerconfig.Production.json"
        };

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
    }
}