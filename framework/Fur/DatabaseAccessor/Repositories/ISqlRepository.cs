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
        , ISqlQueryableRepository<TDbContextLocator>
        , IPrivateRepository
       where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 数据库操作对象
        /// </summary>
        DatabaseFacade Database { get; }

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
    }
}