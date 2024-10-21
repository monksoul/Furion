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
/// 可插入仓储分部类
/// </summary>
public partial class PrivateRepository<TEntity>
    where TEntity : class, IPrivateEntity, new()
{
    /// <summary>
    /// 分表插入一条记录
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="entity"></param>
    /// <param name="keySet"></param>
    public virtual void InsertFromSegments(Func<string, IEnumerable<string>> tableNamesAction, TEntity entity, object keySet = null)
    {
        GenerateInsertSQL(tableNamesAction, entity, out var stringBuilder, out var parameters, keySet);

        Database.ExecuteSqlRaw(stringBuilder.ToString(), parameters.ToArray());
    }

    /// <summary>
    /// 分表插入一条记录
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="entity"></param>
    /// <param name="keySet"></param>
    public virtual async Task InsertFromSegmentsAsync(Func<string, IEnumerable<string>> tableNamesAction, TEntity entity, object keySet = null)
    {
        GenerateInsertSQL(tableNamesAction, entity, out var stringBuilder, out var parameters, keySet);

        await Database.ExecuteSqlRawAsync(stringBuilder.ToString(), parameters.ToArray());
    }

    /// <summary>
    /// 新增一条记录
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>代理的实体</returns>
    public virtual EntityEntry<TEntity> Insert(TEntity entity, bool? ignoreNullValues = null)
    {
        var entryEntity = Entities.Add(entity);

        // 忽略空值
        IgnoreNullValues(ref entity, ignoreNullValues);

        return entryEntity;
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual void Insert(params TEntity[] entities)
    {
        Entities.AddRange(entities);
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual void Insert(IEnumerable<TEntity> entities)
    {
        Entities.AddRange(entities);
    }

    /// <summary>
    /// 新增一条记录
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>代理的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> InsertAsync(TEntity entity, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await Entities.AddAsync(entity, cancellationToken);

        // 忽略空值
        IgnoreNullValues(ref entity, ignoreNullValues);

        return entityEntry;
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    public virtual Task InsertAsync(params TEntity[] entities)
    {
        return Entities.AddRangeAsync(entities);
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns></returns>
    public virtual Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return Entities.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// 新增一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中返回的实体</returns>
    public virtual EntityEntry<TEntity> InsertNow(TEntity entity, bool? ignoreNullValues = null)
    {
        var entityEntry = Insert(entity, ignoreNullValues);
        SaveNow();
        return entityEntry;
    }

    /// <summary>
    /// 新增一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <returns>数据库中返回的实体</returns>
    public virtual EntityEntry<TEntity> InsertNow(TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null)
    {
        var entityEntry = Insert(entity, ignoreNullValues);
        SaveNow(acceptAllChangesOnSuccess);
        return entityEntry;
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual int InsertNow(params TEntity[] entities)
    {
        Insert(entities);
        return SaveNow();
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    public virtual int InsertNow(TEntity[] entities, bool acceptAllChangesOnSuccess)
    {
        Insert(entities);
        return SaveNow(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    public virtual int InsertNow(IEnumerable<TEntity> entities)
    {
        Insert(entities);
        return SaveNow();
    }

    /// <summary>
    /// 新增多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    public virtual int InsertNow(IEnumerable<TEntity> entities, bool acceptAllChangesOnSuccess)
    {
        Insert(entities);
        return SaveNow(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 新增一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中返回的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> InsertNowAsync(TEntity entity, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await InsertAsync(entity, ignoreNullValues, cancellationToken);
        await SaveNowAsync(cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 新增一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="ignoreNullValues"></param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>数据库中返回的实体</returns>
    public virtual async Task<EntityEntry<TEntity>> InsertNowAsync(TEntity entity, bool acceptAllChangesOnSuccess, bool? ignoreNullValues = null, CancellationToken cancellationToken = default)
    {
        var entityEntry = await InsertAsync(entity, ignoreNullValues, cancellationToken);
        await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
        return entityEntry;
    }

    /// <summary>
    /// 新增多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    public virtual async Task<int> InsertNowAsync(params TEntity[] entities)
    {
        await InsertAsync(entities);
        return await SaveNowAsync();
    }

    /// <summary>
    /// 新增多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    public virtual async Task<int> InsertNowAsync(TEntity[] entities, CancellationToken cancellationToken = default)
    {
        await InsertAsync(entities);
        return await SaveNowAsync(cancellationToken);
    }

    /// <summary>
    /// 新增多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    public virtual async Task<int> InsertNowAsync(TEntity[] entities, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await InsertAsync(entities);
        return await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 新增多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    public virtual async Task<int> InsertNowAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await InsertAsync(entities, cancellationToken);
        return await SaveNowAsync(cancellationToken);
    }

    /// <summary>
    /// 新增多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    public virtual async Task<int> InsertNowAsync(IEnumerable<TEntity> entities, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await InsertAsync(entities, cancellationToken);
        return await SaveNowAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 生成 INSERT 语句
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="entity"></param>
    /// <param name="stringBuilder"></param>
    /// <param name="parameters"></param>
    /// <param name="keySet"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private void GenerateInsertSQL(Func<string, IEnumerable<string>> tableNamesAction
        , TEntity entity
        , out StringBuilder stringBuilder
        , out List<object> parameters, object keySet = null)
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

        // 获取主键的值
        var keyValue = keySet ?? (
            columnProperty.ClrType == typeof(Guid)
                ? ((columnPropertyValue == null || columnPropertyValue == (object)Guid.Empty) ? Guid.NewGuid() : columnPropertyValue)
                : null
        );

        // 查询主键列名
        var keyColumn = FormatDbElement(columnProperty?.GetColumnName(StoreObjectIdentifier.Table(EntityType?.GetTableName(), EntityType?.GetSchema())));

        // 获取列名
        var columnNames = EntityType.GetProperties()
            .ToDictionary(p => p.Name, p => FormatDbElement(p.GetColumnName(StoreObjectIdentifier.Table(EntityType?.GetTableName(), EntityType?.GetSchema()))))
            .Where(keyValue == null, u => !u.Value.Equals(keyColumn, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(p => p.Key, p => p.Value);

        var columnList = string.Join(", ", columnNames.Values);
        var parameterValues = string.Join(", ", columnNames.Keys.Select((p, i) => $"{{{i}}}"));

        stringBuilder = new StringBuilder();
        parameters = new();

        // 获取每个属性的值
        foreach (var propName in columnNames.Keys)
        {
            var propertyValue = Entry(entity).Property(propName).CurrentValue;
            parameters.Add(propertyValue);
        }

        // 生成插入语句
        foreach (var tableName in returnTableNames)
        {
            stringBuilder.AppendLine($"INSERT INTO {tableName} ({columnList}) VALUES ({parameterValues});");
        }
    }
}