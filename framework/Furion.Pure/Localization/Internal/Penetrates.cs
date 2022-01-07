// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.AspNetCore.Builder;

namespace Furion.Localization
{
    /// <summary>
    /// 常量、公共方法配置类
    /// </summary>
    internal static class Penetrates
    {
        /// <summary>
        /// 设置请求多语言对象
        /// </summary>
        /// <param name="requestLocalization"></param>
        /// <param name="localizationSettings"></param>
        internal static void SetRequestLocalization(RequestLocalizationOptions requestLocalization, LocalizationSettingsOptions localizationSettings)
        {
            // 如果设置了默认语言，则取默认语言，否则取第一个
            requestLocalization.SetDefaultCulture(string.IsNullOrWhiteSpace(localizationSettings.DefaultCulture) ? localizationSettings.SupportedCultures[0] : localizationSettings.DefaultCulture)
                   .AddSupportedCultures(localizationSettings.SupportedCultures)
                   .AddSupportedUICultures(localizationSettings.SupportedCultures);

            // 自动根据客户端浏览器的语言实现多语言机制
            requestLocalization.ApplyCurrentCultureToResponseHeaders = true;
        }
    }
}