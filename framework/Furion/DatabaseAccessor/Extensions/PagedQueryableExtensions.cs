// Copyright (c) 2020-2021 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 分部拓展类
    /// </summary>
    [SuppressSniffer]
    public static class PagedQueryableExtensions
    {
        /// <summary>
        /// 分页拓展
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <param name="pageIndex">页码，必须大于0</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static PagedList<TEntity> ToPagedList<TEntity>(this IQueryable<TEntity> entities, int pageIndex = 1, int pageSize = 20)
        {
            if (pageIndex <= 0) throw new InvalidOperationException($"{nameof(pageIndex)} must be a positive integer greater than 0.");

            var totalCount = entities.Count();
            var items = entities.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedList<TEntity>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPages = pageIndex < totalPages,
                HasPrevPages = pageIndex - 1 > 0
            };
        }

        /// <summary>
        /// 分页拓展
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <param name="pageIndex">页码，必须大于0</param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<PagedList<TEntity>> ToPagedListAsync<TEntity>(this IQueryable<TEntity> entities, int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default)
            where TEntity : new()
        {
            if (pageIndex <= 0) throw new InvalidOperationException($"{nameof(pageIndex)} must be a positive integer greater than 0.");

            var totalCount = await entities.CountAsync(cancellationToken);
            var items = await entities.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedList<TEntity>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPages = pageIndex < totalPages,
                HasPrevPages = pageIndex - 1 > 0
            };
        }
    }
}