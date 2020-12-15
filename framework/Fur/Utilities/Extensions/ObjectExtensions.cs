using Fur.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fur.Extensions
{
    /// <summary>
    /// 对象拓展类
    /// </summary>
    [SkipScan]
    public static class ObjectExtensions
    {
        /// <summary>
        /// 将 DateTimeOffset 转换成 DateTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(this DateTimeOffset dateTime)
        {
            if (dateTime.Offset.Equals(TimeSpan.Zero))
                return dateTime.UtcDateTime;
            else if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
                return DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local);
            else
                return dateTime.DateTime;
        }

        /// <summary>
        /// 判断是否是富基元类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        internal static bool IsRichPrimitive(this Type type)
        {
            // 处理元组类型
            if (type.IsValueTuple()) return false;

            // 基元类型或值类型或字符串类型
            if (type.IsPrimitive || type.IsValueType || type == typeof(string)) return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return type.GenericTypeArguments[0].IsRichPrimitive();

            return false;
        }

        /// <summary>
        /// 合并两个字典
        /// </summary>
        /// <param name="dic">字典</param>
        /// <param name="newDic">新字典</param>
        /// <returns></returns>
        internal static Dictionary<string, string> AddOrUpdate(this Dictionary<string, string> dic, Dictionary<string, string> newDic)
        {
            foreach (var key in newDic.Keys)
            {
                if (dic.ContainsKey(key))
                    dic[key] = newDic[key];
                else
                    dic.Add(key, newDic[key]);
            }

            return dic;
        }

        /// <summary>
        /// 判断是否是元组类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        internal static bool IsValueTuple(this Type type)
        {
            return type.ToString().StartsWith(typeof(ValueTuple).FullName);
        }

        /// <summary>
        /// 判断方法是否是异步
        /// </summary>
        /// <param name="method">方法</param>
        /// <returns></returns>
        internal static bool IsAsync(this MethodInfo method)
        {
            return method.ReturnType.IsAsync();
        }

        /// <summary>
        /// 判断类型是否是异步类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsAsync(this Type type)
        {
            return type.ToString().StartsWith(typeof(Task).FullName);
        }

        /// <summary>
        /// 判断类型是否实现某个泛型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="generic">泛型类型</param>
        /// <returns>bool</returns>
        internal static bool HasImplementedRawGeneric(this Type type, Type generic)
        {
            // 检查接口类型
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;

            // 检查类型
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }

            return false;

            // 判断逻辑
            bool IsTheRawGenericType(Type type) => generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
        }

        /// <summary>
        /// 判断是否是匿名类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        internal static bool IsAnonymous(this object obj)
        {
            var type = obj.GetType();

            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && type.Attributes.HasFlag(TypeAttributes.NotPublic);
        }

        /// <summary>
        /// 获取所有祖先类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetAncestorTypes(this Type type)
        {
            var ancestorTypes = new List<Type>();
            while (type != null && type != typeof(object))
            {
                if (IsNoObjectBaseType(type))
                {
                    var baseType = type.BaseType;
                    ancestorTypes.Add(baseType);
                    type = baseType;
                }
                else break;
            }

            return ancestorTypes;

            static bool IsNoObjectBaseType(Type type) => type.BaseType != typeof(object);
        }

        /// <summary>
        /// 获取方法真实返回类型
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        internal static Type GetMethodRealReturnType(this MethodInfo method)
        {
            // 判断是否是异步方法
            var isAsyncMethod = method.IsAsync();

            // 获取类型返回值并处理 Task 和 Task<T> 类型返回值
            var returnType = method.ReturnType;
            return isAsyncMethod ? (returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(void)) : returnType;
        }

        /// <summary>
        /// 返回异步类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="realType"></param>
        /// <returns></returns>
        internal static object ToTaskResult(this object obj, Type realType)
        {
            return typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(realType).Invoke(null, new object[] { obj });
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string ToTitleCase(this string str)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        /// <summary>
        /// 首字母小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string ToTitlePascal(this string str)
        {
            if (str == null) return string.Empty;

            int iLen = str.Length;
            return iLen == 0
                ? string.Empty
                : iLen == 1
                    ? str.ToLower()
                    : str[0].ToString().ToLower() + str[1..];
        }

        /// <summary>
        /// 将一个对象转换为指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static T ChangeType<T>(this object obj)
        {
            return (T)ChangeType(obj, typeof(T));
        }

        /// <summary>
        /// 将一个对象转换为指定类型
        /// </summary>
        /// <param name="obj">待转换的对象</param>
        /// <param name="type">目标类型</param>
        /// <returns>转换后的对象</returns>
        internal static object ChangeType(this object obj, Type type)
        {
            if (type == null) return obj;
            if (obj == null) return type.IsValueType ? Activator.CreateInstance(type) : null;

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (type.IsAssignableFrom(obj.GetType())) return obj;
            else if ((underlyingType ?? type).IsEnum)
            {
                if (underlyingType != null && string.IsNullOrEmpty(obj.ToString())) return null;
                else return Enum.Parse(underlyingType ?? type, obj.ToString());
            }
            else if (typeof(IConvertible).IsAssignableFrom(underlyingType ?? type))
            {
                try
                {
                    return Convert.ChangeType(obj, underlyingType ?? type, null);
                }
                catch
                {
                    return underlyingType == null ? Activator.CreateInstance(type) : null;
                }
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(obj.GetType())) return converter.ConvertFrom(obj);

                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    var o = constructor.Invoke(null);
                    var propertys = type.GetProperties();
                    var oldType = obj.GetType();

                    foreach (var property in propertys)
                    {
                        var p = oldType.GetProperty(property.Name);
                        if (property.CanWrite && p != null && p.CanRead)
                        {
                            property.SetValue(o, ChangeType(p.GetValue(obj, null), property.PropertyType), null);
                        }
                    }
                    return o;
                }
            }
            return obj;
        }
    }
}