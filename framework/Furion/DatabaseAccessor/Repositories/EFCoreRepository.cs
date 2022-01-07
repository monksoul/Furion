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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 非泛型EF Core仓储实现
    /// </summary>
    [SuppressSniffer]
    public partial class EFCoreRepository : IRepository
    {
        /// <summary>
        /// 服务提供器
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供器</param>
        public EFCoreRepository(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        public virtual IRepository<TEntity> Change<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _serviceProvider.GetService<IRepository<TEntity>>();
        }

        /// <summary>
        /// 切换多数据库上下文仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        public virtual IRepository<TEntity, TDbContextLocator> Change<TEntity, TDbContextLocator>()
            where TEntity : class, IPrivateEntity, new()
            where TDbContextLocator : class, IDbContextLocator
        {
            return _serviceProvider.GetService<IRepository<TEntity, TDbContextLocator>>();
        }

        /// <summary>
        /// 重新构建并切换仓储
        /// </summary>
        /// <remarks>特别注意，Scoped 必须手动释放</remarks>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        public virtual (IRepository<TEntity> Repository, IServiceScope Scoped) BuildChange<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            var scoped = _serviceProvider.CreateScope();
            var repository = scoped.ServiceProvider.GetService<IRepository<TEntity>>();

            // 添加未托管对象
            App.UnmanagedObjects.Add(scoped);

            return (repository, scoped);
        }

        /// <summary>
        /// 重新构建并切换多数据库上下文仓储
        /// </summary>
        /// <remarks>特别注意，Scoped 必须手动释放</remarks>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        public virtual (IRepository<TEntity, TDbContextLocator> Repository, IServiceScope Scoped) BuildChange<TEntity, TDbContextLocator>()
            where TEntity : class, IPrivateEntity, new()
            where TDbContextLocator : class, IDbContextLocator
        {
            var scoped = _serviceProvider.CreateScope();
            var repository = scoped.ServiceProvider.GetService<IRepository<TEntity, TDbContextLocator>>();

            // 添加未托管对象
            App.UnmanagedObjects.Add(scoped);

            return (repository, scoped);
        }

        /// <summary>
        /// 获取 Sql 操作仓储
        /// </summary>
        /// <returns>ISqlRepository</returns>
        public virtual ISqlRepository Sql()
        {
            return _serviceProvider.GetService<ISqlRepository>();
        }

        /// <summary>
        /// 获取多数据库上下文 Sql 操作仓储
        /// </summary>
        /// <returns>ISqlRepository{TDbContextLocator}</returns>
        public virtual ISqlRepository<TDbContextLocator> Sql<TDbContextLocator>()
             where TDbContextLocator : class, IDbContextLocator
        {
            return _serviceProvider.GetService<ISqlRepository<TDbContextLocator>>();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public virtual TService GetService<TService>()
        {
            return _serviceProvider.GetService<TService>();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public virtual TService GetRequiredService<TService>()
        {
            return _serviceProvider.GetRequiredService<TService>();
        }
    }

    /// <summary>
    /// EF Core仓储实现
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    [SuppressSniffer]
    public partial class EFCoreRepository<TEntity> : EFCoreRepository<TEntity, MasterDbContextLocator>
        , IRepository<TEntity>
        where TEntity : class, IPrivateEntity, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供器</param>
        public EFCoreRepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    /// <summary>
    /// 多数据库上下文仓储
    /// </summary>
    [SuppressSniffer]
    public partial class EFCoreRepository<TEntity, TDbContextLocator> : PrivateRepository<TEntity>
        , IRepository<TEntity, TDbContextLocator>
        where TEntity : class, IPrivateEntity, new()
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供器</param>
        public EFCoreRepository(IServiceProvider serviceProvider) : base(typeof(TDbContextLocator), serviceProvider)
        {
        }
    }

    /// <summary>
    /// 私有仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial class PrivateRepository<TEntity> : PrivateSqlRepository, IPrivateRepository<TEntity>
        where TEntity : class, IPrivateEntity, new()
    {
        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 数据库上下文池
        /// </summary>
        private readonly IDbContextPool _dbContextPool;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContextLocator"></param>
        /// <param name="serviceProvider">服务提供器</param>
        public PrivateRepository(Type dbContextLocator, IServiceProvider serviceProvider) : base(dbContextLocator, serviceProvider)
        {
            // 初始化服务提供器
            ServiceProvider = serviceProvider;

            // 设置提供器名称
            ProviderName = Database.ProviderName;

            // 只有关系型数据库才有连接信息
            if (Database.IsRelational()) DbConnection = Database.GetDbConnection();
            ChangeTracker = Context.ChangeTracker;
            Model = Context.Model;

            // 内置多租户
            Tenant = DynamicContext.Tenant;

            //初始化实体
            Entities = Context.Set<TEntity>();
            DetachedEntities = Entities.AsNoTracking();
            EntityType = Entities.EntityType;

            // 初始化数据上下文池
            _dbContextPool = serviceProvider.GetService<IDbContextPool>();

            // 非泛型仓储
            _repository = serviceProvider.GetService<IRepository>();
        }

        /// <summary>
        /// 实体集合
        /// </summary>
        public virtual DbSet<TEntity> Entities { get; }

        /// <summary>
        /// 不跟踪的（脱轨）实体
        /// </summary>
        public virtual IQueryable<TEntity> DetachedEntities { get; }

        /// <summary>
        /// 查看实体类型
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public virtual DbConnection DbConnection { get; }

        /// <summary>
        /// 实体追综器
        /// </summary>
        public virtual ChangeTracker ChangeTracker { get; }

        /// <summary>
        /// 实体模型
        /// </summary>
        public virtual IModel Model { get; }

        /// <summary>
        /// 租户信息
        /// </summary>
        public virtual Tenant Tenant { get; }

        /// <summary>
        /// 数据库提供器名
        /// </summary>
        public virtual string ProviderName { get; }

        /// <summary>
        /// 服务提供器
        /// </summary>
        public virtual IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 租户Id
        /// </summary>
        public virtual Guid? TenantId { get; }

        /// <summary>
        /// 判断上下文是否更改
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool HasChanges()
        {
            return ChangeTracker.HasChanges();
        }

        /// <summary>
        /// 将实体加入数据上下文托管
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>EntityEntry</returns>
        public virtual EntityEntry Entry(object entity)
        {
            return Context.Entry(entity);
        }

        /// <summary>
        /// 将实体加入数据上下文托管
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public virtual EntityEntry<TEntity> Entry(TEntity entity)
        {
            return Context.Entry(entity);
        }

        /// <summary>
        /// 获取实体状态
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public virtual EntityState EntityEntryState(object entity)
        {
            return Entry(entity).State;
        }

        /// <summary>
        /// 获取实体状态
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>EntityState</returns>
        public virtual EntityState EntityEntryState(TEntity entity)
        {
            return Entry(entity).State;
        }

        /// <summary>
        /// 将实体属性加入托管
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>PropertyEntry</returns>
        public virtual PropertyEntry EntityPropertyEntry(object entity, string propertyName)
        {
            return Entry(entity).Property(propertyName);
        }

        /// <summary>
        /// 将实体属性加入托管
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>PropertyEntry</returns>
        public virtual PropertyEntry EntityPropertyEntry(TEntity entity, string propertyName)
        {
            return Entry(entity).Property(propertyName);
        }

        /// <summary>
        /// 将实体属性加入托管
        /// </summary>
        /// <typeparam name="TProperty">属性类型</typeparam>
        /// <param name="entity">实体</param>
        /// <param name="propertyPredicate">属性表达式</param>
        /// <returns>PropertyEntry</returns>
        public virtual PropertyEntry<TEntity, TProperty> EntityPropertyEntry<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyPredicate)
        {
            return Entry(entity).Property(propertyPredicate);
        }

        /// <summary>
        /// 改变实体状态
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="entityState">实体状态</param>
        /// <returns>EntityEntry</returns>
        public virtual EntityEntry ChangeEntityState(object entity, EntityState entityState)
        {
            var entityEntry = Entry(entity);
            entityEntry.State = entityState;
            return entityEntry;
        }

        /// <summary>
        /// 改变实体状态
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="entityState">实体状态</param>
        /// <returns>EntityEntry{TEntity}</returns>
        public virtual EntityEntry<TEntity> ChangeEntityState(TEntity entity, EntityState entityState)
        {
            var entityEntry = Entry(entity);
            entityEntry.State = entityState;
            return entityEntry;
        }

        /// <summary>
        /// 改变实体状态
        /// </summary>
        /// <param name="entityEntry">实体条目</param>
        /// <param name="entityState">实体状态</param>
        /// <returns>EntityEntry</returns>
        public virtual EntityEntry ChangeEntityState(EntityEntry entityEntry, EntityState entityState)
        {
            entityEntry.State = entityState;
            return entityEntry;
        }

        /// <summary>
        /// 改变实体状态
        /// </summary>
        /// <param name="entityEntry">实体条目</param>
        /// <param name="entityState">实体状态</param>
        /// <returns>EntityEntry{TEntity}</returns>
        public virtual EntityEntry<TEntity> ChangeEntityState(EntityEntry<TEntity> entityEntry, EntityState entityState)
        {
            entityEntry.State = entityState;
            return entityEntry;
        }

        /// <summary>
        /// 检查实体跟踪状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entityEntry"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public virtual bool CheckTrackState(object id, out EntityEntry entityEntry, string keyName = default)
        {
            return CheckTrackState<TEntity>(id, out entityEntry, keyName);
        }

        /// <summary>
        /// 检查实体跟踪状态
        /// </summary>
        /// <typeparam name="TTrackEntity"></typeparam>
        /// <param name="id"></param>
        /// <param name="entityEntry"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public virtual bool CheckTrackState<TTrackEntity>(object id, out EntityEntry entityEntry, string keyName = default)
            where TTrackEntity : class, IPrivateEntity, new()
        {
            // 获取主键名
            keyName ??= (typeof(TTrackEntity) == typeof(TEntity) ? EntityType : Context.Set<TTrackEntity>().EntityType)
                        .FindPrimaryKey()?.Properties?.AsEnumerable()?.FirstOrDefault()?.PropertyInfo?.Name;

            // 检查是否已经跟踪
            entityEntry = ChangeTracker.Entries().FirstOrDefault(u => u.Entity.GetType() == typeof(TTrackEntity)
                                             && u.CurrentValues[keyName].ToString().Equals(id.ToString()));

            return entityEntry != null;
        }

        /// <summary>
        /// 判断是否被附加
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>bool</returns>
        public virtual bool IsAttached(object entity)
        {
            return EntityEntryState(entity) != EntityState.Detached;
        }

        /// <summary>
        /// 判断是否被附加
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>bool</returns>
        public virtual bool IsAttached(TEntity entity)
        {
            return EntityEntryState(entity) != EntityState.Detached;
        }

        /// <summary>
        /// 附加实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>EntityEntry</returns>
        public virtual EntityEntry Attach(object entity)
        {
            return Context.Attach(entity);
        }

        /// <summary>
        /// 附加实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>EntityEntry</returns>
        public virtual EntityEntry<TEntity> Attach(TEntity entity)
        {
            return Context.Attach(entity);
        }

        /// <summary>
        /// 附加多个实体
        /// </summary>
        /// <param name="entities">多个实体</param>
        public virtual void AttachRange(params object[] entities)
        {
            Context.AttachRange(entities);
        }

        /// <summary>
        /// 附加多个实体
        /// </summary>
        /// <param name="entities">多个实体</param>
        public virtual void AttachRange(IEnumerable<TEntity> entities)
        {
            Context.AttachRange(entities);
        }

        /// <summary>
        /// 取消附加实体
        /// </summary>
        /// <param name="entity">实体</param>
        public virtual void Detach(object entity)
        {
            ChangeEntityState(entity, EntityState.Detached);
        }

        /// <summary>
        /// 取消附加实体
        /// </summary>
        /// <param name="entity">实体</param>
        public virtual void Detach(TEntity entity)
        {
            ChangeEntityState(entity, EntityState.Detached);
        }

        /// <summary>
        /// 取消附加实体
        /// </summary>
        /// <param name="entityEntry">实体条目</param>
        public virtual void Detach(EntityEntry entityEntry)
        {
            ChangeEntityState(entityEntry, EntityState.Detached);
        }

        /// <summary>
        /// 取消附加实体
        /// </summary>
        /// <param name="entityEntry">实体条目</param>
        public virtual void Detach(EntityEntry<TEntity> entityEntry)
        {
            ChangeEntityState(entityEntry, EntityState.Detached);
        }

        /// <summary>
        /// 获取所有数据库上下文
        /// </summary>
        /// <returns>ConcurrentBag{DbContext}</returns>
        public virtual ConcurrentDictionary<Guid, DbContext> GetDbContexts()
        {
            return _dbContextPool.GetDbContexts();
        }

        /// <summary>
        /// 判断实体是否设置了主键
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>bool</returns>
        public virtual bool IsKeySet(TEntity entity)
        {
            return Entry(entity).IsKeySet;
        }

        /// <summary>
        /// 删除数据库
        /// </summary>
        public virtual void EnsureDeleted()
        {
            Context.Database.EnsureDeleted();
        }

        /// <summary>
        /// 删除数据库
        /// </summary>
        public virtual Task EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            return Context.Database.EnsureDeletedAsync(cancellationToken);
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        public virtual void EnsureCreated()
        {
            Context.Database.EnsureCreated();
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        public virtual Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            return Context.Database.EnsureCreatedAsync(cancellationToken);
        }

        /// <summary>
        /// 动态改变表名
        /// </summary>
        /// <param name="tableName">表名</param>
        [Obsolete("该方法已过时，请调用 BuildChange<TEntity> 方法代替。")]
        public virtual void ChangeTable(string tableName)
        {
            if (EntityType is IConventionEntityType convention)
            {
                convention.SetTableName(tableName);
            }
        }

        /// <summary>
        /// 动态改变数据库
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public virtual void ChangeDatabase(string connectionString)
        {
            if (DbConnection.State == ConnectionState.Open) DbConnection.ChangeDatabase(connectionString);
            else DbConnection.ConnectionString = connectionString;
        }

        /// <summary>
        /// 动态改变数据库
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        public virtual async Task ChangeDatabaseAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            if (DbConnection.State == ConnectionState.Open)
            {
                await DbConnection.ChangeDatabaseAsync(connectionString, cancellationToken);
            }
            else
            {
                DbConnection.ConnectionString = connectionString;
            }
        }

        /// <summary>
        /// 判断是否是 SqlServer 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsSqlServer()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.SqlServer);
        }

        /// <summary>
        /// 判断是否是 Sqlite 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsSqlite()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.Sqlite);
        }

        /// <summary>
        /// 判断是否是 Cosmos 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsCosmos()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.Cosmos);
        }

        /// <summary>
        /// 判断是否是 内存中 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool InMemoryDatabase()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.InMemoryDatabase);
        }

        /// <summary>
        /// 判断是否是 MySql 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsMySql()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.MySql);
        }

        /// <summary>
        /// 判断是否是 MySql 数据库 官方包（更新不及时，只支持 8.0.23+ 版本， 所以单独弄一个分类）
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsMySqlOfficial()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.MySqlOfficial);
        }

        /// <summary>
        /// 判断是否是 PostgreSQL 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsNpgsql()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.Npgsql);
        }

        /// <summary>
        /// 判断是否是 Oracle 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsOracle()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.Oracle);
        }

        /// <summary>
        /// 判断是否是 Firebird 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsFirebird()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.Firebird);
        }

        /// <summary>
        /// 判断是否是 Dm 数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsDm()
        {
            return DbProvider.IsDatabaseFor(ProviderName, DbProvider.Dm);
        }

        /// <summary>
        /// 判断是否是关系型数据库
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool IsRelational()
        {
            return Database.IsRelational();
        }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        public virtual new IRepository<TChangeEntity> Change<TChangeEntity>()
            where TChangeEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TChangeEntity>();
        }

        /// <summary>
        /// 切换多数据库上下文仓储
        /// </summary>
        /// <typeparam name="TChangeEntity">实体类型</typeparam>
        /// <typeparam name="TChangeDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        public virtual IRepository<TChangeEntity, TChangeDbContextLocator> Change<TChangeEntity, TChangeDbContextLocator>()
            where TChangeEntity : class, IPrivateEntity, new()
            where TChangeDbContextLocator : class, IDbContextLocator
        {
            return _repository.Change<TChangeEntity, TChangeDbContextLocator>();
        }

        /// <summary>
        /// 重新构建并切换仓储
        /// </summary>
        /// <remarks>特别注意，Scoped 必须手动释放</remarks>
        /// <typeparam name="TChangeEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        public virtual (IRepository<TChangeEntity> Repository, IServiceScope Scoped) BuildChange<TChangeEntity>()
            where TChangeEntity : class, IPrivateEntity, new()
        {
            return _repository.BuildChange<TChangeEntity>();
        }

        /// <summary>
        /// 重新构建并切换多数据库上下文仓储
        /// </summary>
        /// <remarks>特别注意，Scoped 必须手动释放</remarks>
        /// <typeparam name="TChangeEntity">实体类型</typeparam>
        /// <typeparam name="TChangeDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        public virtual (IRepository<TChangeEntity, TChangeDbContextLocator> Repository, IServiceScope Scoped) BuildChange<TChangeEntity, TChangeDbContextLocator>()
            where TChangeEntity : class, IPrivateEntity, new()
            where TChangeDbContextLocator : class, IDbContextLocator
        {
            return _repository.BuildChange<TChangeEntity, TChangeDbContextLocator>();
        }
    }
}