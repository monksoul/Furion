// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion;
using Furion.DependencyInjection;
using Furion.SpecificationDocument;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// 规范化文档中间件拓展
    /// </summary>
    [SuppressSniffer]
    public static class SpecificationDocumentApplicationBuilderExtensions
    {
        /// <summary>
        /// 添加规范化文档中间件
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routePrefix"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSpecificationDocuments(this IApplicationBuilder app, string routePrefix = default, Action<SpecificationDocumentConfigureOptions> configure = default)
        {
            // 判断是否启用规范化文档
            if (App.Settings.InjectSpecificationDocument != true) return app;

            // 载入服务配置选项
            var configureOptions = new SpecificationDocumentConfigureOptions();
            configure?.Invoke(configureOptions);

            // 配置 Swagger 全局参数
            app.UseSwagger(options => SpecificationDocumentBuilder.Build(options, configureOptions?.SwaggerConfigure));

            // 配置 Swagger UI 参数
            app.UseSwaggerUI(options => SpecificationDocumentBuilder.BuildUI(options, routePrefix, configureOptions?.SwaggerUIConfigure));

            // 启用 MiniProfiler组件
            if (App.Settings.InjectMiniProfiler == true) app.UseMiniProfiler();

            return app;
        }
    }
}