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
    /// 基于多个数据库多租户模式
    /// </summary>
    public interface IMultiTenantOnDatabase : IPrivateMultiTenant
    {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns></returns>
        string GetDatabaseConnectionString();
    }
}