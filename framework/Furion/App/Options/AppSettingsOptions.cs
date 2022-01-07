// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ConfigurableOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Furion
{
    /// <summary>
    /// 应用全局配置
    /// </summary>
    public sealed class AppSettingsOptions : IConfigurableOptions<AppSettingsOptions>
    {
        /// <summary>
        /// 集成 MiniProfiler 组件
        /// </summary>
        public bool? InjectMiniProfiler { get; set; }

        /// <summary>
        /// 是否启用规范化文档
        /// </summary>
        public bool? InjectSpecificationDocument { get; set; }

        /// <summary>
        /// 是否启用引用程序集扫描
        /// </summary>
        public bool? EnabledReferenceAssemblyScan { get; set; }

        /// <summary>
        /// 外部程序集
        /// </summary>
        public string[] ExternalAssemblies { get; set; }

        /// <summary>
        /// 是否打印数据库连接信息到 MiniProfiler 中
        /// </summary>
        public bool? PrintDbConnectionInfo { get; set; }

        /// <summary>
        /// 是否输出原始 Sql 执行日志（ADO.NET）
        /// </summary>
        public bool? OutputOriginalSqlExecuteLog { get; set; }

        /// <summary>
        /// 配置支持的包前缀名
        /// </summary>
        public string[] SupportPackageNamePrefixs { get; set; }

        /// <summary>
        /// 【部署】二级虚拟目录
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// 后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(AppSettingsOptions options, IConfiguration configuration)
        {
            // 非 Web 环境总是 false，如果是生产环境且不配置 InjectMiniProfiler，默认总是false，MiniProfiler 生产环境耗内存
            if (App.WebHostEnvironment == default
                || (App.HostEnvironment.IsProduction() && options.InjectMiniProfiler == null)) options.InjectMiniProfiler = false;
            else options.InjectMiniProfiler ??= true;

            options.InjectSpecificationDocument ??= true;
            options.EnabledReferenceAssemblyScan ??= false;
            options.ExternalAssemblies ??= Array.Empty<string>();
            options.PrintDbConnectionInfo ??= true;
            options.OutputOriginalSqlExecuteLog ??= true;
            options.SupportPackageNamePrefixs ??= Array.Empty<string>();
            options.VirtualPath ??= string.Empty;
        }
    }
}