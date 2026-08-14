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

using System.Reflection;

namespace Furion.ConfigurableOptions;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 获取选项配置
    /// </summary>
    /// <param name="optionsType">选项类型</param>
    /// <returns></returns>
    internal static (OptionsSettingsAttribute, string) GetOptionsConfiguration(Type optionsType)
    {
        var optionsSettings = optionsType.GetCustomAttribute<OptionsSettingsAttribute>(false);

        // 默认后缀
        var defaultSuffix = nameof(Options);

        return (optionsSettings, optionsSettings switch
        {
            // 没有贴 [OptionsSettings]，如果选项类以 `Options` 结尾，则移除，否则返回类名称
            null => optionsType.Name.EndsWith(defaultSuffix) ? optionsType.Name[0..^defaultSuffix.Length] : optionsType.Name,
            // 如果贴有 [OptionsSettings] 特性，但未指定 Path 参数，则直接返回类名，否则返回 Path
            _ => string.IsNullOrWhiteSpace(optionsSettings.Path) ? optionsType.Name : optionsSettings.Path,
        });
    }

    /// <summary>
    /// 在主机启动时获取选项
    /// </summary>
    /// <remarks>解决 v4.5.2+ 历史版本升级问题</remarks>
    /// <typeparam name="TOptions"></typeparam>
    /// <returns></returns>
    internal static TOptions GetOptionsOnStarting<TOptions>()
        where TOptions : class, new()
    {
        if (App.RootServices == null && typeof(IConfigurableOptions).IsAssignableFrom(typeof(TOptions)))
        {
            var (_, path) = GetOptionsConfiguration(typeof(TOptions));
            return App.GetConfig<TOptions>(path, true);
        }

        return null;
    }
}