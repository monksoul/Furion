// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Dapper
{
    /// <summary>
    /// Sql 类型
    /// </summary>
    public static class SqlProvider
    {
        /// <summary>
        /// SqlServer 提供器程序集
        /// </summary>
        public const string SqlServer = "Microsoft.Data.SqlClient";

        /// <summary>
        /// Sqlite 提供器程序集
        /// </summary>
        public const string Sqlite = "Microsoft.Data.Sqlite";

        /// <summary>
        /// MySql 提供器程序集
        /// </summary>
        public const string MySql = "MySql.Data";

        /// <summary>
        /// PostgreSQL 提供器程序集
        /// </summary>
        public const string Npgsql = "Npgsql";

        /// <summary>
        /// Oracle 提供器程序集
        /// </summary>
        public const string Oracle = "Oracle.ManagedDataAccess";

        /// <summary>
        /// Firebird 提供器程序集
        /// </summary>
        public const string Firebird = "FirebirdSql.Data.FirebirdClient";

        /// <summary>
        /// 数据库提供器连接对象类型集合
        /// </summary>
        internal static readonly ConcurrentDictionary<string, Type> SqlProviderDbConnectionTypeCollection;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SqlProvider()
        {
            SqlProviderDbConnectionTypeCollection = new ConcurrentDictionary<string, Type>();
        }

        /// <summary>
        /// 获取数据库连接对象类型
        /// </summary>
        /// <param name="sqlProvider"></param>
        /// <returns></returns>
        internal static Type GetDbConnectionType(string sqlProvider)
        {
            return SqlProviderDbConnectionTypeCollection.GetOrAdd(sqlProvider, Function);

            // 本地静态方法
            static Type Function(string sqlProvider)
            {
                // 加载对应的数据库提供器程序集
                var databaseProviderAssembly = Assembly.Load(sqlProvider);

                // 获取对应数据库连接对象
                var databaseDbConnectionTypeName = sqlProvider switch
                {
                    SqlServer => "Microsoft.Data.SqlClient.SqlConnection",
                    Sqlite => "Microsoft.Data.Sqlite.SqliteConnection",
                    MySql => "MySql.Data.MySqlClient.MySqlConnection",
                    Npgsql => "Npgsql.NpgsqlConnection",
                    Oracle => "Oracle.ManagedDataAccess.Client.OracleConnection",
                    Firebird => "FirebirdSql.Data.FirebirdClient.FbConnection",
                    _ => null
                };

                // 加载数据库连接对象类型
                var dbConnectionType = databaseProviderAssembly.GetType(databaseDbConnectionTypeName);

                return dbConnectionType;
            }
        }
    }
}