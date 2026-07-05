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

using Furion.TimeCrontab;

namespace Furion.Schedule;

/// <summary>
/// 作业触发器
/// </summary>
public sealed partial class TriggerBuilder
{
    /// <summary>
    /// 设置作业触发器
    /// </summary>
    /// <typeparam name="TTrigger"><see cref="Trigger"/> 派生类</typeparam>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterTo<TTrigger>()
        where TTrigger : Trigger
    {
        return AlterTo<TTrigger>();
    }

    /// <summary>
    /// 设置作业触发器
    /// </summary>
    /// <typeparam name="TTrigger"><see cref="Trigger"/> 派生类</typeparam>
    /// <param name="args">作业触发器参数</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterTo<TTrigger>(params object[] args)
        where TTrigger : Trigger
    {
        return SetTriggerType<TTrigger>().SetArgs(args);
    }

    /// <summary>
    /// 设置作业触发器
    /// </summary>
    /// <param name="assemblyName">作业触发器类型所在程序集 Name</param>
    /// <param name="triggerTypeFullName">作业触发器类型 FullName</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterTo(string assemblyName, string triggerTypeFullName)
    {
        return AlterTo(assemblyName, triggerTypeFullName);
    }

    /// <summary>
    /// 设置作业触发器
    /// </summary>
    /// <param name="assemblyName">作业触发器类型所在程序集 Name</param>
    /// <param name="triggerTypeFullName">作业触发器类型 FullName</param>
    /// <param name="args">作业触发器参数</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterTo(string assemblyName, string triggerTypeFullName, params object[] args)
    {
        return SetTriggerType(assemblyName, triggerTypeFullName).SetArgs(args);
    }

    /// <summary>
    /// 设置作业触发器
    /// </summary>
    /// <param name="triggerType"><see cref="Trigger"/> 派生类</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterTo(Type triggerType)
    {
        return AlterTo(triggerType);
    }

    /// <summary>
    /// 设置作业触发器
    /// </summary>
    /// <param name="triggerType"><see cref="Trigger"/> 派生类</param>
    /// <param name="args">作业触发器参数</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterTo(Type triggerType, params object[] args)
    {
        return SetTriggerType(triggerType).SetArgs(args);
    }

    /// <summary>
    /// 设置毫秒周期（间隔）作业触发器
    /// </summary>
    /// <param name="interval">间隔（毫秒）</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToPeriod(long interval)
    {
        return SetTriggerType<PeriodTrigger>().SetArgs(interval);
    }

    /// <summary>
    /// 设置秒周期（间隔）作业触发器
    /// </summary>
    /// <param name="interval">间隔（秒）</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToPeriodSeconds(long interval)
    {
        return AlterToPeriod(interval * 1000);
    }

    /// <summary>
    /// 设置分钟周期（间隔）作业触发器
    /// </summary>
    /// <param name="interval">间隔（分钟）</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToPeriodMinutes(long interval)
    {
        return AlterToPeriod(interval * 1000 * 60);
    }

    /// <summary>
    /// 设置小时周期（间隔）作业触发器
    /// </summary>
    /// <param name="interval">间隔（小时）</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToPeriodHours(long interval)
    {
        return AlterToPeriod(interval * 1000 * 60 * 60);
    }

    /// <summary>
    /// 设置 Cron 表达式作业触发器
    /// </summary>
    /// <param name="schedule">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型，默认 <see cref="CronStringFormat.Default"/></param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToCron(string schedule, CronStringFormat format = CronStringFormat.Default)
    {
        return SetTriggerType<CronTrigger>().SetArgs(schedule, format);
    }

    /// <summary>
    /// 设置 Cron 表达式作业触发器
    /// </summary>
    /// <param name="schedule">Cron 表达式</param>
    /// <param name="args">动态参数类型，支持 <see cref="int"/> 和 object[]</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToCron(string schedule, object args)
    {
        return SetTriggerType<CronTrigger>().SetArgs(schedule, args);
    }

    /// <summary>
    /// 设置每秒开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToSecondly()
    {
        return AlterToCron("@secondly");
    }

    /// <summary>
    /// 设置指定特定秒开始作业触发器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToSecondlyAt(params object[] fields)
    {
        return AlterToCron("@secondly", fields);
    }

    /// <summary>
    /// 设置每分钟开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToMinutely()
    {
        return AlterToCron("@minutely");
    }

    /// <summary>
    /// 设置每分钟特定秒开始作业触发器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToMinutelyAt(params object[] fields)
    {
        return AlterToCron("@minutely", fields);
    }

    /// <summary>
    /// 设置每小时开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToHourly()
    {
        return AlterToCron("@hourly");
    }

    /// <summary>
    /// 设置每小时特定分钟开始作业触发器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToHourlyAt(params object[] fields)
    {
        return AlterToCron("@hourly", fields);
    }

    /// <summary>
    /// 设置每天（午夜）开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToDaily()
    {
        return AlterToCron("@daily");
    }

    /// <summary>
    /// 设置每天特定小时开始作业触发器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToDailyAt(params object[] fields)
    {
        return AlterToCron("@daily", fields);
    }

    /// <summary>
    /// 设置每月1号（午夜）开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToMonthly()
    {
        return AlterToCron("@monthly");
    }

    /// <summary>
    /// 设置每月特定天（午夜）开始作业触发器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToMonthlyAt(params object[] fields)
    {
        return AlterToCron("@monthly", fields);
    }

    /// <summary>
    /// 设置每周日（午夜）开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToWeekly()
    {
        return AlterToCron("@weekly");
    }

    /// <summary>
    /// 设置每周特定星期几（午夜）开始作业触发器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToWeeklyAt(params object[] fields)
    {
        return AlterToCron("@weekly", fields);
    }

    /// <summary>
    /// 设置每年1月1号（午夜）开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToYearly()
    {
        return AlterToCron("@yearly");
    }

    /// <summary>
    /// 设置每年特定月1号（午夜）开始作业触发器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToYearlyAt(params object[] fields)
    {
        return AlterToCron("@yearly", fields);
    }

    /// <summary>
    /// 设置每周一至周五（午夜）开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToWeekdays()
    {
        return AlterToCron("@weekdays");
    }

    /// <summary>
    /// 设置每周六和周日（午夜）开始作业触发器
    /// </summary>
    /// <returns><see cref="TriggerBuilder"/></returns>
    public TriggerBuilder AlterToWeekends()
    {
        return AlterToCron("@weekends");
    }
}