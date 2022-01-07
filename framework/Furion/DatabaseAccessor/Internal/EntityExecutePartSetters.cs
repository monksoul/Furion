// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 实体执行部件
    /// </summary>
    public sealed partial class EntityExecutePart<TEntity>
        where TEntity : class, IPrivateEntity, new()
    {
        /// <summary>
        /// 设置实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityExecutePart<TEntity> SetEntity(TEntity entity)
        {
            Entity = entity;
            return this;
        }

        /// <summary>
        /// 设置数据库执行作用域
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public EntityExecutePart<TEntity> SetContextScoped(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null) ContextScoped = serviceProvider;
            return this;
        }

        /// <summary>
        /// 设置数据库上下文定位器
        /// </summary>
        /// <typeparam name="TDbContextLocator"></typeparam>
        /// <returns></returns>
        public EntityExecutePart<TEntity> Change<TDbContextLocator>()
            where TDbContextLocator : class, IDbContextLocator
        {
            return Change(typeof(TDbContextLocator));
        }

        /// <summary>
        /// 设置数据库上下文定位器
        /// </summary>
        /// <returns></returns>
        public EntityExecutePart<TEntity> Change(Type dbContextLocator)
        {
            if (dbContextLocator != null) DbContextLocator = dbContextLocator;
            return this;
        }
    }
}