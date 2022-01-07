// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ConfigurableOptions;
using Furion.Reflection;
using Microsoft.Extensions.Configuration;
using System;

namespace Furion.Localization
{
    /// <summary>
    /// 多语言配置选项
    /// </summary>
    public sealed class LocalizationSettingsOptions : IConfigurableOptions<LocalizationSettingsOptions>
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string ResourcesPath { get; set; }

        /// <summary>
        /// 支持的语言列表
        /// </summary>
        public string[] SupportedCultures { get; set; }

        /// <summary>
        /// 默认的语言
        /// </summary>
        public string DefaultCulture { get; set; }

        /// <summary>
        /// 资源文件名前缀
        /// </summary>
        public string LanguageFilePrefix { get; set; }

        /// <summary>
        /// 资源所在程序集名称
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 选项后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(LocalizationSettingsOptions options, IConfiguration configuration)
        {
            ResourcesPath ??= "Resources";
            SupportedCultures ??= Array.Empty<string>();
            DefaultCulture ??= string.Empty;
            LanguageFilePrefix ??= "Lang";
            AssemblyName ??= Reflect.GetAssemblyName(Reflect.GetEntryAssembly());
        }
    }
}