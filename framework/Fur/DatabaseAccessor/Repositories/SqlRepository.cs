using Fur.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// Sql 操作仓储实现
    /// </summary>
    [SkipScan]
    public partial class SqlRepository : SqlRepository<MasterDbContextLocator>, ISqlRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContextResolve">数据库上下文解析器</param>
        /// <param name="serviceProvider">服务提供器</param>
        public SqlRepository(
            Func<Type, IScoped, DbContext> dbContextResolve
            , IServiceProvider serviceProvider) : base(dbContextResolve, serviceProvider)
        {
        }
    }

    /// <summary>
    /// Sql 操作仓储实现
    /// </summary>
    [SkipScan]
    public partial class SqlRepository<TDbContextLocator> : ISqlRepository<TDbContextLocator>
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 服务提供器
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContextResolve">数据库上下文解析器</param>
        /// <param name="serviceProvider">服务提供器</param>
        public SqlRepository(
            Func<Type, IScoped, DbContext> dbContextResolve
            , IServiceProvider serviceProvider)
        {
            // 解析数据库上下文
            var dbContext = dbContextResolve(typeof(TDbContextLocator), default);
            DynamicDbContext = DbContext = dbContext;

            // 初始化数据库相关数据
            Database = dbContext.Database;

            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 数据库操作对象
        /// </summary>
        public virtual DatabaseFacade Database { get; }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        public virtual DbContext DbContext { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        public virtual dynamic DynamicDbContext { get; }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        public virtual ISqlRepository<TChangeDbContextLocator> Change<TChangeDbContextLocator>()
             where TChangeDbContextLocator : class, IDbContextLocator
        {
            return _serviceProvider.GetService<ISqlRepository<TChangeDbContextLocator>>();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public virtual TService GetService<TService>()
            where TService : class
        {
            return _serviceProvider.GetService<TService>();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public virtual TService GetRequiredService<TService>()
            where TService : class
        {
            return _serviceProvider.GetRequiredService<TService>();
        }

        /// <summary>
        /// 将仓储约束为特定仓储
        /// </summary>
        /// <typeparam name="TRestrainRepository">特定仓储</typeparam>
        /// <returns>TRestrainRepository</returns>
        public virtual TRestrainRepository Constraint<TRestrainRepository>()
            where TRestrainRepository : class, IPrivateRepository
        {
            var type = typeof(TRestrainRepository);
            if (!type.IsInterface || typeof(IPrivateRepository) == type || type.Name.Equals(nameof(IRepository)) || (type.IsGenericType && type.GetGenericTypeDefinition().Name.Equals(nameof(IRepository))))
            {
                throw new InvalidCastException("Invalid type conversion");
            }

            return this as TRestrainRepository;
        }
    }
}