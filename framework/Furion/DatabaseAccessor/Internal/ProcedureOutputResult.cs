// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System.Collections.Generic;
using System.Data;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 存储过程输出返回值
    /// </summary>
    [SuppressSniffer]
    public sealed class ProcedureOutputResult : ProcedureOutputResult<DataSet>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcedureOutputResult() : base()
        {
        }
    }

    /// <summary>
    /// 存储过程输出返回值
    /// </summary>
    /// <typeparam name="TResult">泛型版本</typeparam>
    [SuppressSniffer]
    public class ProcedureOutputResult<TResult>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcedureOutputResult()
        {
            OutputValues = new List<ProcedureOutputValue>();
        }

        /// <summary>
        /// 输出值
        /// </summary>
        public IEnumerable<ProcedureOutputValue> OutputValues { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// 结果集
        /// </summary>
        public TResult Result { get; set; }
    }
}