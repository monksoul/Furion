using Fur.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 动态模型缓存工厂
    /// </summary>
    /// <remarks>主要用来实现数据库分表分库</remarks>
    [SkipScan]
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