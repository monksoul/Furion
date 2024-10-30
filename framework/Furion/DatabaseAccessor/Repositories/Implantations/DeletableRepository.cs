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
using System.Text;

namespace Furion.DatabaseAccessor;

/// <summary>
/// 可删除仓储分部类
/// </summary>
public partial class PrivateRepository<TEntity>
    where TEntity : class, IPrivateEntity, new()
{
    /// <summary>
    /// 根据主键分表删除记录
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="keys"></param>
    public virtual void DeleteFromSegments(Func<string, IEnumerable<string>> tableNamesAction, params object[] keys)
    {
        if (keys == null || keys.Length == 0)
        {
            return;
        }

        GenerateDeleteSQL(tableNamesAction, keys, out var stringBuilder);

        Database.ExecuteSqlRaw(stringBuilder.ToString(), keys);
    }

    /// <summary>
    /// 根据主键分表删除记录
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    public virtual async Task DeleteFromSegmentsAsync(Func<string, IEnumerable<string>> tableNamesAction, params object[] keys)
    {
        if (keys == null || keys.Length == 0)
        {
            return;
        }

        GenerateDeleteSQL(tableNamesAction, keys, out var stringBuilder);

        await Database.ExecuteSqlRawAsync(stringBuilder.ToString(), keys);
    }

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> Delete(TEntity entity)
    {
        return Entities.Remove(entity);
    }

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual void Delete(params TEntity[] entities)
    {
        Entities.RemoveRange(entities);
    }

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual void Delete(IEnumerable<TEntity> entities)
    {
        Entities.RemoveRange(entities);
    }

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>代理中的实体</returns>
    public virtual Task<EntityEntry<TEntity>> DeleteAsync(TEntity entity)
    {
        return Task.FromResult(Delete(entity));
    }

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    public virtual Task DeleteAsync(params TEntity[] entities)
    {
        Delete(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    public virtual Task DeleteAsync(IEnumerable<TEntity> entities)
    {
        Delete(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 删除一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>代理中的实体</returns>
    public virtual EntityEntry<TEntity> DeleteNow(TEntity entity)
    {
        var entityEntry = Delete(entity);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 删除一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <returns></returns>
    public virtual EntityEntry<TEntity> DeleteNow(TEntity entity, bool acceptAllChangesOnSuccess)
    {
        var entityEntry = Delete(entity);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual int DeleteNow(params TEntity[] entities)
    {
        Delete(entities);
        return SaveNow();
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    public virtual int DeleteNow(TEntity[] entities, bool acceptAllChangesOnSuccess)
    {
        Delete(entities);
        return SaveNow(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual int DeleteNow(IEnumerable<TEntity> entities)
    {
        Delete(entities);
        return SaveNow();
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    public virtual int DeleteNow(IEnumerable<TEntity> entities, bool acceptAllChangesOnSuccess)
    {
        Delete(entities);
        return SaveNow(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 删除一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>代理中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> DeleteNowAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = await DeleteAsync(entity);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 删除一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>代理中的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> DeleteNowAsync(TEntity entity, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var entityEntry = await DeleteAsync(entity);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    public virtual async Task<int> DeleteNowAsync(params TEntity[] entities)
    {
        await DeleteAsync(entities);
        return await SaveNowAsync();
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    public virtual async Task<int> DeleteNowAsync(TEntity[] entities, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(entities);
        return await SaveNowAsync(cancellationToken);
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    public virtual async Task<int> DeleteNowAsync(TEntity[] entities, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(entities);
        return await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    public virtual async Task<int> DeleteNowAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(entities);
        return await SaveNowAsync(cancellationToken);
    }

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    public virtual async Task<int> DeleteNowAsync(IEnumerable<TEntity> entities, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(entities);
        return await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 根据主键删除一条记录
    /// </summary>
    /// <param name="key">主键</param>
    public virtual void Delete(object key)
    {
        var deletedEntity = BuildDeletedEntity(key);
        if (deletedEntity != null) return;

        // 如果主键不存在，则采用 Find 查询
        var entity = FindOrDefault(key);
        if (entity != null) Delete(entity);
    }

    /// <summary>
    /// 根据主键删除一条记录
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns>Task</returns>
    public virtual async Task DeleteAsync(object key, CancellationToken cancellationToken = default)
    {
        var deletedEntity = BuildDeletedEntity(key);
        if (deletedEntity != null) return;

        // 如果主键不存在，则采用 FindAsync 查询
        var entity = await FindOrDefaultAsync(key, cancellationToken);
        if (entity != null) await DeleteAsync(entity);
    }

    /// <summary>
    /// 根据主键删除一条记录并立即提交
    /// </summary>
    /// <param name="key">主键</param>
    public virtual int DeleteNow(object key)
    {
        Delete(key);
        return SaveNow();
    }

    /// <summary>
    /// 根据主键删除一条记录并立即提交
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    public virtual int DeleteNow(object key, bool acceptAllChangesOnSuccess)
    {
        Delete(key);
        return SaveNow(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 根据主键删除一条记录并立即提交
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    public virtual async Task<int> DeleteNowAsync(object key, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(key, cancellationToken);
        return await SaveNowAsync(cancellationToken);
    }

    /// <summary>
    /// 根据主键删除一条记录并立即提交
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    public virtual async Task<int> DeleteNowAsync(object key, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(key, cancellationToken);
        return await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 构建被删除的实体
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="isRealDelete">是否真删除</param>
    /// <returns></returns>
    private TEntity BuildDeletedEntity(object key, bool isRealDelete = true)
    {
        // 读取主键
        var keyProperty = EntityType.FindPrimaryKey().Properties.AsEnumerable().FirstOrDefault()?.PropertyInfo;
        if (keyProperty == null) return default;

        // 判断当前主键是否被跟踪了
        var tracking = CheckTrackState(key, out var entityEntry, keyProperty.Name);
        if (tracking)
        {
            // 设置实体状态为已删除
            if (isRealDelete) ChangeEntityState(entityEntry, EntityState.Deleted);

            return entityEntry.Entity as TEntity;
        }

        // 如果没有被跟踪，创建实体对象并设置主键值
        var entity = Activator.CreateInstance<TEntity>();
        keyProperty.SetValue(entity, key);

        // 设置实体状态为已删除
        if (isRealDelete) ChangeEntityState(entity, EntityState.Deleted);

        return entity;
    }

    /// <summary>
    /// 生成 Delete 语句
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="keys"></param>
    /// <param name="stringBuilder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private void GenerateDeleteSQL(Func<string, IEnumerable<string>> tableNamesAction
        , object[] keys
        , out StringBuilder stringBuilder)
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

        // 查询主键列名
        var keyColumn = FormatDbElement(columnProperty?.GetColumnName(StoreObjectIdentifier.Table(EntityType?.GetTableName(), EntityType?.GetSchema())));

        var keyInStringBuilder = new StringBuilder();

        for (var i = 0; i < keys.Length; i++)
        {
            keyInStringBuilder.Append($"{{{i}}}");
            if (i != keys.Length - 1)
            {
                keyInStringBuilder.Append(", ");
            }
        }

        stringBuilder = new StringBuilder();

        // 生成删除语句
        foreach (var tableName in returnTableNames)
        {
            stringBuilder.AppendLine($"DELETE FROM {tableName} WHERE {keyColumn} IN ({keyInStringBuilder});");
        }
    }
}