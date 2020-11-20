using Fur.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 实体拓展类
    /// </summary>
    [SkipScan]
    public static class IEntityExtensions
    {
        /// <summary>
        /// 获取实体同类（族群）
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <returns>DbSet{TEntity}</returns>
        public static DbSet<TEntity> Ethnics<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().Entities;
        }

        /// <summary>
        /// 新增一条记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理的实体</returns>
        public static EntityEntry<TEntity> Insert<TEntity>(this TEntity entity, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().Insert(entity, ignoreNullValues);
        }

        /// <summary>
        /// 新增一条记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>代理的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertAsync<TEntity>(this TEntity entity, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertAsync(entity, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 新增一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertNow<TEntity>(this TEntity entity, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertNow(entity, ignoreNullValues);
        }

        /// <summary>
        /// 新增一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有提交更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertNow<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertNow(entity, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 新增一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertNowAsync<TEntity>(this TEntity entity, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertNowAsync(entity, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 新增一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有提交更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertNowAsync<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertNowAsync(entity, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> Update<TEntity>(this TEntity entity, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().Update(entity, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateAsync<TEntity>(this TEntity entity, bool? ignoreNullValues = null)
             where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateAsync(entity, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateNow<TEntity>(this TEntity entity, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateNow(entity, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateNow<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
             where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateNow(entity, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateNowAsync<TEntity>(this TEntity entity, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateNowAsync(entity, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateNowAsync<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateNowAsync(entity, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateInclude<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateInclude(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateInclude<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateInclude(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateInclude<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateInclude(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateInclude<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateInclude(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录中的特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeAsync<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeAsync(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中的特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeAsync<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录中的特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeAsync(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中的特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNow(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNow(entity, propertyNames, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNow(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNow(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess)
             where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNow(entity, propertyNames, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNow(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateIncludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateExclude<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExclude(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateExclude<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExclude(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateExclude<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExclude(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateExclude<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExclude(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录并排除属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeAsync<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeAsync(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录并排除属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeAsync<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录并排除属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeAsync(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录并排除属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNow(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNow(entity, propertyNames, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNow(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNow(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess)
             where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNow(entity, propertyNames, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNow(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyNames);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().UpdateExcludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> Delete<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().Delete(entity);
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> DeleteAsync<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().DeleteAsync(entity);
        }

        /// <summary>
        /// 删除一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> DeleteNow<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().DeleteNow(entity);
        }

        /// <summary>
        /// 删除一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns></returns>
        public static EntityEntry<TEntity> DeleteNow<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().DeleteNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 删除一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> DeleteNowAsync<TEntity>(this TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().DeleteNowAsync(entity, cancellationToken);
        }

        /// <summary>
        /// 删除一条记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> DeleteNowAsync<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().DeleteNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdate<TEntity>(this TEntity entity, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdate(entity, ignoreNullValues);
        }

        /// <summary>
        /// 新增或更新一条记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateAsync<TEntity>(this TEntity entity, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateAsync(entity, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条记录并立即执行
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateNow<TEntity>(this TEntity entity, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateNow(entity, ignoreNullValues);
        }

        /// <summary>
        /// 新增或更新一条记录并立即执行
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateNow<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateNow(entity, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 新增或更新一条记录并立即执行
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateNowAsync<TEntity>(this TEntity entity, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateNowAsync(entity, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条记录并立即执行
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateNowAsync<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateNowAsync(entity, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateInclude<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateInclude(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateInclude<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateInclude(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateInclude<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateInclude(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateInclude<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateInclude(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeAsync(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync<TEntity>(this TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateIncludeNow<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNow(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateIncludeNow<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNow(entity, propertyNames, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateIncludeNow<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNow(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateIncludeNow<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNow(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNow(entity, propertyNames, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNow(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateIncludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExclude<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExclude(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExclude<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExclude(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExclude<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExclude(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExclude<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExclude(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeAsync(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync<TEntity>(this TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExcludeNow<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNow(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExcludeNow<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNow(entity, propertyNames, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExcludeNow<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNow(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExcludeNow<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNow(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNow(entity, propertyNames, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNow(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> InsertOrUpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, params string[] propertyNames)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyNames);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyPredicates);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyNames, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyPredicates, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().InsertOrUpdateExcludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 假删除
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public static EntityEntry<TEntity> FakeDelete<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().FakeDelete(entity);
        }

        /// <summary>
        /// 假删除
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public static Task<EntityEntry<TEntity>> FakeDeleteAsync<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().FakeDeleteAsync(entity);
        }

        /// <summary>
        /// 假删除并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public static EntityEntry<TEntity> FakeDeleteNow<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().FakeDeleteNow(entity);
        }

        /// <summary>
        /// 假删除并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns></returns>
        public static EntityEntry<TEntity> FakeDeleteNow<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().FakeDeleteNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 假删除并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns></returns>
        public static Task<EntityEntry<TEntity>> FakeDeleteNowAsync<TEntity>(this TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().FakeDeleteNowAsync(entity, cancellationToken);
        }

        /// <summary>
        /// 假删除并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns></returns>
        public static Task<EntityEntry<TEntity>> FakeDeleteNowAsync<TEntity>(this TEntity entity, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
             where TEntity : class, IPrivateEntity, new()
        {
            return Db.GetRepository<TEntity>().FakeDeleteNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}