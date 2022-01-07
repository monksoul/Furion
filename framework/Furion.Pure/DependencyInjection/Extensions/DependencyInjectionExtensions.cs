// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 依赖注入拓展类
    /// </summary>
    internal static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 注册服务（如果服务存在，覆盖注册）
        /// </summary>
        /// <param name="dependencyType"></param>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        internal static IServiceCollection InnerAdd(this IServiceCollection collection, Type dependencyType, Type service)
        {
            Call(dependencyType, MethodBase.GetCurrentMethod()
                , new object[] { collection, service });

            return collection;
        }

        /// <summary>
        /// 注册服务（如果服务存在，覆盖注册）
        /// </summary>
        /// <param name="dependencyType"></param>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationFactory"></param>
        internal static IServiceCollection InnerAdd(this IServiceCollection collection, Type dependencyType, Type service, Func<IServiceProvider, object> implementationFactory)
        {
            Call(dependencyType, MethodBase.GetCurrentMethod()
                , new object[] { collection, service, implementationFactory });

            return collection;
        }

        /// <summary>
        /// 注册服务（如果服务存在，覆盖注册）
        /// </summary>
        /// <param name="dependencyType"></param>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationType"></param>
        internal static IServiceCollection InnerAdd(this IServiceCollection collection, Type dependencyType, Type service, Type implementationType)
        {
            Call(dependencyType, MethodBase.GetCurrentMethod()
                , new object[] { collection, service, implementationType });

            return collection;
        }

        /// <summary>
        /// 注册服务（如果服务存在，覆盖注册）
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="dependencyType"></param>
        /// <param name="collection"></param>
        /// <param name="implementationFactory"></param>
        internal static IServiceCollection InnerAdd<TService>(this IServiceCollection collection, Type dependencyType, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            Call(dependencyType, MethodBase.GetCurrentMethod()
                , new object[] { collection, implementationFactory }
                , new[] { typeof(TService) });

            return collection;
        }

        /// <summary>
        /// 注册服务（如果服务存在，跳过注册）
        /// </summary>
        /// <param name="dependencyType"></param>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        internal static void InnerTryAdd(this IServiceCollection collection, Type dependencyType, Type service)
        {
            Call(dependencyType, MethodBase.GetCurrentMethod()
                , new object[] { collection, service });
        }

        /// <summary>
        /// 注册服务（如果服务存在，跳过注册）
        /// </summary>
        /// <param name="dependencyType"></param>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationType"></param>
        internal static void InnerTryAdd(this IServiceCollection collection, Type dependencyType, Type service, Type implementationType)
        {
            Call(dependencyType, MethodBase.GetCurrentMethod()
                , new object[] { collection, service, implementationType });
        }

        /// <summary>
        /// 反射调用
        /// </summary>
        /// <param name="dependencyType">dependencyType</param>
        /// <param name="currentMethod"></param>
        /// <param name="args"></param>
        /// <param name="genericArguments"></param>
        private static void Call(Type dependencyType, MethodBase currentMethod, object[] args, Type[] genericArguments = default)
        {
            var tryWay = currentMethod.Name.StartsWith("InnerTry");
            var methodName = $"{currentMethod.Name[5..]}{dependencyType.Name[1..]}";

            // 获取方法签名（很笨的方式）
            var methodSignature = currentMethod.ToString().Replace($"IServiceCollection Inner{(tryWay ? "Try" : string.Empty)}Add", $"IServiceCollection {methodName}")
                                                                .Replace("Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Type", "Microsoft.Extensions.DependencyInjection.IServiceCollection");

            // 调用静态方法
            Invoke(tryWay ? typeof(ServiceCollectionDescriptorExtensions) : typeof(ServiceCollectionServiceExtensions)
                , methodSignature
                , genericArguments
                , args);
        }

        /// <summary>
        /// 反射调用微软内部注册服务方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodSignature"></param>
        /// <param name="genericParameters"></param>
        /// <param name="args"></param>
        private static void Invoke(Type type, string methodSignature, Type[] genericParameters, object[] args = null)
        {
            var isGeneric = genericParameters != null && genericParameters.Length > 0;

            // 查找符合方法签名的方法
            var method = type.GetMethods()
                                     .Where(m => m.ToString().Equals(methodSignature))
                                     .FirstOrDefault() ?? throw new InvalidOperationException($"Not found method: {methodSignature}.");

            // 处理泛型
            var realMethod = method?.IsGenericMethod == true ? method.MakeGenericMethod(genericParameters) : method;
            realMethod?.Invoke(null, args ?? Array.Empty<object>());
        }
    }
}