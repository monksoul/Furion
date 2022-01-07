// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;

namespace Furion.SpecificationDocument
{
    /// <summary>
    /// 规范化结果中间件配置选项
    /// </summary>
    public sealed class SpecificationDocumentConfigureOptions
    {
        /// <summary>
        /// Swagger 配置
        /// </summary>
        public Action<SwaggerOptions> SwaggerConfigure { get; set; }

        /// <summary>
        /// Swagger UI 配置
        /// </summary>
        public Action<SwaggerUIOptions> SwaggerUIConfigure { get; set; }
    }
}