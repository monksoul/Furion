// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

namespace Furion.Schedule;

/// <summary>
/// 作业计划工厂服务
/// </summary>
public partial interface ISchedulerFactory : IDisposable
{
    /// <summary>
    /// 查找所有作业
    /// </summary>
    /// <param name="group">作业组名称</param>
    /// <param name="active">是否是有效的作业</param>
    /// <returns><see cref="IEnumerable{IScheduler}"/></returns>
    IEnumerable<IScheduler> GetJobs(string group = default, bool active = false);

    /// <summary>
    /// 查找所有作业并转换成 <see cref="SchedulerModel"/>
    /// </summary>
    /// <param name="group">作业组名称</param>
    /// <param name="active">是否是有效的作业</param>
    /// <returns><see cref="IEnumerable{SchedulerModel}"/></returns>
    IEnumerable<SchedulerModel> GetJobsOfModels(string group = default, bool active = false);

    /// <summary>
    /// 查找下一批触发的作业
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <param name="group">作业组名称</param>
    /// <returns><see cref="IEnumerable{IScheduler}"/></returns>
    IEnumerable<IScheduler> GetNextRunJobs(DateTime startAt, string group = default);

    /// <summary>
    /// 查找下一批触发的作业并转换成 <see cref="SchedulerModel"/>
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <param name="group">作业组名称</param>
    /// <returns><see cref="IEnumerable{SchedulerModel}"/></returns>
    IEnumerable<SchedulerModel> GetNextRunJobsOfModels(DateTime startAt, string group = default);

    /// <summary>
    /// 获取作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="scheduler">作业计划</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryGetJob(string jobId, out IScheduler scheduler);

    /// <summary>
    /// 获取作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <returns><see cref="IScheduler"/></returns>
    IScheduler GetJob(string jobId);

    /// <summary>
    /// 保存作业
    /// </summary>
    /// <param name="schedulerBuilder">作业计划构建器</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TrySaveJob(SchedulerBuilder schedulerBuilder, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 保存作业
    /// </summary>
    /// <param name="schedulerBuilders">作业计划构建器集合</param>
    void SaveJob(params SchedulerBuilder[] schedulerBuilders);

    /// <summary>
    /// 添加作业组作业
    /// </summary>
    /// <param name="groupSet">作业组名称</param>
    /// <param name="setAction"><see cref="Action"/></param>
    void GroupSet(string groupSet, Action setAction);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="schedulerBuilder">作业计划构建器</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob(SchedulerBuilder schedulerBuilder, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="schedulerBuilders">作业计划构建器集合</param>
    void AddJob(params SchedulerBuilder[] schedulerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobBuilder">作业信息构建器</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob(JobBuilder jobBuilder, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobBuilder">作业信息构建器</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(JobBuilder jobBuilder, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <remarks><see cref="ScheduleResult"/></remarks>
    ScheduleResult TryAddJob<TJob>(TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true)
         where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="buildJob">作业构建器委托</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <remarks><see cref="ScheduleResult"/></remarks>
    ScheduleResult TryAddJob<TJob>(Action<JobBuilder> buildJob, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true)
        where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobType"><see cref="IJob"/> 实现类型</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <remarks><see cref="ScheduleResult"/></remarks>
    ScheduleResult TryAddJob(Type jobType, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="dynamicExecuteAsync">运行时动态作业执行逻辑</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <remarks><see cref="ScheduleResult"/></remarks>
    ScheduleResult TryAddJob(Func<JobExecutingContext, CancellationToken, Task> dynamicExecuteAsync, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob<TJob>(params TriggerBuilder[] triggerBuilders)
         where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    ///  <param name="buildJob">作业构建器委托</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob<TJob>(Action<JobBuilder> buildJob, params TriggerBuilder[] triggerBuilders)
        where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobType"><see cref="IJob"/> 实现类型</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(Type jobType, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="dynamicExecuteAsync">运行时动态作业执行逻辑</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(Func<JobExecutingContext, CancellationToken, Task> dynamicExecuteAsync, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="jobId">作业 Id</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob<TJob>(string jobId, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true)
         where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobType"><see cref="IJob"/> 实现类型</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob(Type jobType, string jobId, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="dynamicExecuteAsync">运行时动态作业执行逻辑</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob(Func<JobExecutingContext, CancellationToken, Task> dynamicExecuteAsync, string jobId, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="jobId">作业 Id</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob<TJob>(string jobId, params TriggerBuilder[] triggerBuilders)
         where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobType"><see cref="IJob"/> 实现类型</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(Type jobType, string jobId, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="dynamicExecuteAsync">运行时动态作业执行逻辑</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(Func<JobExecutingContext, CancellationToken, Task> dynamicExecuteAsync, string jobId, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="jobId">作业 Id</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob<TJob>(string jobId, bool concurrent, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true)
         where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobType"><see cref="IJob"/> 实现类型</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob(Type jobType, string jobId, bool concurrent, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="dynamicExecuteAsync">运行时动态作业执行逻辑</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob(Func<JobExecutingContext, CancellationToken, Task> dynamicExecuteAsync, string jobId, bool concurrent, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="jobId">作业 Id</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob<TJob>(string jobId, bool concurrent, params TriggerBuilder[] triggerBuilders)
         where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobType"><see cref="IJob"/> 实现类型</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(Type jobType, string jobId, bool concurrent, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="dynamicExecuteAsync">运行时动态作业执行逻辑</param>
    /// <param name="jobId">作业 Id</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(Func<JobExecutingContext, CancellationToken, Task> dynamicExecuteAsync, string jobId, bool concurrent, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob<TJob>(bool concurrent, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true)
         where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobType"><see cref="IJob"/> 实现类型</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob(Type jobType, bool concurrent, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="dynamicExecuteAsync">运行时动态作业执行逻辑</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddJob(Func<JobExecutingContext, CancellationToken, Task> dynamicExecuteAsync, bool concurrent, TriggerBuilder[] triggerBuilders, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob<TJob>(bool concurrent, params TriggerBuilder[] triggerBuilders)
         where TJob : class, IJob;

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="jobType"><see cref="IJob"/> 实现类型</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(Type jobType, bool concurrent, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <param name="dynamicExecuteAsync">运行时动态作业执行逻辑</param>
    /// <param name="concurrent">是否采用并发执行</param>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void AddJob(Func<JobExecutingContext, CancellationToken, Task> dynamicExecuteAsync, bool concurrent, params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 更新作业
    /// </summary>
    /// <param name="schedulerBuilder">作业计划构建器</param>
    /// <param name="scheduler">新的作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryUpdateJob(SchedulerBuilder schedulerBuilder, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 更新作业
    /// </summary>
    /// <param name="schedulerBuilders">作业计划构建器集合</param>
    void UpdateJob(params SchedulerBuilder[] schedulerBuilders);

    /// <summary>
    /// 删除作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryRemoveJob(string jobId, out IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 删除作业
    /// </summary>
    /// <param name="jobIds">作业 Id 集合</param>
    void RemoveJob(params string[] jobIds);

    /// <summary>
    /// 删除作业
    /// </summary>
    /// <param name="scheduler">作业计划</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryRemoveJob(IScheduler scheduler, bool immediately = true);

    /// <summary>
    /// 删除作业
    /// </summary>
    /// <param name="schedulers">作业计划集合</param>
    void RemoveJob(params IScheduler[] schedulers);

    /// <summary>
    /// 检查作业是否存在
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="group">作业组名称</param>
    /// <returns><see cref="bool"/></returns>
    bool ContainsJob(string jobId, string group = default);

    /// <summary>
    /// 启动所有作业
    /// </summary>
    /// <param name="group">作业组名称</param>
    void StartAll(string group = default);

    /// <summary>
    /// 暂停所有作业
    /// </summary>
    /// <param name="group">作业组名称</param>
    void PauseAll(string group = default);

    /// <summary>
    /// 删除所有作业
    /// </summary>
    /// <param name="group">作业组名称</param>
    void RemoveAll(string group = default);

    /// <summary>
    /// 强制触发所有作业持久化记录
    /// </summary>
    /// <param name="group">作业组名称</param>
    void PersistAll(string group = default);

    /// <summary>
    /// 校对所有作业
    /// </summary>
    /// <param name="group">作业组名称</param>
    void CollateAll(string group = default);

    /// <summary>
    /// 手动执行作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="customData">自定义数据</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryRunJob(string jobId, out IScheduler scheduler, string triggerId = null, IDictionary<string, object>? customData = null);

    /// <summary>
    /// 手动执行作业
    /// </summary>
    /// <param name="jobIds">作业 Id 集合</param>
    void RunJob(params string[] jobIds);

    /// <summary>
    /// 手动执行作业
    /// </summary>
    /// <param name="scheduler">作业计划</param>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="customData">自定义数据</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryRunJob(IScheduler scheduler, string triggerId = null, IDictionary<string, object>? customData = null);

    /// <summary>
    /// 手动执行作业
    /// </summary>
    /// <param name="schedulers">作业计划集合</param>
    void RunJob(params IScheduler[] schedulers);

    /// <summary>
    /// 取消正在执行的作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="scheduler">作业计划</param>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryCancelJob(string jobId, out IScheduler scheduler, string triggerId = null);

    /// <summary>
    /// 取消正在执行的作业
    /// </summary>
    /// <param name="jobIds">作业 Id 集合</param>
    void CancelJob(params string[] jobIds);

    /// <summary>
    /// 取消正在执行的作业
    /// </summary>
    /// <param name="scheduler">作业计划</param>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryCancelJob(IScheduler scheduler, string triggerId = null);

    /// <summary>
    /// 取消正在执行的作业
    /// </summary>
    /// <param name="schedulers">作业计划集合</param>
    void CancelJob(params IScheduler[] schedulers);

    /// <summary>
    /// 启动作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="scheduler">作业计划</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryStartJob(string jobId, out IScheduler scheduler);

    /// <summary>
    /// 启动作业
    /// </summary>
    /// <param name="jobIds">作业 Id 集合</param>
    void StartJob(params string[] jobIds);

    /// <summary>
    /// 启动作业
    /// </summary>
    /// <param name="scheduler">作业计划</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryStartJob(IScheduler scheduler);

    /// <summary>
    /// 启动作业
    /// </summary>
    /// <param name="schedulers">作业计划集合</param>
    void StartJob(params IScheduler[] schedulers);

    /// <summary>
    /// 暂停作业
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="scheduler">作业计划</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryPauseJob(string jobId, out IScheduler scheduler);

    /// <summary>
    /// 暂停作业
    /// </summary>
    /// <param name="jobIds">作业 Id 集合</param>
    void PauseJob(params string[] jobIds);

    /// <summary>
    /// 暂停作业
    /// </summary>
    /// <param name="scheduler">作业计划</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryPauseJob(IScheduler scheduler);

    /// <summary>
    /// 暂停作业
    /// </summary>
    /// <param name="schedulers">作业计划集合</param>
    void PauseJob(params IScheduler[] schedulers);
}