// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion;
using Furion.DependencyInjection;
using Furion.Localization;

namespace Microsoft.AspNetCore.Mvc.Localization
{
    /// <summary>
    /// IHtmlLocalizerFactory 拓展类
    /// </summary>
    [SuppressSniffer]
    public static class IHtmlLocalizerFactoryExtensions
    {
        /// <summary>
        /// 创建默认多语言工厂
        /// </summary>
        /// <param name="htmlLocalizerFactory"></param>
        /// <returns></returns>
        public static IHtmlLocalizer Create(this IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            var localizationSettings = App.GetOptions<LocalizationSettingsOptions>();
            return htmlLocalizerFactory.Create(localizationSettings.LanguageFilePrefix, localizationSettings.AssemblyName);
        }
    }
}