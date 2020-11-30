using Fur.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// DatabaseFacade 拓展类
    /// </summary>
    [SkipScan]
    public static class DbObjectExtensions
    {
        /// <summary>
        /// MiniProfiler 分类名
        /// </summary>
        private const string MiniProfilerCategory = "connection";

        /// <summary>
        /// 是否是开发环境
        /// </summary>
        private static readonly bool IsDevelopment;

        /// <summary>
        /// MiniProfiler 组件状态
        /// </summary>
        private static readonly bool InjectMiniProfiler;

        /// <summary>
        /// 构造函数
        /// </summary>
        static DbObjectExtensions()
        {
            IsDevelopment = App.WebHostEnvironment.IsDevelopment();
            InjectMiniProfiler = App.Settings.InjectMiniProfiler.Value;
        }

        /// <summary>
        /// 初始化数据库命令对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand)</returns>
        public static (DbConnection dbConnection, DbCommand dbCommand) PrepareDbCommand(this DatabaseFacade databaseFacade, string sql, DbParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            // 创建数据库连接对象及数据库命令对象
            var (dbConnection, dbCommand) = databaseFacade.CreateDbCommand(sql, commandType);
            SetDbParameters(ref dbCommand, parameters);

            // 打开数据库连接
            OpenConnection(databaseFacade, dbConnection);

            // 返回
            return (dbConnection, dbCommand);
        }

        /// <summary>
        /// 初始化数据库命令对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">命令模型</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand, DbParameter[] dbParameters)</returns>
        public static (DbConnection dbConnection, DbCommand dbCommand, DbParameter[] dbParameters) PrepareDbCommand(this DatabaseFacade databaseFacade, string sql, object model, CommandType commandType = CommandType.Text)
        {
            // 创建数据库连接对象及数据库命令对象
            var (dbConnection, dbCommand) = databaseFacade.CreateDbCommand(sql, commandType);
            SetDbParameters(ref dbCommand, model, out var dbParameters);

            // 打开数据库连接
            OpenConnection(databaseFacade, dbConnection);

            // 返回
            return (dbConnection, dbCommand, dbParameters);
        }

        /// <summary>
        /// 初始化数据库命令对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand)</returns>
        public static async Task<(DbConnection dbConnection, DbCommand dbCommand)> PrepareDbCommandAsync(this DatabaseFacade databaseFacade, string sql, DbParameter[] parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            // 创建数据库连接对象及数据库命令对象
            var (dbConnection, dbCommand) = databaseFacade.CreateDbCommand(sql, commandType);
            SetDbParameters(ref dbCommand, parameters);

            // 打开数据库连接
            await OpenConnectionAsync(databaseFacade, dbConnection, cancellationToken);

            // 返回
            return (dbConnection, dbCommand);
        }

        /// <summary>
        /// 初始化数据库命令对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">命令模型</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand, DbParameter[] dbParameters)</returns>
        public static async Task<(DbConnection dbConnection, DbCommand dbCommand, DbParameter[] dbParameters)> PrepareDbCommandAsync(this DatabaseFacade databaseFacade, string sql, object model, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            // 创建数据库连接对象及数据库命令对象
            var (dbConnection, dbCommand) = databaseFacade.CreateDbCommand(sql, commandType);
            SetDbParameters(ref dbCommand, model, out var dbParameters);

            // 打开数据库连接
            await OpenConnectionAsync(databaseFacade, dbConnection, cancellationToken);

            // 返回
            return (dbConnection, dbCommand, dbParameters);
        }

        /// <summary>
        /// 初始化数据库适配器对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter)</returns>
        public static (DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter) PrepareDbDbDataAdapter(this DatabaseFacade databaseFacade, string sql, DbParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            // 创建数据库连接对象、数据库命令对象和数据库适配器对象
            var (dbConnection, dbCommand, dbDataAdapter) = databaseFacade.CreateDbDataAdapter(sql, commandType);
            SetDbParameters(ref dbCommand, parameters);

            // 打开数据库连接
            OpenConnection(databaseFacade, dbConnection);

            // 返回
            return (dbConnection, dbCommand, dbDataAdapter);
        }

        /// <summary>
        /// 初始化数据库适配器对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">命令模型</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter, DbParameter[] dbParameters)</returns>
        public static (DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter, DbParameter[] dbParameters) PrepareDbDbDataAdapter(this DatabaseFacade databaseFacade, string sql, object model, CommandType commandType = CommandType.Text)
        {
            // 创建数据库连接对象、数据库命令对象和数据库适配器对象
            var (dbConnection, dbCommand, dbDataAdapter) = databaseFacade.CreateDbDataAdapter(sql, commandType);
            SetDbParameters(ref dbCommand, model, out var dbParameters);

            // 打开数据库连接
            OpenConnection(databaseFacade, dbConnection);

            // 返回
            return (dbConnection, dbCommand, dbDataAdapter, dbParameters);
        }

        /// <summary>
        /// 初始化数据库适配器对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter)</returns>
        public static async Task<(DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter)> PrepareDbDbDataAdapterAsync(this DatabaseFacade databaseFacade, string sql, DbParameter[] parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            // 创建数据库连接对象、数据库命令对象和数据库适配器对象
            var (dbConnection, dbCommand, dbDataAdapter) = databaseFacade.CreateDbDataAdapter(sql, commandType);
            SetDbParameters(ref dbCommand, parameters);

            // 打开数据库连接
            await OpenConnectionAsync(databaseFacade, dbConnection, cancellationToken);

            // 返回
            return (dbConnection, dbCommand, dbDataAdapter);
        }

        /// <summary>
        /// 初始化数据库适配器对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter, DbParameter[] dbParameters)</returns>
        public static async Task<(DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter, DbParameter[] dbParameters)> PrepareDbDbDataAdapterAsync(this DatabaseFacade databaseFacade, string sql, object model, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            // 创建数据库连接对象、数据库命令对象和数据库适配器对象
            var (dbConnection, dbCommand, dbDataAdapter) = databaseFacade.CreateDbDataAdapter(sql, commandType);
            SetDbParameters(ref dbCommand, model, out var dbParameters);

            // 打开数据库连接
            await OpenConnectionAsync(databaseFacade, dbConnection, cancellationToken);

            // 返回
            return (dbConnection, dbCommand, dbDataAdapter, dbParameters);
        }

        /// <summary>
        /// 创建数据库命令对象
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand)</returns>
        private static (DbConnection dbConnection, DbCommand dbCommand) CreateDbCommand(this DatabaseFacade databaseFacade, string sql, CommandType commandType = CommandType.Text)
        {
            // 检查是否支持存储过程
            DbProvider.CheckStoredProcedureSupported(databaseFacade.ProviderName, commandType);

            // 判断是否启用 MiniProfiler 组件，如果有，则包装链接
            var dbConnection = InjectMiniProfiler ? new ProfiledDbConnection(databaseFacade.GetDbConnection(), MiniProfiler.Current) : databaseFacade.GetDbConnection();

            // 创建数据库命令对象
            var dbCommand = dbConnection.CreateCommand();

            // 设置基本参数
            dbCommand.CommandType = commandType;
            dbCommand.CommandText = sql;

            // 返回
            return (dbConnection, dbCommand);
        }

        /// <summary>
        /// 创建数据库适配器
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="sql">sql 语句</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>(DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter)</returns>
        private static (DbConnection dbConnection, DbCommand dbCommand, DbDataAdapter dbDataAdapter) CreateDbDataAdapter(this DatabaseFacade databaseFacade, string sql, CommandType commandType = CommandType.Text)
        {
            // 检查是否支持存储过程
            DbProvider.CheckStoredProcedureSupported(databaseFacade.ProviderName, commandType);

            // 获取数据库连接字符串
            var dbConnection = databaseFacade.GetDbConnection();

            // 解析数据库提供器
            var dbProviderFactory = DbProviderFactories.GetFactory(dbConnection);

            // 判断是否启用 MiniProfiler 组件，如果有，则包装链接和数据库提供器工厂
            var profiledDbProviderFactory = InjectMiniProfiler ? new ProfiledDbProviderFactory(dbProviderFactory, true) : dbProviderFactory;

            // 创建数据库连接对象及数据库命令对象
            var (_dbConnection, dbCommand) = databaseFacade.CreateDbCommand(sql, commandType);
            dbConnection = _dbConnection;

            // 创建数据适配器并设置查询命令对象
            var dbDataAdapter = profiledDbProviderFactory.CreateDataAdapter();
            dbDataAdapter.SelectCommand = dbCommand;

            // 返回
            return (dbConnection, dbCommand, dbDataAdapter);
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="dbConnection">数据库连接对象</param>
        private static void OpenConnection(DatabaseFacade databaseFacade, DbConnection dbConnection)
        {
            // 判断连接字符串是否关闭，如果是，则开启
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();

                // 打印数据库连接信息到 MiniProfiler
                PrintConnectionToMiniProfiler(databaseFacade, dbConnection);
            }
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="dbConnection">数据库连接对象</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns></returns>
        private static async Task OpenConnectionAsync(DatabaseFacade databaseFacade, DbConnection dbConnection, CancellationToken cancellationToken = default)
        {
            // 判断连接字符串是否关闭，如果是，则开启
            if (dbConnection.State == ConnectionState.Closed)
            {
                await dbConnection.OpenAsync(cancellationToken);

                // 打印数据库连接信息到 MiniProfiler
                PrintConnectionToMiniProfiler(databaseFacade, dbConnection);
            }
        }

        /// <summary>
        /// 设置数据库命令对象参数
        /// </summary>
        /// <param name="dbCommand">数据库命令对象</param>
        /// <param name="parameters">命令参数</param>
        private static void SetDbParameters(ref DbCommand dbCommand, DbParameter[] parameters = null)
        {
            if (parameters == null || parameters.Length == 0) return;

            // 添加 @ 前缀
            foreach (var parameter in parameters)
            {
                if (!parameter.ParameterName.Contains("@"))
                {
                    parameter.ParameterName = $"@{parameter.ParameterName}";
                }
                dbCommand.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// 设置数据库命令对象参数
        /// </summary>
        /// <param name="dbCommand">数据库命令对象</param>
        /// <param name="model">参数模型</param>
        /// <param name="dbParameters">命令参数</param>
        private static void SetDbParameters(ref DbCommand dbCommand, object model, out DbParameter[] dbParameters)
        {
            dbParameters = DbHelpers.ConvertToDbParameters(model, dbCommand);
            SetDbParameters(ref dbCommand, dbParameters);
        }

        /// <summary>
        /// 打印数据库连接信息到 MiniProfiler
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="dbConnection">数据库连接对象</param>
        private static void PrintConnectionToMiniProfiler(DatabaseFacade databaseFacade, DbConnection dbConnection)
        {
            if (InjectMiniProfiler && IsDevelopment)
            {
                var connectionId = databaseFacade.GetService<IRelationalConnection>().ConnectionId;
                // 打印连接信息消息
                App.PrintToMiniProfiler(MiniProfilerCategory, "Information", $"[Connection Id: {connectionId}] / [Database: {dbConnection.Database}] / [Connection String: {dbConnection.ConnectionString}]");
            }
        }
    }
}