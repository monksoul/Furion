// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 数据库上下文池
    /// </summary>
    [SuppressSniffer]
    public class DbContextPool : IDbContextPool
    {
        /// <summary>
        ///  MiniProfiler 分类名
        /// </summary>
        private const string MiniProfilerCategory = "Transaction";

        /// <summary>
        /// MiniProfiler 组件状态
        /// </summary>
        private readonly bool InjectMiniProfiler;

        /// <summary>
        /// 是否打印数据库连接信息
        /// </summary>
        private readonly bool IsPrintDbConnectionInfo;

        /// <summary>
        /// 线程安全的数据库上下文集合
        /// </summary>
        private readonly ConcurrentDictionary<Guid, DbContext> _dbContexts;

        /// <summary>
        /// 登记错误的数据库上下文
        /// </summary>
        private readonly ConcurrentDictionary<Guid, DbContext> _failedDbContexts;

        /// <summary>
        /// 服务提供器
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DbContextPool(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            InjectMiniProfiler = App.Settings.InjectMiniProfiler.Value;
            IsPrintDbConnectionInfo = App.Settings.PrintDbConnectionInfo.Value;

            _dbContexts = new ConcurrentDictionary<Guid, DbContext>();
            _failedDbContexts = new ConcurrentDictionary<Guid, DbContext>();
        }

        /// <summary>
        /// 数据库上下文事务
        /// </summary>
        public IDbContextTransaction DbContextTransaction { get; private set; }

        /// <summary>
        /// 获取所有数据库上下文
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<Guid, DbContext> GetDbContexts()
        {
            return _dbContexts;
        }

        /// <summary>
        /// 保存数据库上下文
        /// </summary>
        /// <param name="dbContext"></param>
        public void AddToPool(DbContext dbContext)
        {
            // 跳过非关系型数据库
            if (!dbContext.Database.IsRelational()) return;

            var instanceId = dbContext.ContextId.InstanceId;
            if (!_dbContexts.TryAdd(instanceId, dbContext)) return;

            // 订阅数据库上下文操作失败事件
            dbContext.SaveChangesFailed += (s, e) =>
            {
                // 排除已经存在的数据库上下文
                if (!_failedDbContexts.TryAdd(instanceId, dbContext)) return;

                // 当前事务
                dynamic context = s as DbContext;
                var database = context.Database as DatabaseFacade;
                var currentTransaction = database?.CurrentTransaction;

                // 只有事务不等于空且支持自动回滚
                if (!(currentTransaction != null && context.FailedAutoRollback == true)) return;

                // 获取数据库连接信息
                var connection = database.GetDbConnection();

                // 回滚事务
                currentTransaction.Rollback();

                // 打印事务回滚消息
                App.PrintToMiniProfiler("transaction", "Rollback", $"[Connection Id: {context.ContextId}] / [Database: {connection.Database}]{(IsPrintDbConnectionInfo ? $" / [Connection String: {connection.ConnectionString}]" : string.Empty)}", isError: true);
            };
        }

        /// <summary>
        /// 保存数据库上下文（异步）
        /// </summary>
        /// <param name="dbContext"></param>
        public Task AddToPoolAsync(DbContext dbContext)
        {
            AddToPool(dbContext);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <returns></returns>
        public int SavePoolNow()
        {
            // 查找所有已改变的数据库上下文并保存更改
            return _dbContexts
                .Where(u => u.Value != null && u.Value.ChangeTracker.HasChanges() && !_failedDbContexts.Contains(u))
                .Select(u => u.Value.SaveChanges()).Count();
        }

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <returns></returns>
        public int SavePoolNow(bool acceptAllChangesOnSuccess)
        {
            // 查找所有已改变的数据库上下文并保存更改
            return _dbContexts
                .Where(u => u.Value != null && u.Value.ChangeTracker.HasChanges() && !_failedDbContexts.Contains(u))
                .Select(u => u.Value.SaveChanges(acceptAllChangesOnSuccess)).Count();
        }

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> SavePoolNowAsync(CancellationToken cancellationToken = default)
        {
            // 查找所有已改变的数据库上下文并保存更改
            var tasks = _dbContexts
                .Where(u => u.Value != null && u.Value.ChangeTracker.HasChanges() && !_failedDbContexts.Contains(u))
                .Select(u => u.Value.SaveChangesAsync(cancellationToken));

            // 等待所有异步完成
            var results = await Task.WhenAll(tasks);
            return results.Length;
        }

        /// <summary>
        /// 保存数据库上下文池中所有已更改的数据库上下文（异步）
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> SavePoolNowAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            // 查找所有已改变的数据库上下文并保存更改
            var tasks = _dbContexts
                .Where(u => u.Value != null && u.Value.ChangeTracker.HasChanges() && !_failedDbContexts.Contains(u))
                .Select(u => u.Value.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));

            // 等待所有异步完成
            var results = await Task.WhenAll(tasks);
            return results.Length;
        }

        /// <summary>
        /// 打开事务
        /// </summary>
        /// <param name="ensureTransaction"></param>
        /// <returns></returns>
        public void BeginTransaction(bool ensureTransaction = false)
        {
        // 判断 dbContextPool 中是否包含DbContext，如果是，则使用第一个数据库上下文开启事务，并应用于其他数据库上下文
        EnsureTransaction: if (_dbContexts.Any())
            {
                // 如果共享事务不为空，则直接共享
                if (DbContextTransaction != null) goto ShareTransaction;

                // 先判断是否已经有上下文开启了事务
                var transactionDbContext = _dbContexts.FirstOrDefault(u => u.Value.Database.CurrentTransaction != null);

                DbContextTransaction = transactionDbContext.Value != null
                       ? transactionDbContext.Value.Database.CurrentTransaction
                       // 如果没有任何上下文有事务，则将第一个开启事务
                       : _dbContexts.First().Value.Database.BeginTransaction();

            // 共享事务
            ShareTransaction: ShareTransaction(DbContextTransaction.GetDbTransaction());

                // 打印事务实际开启信息
                App.PrintToMiniProfiler(MiniProfilerCategory, "Began");
            }
            else
            {
                // 判断是否确保事务强制可用（此处是无奈之举）
                if (!ensureTransaction) return;

                var defaultDbContextLocator = Penetrates.DbContextDescriptors.LastOrDefault();
                if (defaultDbContextLocator.Key == null) return;

                // 创建一个新的上下文
                var newDbContext = Db.GetDbContext(defaultDbContextLocator.Key, _serviceProvider);
                DbContextTransaction = newDbContext.Database.BeginTransaction();
                goto EnsureTransaction;
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="isManualSaveChanges"></param>
        /// <param name="exception"></param>
        /// <param name="withCloseAll">是否自动关闭所有连接</param>
        public void CommitTransaction(bool isManualSaveChanges = true, Exception exception = default, bool withCloseAll = false)
        {
            // 判断是否异常
            if (exception == null)
            {
                try
                {
                    // 将所有数据库上下文修改 SaveChanges();，这里另外判断是否需要手动提交
                    var hasChangesCount = !isManualSaveChanges ? SavePoolNow() : 0;

                    // 如果事务为空，则执行完毕后关闭连接
                    if (DbContextTransaction == null) goto CloseAll;

                    // 提交共享事务
                    DbContextTransaction?.Commit();

                    // 打印事务提交消息
                    App.PrintToMiniProfiler(MiniProfilerCategory, "Completed", $"Transaction Completed! Has {hasChangesCount} DbContext Changes.");
                }
                catch
                {
                    // 回滚事务
                    if (DbContextTransaction?.GetDbTransaction()?.Connection != null) DbContextTransaction?.Rollback();

                    // 打印事务回滚消息
                    App.PrintToMiniProfiler(MiniProfilerCategory, "Rollback", isError: true);

                    throw;
                }
                finally
                {
                    if (DbContextTransaction?.GetDbTransaction()?.Connection != null)
                    {
                        DbContextTransaction = null;
                        DbContextTransaction?.Dispose();
                    }
                }
            }
            else
            {
                // 回滚事务
                if (DbContextTransaction?.GetDbTransaction()?.Connection != null) DbContextTransaction?.Rollback();
                DbContextTransaction?.Dispose();
                DbContextTransaction = null;

                // 打印事务回滚消息
                App.PrintToMiniProfiler(MiniProfilerCategory, "Rollback", isError: true);
            }

        // 关闭所有连接
        CloseAll: if (withCloseAll) CloseAll();
        }

        /// <summary>
        /// 释放所有数据库上下文
        /// </summary>
        public void CloseAll()
        {
            if (!_dbContexts.Any()) return;

            foreach (var item in _dbContexts)
            {
                var conn = item.Value.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open) continue;

                conn.Close();
                // 打印数据库关闭信息
                App.PrintToMiniProfiler("sql", $"Close", $"Connection Close()");
            }
        }

        /// <summary>
        /// 设置数据库上下文共享事务
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private void ShareTransaction(DbTransaction transaction)
        {
            // 跳过第一个数据库上下文并设置共享事务
            _ = _dbContexts
                   .Where(u => u.Value != null && ((dynamic)u.Value).UseUnitOfWork == true && u.Value.Database.CurrentTransaction == null)
                   .Select(u => u.Value.Database.UseTransaction(transaction))
                   .Count();
        }
    }
}