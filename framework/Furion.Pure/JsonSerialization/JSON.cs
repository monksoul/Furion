// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.JsonSerialization
{
    /// <summary>
    /// JSON 静态帮助类
    /// </summary>
    [SuppressSniffer]
    public static class JSON
    {
        /// <summary>
        /// 获取 JSON 序列化提供器
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IJsonSerializerProvider GetJsonSerializer(IServiceProvider serviceProvider = default)
        {
            return App.GetService<IJsonSerializerProvider>(serviceProvider ?? App.RootServices);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="value"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static string Serialize(object value, object jsonSerializerOptions = default, IServiceProvider serviceProvider = default)
        {
            return GetJsonSerializer(serviceProvider).Serialize(value, jsonSerializerOptions);
        }

        /// <summary>
        /// 反序列化字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json, object jsonSerializerOptions = default, IServiceProvider serviceProvider = default)
        {
            return GetJsonSerializer(serviceProvider).Deserialize<T>(json, jsonSerializerOptions);
        }

        /// <summary>
        /// 获取 JSON 配置选项
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static TOptions GetSerializerOptions<TOptions>(IServiceProvider serviceProvider = default)
            where TOptions : class
        {
            return GetJsonSerializer(serviceProvider).GetSerializerOptions() as TOptions;
        }
    }
}