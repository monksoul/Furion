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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace Furion.DatabaseAccessor;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 数据库上下文描述器
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, Type> DbContextDescriptors;

    /// <summary>
    /// 构造函数
    /// </summary>
    static Penetrates()
    {
        DbContextDescriptors = new ConcurrentDictionary<Type, Type>();
    }

    /// <summary>
    /// 配置 SqlServer 数据库上下文
    /// </summary>
    /// <param name="optionBuilder">数据库上下文选项构建器</param>
    /// <param name="interceptors">拦截器</param>
    /// <returns></returns>
    internal static Action<IServiceProvider, DbContextOptionsBuilder> ConfigureDbContext(Action<IServiceProvider, DbContextOptionsBuilder> optionBuilder, params IInterceptor[] interceptors)
    {
        return (serviceProvider, options) =>
        {
            // 只有开发环境开启
            if (App.HostEnvironment?.IsDevelopment() ?? false)
            {
                options/*.UseLazyLoadingProxies()*/
                         .EnableDetailedErrors()
                         .EnableSensitiveDataLogging();
            }

            optionBuilder?.Invoke(serviceProvider, options);

            // 添加拦截器
            AddInterceptors(interceptors, options);
        };
    }

    /// <summary>
    /// 检查数据库上下文是否绑定
    /// </summary>
    /// <param name="dbContextLocatorType"></param>
    /// <param name="dbContextType"></param>
    /// <returns></returns>
    internal static void CheckDbContextLocator(Type dbContextLocatorType, out Type dbContextType)
    {
        if (!DbContextDescriptors.TryGetValue(dbContextLocatorType, out dbContextType)) throw new InvalidCastException($" The dbcontext locator `{dbContextLocatorType.Name}` is not bind.");
    }

    /// <summary>
    /// 数据库数据库拦截器
    /// </summary>
    /// <param name="interceptors">拦截器</param>
    /// <param name="options"></param>
    private static void AddInterceptors(IInterceptor[] interceptors, DbContextOptionsBuilder options)
    {
        // 添加拦截器
        var interceptorList = DbProvider.GetDefaultInterceptors();

        if (interceptors != null && interceptors.Length > 0)
        {
            interceptorList.AddRange(interceptors);
        }
        options.AddInterceptors(interceptorList.ToArray());
    }
}