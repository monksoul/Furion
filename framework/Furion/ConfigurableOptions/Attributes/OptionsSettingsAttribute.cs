// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.ConfigurableOptions
{
    /// <summary>
    /// 选项配置特性
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Class)]
    public sealed class OptionsSettingsAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OptionsSettingsAttribute()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path">appsetting.json 对应键</param>
        public OptionsSettingsAttribute(string path)
        {
            Path = path;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="postConfigureAll">启动所有实例进行后期配置</param>
        public OptionsSettingsAttribute(bool postConfigureAll)
        {
            PostConfigureAll = postConfigureAll;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path">appsetting.json 对应键</param>
        /// <param name="postConfigureAll">启动所有实例进行后期配置</param>
        public OptionsSettingsAttribute(string path, bool postConfigureAll)
        {
            Path = path;
            PostConfigureAll = postConfigureAll;
        }

        /// <summary>
        /// 对应配置文件中的路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 对所有配置实例进行后期配置
        /// </summary>
        public bool PostConfigureAll { get; set; }
    }
}