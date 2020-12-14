using Fur.DependencyInjection;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 存储过程输出值模型
    /// </summary>
    [SkipScan]
    public sealed class ProcedureOutputValue
    {
        /// <summary>
        /// 输出参数名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 输出参数值
        /// </summary>
        public object Value { get; set; }
    }
}