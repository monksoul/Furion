using Fur;
using Fur.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 对象映射拓展类
    /// </summary>
    [SkipScan]
    public static class ObjectMapperServiceCollectionExtensions
    {
        /// <summary>
        /// 添加对象映射
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns></returns>
        public static IServiceCollection AddObjectMapper(this IServiceCollection services)
        {
            // 判断是否安装了 Mapster 程序集
            var objectMapperAssembly = App.Assemblies.FirstOrDefault(u => u.GetName().Name.Equals(AppExtra.OBJECTMAPPER_MAPSTER));
            if (objectMapperAssembly != null)
            {
                // 加载 ObjectMapper 拓展类型和拓展方法
                var objectMapperServiceCollectionExtensionsType = objectMapperAssembly.GetType($"Microsoft.Extensions.DependencyInjection.ObjectMapperServiceCollectionExtensions");
                var addObjectMapperMethod = objectMapperServiceCollectionExtensionsType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(u => u.Name == "AddObjectMapper");

                return addObjectMapperMethod.Invoke(null, new object[] { services, App.Assemblies.ToArray() }) as IServiceCollection;
            }

            return services;
        }
    }
}