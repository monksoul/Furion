using Fur.Utilities;

namespace Fur.Extensions.DependencyInjection
{
    /// <summary>
    /// 依赖注入拓展
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TService GetService<TService>(this object obj)
            where TService : class
        {
            return HttpContextUtility.GetCurrentHttpContext()?.RequestServices?.GetService<TService>();
        }

        /// <summary>
        /// 解析服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService GetRequiredService<TService>(this object obj)
            where TService : class
        {
            return HttpContextUtility.GetCurrentHttpContext()?.RequestServices?.GetRequiredService<TService>();
        }
    }
}