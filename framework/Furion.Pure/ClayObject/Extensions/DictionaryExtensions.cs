// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Furion.ClayObject.Extensions
{
    /// <summary>
    /// 字典类型拓展类
    /// </summary>
    [SuppressSniffer]
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 将对象转成字典
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this object input)
        {
            if (input == null) return default;

            if (input is IDictionary<string, object> dictionary)
                return dictionary;

            if (input is Clay clay && clay.IsObject)
            {
                var dic = new Dictionary<string, object>();
                foreach (KeyValuePair<string, dynamic> item in (dynamic)clay)
                {
                    dic.Add(item.Key, item.Value is Clay v ? v.ToDictionary() : item.Value);
                }
                return dic;
            }

            if (input is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                return jsonElement.ToObject() as IDictionary<string, object>;
            }

            var properties = input.GetType().GetProperties();
            var fields = input.GetType().GetFields();
            var members = properties.Cast<MemberInfo>().Concat(fields.Cast<MemberInfo>());

            return members.ToDictionary(m => m.Name, m => GetValue(input, m));
        }

        /// <summary>
        /// 将对象转字典类型，其中值返回原始类型 Type 类型
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IDictionary<string, Tuple<Type, object>> ToDictionaryWithType(this object input)
        {
            if (input == null) return default;

            if (input is IDictionary<string, object> dictionary)
                return dictionary.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value == null ?
                        new Tuple<Type, object>(typeof(object), kvp.Value) :
                        new Tuple<Type, object>(kvp.Value.GetType(), kvp.Value)
                );

            var dict = new Dictionary<string, Tuple<Type, object>>();

            // 获取所有属性列表
            foreach (var property in input.GetType().GetProperties())
            {
                dict.Add(property.Name, new Tuple<Type, object>(property.PropertyType, property.GetValue(input, null)));
            }

            // 获取所有成员列表
            foreach (var field in input.GetType().GetFields())
            {
                dict.Add(field.Name, new Tuple<Type, object>(field.FieldType, field.GetValue(input)));
            }

            return dict;
        }

        /// <summary>
        /// 获取成员值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        private static object GetValue(object obj, MemberInfo member)
        {
            if (member is PropertyInfo info)
                return info.GetValue(obj, null);

            if (member is FieldInfo info1)
                return info1.GetValue(obj);

            throw new ArgumentException("Passed member is neither a PropertyInfo nor a FieldInfo.");
        }
    }
}