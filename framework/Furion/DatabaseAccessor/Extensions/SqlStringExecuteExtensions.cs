// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor.Extensions
{
    /// <summary>
    /// Sql 字符串执行拓展类
    /// </summary>
    [SuppressSniffer]
    public static class SqlStringExecuteExtensions
    {
        /// <summary>
        /// 切换数据库
        /// </summary>
        /// <typeparam name="TDbContextLocator"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static SqlExecutePart Change<TDbContextLocator>(this string sql)
            where TDbContextLocator : class, IDbContextLocator
        {
            return new SqlExecutePart().SetSqlString(sql).Change<TDbContextLocator>();
        }

        /// <summary>
        /// 切换数据库
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dbContextLocator"></param>
        /// <returns></returns>
        public static SqlExecutePart Change(this string sql, Type dbContextLocator)
        {
            return new SqlExecutePart().SetSqlString(sql).Change(dbContextLocator);
        }

        /// <summary>
        /// 设置数据库执行作用域
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static SqlExecutePart SetContextScoped(this string sql, IServiceProvider serviceProvider)
        {
            return new SqlExecutePart().SetSqlString(sql).SetContextScoped(serviceProvider);
        }

        /// <summary>
        /// 设置 ADO.NET 超时时间
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="timeout">单位秒</param>
        /// <returns></returns>
        public static SqlExecutePart SetCommandTimeout(this string sql, int timeout)
        {
            return new SqlExecutePart().SetSqlString(sql).SetCommandTimeout(timeout);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlQuery(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQuery(parameters);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlQuery(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQuery(model);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlQueryAsync(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueryAsync(parameters);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlQueryAsync(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueryAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlQueryAsync(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueryAsync(model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <param name="sql"></param>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlQuery<T>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQuery<T>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <param name="sql"></param>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlQuery<T>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQuery<T>(model);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        public static Task<List<T>> SqlQueryAsync<T>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueryAsync<T>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public static Task<List<T>> SqlQueryAsync<T>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueryAsync<T>(parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public static Task<List<T>> SqlQueryAsync<T>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueryAsync<T>(model, cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public static DataSet SqlQueries(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries(parameters);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        public static DataSet SqlQueries(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries(model);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataSet}</returns>
        public static Task<DataSet> SqlQueriesAsync(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync(parameters);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataSet}</returns>
        public static Task<DataSet> SqlQueriesAsync(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync(parameters, cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataSet}</returns>
        public static Task<DataSet> SqlQueriesAsync(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync(model, cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        public static List<T1> SqlQueries<T1>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2) SqlQueries<T1, T2>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3) SqlQueries<T1, T2, T3>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlQueries<T1, T2, T3, T4>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlQueries<T1, T2, T3, T4, T5>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4, T5>(parameters);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlQueries<T1, T2, T3, T4, T5, T6>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4, T5, T6>(parameters);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlQueries<T1, T2, T3, T4, T5, T6, T7>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4, T5, T6, T7>(parameters);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(parameters);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        public static List<T1> SqlQueries<T1>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1>(model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2) SqlQueries<T1, T2>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2>(model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3) SqlQueries<T1, T2, T3>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3>(model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlQueries<T1, T2, T3, T4>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4>(model);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlQueries<T1, T2, T3, T4, T5>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4, T5>(model);
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
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlQueries<T1, T2, T3, T4, T5, T6>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4, T5, T6>(model);
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
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlQueries<T1, T2, T3, T4, T5, T6, T7>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4, T5, T6, T7>(model);
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
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(model);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        public static Task<List<T1>> SqlQueriesAsync<T1>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1>(parameters);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T1}}</returns>
        public static Task<List<T1>> SqlQueriesAsync<T1>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1>(parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2>(parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3>(parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4>(parameters, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5>(parameters);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5>(parameters, cancellationToken);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(parameters);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(parameters, cancellationToken);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(parameters);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(parameters, cancellationToken);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(parameters);
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
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(parameters, cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T1}</returns>
        public static Task<List<T1>> SqlQueriesAsync<T1>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1>(model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2>(model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3>(model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4>(model, cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5>(model, cancellationToken);
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
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(model, cancellationToken);
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
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(model, cancellationToken);
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
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(model, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public static int SqlNonQuery(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlNonQuery(parameters);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        public static int SqlNonQuery(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlNonQuery(model);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public static Task<int> SqlNonQueryAsync(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlNonQueryAsync(parameters);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public static Task<int> SqlNonQueryAsync(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlNonQueryAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public static Task<int> SqlNonQueryAsync(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlNonQueryAsync(model, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static object SqlScalar(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalar(parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public static object SqlScalar(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalar(model);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static Task<object> SqlScalarAsync(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalarAsync(parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlScalarAsync(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalarAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlScalarAsync(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalarAsync(model, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static TResult SqlScalar<TResult>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalar<TResult>(parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public static TResult SqlScalar<TResult>(this string sql, object model)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalar<TResult>(model);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static Task<TResult> SqlScalarAsync<TResult>(this string sql, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalarAsync<TResult>(parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static Task<TResult> SqlScalarAsync<TResult>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalarAsync<TResult>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static Task<TResult> SqlScalarAsync<TResult>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(sql).SqlScalarAsync<TResult>(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlProcedureQuery(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQuery(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlProcedureQuery(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQuery(model);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public static Task<DataTable> SqlProcedureQueryAsync(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueryAsync(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        public static Task<DataTable> SqlProcedureQueryAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueryAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        public static Task<DataTable> SqlProcedureQueryAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueryAsync(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlProcedureQuery<T>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQuery<T>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlProcedureQuery<T>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQuery<T>(model);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public static Task<List<T>> SqlProcedureQueryAsync<T>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueryAsync<T>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        public static Task<List<T>> SqlProcedureQueryAsync<T>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueryAsync<T>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        public static Task<List<T>> SqlProcedureQueryAsync<T>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueryAsync<T>(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public static DataSet SqlProcedureQueries(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        public static DataSet SqlProcedureQueries(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries(model);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public static Task<DataSet> SqlProcedureQueriesAsync(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        public static Task<DataSet> SqlProcedureQueriesAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        public static Task<DataSet> SqlProcedureQueriesAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync(model, cancellationToken);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        public static List<T1> SqlProcedureQueries<T1>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4, T5>(parameters);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(parameters);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(parameters);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(parameters);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        public static List<T1> SqlProcedureQueries<T1>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1>(model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2>(model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3>(model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4>(model);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4, T5>(model);
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
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(model);
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
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(model);
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
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(model);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        public static Task<List<T1>> SqlProcedureQueriesAsync<T1>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1>(parameters);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T1}}</returns>
        public static Task<List<T1>> SqlProcedureQueriesAsync<T1>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(parameters, cancellationToken);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(parameters);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(parameters, cancellationToken);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(parameters);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(parameters, cancellationToken);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(parameters);
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
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(parameters, cancellationToken);
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T1}</returns>
        public static Task<List<T1>> SqlProcedureQueriesAsync<T1>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1>(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2>(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3>(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4>(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <typeparam name="T4">元组元素类型</typeparam>
        /// <typeparam name="T5">元组元素类型</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(model, cancellationToken);
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
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(model, cancellationToken);
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
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(model, cancellationToken);
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
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static object SqlProcedureScalar(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalar(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public static object SqlProcedureScalar(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalar(model);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static Task<object> SqlProcedureScalarAsync(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalarAsync(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlProcedureScalarAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalarAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlProcedureScalarAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalarAsync(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static TResult SqlProcedureScalar<TResult>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalar<TResult>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public static TResult SqlProcedureScalar<TResult>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalar<TResult>(model);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static Task<TResult> SqlProcedureScalarAsync<TResult>(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalarAsync<TResult>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static Task<TResult> SqlProcedureScalarAsync<TResult>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalarAsync<TResult>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static Task<TResult> SqlProcedureScalarAsync<TResult>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureScalarAsync<TResult>(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public static int SqlProcedureNonQuery(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureNonQuery(parameters);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        public static int SqlProcedureNonQuery(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureNonQuery(model);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public static Task<int> SqlProcedureNonQueryAsync(this string procName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureNonQueryAsync(parameters);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public static Task<int> SqlProcedureNonQueryAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureNonQueryAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public static Task<int> SqlProcedureNonQueryAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureNonQueryAsync(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        public static ProcedureOutputResult SqlProcedureOutput(this string procName, DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureOutput(parameters);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public static Task<ProcedureOutputResult> SqlProcedureOutputAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureOutputAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        public static ProcedureOutputResult SqlProcedureOutput(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureOutput(model);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="model">命令模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public static Task<ProcedureOutputResult> SqlProcedureOutputAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureOutputAsync(model, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        public static ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(this string procName, DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureOutput<TResult>(parameters);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public static Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureOutputAsync<TResult>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        public static ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(this string procName, object model)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureOutput<TResult>(model);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName"></param>
        /// <param name="model">命令模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public static Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(procName).SqlProcedureOutputAsync<TResult>(model, cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static object SqlFunctionScalar(this string funcName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalar(parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="model"></param>
        /// <returns>object</returns>
        public static object SqlFunctionScalar(this string funcName, object model)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalar(model);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static Task<object> SqlFunctionScalarAsync(this string funcName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalarAsync(parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlFunctionScalarAsync(this string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalarAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlFunctionScalarAsync(this string funcName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalarAsync(model, cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static TResult SqlFunctionScalar<TResult>(this string funcName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalar<TResult>(parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public static TResult SqlFunctionScalar<TResult>(this string funcName, object model)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalar<TResult>(model);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static Task<TResult> SqlFunctionScalarAsync<TResult>(this string funcName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalarAsync<TResult>(parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static Task<TResult> SqlFunctionScalarAsync<TResult>(this string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalarAsync<TResult>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<TResult> SqlFunctionScalarAsync<TResult>(this string funcName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionScalarAsync<TResult>(model, cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlFunctionQuery(this string funcName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQuery(parameters);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlFunctionQuery(this string funcName, object model)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQuery(model);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlFunctionQueryAsync(this string funcName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQueryAsync(parameters);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlFunctionQueryAsync(this string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQueryAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlFunctionQueryAsync(this string funcName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQueryAsync(model, cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlFunctionQuery<T>(this string funcName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQuery<T>(parameters);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlFunctionQuery<T>(this string funcName, object model)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQuery<T>(model);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        public static Task<List<T>> SqlFunctionQueryAsync<T>(this string funcName, params DbParameter[] parameters)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQueryAsync<T>(parameters);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public static Task<List<T>> SqlFunctionQueryAsync<T>(this string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQueryAsync<T>(parameters, cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName"></param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public static Task<List<T>> SqlFunctionQueryAsync<T>(this string funcName, object model, CancellationToken cancellationToken = default)
        {
            return new SqlExecutePart().SetSqlString(funcName).SqlFunctionQueryAsync<T>(model, cancellationToken);
        }
    }
}