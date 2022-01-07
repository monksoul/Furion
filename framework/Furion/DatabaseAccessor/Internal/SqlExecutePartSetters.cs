// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 构建 Sql 字符串执行部件
    /// </summary>
    public sealed partial class SqlExecutePart
    {
        /// <summary>
        /// 设置 Sql 字符串
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlExecutePart SetSqlString(string sql)
        {
            if (!string.IsNullOrWhiteSpace(sql)) SqlString = sql;
            return this;
        }

        /// <summary>
        /// 设置 ADO.NET 超时时间
        /// </summary>
        /// <param name="timeout">单位秒</param>
        /// <returns></returns>
        public SqlExecutePart SetCommandTimeout(int timeout)
        {
            if (timeout > 0) Timeout = timeout;
            return this;
        }

        /// <summary>
        /// 设置数据库执行作用域
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public SqlExecutePart SetContextScoped(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null) ContextScoped = serviceProvider;
            return this;
        }

        /// <summary>
        /// 设置数据库上下文定位器
        /// </summary>
        /// <typeparam name="TDbContextLocator"></typeparam>
        /// <returns></returns>
        public SqlExecutePart Change<TDbContextLocator>()
            where TDbContextLocator : class, IDbContextLocator
        {
            return Change(typeof(TDbContextLocator));
        }

        /// <summary>
        /// 设置数据库上下文定位器
        /// </summary>
        /// <returns></returns>
        public SqlExecutePart Change(Type dbContextLocator)
        {
            if (dbContextLocator != null) DbContextLocator = dbContextLocator;
            return this;
        }
    }
}