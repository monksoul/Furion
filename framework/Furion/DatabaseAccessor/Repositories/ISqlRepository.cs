// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// Sql 操作仓储接口
    /// </summary>
    public interface ISqlRepository : ISqlRepository<MasterDbContextLocator>
        , ISqlExecutableRepository
        , ISqlReaderRepository
    {
    }

    /// <summary>
    /// Sql 操作仓储接口
    /// </summary>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    public interface ISqlRepository<TDbContextLocator> : IPrivateSqlRepository
        , ISqlExecutableRepository<TDbContextLocator>
        , ISqlReaderRepository<TDbContextLocator>
       where TDbContextLocator : class, IDbContextLocator
    {
    }

    /// <summary>
    /// 私有 Sql 仓储
    /// </summary>
    public interface IPrivateSqlRepository : IPrivateSqlExecutableRepository
        , IPrivateSqlReaderRepository
        , IPrivateRootRepository
    {
        /// <summary>
        /// 数据库操作对象
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        DbContext Context { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        dynamic DynamicContext { get; }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeDbContextLocator">数据库上下文定位器</typeparam>
        /// <returns>仓储</returns>
        ISqlRepository<TChangeDbContextLocator> Change<TChangeDbContextLocator>()
            where TChangeDbContextLocator : class, IDbContextLocator;

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetService<TService>()
            where TService : class;

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetRequiredService<TService>()
            where TService : class;

        /// <summary>
        /// 将仓储约束为特定仓储
        /// </summary>
        /// <typeparam name="TRestrainRepository">特定仓储</typeparam>
        /// <returns>TRestrainRepository</returns>
        TRestrainRepository Constraint<TRestrainRepository>()
            where TRestrainRepository : class, IPrivateRootRepository;

        /// <summary>
        /// 确保工作单元（事务）可用
        /// </summary>
        void EnsureTransaction();
    }
}