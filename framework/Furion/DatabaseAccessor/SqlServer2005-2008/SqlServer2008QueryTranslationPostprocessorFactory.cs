// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// SqlServer 查询转换工厂（处理 SqlServer 2008 分页问题）
    /// </summary>
    [SuppressSniffer]
    public class SqlServer2008QueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
    {
        /// <summary>
        /// 查询转换依赖集合
        /// </summary>
        private readonly QueryTranslationPostprocessorDependencies _dependencies;

        /// <summary>
        /// 关系查询转换依赖集合
        /// </summary>
        private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="relationalDependencies"></param>
        public SqlServer2008QueryTranslationPostprocessorFactory(QueryTranslationPostprocessorDependencies dependencies, RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        /// <summary>
        /// 创建查询转换实例工厂
        /// </summary>
        /// <param name="queryCompilationContext"></param>
        /// <returns></returns>
        public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
        {
            return new SqlServer2008QueryTranslationPostprocessor(
                  _dependencies,
                  _relationalDependencies,
                  queryCompilationContext);
        }
    }
}