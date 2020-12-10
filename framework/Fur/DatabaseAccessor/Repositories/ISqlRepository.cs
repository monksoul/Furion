using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// Sql 操作仓储接口
    /// </summary>
    public interface ISqlRepository : ISqlRepository<MasterDbContextLocator>
    {
    }

    /// <summary>
    /// Sql 操作仓储接口
    /// </summary>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    public interface ISqlRepository<TDbContextLocator>
        : ISqlExecutableRepository<TDbContextLocator>
        , ISqlReaderRepository<TDbContextLocator>
        , IPrivateRepository
       where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 数据库操作对象
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        DbContext DbContext { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        dynamic DynamicDbContext { get; }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        ISqlRepository<TChangeDbContextLocator> Change<TChangeDbContextLocator>()
            where TChangeDbContextLocator : class, IDbContextLocator;

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetService<TService>()
            where TService : class;

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetRequiredService<TService>()
            where TService : class;

        /// <summary>
        /// 将仓储约束为特定仓储
        /// </summary>
        /// <typeparam name="TRestrainRepository">特定仓储</typeparam>
        /// <returns>TRestrainRepository</returns>
        TRestrainRepository Constraint<TRestrainRepository>()
            where TRestrainRepository : class, IPrivateRepository;
    }
}