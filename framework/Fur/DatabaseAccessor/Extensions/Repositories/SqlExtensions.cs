using Fur.DependencyInjection;
using Mapster;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// Sql 拓展类
    /// </summary>
    [SkipScan]
    public static class SqlExtensions
    {
        /// <summary>
        /// 切换数据库上下文
        /// </summary>
        /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <returns></returns>
        public static string Change<TDbContextLocator>(this string sql)
            where TDbContextLocator : class, IDbContextLocator
        {
            if (sql.Contains(dbContextLocatorSqlSplit))
            {
                sql = sql[(sql.IndexOf(dbContextLocatorSqlSplit) + dbContextLocatorSqlSplit.Length)..];
            }

            return $"{typeof(TDbContextLocator).FullName}{dbContextLocatorSqlSplit}{sql}";
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlQuery(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).ExecuteReader(sql, parameters);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlQuery(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).ExecuteReader(sql, model).dataTable;
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlQueryAsync(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).ExecuteReaderAsync(sql, parameters);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlQueryAsync(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlDatabase(ref sql).ExecuteReaderAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sql 查询返回 DataTable
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public static async Task<DataTable> SqlQueryAsync(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataTable, _) = await GetSqlDatabase(ref sql).ExecuteReaderAsync(sql, model, cancellationToken: cancellationToken);
            return dataTable;
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlQuery<T>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).ExecuteReader(sql, parameters).ToList<T>();
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlQuery<T>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).ExecuteReader(sql, model).dataTable.ToList<T>();
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        public static async Task<List<T>> SqlQueryAsync<T>(this string sql, params DbParameter[] parameters)
        {
            var dataTable = await GetSqlDatabase(ref sql).ExecuteReaderAsync(sql, parameters);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public static async Task<List<T>> SqlQueryAsync<T>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataTable = await GetSqlDatabase(ref sql).ExecuteReaderAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public static async Task<List<T>> SqlQueryAsync<T>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataTable, _) = await GetSqlDatabase(ref sql).ExecuteReaderAsync(sql, model, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public static DataSet SqlQueries(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        public static DataSet SqlQueries(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet;
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataSet}</returns>
        public static Task<DataSet> SqlQueriesAsync(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataSet}</returns>
        public static Task<DataSet> SqlQueriesAsync(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        ///  Sql 查询返回 DataSet
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataSet}</returns>
        public static async Task<DataSet> SqlQueriesAsync(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataSet, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataSet;
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        public static List<T1> SqlQueries<T1>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters).ToList<T1>();
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2) SqlQueries<T1, T2>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters).ToList<T1, T2>();
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3) SqlQueries<T1, T2, T3>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters).ToList<T1, T2, T3>();
        }

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
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlQueries<T1, T2, T3, T4>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters).ToList<T1, T2, T3, T4>();
        }

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
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlQueries<T1, T2, T3, T4, T5>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters).ToList<T1, T2, T3, T4, T5>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlQueries<T1, T2, T3, T4, T5, T6>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters).ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlQueries<T1, T2, T3, T4, T5, T6, T7>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters).ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, parameters).ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        public static List<T1> SqlQueries<T1>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet.ToList<T1>();
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2) SqlQueries<T1, T2>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet.ToList<T1, T2>();
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3) SqlQueries<T1, T2, T3>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet.ToList<T1, T2, T3>();
        }

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
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlQueries<T1, T2, T3, T4>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet.ToList<T1, T2, T3, T4>();
        }

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
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlQueries<T1, T2, T3, T4, T5>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet.ToList<T1, T2, T3, T4, T5>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlQueries<T1, T2, T3, T4, T5, T6>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet.ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlQueries<T1, T2, T3, T4, T5, T6, T7>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlQueries<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).DataAdapterFill(sql, model).dataSet.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        public static async Task<List<T1>> SqlQueriesAsync<T1>(this string sql, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
            return dataset.ToList<T1>();
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T1}}</returns>
        public static async Task<List<T1>> SqlQueriesAsync<T1>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataset.ToList<T1>();
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(this string sql, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
            return dataset.ToList<T1, T2>();
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2>();
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(this string sql, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
            return dataset.ToList<T1, T2, T3>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(this string sql, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
            return dataset.ToList<T1, T2, T3, T4>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(this string sql, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
            return dataset.ToList<T1, T2, T3, T4, T5>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(this string sql, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
            return dataset.ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string sql, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        ///  Sql 查询返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T1}</returns>
        public static async Task<List<T1>> SqlQueriesAsync<T1>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataset.ToList<T1>();
        }

        /// <summary>
        /// Sql 查询返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2)> SqlQueriesAsync<T1, T2>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlQueriesAsync<T1, T2, T3>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlQueriesAsync<T1, T2, T3, T4>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlQueriesAsync<T1, T2, T3, T4, T5>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref sql).DataAdapterFillAsync(sql, model, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlProcedureQuery(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).ExecuteReader(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlProcedureQuery(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).ExecuteReader(procName, model, CommandType.StoredProcedure).dataTable;
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public static Task<DataTable> SqlProcedureQueryAsync(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).ExecuteReaderAsync(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        public static Task<DataTable> SqlProcedureQueryAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlDatabase(ref procName).ExecuteReaderAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        public static async Task<DataTable> SqlProcedureQueryAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataTable, _) = await GetSqlDatabase(ref procName).ExecuteReaderAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataTable;
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlProcedureQuery<T>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).ExecuteReader(procName, parameters, CommandType.StoredProcedure).ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlProcedureQuery<T>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).ExecuteReader(procName, model, CommandType.StoredProcedure).dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public static async Task<List<T>> SqlProcedureQueryAsync<T>(this string procName, params DbParameter[] parameters)
        {
            var dataTable = await GetSqlDatabase(ref procName).ExecuteReaderAsync(procName, parameters, CommandType.StoredProcedure);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        public static async Task<List<T>> SqlProcedureQueryAsync<T>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataTable = await GetSqlDatabase(ref procName).ExecuteReaderAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        public static async Task<List<T>> SqlProcedureQueryAsync<T>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataTable, _) = await GetSqlDatabase(ref procName).ExecuteReaderAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public static DataSet SqlProcedureQueries(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        public static DataSet SqlProcedureQueries(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet;
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public static Task<DataSet> SqlProcedureQueriesAsync(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        public static Task<DataSet> SqlProcedureQueriesAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        public static async Task<DataSet> SqlProcedureQueriesAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataSet, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataSet;
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        public static List<T1> SqlProcedureQueries<T1>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3>();
        }

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
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4>();
        }

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
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4, T5>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        public static List<T1> SqlProcedureQueries<T1>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3>();
        }

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
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4>();
        }

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
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4, T5>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public static (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        public static async Task<List<T1>> SqlProcedureQueriesAsync<T1>(this string procName, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
            return dataset.ToList<T1>();
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T1}}</returns>
        public static async Task<List<T1>> SqlProcedureQueriesAsync<T1>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(this string procName, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
            return dataset.ToList<T1, T2>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <typeparam name="T3">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(this string procName, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
            return dataset.ToList<T1, T2, T3>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(this string procName, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
            return dataset.ToList<T1, T2, T3, T4>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(this string procName, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
            return dataset.ToList<T1, T2, T3, T4, T5>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(this string procName, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
            return dataset.ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string procName, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, params DbParameter[] parameters)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T1}</returns>
        public static async Task<List<T1>> SqlProcedureQueriesAsync<T1>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4>();
        }

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
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>元组类型</returns>
        public static async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static object SqlProcedureScalar(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).ExecuteScalar(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public static object SqlProcedureScalar(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).ExecuteScalar(procName, model, CommandType.StoredProcedure).result;
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static Task<object> SqlProcedureScalarAsync(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).ExecuteScalarAsync(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlProcedureScalarAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlDatabase(ref procName).ExecuteScalarAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static async Task<object> SqlProcedureScalarAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (result, _) = await GetSqlDatabase(ref procName).ExecuteScalarAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return result;
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static TResult SqlProcedureScalar<TResult>(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).ExecuteScalar(procName, parameters, CommandType.StoredProcedure).Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public static TResult SqlProcedureScalar<TResult>(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).ExecuteScalar(procName, model, CommandType.StoredProcedure).result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static async Task<TResult> SqlProcedureScalarAsync<TResult>(this string procName, params DbParameter[] parameters)
        {
            var result = await GetSqlDatabase(ref procName).ExecuteScalarAsync(procName, parameters, CommandType.StoredProcedure);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static async Task<TResult> SqlProcedureScalarAsync<TResult>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var result = await GetSqlDatabase(ref procName).ExecuteScalarAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static async Task<TResult> SqlProcedureScalarAsync<TResult>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (result, _) = await GetSqlDatabase(ref procName).ExecuteScalarAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public static int SqlProcedureNonQuery(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).ExecuteNonQuery(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        public static int SqlProcedureNonQuery(this string procName, object model)
        {
            return GetSqlDatabase(ref procName).ExecuteNonQuery(procName, model, CommandType.StoredProcedure).rowEffects;
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public static Task<int> SqlProcedureNonQueryAsync(this string procName, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref procName).ExecuteNonQueryAsync(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public static Task<int> SqlProcedureNonQueryAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlDatabase(ref procName).ExecuteNonQueryAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public static async Task<int> SqlProcedureNonQueryAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            var (rowEffects, _) = await GetSqlDatabase(ref procName).ExecuteNonQueryAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return rowEffects;
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public static int SqlNonQuery(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        public static int SqlNonQuery(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).ExecuteNonQuery(sql, model).rowEffects;
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public static Task<int> SqlNonQueryAsync(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public static Task<int> SqlNonQueryAsync(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlDatabase(ref sql).ExecuteNonQueryAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public static async Task<int> SqlNonQueryAsync(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (rowEffects, _) = await GetSqlDatabase(ref sql).ExecuteNonQueryAsync(sql, model, cancellationToken: cancellationToken);
            return rowEffects;
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static object SqlScalar(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).ExecuteScalar(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public static object SqlScalar(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).ExecuteScalar(sql, model).result;
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static Task<object> SqlScalarAsync(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).ExecuteScalarAsync(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlScalarAsync(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return GetSqlDatabase(ref sql).ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static async Task<object> SqlScalarAsync(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (result, _) = await GetSqlDatabase(ref sql).ExecuteScalarAsync(sql, model, cancellationToken: cancellationToken);
            return result;
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static TResult SqlScalar<TResult>(this string sql, params DbParameter[] parameters)
        {
            return GetSqlDatabase(ref sql).ExecuteScalar(sql, parameters).Adapt<TResult>();
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public static TResult SqlScalar<TResult>(this string sql, object model)
        {
            return GetSqlDatabase(ref sql).ExecuteScalar(sql, model).result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static async Task<TResult> SqlScalarAsync<TResult>(this string sql, params DbParameter[] parameters)
        {
            var result = await GetSqlDatabase(ref sql).ExecuteScalarAsync(sql, parameters);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static async Task<TResult> SqlScalarAsync<TResult>(this string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var result = await GetSqlDatabase(ref sql).ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static async Task<TResult> SqlScalarAsync<TResult>(this string sql, object model, CancellationToken cancellationToken = default)
        {
            var (result, _) = await GetSqlDatabase(ref sql).ExecuteScalarAsync(sql, model, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        public static ProcedureOutputResult SqlProcedureOutput(this string procName, DbParameter[] parameters)
        {
            parameters ??= Array.Empty<DbParameter>();

            // 执行存储过程
            var dataSet = GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput(parameters, dataSet);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public static async Task<ProcedureOutputResult> SqlProcedureOutputAsync(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            parameters ??= Array.Empty<DbParameter>();

            // 执行存储过程
            var dataSet = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput(parameters, dataSet);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        public static ProcedureOutputResult SqlProcedureOutput(this string procName, object model)
        {
            // 执行存储过程
            var (dataSet, parameters) = GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput(parameters, dataSet);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public static async Task<ProcedureOutputResult> SqlProcedureOutputAsync(this string procName, object model, CancellationToken cancellationToken = default)
        {
            // 执行存储过程
            var (dataSet, parameters) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput(parameters, dataSet);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        public static ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(this string procName, DbParameter[] parameters)
        {
            parameters ??= Array.Empty<DbParameter>();

            // 执行存储过程
            var dataSet = GetSqlDatabase(ref procName).DataAdapterFill(procName, parameters, CommandType.StoredProcedure);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput<TResult>(parameters, dataSet);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public static async Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(this string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            parameters ??= Array.Empty<DbParameter>();

            // 执行存储过程
            var dataSet = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput<TResult>(parameters, dataSet);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        public static ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(this string procName, object model)
        {
            // 执行存储过程
            var (dataSet, parameters) = GetSqlDatabase(ref procName).DataAdapterFill(procName, model, CommandType.StoredProcedure);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput<TResult>(parameters, dataSet);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <typeparam name="TResult">数据集结果</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>ProcedureOutput</returns>
        public static async Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(this string procName, object model, CancellationToken cancellationToken = default)
        {
            // 执行存储过程
            var (dataSet, parameters) = await GetSqlDatabase(ref procName).DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput<TResult>(parameters, dataSet);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static object SqlFunctionScalar(this string funcName, params DbParameter[] parameters)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            return database.ExecuteScalar(sql, parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public static object SqlFunctionScalar(this string funcName, object model)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, model);
            return database.ExecuteScalar(sql, model).result;
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public static Task<object> SqlFunctionScalarAsync(this string funcName, params DbParameter[] parameters)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            return database.ExecuteScalarAsync(sql, parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static Task<object> SqlFunctionScalarAsync(this string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            return database.ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static async Task<object> SqlFunctionScalarAsync(this string funcName, object model, CancellationToken cancellationToken = default)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, model);
            var (result, _) = await database.ExecuteScalarAsync(sql, model, cancellationToken: cancellationToken);
            return result;
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static TResult SqlFunctionScalar<TResult>(this string funcName, params DbParameter[] parameters)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            return database.ExecuteScalar(sql, parameters).Adapt<TResult>();
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public static TResult SqlFunctionScalar<TResult>(this string funcName, object model)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, model);
            return database.ExecuteScalar(sql, model).result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public static async Task<TResult> SqlFunctionScalarAsync<TResult>(this string funcName, params DbParameter[] parameters)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            var result = await database.ExecuteScalarAsync(sql, parameters);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public static async Task<TResult> SqlFunctionScalarAsync<TResult>(this string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            var result = await database.ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public static async Task<TResult> SqlFunctionScalarAsync<TResult>(this string funcName, object model, CancellationToken cancellationToken = default)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Scalar, funcName, model);
            var (result, _) = await database.ExecuteScalarAsync(sql, model, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlFunctionQuery(this string funcName, params DbParameter[] parameters)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, parameters);
            return database.ExecuteReader(sql, parameters);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public static DataTable SqlFunctionQuery(this string funcName, object model)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, model);
            return database.ExecuteReader(sql, model).dataTable;
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlFunctionQueryAsync(this string funcName, params DbParameter[] parameters)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, parameters);
            return database.ExecuteReaderAsync(sql, parameters);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public static Task<DataTable> SqlFunctionQueryAsync(this string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, parameters);
            return database.ExecuteReaderAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public static async Task<DataTable> SqlFunctionQueryAsync(this string funcName, object model, CancellationToken cancellationToken = default)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, model);
            var (dataTable, _) = await database.ExecuteReaderAsync(sql, model, cancellationToken: cancellationToken);
            return dataTable;
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlFunctionQuery<T>(this string funcName, params DbParameter[] parameters)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, parameters);
            return database.ExecuteReader(sql, parameters).ToList<T>();
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public static List<T> SqlFunctionQuery<T>(this string funcName, object model)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, model);
            return database.ExecuteReader(sql, model).dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        public static async Task<List<T>> SqlFunctionQueryAsync<T>(this string funcName, params DbParameter[] parameters)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, parameters);
            var dataTable = await database.ExecuteReaderAsync(sql, parameters);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public static async Task<List<T>> SqlFunctionQueryAsync<T>(this string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, parameters);
            var dataTable = await database.ExecuteReaderAsync(sql, parameters, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{List{T}}</returns>
        public static async Task<List<T>> SqlFunctionQueryAsync<T>(this string funcName, object model, CancellationToken cancellationToken = default)
        {
            var database = GetSqlDatabase(ref funcName);
            var sql = DbHelpers.GenerateFunctionSql(database.ProviderName, DbFunctionType.Table, funcName, model);
            var (dataTable, _) = await database.ExecuteReaderAsync(sql, model, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 获取 Sql 数据库操作对象
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static DatabaseFacade GetSqlDatabase(ref string sql)
        {
            // 数据库上下文定位器完整类型名称
            string dbContextLocatorFullName;
            if (sql.Contains(dbContextLocatorSqlSplit))
            {
                dbContextLocatorFullName = sql.Split(dbContextLocatorSqlSplit).First();
                sql = sql[(sql.IndexOf(dbContextLocatorSqlSplit) + dbContextLocatorSqlSplit.Length)..];
            }
            else dbContextLocatorFullName = typeof(MasterDbContextLocator).FullName;

            // 获取数据库上下文定位器类型
            var dbContextLocator = Penetrates.DbContextLocatorTypeCached.GetValueOrDefault(dbContextLocatorFullName)
            ?? throw new InvalidOperationException(string.Format("Failed to load \"{0}\" DbContextLocator Type.", dbContextLocatorFullName));

            // 创建Sql仓储
            var sqlRepositoryType = typeof(ISqlRepository<>).MakeGenericType(dbContextLocator);
            var sqlRepository = App.GetService(sqlRepositoryType);

            return sqlRepositoryType.GetProperty(nameof(ISqlRepository.Database)).GetValue(sqlRepository) as DatabaseFacade;
        }

        // 分隔符
        private static readonly string dbContextLocatorSqlSplit;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SqlExtensions()
        {
            dbContextLocatorSqlSplit = "-=>";
        }
    }
}