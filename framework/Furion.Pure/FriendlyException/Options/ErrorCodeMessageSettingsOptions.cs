// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ConfigurableOptions;

namespace Furion.FriendlyException
{
    /// <summary>
    /// 异常配置选项，最优的方式是采用后期配置，也就是所有异常状态码先不设置（推荐）
    /// </summary>
    public sealed class ErrorCodeMessageSettingsOptions : IConfigurableOptions
    {
        /// <summary>
        /// 异常状态码配置列表
        /// </summary>
        public object[][] Definitions { get; set; }
    }
}