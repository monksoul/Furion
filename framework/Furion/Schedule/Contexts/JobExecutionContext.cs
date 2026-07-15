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

using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Furion.Schedule;

/// <summary>
/// 作业执行上下文基类
/// </summary>
public abstract class JobExecutionContext : IServiceProvider
{
    /// <summary>
    ///     <see cref="IServiceProvider" /> 委托
    /// </summary>
    internal Func<Type, object?>? _serviceProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="trigger">作业触发器</param>
    /// <param name="occurrenceTime">作业计划触发时间</param>
    /// <param name="runId">作业触发器触发的唯一标识</param>
    /// <param name="serviceProvider">服务提供器</param>
    internal JobExecutionContext(JobDetail jobDetail
        , Trigger trigger
        , DateTime occurrenceTime
        , string runId
        , IServiceProvider? serviceProvider)
    {
        // 空检查
        if (serviceProvider is not null)
        {
            var localServiceProvider = serviceProvider;
            InitializeServiceProvider(localServiceProvider.GetService);
        }

        JobId = jobDetail.JobId;
        TriggerId = trigger.TriggerId;
        JobDetail = jobDetail;
        Trigger = trigger;
        OccurrenceTime = occurrenceTime;
        RunId = runId;
    }

    /// <summary>
    /// 作业 Id
    /// </summary>
    public string JobId { get; }

    /// <summary>
    /// 作业触发器 Id
    /// </summary>
    public string TriggerId { get; }

    /// <summary>
    /// 作业信息
    /// </summary>
    public JobDetail JobDetail { get; }

    /// <summary>
    /// 作业触发器
    /// </summary>
    public Trigger Trigger { get; }

    /// <summary>
    /// 作业计划触发时间
    /// </summary>
    public DateTime OccurrenceTime { get; }

    /// <summary>
    /// 作业触发器触发的唯一标识
    /// </summary>
    public string RunId { get; }

    /// <summary>
    /// 本次执行结果
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    /// 触发模式
    /// </summary>
    /// <remarks>默认为定时触发，0:定时，1:手动</remarks>
    public int Mode { get; internal set; }

    /// <summary>
    /// 存储作业执行过程中需要传递的数据
    /// </summary>
    public IDictionary<string, object> Items { get; internal set; }

    /// <summary>
    /// 作业计划工厂
    /// </summary>
    public ISchedulerFactory Factory => this.GetRequiredService<ISchedulerFactory>();

    /// <summary>
    /// 获取当前作业
    /// </summary>
    public IScheduler GetJob()
    {
        return Factory.GetJob(JobId);
    }

    /// <summary>
    /// 获取作业执行过程中传递的数据
    /// </summary>
    /// <param name="key">键</param>
    /// <returns><see cref="object"/></returns>
    public object GetItem(string key)
    {
        return Items[key];
    }

    /// <summary>
    /// 获取作业执行过程中传递的数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="key">键</param>
    /// <returns><typeparamref name="T"/></returns>
    public T GetItem<T>(string key)
    {
        if (Items.TryGetValue(key, out var value))
        {
            // 处理手动执行传递的自定义数据
            if (value is JsonElement element && typeof(T) != typeof(object))
            {
                return element.Deserialize<T>(Penetrates.GetDefaultJsonSerializerOptions());
            }

            return (T)value;
        }

        return default;
    }

    /// <summary>
    /// 获取作业执行过程中传递的数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns><see cref="IEnumerable{T}"/></returns>
    public IEnumerable<T> GetItems<T>()
    {
        return Items.Values.OfType<T>();
    }

    /// <summary>
    /// 获取作业执行过程中传递的数据
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns><typeparamref name="T"/></returns>
    public T GetItem<T>()
    {
        return GetItems<T>().FirstOrDefault();
    }

    /// <summary>
    /// 检查作业任务是否处于正常状态
    /// </summary>
    /// <param name="schedulerFactory"><see cref="ISchedulerFactory"/></param>
    /// <returns><see cref="bool"/></returns>
    public bool IsNormalStatus(ISchedulerFactory schedulerFactory = null)
    {
        // 解析作业计划工厂服务
        schedulerFactory ??= Factory;

        // 检查作业是否存在
        if (schedulerFactory.TryGetJob(JobId, out var scheduler) != ScheduleResult.Succeed)
        {
            return false;
        }

        // 检查作业触发器是否存在
        if (scheduler.TryGetTrigger(TriggerId, out var trigger) != ScheduleResult.Succeed)
        {
            return false;
        }

        // 检查作业触发器是否正常运行
        return trigger.IsNormalStatus();
    }

    /// <summary>
    /// 转换成 JSON 字符串
    /// </summary>
    /// <param name="naming">命名法</param>
    /// <returns><see cref="string"/></returns>
    public string ConvertToJSON(NamingConventions naming = NamingConventions.CamelCase)
    {
        return Penetrates.Write(writer =>
        {
            writer.WriteStartObject();

            // 输出 JobDetail
            writer.WritePropertyName(Penetrates.GetNaming(nameof(JobDetail), naming));
            writer.WriteRawValue(JobDetail.ConvertToJSON(naming));

            // 输出 Trigger
            writer.WritePropertyName(Penetrates.GetNaming(nameof(Trigger), naming));
            writer.WriteRawValue(Trigger.ConvertToJSON(naming));

            writer.WriteEndObject();
        });
    }

    /// <summary>
    /// 作业执行上下文转字符串输出输出
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        return $"{JobDetail} {Trigger}{(Mode == 1 ? " Manual" : string.Empty)} {OccurrenceTime.ToFormatString()}{(Trigger.NextRunTime == null ? $" [{Trigger.Status}]" : $" -> {Trigger.NextRunTime.ToFormatString()}")}";
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        return _serviceProvider?.Invoke(serviceType);
    }

    /// <summary>
    ///     初始化 <see cref="IServiceProvider" />
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider" /> 委托</param>
    internal void InitializeServiceProvider(Func<Type, object?> serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
}