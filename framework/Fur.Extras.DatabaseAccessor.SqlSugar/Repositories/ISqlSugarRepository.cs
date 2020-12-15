using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 非泛型 SqlSugar 仓储
    /// </summary>
    public partial interface ISqlSugarRepository
    {
        /// <summary>
        /// 切换仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>仓储</returns>
        ISqlSugarRepository<TEntity> Change<TEntity>()
            where TEntity : class, new();
    }

    /// <summary>
    /// SqlSugar 仓储接口定义
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial interface ISqlSugarRepository<TEntity>
        where TEntity : class, new()
    {
        /// <summary>
        /// 实体集合
        /// </summary>
        ISugarQueryable<TEntity> Entities { get; }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        ISqlSugarClient DbContext { get; }

        /// <summary>
        /// 动态数据库上下文
        /// </summary>
        dynamic DynamicDbContext { get; }

        /// <summary>
        /// 原生 Ado 对象
        /// </summary>
        IAdo Ado { get; }

        /// <summary>
        /// 新增一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Insert(TEntity entity);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Insert(params TEntity[] entities);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Insert(IEnumerable<TEntity> entities);

        /// <summary>
        /// 新增一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> InsertAsync(TEntity entity);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> InsertAsync(params TEntity[] entities);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> InsertAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// 更新一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Update(TEntity entity);

        /// <summary>
        /// 更新多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Update(params TEntity[] entities);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// 更新一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(TEntity entity);

        /// <summary>
        /// 更新多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(params TEntity[] entities);

        /// <summary>
        /// 新增多条记录
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Delete(TEntity entity);

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        int Delete(object key);

        /// <summary>
        /// 删除多条记录
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        int Delete(params object[] keys);

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(TEntity entity);

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(object key);

        /// <summary>
        /// 删除多条记录
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(params object[] keys);

        /// <summary>
        /// 根据表达式查询多条记录
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ISugarQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 根据表达式查询多条记录
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ISugarQueryable<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <returns></returns>
        ISugarQueryable<TEntity> AsQueryable();

        /// <summary>
        /// 构建查询分析器
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ISugarQueryable<TEntity> AsQueryable(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <returns></returns>
        List<TEntity> AsEnumerable();

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        List<TEntity> AsEnumerable(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <returns></returns>
        Task<List<TEntity>> AsAsyncEnumerable();

        /// <summary>
        /// 直接返回数据库结果
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<List<TEntity>> AsAsyncEnumerable(Expression<Func<TEntity, bool>> predicate);
    }
}