// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ConfigurableOptions;
using Microsoft.Extensions.Configuration;
using System;

namespace Furion.DependencyInjection
{
    /// <summary>
    /// 依赖注入配置选项
    /// </summary>
    public sealed class DependencyInjectionSettingsOptions : IConfigurableOptions<DependencyInjectionSettingsOptions>
    {
        /// <summary>
        /// 外部注册定义
        /// </summary>
        public ExternalService[] Definitions { get; set; }

        /// <summary>
        /// 后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(DependencyInjectionSettingsOptions options, IConfiguration configuration)
        {
            options.Definitions ??= Array.Empty<ExternalService>();
        }
    }
}