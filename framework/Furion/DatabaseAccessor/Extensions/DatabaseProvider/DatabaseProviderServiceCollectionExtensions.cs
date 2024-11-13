// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

using Furion.DatabaseAccessor;
using Furion.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Concurrent;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Sqlite 数据库服务拓展
/// </summary>
[SuppressSniffer]
public static class DatabaseProviderServiceCollectionExtensions
{
    /// <summary>
    /// 添加默认数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <param name="services">服务</param>
    /// <param name="providerName">数据库提供器</param>
    /// <param name="optionBuilder"></param>
    /// <param name="connectionMetadata">支持数据库连接字符串，配置文件的 ConnectionStrings 中的Key或 配置文件的完整的配置路径，如果是内存数据库，则为数据库名称</param>
    /// <param name="poolSize">池大小</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDbPool<TDbContext>(this IServiceCollection services, string providerName = default, Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder = null, string connectionMetadata = default, int poolSize = 100, params IInterceptor[] interceptors)
        where TDbContext : DbContext
    {
        // 注册数据库上下文
        return services.AddDbPool<TDbContext, MasterDbContextLocator>(providerName, optionBuilder, connectionMetadata, poolSize, interceptors);
    }

    /// <summary>
    /// 添加默认数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <param name="services">服务</param>
    /// <param name="optionBuilder">自定义配置</param>
    /// <param name="poolSize">池大小</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDbPool<TDbContext>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder, int poolSize = 100, params IInterceptor[] interceptors)
        where TDbContext : DbContext
    {
        // 注册数据库上下文
        return services.AddDbPool<TDbContext, MasterDbContextLocator>(optionBuilder, poolSize, interceptors);
    }

    /// <summary>
    /// 添加其他数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    /// <param name="services">服务</param>
    /// <param name="providerName">数据库提供器</param>
    /// <param name="optionBuilder"></param>
    /// <param name="connectionMetadata">支持数据库连接字符串，配置文件的 ConnectionStrings 中的Key或 配置文件的完整的配置路径，如果是内存数据库，则为数据库名称</param>
    /// <param name="poolSize">池大小</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDbPool<TDbContext, TDbContextLocator>(this IServiceCollection services, string providerName = default, Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder = null, string connectionMetadata = default, int poolSize = 100, params IInterceptor[] interceptors)
        where TDbContext : DbContext
        where TDbContextLocator : class, IDbContextLocator
    {
        // 注册数据库上下文
        services.RegisterDbContext<TDbContext, TDbContextLocator>();

        // 配置数据库上下文
        var connStr = DbProvider.GetConnectionString<TDbContext>(connectionMetadata);
        services.AddDbContextPool<TDbContext>(Penetrates.ConfigureDbContext((serviceProvider, options) =>
        {
            var _options = ConfigureDatabase<TDbContext>(providerName, connStr, options);
            optionBuilder?.Invoke(serviceProvider, _options);
        }, interceptors), poolSize: poolSize);

        return services;
    }

    /// <summary>
    /// 添加其他数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    /// <param name="services">服务</param>
    /// <param name="optionBuilder">自定义配置</param>
    /// <param name="poolSize">池大小</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDbPool<TDbContext, TDbContextLocator>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder, int poolSize = 100, params IInterceptor[] interceptors)
        where TDbContext : DbContext
        where TDbContextLocator : class, IDbContextLocator
    {
        // 注册数据库上下文
        services.RegisterDbContext<TDbContext, TDbContextLocator>();

        // 配置数据库上下文
        services.AddDbContextPool<TDbContext>(Penetrates.ConfigureDbContext(optionBuilder, interceptors), poolSize: poolSize);

        return services;
    }

    /// <summary>
    ///  添加默认数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <param name="services">服务</param>
    /// <param name="providerName">数据库提供器</param>
    /// <param name="optionBuilder"></param>
    /// <param name="connectionMetadata">支持数据库连接字符串，配置文件的 ConnectionStrings 中的Key或 配置文件的完整的配置路径，如果是内存数据库，则为数据库名称</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDb<TDbContext>(this IServiceCollection services, string providerName = default, Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder = null, string connectionMetadata = default, params IInterceptor[] interceptors)
        where TDbContext : DbContext
    {
        // 注册数据库上下文
        return services.AddDb<TDbContext, MasterDbContextLocator>(providerName, optionBuilder, connectionMetadata, interceptors);
    }

    /// <summary>
    ///  添加默认数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <param name="services">服务</param>
    /// <param name="optionBuilder">自定义配置</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDb<TDbContext>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder, params IInterceptor[] interceptors)
        where TDbContext : DbContext
    {
        // 注册数据库上下文
        return services.AddDb<TDbContext, MasterDbContextLocator>(optionBuilder, interceptors);
    }

    /// <summary>
    /// 添加数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    /// <param name="services">服务</param>
    /// <param name="providerName">数据库提供器</param>
    /// <param name="optionBuilder"></param>
    /// <param name="connectionMetadata">支持数据库连接字符串，配置文件的 ConnectionStrings 中的Key或 配置文件的完整的配置路径，如果是内存数据库，则为数据库名称</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDb<TDbContext, TDbContextLocator>(this IServiceCollection services, string providerName = default, Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder = null, string connectionMetadata = default, params IInterceptor[] interceptors)
        where TDbContext : DbContext
        where TDbContextLocator : class, IDbContextLocator
    {
        // 注册数据库上下文
        services.RegisterDbContext<TDbContext, TDbContextLocator>();

        // 配置数据库上下文
        var connStr = DbProvider.GetConnectionString<TDbContext>(connectionMetadata);
        services.AddDbContext<TDbContext>(Penetrates.ConfigureDbContext((serviceProvider, options) =>
        {
            var _options = ConfigureDatabase<TDbContext>(providerName, connStr, options);
            optionBuilder?.Invoke(serviceProvider, _options);
        }, interceptors));

        return services;
    }

    /// <summary>
    /// 添加数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文</typeparam>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    /// <param name="services">服务</param>
    /// <param name="optionBuilder">自定义配置</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDb<TDbContext, TDbContextLocator>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder, params IInterceptor[] interceptors)
        where TDbContext : DbContext
        where TDbContextLocator : class, IDbContextLocator
    {
        // 注册数据库上下文
        services.RegisterDbContext<TDbContext, TDbContextLocator>();

        // 配置数据库上下文
        services.AddDbContext<TDbContext>(Penetrates.ConfigureDbContext(optionBuilder, interceptors));

        return services;
    }

    /// <summary>
    /// 配置数据库
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <param name="providerName">数据库提供器</param>
    /// <param name="connectionMetadata">支持数据库连接字符串，配置文件的 ConnectionStrings 中的Key或 配置文件的完整的配置路径，如果是内存数据库，则为数据库名称</param>
    /// <param name="options">数据库上下文选项构建器</param>
    private static DbContextOptionsBuilder ConfigureDatabase<TDbContext>(string providerName, string connectionMetadata, DbContextOptionsBuilder options)
         where TDbContext : DbContext
    {
        var dbContextOptionsBuilder = options;

        // 获取数据库上下文特性
        var dbContextAttribute = DbProvider.GetAppDbContextAttribute(typeof(TDbContext));
        if (!string.IsNullOrWhiteSpace(connectionMetadata))
        {
            providerName ??= dbContextAttribute?.ProviderName;

            // 解析数据库提供器信息
            (var name, var version) = ReadProviderInfo(providerName);
            providerName = name;

            // 调用对应数据库程序集
            var (UseMethod, MySqlVersion) = GetDatabaseProviderUseMethod(providerName, version);

            // 处理最新第三方 MySql 包兼容问题
            // https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/commit/83c699f5b747253dc1b6fa9c470f469467d77686
            if (DbProvider.IsDatabaseFor(providerName, DbProvider.MySql))
            {
                dbContextOptionsBuilder = UseMethod
                    .Invoke(null, new object[] { options, connectionMetadata, MySqlVersion, MigrationsAssemblyAction }) as DbContextOptionsBuilder;
            }
            // 处理 SqlServer 2005-2008 兼容问题
            else if (DbProvider.IsDatabaseFor(providerName, DbProvider.SqlServer) && (version == "2008" || version == "2005"))
            {
                // 替换工厂
                dbContextOptionsBuilder.ReplaceService<IQueryTranslationPostprocessorFactory, SqlServer2008QueryTranslationPostprocessorFactory>();

                dbContextOptionsBuilder = UseMethod
                    .Invoke(null, new object[] { options, connectionMetadata, MigrationsAssemblyAction }) as DbContextOptionsBuilder;
            }
            // 处理 Oracle 11 兼容问题
            else if (DbProvider.IsDatabaseFor(providerName, DbProvider.Oracle) && !string.IsNullOrWhiteSpace(version))
            {
                Action<IRelationalDbContextOptionsBuilderInfrastructure> oracleOptionsAction = options =>
                {
                    var optionsType = options.GetType();

                    // 处理版本号
                    optionsType.GetMethod("UseOracleSQLCompatibility")
                           .Invoke(options, new[] { version });

                    // 处理迁移程序集
                    optionsType.GetMethod("MigrationsAssembly")
                           .Invoke(options, new[] { Db.MigrationAssemblyName });
                };

                dbContextOptionsBuilder = UseMethod
                    .Invoke(null, new object[] { options, connectionMetadata, oracleOptionsAction }) as DbContextOptionsBuilder;
            }
            // 处理内存数据库
            else if (DbProvider.IsDatabaseFor(providerName, DbProvider.InMemoryDatabase))
            {
                dbContextOptionsBuilder = UseMethod
                    .Invoke(null, new object[] { options, connectionMetadata, null }) as DbContextOptionsBuilder;
            }
            else
            {
                dbContextOptionsBuilder = UseMethod
                    .Invoke(null, new object[] { options, connectionMetadata, MigrationsAssemblyAction }) as DbContextOptionsBuilder;
            }
        }

        // 解决分表分库
        if (dbContextAttribute?.Mode == DbContextMode.Dynamic) dbContextOptionsBuilder
              .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();

        return dbContextOptionsBuilder;
    }

    /// <summary>
    /// 数据库提供器 UseXXX 方法缓存集合
    /// </summary>
    private static readonly ConcurrentDictionary<string, (MethodInfo, object)> DatabaseProviderUseMethodCollection;

    /// <summary>
    /// 配置Code First 程序集 Action委托
    /// </summary>
    private static readonly Action<IRelationalDbContextOptionsBuilderInfrastructure> MigrationsAssemblyAction;

    /// <summary>
    /// 静态构造方法
    /// </summary>
    static DatabaseProviderServiceCollectionExtensions()
    {
        DatabaseProviderUseMethodCollection = new ConcurrentDictionary<string, (MethodInfo, object)>();
        MigrationsAssemblyAction = options =>
        {
            var optionsType = options.GetType();

            optionsType.GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(u => u.Name == "MigrationsAssembly" && u.GetParameters().Length == 1 && u.GetParameters().First().ParameterType == typeof(string))
                   .Invoke(options, new[] { Db.MigrationAssemblyName });

            // 解决 MySQL/SqlServer/PostgreSQL 有时候出现短暂连接失败问题（v4.8.1.7 版本关闭）
            // https://learn.microsoft.com/zh-cn/ef/core/miscellaneous/connection-resiliency
            //var enableRetryOnFailureMethod = optionsType.GetMethod("EnableRetryOnFailure", new[]
            //{
            //    typeof(int),typeof(TimeSpan),typeof(IEnumerable<int>)
            //});

            //enableRetryOnFailureMethod?.Invoke(options, new object[]
            //{
            //    5,TimeSpan.FromSeconds(30),new int[] { 2 }
            //});
        };
    }

    /// <summary>
    /// 获取数据库提供器对应的 useXXX 方法
    /// </summary>
    /// <param name="providerName">数据库提供器</param>
    /// <param name="version"></param>
    /// <returns></returns>
    private static (MethodInfo UseMethod, object MySqlVersion) GetDatabaseProviderUseMethod(string providerName, string version)
    {
        return DatabaseProviderUseMethodCollection.GetOrAdd(providerName, Function(providerName, version));

        // 本地静态方法
        static (MethodInfo, object) Function(string providerName, string version)
        {
            // 处理最新 MySql 包兼容问题
            // https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/commit/83c699f5b747253dc1b6fa9c470f469467d77686
            object mySqlVersionInstance = default;

            // 加载对应的数据库提供器程序集
            var databaseProviderAssembly = Reflect.GetAssembly(providerName);

            // 数据库提供器服务拓展类型名
            var databaseProviderServiceExtensionTypeName = providerName switch
            {
                DbProvider.SqlServer => "SqlServerDbContextOptionsExtensions",
                DbProvider.Sqlite => "SqliteDbContextOptionsBuilderExtensions",
                DbProvider.Cosmos => "CosmosDbContextOptionsExtensions",
                DbProvider.InMemoryDatabase => "InMemoryDbContextOptionsExtensions",
                DbProvider.MySql => "MySqlDbContextOptionsBuilderExtensions",
                DbProvider.MySqlOfficial => "MySQLDbContextOptionsExtensions",
                DbProvider.Npgsql => "NpgsqlDbContextOptionsBuilderExtensions",
                DbProvider.Oracle => "OracleDbContextOptionsExtensions",
                DbProvider.Firebird => "FbDbContextOptionsBuilderExtensions",
                DbProvider.Dm => "DmDbContextOptionsExtensions",
                _ => null
            };

            // 加载拓展类型
            var databaseProviderServiceExtensionType = Reflect.GetType(databaseProviderAssembly, $"Microsoft.EntityFrameworkCore.{databaseProviderServiceExtensionTypeName}");

            // useXXX方法名
            var useMethodName = providerName switch
            {
                DbProvider.SqlServer => $"Use{nameof(DbProvider.SqlServer)}",
                DbProvider.Sqlite => $"Use{nameof(DbProvider.Sqlite)}",
                DbProvider.Cosmos => $"Use{nameof(DbProvider.Cosmos)}",
                DbProvider.InMemoryDatabase => $"Use{nameof(DbProvider.InMemoryDatabase)}",
                DbProvider.MySql => $"Use{nameof(DbProvider.MySql)}",
                DbProvider.MySqlOfficial => $"UseMySQL",
                DbProvider.Npgsql => $"Use{nameof(DbProvider.Npgsql)}",
                DbProvider.Oracle => $"Use{nameof(DbProvider.Oracle)}",
                DbProvider.Firebird => $"Use{nameof(DbProvider.Firebird)}",
                DbProvider.Dm => $"Use{nameof(DbProvider.Dm)}",
                _ => null
            };

            // 获取UseXXX方法
            MethodInfo useMethod;

            // 处理最新 MySql 第三方包兼容问题
            // https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/commit/83c699f5b747253dc1b6fa9c470f469467d77686
            if (DbProvider.IsDatabaseFor(providerName, DbProvider.MySql))
            {
                useMethod = databaseProviderServiceExtensionType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(u => u.Name == useMethodName && !u.IsGenericMethod && u.GetParameters().Length == 4 && u.GetParameters()[1].ParameterType == typeof(string));

                // 解析mysql版本类型
                var mysqlVersionType = Reflect.GetType(databaseProviderAssembly, "Microsoft.EntityFrameworkCore.MySqlServerVersion");
                mySqlVersionInstance = Activator.CreateInstance(mysqlVersionType, new object[] { new Version(version ?? "8.0.22") });
            }
            else
            {
                useMethod = databaseProviderServiceExtensionType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(u => u.Name == useMethodName && !u.IsGenericMethod && u.GetParameters().Length == 3 && u.GetParameters()[1].ParameterType == typeof(string));
            }

            return (useMethod, mySqlVersionInstance);
        }
    }

    /// <summary>
    /// 解析数据库提供器信息
    /// </summary>
    /// <param name="providerName"></param>
    /// <returns></returns>
    private static (string name, string version) ReadProviderInfo(string providerName)
    {
        // 解析真实的数据库提供器
        var providerNameAndVersion = providerName.Split('@', StringSplitOptions.RemoveEmptyEntries);
        providerName = providerNameAndVersion.First();

        var providerVersion = providerNameAndVersion.Length > 1 ? providerNameAndVersion[1] : default;
        return (providerName, providerVersion);
    }
}