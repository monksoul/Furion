using Fur;
using Fur.DatabaseAccessor;
using Fur.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 数据库访问器服务拓展类
    /// </summary>
    [SkipScan]
    public static class DatabaseAccessorServiceCollectionExtensions
    {
        /// <summary>
        /// 添加数据库上下文
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置</param>
        /// <param name="migrationAssemblyName">迁移类库名称</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddDatabaseAccessor(this IServiceCollection services, Action<IServiceCollection> configure = null, string migrationAssemblyName = default)
        {
            // 设置迁移类库名称
            if (!string.IsNullOrEmpty(migrationAssemblyName)) Db.MigrationAssemblyName = migrationAssemblyName;

            // 配置数据库上下文
            configure?.Invoke(services);

            // 注册数据库上下文池
            services.TryAddScoped<IDbContextPool, DbContextPool>();

            // 注册 Sql 仓储
            services.TryAddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));

            // 注册 Sql 非泛型仓储
            services.TryAddScoped<ISqlRepository, SqlRepository>();

            // 注册多数据库上下文仓储
            services.TryAddScoped(typeof(IRepository<,>), typeof(EFCoreRepository<,>));

            // 注册泛型仓储
            services.TryAddScoped(typeof(IRepository<>), typeof(EFCoreRepository<>));

            // 注册主从库仓储
            services.TryAddScoped(typeof(IMSRepository<,>), typeof(MSRepository<,>));
            services.TryAddScoped(typeof(IMSRepository<,,>), typeof(MSRepository<,,>));
            services.TryAddScoped(typeof(IMSRepository<,,,>), typeof(MSRepository<,,,>));
            services.TryAddScoped(typeof(IMSRepository<,,,,>), typeof(MSRepository<,,,,>));
            services.TryAddScoped(typeof(IMSRepository<,,,,,>), typeof(MSRepository<,,,,,>));
            services.TryAddScoped(typeof(IMSRepository<,,,,,,>), typeof(MSRepository<,,,,,,>));
            services.TryAddScoped(typeof(IMSRepository<,,,,,,,>), typeof(MSRepository<,,,,,,,>));

            // 注册非泛型仓储
            services.TryAddScoped<IRepository, EFCoreRepository>();

            // 注册多数据库仓储
            services.TryAddScoped(typeof(IDbRepository<>), typeof(DbRepository<>));

            // 解析数据库上下文
            services.AddTransient(provider =>
            {
                DbContext dbContextResolve(Type locator, ITransient transient)
                {
                    // 判断定位器是否绑定了数据库上下文
                    var isRegistered = Penetrates.DbContextWithLocatorCached.TryGetValue(locator, out var dbContextType);
                    if (!isRegistered) throw new InvalidOperationException("The DbContext for locator binding was not found");

                    // 动态解析数据库上下文
                    var dbContext = provider.GetService(dbContextType) as DbContext;

                    // 实现动态数据库上下文功能，刷新 OnModelCreating
                    var dbContextAttribute = DbProvider.GetAppDbContextAttribute(dbContextType);
                    if (dbContextAttribute?.Mode == DbContextMode.Dynamic)
                    {
                        DynamicModelCacheKeyFactory.RebuildModels();
                    }

                    // 添加数据库上下文到池中
                    var dbContextPool = App.GetService<IDbContextPool>();
                    dbContextPool?.AddToPool(dbContext);

                    return dbContext;
                }
                return (Func<Type, ITransient, DbContext>)dbContextResolve;
            });

            services.AddScoped(provider =>
            {
                DbContext dbContextResolve(Type locator, IScoped scoped)
                {
                    // 判断定位器是否绑定了数据库上下文
                    var isRegistered = Penetrates.DbContextWithLocatorCached.TryGetValue(locator, out var dbContextType);
                    if (!isRegistered) throw new InvalidOperationException("The DbContext for locator binding was not found");

                    // 动态解析数据库上下文
                    var dbContext = provider.GetService(dbContextType) as DbContext;

                    // 实现动态数据库上下文功能，刷新 OnModelCreating
                    var dbContextAttribute = DbProvider.GetAppDbContextAttribute(dbContextType);
                    if (dbContextAttribute?.Mode == DbContextMode.Dynamic)
                    {
                        DynamicModelCacheKeyFactory.RebuildModels();
                    }

                    // 添加数据库上下文到池中
                    var dbContextPool = App.GetService<IDbContextPool>();
                    dbContextPool?.AddToPool(dbContext);

                    return dbContext;
                }
                return (Func<Type, IScoped, DbContext>)dbContextResolve;
            });

            // 注册 Sql 代理接口
            services.AddScopedDispatchProxyForInterface<SqlDispatchProxy, ISqlDispatchProxy>();

            // 注册全局工作单元过滤器
            services.Configure<MvcOptions>(options => options.Filters.Add<UnitOfWorkFilter>());

            return services;
        }

        /// <summary>
        /// 启动自定义租户类型
        /// </summary>
        /// <param name="services"></param>
        /// <param name="onTableTenantId">基于表的多租户Id名称</param>
        /// <returns></returns>
        public static IServiceCollection CustomizeMultiTenants(this IServiceCollection services, string onTableTenantId = default)
        {
            Db.CustomizeMultiTenants = true;
            if (!string.IsNullOrEmpty(onTableTenantId)) Db.OnTableTenantId = onTableTenantId;

            return services;
        }

        /// <summary>
        /// 注册数据库上下文
        /// </summary>
        /// <typeparam name="TDbContext">数据库上下文</typeparam>
        /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
        /// <param name="services">服务提供器</param>
        public static IServiceCollection RegisterDbContext<TDbContext, TDbContextLocator>(this IServiceCollection services)
        where TDbContext : DbContext
        where TDbContextLocator : class, IDbContextLocator
        {
            var dbContextLocatorType = (typeof(TDbContextLocator));

            // 将数据库上下文和定位器一一保存起来
            var isSuccess = Penetrates.DbContextWithLocatorCached.TryAdd(dbContextLocatorType, typeof(TDbContext));
            Penetrates.DbContextLocatorTypeCached.TryAdd(dbContextLocatorType.FullName, dbContextLocatorType);

            if (!isSuccess) throw new InvalidOperationException("The locator is bound to another DbContext");

            // 注册数据库上下文
            services.TryAddScoped<TDbContext>();

            return services;
        }
    }
}