using System;

namespace Fur.DatabaseAccessor
{
    /// <summary>
    /// 数据库上下文配置特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AppDbContextAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString"></param>
        public AppDbContextAttribute(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        public AppDbContextAttribute(string connectionString, string providerName)
        {
            ConnectionString = connectionString;
            ProviderName = providerName;
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 数据库提供器名称
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 数据库上下文模式
        /// </summary>
        public DbContextMode Mode { get; set; } = DbContextMode.Cached;

        /// <summary>
        /// 表统一前缀
        /// </summary>
        public string TablePrefix { get; set; }

        /// <summary>
        /// 表统一后缀
        /// </summary>
        public string TableSuffix { get; set; }
    }
}