// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Collections.Generic;

namespace SqlSugar
{
    /// <summary>
    /// 分页泛型集合
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SqlSugarPagedList<TEntity>
        where TEntity : new()
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页容量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 当前页集合
        /// </summary>
        public IEnumerable<TEntity> Items { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevPages { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPages { get; set; }
    }

    /// <summary>
    /// 分页集合
    /// </summary>
    public class PagedModel : SqlSugarPagedList<object>
    {
    }
}