using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 仓储接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public partial interface IRepository<TEntity>
        : IRepository<TEntity, MasterDbContextLocator>
        , IWritableRepository<TEntity>
        , IReadableRepository<TEntity>
        , ISqlRepository
        where TEntity : class, IPrivateEntity, new()
    {
    }

    /// <summary>
    /// 非泛型仓储
    /// </summary>
    public partial interface IRepository
    {
        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        IRepository<TEntity> Change<TEntity>()
            where TEntity : class, IPrivateEntity, new();

        /// <summary>
        /// 切换多数据库上下文仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        IRepository<TEntity, TDbContextLocator> Change<TEntity, TDbContextLocator>()
            where TEntity : class, IPrivateEntity, new()
            where TDbContextLocator : class, IDbContextLocator;

        /// <summary>
        /// 获取 Sql 操作仓储
        /// </summary>
        /// <returns>ISqlRepository</returns>
        ISqlRepository Sql();

        /// <summary>
        /// 获取多数据库上下文 Sql 操作仓储
        /// </summary>
        /// <returns>ISqlRepository{TDbContextLocator}</returns>
        ISqlRepository<TDbContextLocator> Sql<TDbContextLocator>()
             where TDbContextLocator : class, IDbContextLocator;

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

    /// <summary>
    /// 多数据库上下文仓储
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    public partial interface IRepository<TEntity, TDbContextLocator>
        : IWritableRepository<TEntity, TDbContextLocator>
        , IReadableRepository<TEntity, TDbContextLocator>
        , ISqlRepository<TDbContextLocator>
        where TEntity : class, IPrivateEntity, new()
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        DbContext DbContext { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        dynamic DynamicDbContext { get; }

        /// <summary>
        /// 实体集合
        /// </summary>
        DbSet<TEntity> Entities { get; }

        /// <summary>
        /// 不跟踪的（脱轨）实体
        /// </summary>
        IQueryable<TEntity> DetachedEntities { get; }

        /// <summary>
        /// 查看实体类型
        /// </summary>
        IEntityType EntityType { get; }

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        DbConnection DbConnection { get; }

        /// <summary>
        /// 实体追综器
        /// </summary>
        ChangeTracker ChangeTracker { get; }

        /// <summary>
        /// 实体模型
        /// </summary>
        IModel Model { get; }

        /// <summary>
        /// 租户信息
        /// </summary>
        Tenant Tenant { get; }

        /// <summary>
        /// 数据库提供器名
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// 服务提供器
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 租户Id
        /// </summary>
        Guid? TenantId { get; }

        /// <summary>
        /// 判断上下文是否更改
        /// </summary>
        /// <returns>bool</returns>
        bool HasChanges();

        /// <summary>
        /// 将实体加入数据上下文托管
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>EntityEntry</returns>
        EntityEntry Entry(object entity);

        /// <summary>
        /// 将实体加入数据上下文托管
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        EntityEntry<TEntity> Entry(TEntity entity);

        /// <summary>
        /// 获取实体状态
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        EntityState EntityEntryState(object entity);

        /// <summary>
        /// 获取实体状态
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>EntityState</returns>
        EntityState EntityEntryState(TEntity entity);

        /// <summary>
        /// 将实体属性加入托管
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>PropertyEntry</returns>
        PropertyEntry EntityPropertyEntry(object entity, string propertyName);

        /// <summary>
        /// 将实体属性加入托管
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>PropertyEntry</returns>
        PropertyEntry EntityPropertyEntry(TEntity entity, string propertyName);

        /// <summary>
        /// 将实体属性加入托管
        /// </summary>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicate">属性表达式</param>
        /// <returns>PropertyEntry</returns>
        PropertyEntry<TEntity, TProperty> EntityPropertyEntry<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyPredicate);

        /// <summary>
        /// 改变实体状态
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="entityState">实体状态</param>
        /// <returns>EntityEntry</returns>
        EntityEntry ChangeEntityState(object entity, EntityState entityState);

        /// <summary>
        /// 改变实体状态
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="entityState">实体状态</param>
        /// <returns>EntityEntry{TEntity}</returns>
        EntityEntry<TEntity> ChangeEntityState(TEntity entity, EntityState entityState);

        /// <summary>
        /// 改变实体状态
        /// </summary>
        /// <param name="entityEntry">实体条目</param>
        /// <param name="entityState">实体状态</param>
        /// <returns>EntityEntry</returns>
        EntityEntry ChangeEntityState(EntityEntry entityEntry, EntityState entityState);

        /// <summary>
        /// 改变实体状态
        /// </summary>
        /// <param name="entityEntry">实体条目</param>
        /// <param name="entityState">实体状态</param>
        /// <returns>EntityEntry{TEntity}</returns>
        EntityEntry<TEntity> ChangeEntityState(EntityEntry<TEntity> entityEntry, EntityState entityState);

        /// <summary>
        /// 判断是否被附加
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>bool</returns>
        bool IsAttached(object entity);

        /// <summary>
        /// 判断是否被附加
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>bool</returns>
        bool IsAttached(TEntity entity);

        /// <summary>
        /// 附加实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>EntityEntry</returns>
        EntityEntry Attach(object entity);

        /// <summary>
        /// 附加实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>EntityEntry</returns>
        EntityEntry<TEntity> Attach(TEntity entity);

        /// <summary>
        /// 附加多个实体
        /// </summary>
        /// <param name="entities">多个实体</param>
        void AttachRange(params object[] entities);

        /// <summary>
        /// 附加多个实体
        /// </summary>
        /// <param name="entities">多个实体</param>
        void AttachRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// 取消附加实体
        /// </summary>
        /// <param name="entity">实体</param>
        void Detach(object entity);

        /// <summary>
        /// 取消附加实体
        /// </summary>
        /// <param name="entity">实体</param>
        void Detach(TEntity entity);

        /// <summary>
        /// 取消附加实体
        /// </summary>
        /// <param name="entityEntry">实体条目</param>
        void Detach(EntityEntry entityEntry);

        /// <summary>
        /// 取消附加实体
        /// </summary>
        /// <param name="entityEntry">实体条目</param>
        void Detach(EntityEntry<TEntity> entityEntry);

        /// <summary>
        /// 获取所有数据库上下文
        /// </summary>
        /// <returns>ConcurrentBag{DbContext}</returns>
        public ConcurrentBag<DbContext> GetDbContexts();

        /// <summary>
        /// 判断实体是否设置了主键
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>bool</returns>
        bool IsKeySet(TEntity entity);

        /// <summary>
        /// 删除数据库
        /// </summary>
        void EnsureDeleted();

        /// <summary>
        /// 删除数据库
        /// </summary>
        Task EnsureDeletedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建数据库
        /// </summary>
        void EnsureCreated();

        /// <summary>
        /// 创建数据库
        /// </summary>
        Task EnsureCreatedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 动态改变表名
        /// </summary>
        /// <param name="tableName">表名</param>
        void ChangeTable(string tableName);

        /// <summary>
        /// 动态改变数据库
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        void ChangeDatabase(string connectionString);

        /// <summary>
        /// 动态改变数据库
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        Task ChangeDatabaseAsync(string connectionString, CancellationToken cancellationToken = default);

        /// <summary>
        /// 判断是否是 SqlServer 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsSqlServer();

        /// <summary>
        /// 判断是否是 Sqlite 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsSqlite();

        /// <summary>
        /// 判断是否是 Cosmos 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsCosmos();

        /// <summary>
        /// 判断是否是 内存中 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool InMemoryDatabase();

        /// <summary>
        /// 判断是否是 MySql 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsMySql();

        /// <summary>
        /// 判断是否是 PostgreSQL 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsNpgsql();

        /// <summary>
        /// 判断是否是 Oracle 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsOracle();

        /// <summary>
        /// 判断是否是 Firebird 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsFirebird();

        /// <summary>
        /// 判断是否是 Dm 数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsDm();

        /// <summary>
        /// 判断是否是关系型数据库
        /// </summary>
        /// <returns>bool</returns>
        bool IsRelational();

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        new IRepository<TChangeEntity> Change<TChangeEntity>()
                where TChangeEntity : class, IPrivateEntity, new();

        /// <summary>
        /// 切换多数据库上下文仓储
        /// </summary>
        /// <typeparam name="TChangeEntity">实体类型</typeparam>
        /// <typeparam name="TChangeDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        IRepository<TChangeEntity, TChangeDbContextLocator> Change<TChangeEntity, TChangeDbContextLocator>()
            where TChangeEntity : class, IPrivateEntity, new()
            where TChangeDbContextLocator : class, IDbContextLocator;
    }
}