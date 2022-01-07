// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 数据库上下文提交拦截器
    /// </summary>
    [SuppressSniffer]
    public class DbContextSaveChangesInterceptor : SaveChangesInterceptor
    {
        /// <summary>
        /// 拦截保存数据库之前
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            dynamic dbContext = eventData.Context;
            dbContext.SavingChangesEventInner(eventData, result);

            return base.SavingChanges(eventData, result);
        }

        /// <summary>
        /// 拦截保存数据库之前
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            dynamic dbContext = eventData.Context;
            dbContext.SavingChangesEventInner(eventData, result);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// 拦截保存数据库成功
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            dynamic dbContext = eventData.Context;
            dbContext.SavedChangesEventInner(eventData, result);

            return base.SavedChanges(eventData, result);
        }

        /// <summary>
        /// 拦截保存数据库成功
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            dynamic dbContext = eventData.Context;
            dbContext.SavedChangesEventInner(eventData, result);

            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// 拦截保存数据库失败
        /// </summary>
        /// <param name="eventData"></param>
        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            dynamic dbContext = eventData.Context;
            dbContext.SaveChangesFailedEventInner(eventData);

            base.SaveChangesFailed(eventData);
        }

        /// <summary>
        /// 拦截保存数据库失败
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            dynamic dbContext = eventData.Context;
            dbContext.SaveChangesFailedEventInner(eventData);

            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }
    }
}