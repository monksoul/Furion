// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 构建 Sql 字符串执行部件
    /// </summary>
    public sealed partial class SqlExecutePart
    {
        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public DataTable SqlQuery(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQuery(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public DataTable SqlQuery(object model)
        {
            return GetSqlRepository().SqlQuery(SqlString, model);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        public Task<DataTable> SqlQueryAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueryAsync(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public Task<DataTable> SqlQueryAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueryAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public Task<DataTable> SqlQueryAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueryAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public List<T> SqlQuery<T>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQuery<T>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public List<T> SqlQuery<T>(object model)
        {
            return GetSqlRepository().SqlQuery<T>(SqlString, model);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        public Task<List<T>> SqlQueryAsync<T>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueryAsync<T>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public Task<List<T>> SqlQueryAsync<T>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueryAsync<T>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public Task<List<T>> SqlQueryAsync<T>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueryAsync<T>(SqlString, model, cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public DataSet SqlQueries(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries(SqlString, parameters);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        public DataSet SqlQueries(object model)
        {
            return GetSqlRepository().SqlQueries(SqlString, model);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataSet}</returns>
        public Task<DataSet> SqlQueriesAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync(SqlString, parameters);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataSet}</returns>
        public Task<DataSet> SqlQueriesAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataSet}</returns>
        public Task<DataSet> SqlQueriesAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        public List<T1> SqlQueries<T1>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries<T1>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2) SqlQueries<T1, T2>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries<T1, T2>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3) SqlQueries<T1, T2, T3>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlQueries<T1, T2, T3, T4>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlQueries<T1, T2, T3, T4, T5>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4, T5>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlQueries<T1, T2, T3, T4, T5, T6>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4, T5, T6>(SqlString, parameters);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlQueries<T1, T2, T3, T4, T5, T6, T7>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4, T5, T6, T7>(SqlString, parameters);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, parameters);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        public List<T1> SqlQueries<T1>(object model)
        {
            return GetSqlRepository().SqlQueries<T1>(SqlString, model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2) SqlQueries<T1, T2>(object model)
        {
            return GetSqlRepository().SqlQueries<T1, T2>(SqlString, model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3) SqlQueries<T1, T2, T3>(object model)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3>(SqlString, model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlQueries<T1, T2, T3, T4>(object model)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4>(SqlString, model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlQueries<T1, T2, T3, T4, T5>(object model)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4, T5>(SqlString, model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlQueries<T1, T2, T3, T4, T5, T6>(object model)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4, T5, T6>(SqlString, model);
        }

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
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlQueries<T1, T2, T3, T4, T5, T6, T7>(object model)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4, T5, T6, T7>(SqlString, model);
        }

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
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(object model)
        {
            return GetSqlRepository().SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, model);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        public Task<List<T1>> SqlQueriesAsync<T1>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync<T1>(SqlString, parameters);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T1}}</returns>
        public Task<List<T1>> SqlQueriesAsync<T1>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(SqlString, parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(SqlString, parameters, cancellationToken);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(SqlString, parameters);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(SqlString, parameters, cancellationToken);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, parameters);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T1}</returns>
        public Task<List<T1>> SqlQueriesAsync<T1>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(SqlString, model, cancellationToken);
        }

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
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(SqlString, model, cancellationToken);
        }

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
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public int SqlNonQuery(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlNonQuery(SqlString, parameters);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        public int SqlNonQuery(object model)
        {
            return GetSqlRepository().SqlNonQuery(SqlString, model);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public Task<int> SqlNonQueryAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlNonQueryAsync(SqlString, parameters);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public Task<int> SqlNonQueryAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlNonQueryAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public Task<int> SqlNonQueryAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlNonQueryAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public object SqlScalar(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlScalar(SqlString, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public object SqlScalar(object model)
        {
            return GetSqlRepository().SqlScalar(SqlString, model);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public Task<object> SqlScalarAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlScalarAsync(SqlString, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public Task<object> SqlScalarAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlScalarAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public Task<object> SqlScalarAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlScalarAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public TResult SqlScalar<TResult>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlScalar<TResult>(SqlString, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public TResult SqlScalar<TResult>(object model)
        {
            return GetSqlRepository().SqlScalar<TResult>(SqlString, model);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public Task<TResult> SqlScalarAsync<TResult>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlScalarAsync<TResult>(SqlString, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public Task<TResult> SqlScalarAsync<TResult>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlScalarAsync<TResult>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public Task<TResult> SqlScalarAsync<TResult>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlScalarAsync<TResult>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public DataTable SqlProcedureQuery(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQuery(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public DataTable SqlProcedureQuery(object model)
        {
            return GetSqlRepository().SqlProcedureQuery(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public Task<DataTable> SqlProcedureQueryAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueryAsync(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        public Task<DataTable> SqlProcedureQueryAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueryAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        public Task<DataTable> SqlProcedureQueryAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueryAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public List<T> SqlProcedureQuery<T>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQuery<T>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public List<T> SqlProcedureQuery<T>(object model)
        {
            return GetSqlRepository().SqlProcedureQuery<T>(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public Task<List<T>> SqlProcedureQueryAsync<T>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueryAsync<T>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        public Task<List<T>> SqlProcedureQueryAsync<T>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueryAsync<T>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        public Task<List<T>> SqlProcedureQueryAsync<T>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueryAsync<T>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public DataSet SqlProcedureQueries(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        public DataSet SqlProcedureQueries(object model)
        {
            return GetSqlRepository().SqlProcedureQueries(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public Task<DataSet> SqlProcedureQueriesAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        public Task<DataSet> SqlProcedureQueriesAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        public Task<DataSet> SqlProcedureQueriesAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        public List<T1> SqlProcedureQueries<T1>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries<T1>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4, T5>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(SqlString, parameters);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(SqlString, parameters);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, parameters);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        public List<T1> SqlProcedureQueries<T1>(object model)
        {
            return GetSqlRepository().SqlProcedureQueries<T1>(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(object model)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2>(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(object model)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3>(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(object model)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4>(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(object model)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4, T5>(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(object model)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(SqlString, model);
        }

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
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(object model)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(SqlString, model);
        }

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
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(object model)
        {
            return GetSqlRepository().SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, model);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        public Task<List<T1>> SqlProcedureQueriesAsync<T1>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1>(SqlString, parameters);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T1}}</returns>
        public Task<List<T1>> SqlProcedureQueriesAsync<T1>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(SqlString, parameters, cancellationToken);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(SqlString, parameters);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(SqlString, parameters, cancellationToken);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, parameters);
        }

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
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T1}</returns>
        public Task<List<T1>> SqlProcedureQueriesAsync<T1>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <typeparam name="T6">元组元素类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(SqlString, model, cancellationToken);
        }

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
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(SqlString, model, cancellationToken);
        }

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
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public object SqlProcedureScalar(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureScalar(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public object SqlProcedureScalar(object model)
        {
            return GetSqlRepository().SqlProcedureScalar(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public Task<object> SqlProcedureScalarAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureScalarAsync(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public Task<object> SqlProcedureScalarAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureScalarAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public Task<object> SqlProcedureScalarAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureScalarAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public TResult SqlProcedureScalar<TResult>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureScalar<TResult>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public TResult SqlProcedureScalar<TResult>(object model)
        {
            return GetSqlRepository().SqlProcedureScalar<TResult>(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public Task<TResult> SqlProcedureScalarAsync<TResult>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureScalarAsync<TResult>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public Task<TResult> SqlProcedureScalarAsync<TResult>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureScalarAsync<TResult>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public Task<TResult> SqlProcedureScalarAsync<TResult>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureScalarAsync<TResult>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public int SqlProcedureNonQuery(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureNonQuery(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        public int SqlProcedureNonQuery(object model)
        {
            return GetSqlRepository().SqlProcedureNonQuery(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public Task<int> SqlProcedureNonQueryAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureNonQueryAsync(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public Task<int> SqlProcedureNonQueryAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureNonQueryAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public Task<int> SqlProcedureNonQueryAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureNonQueryAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        public ProcedureOutputResult SqlProcedureOutput(DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureOutput(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public Task<ProcedureOutputResult> SqlProcedureOutputAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureOutputAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        public ProcedureOutputResult SqlProcedureOutput(object model)
        {
            return GetSqlRepository().SqlProcedureOutput(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="model">命令模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public Task<ProcedureOutputResult> SqlProcedureOutputAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureOutputAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        public ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(DbParameter[] parameters)
        {
            return GetSqlRepository().SqlProcedureOutput<TResult>(SqlString, parameters);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureOutputAsync<TResult>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        public ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(object model)
        {
            return GetSqlRepository().SqlProcedureOutput<TResult>(SqlString, model);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="model">命令模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlProcedureOutputAsync<TResult>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public object SqlFunctionScalar(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlFunctionScalar(SqlString, parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="model"></param>
        /// <returns>object</returns>
        public object SqlFunctionScalar(object model)
        {
            return GetSqlRepository().SqlFunctionScalar(SqlString, model);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public Task<object> SqlFunctionScalarAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlFunctionScalarAsync(SqlString, parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public Task<object> SqlFunctionScalarAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlFunctionScalarAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public Task<object> SqlFunctionScalarAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlFunctionScalarAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public TResult SqlFunctionScalar<TResult>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlFunctionScalar<TResult>(SqlString, parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public TResult SqlFunctionScalar<TResult>(object model)
        {
            return GetSqlRepository().SqlFunctionScalar<TResult>(SqlString, model);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public Task<TResult> SqlFunctionScalarAsync<TResult>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlFunctionScalarAsync<TResult>(SqlString, parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public Task<TResult> SqlFunctionScalarAsync<TResult>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlFunctionScalarAsync<TResult>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public Task<TResult> SqlFunctionScalarAsync<TResult>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlFunctionScalarAsync<TResult>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public DataTable SqlFunctionQuery(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlFunctionQuery(SqlString, parameters);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public DataTable SqlFunctionQuery(object model)
        {
            return GetSqlRepository().SqlFunctionQuery(SqlString, model);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        public Task<DataTable> SqlFunctionQueryAsync(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlFunctionQueryAsync(SqlString, parameters);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public Task<DataTable> SqlFunctionQueryAsync(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlFunctionQueryAsync(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public Task<DataTable> SqlFunctionQueryAsync(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlFunctionQueryAsync(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public List<T> SqlFunctionQuery<T>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlFunctionQuery<T>(SqlString, parameters);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public List<T> SqlFunctionQuery<T>(object model)
        {
            return GetSqlRepository().SqlFunctionQuery<T>(SqlString, model);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        public Task<List<T>> SqlFunctionQueryAsync<T>(params DbParameter[] parameters)
        {
            return GetSqlRepository().SqlFunctionQueryAsync<T>(SqlString, parameters);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public Task<List<T>> SqlFunctionQueryAsync<T>(DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlFunctionQueryAsync<T>(SqlString, parameters, cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public Task<List<T>> SqlFunctionQueryAsync<T>(object model, CancellationToken cancellationToken = default)
        {
            return GetSqlRepository().SqlFunctionQueryAsync<T>(SqlString, model, cancellationToken);
        }

        /// <summary>
        /// 获取 Sql 执行仓储
        /// </summary>
        /// <returns></returns>
        private IPrivateSqlRepository GetSqlRepository()
        {
            var repository = App.GetService(typeof(ISqlRepository<>).MakeGenericType(DbContextLocator), ContextScoped) as IPrivateSqlRepository;
            // 设置超时时间
            if (Timeout > 0)
            {
                repository.Context.Database.SetCommandTimeout(TimeSpan.FromSeconds(Timeout));
            }
            return repository;
        }
    }
}