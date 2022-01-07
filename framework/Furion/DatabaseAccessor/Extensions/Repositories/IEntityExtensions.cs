// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor.Extensions
{
    /// <summary>
    /// 实体拓展类
    /// </summary>
    [SuppressSniffer]
    public static class IEntityExtensions
    {
        /// <summary>
        /// 设置实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static EntityExecutePart<TEntity> SetEntity<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity);
        }

        /// <summary>
        /// 设置数据库执行作用域
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static EntityExecutePart<TEntity> SetContextScoped<TEntity>(this TEntity entity, IServiceProvider serviceProvider)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).SetContextScoped(serviceProvider);
        }

        /// <summary>
        /// 设置数据库上下文定位器
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TDbContextLocator"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static EntityExecutePart<TEntity> Change<TEntity, TDbContextLocator>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
            where TDbContextLocator : class, IDbContextLocator
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).Change<TDbContextLocator>();
        }

        /// <summary>
        /// 设置数据库上下文定位器
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="dbContextLocator"></param>
        /// <returns></returns>
        public static EntityExecutePart<TEntity> Change<TEntity>(this TEntity entity, Type dbContextLocator)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).Change(dbContextLocator);
        }

        /// <summary>
        /// 获取实体同类（族群）
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <returns>DbSet{TEntity}</returns>
        public static DbSet<TEntity> Ethnics<TEntity>(this TEntity entity)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).Ethnics();
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).Insert(ignoreNullValues);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).InsertAsync(ignoreNullValues, cancellationToken);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).InsertNow(ignoreNullValues);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).InsertNow(acceptAllChangesOnSuccess, ignoreNullValues);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).InsertNowAsync(ignoreNullValues, cancellationToken);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).InsertNowAsync(acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).Update(ignoreNullValues);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateAsync(ignoreNullValues);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateNow(ignoreNullValues);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateNow(acceptAllChangesOnSuccess, ignoreNullValues);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateNowAsync(ignoreNullValues, cancellationToken);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateNowAsync(acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateInclude<TEntity>(this TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateInclude(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateInclude<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateInclude(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateInclude<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateInclude(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateInclude<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateInclude(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeAsync<TEntity>(this TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeAsync(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeAsync(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeAsync(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeAsync(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNow(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNow(propertyNames, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNow(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNow(propertyPredicates, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNow(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
             where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNow(propertyNames, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNow(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateIncludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNow(propertyPredicates, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNowAsync(propertyNames, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNowAsync(propertyNames, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNowAsync(propertyPredicates, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNowAsync(propertyPredicates, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNowAsync(propertyNames, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNowAsync(propertyNames, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNowAsync(propertyPredicates, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中的特定属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateIncludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateIncludeNowAsync(propertyPredicates, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateExclude<TEntity>(this TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExclude(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateExclude<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExclude(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateExclude<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExclude(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录中特定属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static EntityEntry<TEntity> UpdateExclude<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExclude(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeAsync<TEntity>(this TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeAsync(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeAsync(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeAsync(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>代理中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeAsync(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNow(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNow(propertyNames, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNow(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNow(propertyPredicates, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNow(propertyNames, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
             where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNow(propertyNames, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNow(propertyPredicates, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <returns>数据库中的实体</returns>
        public static EntityEntry<TEntity> UpdateExcludeNow<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNow(propertyPredicates, acceptAllChangesOnSuccess, ignoreNullValues);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNowAsync(propertyNames, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNowAsync(propertyNames, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNowAsync(propertyPredicates, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNowAsync(propertyPredicates, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNowAsync(propertyNames, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyNames">属性名</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNowAsync(propertyNames, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNowAsync(propertyPredicates, ignoreNullValues, cancellationToken);
        }

        /// <summary>
        /// 更新一条记录并排除属性并立即提交
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicates">属性表达式</param>
        /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
        /// <param name="ignoreNullValues"></param>
        /// <param name="cancellationToken">取消异步令牌</param>
        /// <returns>数据库中的实体</returns>
        public static Task<EntityEntry<TEntity>> UpdateExcludeNowAsync<TEntity>(this TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
            where TEntity : class, IPrivateEntity, new()
        {
            return new EntityExecutePart<TEntity>().SetEntity(entity).UpdateExcludeNowAsync(propertyPredicates, acceptAllChangesOnSuccess, ignoreNullValues, cancellationToken);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).Delete();
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).DeleteAsync();
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).DeleteNow();
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).DeleteNow(acceptAllChangesOnSuccess);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).DeleteNowAsync(cancellationToken);
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
            return new EntityExecutePart<TEntity>().SetEntity(entity).DeleteNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}