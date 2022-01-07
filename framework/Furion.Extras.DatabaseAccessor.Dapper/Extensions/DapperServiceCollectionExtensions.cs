// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Dapper;
using System;
using System.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Dapper 拓展类
    /// </summary>
    public static class DapperServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 Dapper 拓展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="sqlProvider"> <see cref="Dapper.SqlProvider"/> 类型</param>
        /// <returns></returns>
        public static IServiceCollection AddDapper(this IServiceCollection services, string connectionString, string sqlProvider)
        {
            // 获取数据库类型
            var dbConnectionType = SqlProvider.GetDbConnectionType(sqlProvider);

            // 创建数据库连接对象
            services.AddScoped(u =>
            {
                var dbConnection = Activator.CreateInstance(dbConnectionType, new[] { connectionString }) as IDbConnection;
                if (dbConnection.State != ConnectionState.Open) dbConnection.Open();

                return dbConnection;
            });

            // 注册非泛型仓储
            services.AddScoped<IDapperRepository, DapperRepository>();

            // 注册 Dapper 仓储
            services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));

            return services;
        }
    }
}