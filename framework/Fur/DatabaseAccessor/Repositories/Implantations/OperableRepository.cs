using Fur.LinqBuilder;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 可操作仓储分部类
    /// </summary>
    public partial class EFCoreRepository<TEntity, TDbContextLocator>
        where TEntity : class, IPrivateEntity, new()
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 新增或更新一条记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="checkProperty"></param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdate(TEntity entity, bool? ignoreNullValues = null, Expression<Func<TEntity, object>> checkProperty = null)
        {
            return IsPropertyValueSet(entity, checkProperty) ? Update(entity, ignoreNullValues) : Insert(entity, ignoreNullValues);
        }

        /// <summary>
        /// 新增或更新一条记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="checkProperty"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateAsync(TEntity entity, bool? ignoreNullValues = null, Expression<Func<TEntity, object>> checkProperty = null, CancellationToken cancellationToken = default)
        {
            return IsPropertyValueSet(entity, checkProperty) ? UpdateAsync(entity, ignoreNullValues) : InsertAsync(entity, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条记录并立即执行
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="checkProperty"></param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateNow(TEntity entity, bool? ignoreNullValues = null, Expression<Func<TEntity, object>> checkProperty = null)
        {
            return IsPropertyValueSet(entity, checkProperty) ? UpdateNow(entity, ignoreNullValues) : InsertNow(entity, ignoreNullValues);
        }

        /// <summary>
        /// 新增或更新一条记录并立即执行
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="checkProperty"></param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateNow(TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, Expression<Func<TEntity, object>> checkProperty = null)
        {
            return IsPropertyValueSet(entity, checkProperty) ? UpdateNow(entity, acceptAllChangesOnSuccess, ignoreNullValues) : InsertNow(entity, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 新增或更新一条记录并立即执行
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="checkProperty"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateNowAsync(TEntity entity, bool? ignoreNullValues = null, Expression<Func<TEntity, object>> checkProperty = null, CancellationToken cancellationToken = default)
        {
            return IsPropertyValueSet(entity, checkProperty) ? UpdateNowAsync(entity, ignoreNullValues, cancellationToken) : InsertNowAsync(entity, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条记录并立即执行
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="checkProperty"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateNowAsync(TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, Expression<Func<TEntity, object>> checkProperty = null, CancellationToken cancellationToken = default)
        {
            return IsPropertyValueSet(entity, checkProperty) ? UpdateNowAsync(entity, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateInclude(TEntity entity, params string[] propertyNames)
        {
            return IsKeySet(entity) ? UpdateInclude(entity, propertyNames) : Insert(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateInclude(TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateInclude(entity, propertyPredicates) : Insert(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateInclude(TEntity entity, IEnumerable<string> propertyNames)
        {
            return IsKeySet(entity) ? UpdateInclude(entity, propertyNames) : Insert(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateInclude(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateInclude(entity, propertyPredicates) : Insert(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync(TEntity entity, params string[] propertyNames)
        {
            return IsKeySet(entity) ? UpdateIncludeAsync(entity, propertyNames) : InsertAsync(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync(TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeAsync(entity, propertyNames) : InsertAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync(TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateIncludeAsync(entity, propertyPredicates) : InsertAsync(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeAsync(entity, propertyPredicates) : InsertAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync(TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeAsync(entity, propertyNames) : InsertAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeAsync(entity, propertyPredicates) : InsertAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateIncludeNow(TEntity entity, params string[] propertyNames)
        {
            return IsKeySet(entity) ? UpdateIncludeNow(entity, propertyNames) : InsertNow(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateIncludeNow(TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess)
        {
            return IsKeySet(entity) ? UpdateIncludeNow(entity, propertyNames, acceptAllChangesOnSuccess) : InsertNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateIncludeNow(TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateIncludeNow(entity, propertyPredicates) : InsertNow(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateIncludeNow(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess)
        {
            return IsKeySet(entity) ? UpdateIncludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess) : InsertNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateIncludeNow(TEntity entity, IEnumerable<string> propertyNames)
        {
            return IsKeySet(entity) ? UpdateIncludeNow(entity, propertyNames) : InsertNow(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateIncludeNow(TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess)
        {
            return IsKeySet(entity) ? UpdateIncludeNow(entity, propertyNames, acceptAllChangesOnSuccess) : InsertNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateIncludeNow(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateIncludeNow(entity, propertyPredicates) : InsertNow(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateIncludeNow(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess)
        {
            return IsKeySet(entity) ? UpdateIncludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess) : InsertNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, params string[] propertyNames)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyNames) : InsertNowAsync(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyNames, cancellationToken) : InsertNowAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyPredicates) : InsertNowAsync(entity);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyPredicates, cancellationToken) : InsertNowAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyNames, cancellationToken) : InsertNowAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyPredicates, cancellationToken) : InsertNowAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateIncludeNowAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateIncludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExclude(TEntity entity, params string[] propertyNames)
        {
            return IsKeySet(entity) ? UpdateExclude(entity, propertyNames) : Insert(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExclude(TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateExclude(entity, propertyPredicates) : Insert(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExclude(TEntity entity, IEnumerable<string> propertyNames)
        {
            return IsKeySet(entity) ? UpdateExclude(entity, propertyNames) : Insert(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExclude(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateExclude(entity, propertyPredicates) : Insert(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync(TEntity entity, params string[] propertyNames)
        {
            return IsKeySet(entity) ? UpdateExcludeAsync(entity, propertyNames) : InsertAsync(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync(TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeAsync(entity, propertyNames) : InsertAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync(TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateExcludeAsync(entity, propertyPredicates) : InsertAsync(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeAsync(entity, propertyPredicates) : InsertAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync(TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeAsync(entity, propertyNames) : InsertAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>代理中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeAsync(entity, propertyPredicates) : InsertAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExcludeNow(TEntity entity, params string[] propertyNames)
        {
            return IsKeySet(entity) ? UpdateExcludeNow(entity, propertyNames) : InsertNow(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExcludeNow(TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess)
        {
            return IsKeySet(entity) ? UpdateExcludeNow(entity, propertyNames, acceptAllChangesOnSuccess) : InsertNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExcludeNow(TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateExcludeNow(entity, propertyPredicates) : InsertNow(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExcludeNow(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess)
        {
            return IsKeySet(entity) ? UpdateExcludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess) : InsertNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExcludeNow(TEntity entity, IEnumerable<string> propertyNames)
        {
            return IsKeySet(entity) ? UpdateExcludeNow(entity, propertyNames) : InsertNow(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExcludeNow(TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess)
        {
            return IsKeySet(entity) ? UpdateExcludeNow(entity, propertyNames, acceptAllChangesOnSuccess) : InsertNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExcludeNow(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateExcludeNow(entity, propertyPredicates) : InsertNow(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <returns>数据库中的实体</returns>
        public virtual EntityEntry<TEntity> InsertOrUpdateExcludeNow(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess)
        {
            return IsKeySet(entity) ? UpdateExcludeNow(entity, propertyPredicates, acceptAllChangesOnSuccess) : InsertNow(entity, acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, params string[] propertyNames)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyNames) : InsertNowAsync(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyNames, cancellationToken) : InsertNowAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, params Expression<Func<TEntity, object>>[] propertyPredicates)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyPredicates) : InsertNowAsync(entity);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyPredicates, cancellationToken) : InsertNowAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyNames, cancellationToken) : InsertNowAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyNames, acceptAllChangesOnSuccess, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyPredicates, cancellationToken) : InsertNowAsync(entity, false, cancellationToken);
        }

        /// <summary>
        /// 新增或更新一条排除特定属性记录并立即提交
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public virtual Task<EntityEntry<TEntity>> InsertOrUpdateExcludeNowAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return IsKeySet(entity) ? UpdateExcludeNowAsync(entity, propertyPredicates, acceptAllChangesOnSuccess, cancellationToken) : InsertNowAsync(entity, acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 忽略控制属性
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ignoreNullValues"></param>
        private void IgnoreNullValues(ref TEntity entity, bool? ignoreNullValues = null)
        {
            var isIgnore = ignoreNullValues ?? DynamicDbContext.InsertOrUpdateIgnoreNullValues;
            if (isIgnore == false) return;

            // 获取所有的属性
            var properties = EntityType.GetProperties();
            foreach (var propety in properties)
            {
                var entityProperty = EntityPropertyEntry(entity, propety.Name);
                if (entityProperty.CurrentValue == null)
                {
                    entityProperty.IsModified = false;
                }
            }
        }

        /// <summary>
        /// 判断属性值是否设置
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="checkProperty"></param>
        /// <returns></returns>
        private bool IsPropertyValueSet(TEntity entity, Expression<Func<TEntity, object>> checkProperty = null)
        {
            if (checkProperty == null) return IsKeySet(entity);

            // 获取属性信息
            var entityProperty = EntityPropertyEntry(entity, checkProperty.GetExpressionPropertyName());
            var propertyValue = entityProperty.CurrentValue;

            return !(propertyValue == null || (entityProperty.Metadata.PropertyInfo.PropertyType.IsValueType && (propertyValue.Equals(0) || propertyValue.Equals(Guid.Empty))));
        }
    }
}