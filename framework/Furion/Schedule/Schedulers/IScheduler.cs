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
/// 作业计划接口
/// </summary>
public interface IScheduler
{
    /// <summary>
    /// 作业 Id
    /// </summary>
    string JobId { get; }

    /// <summary>
    /// 作业组名称
    /// </summary>
    string GroupName { get; }

    /// <summary>
    /// 作业触发器数量
    /// </summary>
    int TriggerCount { get; }

    /// <summary>
    /// 返回可公开访问的作业计划模型
    /// </summary>
    /// <remarks>常用于接口返回或序列化操作</remarks>
    /// <returns><see cref="SchedulerModel"/></returns>
    SchedulerModel GetModel();

    /// <summary>
    /// 获取作业计划构建器
    /// </summary>
    /// <returns><see cref="SchedulerBuilder"/></returns>
    SchedulerBuilder GetBuilder();

    /// <summary>
    /// 获取作业信息构建器
    /// </summary>
    /// <returns><see cref="JobBuilder"/></returns>
    JobBuilder GetJobBuilder();

    /// <summary>
    /// 获取作业触发器构建器集合
    /// </summary>
    /// <returns><see cref="List{TriggerBuilder}"/></returns>
    IReadOnlyList<TriggerBuilder> GetTriggerBuilders();

    /// <summary>
    /// 获取作业触发器构建器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    TriggerBuilder GetTriggerBuilder(string triggerId);

    /// <summary>
    /// 查找作业信息
    /// </summary>
    /// <returns><see cref="JobDetail"/></returns>
    JobDetail GetJobDetail();

    /// <summary>
    /// 查找作业触发器集合
    /// </summary>
    /// <returns><see cref="IEnumerable{Trigger}"/></returns>
    IEnumerable<Trigger> GetTriggers();

    /// <summary>
    /// 查找作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryGetTrigger(string triggerId, out Trigger trigger);

    /// <summary>
    /// 查找作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="Trigger"/></returns>
    Trigger GetTrigger(string triggerId);

    /// <summary>
    /// 保存作业触发器
    /// </summary>
    /// <param name="triggerBuilder">作业触发器构建器</param>
    /// <param name="trigger">作业触发器</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TrySaveTrigger(TriggerBuilder triggerBuilder, out Trigger trigger, bool immediately = true);

    /// <summary>
    /// 保存作业触发器
    /// </summary>
    /// <param name="triggerBuilders">作业触发器构建器集合</param>
    void SaveTrigger(params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 更新作业计划信息
    /// </summary>
    /// <param name="jobBuilder">作业信息构建器</param>
    /// <param name="jobDetail">作业信息</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryUpdateDetail(JobBuilder jobBuilder, out JobDetail jobDetail);

    /// <summary>
    /// 更新作业信息
    /// </summary>
    /// <param name="jobBuilder">作业信息构建器</param>
    void UpdateDetail(JobBuilder jobBuilder);

    /// <summary>
    /// 更新作业计划信息
    /// </summary>
    /// <param name="jobBuilderAction">作业信息构建器委托</param>
    /// <param name="jobDetail">作业信息</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryUpdateDetail(Action<JobBuilder> jobBuilderAction, out JobDetail jobDetail);

    /// <summary>
    /// 更新作业信息
    /// </summary>
    /// <param name="jobBuilderAction">作业信息构建器委托</param>
    void UpdateDetail(Action<JobBuilder> jobBuilderAction);

    /// <summary>
    /// 添加作业触发器
    /// </summary>
    /// <param name="triggerBuilder">作业触发器构建器</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryAddTrigger(TriggerBuilder triggerBuilder, out Trigger trigger);

    /// <summary>
    /// 添加作业触发器
    /// </summary>
    /// <param name="triggerBuilders">作业触发器构建器</param>
    void AddTrigger(params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 更新作业触发器
    /// </summary>
    /// <param name="triggerBuilder">作业触发器构建器</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryUpdateTrigger(TriggerBuilder triggerBuilder, out Trigger trigger);

    /// <summary>
    /// 更新作业触发器
    /// </summary>
    /// <param name="triggerBuilders">作业触发器构建器</param>
    void UpdateTrigger(params TriggerBuilder[] triggerBuilders);

    /// <summary>
    /// 更新作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="triggerBuilderAction">作业触发器构建器委托</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryUpdateTrigger(string triggerId, Action<TriggerBuilder> triggerBuilderAction, out Trigger trigger);

    /// <summary>
    /// 更新作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="triggerBuilderAction">作业触发器构建器委托</param>
    void UpdateTrigger(string triggerId, Action<TriggerBuilder> triggerBuilderAction);

    /// <summary>
    /// 删除作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="trigger">作业触发器</param>
    /// <returns><see cref="ScheduleResult"/></returns>
    ScheduleResult TryRemoveTrigger(string triggerId, out Trigger trigger);

    /// <summary>
    /// 删除作业触发器
    /// </summary>
    /// <param name="triggerIds">作业触发器 Id 集合</param>
    void RemoveTrigger(params string[] triggerIds);

    /// <summary>
    /// 将当前作业计划从调度器中删除
    /// </summary>
    /// <remarks><see cref="ScheduleResult"/></remarks>
    ScheduleResult TryRemove();

    /// <summary>
    /// 将当前作业计划从调度器中删除
    /// </summary>
    void Remove();

    /// <summary>
    /// 检查作业触发器是否存在
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <returns><see cref="bool"/></returns>
    bool ContainsTrigger(string triggerId);

    /// <summary>
    /// 启动作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="bool"/></returns>
    bool StartTrigger(string triggerId, bool immediately = true);

    /// <summary>
    /// 暂停作业触发器
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    /// <returns><see cref="bool"/></returns>
    bool PauseTrigger(string triggerId, bool immediately = true);

    /// <summary>
    /// 强制触发作业持久化记录
    /// </summary>
    void Persist();

    /// <summary>
    /// 启动作业
    /// </summary>
    void Start();

    /// <summary>
    /// 暂停作业
    /// </summary>
    void Pause();

    /// <summary>
    /// 校对作业
    /// </summary>
    /// <param name="immediately">是否立即通知作业调度器重新载入</param>
    void Collate(bool immediately = true);

    /// <summary>
    /// 刷新作业计划
    /// </summary>
    void Reload();

    /// <summary>
    /// 转换成 JSON 字符串
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    string ConvertToJSON(NamingConventions naming = NamingConventions.CamelCase);

    /// <summary>
    /// 将作业计划转换成可枚举集合
    /// </summary>
    /// <returns><see cref="Dictionary{JobDetail, Trigger}"/></returns>
    Dictionary<JobDetail, Trigger> GetEnumerable();

    /// <summary>
    /// 手动执行作业
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    /// <param name="customData">自定义数据</param>
    void Run(string triggerId = null, IDictionary<string, object>? customData = null);

    /// <summary>
    /// 取消正在执行的作业
    /// </summary>
    /// <param name="triggerId">作业触发器 Id</param>
    void Cancel(string triggerId = null);
}