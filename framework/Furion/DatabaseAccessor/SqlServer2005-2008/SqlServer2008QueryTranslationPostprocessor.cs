// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// SqlServer 查询转换器
    /// </summary>
    internal class SqlServer2008QueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="relationalDependencies"></param>
        /// <param name="queryCompilationContext"></param>
        public SqlServer2008QueryTranslationPostprocessor(QueryTranslationPostprocessorDependencies dependencies, RelationalQueryTranslationPostprocessorDependencies relationalDependencies, QueryCompilationContext queryCompilationContext)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
        }

        /// <summary>
        /// 替换分页语句
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override Expression Process(Expression query)
        {
            query = base.Process(query);
            query = new SqlServer2008OffsetToRowNumberConvertVisitor(query, RelationalDependencies.SqlExpressionFactory).Visit(query);
            return query;
        }
    }
}