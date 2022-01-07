// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 动态模型缓存工厂
    /// </summary>
    /// <remarks>主要用来实现数据库分表分库</remarks>
    [SuppressSniffer]
    public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
    {
        /// <summary>
        /// 动态模型缓存Key
        /// </summary>
        private static int cacheKey;

        /// <summary>
        /// 重写构建模型
        /// </summary>
        /// <remarks>动态切换表之后需要调用该方法</remarks>
        public static void RebuildModels()
        {
            Interlocked.Increment(ref cacheKey);
        }

        /// <summary>
        /// 更新模型缓存
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Create(DbContext context)
        {
            return (context.GetType(), cacheKey);
        }
    }
}