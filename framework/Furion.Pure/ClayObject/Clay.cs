// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ClayObject.Extensions;
using Furion.DependencyInjection;
using Furion.JsonSerialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Furion.ClayObject
{
    /// <summary>
    /// 粘土对象
    /// </summary>
    [SuppressSniffer]
    public class Clay : DynamicObject
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Clay()
        {
            XmlElement = new XElement("root", CreateTypeAttr(JsonType.@object));
            jsonType = JsonType.@object;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="element"></param>
        /// <param name="type"></param>
        private Clay(XElement element, JsonType type)
        {
            Debug.Assert(type == JsonType.array || type == JsonType.@object);

            XmlElement = element;
            jsonType = type;
        }

        /// <summary>
        /// 是否是 Object 类型
        /// </summary>
        public bool IsObject => jsonType == JsonType.@object;

        /// <summary>
        /// 是否是 Array 类型
        /// </summary>
        public bool IsArray => jsonType == JsonType.array;

        /// <summary>
        /// XML 元素
        /// </summary>
        public XElement XmlElement { get; private set; }

        /// <summary>
        /// 创建一个超级类型
        /// </summary>
        /// <returns></returns>
        public static dynamic Object()
        {
            return new Clay();
        }

        /// <summary>
        /// 基于现有类型创建一个超级类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static dynamic Object(object obj)
        {
            return Parse(Serialize(obj));
        }

        /// <summary>
        /// 将 Json 转换成动态类型
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static dynamic Parse(string json)
        {
            return Parse(json, Encoding.Unicode);
        }

        /// <summary>
        /// 将 Json 转换成动态类型
        /// </summary>
        /// <param name="json"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static dynamic Parse(string json, Encoding encoding)
        {
            using var reader = JsonReaderWriterFactory.CreateJsonReader(encoding.GetBytes(json), XmlDictionaryReaderQuotas.Max);
            return ToValue(XElement.Load(reader));
        }

        /// <summary>
        /// 将 Steam 转换成动态类型
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static dynamic Parse(Stream stream)
        {
            using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);
            return ToValue(XElement.Load(reader));
        }

        /// <summary>
        /// 将 Steam 转换成动态类型
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static dynamic Parse(Stream stream, Encoding encoding)
        {
            using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, encoding, XmlDictionaryReaderQuotas.Max, _ => { });
            return ToValue(XElement.Load(reader));
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return CreateJsonString(new XStreamingElement("root", CreateTypeAttr(GetJsonType(obj)), CreateJsonNode(obj)));
        }

        /// <summary>
        /// 是否定义某个键
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsDefined(string name)
        {
            return IsObject && (XmlElement.Element(name) != null);
        }

        /// <summary>
        /// 判断数组索引是否存在
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsDefined(int index)
        {
            return IsArray && (XmlElement.Elements().ElementAtOrDefault(index) != null);
        }

        /// <summary>
        /// 删除键
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Delete(string name)
        {
            var elem = XmlElement.Element(name);
            if (elem != null)
            {
                elem.Remove();
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 根据索引删除元素
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool Delete(int index)
        {
            var elem = XmlElement.Elements().ElementAtOrDefault(index);
            if (elem != null)
            {
                elem.Remove();
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Deserialize<T>()
        {
            return (T)Deserialize(typeof(T));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = (IsArray)
                ? Delete((int)args[0])
                : Delete((string)args[0]);
            return true;
        }

        /// <summary>
        /// 判断是否定义
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length > 0)
            {
                result = null;
                return false;
            }

            result = IsDefined(binder.Name);
            return true;
        }

        /// <summary>
        /// 支持 Foreach 遍历
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(IEnumerable) || binder.Type == typeof(object[]))
            {
                var ie = (IsArray)
                    ? XmlElement.Elements().Select(x => ToValue(x))
                    : XmlElement.Elements().Select(x => (dynamic)new KeyValuePair<string, object>(x.Name.LocalName, ToValue(x)));
                result = (binder.Type == typeof(object[])) ? ie.ToArray() : ie;
            }
            else
            {
                result = Deserialize(binder.Type);
            }
            return true;
        }

        /// <summary>
        /// 获取索引值
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            return (IsArray)
                ? TryGet(XmlElement.Elements().ElementAtOrDefault((int)indexes[0]), out result)
                : TryGet(XmlElement.Element((string)indexes[0]), out result);
        }

        /// <summary>
        /// 获取成员值
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return (IsArray)
                ? TryGet(XmlElement.Elements().ElementAtOrDefault(int.Parse(binder.Name)), out result)
                : TryGet(XmlElement.Element(binder.Name), out result);
        }

        /// <summary>
        /// 设置索引
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return (IsArray)
                ? TrySet((int)indexes[0], value)
                : TrySet((string)indexes[0], value);
        }

        /// <summary>
        /// 设置成员
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return (IsArray)
                ? TrySet(int.Parse(binder.Name), value)
                : TrySet(binder.Name, value);
        }

        /// <summary>
        /// 获取动态成员名称
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return (IsArray)
                ? XmlElement.Elements().Select((x, i) => i.ToString())
                : XmlElement.Elements().Select(x => x.Name.LocalName);
        }

        /// <summary>
        /// 重写 .ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // <foo type="null"></foo> is can't serialize. replace to <foo type="null" />
            foreach (var elem in XmlElement.Descendants().Where(x => x.Attribute("type").Value == "null"))
            {
                elem.RemoveNodes();
            }
            return CreateJsonString(new XStreamingElement("root", CreateTypeAttr(jsonType), XmlElement.Elements()));
        }

        /// <summary>
        /// 固化粘土，也就是直接输出对象
        /// </summary>
        /// <returns></returns>
        public object Solidify()
        {
            return Solidify<object>();
        }

        /// <summary>
        /// 固化粘土，也就是直接输出对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Solidify<T>()
        {
            return JSON.Deserialize<T>(ToString());
        }

        /// <summary>
        /// 输出字典类型
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> ToDictionary()
        {
            return Solidify().ToDictionary();
        }

        /// <summary>
        /// JSON 类型
        /// </summary>
        private enum JsonType
        {
            @string, number, boolean, @object, array, @null
        }

        /// <summary>
        /// XElement 转动态类型
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static dynamic ToValue(XElement element)
        {
            var type = (JsonType)Enum.Parse(typeof(JsonType), element.Attribute("type").Value);
            return type switch
            {
                JsonType.boolean => (bool)element,
                JsonType.number => (double)element,
                JsonType.@string => (string)element,
                JsonType.@object or JsonType.array => new Clay(element, type),
                _ => null,
            };
        }

        /// <summary>
        /// 获取 JSON 类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static JsonType GetJsonType(object obj)
        {
            if (obj == null) return JsonType.@null;

            var objType = obj.GetType();

            // 将特别类型转换成 string
            if (ToBeConvertStringTypes.Contains(objType)) return JsonType.@string;

            // 处理循环 Clay 类型
            if (obj is ExpandoObject) return JsonType.@object;

            return Type.GetTypeCode(objType) switch
            {
                TypeCode.Boolean => JsonType.boolean,
                TypeCode.String or TypeCode.Char or TypeCode.DateTime => JsonType.@string,
                TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal or TypeCode.SByte or TypeCode.Byte => JsonType.number,
                TypeCode.Object => (obj is IEnumerable) ? JsonType.array : JsonType.@object,
                _ => JsonType.@null,
            };
        }

        /// <summary>
        /// 创建类型属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static XAttribute CreateTypeAttr(JsonType type)
        {
            return new XAttribute("type", type.ToString());
        }

        /// <summary>
        /// 创建 JSON 节点
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static object CreateJsonNode(object obj)
        {
            var type = GetJsonType(obj);
            return type switch
            {
                JsonType.@string or JsonType.number => obj,
                JsonType.boolean => obj.ToString().ToLower(),
                JsonType.@object => CreateXObject(obj),
                JsonType.array => CreateXArray(obj as IEnumerable),
                _ => null,
            };
        }

        /// <summary>
        /// 创建 XStreamingElement 对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static IEnumerable<XStreamingElement> CreateXArray<T>(T obj) where T : IEnumerable
        {
            return obj.Cast<object>()
                .Select(o => new XStreamingElement("item", CreateTypeAttr(GetJsonType(o)), CreateJsonNode(o)));
        }

        /// <summary>
        /// 创建 XStreamingElement 对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static IEnumerable<XStreamingElement> CreateXObject(object obj)
        {
            if (obj is ExpandoObject expando)
            {
                var dict = (IDictionary<string, object>)expando;
                return dict.Select(a => new XStreamingElement(a.Key, CreateTypeAttr(GetJsonType(a.Value)), CreateJsonNode(a.Value)));
            }

            return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(pi => new { pi.Name, Value = pi.GetValue(obj, null) })
                .Select(a => new XStreamingElement(a.Name, CreateTypeAttr(GetJsonType(a.Value)), CreateJsonNode(a.Value)));
        }

        /// <summary>
        /// 创建 JSON 字符串
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static string CreateJsonString(XStreamingElement element)
        {
            using var ms = new MemoryStream();
            using var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
            element.WriteTo(writer);
            writer.Flush();
            return Encoding.Unicode.GetString(ms.ToArray());
        }

        /// <summary>
        /// JSON 类型
        /// </summary>
        private readonly JsonType jsonType;

        /// <summary>
        /// 读取值
        /// </summary>
        /// <param name="element"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static bool TryGet(XElement element, out object result)
        {
            if (element == null)
            {
                result = null;
                return false;
            }

            result = ToValue(element);
            return true;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool TrySet(string name, object value)
        {
            var type = GetJsonType(value);

            // 处理循环 Clay 类型
            if (value is Clay clay)
            {
                if (clay.IsObject) value = value.ToExpandoObject();
                else if (clay.IsArray)
                {
                    var list = new List<object>();
                    foreach (var item in (dynamic)clay)
                    {
                        list.Add(item is Clay c ? c.ToExpandoObject() : item);
                    }
                    value = list;
                }
            }

            var element = XmlElement.Element(name);
            if (element == null)
            {
                XmlElement.Add(new XElement(name, CreateTypeAttr(type), CreateJsonNode(value)));
            }
            else
            {
                element.Attribute("type").Value = type.ToString();
                element.ReplaceNodes(CreateJsonNode(value));
            }

            return true;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool TrySet(int index, object value)
        {
            var type = GetJsonType(value);
            var e = XmlElement.Elements().ElementAtOrDefault(index);
            if (e == null)
            {
                XmlElement.Add(new XElement("item", CreateTypeAttr(type), CreateJsonNode(value)));
            }
            else
            {
                e.Attribute("type").Value = type.ToString();
                e.ReplaceNodes(CreateJsonNode(value));
            }

            return true;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private object Deserialize(Type type)
        {
            return (IsArray) ? DeserializeArray(type) : DeserializeObject(type);
        }

        /// <summary>
        /// 反序列化值
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private static dynamic DeserializeValue(XElement element, Type elementType)
        {
            var value = ToValue(element);

            if (value is Clay json)
            {
                value = json.Deserialize(elementType);
            }

            return Furion.Extensions.ObjectExtensions.ChangeType(value, elementType);
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private object DeserializeObject(Type targetType)
        {
            var result = Activator.CreateInstance(targetType);
            var dict = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(pi => pi.Name, pi => pi);

            foreach (var item in XmlElement.Elements())
            {
                if (!dict.TryGetValue(item.Name.LocalName, out var propertyInfo)) continue;
                var value = Clay.DeserializeValue(item, propertyInfo.PropertyType);
                propertyInfo.SetValue(result, value, null);
            }
            return result;
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private object DeserializeArray(Type targetType)
        {
            if (targetType.IsArray)
            {
                var elemType = targetType.GetElementType();
                dynamic array = Array.CreateInstance(elemType, XmlElement.Elements().Count());
                var index = 0;
                foreach (var item in XmlElement.Elements())
                {
                    array[index++] = Clay.DeserializeValue(item, elemType);
                }
                return array;
            }
            else
            {
                var elemType = targetType.GetGenericArguments()[0];
                dynamic list = Activator.CreateInstance(targetType);
                foreach (var item in XmlElement.Elements())
                {
                    list.Add(Clay.DeserializeValue(item, elemType));
                }
                return list;
            }
        }

        /// <summary>
        /// 将被转换成字符串的类型
        /// </summary>
        private static readonly Type[] ToBeConvertStringTypes = new[] {
            typeof(DateTimeOffset)
        };
    }
}