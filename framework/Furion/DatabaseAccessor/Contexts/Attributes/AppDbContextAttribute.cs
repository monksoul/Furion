// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 数据库上下文配置特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AppDbContextAttribute : Attribute
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="slaveDbContextLocators"></param>
        public AppDbContextAttribute(params Type[] slaveDbContextLocators)
        {
            SlaveDbContextLocators = slaveDbContextLocators;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionMetadata">支持数据库连接字符串，配置文件的 ConnectionStrings 中的Key或配置文件的完整的配置路径，如果是内存数据库，则为数据库名称</param>
        /// <param name="slaveDbContextLocators"></param>
        public AppDbContextAttribute(string connectionMetadata, params Type[] slaveDbContextLocators)
        {
            ConnectionMetadata = connectionMetadata;
            SlaveDbContextLocators = slaveDbContextLocators;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionMetadata">支持数据库连接字符串，配置文件的 ConnectionStrings 中的Key或配置文件的完整的配置路径，如果是内存数据库，则为数据库名称</param>
        /// <param name="providerName"></param>
        /// <param name="slaveDbContextLocators"></param>
        public AppDbContextAttribute(string connectionMetadata, string providerName, params Type[] slaveDbContextLocators)
        {
            ConnectionMetadata = connectionMetadata;
            ProviderName = providerName;
            SlaveDbContextLocators = slaveDbContextLocators;
        }

        /// <summary>
        /// 数据库连接元数据
        /// </summary>
        /// <remarks>支持数据库连接字符串，配置文件的 ConnectionStrings 中的Key或配置文件的完整的配置路径，如果是内存数据库，则为数据库名称</remarks>
        public string ConnectionMetadata { get; set; }

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

        /// <summary>
        /// 指定从库定位器
        /// </summary>
        public Type[] SlaveDbContextLocators { get; set; }
    }
}