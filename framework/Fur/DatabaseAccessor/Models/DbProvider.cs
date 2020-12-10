using Fur.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 数据库提供器选项
    /// </summary>
    [SkipScan]
    public static class DbProvider
    {
        /// <summary>
        /// SqlServer 提供器程序集
        /// </summary>
        public const string SqlServer = "Microsoft.EntityFrameworkCore.SqlServer";

        /// <summary>
        /// Sqlite 提供器程序集
        /// </summary>
        public const string Sqlite = "Microsoft.EntityFrameworkCore.Sqlite";

        /// <summary>
        /// Cosmos 提供器程序集
        /// </summary>
        public const string Cosmos = "Microsoft.EntityFrameworkCore.Cosmos";

        /// <summary>
        /// 内存数据库 提供器程序集
        /// </summary>
        public const string InMemoryDatabase = "Microsoft.EntityFrameworkCore.InMemory";

        /// <summary>
        /// MySql 提供器程序集
        /// </summary>
        public const string MySql = "Pomelo.EntityFrameworkCore.MySql";

        /// <summary>
        /// PostgreSQL 提供器程序集
        /// </summary>
        public const string Npgsql = "Npgsql.EntityFrameworkCore.PostgreSQL";

        /// <summary>
        /// Oracle 提供器程序集
        /// </summary>
        public const string Oracle = "Citms.EntityFrameworkCore.Oracle";

        /// <summary>
        /// Firebird 提供器程序集
        /// </summary>
        public const string Firebird = "FirebirdSql.EntityFrameworkCore.Firebird";

        /// <summary>
        /// Dm 提供器程序集
        /// </summary>
        public const string Dm = "Microsoft.EntityFrameworkCore.Dm";

        /// <summary>
        /// 不支持存储过程的数据库
        /// </summary>
        internal static readonly string[] NotSupportStoredProcedureDatabases;

        /// <summary>
        /// 不支持函数的数据库
        /// </summary>
        internal static readonly string[] NotSupportFunctionDatabases;

        /// <summary>
        /// 不支持表值函数的数据库
        /// </summary>
        internal static readonly string[] NotSupportTableFunctionDatabases;

        /// <summary>
        /// 不支持环境事务的数据库
        /// </summary>
        internal static readonly string[] NotSupportTransactionScopeDatabase;

        /// <summary>
        /// 构造函数
        /// </summary>
        static DbProvider()
        {
            NotSupportStoredProcedureDatabases = new[]
            {
                Sqlite,
                InMemoryDatabase
            };

            NotSupportFunctionDatabases = new[]
            {
                Sqlite,
                InMemoryDatabase,
                Firebird,
                Dm,
            };

            NotSupportTableFunctionDatabases = new[]
            {
                Sqlite,
                InMemoryDatabase,
                MySql,
                Firebird,
                Dm
            };

            NotSupportTransactionScopeDatabase = new[]
            {
                Sqlite,
                InMemoryDatabase
            };

            DbContextAppDbContextAttributes = new ConcurrentDictionary<Type, AppDbContextAttribute>();
        }

        /// <summary>
        /// 获取数据库上下文连接字符串
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static string GetConnectionString<TDbContext>(string connectionString = default)
            where TDbContext : DbContext
        {
            if (!string.IsNullOrEmpty(connectionString)) return connectionString;

            // 如果没有配置数据库连接字符串，那么查找特性
            var dbContextAttribute = GetAppDbContextAttribute(typeof(TDbContext));
            if (dbContextAttribute == null) return default;

            // 获取特性连接字符串
            var connStr = dbContextAttribute.ConnectionString;

            if (string.IsNullOrEmpty(connStr)) return default;
            // 如果包含 = 符号，那么认为是连接字符串
            if (connStr.Contains("=")) return connStr;
            else
            {
                var configuration = App.Configuration;

                // 如果包含 : 符号，那么认为是一个 Key 路径
                if (connStr.Contains(":")) return configuration[connStr];
                else
                {
                    // 首先查找 DbConnectionString 键，如果没有找到，则当成 Key 去查找
                    var connStrValue = configuration.GetConnectionString(connStr);
                    return !string.IsNullOrEmpty(connStrValue) ? connStrValue : configuration[connStr];
                }
            }
        }

        /// <summary>
        /// 数据库上下文 [AppDbContext] 特性缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, AppDbContextAttribute> DbContextAppDbContextAttributes;

        /// <summary>
        /// 获取数据库上下文 [AppDbContext] 特性
        /// </summary>
        /// <param name="dbContexType"></param>
        /// <returns></returns>
        internal static AppDbContextAttribute GetAppDbContextAttribute(Type dbContexType)
        {
            return DbContextAppDbContextAttributes.GetOrAdd(dbContexType, Function);

            // 本地静态函数
            static AppDbContextAttribute Function(Type dbContextType)
            {
                if (!dbContextType.IsDefined(typeof(AppDbContextAttribute), true)) return default;

                var appDbContextAttribute = dbContextType.GetCustomAttribute<AppDbContextAttribute>(true);

                return appDbContextAttribute;
            }
        }

        /// <summary>
        /// 不支持操作类型
        /// </summary>
        private const string NotSupportException = "The database provider does not support {0} operations";

        /// <summary>
        /// 检查是否支持存储过程
        /// </summary>
        /// <param name="providerName">数据库提供器名词</param>
        /// <param name="commandType">命令类型</param>
        internal static void CheckStoredProcedureSupported(string providerName, CommandType commandType)
        {
            if (commandType == CommandType.StoredProcedure && NotSupportStoredProcedureDatabases.Contains(providerName))
            {
                throw new NotSupportedException(string.Format(NotSupportException, "stored procedure"));
            }
        }

        /// <summary>
        /// 检查是否支持函数
        /// </summary>
        /// <param name="providerName">数据库提供器名</param>
        /// <param name="dbFunctionType">数据库函数类型</param>
        internal static void CheckFunctionSupported(string providerName, DbFunctionType dbFunctionType)
        {
            if (NotSupportFunctionDatabases.Contains(providerName))
            {
                throw new NotSupportedException(string.Format(NotSupportException, "function"));
            }

            if (dbFunctionType == DbFunctionType.Table && NotSupportTableFunctionDatabases.Contains(providerName))
            {
                throw new NotSupportedException(string.Format(NotSupportException, "table function"));
            }
        }
    }
}