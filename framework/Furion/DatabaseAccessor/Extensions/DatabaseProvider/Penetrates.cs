// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 常量、公共方法配置类
    /// </summary>
    internal static class Penetrates
    {
        /// <summary>
        /// 数据库上下文描述器
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, Type> DbContextDescriptors;

        /// <summary>
        /// 构造函数
        /// </summary>
        static Penetrates()
        {
            DbContextDescriptors = new ConcurrentDictionary<Type, Type>();
        }

        /// <summary>
        /// 配置 SqlServer 数据库上下文
        /// </summary>
        /// <param name="optionBuilder">数据库上下文选项构建器</param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        internal static Action<IServiceProvider, DbContextOptionsBuilder> ConfigureDbContext(Action<DbContextOptionsBuilder> optionBuilder, params IInterceptor[] interceptors)
        {
            return (serviceProvider, options) =>
            {
                // 只有开发环境开启
                if (App.HostEnvironment?.IsDevelopment() ?? false)
                {
                    options/*.UseLazyLoadingProxies()*/
                             .EnableDetailedErrors()
                             .EnableSensitiveDataLogging();
                }

                optionBuilder.Invoke(options);

                // 添加拦截器
                AddInterceptors(interceptors, options);
            };
        }

        /// <summary>
        /// 检查数据库上下文是否绑定
        /// </summary>
        /// <param name="dbContextLocatorType"></param>
        /// <param name="dbContextType"></param>
        /// <returns></returns>
        internal static void CheckDbContextLocator(Type dbContextLocatorType, out Type dbContextType)
        {
            if (!DbContextDescriptors.TryGetValue(dbContextLocatorType, out dbContextType)) throw new InvalidCastException($" The dbcontext locator `{dbContextLocatorType.Name}` is not bind.");
        }

        /// <summary>
        /// 数据库数据库拦截器
        /// </summary>
        /// <param name="interceptors">拦截器</param>
        /// <param name="options"></param>
        private static void AddInterceptors(IInterceptor[] interceptors, DbContextOptionsBuilder options)
        {
            // 添加拦截器
            var interceptorList = DbProvider.GetDefaultInterceptors();

            if (interceptors != null || interceptors.Length > 0)
            {
                interceptorList.AddRange(interceptors);
            }
            options.AddInterceptors(interceptorList.ToArray());
        }
    }
}