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
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        /// <returns></returns>
        public static IQueryable<T> AsTemporalAll<T>(this DbSet<T> dbSet) where T : class
        {
            return dbSet.AsTemporal("ALL");
        }

        /// <summary>
        /// 返回一个包含实际值（当前）的行的表。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static IQueryable<T> AsTemporalAsOf<T>(this DbSet<T> dbSet, DateTime date) where T : class
        {
            return dbSet.AsTemporal("AS OF {0}", date.ToUniversalTime());
        }

        /// <summary>
        /// 返回一个表，其中具有在指定的时间范围，无论它们是否在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IQueryable<T> AsTemporalFrom<T>(this DbSet<T> dbSet, DateTime startDate, DateTime endDate) where T : class
        {
            return dbSet.AsTemporal("FROM {0} TO {1}", startDate.ToUniversalTime(), endDate.ToUniversalTime());
        }

        /// <summary>
        /// 返回一个表，其中具有在指定的时间范围，无论它们是否在，但是结束时间有边界值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IQueryable<T> AsTemporalBetween<T>(this DbSet<T> dbSet, DateTime startDate, DateTime endDate) where T : class
        {
            return dbSet.AsTemporal("BETWEEN {0} AND {1}", startDate.ToUniversalTime(), endDate.ToUniversalTime());
        }

        /// <summary>
        /// 返回一个表，该表包含已打开和关闭的所有行版本的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IQueryable<T> AsTemporalContained<T>(this DbSet<T> dbSet, DateTime startDate, DateTime endDate) where T : class
        {
            return dbSet.AsTemporal("CONTAINED IN ({0}, {1})", startDate.ToUniversalTime(), endDate.ToUniversalTime());
        }

        /// <summary>
        /// 创建时态表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="temporalCriteria"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static IQueryable<T> AsTemporal<T>(this DbSet<T> dbSet, string temporalCriteria, params object[] arguments) where T : class
        {
            // 获取当前数据库上下文
            var table = dbSet
                .GetService<ICurrentDbContext>()
                .GetTableName<T>();

            var selectSql = $"SELECT * FROM {table}";
            var sql = FormattableStringFactory.Create(selectSql + " FOR SYSTEM_TIME " + temporalCriteria, arguments);
            return dbSet.FromSqlInterpolated(sql);
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        private static string GetTableName<T>(this ICurrentDbContext dbContext) where T : class
        {
            var entityType = dbContext.Context.Model.FindEntityType(typeof(T));
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