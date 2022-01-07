// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Furion.DatabaseAccessor.Extensions
{
    /// <summary>
    /// 多租户数据库上下文拓展
    /// </summary>
    [SuppressSniffer]
    public static class MultiTenantDbContextExtensions
    {
        /// <summary>
        /// 刷新多租户缓存
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RefreshTenantCache(this DbContext dbContext)
        {
            _ = dbContext;

            // 判断 HttpContext 是否存在
            var httpContext = App.HttpContext;
            if (httpContext == null) return;

            // 获取主机地址
            var host = httpContext.Request.Host.Value;

            // 获取服务提供器
            var serviceProvider = httpContext.RequestServices;

            // 缓存的 Key
            var tenantCachedKey = $"MULTI_TENANT:{host}";

            // 从内存缓存中移除多租户信息
            var distributedCache = serviceProvider.GetService<IDistributedCache>();
            distributedCache.Remove(tenantCachedKey);
        }
    }
}