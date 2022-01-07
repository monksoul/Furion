// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.Data;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// DbParameter 配置特性
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Property)]
    public sealed class DbParameterAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public DbParameterAttribute()
        {
            Direction = ParameterDirection.Input;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="direction">参数方向</param>
        public DbParameterAttribute(ParameterDirection direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// 参数输出方向
        /// </summary>
        public ParameterDirection Direction { get; set; }

        /// <summary>
        /// 数据库对应类型
        /// </summary>
        public object DbType { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        /// <remarks>Nvarchar/varchar类型需指定</remarks>
        public int Size { get; set; }
    }
}