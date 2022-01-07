// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Reflection;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// Sql 代理方法元数据
    /// </summary>
    [SuppressSniffer]
    public sealed class SqlProxyMethod
    {
        /// <summary>
        /// 参数模型
        /// </summary>
        public object ParameterModel { get; set; }

        /// <summary>
        /// 方法返回值
        /// </summary>
        public Type ReturnType { get; internal set; }

        /// <summary>
        /// 数据库操作上下文
        /// </summary>
        public DbContext Context { get; set; }

        /// <summary>
        /// 是否是异步方法
        /// </summary>
        public bool IsAsync { get; internal set; }

        /// <summary>
        /// 命令类型
        /// </summary>
        public CommandType CommandType { get; set; }

        /// <summary>
        /// 最终 Sql 语句
        /// </summary>
        public string FinalSql { get; set; }

        /// <summary>
        /// 当前执行的方法
        /// </summary>
        public MethodInfo Method { get; internal set; }

        /// <summary>
        /// 传递参数
        /// </summary>
        public object[] Arguments { get; internal set; }

        /// <summary>
        /// 拦截Id
        /// </summary>
        public string InterceptorId { get; internal set; }
    }
}