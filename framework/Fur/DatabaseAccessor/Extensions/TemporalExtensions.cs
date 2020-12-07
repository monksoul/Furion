using Fur.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 时态查询拓展
    /// </summary>
    [SkipScan]
    public static class TemporalExtensions
    {
        /// <summary>
        /// 返回属于当前表和历史记录表的行的联合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbSet"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AsTemporalAll<TEntity>(this DbSet<TEntity> dbSet)
            where TEntity : class, new()
        {
            return dbSet.AsTemporal("ALL");
        }

        /// <summary>
        /// 返回一个包含实际值（当前）的行的表。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AsTemporalAsOf<TEntity>(this DbSet<TEntity> dbSet, DateTime date)
            where TEntity : class, new()
        {
            return dbSet.AsTemporal("AS OF {0}", date.ToUniversalTime());
        }

        /// <summary>
        /// 返回一个表，其中具有在指定的时间范围，无论它们是否在
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AsTemporalFrom<TEntity>(this DbSet<TEntity> dbSet, DateTime startDate, DateTime endDate)
            where TEntity : class, new()
        {
            return dbSet.AsTemporal("FROM {0} TO {1}", startDate.ToUniversalTime(), endDate.ToUniversalTime());
        }

        /// <summary>
        /// 返回一个表，其中具有在指定的时间范围，无论它们是否在，但是结束时间有边界值
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AsTemporalBetween<TEntity>(this DbSet<TEntity> dbSet, DateTime startDate, DateTime endDate)
            where TEntity : class, new()
        {
            return dbSet.AsTemporal("BETWEEN {0} AND {1}", startDate.ToUniversalTime(), endDate.ToUniversalTime());
        }

        /// <summary>
        /// 返回一个表，该表包含已打开和关闭的所有行版本的值
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AsTemporalContained<TEntity>(this DbSet<TEntity> dbSet, DateTime startDate, DateTime endDate)
            where TEntity : class, new()
        {
            return dbSet.AsTemporal("CONTAINED IN ({0}, {1})", startDate.ToUniversalTime(), endDate.ToUniversalTime());
        }

        /// <summary>
        /// 创建时态表
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="temporalCriteria"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static IQueryable<TEntity> AsTemporal<TEntity>(this DbSet<TEntity> dbSet, string temporalCriteria, params object[] arguments)
            where TEntity : class, new()
        {
            // 获取当前数据库上下文
            var table = dbSet
                .GetService<ICurrentDbContext>()
                .GetTableName<TEntity>();

            var selectSql = $"SELECT * FROM {table}";
            var sql = FormattableStringFactory.Create(selectSql + " FOR SYSTEM_TIME " + temporalCriteria, arguments);
            return dbSet.FromSqlInterpolated(sql);
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        private static string GetTableName<TEntity>(this ICurrentDbContext dbContext)
            where TEntity : class, new()
        {
            var entityType = dbContext.Context.Model.FindEntityType(typeof(TEntity));
            var schema = entityType.GetSchema().GetSqlSafeName();
            var table = entityType.GetTableName().GetSqlSafeName();

            return string.IsNullOrEmpty(schema)
                ? table
                : string.Join(".", schema, table);
        }

        /// <summary>
        /// 获取 Sql 安全名
        /// </summary>
        /// <param name="sqlName"></param>
        /// <returns></returns>
        private static string GetSqlSafeName(this string sqlName)
        {
            return string.IsNullOrEmpty(sqlName)
                ? string.Empty
                : $"[{sqlName}]";
        }
    }
}