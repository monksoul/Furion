// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDB.Driver
{
    /// <summary>
    /// MongoDB 仓储
    /// </summary>
    public partial interface IMongoDBRepository
    {
        /// <summary>
        /// 连接上下文
        /// </summary>
        MongoClient Context { get; }

        /// <summary>
        /// 动态连接上下文
        /// </summary>
        dynamic DynamicContext { get; }

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <param name="name"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        IMongoDatabase GetDatabase(string name, MongoDatabaseSettings settings = null);

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TDocument">实体类型</typeparam>
        /// <typeparam name="TKey">主键类型</typeparam>
        /// <returns>仓储</returns>
        IMongoDBRepository<TDocument, TKey> Change<TDocument, TKey>()
            where TDocument : class, IMongoDBEntity<TKey>, new()
            where TKey : class;
    }

    /// <summary>
    /// MongoDB 泛型仓储
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public partial interface IMongoDBRepository<TDocument, TKey>
        where TDocument : IMongoDBEntity<TKey>, new()
    {
        /// <summary>
        /// 文档集合
        /// </summary>
        public IMongoCollection<TDocument> Entities { get; }

        /// <summary>
        /// 连接上下文
        /// </summary>
        MongoClient Context { get; }

        /// <summary>
        /// 动态连接上下文
        /// </summary>
        dynamic DynamicContext { get; }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <returns></returns>
        bool Exists(Expression<Func<TDocument, bool>> predicate);

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Expression<Func<TDocument, bool>> predicate);

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <returns></returns>
        long Count();

        /// <summary>
        /// 异步获取记录数
        /// </summary>
        /// <returns></returns>
        Task<long> CountAsync();

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <returns></returns>
        long Count(Expression<Func<TDocument, bool>> expression);

        /// <summary>
        /// 异步获取记录数
        /// </summary>
        /// <returns></returns>
        Task<long> CountAsync(Expression<Func<TDocument, bool>> expression);

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <returns></returns>
        long Count(FilterDefinition<TDocument> filter);

        /// <summary>
        /// 异步获取记录数
        /// </summary>
        /// <returns></returns>
        Task<long> CountAsync(FilterDefinition<TDocument> filter);

        /// <summary>
        /// 获取单个对象
        /// </summary>
        /// <param name="id">objectId</param>
        /// <returns></returns>
        TDocument Get(TKey id);

        /// <summary>
        /// 获取单个对象
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <returns></returns>
        TDocument Get(Expression<Func<TDocument, bool>> expression);

        /// <summary>
        /// 获取单个对象
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        TDocument Get(FilterDefinition<TDocument> filter);

        /// <summary>
        /// 异步获取单个对象
        /// </summary>
        /// <param name="id">objectId</param>
        /// <returns></returns>
        Task<TDocument> GetAsync(TKey id);

        /// <summary>
        /// 异步获取单个对象
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <returns></returns>
        Task<TDocument> GetAsync(Expression<Func<TDocument, bool>> expression);

        /// <summary>
        /// 异步获取单个对象
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        Task<TDocument> GetAsync(FilterDefinition<TDocument> filter);

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="value">对象</param>
        void Insert(TDocument value);

        /// <summary>
        /// 异步插入
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns></returns>
        Task InsertAsync(TDocument value);

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="values">对象集合</param>
        void BatchInsert(IEnumerable<TDocument> values);

        /// <summary>
        /// 异步批量插入
        /// </summary>
        /// <param name="values">对象集合</param>
        /// <returns></returns>
        Task BatchInsertAsync(IEnumerable<TDocument> values);

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        UpdateResult Update(TKey id, UpdateDefinition<TDocument> update);

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        Task<UpdateResult> UpdateAsync(TKey id, UpdateDefinition<TDocument> update);

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        UpdateResult Update(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update);

        /// <summary>
        /// 异步局部更新（仅更新一条记录）
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        Task<UpdateResult> UpdateAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update);

        /// <summary>
        /// 局部更新（仅更新一条记录）
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        UpdateResult Update(Expression<Func<TDocument, bool>> expression,
            UpdateDefinition<TDocument> update);

        /// <summary>
        /// 异步局部更新（仅更新一条记录）
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        Task<UpdateResult> UpdateAsync<T>(Expression<Func<TDocument, bool>> expression,
            UpdateDefinition<TDocument> update);

        /// <summary>
        /// 局部更新（更新多条记录）
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        UpdateResult UpdateMany(Expression<Func<TDocument, bool>> expression,
            UpdateDefinition<TDocument> update);

        /// <summary>
        /// 异步局部更新（更新多条记录）
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        Task<UpdateResult> UpdateManyAsync(Expression<Func<TDocument, bool>> expression,
            UpdateDefinition<TDocument> update);

        /// <summary>
        /// 局部更新（仅更新一条记录）
        /// <para><![CDATA[expression 参数示例：x => x.Id == 1 && x.Age > 18 && x.Gender == 0]]></para>
        /// <para><![CDATA[entity 参数示例：y => new T{ RealName = "Ray", Gender = 1}]]></para>
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="entity">更新条件</param>
        /// <returns></returns>
        UpdateResult Update(Expression<Func<TDocument, bool>> expression,
            Expression<Action<TDocument>> entity);

        /// <summary>
        /// 异步局部更新（仅更新一条记录）
        /// <para><![CDATA[expression 参数示例：x => x.Id == 1 && x.Age > 18 && x.Gender == 0]]></para>
        /// <para><![CDATA[entity 参数示例：y => new T{ RealName = "Ray", Gender = 1}]]></para>
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="entity">更新条件</param>
        /// <returns></returns>
        Task<UpdateResult> UpdateAsync(Expression<Func<TDocument, bool>> expression,
            Expression<Action<TDocument>> entity);

        /// <summary>
        /// 覆盖更新
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns></returns>
        ReplaceOneResult Update(TDocument value);

        /// <summary>
        /// 异步覆盖更新
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns></returns>
        Task<ReplaceOneResult> UpdateAsync(TDocument value);

        /// <summary>
        /// 删除指定对象
        /// </summary>
        /// <param name="id">对象Id</param>
        /// <returns></returns>
        DeleteResult Delete(TKey id);

        /// <summary>
        /// 异步删除指定对象
        /// </summary>
        /// <param name="id">对象Id</param>
        /// <returns></returns>
        Task<DeleteResult> DeleteAsync(TKey id);

        /// <summary>
        /// 删除指定对象
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <returns></returns>
        DeleteResult Delete(Expression<Func<TDocument, bool>> expression);

        /// <summary>
        /// 异步删除指定对象
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <returns></returns>
        Task<DeleteResult> DeleteAsync(Expression<Func<TDocument, bool>> expression);

        /// <summary>
        /// 批量删除对象
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        DeleteResult BatchDelete(IEnumerable<ObjectId> ids);

        /// <summary>
        /// 异步批量删除对象
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        Task<DeleteResult> BatchDeleteAsync(IEnumerable<ObjectId> ids);

        /// <summary>
        /// 批量删除对象
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        DeleteResult BatchDelete(FilterDefinition<TDocument> filter);

        /// <summary>
        /// 异步批量删除对象
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        Task<DeleteResult> BatchDeleteAsync(FilterDefinition<TDocument> filter);

        /// <summary>
        /// 批量删除对象
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <returns></returns>
        DeleteResult BatchDelete(Expression<Func<TDocument, bool>> expression);

        /// <summary>
        /// 异步批量删除对象
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <returns></returns>
        Task<DeleteResult> BatchDeleteAsync(Expression<Func<TDocument, bool>> expression);

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeEntity">实体类型</typeparam>
        /// <typeparam name="TChangeKey">主键类型</typeparam>
        /// <returns>仓储</returns>
        IMongoDBRepository<TChangeEntity, TChangeKey> Change<TChangeEntity, TChangeKey>()
            where TChangeEntity : class, IMongoDBEntity<TChangeKey>, new()
            where TChangeKey : class;

        /// <summary>
        /// 根据表达式查询多条记录
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns></returns>
        IQueryable<TDocument> Where(Expression<Func<TDocument, bool>> predicate);

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <returns></returns>
        IQueryable<TDocument> AsQueryable();

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns></returns>
        IQueryable<TDocument> AsQueryable(Expression<Func<TDocument, bool>> predicate);

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <returns></returns>
        List<TDocument> AsEnumerable();

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <returns></returns>
        Task<List<TDocument>> AsAsyncEnumerable();

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns></returns>

        List<TDocument> AsEnumerable(Expression<Func<TDocument, bool>> predicate);

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns></returns>

        Task<List<TDocument>> AsAsyncEnumerable(Expression<Func<TDocument, bool>> predicate);
    }

    /// <summary>
    /// MongoDB 泛型仓储
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    public partial interface IMongoDBRepository<TDocument> : IMongoDBRepository<TDocument, ObjectId>
        where TDocument : IMongoDBEntity<ObjectId>, new()
    { 
    }
}