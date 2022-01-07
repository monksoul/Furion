// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ConfigurableOptions;
using Microsoft.Extensions.Configuration;

namespace Furion.UnifyResult
{
    /// <summary>
    /// 规范化配置选项
    /// </summary>
    public sealed class UnifyResultSettingsOptions : IConfigurableOptions<UnifyResultSettingsOptions>
    {
        /// <summary>
        /// 设置返回 200 状态码列表
        /// <para>默认：401，403，如果设置为 null，则标识所有状态码都返回 200 </para>
        /// </summary>
        public int[] Return200StatusCodes { get; set; }

        /// <summary>
        /// 适配（篡改）Http 状态码（只支持短路状态码，比如 401，403，500 等）
        /// </summary>
        public int[][] AdaptStatusCodes { get; set; }

        /// <summary>
        /// 是否支持 MVC 控制台规范化处理
        /// </summary>
        public bool? SupportMvcController { get; set; }

        /// <summary>
        /// 选项后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(UnifyResultSettingsOptions options, IConfiguration configuration)
        {
            options.Return200StatusCodes ??= new[] { 401, 403 };
            options.SupportMvcController ??= false;
        }
    }
}