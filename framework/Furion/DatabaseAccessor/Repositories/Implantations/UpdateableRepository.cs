// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Text;

namespace Furion.DatabaseAccessor;

/// <summary>
/// 可更新仓储分部类
/// </summary>
public partial class PrivateRepository<TEntity>
    where TEntity : class, IPrivateEntity, new()
{
    /// <summary>
    /// 分表更新一条记录
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="entity"></param>
    /// <param name="includePropertyNames"></param>
    /// <param name="excludePropertyNames"></param>
    public virtual void UpdateFromSegments(Func<string, IEnumerable<string>> tableNamesAction, TEntity entity, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        GenerateUpdateSQL(tableNamesAction, entity, out var stringBuilder, out var parameters, includePropertyNames, excludePropertyNames);

        Database.ExecuteSqlRaw(stringBuilder.ToString(), parameters.ToArray());
    }

    /// <summary>
    /// 分表更新一条记录
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="entity"></param>
    /// <param name="includePropertyNames"></param>
    /// <param name="excludePropertyNames"></param>
    /// <returns></returns>
    public virtual async Task UpdateFromSegmentsAsync(Func<string, IEnumerable<string>> tableNamesAction, TEntity entity, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        GenerateUpdateSQL(tableNamesAction, entity, out var stringBuilder, out var parameters, includePropertyNames, excludePropertyNames);

        await Database.ExecuteSqlRawAsync(stringBuilder.ToString(), parameters.ToArray());
    }

    /// <summary>
    /// 更新一条记录
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> Update(TEntity entity, bool? ignoreNullValues = null)
    {
        if (Entities.Local.All(e => e != entity))
        {
            Entities.Attach(entity);

            var entityEntry = Entities.Update(entity);

            // 忽略空值
            IgnoreNullValues(ref entity, ignoreNullValues);

            return entityEntry;
        }

        return Entry(entity);
    }

    /// <summary>
    /// 更新多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual void Update(params TEntity[] entities)
    {
        Entities.UpdateRange(entities);
    }

    /// <summary>
    /// 更新多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    public virtual void Update(IEnumerable<TEntity> entities, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        // 包含和排除参数不能同时设置
        if (includePropertyNames is { Length: > 0 } && excludePropertyNames is { Length: > 0 })
        {
            throw new ArgumentException($"The parameters `{nameof(includePropertyNames)}` and `{nameof(excludePropertyNames)}` cannot coexist.");
        }

        Entities.UpdateRange(entities);

        // 处理包含或排除属性
        foreach (var entry in Context.ChangeTracker.Entries<TEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                // 处理包含的属性
                if (includePropertyNames is { Length: > 0 })
                {
                    foreach (var propEntry in entry.Properties)
                    {
                        if (!includePropertyNames.Contains(propEntry.Metadata.Name))
                        {
                            propEntry.IsModified = false;
                        }
                        else
                        {
                            propEntry.IsModified = true;
                        }
                    }
                }
                // 处理排除的属性
                else if (excludePropertyNames is { Length: > 0 })
                {
                    foreach (var propEntry in entry.Properties)
                    {
                        if (excludePropertyNames.Contains(propEntry.Metadata.Name))
                        {
                            propEntry.IsModified = false;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新一条记录
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateAsync(TEntity entity, bool? ignoreNullValues = null)
    {
        return Task.FromResult(Update(entity, ignoreNullValues));
    }

    /// <summary>
    /// 更新多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    public virtual Task UpdateAsync(params TEntity[] entities)
    {
        Update(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    /// <returns>Task</returns>
    public virtual Task UpdateAsync(IEnumerable<TEntity> entities, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        Update(entities, includePropertyNames, excludePropertyNames);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateNow(TEntity entity, bool? ignoreNullValues = null)
    {
        var entityEntry = Update(entity, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateNow(TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = Update(entity, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual int UpdateNow(params TEntity[] entities)
    {
        Update(entities);
        return SaveNow();
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    public virtual int UpdateNow(TEntity[] entities, bool acceptAllChangesOnSuccess, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        Update(entities, includePropertyNames, excludePropertyNames);
        return SaveNow(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    public virtual int UpdateNow(IEnumerable<TEntity> entities, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        Update(entities, includePropertyNames, excludePropertyNames);
        return SaveNow();
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    public virtual int UpdateNow(IEnumerable<TEntity> entities, bool acceptAllChangesOnSuccess, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        Update(entities, includePropertyNames, excludePropertyNames);
        return SaveNow(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 更新一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateNowAsync(TEntity entity, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateAsync(entity, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateNowAsync(TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateAsync(entity, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    public virtual async Task<int> UpdateNowAsync(params TEntity[] entities)
    {
        await UpdateAsync(entities);
        return await SaveNowAsync();
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    /// <returns></returns>
    public virtual async Task<int> UpdateNowAsync(TEntity[] entities, CancellationToken cancellationToken = default, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        await UpdateAsync(entities, includePropertyNames, excludePropertyNames);
        return await SaveNowAsync(cancellationToken);
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    /// <returns>Task</returns>
    public virtual async Task<int> UpdateNowAsync(TEntity[] entities, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        await UpdateAsync(entities, includePropertyNames, excludePropertyNames);
        return await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    /// <returns>Task</returns>
    public virtual async Task<int> UpdateNowAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        await UpdateAsync(entities, includePropertyNames, excludePropertyNames);
        return await SaveNowAsync(cancellationToken);
    }

    /// <summary>
    /// 更新多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <param name="includePropertyNames">包含属性</param>
    /// <param name="excludePropertyNames">排除属性</param>
    /// <returns>Task</returns>
    public virtual async Task<int> UpdateNowAsync(IEnumerable<TEntity> entities, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default, string[] includePropertyNames = null, string[] excludePropertyNames = null)
    {
        await UpdateAsync(entities, includePropertyNames, excludePropertyNames);
        return await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 更新一条记录中特定属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateInclude(TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
    {
        var entityEntry = ChangeEntityState(entity, EntityState.Detached);
        foreach (var propertyName in propertyNames)
        {
            EntityPropertyEntry(entity, propertyName).IsModified = true;
        }

        // 忽略空值
        IgnoreNullValues(ref entity, ignoreNullValues);

        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中特定属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateInclude(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
    {
        // 判断是非参数只有一个，且是一个匿名类型
        if (propertyPredicates?.Length == 1 && propertyPredicates[0].Body is NewExpression newExpression)
        {
            var propertyNames = newExpression.Members.Select(u => u.Name);
            return UpdateInclude(entity, propertyNames, ignoreNullValues);
        }
        else
        {
            var entityEntry = ChangeEntityState(entity, EntityState.Detached);
            foreach (var propertyPredicate in propertyPredicates)
            {
                EntityPropertyEntry(entity, propertyPredicate).IsModified = true;
            }

            // 忽略空值
            IgnoreNullValues(ref entity, ignoreNullValues);

            return entityEntry;
        }
    }

    /// <summary>
    /// 更新一条记录中特定属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateInclude(TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
    {
        return UpdateInclude(entity, propertyNames.ToArray(), ignoreNullValues);
    }

    /// <summary>
    /// 更新一条记录中特定属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateInclude(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
    {
        return UpdateInclude(entity, propertyPredicates.ToArray(), ignoreNullValues);
    }

    /// <summary>
    /// 更新一条记录中的特定属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateIncludeAsync(TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
    {
        return Task.FromResult(UpdateInclude(entity, propertyNames, ignoreNullValues));
    }

    /// <summary>
    /// 更新一条记录中的特定属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateIncludeAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
    {
        return Task.FromResult(UpdateInclude(entity, propertyPredicates, ignoreNullValues));
    }

    /// <summary>
    /// 更新一条记录中的特定属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateIncludeAsync(TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
    {
        return Task.FromResult(UpdateInclude(entity, propertyNames, ignoreNullValues));
    }

    /// <summary>
    /// 更新一条记录中的特定属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateIncludeAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
    {
        return Task.FromResult(UpdateInclude(entity, propertyPredicates, ignoreNullValues));
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateIncludeNow(TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateInclude(entity, propertyNames, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateIncludeNow(TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateInclude(entity, propertyNames, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中实体</returns>
    public virtual EntityEntry<TEntity> UpdateIncludeNow(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateInclude(entity, propertyPredicates, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateIncludeNow(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateInclude(entity, propertyPredicates, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateIncludeNow(TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateInclude(entity, propertyNames, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateIncludeNow(TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateInclude(entity, propertyNames, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateIncludeNow(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateInclude(entity, propertyPredicates, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateIncludeNow(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateInclude(entity, propertyPredicates, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateIncludeNowAsync(TEntity entity, string[] propertyNames, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateIncludeAsync(entity, propertyNames, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateIncludeNowAsync(TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateIncludeAsync(entity, propertyNames, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateIncludeNowAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateIncludeAsync(entity, propertyPredicates, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateIncludeNowAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateIncludeAsync(entity, propertyPredicates, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateIncludeNowAsync(TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateIncludeAsync(entity, propertyNames, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateIncludeNowAsync(TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateIncludeAsync(entity, propertyNames, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateIncludeNowAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateIncludeAsync(entity, propertyPredicates, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录中的特定属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateIncludeNowAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateIncludeAsync(entity, propertyPredicates, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExclude(TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
    {
        var entityEntry = ChangeEntityState(entity, EntityState.Modified);
        foreach (var propertyName in propertyNames)
        {
            EntityPropertyEntry(entity, propertyName).IsModified = false;
        }

        // 忽略空值
        IgnoreNullValues(ref entity, ignoreNullValues);

        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExclude(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
    {
        // 判断是非参数只有一个，且是一个匿名类型
        if (propertyPredicates?.Length == 1 && propertyPredicates[0].Body is NewExpression newExpression)
        {
            var propertyNames = newExpression.Members.Select(u => u.Name);
            return UpdateExclude(entity, propertyNames, ignoreNullValues);
        }
        else
        {
            var entityEntry = ChangeEntityState(entity, EntityState.Modified);
            foreach (var propertyPredicate in propertyPredicates)
            {
                EntityPropertyEntry(entity, propertyPredicate).IsModified = false;
            }

            // 忽略空值
            IgnoreNullValues(ref entity, ignoreNullValues);

            return entityEntry;
        }
    }

    /// <summary>
    /// 更新一条记录并排除属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExclude(TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
    {
        return UpdateExclude(entity, propertyNames.ToArray(), ignoreNullValues);
    }

    /// <summary>
    /// 更新一条记录并排除属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExclude(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
    {
        return UpdateExclude(entity, propertyPredicates.ToArray(), ignoreNullValues);
    }

    /// <summary>
    /// 更新一条记录并排除属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateExcludeAsync(TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
    {
        return Task.FromResult(UpdateExclude(entity, propertyNames, ignoreNullValues));
    }

    /// <summary>
    /// 更新一条记录并排除属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateExcludeAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
    {
        return Task.FromResult(UpdateExclude(entity, propertyPredicates, ignoreNullValues));
    }

    /// <summary>
    /// 更新一条记录并排除属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateExcludeAsync(TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
    {
        return Task.FromResult(UpdateExclude(entity, propertyNames, ignoreNullValues));
    }

    /// <summary>
    /// 更新一条记录并排除属性
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> UpdateExcludeAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
    {
        return Task.FromResult(UpdateExclude(entity, propertyPredicates, ignoreNullValues));
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExcludeNow(TEntity entity, string[] propertyNames, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateExclude(entity, propertyNames, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExcludeNow(TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateExclude(entity, propertyNames, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中实体</returns>
    public virtual EntityEntry<TEntity> UpdateExcludeNow(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateExclude(entity, propertyPredicates, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExcludeNow(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateExclude(entity, propertyPredicates, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExcludeNow(TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateExclude(entity, propertyNames, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExcludeNow(TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateExclude(entity, propertyNames, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExcludeNow(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateExclude(entity, propertyPredicates, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中的实体</returns>
    public virtual EntityEntry<TEntity> UpdateExcludeNow(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = UpdateExclude(entity, propertyPredicates, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateExcludeNowAsync(TEntity entity, string[] propertyNames, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateExcludeAsync(entity, propertyNames, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateExcludeNowAsync(TEntity entity, string[] propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateExcludeAsync(entity, propertyNames, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateExcludeNowAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateExcludeAsync(entity, propertyPredicates, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateExcludeNowAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateExcludeAsync(entity, propertyPredicates, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateExcludeNowAsync(TEntity entity, IEnumerable<string> propertyNames, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateExcludeAsync(entity, propertyNames, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyNames">属性名</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateExcludeNowAsync(TEntity entity, IEnumerable<string> propertyNames, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateExcludeAsync(entity, propertyNames, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateExcludeNowAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateExcludeAsync(entity, propertyPredicates, ignoreNullValues);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 更新一条记录并排除属性并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="propertyPredicates">属性表达式</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> UpdateExcludeNowAsync(TEntity entity, IEnumerable<Expression<Func<TEntity, object>>> propertyPredicates, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await UpdateExcludeAsync(entity, propertyPredicates, ignoreNullValues);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 忽略空值属性
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="ignoreNullValues"></param>
    private void IgnoreNullValues(ref TEntity entity, bool? ignoreNullValues = null)
    {
        var isIgnore = ignoreNullValues ?? DynamicContext.InsertOrUpdateIgnoreNullValues;
        if (isIgnore == false) return;

        // 获取所有的属性
        var properties = EntityType?.GetProperties();
        if (properties == null) return;

        foreach (var propety in properties)
        {
            var entityProperty = EntityPropertyEntry(entity, propety.Name);
            var propertyValue = entityProperty?.CurrentValue;
            var propertyType = entityProperty?.Metadata?.PropertyInfo?.PropertyType;

            // 判断是否是无效的值，比如为 null，默认时间，以及空 Guid 值
            var isInvalid = propertyValue == null
                            || (propertyType == typeof(DateTime) && propertyValue?.ToString() == new DateTime().ToString())
                            || (propertyType == typeof(DateTimeOffset) && propertyValue?.ToString() == new DateTimeOffset().ToString())
                            || (propertyType == typeof(Guid) && propertyValue?.ToString() == Guid.Empty.ToString());

            if (isInvalid && entityProperty != null)
            {
                entityProperty.IsModified = false;
            }
        }
    }

    /// <summary>
    /// 生成 UPDATE 语句
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="entity"></param>
    /// <param name="stringBuilder"></param>
    /// <param name="parameters"></param>
    /// <param name="includePropertyNames"></param>
    /// <param name="excludePropertyNames"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private void GenerateUpdateSQL(Func<string, IEnumerable<string>> tableNamesAction
        , TEntity entity
        , out StringBuilder stringBuilder
        , out List<object> parameters
        , string[] includePropertyNames = null
        , string[] excludePropertyNames = null)
    {
        if (tableNamesAction == null)
        {
            throw new ArgumentNullException(nameof(tableNamesAction));
        }

        // 原始表
        var originTableName = GetFullTableName();

        // 获取分表名称集合
        var returnTableNames = tableNamesAction(originTableName)?.ToArray();
        var tableSegments = ((returnTableNames == null || returnTableNames.Length == 0) ? [originTableName] : returnTableNames)
            .Distinct()
        .Select(u => string.IsNullOrWhiteSpace(u) ? originTableName : FormatDbElement(u));

        // 获取主键属性
        var columnProperty = EntityType.FindPrimaryKey().Properties
            .FirstOrDefault();
        var columnPropertyValue = Entry(entity).Property(columnProperty.Name).CurrentValue;

        if (columnPropertyValue == null)
        {
            throw new InvalidOperationException("No definition of the primary key found.");
        }

        // 查询主键列名
        var keyColumn = FormatDbElement(columnProperty?.GetColumnName(StoreObjectIdentifier.Table(EntityType?.GetTableName(), EntityType?.GetSchema())));

        // 获取列名
        var columnNames = EntityType.GetProperties()
            .ToDictionary(p => p.Name, p => FormatDbElement(p.GetColumnName(StoreObjectIdentifier.Table(EntityType?.GetTableName(), EntityType?.GetSchema()))))
            .Where(u => !u.Value.Equals(keyColumn, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(p => p.Key, p => p.Value);

        var setColumnStringBuilder = new StringBuilder();
        parameters = new();

        var i = 0;
        var j = 0;
        foreach (var (key, value) in columnNames)
        {
            j++;

            var canUpdate = true;

            if (includePropertyNames is { Length: > 0 })
            {
                canUpdate = includePropertyNames.Contains(key, StringComparer.OrdinalIgnoreCase);
            }
            else if (excludePropertyNames is { Length: > 0 })
            {
                if (excludePropertyNames.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    canUpdate = false;
                }
            }

            if (canUpdate)
            {
                setColumnStringBuilder.Append($"\"{key}\" = {{{i}}}");

                if (j < columnNames.Count)
                {
                    setColumnStringBuilder.Append(", ");
                }

                var propertyValue = Entry(entity).Property(key).CurrentValue;
                parameters.Add(propertyValue);

                i++;
            }
        }

        parameters.Add(columnPropertyValue);

        stringBuilder = new StringBuilder();

        // 生成更新语句
        foreach (var tableName in returnTableNames)
        {
            stringBuilder.AppendLine($"UPDATE {tableName} SET {setColumnStringBuilder} WHERE {keyColumn} = {{{i}}};");
        }
    }
}