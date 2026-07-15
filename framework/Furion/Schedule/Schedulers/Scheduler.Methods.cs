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
/// 作业计划
/// </summary>
internal sealed partial class Scheduler
{
    /// <summary>
    /// 返回可公开访问的作业计划模型
    /// </summary>
    /// <remarks>常用于接口返回或序列化操作</remarks>
    /// <returns><see cref="SchedulerModel"/></returns>
    public SchedulerModel GetModel()
    {
        return new SchedulerModel
        {
            JobDetail = JobDetail,
            Triggers = Triggers.Values.ToArray()
        };
    }

    /// <summary>
    /// 获取作业计划构建器
    /// </summary>
    /// <returns><see cref="SchedulerBuilder"/></returns>
    public SchedulerBuilder GetBuilder()
    {
        return SchedulerBuilder.From(this);
    }

    /// <summary>
    /// 获取作业信息构建器
    /// </summary>
    /// <returns><see cref="JobBuilder"/></returns>
    public JobBuilder GetJobBuilder()
    {
        return GetBuilder().GetJobBuilder();
    }

    /// <summary>
    /// 获取作业触发器构建器集合
    /// </summary>
    /// <returns><see cref="List{TriggerBuilder}"/></returns>
    public IReadOnlyList<TriggerBuilder> GetTriggerBuilders()
    {
        return GetBuilder().GetTriggerBuilders();
    }

    /// <summary>
    /// 获取作业触发器构建器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder GetTriggerBuilder(string triggerId)
    {
        return GetBuilder().GetTriggerBuilder(triggerId);
    }

    /// <summary>
    /// 查找作业信息
    /// </summary>
    /// <returns><see cref="JobDetail"/></returns>
    public JobDetail GetJobDetail()
    {
        return JobDetail;
    }

    /// <summary>
    /// 查找作业触发器集合
    /// </summary>
    /// <returns><see cref="IEnumerable{Trigger}"/></returns>
    public IEnumerable<Trigger> GetTriggers()
    {
        return Triggers.Values;
    }

    /// <summary>
    /// 查找作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    public ScheduleResult TryGetTrigger(string triggerId, out Trigger trigger)
    {
        return InternalTryGetTrigger(triggerId, out trigger);
    }

    /// <summary>
    /// 查找作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="Trigger"/></returns>
    public Trigger GetTrigger(string triggerId)
    {
        _ = TryGetTrigger(triggerId, out var trigger);
        return trigger;
    }

    /// <summary>
    /// 保存作业触发器
    /// </summary>
    /// <param name="triggerBuilder">作业触发器构建器</param>
    /// <param name="trigger">作业触发器</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    public ScheduleResult TrySaveTrigger(TriggerBuilder triggerBuilder, out Trigger trigger, bool immediately = true)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(triggerBuilder);

        // 解析作业触发器构建器状态
        var isAppended = triggerBuilder.Behavior == PersistenceBehavior.Appended;
        var isUpdated = triggerBuilder.Behavior == PersistenceBehavior.Updated;
        var isRemoved = triggerBuilder.Behavior == PersistenceBehavior.Removed;

        // 原始作业触发器
        Trigger originTrigger = default;

        // 获取作业触发器 Id
        var triggerId = triggerBuilder.TriggerId;

        // 检查更新和删除作业触发器时是否正确配置作业触发器 Id 及检查作业触发器是否存在
        if (isUpdated || isRemoved)
        {
            // 查找作业触发器
            var scheduleResult = InternalTryGetTrigger(triggerId, out originTrigger, true);
            if (scheduleResult != ScheduleResult.Succeed)
            {
                trigger = default;
                return scheduleResult;
            }
        }

        // 构建新的作业触发器
        var newTrigger = triggerBuilder.Build(JobId);
        triggerId = newTrigger.TriggerId;

        // 获取作业触发器构建器
        var schedulerBuilder = GetBuilder();
        if (isAppended) schedulerBuilder.AddTriggerBuilder(triggerBuilder);
        else if (isUpdated) schedulerBuilder.UpdateTriggerBuilder(triggerBuilder);
        else if (isRemoved) schedulerBuilder.RemoveTriggerBuilder(triggerId);
        else { }

        // 更新作业
        if (Factory.TryUpdateJob(schedulerBuilder, out _, immediately) != ScheduleResult.Succeed)
        {
            trigger = null;
            return ScheduleResult.Failed;
        }

        if (isRemoved)
        {
            trigger = originTrigger;

            // 刷新作业
            Reload();
            return ScheduleResult.Succeed;
        }
        else
        {
            // 刷新作业
            Reload();
            return InternalTryGetTrigger(triggerId, out trigger, true);
        }
    }

    /// <summary>
    /// 保存作业触发器
    /// </summary>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    public void SaveTrigger(params TriggerBuilder[] triggerBuilders)
    {
        // 空检查
        if (triggerBuilders == null || triggerBuilders.Length == 0) throw new ArgumentNullException(nameof(triggerBuilders));

        // 逐条将作业触发器构建器保存到作业计划中
        foreach (var triggerBuilder in triggerBuilders)
        {
            _ = TrySaveTrigger(triggerBuilder, out _);
        }
    }

    /// <summary>
    /// 更新作业计划信息
    /// </summary>
    /// <param name="jobBuilder">作业信息构建器</param>
    /// <param name="jobDetail">作业信息</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    public ScheduleResult TryUpdateDetail(JobBuilder jobBuilder, out JobDetail jobDetail)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(jobBuilder);

        // 获取作业信息构建器
        var schedulerBuilder = GetBuilder();
        schedulerBuilder.UpdateJobBuilder(jobBuilder);

        // 更新作业
        var scheduleResult = Factory.TryUpdateJob(schedulerBuilder, out var scheduler);
        if (scheduleResult != ScheduleResult.Succeed)
        {
            jobDetail = null;
            return scheduleResult;
        }

        jobDetail = (scheduler as Scheduler).JobDetail;

        // 刷新作业
        Reload();
        return scheduleResult;
    }

    /// <summary>
    /// 更新作业信息
    /// </summary>
    /// <param name="jobBuilder">作业信息构建器</param>
    public void UpdateDetail(JobBuilder jobBuilder)
    {
        _ = TryUpdateDetail(jobBuilder, out _);
    }

    /// <summary>
    /// 更新作业计划信息
    /// </summary>
    /// <param name="jobBuilderAction">作业信息构建器委托</param>
    /// <param name="jobDetail">作业信息</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    public ScheduleResult TryUpdateDetail(Action<JobBuilder> jobBuilderAction, out JobDetail jobDetail)
    {
        var jobBuilder = GetJobBuilder();
        jobBuilderAction?.Invoke(jobBuilder);
        return TryUpdateDetail(jobBuilder, out jobDetail);
    }

    /// <summary>
    /// 更新作业信息
    /// </summary>
    /// <param name="jobBuilderAction">作业信息构建器委托</param>
    public void UpdateDetail(Action<JobBuilder> jobBuilderAction)
    {
        var jobBuilder = GetJobBuilder();
        jobBuilderAction?.Invoke(jobBuilder);
        UpdateDetail(jobBuilder);
    }

    /// <summary>
    /// 添加作业触发器
    /// </summary>
    /// <param name="triggerBuilder">作业触发器构建器</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    public ScheduleResult TryAddTrigger(TriggerBuilder triggerBuilder, out Trigger trigger)
    {
        return TrySaveTrigger(triggerBuilder?.Appended(), out trigger);
    }

    /// <summary>
    /// 添加作业触发器
    /// </summary>
    /// <param name="triggerBuilders">作业触发器构建器</param>
    public void AddTrigger(params TriggerBuilder[] triggerBuilders)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(triggerBuilders);

        foreach (var triggerBuilder in triggerBuilders)
        {
            _ = TryAddTrigger(triggerBuilder, out _);
        }
    }

    /// <summary>
    /// 更新作业触发器
    /// </summary>
    /// <param name="triggerBuilder">作业触发器构建器</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    public ScheduleResult TryUpdateTrigger(TriggerBuilder triggerBuilder, out Trigger trigger)
    {
        return TrySaveTrigger(triggerBuilder?.Updated(), out trigger);
    }

    /// <summary>
    /// 更新作业触发器
    /// </summary>
    /// <param name="triggerBuilders">作业触发器构建器</param>
    public void UpdateTrigger(params TriggerBuilder[] triggerBuilders)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(triggerBuilders);

        foreach (var triggerBuilder in triggerBuilders)
        {
            _ = TryUpdateTrigger(triggerBuilder, out _);
        }
    }

    /// <summary>
    /// 更新作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="triggerBuilderAction">作业触发器构建器委托</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    public ScheduleResult TryUpdateTrigger(string triggerId, Action<TriggerBuilder> triggerBuilderAction, out Trigger trigger)
    {
        var triggerBuilder = GetTriggerBuilder(triggerId);
        triggerBuilderAction?.Invoke(triggerBuilder);
        return TryUpdateTrigger(triggerBuilder, out trigger);
    }

    /// <summary>
    /// 更新作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="triggerBuilderAction">作业触发器构建器委托</param>
    public void UpdateTrigger(string triggerId, Action<TriggerBuilder> triggerBuilderAction)
    {
        var triggerBuilder = GetTriggerBuilder(triggerId);
        triggerBuilderAction?.Invoke(triggerBuilder);
        UpdateTrigger(triggerBuilder);
    }

    /// <summary>
    /// 删除作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    public ScheduleResult TryRemoveTrigger(string triggerId, out Trigger trigger)
    {
        return TrySaveTrigger(TriggerBuilder.Create(triggerId).Removed(), out trigger);
    }

    /// <summary>
    /// 删除作业触发器
    /// </summary>
    /// <param name="triggerIds">作业触发器 Id 集合</param>
    public void RemoveTrigger(params string[] triggerIds)
    {
        // 空检查
        if (triggerIds == null || triggerIds.Length == 0) throw new ArgumentNullException(nameof(triggerIds));

        foreach (var triggerId in triggerIds)
        {
            _ = TryRemoveTrigger(triggerId, out _);
        }
    }

    /// <summary>
    /// 将当前作业计划从调度器中删除
    /// </summary>
    /// <remarks><see cref="ScheduleResult"/></remarks>
    public ScheduleResult TryRemove()
    {
        return Factory.TryRemoveJob(this);
    }

    /// <summary>
    /// 将当前作业计划从调度器中删除
    /// </summary>
    public void Remove()
    {
        _ = TryRemove();
    }

    /// <summary>
    /// 检查作业触发器是否存在
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="bool"/></returns>
    public bool ContainsTrigger(string triggerId)
    {
        return InternalTryGetTrigger(triggerId, out _) == ScheduleResult.Succeed;
    }

    /// <summary>
    /// 启动作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="bool"/></returns>
    public bool StartTrigger(string triggerId, bool immediately = true)
    {
        var triggerBuilder = GetTriggerBuilder(triggerId);
        triggerBuilder?.StartNow = true;
        triggerBuilder?.SetStatus(TriggerStatus.Ready);

        var succeed = TrySaveTrigger(triggerBuilder?.Updated(), out _, immediately) == ScheduleResult.Succeed;
        if (succeed)
        {
            // 输出日志
            Logger.LogInformation("The <{triggerId}> trigger for scheduler of <{JobId}> successfully started to the schedule.", triggerId, JobId);
        }

        return succeed;
    }

    /// <summary>
    /// 暂停作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="bool"/></returns>
    public bool PauseTrigger(string triggerId, bool immediately = true)
    {
        var triggerBuilder = GetTriggerBuilder(triggerId);
        triggerBuilder?.SetStatus(TriggerStatus.Pause);

        var succeed = TrySaveTrigger(triggerBuilder?.Updated(), out var trigger, immediately) == ScheduleResult.Succeed;
        if (succeed)
        {
            // 在另一个线程上启动异步操作，不阻塞当前线程
            Task.Run(async () =>
            {
                // 记录作业触发器运行信息
                await trigger.RecordTimelineAsync(Factory, JobId);
            });

            // 输出日志
            Logger.LogInformation("The <{triggerId}> trigger for scheduler of <{JobId}> successfully paused to the schedule.", triggerId, JobId);
        }

        return succeed;
    }

    /// <summary>
    /// 强制触发作业持久化记录
    /// </summary>
    public void Persist()
    {
        // 将作业信息运行数据写入持久化
        Factory.Shorthand(JobDetail);

        // 逐条将作业触发器运行数据写入持久化
        foreach (var trigger in Triggers.Values)
        {
            Factory.Shorthand(JobDetail, trigger);
        }

        // 输出日志
        Logger.LogInformation("The scheduler of <{JobId}> successfully persisted to the schedule.", JobId);
    }

    /// <summary>
    /// 启动作业
    /// </summary>
    public void Start()
    {
        // 获取当前时间用来计算触发器下一次触发时间
        var nowTime = Penetrates.GetNowTime();

        // 逐条启用所有作业触发器
        foreach (var (_, trigger) in Triggers)
        {
            // 启动内存作业触发器后更新
            trigger.StartNow = true;
            trigger.SetStatus(TriggerStatus.Ready);
            trigger.NextRunTime = trigger.GetNextRunTime(nowTime);
        }

        // 更新作业
        if (Factory.TryUpdateJob(GetBuilder().Updated(), out _, false) != ScheduleResult.Succeed)
        {
            // 输出日志
            Logger.LogWarning("The scheduler of <{JobId}> started failed.", JobId);

            return;
        }

        // 刷新作业
        Reload();

        // 输出日志
        Logger.LogInformation("The scheduler of <{JobId}> successfully started to the schedule.", JobId);

        // 取消作业调度器休眠状态（强制唤醒）
        Factory.CancelSleep();
    }

    /// <summary>
    /// 暂停作业
    /// </summary>
    public void Pause()
    {
        // 逐条暂停所有作业触发器
        foreach (var (_, trigger) in Triggers)
        {
            // 暂停内存作业触发器后更新
            trigger.SetStatus(TriggerStatus.Pause);
        }

        // 更新作业
        if (Factory.TryUpdateJob(GetBuilder().Updated(), out _, false) != ScheduleResult.Succeed)
        {
            // 输出日志
            Logger.LogWarning("The scheduler of <{JobId}> paused failed.", JobId);

            return;
        }

        // 刷新作业
        Reload();

        // 输出日志
        Logger.LogInformation("The scheduler of <{JobId}> successfully paused to the schedule.", JobId);

        // 取消作业调度器休眠状态（强制唤醒）
        Factory.CancelSleep();
    }

    /// <summary>
    /// 校对作业
    /// </summary>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    public void Collate(bool immediately = true)
    {
        if (Factory.TryUpdateJob(GetBuilder().Updated(), out _, immediately) == ScheduleResult.Succeed)
        {
            // 刷新作业
            Reload();

            // 输出日志
            Logger.LogInformation("The scheduler of <{JobId}> successfully collated to the schedule.", JobId);
        }
    }

    /// <summary>
    /// 刷新作业计划
    /// </summary>
    public void Reload()
    {
        if (Factory.TryGetJob(JobId, out var scheduler) != ScheduleResult.Succeed)
        {
            // 输出日志
            Logger.LogWarning("The scheduler of <{JobId}> is not found.", JobId);

            return;
        }

        _ = scheduler.MapTo<Scheduler>(this);
    }

    /// <summary>
    /// 转换成 JSON 字符串
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertToJSON(NamingConventions naming = NamingConventions.CamelCase)
    {
        return GetBuilder().ConvertToJSON(naming);
    }

    /// <summary>
    /// 将作业计划转换成可枚举集合
    /// </summary>
    /// <returns><see cref="Dictionary{JobDetail, Trigger}"/></returns>
    public Dictionary<JobDetail, Trigger> GetEnumerable()
    {
        var enumerable = new Dictionary<JobDetail, Trigger>(new RepeatKeyEqualityComparer());
        foreach (var (_, trigger) in Triggers)
        {
            enumerable.Add(JobDetail, trigger);
        }

        return enumerable;
    }

    /// <summary>
    /// 手动执行作业
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="customData">自定义数据</param>
    public void Run(string triggerId = null, IDictionary<string, object>? customData = null)
    {
        Factory?.TryRunJob(JobId, out _, triggerId, customData);
    }

    /// <summary>
    /// 取消正在执行作业
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    public void Cancel(string triggerId = null)
    {
        Factory?.TryCancelJob(JobId, out _, triggerId);
    }

    /// <summary>
    /// 内部获取作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="trigger">作业触发器</param>
    /// <param name="outputLog">是否显示日志</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    private ScheduleResult InternalTryGetTrigger(string triggerId, out Trigger trigger, bool outputLog = false)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(triggerId))
        {
            // 输出日志
            if (outputLog) Logger.LogWarning("Empty identity trigger.");

            trigger = default;
            return ScheduleResult.NotIdentify;
        }

        // 查找内存最新的作业信息
        var scheduleResult = Factory.TryGetJob(JobId, out var scheduler);
        if (scheduleResult != ScheduleResult.Succeed)
        {
            // 输出日志
            if (outputLog) Logger.LogWarning("The scheduler of <{JobId}> is not found.", JobId);

            trigger = default;
            return ScheduleResult.NotFound;
        }

        // 查找作业
        var succeed = (scheduler as Scheduler).Triggers.TryGetValue(triggerId, out var originTrigger);

        if (!succeed)
        {
            // 输出日志
            if (outputLog) Logger.LogWarning("The <{triggerId}> trigger for scheduler of <{JobId}> is not found.", triggerId, JobId);

            trigger = default;
            return ScheduleResult.NotFound;
        }

        trigger = originTrigger;
        return ScheduleResult.Succeed;
    }
}