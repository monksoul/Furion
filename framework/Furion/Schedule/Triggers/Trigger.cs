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

using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Furion.Schedule;

/// <summary>
/// 作业触发器基类
/// </summary>
public partial class Trigger
{
    /// <summary>
    /// 作业触发器 Id
    /// </summary>
    [JsonInclude]
    public string TriggerId { get; internal set; }

    /// <summary>
    /// 作业 Id
    /// </summary>
    [JsonInclude]
    public string JobId { get; internal set; }

    /// <summary>
    /// 作业触发器类型
    /// </summary>
    /// <remarks>存储的是类型的 FullName</remarks>
    [JsonInclude]
    public string TriggerType { get; internal set; }

    /// <summary>
    /// 作业触发器类型所在程序集
    /// </summary>
    /// <remarks>存储的是程序集 Name</remarks>
    [JsonInclude]
    public string AssemblyName { get; internal set; }

    /// <summary>
    /// 作业触发器参数
    /// </summary>
    /// <remarks>运行时将反序列化为 object[] 类型并作为构造函数参数</remarks>
    [JsonInclude]
    public string Args { get; internal set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    [JsonInclude]
    public string Description { get; internal set; }

    /// <summary>
    /// 作业触发器状态
    /// </summary>
    [JsonInclude]
    public TriggerStatus Status { get; internal set; } = TriggerStatus.Ready;

    /// <summary>
    /// 起始时间
    /// </summary>
    [JsonInclude]
    public DateTime? StartTime { get; internal set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [JsonInclude]
    public DateTime? EndTime { get; internal set; }

    /// <summary>
    /// 最近运行时间
    /// </summary>
    [JsonInclude]
    public DateTime? LastRunTime { get; internal set; }

    /// <summary>
    /// 下一次运行时间
    /// </summary>
    [JsonInclude]
    public DateTime? NextRunTime { get; internal set; }

    /// <summary>
    /// 触发次数
    /// </summary>
    [JsonInclude]
    public long NumberOfRuns { get; internal set; }

    /// <summary>
    /// 最大触发次数
    /// </summary>
    /// <remarks>
    /// <para>0：不限制</para>
    /// <para>n：N 次</para>
    /// </remarks>
    [JsonInclude]
    public long MaxNumberOfRuns { get; internal set; }

    /// <summary>
    /// 出错次数
    /// </summary>
    [JsonInclude]
    public long NumberOfErrors { get; internal set; }

    /// <summary>
    /// 最大出错次数
    /// </summary>
    /// <remarks>
    /// <para>0：不限制</para>
    /// <para>n：N 次</para>
    /// </remarks>
    [JsonInclude]
    public long MaxNumberOfErrors { get; internal set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    [JsonInclude]
    public int NumRetries { get; internal set; } = 0;

    /// <summary>
    /// 重试间隔时间
    /// </summary>
    /// <remarks>默认1000毫秒</remarks>
    [JsonInclude]
    public int RetryTimeout { get; internal set; } = 1000;

    /// <summary>
    /// 是否立即启动
    /// </summary>
    [JsonInclude]
    public bool StartNow { get; internal set; } = true;

    /// <summary>
    /// 是否启动时执行一次
    /// </summary>
    [JsonInclude]
    public bool RunOnStart { get; internal set; } = false;

    /// <summary>
    /// 是否在启动时重置最大触发次数等于一次的作业
    /// </summary>
    /// <remarks>解决因持久化数据已完成一次触发但启动时不再执行的问题</remarks>
    [JsonInclude]
    public bool ResetOnlyOnce { get; internal set; } = true;

    /// <summary>
    /// 本次执行结果
    /// </summary>
    [JsonInclude]
    public string Result { get; internal set; }

    /// <summary>
    /// 本次执行耗时
    /// </summary>
    [JsonInclude]
    public long ElapsedTime { get; internal set; }

    /// <summary>
    /// 作业触发器更新时间
    /// </summary>
    [JsonInclude]
    public DateTime? UpdatedTime { get; internal set; }

    /// <summary>
    /// 标记作业触发器持久化行为
    /// </summary>
    internal PersistenceBehavior Behavior { get; set; } = PersistenceBehavior.Appended;

    /// <summary>
    /// 作业触发器运行时类型
    /// </summary>
    internal Type RuntimeTriggerType { get; set; }

    /// <summary>
    /// 作业触发器运行时参数
    /// </summary>
    internal object[] RuntimeTriggerArgs { get; set; }

    /// <summary>
    /// 作业触发器最近运行记录
    /// </summary>
    /// <remarks>默认只保存 10 条</remarks>
    internal ConcurrentQueue<TriggerTimeline> Timelines { get; set; } = new();

    /// <summary>
    /// 触发模式
    /// </summary>
    /// <remarks>默认为定时触发，0:定时，1:手动</remarks>
    internal int Mode { get; set; }

    /// <summary>
    /// 手动执行时传入的临时自定义数据
    /// </summary>
    internal IDictionary<string, object>? ManualRunCustomData { get; set; }
}