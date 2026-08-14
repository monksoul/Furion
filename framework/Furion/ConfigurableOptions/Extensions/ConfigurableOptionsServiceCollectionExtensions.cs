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

using Furion;
using Furion.ConfigurableOptions;
using Furion.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 可变选项服务扩展类
/// </summary>
public static class ConfigurableOptionsServiceCollectionExtensions
{
    /// <summary>
    /// 添加选项配置
    /// </summary>
    /// <typeparam name="TOptions">选项类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddConfigurableOptions<TOptions>(this IServiceCollection services)
        where TOptions : class, IConfigurableOptions
    {
        var optionsType = typeof(TOptions);

        // 获取选项配置
        var (optionsSettings, path) = Penetrates.GetOptionsConfiguration(optionsType);

        // 配置选项（含验证信息）
        var configurationRoot = App.Configuration;
        var optionsConfiguration = configurationRoot.GetSection(path);

        // 配置选项监听
        if (typeof(IConfigurableOptionsListener<TOptions>).IsAssignableFrom(optionsType))
        {
            var onListenerMethod = optionsType.GetMethod(nameof(IConfigurableOptionsListener<TOptions>.OnListener));
            if (onListenerMethod != null)
            {
                // 将 MethodInfo 转换为委托
                var onListenerAction = (Action<TOptions, TOptions, IConfiguration>)Delegate.CreateDelegate(typeof(Action<TOptions, TOptions, IConfiguration>), onListenerMethod);

                // 监听全局配置改变，目前该方式存在触发两次的 bug：https://github.com/dotnet/aspnetcore/issues/2542
                ChangeToken.OnChange(() => configurationRoot.GetReloadToken(), ((Action)(() =>
                {
                    var options = optionsConfiguration.Get<TOptions>();
                    if (options != null) onListenerAction(options, options, optionsConfiguration);
                })).Debounce());
            }
        }

        var optionsConfigure = services.AddOptions<TOptions>()
              .Bind(optionsConfiguration, options =>
              {
                  options.BindNonPublicProperties = true; // 绑定私有变量
              })
              .ValidateDataAnnotations()
              .ValidateOnStart();

        // 查找所有贴了 MapSettings 的属性和其路径
        var remapProperties = optionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                          .Where(u => u.IsDefined(typeof(MapSettingsAttribute), true))
                                          .Select(p => (Property: p, p.GetCustomAttribute<MapSettingsAttribute>(true).Path))
                                          .ToArray();

        // 实现 Key 映射
        if (remapProperties.Length > 0)
        {
            services.PostConfigureAll<TOptions>(options =>
            {
                foreach (var item in remapProperties)
                {
                    var realValue = configurationRoot.GetValue(item.Property.PropertyType, $"{path}:{item.Path}");
                    item.Property.SetValue(options, realValue);
                }
            });
        }

        // 配置复杂验证后后期配置
        var validateInterface = optionsType.GetInterfaces()
            .FirstOrDefault(u => u.IsGenericType && typeof(IConfigurableOptions).IsAssignableFrom(u.GetGenericTypeDefinition()));
        if (validateInterface != null)
        {
            var genericArguments = validateInterface.GenericTypeArguments;

            // 配置复杂验证
            if (genericArguments.Length > 1)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IValidateOptions<TOptions>), genericArguments[^1]));
            }

            // 配置后期配置
            var postConfigureMethod = optionsType.GetMethod(nameof(IConfigurableOptions<TOptions>.PostConfigure));
            if (postConfigureMethod != null)
            {
                var postConfigureAction = (Action<TOptions, TOptions, IConfiguration>)Delegate.CreateDelegate(typeof(Action<TOptions, TOptions, IConfiguration>), postConfigureMethod);

                if (optionsSettings?.PostConfigureAll != true)
                    optionsConfigure.PostConfigure(options => postConfigureAction(options, options, optionsConfiguration));
                else
                    services.PostConfigureAll<TOptions>(options => postConfigureAction(options, options, optionsConfiguration));
            }
        }

        return services;
    }
}