// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion;
using Furion.DependencyInjection;
using Furion.Reflection;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 对象映射拓展类
    /// </summary>
    [SuppressSniffer]
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
            var objectMapperAssembly = App.Assemblies.FirstOrDefault(u => u.GetName().Name.Equals("Furion.Extras.ObjectMapper.Mapster"));
            if (objectMapperAssembly != null)
            {
                // 加载 ObjectMapper 拓展类型和拓展方法
                var objectMapperServiceCollectionExtensionsType = Reflect.GetType(objectMapperAssembly, $"Microsoft.Extensions.DependencyInjection.ObjectMapperServiceCollectionExtensions");
                var addObjectMapperMethod = objectMapperServiceCollectionExtensionsType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(u => u.Name == "AddObjectMapper");

                return addObjectMapperMethod.Invoke(null, new object[] { services, App.Assemblies.ToArray() }) as IServiceCollection;
            }

            return services;
        }
    }
}