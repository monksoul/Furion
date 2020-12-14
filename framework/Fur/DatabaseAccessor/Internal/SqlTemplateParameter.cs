using Fur.DependencyInjection;
using System.Data;

namespace Fur.DatabaseAccessor.Models
{
    /// <summary>
    /// Sql 模板参数
    /// </summary>
    [SkipScan]
    internal class SqlTemplateParameter
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 参数输出方向
        /// </summary>
        public ParameterDirection? Direction { get; set; } = ParameterDirection.Input;

        /// <summary>
        /// 数据库对应类型
        /// </summary>
        public DbType? DbType { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        /// <remarks>Nvarchar/varchar类型需指定</remarks>
        public int? Size { get; set; }
    }
}