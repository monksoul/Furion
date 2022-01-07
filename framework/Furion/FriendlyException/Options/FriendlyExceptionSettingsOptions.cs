// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ConfigurableOptions;
using Microsoft.Extensions.Configuration;

namespace Furion.FriendlyException
{
    /// <summary>
    /// 友好异常配置选项
    /// </summary>
    public sealed class FriendlyExceptionSettingsOptions : IConfigurableOptions<FriendlyExceptionSettingsOptions>
    {
        /// <summary>
        /// 隐藏错误码
        /// </summary>
        public bool? HideErrorCode { get; set; }

        /// <summary>
        /// 默认错误码
        /// </summary>
        public string DefaultErrorCode { get; set; }

        /// <summary>
        /// 默认错误消息
        /// </summary>
        public string DefaultErrorMessage { get; set; }

        /// <summary>
        /// 选项后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(FriendlyExceptionSettingsOptions options, IConfiguration configuration)
        {
            options.HideErrorCode ??= false;
            options.DefaultErrorCode ??= string.Empty;
            options.DefaultErrorMessage ??= "Internal Server Error";
        }
    }
}