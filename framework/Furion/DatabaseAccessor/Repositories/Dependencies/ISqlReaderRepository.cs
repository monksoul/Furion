// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// Sql 查询仓储接口
    /// </summary>
    public interface ISqlReaderRepository : ISqlReaderRepository<MasterDbContextLocator>
    {
    }

    /// <summary>
    /// Sql 查询仓储接口
    /// </summary>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    public interface ISqlReaderRepository<TDbContextLocator> : IPrivateSqlReaderRepository
        where TDbContextLocator : class, IDbContextLocator
    {
    }

    /// <summary>
    /// Sql 查询仓储接口
    /// </summary>
    public interface IPrivateSqlReaderRepository : IPrivateRootRepository
    {
        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        DataTable SqlQuery(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        DataTable SqlQuery(string sql, object model);

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        Task<DataTable> SqlQueryAsync(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        Task<DataTable> SqlQueryAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        Task<DataTable> SqlQueryAsync(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        List<T> SqlQuery<T>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        List<T> SqlQuery<T>(string sql, object model);

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        Task<List<T>> SqlQueryAsync<T>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        Task<List<T>> SqlQueryAsync<T>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        Task<List<T>> SqlQueryAsync<T>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        DataSet SqlQueries(string sql, params DbParameter[] parameters);

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        DataSet SqlQueries(string sql, object model);

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataSet}</returns>
        Task<DataSet> SqlQueriesAsync(string sql, params DbParameter[] parameters);

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataSet}</returns>
        Task<DataSet> SqlQueriesAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataSet}</returns>
        Task<DataSet> SqlQueriesAsync(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        List<T1> SqlQueries<T1>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2) SqlQueries<T1, T2>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3) SqlQueries<T1, T2, T3>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlQueries<T1, T2, T3, T4>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlQueries<T1, T2, T3, T4, T5>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlQueries<T1, T2, T3, T4, T5, T6>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlQueries<T1, T2, T3, T4, T5, T6, T7>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(string sql, params DbParameter[] parameters);

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        List<T1> SqlQueries<T1>(string sql, object model);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2) SqlQueries<T1, T2>(string sql, object model);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3) SqlQueries<T1, T2, T3>(string sql, object model);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlQueries<T1, T2, T3, T4>(string sql, object model);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlQueries<T1, T2, T3, T4, T5>(string sql, object model);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlQueries<T1, T2, T3, T4, T5, T6>(string sql, object model);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlQueries<T1, T2, T3, T4, T5, T6, T7>(string sql, object model);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(string sql, object model);

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        Task<List<T1>> SqlQueriesAsync<T1>(string sql, params DbParameter[] parameters);

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T1}}</returns>
        Task<List<T1>> SqlQueriesAsync<T1>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T1}</returns>
        Task<List<T1>> SqlQueriesAsync<T1>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string sql, object model, CancellationToken cancellationToken = default);
    }
}