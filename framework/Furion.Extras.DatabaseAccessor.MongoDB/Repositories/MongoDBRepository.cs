// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.Extensions.DependencyInjection;
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
    public partial class MongoDBRepository : IMongoDBRepository
    {
        /// <summary>
        /// 服务提供器
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供器</param>
        /// <param name="db"></param>
        public MongoDBRepository(IServiceProvider serviceProvider, IMongoDatabase db)
        {
            _serviceProvider = serviceProvider;
            DynamicContext = Context = (MongoClient)db.Client;
        }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        public virtual MongoClient Context { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        public virtual dynamic DynamicContext { get; }

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <param name="name"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public virtual IMongoDatabase GetDatabase(string name, MongoDatabaseSettings settings = null)
        {
            return Context.GetDatabase(name, settings);
        }

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TDocument">实体类型</typeparam>
        /// <typeparam name="TKey">主键类型</typeparam>
        /// <returns>仓储</returns>
        public virtual IMongoDBRepository<TDocument, TKey> Change<TDocument, TKey>()
            where TDocument : class, IMongoDBEntity<TKey>, new()
            where TKey : class
        {
            return _serviceProvider.GetService<IMongoDBRepository<TDocument, TKey>>();
        }
    }

    /// <summary>
    /// MongoDB 泛型仓储
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public partial class MongoDBRepository<TDocument, TKey> : IMongoDBRepository<TDocument, TKey>
        where TDocument : class, IMongoDBEntity<TKey>, new()
    {
        /// <summary>
        /// 非泛型 MongoDB 仓储
        /// </summary>
        private readonly IMongoDBRepository _mongoDBRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mongoDBRepository"></param>
        /// <param name="db"></param>
        public MongoDBRepository(IMongoDBRepository mongoDBRepository, IMongoDatabase db)
        {
            _mongoDBRepository = mongoDBRepository;

            DynamicContext = Context = (MongoClient)db.Client;
            Entities = db.GetCollection<TDocument>(typeof(TDocument).Name);
        }

        /// <summary>
        /// 文档集合
        /// </summary>
        public IMongoCollection<TDocument> Entities { get; }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        public virtual MongoClient Context { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        public virtual dynamic DynamicContext { get; }

        /// <summary>
        /// 根据表达式查询多条记录
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual IQueryable<TDocument> Where(Expression<Func<TDocument, bool>> predicate)
        {
            return AsQueryable(predicate);
        }

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<TDocument> AsQueryable()
        {
            return Entities.AsQueryable();
        }

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual IQueryable<TDocument> AsQueryable(Expression<Func<TDocument, bool>> predicate)
        {
            return Entities.AsQueryable().Where(predicate);
        }

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <returns></returns>
        public virtual List<TDocument> AsEnumerable()
        {
            return AsQueryable().ToList();
        }

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual List<TDocument> AsEnumerable(Expression<Func<TDocument, bool>> predicate)
        {
            return AsQueryable(predicate).ToList();
        }

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<TDocument>> AsAsyncEnumerable()
        {
            return await Task.FromResult(AsQueryable().ToList());
        }

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual async Task<List<TDocument>> AsAsyncEnumerable(Expression<Func<TDocument, bool>> predicate)
        {
            return await Task.FromResult(AsQueryable(predicate).ToList());
        }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <returns></returns>
        public virtual bool Exists(Expression<Func<TDocument, bool>> predicate)
        {
            return Entities.AsQueryable().Any(predicate);
        }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <returns></returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<TDocument, bool>> predicate)
        {
            return await Task.FromResult(Entities.AsQueryable().Any(predicate));
        }

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <returns></returns>
        public virtual long Count()
        {
            return Count(new BsonDocument());
        }

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <returns></returns>
        public virtual long Count(Expression<Func<TDocument, bool>> expression)
        {
            return Entities.CountDocuments(expression);
        }

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public virtual long Count(FilterDefinition<TDocument> filter)
        {
            return Entities.CountDocuments(filter);
        }

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <returns></returns>
        public virtual async Task<long> CountAsync()
        {
            return await CountAsync(new BsonDocument());
        }

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <returns></returns>
        public virtual async Task<long> CountAsync(Expression<Func<TDocument, bool>> expression)
        {
            return await Entities.CountDocumentsAsync(expression);
        }

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public virtual async Task<long> CountAsync(FilterDefinition<TDocument> filter)
        {
            return await Entities.CountDocumentsAsync(filter);
        }

        #region 查询

        /// <summary>
        /// 获取单个对象
        /// </summary>
        /// <param name="id">objectId</param>
        /// <returns></returns>
        public virtual TDocument Get(TKey id)
        {
            return Get(Builders<TDocument>.Filter.Eq(d => d.Id, id));
        }

        /// <summary>
        /// 获取单个对象
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public virtual TDocument Get(FilterDefinition<TDocument> filter)
        {
            return Entities.Find(filter).FirstOrDefault();
        }

        /// <summary>
        /// 获取单个对象
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns></returns>
        public virtual TDocument Get(Expression<Func<TDocument, bool>> predicate)
        {
            return Entities.Find(predicate).FirstOrDefault();
        }

        /// <summary>
        /// 异步获取单个对象
        /// </summary>
        /// <param name="id">objectId</param>
        /// <returns></returns>
        public virtual async Task<TDocument> GetAsync(TKey id)
        {
            return await Entities.Find(Builders<TDocument>.Filter.Eq(d => d.Id, id)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 异步获取单个对象
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public virtual async Task<TDocument> GetAsync(FilterDefinition<TDocument> filter)
        {
            return await Entities.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 异步获取单个对象
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <returns></returns>
        public virtual async Task<TDocument> GetAsync(Expression<Func<TDocument, bool>> predicate)
        {
            return await Entities.Find(predicate).FirstOrDefaultAsync();
        }

        #endregion 查询

        #region 插入

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="value">对象</param>
        public virtual void Insert(TDocument value)
        {
            Entities.InsertOne(value);
        }

        /// <summary>
        /// 异步插入
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns></returns>
        public virtual async Task InsertAsync(TDocument value)
        {
            await Entities.InsertOneAsync(value);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="values">对象集合</param>
        public virtual void BatchInsert(IEnumerable<TDocument> values)
        {
            Entities.InsertMany(values);
        }

        /// <summary>
        /// 异步批量插入
        /// </summary>
        /// <param name="values">对象集合</param>
        /// <returns></returns>
        public virtual async Task BatchInsertAsync(IEnumerable<TDocument> values)
        {
            await Entities.InsertManyAsync(values);
        }

        #endregion 插入

        #region 更新

        /// <summary>
        /// 覆盖更新
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns></returns>
        public virtual ReplaceOneResult Update(TDocument value)
        {
            return Entities.ReplaceOne(Builders<TDocument>.Filter.Eq(d => d.Id, value.Id), value);
        }

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        public virtual UpdateResult Update(TKey id, UpdateDefinition<TDocument> update)
        {
            return Update(Builders<TDocument>.Filter.Eq(d => d.Id, id), update);
        }

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        public virtual async Task<UpdateResult> UpdateAsync(TKey id, UpdateDefinition<TDocument> update)
        {
            return await UpdateAsync(Builders<TDocument>.Filter.Eq(d => d.Id, id), update);
        }

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        public virtual UpdateResult Update(Expression<Func<TDocument, bool>> expression, UpdateDefinition<TDocument> update)
        {
            return Entities.UpdateMany(expression, update);
        }

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        public virtual UpdateResult UpdateMany(Expression<Func<TDocument, bool>> expression, UpdateDefinition<TDocument> update)
        {
            return Entities.UpdateMany(expression, update);
        }

        /// <summary>
        /// 局部更新
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        public virtual UpdateResult Update(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
        {
            return Entities.UpdateOne(filter, update);
        }

        /// <summary>
        /// 异步局部更新（仅更新一条记录）
        /// <para><![CDATA[expression 参数示例：x => x.Id == 1 && x.Age > 18 && x.Gender == 0]]></para>
        /// <para><![CDATA[entity 参数示例：y => new T{ RealName = "Ray", Gender = 1}]]></para>
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="entity">更新条件</param>
        /// <returns></returns>
        public virtual UpdateResult Update(Expression<Func<TDocument, bool>> expression, Expression<Action<TDocument>> entity)
        {
            var fieldList = new List<UpdateDefinition<TDocument>>();

            if (entity.Body is MemberInitExpression param)
            {
                foreach (var item in param.Bindings)
                {
                    var propertyName = item.Member.Name;
                    object propertyValue = null;

                    if (item is not MemberAssignment memberAssignment) continue;

                    if (memberAssignment.Expression.NodeType == ExpressionType.Constant)
                    {
                        if (memberAssignment.Expression is ConstantExpression constantExpression)
                            propertyValue = constantExpression.Value;
                    }
                    else
                    {
                        propertyValue = Expression.Lambda(memberAssignment.Expression, null).Compile().DynamicInvoke();
                    }

                    if (propertyName != "_id") //实体键_id不允许更新
                    {
                        fieldList.Add(Builders<TDocument>.Update.Set(propertyName, propertyValue));
                    }
                }
            }

            return Entities.UpdateOne(expression, Builders<TDocument>.Update.Combine(fieldList));
        }

        /// <summary>
        /// 异步局部更新（仅更新一条记录）
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        public virtual async Task<UpdateResult> UpdateAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
        {
            return await Entities.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// 异步局部更新（仅更新一条记录）
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        public virtual async Task<UpdateResult> UpdateAsync<T>(Expression<Func<TDocument, bool>> expression,
            UpdateDefinition<TDocument> update)
        {
            return await Entities.UpdateOneAsync(expression, update);
        }

        /// <summary>
        /// 异步局部更新（仅更新多条记录）
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="update">更新条件</param>
        /// <returns></returns>
        public virtual async Task<UpdateResult> UpdateManyAsync(Expression<Func<TDocument, bool>> expression,
            UpdateDefinition<TDocument> update)
        {
            return await Entities.UpdateManyAsync(expression, update);
        }

        /// <summary>
        /// 异步局部更新（仅更新一条记录）
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <param name="entity">更新条件</param>
        /// <returns></returns>
        public virtual async Task<UpdateResult> UpdateAsync(Expression<Func<TDocument, bool>> expression,
            Expression<Action<TDocument>> entity)
        {
            var fieldList = new List<UpdateDefinition<TDocument>>();

            if (entity.Body is MemberInitExpression param)
            {
                foreach (var item in param.Bindings)
                {
                    var propertyName = item.Member.Name;
                    object propertyValue = null;

                    if (item is not MemberAssignment memberAssignment) continue;

                    if (memberAssignment.Expression.NodeType == ExpressionType.Constant)
                    {
                        if (memberAssignment.Expression is ConstantExpression constantExpression)
                            propertyValue = constantExpression.Value;
                    }
                    else
                    {
                        propertyValue = Expression.Lambda(memberAssignment.Expression, null).Compile().DynamicInvoke();
                    }

                    if (propertyName != "_id") //实体键_id不允许更新
                    {
                        fieldList.Add(Builders<TDocument>.Update.Set(propertyName, propertyValue));
                    }
                }
            }
            return await Entities.UpdateOneAsync(expression, Builders<TDocument>.Update.Combine(fieldList));
        }

        /// <summary>
        /// 异步覆盖更新
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns></returns>
        public virtual async Task<ReplaceOneResult> UpdateAsync(TDocument value)
        {
            return await Entities.ReplaceOneAsync(Builders<TDocument>.Filter.Eq(d => d.Id, value.Id), value);
        }

        #endregion 更新

        #region 删除

        /// <summary>
        /// 删除指定对象
        /// </summary>
        /// <param name="id">对象Id</param>
        /// <returns></returns>
        public virtual DeleteResult Delete(TKey id)
        {
            return Entities.DeleteOne(Builders<TDocument>.Filter.Eq(d => d.Id, id));
        }

        /// <summary>
        /// 删除指定对象
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <returns></returns>
        public virtual DeleteResult Delete(Expression<Func<TDocument, bool>> expression)
        {
            return Entities.DeleteOne(expression);
        }

        /// <summary>
        /// 异步删除指定对象
        /// </summary>
        /// <param name="id">对象Id</param>
        /// <returns></returns>
        public virtual async Task<DeleteResult> DeleteAsync(TKey id)
        {
            return await Entities.DeleteOneAsync(Builders<TDocument>.Filter.Eq(d => d.Id, id));
        }

        /// <summary>
        /// 异步删除指定对象
        /// </summary>
        /// <param name="expression">查询条件</param>
        /// <returns></returns>
        public virtual async Task<DeleteResult> DeleteAsync(Expression<Func<TDocument, bool>> expression)
        {
            return await Entities.DeleteOneAsync(expression);
        }

        /// <summary>
        /// 批量删除对象
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        public virtual DeleteResult BatchDelete(IEnumerable<ObjectId> ids)
        {
            var filter = Builders<TDocument>.Filter.In("_id", ids);
            return Entities.DeleteMany(filter);
        }

        /// <summary>
        /// 批量删除对象
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public virtual DeleteResult BatchDelete(FilterDefinition<TDocument> filter)
        {
            return Entities.DeleteMany(filter);
        }

        /// <summary>
        /// 批量删除对象
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <returns></returns>
        public virtual DeleteResult BatchDelete(Expression<Func<TDocument, bool>> expression)
        {
            return Entities.DeleteMany(expression);
        }

        /// <summary>
        /// 异步批量删除对象
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        public virtual async Task<DeleteResult> BatchDeleteAsync(IEnumerable<ObjectId> ids)
        {
            var filter = Builders<TDocument>.Filter.In("_id", ids);
            return await Entities.DeleteManyAsync(filter);
        }

        /// <summary>
        /// 异步批量删除对象
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public virtual async Task<DeleteResult> BatchDeleteAsync(FilterDefinition<TDocument> filter)
        {
            return await Entities.DeleteManyAsync(filter);
        }

        /// <summary>
        /// 异步批量删除对象
        /// </summary>
        /// <param name="expression">筛选条件</param>
        /// <returns></returns>
        public virtual async Task<DeleteResult> BatchDeleteAsync(Expression<Func<TDocument, bool>> expression)
        {
            return await Entities.DeleteManyAsync(expression);
        }

        #endregion 删除

        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TChangeEntity">实体类型</typeparam>
        /// <typeparam name="TChangeKey">主键类型</typeparam>
        /// <returns>仓储</returns>
        public virtual IMongoDBRepository<TChangeEntity, TChangeKey> Change<TChangeEntity, TChangeKey>()
            where TChangeEntity : class, IMongoDBEntity<TChangeKey>, new()
            where TChangeKey : class
        {
            return _mongoDBRepository.Change<TChangeEntity, TChangeKey>();
        }
    }

    /// <summary>
    /// MongoDB 泛型仓储
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    public partial class MongoDBRepository<TDocument> : MongoDBRepository<TDocument, ObjectId>, IMongoDBRepository<TDocument>
        where TDocument : class, IMongoDBEntity<ObjectId>, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mongoDBRepository"></param>
        /// <param name="db"></param>
        public MongoDBRepository(IMongoDBRepository mongoDBRepository, IMongoDatabase db)
            :base(mongoDBRepository,db)
        {

        }
    }
}