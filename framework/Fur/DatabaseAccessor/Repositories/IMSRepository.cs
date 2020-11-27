namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    public partial interface IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
    {
        /// <summary>
        /// 获取主库仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        IRepository<TEntity, TMasterDbContextLocator> Master<TEntity>()
             where TEntity : class, IPrivateEntity, new();

        /// <summary>
        /// 获取从库仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        IReadableRepository<TEntity, TSlaveDbContextLocator1> Slave1<TEntity>()
            where TEntity : class, IPrivateEntity, new();
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    public partial interface IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2>
        : IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
    {
        /// <summary>
        /// 获取从库仓储2
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        IReadableRepository<TEntity, TSlaveDbContextLocator2> Slave2<TEntity>()
            where TEntity : class, IPrivateEntity, new();
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    public partial interface IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3>
        : IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
    {
        /// <summary>
        /// 获取从库仓储3
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        IReadableRepository<TEntity, TSlaveDbContextLocator3> Slave3<TEntity>()
            where TEntity : class, IPrivateEntity, new();
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator4">从库</typeparam>
    public partial interface IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4>
        : IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
        where TSlaveDbContextLocator4 : class, IDbContextLocator
    {
        /// <summary>
        /// 获取从库仓储4
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        IReadableRepository<TEntity, TSlaveDbContextLocator4> Slave4<TEntity>()
            where TEntity : class, IPrivateEntity, new();
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator4">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator5">从库</typeparam>
    public partial interface IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5>
        : IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
        where TSlaveDbContextLocator4 : class, IDbContextLocator
        where TSlaveDbContextLocator5 : class, IDbContextLocator
    {
        /// <summary>
        /// 获取从库仓储5
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        IReadableRepository<TEntity, TSlaveDbContextLocator5> Slave5<TEntity>()
            where TEntity : class, IPrivateEntity, new();
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator4">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator5">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator6">从库</typeparam>
    public partial interface IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5, TSlaveDbContextLocator6>
        : IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
        where TSlaveDbContextLocator4 : class, IDbContextLocator
        where TSlaveDbContextLocator5 : class, IDbContextLocator
        where TSlaveDbContextLocator6 : class, IDbContextLocator
    {
        /// <summary>
        /// 获取从库仓储6
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        IReadableRepository<TEntity, TSlaveDbContextLocator6> Slave6<TEntity>()
            where TEntity : class, IPrivateEntity, new();
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator4">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator5">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator6">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator7">从库</typeparam>
    public partial interface IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5, TSlaveDbContextLocator6, TSlaveDbContextLocator7>
        : IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5, TSlaveDbContextLocator6>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
        where TSlaveDbContextLocator4 : class, IDbContextLocator
        where TSlaveDbContextLocator5 : class, IDbContextLocator
        where TSlaveDbContextLocator6 : class, IDbContextLocator
        where TSlaveDbContextLocator7 : class, IDbContextLocator
    {
        /// <summary>
        /// 获取从库仓储7
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        IReadableRepository<TEntity, TSlaveDbContextLocator7> Slave7<TEntity>()
            where TEntity : class, IPrivateEntity, new();
    }
}