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

namespace Furion.TimeCrontab;

/// <summary>
/// Cron 表达式抽象类
/// </summary>
/// <remarks>主要将 Cron 表达式转换成 OOP 类进行操作</remarks>
public sealed partial class Crontab
{
    /// <summary>
    /// 表示每秒开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Secondly = Parse("* * * * * *", CronStringFormat.WithSeconds);

    /// <summary>
    /// 表示每分钟开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Minutely = Parse("* * * * *", CronStringFormat.Default);

    /// <summary>
    /// 表示每小时开始 的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Hourly = Parse("0 * * * *", CronStringFormat.Default);

    /// <summary>
    /// 表示每天（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Daily = Parse("0 0 * * *", CronStringFormat.Default);

    /// <summary>
    /// 表示每月1号（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Monthly = Parse("0 0 1 * *", CronStringFormat.Default);

    /// <summary>
    /// 表示每周日（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Weekly = Parse("0 0 * * 0", CronStringFormat.Default);

    /// <summary>
    /// 表示每年1月1号（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Yearly = Parse("0 0 1 1 *", CronStringFormat.Default);

    /// <summary>
    /// 表示每周一至周五（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Weekdays = Parse("0 0 * * 1-5", CronStringFormat.Default);

    /// <summary>
    /// 表示每周六和周日（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Weekends = Parse("0 0 * * 0,6", CronStringFormat.Default);
}