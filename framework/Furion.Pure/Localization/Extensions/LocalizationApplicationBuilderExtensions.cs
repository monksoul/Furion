// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// 多语言中间件拓展
    /// </summary>
    [SuppressSniffer]
    public static class LocalizationApplicationBuilderExtensions
    {
        /// <summary>
        /// 配置多语言中间件拓展
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAppLocalization(this IApplicationBuilder app)
        {
            // 获取多语言配置选项
            var localizationSettings = app.ApplicationServices.GetRequiredService<IOptions<LocalizationSettingsOptions>>().Value;

            // 如果没有配置多语言选项，则不注册服务
            if (localizationSettings.SupportedCultures == null || localizationSettings.SupportedCultures.Length == 0) return app;

            var requestLocalization = new RequestLocalizationOptions();
            Penetrates.SetRequestLocalization(requestLocalization, localizationSettings);

            // 设置多语言请求中间件
            app.UseRequestLocalization(requestLocalization);

            return app;
        }
    }
}