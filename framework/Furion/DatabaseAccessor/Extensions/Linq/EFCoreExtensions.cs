// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// EntityFramework Core 拓展
    /// </summary>
    [SuppressSniffer]
    public static class EFCoreExtensions
    {
        /// <summary>
        /// 根据条件成立再构建 Include 查询
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <typeparam name="TProperty">泛型属性类型</typeparam>
        /// <param name="sources">集合对象</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="expression">新的集合对象表达式</param>
        /// <returns></returns>
        public static IIncludableQueryable<TSource, TProperty> Include<TSource, TProperty>(this IQueryable<TSource> sources, bool condition, Expression<Func<TSource, TProperty>> expression) where TSource : class
        {
            return condition ? sources.Include(expression) : (IIncludableQueryable<TSource, TProperty>)sources;
        }
    }
}