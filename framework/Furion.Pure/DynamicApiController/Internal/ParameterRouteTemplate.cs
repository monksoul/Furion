// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Collections.Generic;

namespace Furion.DynamicApiController
{
    /// <summary>
    /// 参数路由模板
    /// </summary>
    internal class ParameterRouteTemplate
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ParameterRouteTemplate()
        {
            ControllerStartTemplates = new List<string>();
            ControllerEndTemplates = new List<string>();
            ActionStartTemplates = new List<string>();
            ActionEndTemplates = new List<string>();
        }

        /// <summary>
        /// 控制器之前的参数
        /// </summary>
        public IList<string> ControllerStartTemplates { get; set; }

        /// <summary>
        /// 控制器之后的参数
        /// </summary>
        public IList<string> ControllerEndTemplates { get; set; }

        /// <summary>
        /// 行为之前的参数
        /// </summary>
        public IList<string> ActionStartTemplates { get; set; }

        /// <summary>
        /// 行为之后的参数
        /// </summary>
        public IList<string> ActionEndTemplates { get; set; }
    }
}