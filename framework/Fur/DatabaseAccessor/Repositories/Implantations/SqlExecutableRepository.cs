using Mapster;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// Sql 执行仓储分部类
    /// </summary>
    /// <typeparam name="TDbContextLocator">数据库上下文定位器</typeparam>
    public partial class SqlRepository<TDbContextLocator>
        where TDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public virtual DataTable SqlProcedureQuery(string procName, params DbParameter[] parameters)
        {
            return Database.ExecuteReader(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public virtual DataTable SqlProcedureQuery(string procName, object model)
        {
            return Database.ExecuteReader(procName, model, CommandType.StoredProcedure).dataTable;
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public virtual Task<DataTable> SqlProcedureQueryAsync(string procName, params DbParameter[] parameters)
        {
            return Database.ExecuteReaderAsync(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        public virtual Task<DataTable> SqlProcedureQueryAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return Database.ExecuteReaderAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataTable
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataTable</returns>
        public virtual async Task<DataTable> SqlProcedureQueryAsync(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataTable, _) = await Database.ExecuteReaderAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataTable;
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public virtual List<T> SqlProcedureQuery<T>(string procName, params DbParameter[] parameters)
        {
            return Database.ExecuteReader(procName, parameters, CommandType.StoredProcedure).ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public virtual List<T> SqlProcedureQuery<T>(string procName, object model)
        {
            return Database.ExecuteReader(procName, model, CommandType.StoredProcedure).dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public virtual async Task<List<T>> SqlProcedureQueryAsync<T>(string procName, params DbParameter[] parameters)
        {
            var dataTable = await Database.ExecuteReaderAsync(procName, parameters, CommandType.StoredProcedure);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        public virtual async Task<List<T>> SqlProcedureQueryAsync<T>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataTable = await Database.ExecuteReaderAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 List 集合
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>List{T}</returns>
        public virtual async Task<List<T>> SqlProcedureQueryAsync<T>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataTable, _) = await Database.ExecuteReaderAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public virtual DataSet SqlProcedureQueries(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataSet</returns>
        public virtual DataSet SqlProcedureQueries(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet;
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataSet</returns>
        public virtual Task<DataSet> SqlProcedureQueriesAsync(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        public virtual Task<DataSet> SqlProcedureQueriesAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 DataSet
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>DataSet</returns>
        public virtual async Task<DataSet> SqlProcedureQueriesAsync(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataSet, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataSet;
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T1}</returns>
        public virtual List<T1> SqlProcedureQueries<T1>(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>元组类型</returns>
        public virtual (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4, T5>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4, T5, T6>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, params DbParameter[] parameters)
        {
            return Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure).ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T1}</returns>
        public virtual List<T1> SqlProcedureQueries<T1>(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1>();
        }

        /// <summary>
        /// 执行存储过程返回 元组 集合
        /// </summary>
        /// <typeparam name="T1">元组元素类型</typeparam>
        /// <typeparam name="T2">元组元素类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>元组类型</returns>
        public virtual (List<T1> list1, List<T2> list2) SqlProcedureQueries<T1, T2>(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3) SqlProcedureQueries<T1, T2, T3>(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4) SqlProcedureQueries<T1, T2, T3, T4>(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5) SqlProcedureQueries<T1, T2, T3, T4, T5>(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4, T5>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6) SqlProcedureQueries<T1, T2, T3, T4, T5, T6>(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4, T5, T6>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7>(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4, T5, T6, T7>();
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
        public virtual (List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8) SqlProcedureQueries<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, object model)
        {
            return Database.DataAdapterFill(procName, model, CommandType.StoredProcedure).dataSet.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        ///  执行存储过程返回 List 集合
        /// </summary>
        /// <typeparam name="T1">返回类型</typeparam>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T1}}</returns>
        public virtual async Task<List<T1>> SqlProcedureQueriesAsync<T1>(string procName, params DbParameter[] parameters)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
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
        public virtual async Task<List<T1>> SqlProcedureQueriesAsync<T1>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(string procName, params DbParameter[] parameters)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
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
        public virtual async Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(string procName, params DbParameter[] parameters)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(string procName, params DbParameter[] parameters)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(string procName, params DbParameter[] parameters)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(string procName, params DbParameter[] parameters)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string procName, params DbParameter[] parameters)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, params DbParameter[] parameters)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var dataset = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<List<T1>> SqlProcedureQueriesAsync<T1>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2)> SqlProcedureQueriesAsync<T1, T2>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3)> SqlProcedureQueriesAsync<T1, T2, T3>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4)> SqlProcedureQueriesAsync<T1, T2, T3, T4>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
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
        public virtual async Task<(List<T1> list1, List<T2> list2, List<T3> list3, List<T4> list4, List<T5> list5, List<T6> list6, List<T7> list7, List<T8> list8)> SqlProcedureQueriesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (dataset, _) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return dataset.ToList<T1, T2, T3, T4, T5, T6, T7, T8>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public virtual object SqlProcedureScalar(string procName, params DbParameter[] parameters)
        {
            return Database.ExecuteScalar(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public virtual object SqlProcedureScalar(string procName, object model)
        {
            return Database.ExecuteScalar(procName, model, CommandType.StoredProcedure).result;
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public virtual Task<object> SqlProcedureScalarAsync(string procName, params DbParameter[] parameters)
        {
            return Database.ExecuteScalarAsync(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public virtual Task<object> SqlProcedureScalarAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return Database.ExecuteScalarAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public virtual async Task<object> SqlProcedureScalarAsync(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (result, _) = await Database.ExecuteScalarAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return result;
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public virtual TResult SqlProcedureScalar<TResult>(string procName, params DbParameter[] parameters)
        {
            return Database.ExecuteScalar(procName, parameters, CommandType.StoredProcedure).Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public virtual TResult SqlProcedureScalar<TResult>(string procName, object model)
        {
            return Database.ExecuteScalar(procName, model, CommandType.StoredProcedure).result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public virtual async Task<TResult> SqlProcedureScalarAsync<TResult>(string procName, params DbParameter[] parameters)
        {
            var result = await Database.ExecuteScalarAsync(procName, parameters, CommandType.StoredProcedure);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public virtual async Task<TResult> SqlProcedureScalarAsync<TResult>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var result = await Database.ExecuteScalarAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回 单行单列
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public virtual async Task<TResult> SqlProcedureScalarAsync<TResult>(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (result, _) = await Database.ExecuteScalarAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public virtual int SqlProcedureNonQuery(string procName, params DbParameter[] parameters)
        {
            return Database.ExecuteNonQuery(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        public virtual int SqlProcedureNonQuery(string procName, object model)
        {
            return Database.ExecuteNonQuery(procName, model, CommandType.StoredProcedure).rowEffects;
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public virtual Task<int> SqlProcedureNonQueryAsync(string procName, params DbParameter[] parameters)
        {
            return Database.ExecuteNonQueryAsync(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public virtual Task<int> SqlProcedureNonQueryAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return Database.ExecuteNonQueryAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行存储过程无数据返回
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public virtual async Task<int> SqlProcedureNonQueryAsync(string procName, object model, CancellationToken cancellationToken = default)
        {
            var (rowEffects, _) = await Database.ExecuteNonQueryAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);
            return rowEffects;
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public virtual int SqlNonQuery(string sql, params DbParameter[] parameters)
        {
            return Database.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>int</returns>
        public virtual int SqlNonQuery(string sql, object model)
        {
            return Database.ExecuteNonQuery(sql, model).rowEffects;
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>int</returns>
        public virtual Task<int> SqlNonQueryAsync(string sql, params DbParameter[] parameters)
        {
            return Database.ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public virtual Task<int> SqlNonQueryAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return Database.ExecuteNonQueryAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 无数据返回
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>int</returns>
        public virtual async Task<int> SqlNonQueryAsync(string sql, object model, CancellationToken cancellationToken = default)
        {
            var (rowEffects, _) = await Database.ExecuteNonQueryAsync(sql, model, cancellationToken: cancellationToken);
            return rowEffects;
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public virtual object SqlScalar(string sql, params DbParameter[] parameters)
        {
            return Database.ExecuteScalar(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public virtual object SqlScalar(string sql, object model)
        {
            return Database.ExecuteScalar(sql, model).result;
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public virtual Task<object> SqlScalarAsync(string sql, params DbParameter[] parameters)
        {
            return Database.ExecuteScalarAsync(sql, parameters);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public virtual Task<object> SqlScalarAsync(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            return Database.ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public virtual async Task<object> SqlScalarAsync(string sql, object model, CancellationToken cancellationToken = default)
        {
            var (result, _) = await Database.ExecuteScalarAsync(sql, model, cancellationToken: cancellationToken);
            return result;
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public virtual TResult SqlScalar<TResult>(string sql, params DbParameter[] parameters)
        {
            return Database.ExecuteScalar(sql, parameters).Adapt<TResult>();
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public virtual TResult SqlScalar<TResult>(string sql, object model)
        {
            return Database.ExecuteScalar(sql, model).result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public virtual async Task<TResult> SqlScalarAsync<TResult>(string sql, params DbParameter[] parameters)
        {
            var result = await Database.ExecuteScalarAsync(sql, parameters);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public virtual async Task<TResult> SqlScalarAsync<TResult>(string sql, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var result = await Database.ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行 Sql 返回 单行单列
        /// </summary>
        /// <param name="sql">sql 语句</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>TResult</returns>
        public virtual async Task<TResult> SqlScalarAsync<TResult>(string sql, object model, CancellationToken cancellationToken = default)
        {
            var (result, _) = await Database.ExecuteScalarAsync(sql, model, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>ProcedureOutput</returns>
        public virtual ProcedureOutputResult SqlProcedureOutput(string procName, DbParameter[] parameters)
        {
            parameters ??= Array.Empty<DbParameter>();

            // 执行存储过程
            var dataSet = Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure);

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
        public virtual async Task<ProcedureOutputResult> SqlProcedureOutputAsync(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            parameters ??= Array.Empty<DbParameter>();

            // 执行存储过程
            var dataSet = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput(parameters, dataSet);
        }

        /// <summary>
        /// 执行存储过程返回OUPUT、RETURN、结果集
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="model">命令模型</param>
        /// <returns>ProcedureOutput</returns>
        public virtual ProcedureOutputResult SqlProcedureOutput(string procName, object model)
        {
            // 执行存储过程
            var (dataSet, parameters) = Database.DataAdapterFill(procName, model, CommandType.StoredProcedure);

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
        public virtual async Task<ProcedureOutputResult> SqlProcedureOutputAsync(string procName, object model, CancellationToken cancellationToken = default)
        {
            // 执行存储过程
            var (dataSet, parameters) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);

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
        public virtual ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(string procName, DbParameter[] parameters)
        {
            parameters ??= Array.Empty<DbParameter>();

            // 执行存储过程
            var dataSet = Database.DataAdapterFill(procName, parameters, CommandType.StoredProcedure);

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
        public virtual async Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(string procName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            parameters ??= Array.Empty<DbParameter>();

            // 执行存储过程
            var dataSet = await Database.DataAdapterFillAsync(procName, parameters, CommandType.StoredProcedure, cancellationToken: cancellationToken);

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
        public virtual ProcedureOutputResult<TResult> SqlProcedureOutput<TResult>(string procName, object model)
        {
            // 执行存储过程
            var (dataSet, parameters) = Database.DataAdapterFill(procName, model, CommandType.StoredProcedure);

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
        public virtual async Task<ProcedureOutputResult<TResult>> SqlProcedureOutputAsync<TResult>(string procName, object model, CancellationToken cancellationToken = default)
        {
            // 执行存储过程
            var (dataSet, parameters) = await Database.DataAdapterFillAsync(procName, model, CommandType.StoredProcedure, cancellationToken: cancellationToken);

            // 包装结果集
            return DbHelpers.WrapperProcedureOutput<TResult>(parameters, dataSet);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public virtual object SqlFunctionScalar(string funcName, params DbParameter[] parameters)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            return Database.ExecuteScalar(sql, parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>object</returns>
        public virtual object SqlFunctionScalar(string funcName, object model)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, model);
            return Database.ExecuteScalar(sql, model).result;
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>object</returns>
        public virtual Task<object> SqlFunctionScalarAsync(string funcName, params DbParameter[] parameters)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            return Database.ExecuteScalarAsync(sql, parameters);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public virtual Task<object> SqlFunctionScalarAsync(string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            return Database.ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>object</returns>
        public virtual async Task<object> SqlFunctionScalarAsync(string funcName, object model, CancellationToken cancellationToken = default)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, model);
            var (result, _) = await Database.ExecuteScalarAsync(sql, model, cancellationToken: cancellationToken);
            return result;
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public virtual TResult SqlFunctionScalar<TResult>(string funcName, params DbParameter[] parameters)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            return Database.ExecuteScalar(sql, parameters).Adapt<TResult>();
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>TResult</returns>
        public virtual TResult SqlFunctionScalar<TResult>(string funcName, object model)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, model);
            return Database.ExecuteScalar(sql, model).result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行标量函数返回 单行单列
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>TResult</returns>
        public virtual async Task<TResult> SqlFunctionScalarAsync<TResult>(string funcName, params DbParameter[] parameters)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            var result = await Database.ExecuteScalarAsync(sql, parameters);
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
        public virtual async Task<TResult> SqlFunctionScalarAsync<TResult>(string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, parameters);
            var result = await Database.ExecuteScalarAsync(sql, parameters, cancellationToken: cancellationToken);
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
        public virtual async Task<TResult> SqlFunctionScalarAsync<TResult>(string funcName, object model, CancellationToken cancellationToken = default)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Scalar, funcName, model);
            var (result, _) = await Database.ExecuteScalarAsync(sql, model, cancellationToken: cancellationToken);
            return result.Adapt<TResult>();
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>DataTable</returns>
        public virtual DataTable SqlFunctionQuery(string funcName, params DbParameter[] parameters)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, parameters);
            return Database.ExecuteReader(sql, parameters);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>DataTable</returns>
        public virtual DataTable SqlFunctionQuery(string funcName, object model)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, model);
            return Database.ExecuteReader(sql, model).dataTable;
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{DataTable}</returns>
        public virtual Task<DataTable> SqlFunctionQueryAsync(string funcName, params DbParameter[] parameters)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, parameters);
            return Database.ExecuteReaderAsync(sql, parameters);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public virtual Task<DataTable> SqlFunctionQueryAsync(string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, parameters);
            return Database.ExecuteReaderAsync(sql, parameters, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 执行表值函数返回 DataTable
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>Task{DataTable}</returns>
        public virtual async Task<DataTable> SqlFunctionQueryAsync(string funcName, object model, CancellationToken cancellationToken = default)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, model);
            var (dataTable, _) = await Database.ExecuteReaderAsync(sql, model, cancellationToken: cancellationToken);
            return dataTable;
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>List{T}</returns>
        public virtual List<T> SqlFunctionQuery<T>(string funcName, params DbParameter[] parameters)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, parameters);
            return Database.ExecuteReader(sql, parameters).ToList<T>();
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="model">参数模型</param>
        /// <returns>List{T}</returns>
        public virtual List<T> SqlFunctionQuery<T>(string funcName, object model)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, model);
            return Database.ExecuteReader(sql, model).dataTable.ToList<T>();
        }

        /// <summary>
        /// 执行表值函数返回 List 集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="funcName">函数名</param>
        /// <param name="parameters">命令参数</param>
        /// <returns>Task{List{T}}</returns>
        public virtual async Task<List<T>> SqlFunctionQueryAsync<T>(string funcName, params DbParameter[] parameters)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, parameters);
            var dataTable = await Database.ExecuteReaderAsync(sql, parameters);
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
        public virtual async Task<List<T>> SqlFunctionQueryAsync<T>(string funcName, DbParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, parameters);
            var dataTable = await Database.ExecuteReaderAsync(sql, parameters, cancellationToken: cancellationToken);
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
        public virtual async Task<List<T>> SqlFunctionQueryAsync<T>(string funcName, object model, CancellationToken cancellationToken = default)
        {
            var sql = DbHelpers.GenerateFunctionSql(Database.ProviderName, DbFunctionType.Table, funcName, model);
            var (dataTable, _) = await Database.ExecuteReaderAsync(sql, model, cancellationToken: cancellationToken);
            return dataTable.ToList<T>();
        }
    }
}