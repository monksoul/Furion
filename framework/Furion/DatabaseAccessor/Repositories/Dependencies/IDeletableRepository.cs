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

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Furion.DatabaseAccessor;

/// <summary>
/// 可删除仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public partial interface IDeletableRepository<TEntity> : IDeletableRepository<TEntity, MasterDbContextLocator>
    where TEntity : class, IPrivateEntity, new()
{
}

/// <summary>
/// 可删除仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
public partial interface IDeletableRepository<TEntity, TDbContextLocator> : IPrivateDeletableRepository<TEntity>
    where TEntity : class, IPrivateEntity, new()
    where TDbContextLocator : class, IDbContextLocator
{
}

/// <summary>
/// 可删除仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IPrivateDeletableRepository<TEntity> : IPrivateRootRepository
    where TEntity : class, IPrivateEntity, new()
{
    /// <summary>
    /// 根据主键分表删除记录
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="keys"></param>
    void DeleteFromSegments(Func<string, IEnumerable<string>> tableNamesAction, params object[] keys);

    /// <summary>
    /// 根据主键分表删除记录
    /// </summary>
    /// <param name="tableNamesAction"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    Task DeleteFromSegmentsAsync(Func<string, IEnumerable<string>> tableNamesAction, params object[] keys);

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>代理中的实体</returns>
    EntityEntry<TEntity> Delete(TEntity entity);

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    void Delete(params TEntity[] entities);

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    void Delete(IEnumerable<TEntity> entities);

    /// <summary>
    /// 删除一条记录
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>代理中的实体</returns>
    Task<EntityEntry<TEntity>> DeleteAsync(TEntity entity);

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    Task DeleteAsync(params TEntity[] entities);

    /// <summary>
    /// 删除多条记录
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    Task DeleteAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// 删除一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>代理中的实体</returns>
    EntityEntry<TEntity> DeleteNow(TEntity entity);

    /// <summary>
    /// 删除一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <returns></returns>
    EntityEntry<TEntity> DeleteNow(TEntity entity, bool acceptAllChangesOnSuccess);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    int DeleteNow(params TEntity[] entities);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    int DeleteNow(TEntity[] entities, bool acceptAllChangesOnSuccess);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    int DeleteNow(IEnumerable<TEntity> entities);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    int DeleteNow(IEnumerable<TEntity> entities, bool acceptAllChangesOnSuccess);

    /// <summary>
    /// 删除一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>代理中的实体</returns>
    Task<EntityEntry<TEntity>> DeleteNowAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除一条记录并立即提交
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>代理中的实体</returns>
    Task<EntityEntry<TEntity>> DeleteNowAsync(TEntity entity, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <returns>Task</returns>
    Task<int> DeleteNowAsync(params TEntity[] entities);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    Task<int> DeleteNowAsync(TEntity[] entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    Task<int> DeleteNowAsync(TEntity[] entities, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    Task<int> DeleteNowAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除多条记录并立即提交
    /// </summary>
    /// <param name="entities">多个实体</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">取消异步令牌</param>
    /// <returns>Task</returns>
    Task<int> DeleteNowAsync(IEnumerable<TEntity> entities, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据主键删除一条记录
    /// </summary>
    /// <param name="key">主键</param>
    void Delete(object key);

    /// <summary>
    /// 根据主键删除一条记录
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns>Task</returns>
    Task DeleteAsync(object key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据主键删除一条记录并立即提交
    /// </summary>
    /// <param name="key">主键</param>
    int DeleteNow(object key);

    /// <summary>
    /// 根据主键删除一条记录并立即提交
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    int DeleteNow(object key, bool acceptAllChangesOnSuccess);

    /// <summary>
    /// 根据主键删除一条记录并立即提交
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task<int> DeleteNowAsync(object key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据主键删除一条记录并立即提交
    /// </summary>
    /// <param name="key">主键</param>
    /// <param name="acceptAllChangesOnSuccess">接受所有更改</param>
    /// <param name="cancellationToken">异步取消令牌</param>
    /// <returns></returns>
    Task<int> DeleteNowAsync(object key, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
}