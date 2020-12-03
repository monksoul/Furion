using Fur.DependencyInjection;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// 验证逻辑
    /// </summary>
    [SkipScan]
    public enum ValidationPattern
    {
        /// <summary>
        /// 全部都要验证通过
        /// </summary>
        [Description("全部验证通过才为真")]
        AllOfThem,

        /// <summary>
        /// 至少一个验证通过
        /// </summary>
        [Description("有一个通过就为真")]
        AtLeastOne
    }
}