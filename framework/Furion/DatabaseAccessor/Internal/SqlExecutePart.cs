// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 构建 Sql 字符串执行部件
    /// </summary>
    [SuppressSniffer]
    public sealed partial class SqlExecutePart
    {
        /// <summary>
        /// 静态缺省 Sql 部件
        /// </summary>
        public static SqlExecutePart Default => new();

        /// <summary>
        /// Sql 字符串
        /// </summary>
        public string SqlString { get; private set; }

        /// <summary>
        /// 设置超时时间
        /// </summary>
        public int Timeout { get; private set; }

        /// <summary>
        /// 数据库上下文定位器
        /// </summary>
        public Type DbContextLocator { get; private set; } = typeof(MasterDbContextLocator);

        /// <summary>
        /// 设置服务提供器
        /// </summary>
        public IServiceProvider ContextScoped { get; private set; } = App.HttpContext?.RequestServices;
    }
}