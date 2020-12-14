using Fur.DependencyInjection;

namespace System.Collections.Generic
{
    /// <summary>
    /// 分页泛型集合
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [SkipScan]
    public class PagedList<TEntity>
        where TEntity : class, new()
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
    public class PagedList : PagedList<object>
    {
    }
}