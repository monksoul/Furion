// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 实体执行部件
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [SuppressSniffer]
    public sealed partial class EntityExecutePart<TEntity>
        where TEntity : class, IPrivateEntity, new()
    {
        /// <summary>
        /// 静态缺省 Entity 部件
        /// </summary>
        public static EntityExecutePart<TEntity> Default => new();

        /// <summary>
        /// 实体
        /// </summary>
        public TEntity Entity { get; private set; }

        /// <summary>
        /// 数据库上下文定位器
        /// </summary>
        public Type DbContextLocator { get; private set; } = typeof(MasterDbContextLocator);

        /// <summary>
        /// 数据库上下文执行作用域
        /// </summary>
        public IServiceProvider ContextScoped { get; private set; } = App.HttpContext?.RequestServices;
    }
}