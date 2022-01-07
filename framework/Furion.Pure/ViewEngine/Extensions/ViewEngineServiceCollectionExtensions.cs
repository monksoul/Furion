// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.ViewEngine;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 视图引擎服务拓展类
    /// </summary>
    [SuppressSniffer]
    public static class ViewEngineServiceCollectionExtensions
    {
        /// <summary>
        /// 添加视图引擎
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddViewEngine(this IServiceCollection services)
        {
            services.AddTransient<IViewEngine, ViewEngine>();
            return services;
        }
    }
}