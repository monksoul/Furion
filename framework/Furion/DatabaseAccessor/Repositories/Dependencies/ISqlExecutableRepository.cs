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
    /// Sql 执行仓储接口
    /// </summary>
    public interface ISqlExecutableRepository : ISqlExecutableRepository<MasterDbContextLocator>
    {
    }

    /// <summary>
    /// Sql 执行仓储接口
    /// </summary>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    public interface ISqlExecutableRepository<TDbContextLocator> : IPrivateSqlExecutableRepository
        where TDbContextLocator : class, IDbContextLocator
    {
    }

    /// <summary>
    /// Sql 执行仓储接口
    /// </summary>
    public interface IPrivateSqlExecutableRepository : IPrivateRootRepository
    {
        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        DataTable SqlProcedureQuery(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        DataTable SqlProcedureQuery(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        Task<DataTable> SqlProcedureQueryAsync(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        Task<DataTable> SqlProcedureQueryAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        Task<DataTable> SqlProcedureQueryAsync(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        List<T> SqlProcedureQuery<T>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        List<T> SqlProcedureQuery<T>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        Task<List<T>> SqlProcedureQueryAsync<T>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        Task<List<T>> SqlProcedureQueryAsync<T>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        Task<List<T>> SqlProcedureQueryAsync<T>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        DataSet SqlProcedureQueries(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        DataSet SqlProcedureQueries(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        Task<DataSet> SqlProcedureQueriesAsync(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        Task<DataSet> SqlProcedureQueriesAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        Task<DataSet> SqlProcedureQueriesAsync(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        List<T1> SqlProcedureQueries<T1>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, params DbParameter[] parameters);

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        List<T1> SqlProcedureQueries<T1>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, object model);

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        Task<List<T1>> SqlProcedureQueriesAsync<T1>(string procName, params DbParameter[] parameters);

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T1}}</returns>
        Task<List<T1>> SqlProcedureQueriesAsync<T1>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T1}</returns>
        Task<List<T1>> SqlProcedureQueriesAsync<T1>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <typeparam name="T7">元组元素类型</typeparam>
        /// <typeparam name="T8">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        object SqlProcedureScalar(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        object SqlProcedureScalar(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        Task<object> SqlProcedureScalarAsync(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        Task<object> SqlProcedureScalarAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        Task<object> SqlProcedureScalarAsync(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        TResult SqlProcedureScalar<TResult>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        TResult SqlProcedureScalar<TResult>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        Task<TResult> SqlProcedureScalarAsync<TResult>(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        Task<TResult> SqlProcedureScalarAsync<TResult>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        Task<TResult> SqlProcedureScalarAsync<TResult>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        int SqlProcedureNonQuery(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        int SqlProcedureNonQuery(string procName, object model);

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        Task<int> SqlProcedureNonQueryAsync(string procName, params DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        Task<int> SqlProcedureNonQueryAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        Task<int> SqlProcedureNonQueryAsync(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        int SqlNonQuery(string sql, params DbParameter[] parameters);

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        int SqlNonQuery(string sql, object model);

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        Task<int> SqlNonQueryAsync(string sql, params DbParameter[] parameters);

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        Task<int> SqlNonQueryAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        Task<int> SqlNonQueryAsync(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        object SqlScalar(string sql, params DbParameter[] parameters);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        object SqlScalar(string sql, object model);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        Task<object> SqlScalarAsync(string sql, params DbParameter[] parameters);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        Task<object> SqlScalarAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        Task<object> SqlScalarAsync(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        TResult SqlScalar<TResult>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        TResult SqlScalar<TResult>(string sql, object model);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        Task<TResult> SqlScalarAsync<TResult>(string sql, params DbParameter[] parameters);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        Task<TResult> SqlScalarAsync<TResult>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        Task<TResult> SqlScalarAsync<TResult>(string sql, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        ProcedureOutputResult SqlProcedureOutput(string procName, DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        Task<ProcedureOutputResult> SqlProcedureOutputAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        ProcedureOutputResult SqlProcedureOutput(string procName, object model);

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        Task<ProcedureOutputResult> SqlProcedureOutputAsync(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(string procName, DbParameter[] parameters);

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(string procName, object model);

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(string procName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        object SqlFunctionScalar(string funcName, params DbParameter[] parameters);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        object SqlFunctionScalar(string funcName, object model);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        Task<object> SqlFunctionScalarAsync(string funcName, params DbParameter[] parameters);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        Task<object> SqlFunctionScalarAsync(string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        Task<object> SqlFunctionScalarAsync(string funcName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        TResult SqlFunctionScalar<TResult>(string funcName, params DbParameter[] parameters);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        TResult SqlFunctionScalar<TResult>(string funcName, object model);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        Task<TResult> SqlFunctionScalarAsync<TResult>(string funcName, params DbParameter[] parameters);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        Task<TResult> SqlFunctionScalarAsync<TResult>(string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        Task<TResult> SqlFunctionScalarAsync<TResult>(string funcName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        DataTable SqlFunctionQuery(string funcName, params DbParameter[] parameters);

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        DataTable SqlFunctionQuery(string funcName, object model);

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        Task<DataTable> SqlFunctionQueryAsync(string funcName, params DbParameter[] parameters);

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        Task<DataTable> SqlFunctionQueryAsync(string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        Task<DataTable> SqlFunctionQueryAsync(string funcName, object model, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        List<T> SqlFunctionQuery<T>(string funcName, params DbParameter[] parameters);

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        List<T> SqlFunctionQuery<T>(string funcName, object model);

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        Task<List<T>> SqlFunctionQueryAsync<T>(string funcName, params DbParameter[] parameters);

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        Task<List<T>> SqlFunctionQueryAsync<T>(string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        Task<List<T>> SqlFunctionQueryAsync<T>(string funcName, object model, CancellationToken cancellationToken = default);
    }
}