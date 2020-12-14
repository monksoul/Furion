using Fur.DependencyInjection;

namespace Fur.SpecificationDocument
{
    /// <summary>
    /// 分组排序
    /// </summary>
    [SkipScan]
    internal sealed class GroupOrder
    {
        /// <summary>
        /// 分组名
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 分组排序
        /// </summary>
        public int Order { get; set; }
    }
}