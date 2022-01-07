// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Furion.ConfigurableOptions
{
    /// <summary>
    /// 应用选项依赖接口
    /// </summary>
    public partial interface IConfigurableOptions { }

    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public partial interface IConfigurableOptions<TOptions> : IConfigurableOptions
        where TOptions : class, IConfigurableOptions
    {
        /// <summary>
        /// 选项后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        void PostConfigure(TOptions options, IConfiguration configuration);
    }

    /// <summary>
    /// 带验证的应用选项依赖接口
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TOptionsValidation"></typeparam>
    public partial interface IConfigurableOptions<TOptions, TOptionsValidation> : IConfigurableOptions<TOptions>
        where TOptions : class, IConfigurableOptions
        where TOptionsValidation : class, IValidateOptions<TOptions>
    {
    }

    /// <summary>
    /// 带监听的应用选项依赖接口
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public partial interface IConfigurableOptionsListener<TOptions> : IConfigurableOptions<TOptions>
        where TOptions : class, IConfigurableOptions
    {
        /// <summary>
        /// 监听
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        void OnListener(TOptions options, IConfiguration configuration);
    }
}