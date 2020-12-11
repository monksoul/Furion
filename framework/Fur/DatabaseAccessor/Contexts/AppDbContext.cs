using Fur.DependencyInjection;
using Fur.Extensions;
using Fur.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 默认应用数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    [SkipScan]
    public abstract class AppDbContext<TDbContext> : AppDbContext<TDbContext, MasterDbContextLocator>
        where TDbContext : DbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<TDbContext> options) : base(options)
        {
        }
    }

    /// <summary>
    /// 应用数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    [SkipScan]
    public abstract class AppDbContext<TDbContext, TDbContextLocator> : DbContext
        where TDbContext : DbContext
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<TDbContext> options) : base(options)
        {
            ChangeTrackerEntities = new List<EntityEntry>();
        }

        /// <summary>
        /// 数据库上下文提交更改之前执行事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        protected virtual void SavingChangesEvent(DbContextEventData eventData, InterceptionResult<int> result)
        {
        }

        /// <summary>
        /// 数据库上下文提交更改成功执行事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        protected virtual void SavedChangesEvent(SaveChangesCompletedEventData eventData, int result)
        {
        }

        /// <summary>
        /// 数据库上下文提交更改失败执行事件
        /// </summary>
        /// <param name="eventData"></param>
        protected virtual void SaveChangesFailedEvent(DbContextErrorEventData eventData)
        {
        }

        /// <summary>
        /// 数据库上下文初始化调用方法
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// 数据库上下文配置模型调用方法
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置数据库上下文实体
            AppDbContextBuilder.ConfigureDbContextEntity(modelBuilder, this, typeof(TDbContextLocator));
        }

        /// <summary>
        /// 新增或更新忽略空值（默认值）
        /// </summary>
        public virtual bool InsertOrUpdateIgnoreNullValues { get; protected set; } = false;

        /// <summary>
        /// 启用实体跟踪（默认值）
        /// </summary>
        public virtual bool EnabledEntityStateTracked { get; protected set; } = true;

        /// <summary>
        /// 启用实体数据更改监听
        /// </summary>
        public virtual bool EnabledEntityChangedListener { get; protected set; } = false;

        /// <summary>
        /// 获取租户信息
        /// </summary>
        public virtual Tenant Tenant
        {
            get
            {
                // 如果没有实现多租户方式，则无需查询
                if (Db.CustomizeMultiTenants || !typeof(IPrivateMultiTenant).IsAssignableFrom(GetType())) return default;

                // 判断 HttpContext 是否存在
                var httpContext = HttpContextUtility.GetCurrentHttpContext();
                if (httpContext == null) return default;

                // 获取主机地址
                var host = httpContext.Request.Host.Value;

                // 从内存缓存中读取或查询数据库
                var memoryCache = App.GetService<IMemoryCache>();
                return memoryCache.GetOrCreate($"{host}:MultiTenants", cache =>
                {
                    // 读取数据库
                    var tenantDbContext = Db.GetDuplicateDbContext<MultiTenantDbContextLocator>();
                    if (tenantDbContext == null) return default;

                    return tenantDbContext.Set<Tenant>().FirstOrDefault(u => u.Host == host);
                });
            }
        }

        /// <summary>
        /// 构建基于表租户查询过滤器表达式
        /// </summary>
        /// <param name="entityBuilder">实体类型构建器</param>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="onTableTenantId">多租户Id属性名</param>
        /// <returns>表达式</returns>
        protected virtual LambdaExpression TenantIdQueryFilterExpression(EntityTypeBuilder entityBuilder, DbContext dbContext, string onTableTenantId = default)
        {
            onTableTenantId ??= Db.OnTableTenantId;

            // 获取实体构建器元数据
            var metadata = entityBuilder.Metadata;
            if (metadata.FindProperty(onTableTenantId) == null) return default;

            // 创建表达式元素
            var parameter = Expression.Parameter(metadata.ClrType, "u");
            var properyName = Expression.Constant(onTableTenantId);
            var propertyValue = Expression.Call(Expression.Constant(dbContext), dbContext.GetType().GetMethod(nameof(IMultiTenantOnTable.GetTenantId)));

            var expressionBody = Expression.Equal(Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(object) }, parameter, properyName), propertyValue);
            var expression = Expression.Lambda(expressionBody, parameter);
            return expression;
        }

        /// <summary>
        /// 正在更改并跟踪的数据
        /// </summary>
        private IEnumerable<EntityEntry> ChangeTrackerEntities { get; set; }

        /// <summary>
        /// 内部数据库上下文提交更改之前执行事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        internal void SavingChangesEventInner(DbContextEventData eventData, InterceptionResult<int> result)
        {
            // 附加实体更改通知
            if (EnabledEntityChangedListener)
            {
                var dbContext = eventData.Context;

                // 获取获取数据库操作上下文
                ChangeTrackerEntities = (dbContext).ChangeTracker.Entries()
                    .Where(u => u.State == EntityState.Added || u.State == EntityState.Modified || u.State == EntityState.Deleted).ToList();

                AttachEntityChangedListener(eventData.Context, "OnChanging", ChangeTrackerEntities);
            }

            SavingChangesEvent(eventData, result);
        }

        /// <summary>
        /// 内部数据库上下文提交更改成功执行事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        internal void SavedChangesEventInner(SaveChangesCompletedEventData eventData, int result)
        {
            // 附加实体更改通知
            if (EnabledEntityChangedListener) AttachEntityChangedListener(eventData.Context, "OnChanged", ChangeTrackerEntities);

            SavedChangesEvent(eventData, result);
        }

        /// <summary>
        /// 内部数据库上下文提交更改失败执行事件
        /// </summary>
        /// <param name="eventData"></param>
        internal void SaveChangesFailedEventInner(DbContextErrorEventData eventData)
        {
            // 附加实体更改通知
            if (EnabledEntityChangedListener) AttachEntityChangedListener(eventData.Context, "OnChangeFailed", ChangeTrackerEntities);

            SaveChangesFailedEvent(eventData);
        }

        /// <summary>
        /// 附加实体改变监听
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="triggerMethodName"></param>
        /// <param name="changeTrackerEntities"></param>
        private static void AttachEntityChangedListener(DbContext dbContext, string triggerMethodName, IEnumerable<EntityEntry> changeTrackerEntities = null)
        {
            // 获取所有改变的类型
            var entityChangedTypes = AppDbContextBuilder.DbContextLocatorCorrelationTypes[typeof(TDbContextLocator)].EntityChangedTypes;
            if (!entityChangedTypes.Any()) return;

            // 遍历所有的改变的实体
            foreach (var entity in changeTrackerEntities)
            {
                // 获取该实体类型的种子配置
                var entitiesTypeByChanged = entityChangedTypes
                    .Where(u => u.GetInterfaces()
                        .Any(i => i.HasImplementedRawGeneric(typeof(IPrivateEntityChangedListener<>)) && i.GenericTypeArguments.Contains(entity.Entity.GetType())));

                if (!entitiesTypeByChanged.Any()) continue;

                // 通知所有的监听类型
                foreach (var entityChangedType in entitiesTypeByChanged)
                {
                    var OnChangeMethod = entityChangedType.GetMethod(triggerMethodName);
                    if (OnChangeMethod == null) continue;

                    var instance = Activator.CreateInstance(entityChangedType);
                    OnChangeMethod.Invoke(instance, new object[] { entity.Entity, dbContext, typeof(TDbContextLocator), entity.State });
                }
            }
        }
    }
}