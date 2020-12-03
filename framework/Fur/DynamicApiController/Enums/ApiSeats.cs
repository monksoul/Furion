using Fur.DependencyInjection;
using System.ComponentModel;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 接口参数位置
    /// </summary>
    [SkipScan]
    public enum ApiSeats
    {
        /// <summary>
        /// 控制器之前
        /// </summary>
        [Description("控制器之前")]
        ControllerStart,

        /// <summary>
        /// 控制器之后
        /// </summary>
        [Description("控制器之后")]
        ControllerEnd,

        /// <summary>
        /// 行为之前
        /// </summary>
        [Description("行为之前")]
        ActionStart,

        /// <summary>
        /// 行为之后
        /// </summary>
        [Description("行为之后")]
        ActionEnd
    }
}