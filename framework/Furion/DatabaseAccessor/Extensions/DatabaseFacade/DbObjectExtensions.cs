// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.Templates.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// DatabaseFacade 拓展类
    /// </summary>
    [SuppressSniffer]
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
        /// 是否打印数据库连接信息到 MiniProfiler 中
        /// </summary>
        private static readonly bool IsPrintDbConnectionInfo;

        /// <summary>
        /// 是否记录 EFCore 执行 sql 命令打印日志
        /// </summary>
        private static readonly bool IsLogEntityFrameworkCoreSqlExecuteCommand;

        /// <summary>
        /// 构造函数
        /// </summary>
        static DbObjectExtensions()
        {
            IsDevelopment = App.HostEnvironment?.IsDevelopment() ?? false;

            var appsettings = App.Settings;
            IsPrintDbConnectionInfo = appsettings.PrintDbConnectionInfo.Value;
            IsLogEntityFrameworkCoreSqlExecuteCommand = appsettings.OutputOriginalSqlExecuteLog.Value;
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
            SetDbParameters(databaseFacade.ProviderName, ref dbCommand, parameters);

            // 打开数据库连接
            OpenConnection(databaseFacade, dbConnection, dbCommand);

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
            SetDbParameters(databaseFacade.ProviderName, ref dbCommand, model, out var dbParameters);

            // 打开数据库连接
            OpenConnection(databaseFacade, dbConnection, dbCommand);

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
            SetDbParameters(databaseFacade.ProviderName, ref dbCommand, parameters);

            // 打开数据库连接
            await OpenConnectionAsync(databaseFacade, dbConnection, dbCommand, cancellationToken);

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
            SetDbParameters(databaseFacade.ProviderName, ref dbCommand, model, out var dbParameters);

            // 打开数据库连接
            await OpenConnectionAsync(databaseFacade, dbConnection, dbCommand, cancellationToken);

            // 返回
            return (dbConnection, dbCommand, dbParameters);
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
            // 判断是否是关系型数据库
            if (!databaseFacade.IsRelational()) throw new InvalidOperationException("Only relational databases support ADO.NET operations.");

            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));

            // 支持读取配置渲染
            var realSql = sql.Render();

            // 检查是否支持存储过程
            DbProvider.CheckStoredProcedureSupported(databaseFacade.ProviderName, commandType);

            // 判断是否启用 MiniProfiler 组件，如果有，则包装链接
            var dbConnection = databaseFacade.GetDbConnection();

            // 创建数据库命令对象
            var dbCommand = dbConnection.CreateCommand();

            // 设置基本参数
            dbCommand.Transaction = databaseFacade.CurrentTransaction?.GetDbTransaction();
            dbCommand.CommandType = commandType;
            dbCommand.CommandText = realSql;

            // 设置超时
            var commandTimeout = databaseFacade.GetCommandTimeout();
            if (commandTimeout != null) dbCommand.CommandTimeout = commandTimeout.Value;

            // 返回
            return (dbConnection, dbCommand);
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="dbConnection">数据库连接对象</param>
        /// <param name="dbCommand"></param>
        private static void OpenConnection(DatabaseFacade databaseFacade, DbConnection dbConnection, DbCommand dbCommand)
        {
            // 判断连接字符串是否关闭，如果是，则开启
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();

                // 打印数据库连接信息到 MiniProfiler
                PrintConnectionToMiniProfiler(databaseFacade, dbConnection, false);
            }

            // 记录 Sql 执行命令日志
            LogSqlExecuteCommand(databaseFacade, dbCommand);
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="dbConnection">数据库连接对象</param>
        /// <param name="dbCommand"></param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns></returns>
        private static async Task OpenConnectionAsync(DatabaseFacade databaseFacade, DbConnection dbConnection, DbCommand dbCommand, CancellationToken cancellationToken = default)
        {
            // 判断连接字符串是否关闭，如果是，则开启
            if (dbConnection.State == ConnectionState.Closed)
            {
                await dbConnection.OpenAsync(cancellationToken);

                // 打印数据库连接信息到 MiniProfiler
                PrintConnectionToMiniProfiler(databaseFacade, dbConnection, true);
            }

            // 记录 Sql 执行命令日志
            LogSqlExecuteCommand(databaseFacade, dbCommand);
        }

        /// <summary>
        /// 设置数据库命令对象参数
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="dbCommand">数据库命令对象</param>
        /// <param name="parameters">命令参数</param>
        private static void SetDbParameters(string providerName, ref DbCommand dbCommand, DbParameter[] parameters = null)
        {
            if (parameters == null || parameters.Length == 0) return;

            // 添加命令参数前缀
            foreach (var parameter in parameters)
            {
                parameter.ParameterName = DbHelpers.FixSqlParameterPlaceholder(providerName, parameter.ParameterName);
                dbCommand.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// 设置数据库命令对象参数
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="dbCommand">数据库命令对象</param>
        /// <param name="model">参数模型</param>
        /// <param name="dbParameters">命令参数</param>
        private static void SetDbParameters(string providerName, ref DbCommand dbCommand, object model, out DbParameter[] dbParameters)
        {
            dbParameters = DbHelpers.ConvertToDbParameters(model, dbCommand);
            SetDbParameters(providerName, ref dbCommand, dbParameters);
        }

        /// <summary>
        /// 打印数据库连接信息到 MiniProfiler
        /// </summary>
        /// <param name="databaseFacade">ADO.NET 数据库对象</param>
        /// <param name="dbConnection">数据库连接对象</param>
        /// <param name="isAsync"></param>
        private static void PrintConnectionToMiniProfiler(DatabaseFacade databaseFacade, DbConnection dbConnection, bool isAsync)
        {
            // 打印数据库连接信息
            App.PrintToMiniProfiler("sql", $"Open{(isAsync ? "Async" : string.Empty)}", $"Connection Open{(isAsync ? "Async" : string.Empty)}()");

            if (IsDevelopment && IsPrintDbConnectionInfo)
            {
                var connectionId = databaseFacade.GetService<IRelationalConnection>()?.ConnectionId;
                // 打印连接信息消息
                App.PrintToMiniProfiler(MiniProfilerCategory, "Information", $"[Connection Id: {connectionId}] / [Database: {dbConnection.Database}] / [Connection String: {dbConnection.ConnectionString}]");
            }
        }

        /// <summary>
        /// 输出原始 Sql 执行日志（ADO.NET）
        /// </summary>
        /// <param name="databaseFacade"></param>
        /// <param name="dbCommand"></param>
        private static void LogSqlExecuteCommand(DatabaseFacade databaseFacade, DbCommand dbCommand)
        {
            // 打印执行 SQL
            App.PrintToMiniProfiler("sql", "Execution", dbCommand.CommandText);

            // 判断是否启用
            if (!IsLogEntityFrameworkCoreSqlExecuteCommand) return;

            // 构建日志内容
            var sqlLogBuilder = new StringBuilder();
            sqlLogBuilder.Append(@"Executed DbCommand (NaN) ");
            sqlLogBuilder.Append(@" [Parameters=[");

            // 拼接命令参数
            var parameters = dbCommand.Parameters;
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var parameterType = parameter.GetType();

                // 处理 OracleParameter 参数打印
                var dbType = parameterType.FullName.Equals("Oracle.ManagedDataAccess.Client.OracleParameter", StringComparison.OrdinalIgnoreCase)
                    ? parameterType.GetProperty("OracleDbType").GetValue(parameter) : parameter.DbType;

                sqlLogBuilder.Append($"{parameter.ParameterName}='{parameter.Value}' (Size = {parameter.Size}) (DbType = {dbType})");
                if (i < parameters.Count - 1) sqlLogBuilder.Append(", ");
            }

            sqlLogBuilder.Append(@$"], CommandType='{dbCommand.CommandType}', CommandTimeout='{dbCommand.CommandTimeout}']");
            sqlLogBuilder.Append("\r\n");
            sqlLogBuilder.Append(dbCommand.CommandType == CommandType.StoredProcedure ? "EXEC " + dbCommand.CommandText : dbCommand.CommandText);

            // 打印日志
            var logger = databaseFacade.GetService<ILogger<Microsoft.EntityFrameworkCore.Database.SqlExecuteCommand>>();
            logger.LogInformation(sqlLogBuilder.ToString());
        }
    }
}