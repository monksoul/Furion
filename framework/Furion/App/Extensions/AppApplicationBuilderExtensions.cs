// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion;
using Furion.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// 应用中间件拓展类（由框架内部调用）
    /// </summary>
    [SuppressSniffer]
    public static class AppApplicationBuilderExtensions
    {
        /// <summary>
        /// 注入基础中间件（带Swagger）
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routePrefix">空字符串将为首页</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInject(this IApplicationBuilder app, string routePrefix = default, Action<InjectConfigureOptions> configure = null)
        {
            // 载入中间件配置选项
            var configureOptions = new InjectConfigureOptions();
            configure?.Invoke(configureOptions);

            app.UseSpecificationDocuments(routePrefix, configureOptions?.SpecificationDocumentConfigure);

            return app;
        }

        /// <summary>
        /// 注入基础中间件
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseInjectBase(this IApplicationBuilder app)
        {
            return app;
        }

        /// <summary>
        /// 添加应用中间件
        /// </summary>
        /// <param name="app">应用构建器</param>
        /// <param name="configure">应用配置</param>
        /// <returns>应用构建器</returns>
        internal static IApplicationBuilder UseApp(this IApplicationBuilder app, Action<IApplicationBuilder> configure = null)
        {
            // 调用自定义服务
            configure?.Invoke(app);
            return app;
        }
    }
}