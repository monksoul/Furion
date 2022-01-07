// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// Sql 执行代理依赖接口
    /// </summary>
    public interface ISqlDispatchProxy
    {
        /// <summary>
        /// 切换数据库上下文定位器
        /// </summary>
        /// <typeparam name="TDbContextLocator"></typeparam>
        /// <returns></returns>
        public void Change<TDbContextLocator>()
            where TDbContextLocator : IDbContextLocator
        { }

        /// <summary>
        /// 重置运行时数据库上下文定位器
        /// </summary>
        public void ResetIt() { }
    }
}