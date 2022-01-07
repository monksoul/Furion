// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System.ComponentModel;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 接口参数位置
    /// </summary>
    [SuppressSniffer]
    public enum ApiSeats
    {
        /// <summary>
        /// 控制器之前
        /// </summary>
        [Description("控制器之前")]
        ControllerStart,

        /// <summary>
        /// 控制器之后
        /// </summary>
        [Description("控制器之后")]
        ControllerEnd,

        /// <summary>
        /// 行为之前
        /// </summary>
        [Description("行为之前")]
        ActionStart,

        /// <summary>
        /// 行为之后
        /// </summary>
        [Description("行为之后")]
        ActionEnd
    }
}