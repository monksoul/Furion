using Mapster;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 对象映射拓展类
    /// </summary>
    public static class ObjectMapperServiceCollectionExtensions
    {
        /// <summary>
        /// 添加对象映射
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddObjectMapper(this IServiceCollection services, params Assembly[] assemblies)
        {
            // 扫描所有继承  IRegister 接口的对象映射配置
            if (assemblies != null && assemblies.Length > 0) TypeAdapterConfig.GlobalSettings.Scan(assemblies);

            // 配置默认全局映射（支持覆盖）
            TypeAdapterConfig.GlobalSettings.Default
                .NameMatchingStrategy(NameMatchingStrategy.Flexible)
                .PreserveReference(true);

            return services;
        }
    }
}