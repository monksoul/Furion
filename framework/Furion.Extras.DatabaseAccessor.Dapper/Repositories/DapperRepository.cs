// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Dapper.Contrib.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper
{
    /// <summary>
    /// 非泛型 Dapper 仓储
    /// </summary>
    public partial class DapperRepository : IDapperRepository
    {
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        private readonly IDbConnection _db;

        /// <summary>
        /// 服务提供器
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="db"></param>
        public DapperRepository(
            IServiceProvider serviceProvider
            , IDbConnection db)
        {
            _serviceProvider = serviceProvider;
            _db = db;
        }

        /// <summary>
        /// 连接上下文
        /// </summary>
        public virtual IDbConnection Context
        {
            get
            {
                if (_db.State != ConnectionState.Open) _db.Open();
                return _db;
            }
        }

        /// <summary>
        /// 动态连接上下文
        /// </summary>
        public virtual dynamic DynamicContext { get; }

        /// <summary>
        /// 查询返回动态类型
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public virtual IEnumerable<dynamic> Query(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Context.Query(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        /// 查询返回特定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Context.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        /// 查询返回动态类型
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<dynamic>> QueryAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Context.QueryAsync(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 查询返回特定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Context.QueryAsync<T>(sql: sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public virtual int Execute(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Context.Execute(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public virtual Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Context.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        public virtual IDapperRepository<TEntity> Change<TEntity>()
            where TEntity : class, new()
        {
            return _serviceProvider.GetService<IDapperRepository<TEntity>>();
        }
    }

    /// <summary>
    /// Dapper 仓储实现类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial class DapperRepository<TEntity> : DapperRepository, IDapperRepository<TEntity>
        where TEntity : class, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="db"></param>
        public DapperRepository(IServiceProvider serviceProvider
            , IDbConnection db) : base(serviceProvider, db)
        {
        }

        /// <summary>
        /// 获取一条
        /// </summary>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual TEntity Get(object id, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.Get<TEntity>(id, transaction, commandTimeout);
        }

        /// <summary>
        /// 获取一条
        /// </summary>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual Task<TEntity> GetAsync(object id, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.GetAsync<TEntity>(id, transaction, commandTimeout);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> GetAll(IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.GetAll<TEntity>(transaction, commandTimeout);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<TEntity>> GetAllAsync(IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.GetAllAsync<TEntity>(transaction, commandTimeout);
        }

        /// <summary>
        /// 新增一条
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual long Insert(TEntity entity, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.Insert(entity, transaction, commandTimeout);
        }

        /// <summary>
        /// 新增一条
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="sqlAdapter"></param>
        /// <returns></returns>
        public virtual Task<int> InsertAsync(TEntity entity, IDbTransaction transaction = null, int? commandTimeout = null, ISqlAdapter sqlAdapter = null)
        {
            return Context.InsertAsync(entity, transaction, commandTimeout, sqlAdapter);
        }

        /// <summary>
        /// 新增多条
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual long Insert(IEnumerable<TEntity> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.Insert(entities, transaction, commandTimeout);
        }

        /// <summary>
        /// 新增多条
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="sqlAdapter"></param>
        /// <returns></returns>
        public virtual Task<int> InsertAsync(IEnumerable<TEntity> entities, IDbTransaction transaction = null, int? commandTimeout = null, ISqlAdapter sqlAdapter = null)
        {
            return Context.InsertAsync(entities, transaction, commandTimeout, sqlAdapter);
        }

        /// <summary>
        /// 更新一条
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual bool Update(TEntity entity, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.Update(entity, transaction, commandTimeout);
        }

        /// <summary>
        /// 更新一条
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual Task<bool> UpdateAsync(TEntity entity, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.UpdateAsync(entity, transaction, commandTimeout);
        }

        /// <summary>
        /// 更新多条
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual bool Update(IEnumerable<TEntity> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.Update(entities, transaction, commandTimeout);
        }

        /// <summary>
        /// 更新多条
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual Task<bool> UpdateAsync(IEnumerable<TEntity> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.UpdateAsync(entities, transaction, commandTimeout);
        }

        /// <summary>
        /// 删除一条
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual bool Delete(TEntity entity, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.Delete(entity, transaction, commandTimeout);
        }

        /// <summary>
        /// 删除一条
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual Task<bool> DeleteAsync(TEntity entity, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.DeleteAsync(entity, transaction, commandTimeout);
        }

        /// <summary>
        /// 删除多条
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual bool Delete(IEnumerable<TEntity> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.Delete(entities, transaction, commandTimeout);
        }

        /// <summary>
        /// 删除多条
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public virtual Task<bool> DeleteAsync(IEnumerable<TEntity> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Context.DeleteAsync(entities, transaction, commandTimeout);
        }
    }
}