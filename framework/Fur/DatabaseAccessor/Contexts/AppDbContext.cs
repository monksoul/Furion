using Fur.DependencyInjection;
using Fur.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Caching.Memory;
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
            // 定义数据库上下文提交更改事件
            SavingChanges += SavingChangesEvent;
            SavedChanges += SavedChangesEvent;
            SaveChangesFailed += SaveChangesFailedEvent;
        }

        /// <summary>
        /// 数据库上下文提交更改之前执行事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        protected virtual void SavingChangesEvent(object sender, SavingChangesEventArgs e)
        {
        }

        /// <summary>
        /// 数据库上下文提交更改成功执行事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        protected virtual void SavedChangesEvent(object sender, SavedChangesEventArgs e)
        {
        }

        /// <summary>
        /// 数据库上下文提交更改失败执行事件
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        protected virtual void SaveChangesFailedEvent(object sender, SaveChangesFailedEventArgs e)
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
                    var tenantDbContext = Db.GetDbContext<MultiTenantDbContextLocator>();
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
    }
}