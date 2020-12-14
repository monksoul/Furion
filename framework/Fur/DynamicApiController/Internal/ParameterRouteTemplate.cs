using Fur.DependencyInjection;
using System.Collections.Generic;

namespace Fur.DynamicApiController
{
    /// <summary>
    /// 参数路由模板
    /// </summary>
    [SkipScan]
    internal class ParameterRouteTemplate
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ParameterRouteTemplate()
        {
            ControllerStartTemplates = new List<string>();
            ControllerEndTemplates = new List<string>();
            ActionStartTemplates = new List<string>();
            ActionEndTemplates = new List<string>();
        }

        /// <summary>
        /// 控制器之前的参数
        /// </summary>
        public IList<string> ControllerStartTemplates { get; set; }

        /// <summary>
        /// 控制器之后的参数
        /// </summary>
        public IList<string> ControllerEndTemplates { get; set; }

        /// <summary>
        /// 行为之前的参数
        /// </summary>
        public IList<string> ActionStartTemplates { get; set; }

        /// <summary>
        /// 行为之后的参数
        /// </summary>
        public IList<string> ActionEndTemplates { get; set; }
    }
}