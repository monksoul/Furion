namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 多数据库仓储
    /// </summary>
    /// <typeparam name="TDbContextLocator"></typeparam>
    public partial interface IDbRepository<TDbContextLocator>
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        IRepository<TEntity, TDbContextLocator> Change<TEntity>()
            where TEntity : class, IPrivateEntity, new();

        /// <summary>
        /// 获取 Sql 操作仓储
        /// </summary>
        /// <returns></returns>
        ISqlRepository<TDbContextLocator> Sql();

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetService<TService>();

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetRequiredService<TService>();
    }
}