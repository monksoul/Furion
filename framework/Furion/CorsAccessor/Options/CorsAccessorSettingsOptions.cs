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
using System.ComponentModel.DataAnnotations;

namespace Furion.CorsAccessor
{
    /// <summary>
    /// 跨域配置选项
    /// </summary>
    public sealed class CorsAccessorSettingsOptions : IConfigurableOptions<CorsAccessorSettingsOptions>
    {
        /// <summary>
        /// 策略名称
        /// </summary>
        [Required]
        public string PolicyName { get; set; }

        /// <summary>
        /// 允许来源域名，没有配置则允许所有来源
        /// </summary>
        public string[] WithOrigins { get; set; }

        /// <summary>
        /// 请求表头，没有配置则允许所有表头
        /// </summary>
        public string[] WithHeaders { get; set; }

        /// <summary>
        /// 响应标头
        /// </summary>
        public string[] WithExposedHeaders { get; set; }

        /// <summary>
        /// 设置跨域允许请求谓词，没有配置则允许所有
        /// </summary>
        public string[] WithMethods { get; set; }

        /// <summary>
        /// 跨域请求中的凭据
        /// </summary>
        public bool? AllowCredentials { get; set; }

        /// <summary>
        /// 设置预检过期时间
        /// </summary>
        public int? SetPreflightMaxAge { get; set; }

        /// <summary>
        /// 后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(CorsAccessorSettingsOptions options, IConfiguration configuration)
        {
            PolicyName ??= "App.Cors.Policy";
            WithOrigins ??= Array.Empty<string>();
            AllowCredentials ??= true;
        }
    }
}