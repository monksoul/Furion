// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.Extensions;
using Furion.JsonSerialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 默认应用数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    [SuppressSniffer]
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
    [SuppressSniffer]
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
            ChangeTrackerEntities ??= new Dictionary<EntityEntry, PropertyValues>();
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
        /// 保存失败自动回滚
        /// </summary>
        public virtual bool FailedAutoRollback { get; protected set; } = true;

        /// <summary>
        /// 支持工作单元共享事务
        /// </summary>
        public virtual bool UseUnitOfWork { get; protected set; } = true;

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
                var httpContext = App.HttpContext;
                if (httpContext == null) return default;

                // 获取主机地址
                var host = httpContext.Request.Host.Value;

                // 获取服务提供器
                var serviceProvider = httpContext.RequestServices;

                // 从分布式缓存中读取或查询数据库
                var tenantCachedKey = $"MULTI_TENANT:{host}";
                var distributedCache = serviceProvider.GetService<IDistributedCache>();
                var cachedValue = distributedCache?.GetString(tenantCachedKey);

                // 当前租户
                Tenant currentTenant;

                // 获取序列化库
                var jsonSerializerProvider = serviceProvider.GetService<IJsonSerializerProvider>();

                // 如果 Key 不存在
                if (string.IsNullOrWhiteSpace(cachedValue))
                {
                    // 解析租户上下文
                    var dbContextResolve = serviceProvider.GetService<Func<Type, IScoped, DbContext>>();
                    if (dbContextResolve == null) return default;

                    var tenantDbContext = dbContextResolve(typeof(MultiTenantDbContextLocator), default);
                    ((dynamic)tenantDbContext).UseUnitOfWork = false;   // 无需载入事务

                    currentTenant = tenantDbContext.Set<Tenant>().AsNoTracking().FirstOrDefault(u => u.Host == host);
                    if (currentTenant != null)
                    {
                        distributedCache?.SetString(tenantCachedKey, jsonSerializerProvider.Serialize(currentTenant));
                    }
                }
                else currentTenant = jsonSerializerProvider.Deserialize<Tenant>(cachedValue);

                return currentTenant;
            }
        }

        /// <summary>
        /// 构建基于表租户查询过滤器表达式
        /// </summary>
        /// <param name="entityBuilder">实体类型构建器</param>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="onTableTenantId">多租户Id属性名</param>
        /// <returns>表达式</returns>
        protected virtual LambdaExpression BuildTenantQueryFilter(EntityTypeBuilder entityBuilder, DbContext dbContext, string onTableTenantId = default)
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
        private Dictionary<EntityEntry, PropertyValues> ChangeTrackerEntities { get; set; }

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

                // 获取获取数据库操作上下文，跳过贴了 [NotChangedListener] 特性的实体
                ChangeTrackerEntities = dbContext.ChangeTracker.Entries()
                    .Where(u => !u.Entity.GetType().IsDefined(typeof(SuppressChangedListenerAttribute), true) && (u.State == EntityState.Added || u.State == EntityState.Modified || u.State == EntityState.Deleted)).ToDictionary(u => u, u => u.GetDatabaseValues());

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

            // 清空跟踪实体
            ChangeTrackerEntities.Clear();
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

            // 清空跟踪实体
            ChangeTrackerEntities.Clear();
        }

        /// <summary>
        /// 附加实体改变监听
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="triggerMethodName"></param>
        /// <param name="changeTrackerEntities"></param>
        private static void AttachEntityChangedListener(DbContext dbContext, string triggerMethodName, Dictionary<EntityEntry, PropertyValues> changeTrackerEntities = null)
        {
            // 获取所有改变的类型
            var entityChangedTypes = AppDbContextBuilder.DbContextLocatorCorrelationTypes[typeof(TDbContextLocator)].EntityChangedTypes;
            if (!entityChangedTypes.Any()) return;

            // 遍历所有的改变的实体
            foreach (var trackerEntities in changeTrackerEntities)
            {
                var entryEntity = trackerEntities.Key;
                var entity = entryEntity.Entity;

                // 获取该实体类型监听配置
                var entitiesTypeByChanged = entityChangedTypes
                    .Where(u => u.GetInterfaces()
                        .Any(i => i.HasImplementedRawGeneric(typeof(IPrivateEntityChangedListener<>)) && i.GenericTypeArguments.Contains(entity.GetType())));

                if (!entitiesTypeByChanged.Any()) continue;

                // 通知所有的监听类型
                foreach (var entityChangedType in entitiesTypeByChanged)
                {
                    var OnChangeMethod = entityChangedType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                                    .Where(u => u.Name == triggerMethodName
                                                                        && u.GetParameters().Length > 0
                                                                        && u.GetParameters().First().ParameterType == entity.GetType())
                                                                    .FirstOrDefault();
                    if (OnChangeMethod == null) continue;

                    var instance = Activator.CreateInstance(entityChangedType);

                    // 对 OnChanged 进行特别处理
                    if (triggerMethodName.Equals("OnChanged"))
                    {
                        // 获取实体旧值
                        var oldEntity = trackerEntities.Value?.ToObject();

                        OnChangeMethod.Invoke(instance, new object[] { entity, oldEntity, dbContext, typeof(TDbContextLocator), entryEntity.State });
                    }
                    else
                    {
                        OnChangeMethod.Invoke(instance, new object[] { entity, dbContext, typeof(TDbContextLocator), entryEntity.State });
                    }
                }
            }
        }
    }
}