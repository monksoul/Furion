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

using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Furion.Extensions;

/// <summary>
/// 对象扩展类
/// </summary>
public static class ObsoleteObjectExtensions
{
    /// <summary>
    /// 将 DateTimeOffset 转换成本地 DateTime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ConvertToDateTime(this DateTimeOffset dateTime)
    {
        if (dateTime.Offset.Equals(TimeSpan.Zero))
            return dateTime.UtcDateTime;
        if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
            return dateTime.ToLocalTime().DateTime;
        else
            return dateTime.DateTime;
    }

    /// <summary>
    /// 将 DateTimeOffset? 转换成本地 DateTime?
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime? ConvertToDateTime(this DateTimeOffset? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ConvertToDateTime() : null;
    }

    /// <summary>
    /// 将 DateTime 转换成 DateTimeOffset
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset ConvertToDateTimeOffset(this DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
    }

    /// <summary>
    /// 将 DateTime? 转换成 DateTimeOffset?
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset? ConvertToDateTimeOffset(this DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ConvertToDateTimeOffset() : null;
    }

    /// <summary>
    /// 将时间戳转换为 DateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    internal static DateTime ConvertToDateTime(this long timestamp)
    {
        var timeStampDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var digitCount = (int)Math.Floor(Math.Log10(timestamp) + 1);

        if (digitCount != 13 && digitCount != 10)
        {
            throw new ArgumentException("Data is not a valid timestamp format.");
        }

        return (digitCount == 13
            ? timeStampDateTime.AddMilliseconds(timestamp)  // 13 位时间戳
            : timeStampDateTime.AddSeconds(timestamp)).ToLocalTime();   // 10 位时间戳
    }

    /// <summary>
    /// 将 IFormFile 转换成 byte[]
    /// </summary>
    /// <param name="formFile"></param>
    /// <returns></returns>
    public static byte[] ToByteArray(this IFormFile formFile)
    {
        var fileLength = formFile.Length;
        using var stream = formFile.OpenReadStream();
        var bytes = new byte[fileLength];

        _ = stream.Read(bytes, 0, (int)fileLength);

        return bytes;
    }

    /// <summary>
    /// 将流保存到本地磁盘
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static void CopyToSave(this Stream stream, string path)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

        using var fileStream = File.Create(path);
        stream.CopyTo(fileStream);
    }

    /// <summary>
    /// 将字节数组保存到本地磁盘
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static void CopyToSave(this byte[] bytes, string path)
    {
        using var stream = new MemoryStream(bytes);
        stream.CopyToSave(path);
    }

    /// <summary>
    /// 将流保存到本地磁盘
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="path">需包含文件名完整路径</param>
    /// <returns></returns>
    public static async Task CopyToSaveAsync(this Stream stream, string path)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        // 文件名判断
        if (string.IsNullOrWhiteSpace(Path.GetFileName(path)))
        {
            throw new ArgumentException("The parameter of <path> parameter must include the complete file name.");
        }

        using var fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }

    /// <summary>
    /// 将字节数组保存到本地磁盘
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task CopyToSaveAsync(this byte[] bytes, string path)
    {
        using var stream = new MemoryStream(bytes);
        await stream.CopyToSaveAsync(path);
    }

    /// <summary>
    /// 添加防抖操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 300)
    {
        CancellationTokenSource lastCts = null;
        var lockObj = new object();

        return arg =>
        {
            CancellationTokenSource currentCts;
            lock (lockObj)
            {
                lastCts?.Cancel();
                lastCts?.Dispose();

                currentCts = new CancellationTokenSource();
                lastCts = currentCts;
            }

            Task.Delay(milliseconds, currentCts.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled)
                {
                    func(arg);
                }
                task.Dispose();
            }, TaskScheduler.Default);
        };
    }

    /// <summary>
    /// 添加防抖操作
    /// </summary>
    /// <param name="func"></param>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static Action Debounce(this Action func, int milliseconds = 300)
    {
        CancellationTokenSource lastCts = null;
        var lockObj = new object();

        return () =>
        {
            CancellationTokenSource currentCts;
            lock (lockObj)
            {
                lastCts?.Cancel();
                lastCts?.Dispose();

                currentCts = new CancellationTokenSource();
                lastCts = currentCts;
            }

            Task.Delay(milliseconds, currentCts.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled)
                {
                    func();
                }
                task.Dispose();
            }, TaskScheduler.Default);
        };
    }

    /// <summary>
    /// 检查类型是否是 internal 类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal static bool IsInternal(this Type type)
    {
        return type.IsNested
            ? type.IsNestedAssembly || type.IsNestedFamANDAssem
            : !type.IsPublic;
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

        // 处理数组类型，基元数组类型也可以是基元类型
        if (type.IsArray) return type.GetElementType().IsRichPrimitive();

        // 基元类型或值类型或字符串类型
        if (type.IsPrimitive || type.IsValueType || type == typeof(string)) return true;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return type.GenericTypeArguments[0].IsRichPrimitive();

        return false;
    }

    /// <summary>
    /// 合并两个字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic">字典</param>
    /// <param name="newDic">新字典</param>
    /// <returns></returns>
    internal static Dictionary<string, T> AddOrUpdate<T>(this Dictionary<string, T> dic, IDictionary<string, T> newDic)
    {
        foreach (var key in newDic.Keys)
        {
            if (dic.TryGetValue(key, out var value))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, newDic[key]);
            }
        }

        return dic;
    }

    /// <summary>
    /// 合并两个字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic">字典</param>
    /// <param name="newDic">新字典</param>
    internal static void AddOrUpdate<T>(this ConcurrentDictionary<string, T> dic, Dictionary<string, T> newDic)
    {
        foreach (var (key, value) in newDic)
        {
            dic.AddOrUpdate(key, value, (key, old) => value);
        }
    }

    /// <summary>
    /// 判断是否是元组类型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    internal static bool IsValueTuple(this Type type)
    {
        return type.Namespace == "System" && type.Name.Contains("ValueTuple`");
    }

    /// <summary>
    /// 判断方法是否是异步
    /// </summary>
    /// <param name="method">方法</param>
    /// <returns></returns>
    internal static bool IsAsync(this MethodInfo method)
    {
        return method.GetCustomAttribute<AsyncMethodBuilderAttribute>() != null
            || method.ReturnType.ToString().StartsWith(typeof(Task).FullName);
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
        var type = obj is Type t ? t : obj.GetType();

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
    internal static Type GetRealReturnType(this MethodInfo method)
    {
        // 判断是否是异步方法
        var isAsyncMethod = method.IsAsync();

        // 获取类型返回值并处理 Task 和 Task<T> 类型返回值
        var returnType = method.ReturnType;
        return isAsyncMethod ? (returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(void)) : returnType;
    }

    /// <summary>
    /// 将一个对象转换为指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T ChangeType<T>(this object obj)
    {
        return (T)ChangeType(obj, typeof(T));
    }

    /// <summary>
    /// 将一个对象转换为指定类型
    /// </summary>
    /// <param name="obj">待转换的对象</param>
    /// <param name="type">目标类型</param>
    /// <returns>转换后的对象</returns>
    public static object ChangeType(this object obj, Type type)
    {
        if (type == null) return obj;
        if (type == typeof(string)) return obj?.ToString();
        if (type == typeof(Guid) && obj != null) return Guid.Parse(obj.ToString());
        if (type == typeof(bool) && obj != null && obj is not bool)
        {
            var objStr = obj.ToString().ToLower();
            if (objStr == "1" || objStr == "true" || objStr == "yes" || objStr == "on") return true;
            return false;
        }
        if (obj == null) return type.IsValueType ? Activator.CreateInstance(type) : null;

        var underlyingType = Nullable.GetUnderlyingType(type);
        if (type.IsAssignableFrom(obj.GetType())) return obj;
        else if ((underlyingType ?? type).IsEnum)
        {
            if (underlyingType != null && string.IsNullOrWhiteSpace(obj.ToString())) return null;
            else return Enum.Parse(underlyingType ?? type, obj.ToString());
        }
        // 处理 DateTime -> DateTimeOffset 类型
        else if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(DateTimeOffset)))
        {
            return ((DateTime)obj).ConvertToDateTimeOffset();
        }
        // 处理 DateTimeOffset -> DateTime 类型
        else if (obj.GetType().Equals(typeof(DateTimeOffset)) && (underlyingType ?? type).Equals(typeof(DateTime)))
        {
            return ((DateTimeOffset)obj).ConvertToDateTime();
        }
        // 处理 DateTime -> DateOnly 类型
        else if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(DateOnly)))
        {
            return DateOnly.FromDateTime(((DateTime)obj));
        }
        // 处理 DateTime -> TimeOnly 类型
        else if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(TimeOnly)))
        {
            return TimeOnly.FromDateTime(((DateTime)obj));
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

    /// <summary>
    /// 查找方法指定特性，如果没找到则继续查找声明类
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="method"></param>
    /// <param name="inherit"></param>
    /// <param name="searchFromReflectedType">searchFromRuntimeType</param>
    /// <returns></returns>
    internal static TAttribute GetFoundAttribute<TAttribute>(this MethodInfo method, bool inherit, bool searchFromReflectedType = false)
        where TAttribute : Attribute
    {
        // 获取方法所在类型
        var declaringType = !searchFromReflectedType ? method.DeclaringType : method.ReflectedType;   // 解决嵌套继承问题

        var attributeType = typeof(TAttribute);

        // 判断方法是否定义指定特性，如果没有再查找声明类
        var foundAttribute = method.IsDefined(attributeType, inherit)
            ? method.GetCustomAttribute<TAttribute>(inherit)
            : (
                declaringType.IsDefined(attributeType, inherit)
                ? declaringType.GetCustomAttribute<TAttribute>(inherit)
                : default
            );

        return foundAttribute;
    }

    /// <summary>
    /// 格式化字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string Format(this string str, params object[] args)
    {
        return args == null || args.Length == 0 ? str : string.Format(str, args);
    }

    /// <summary>
    /// 切割骆驼命名式字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string[] SplitCamelCase(this string str)
    {
        if (str == null) return [];

        if (string.IsNullOrWhiteSpace(str)) return [str];
        if (str.Length == 1) return [str];

        return Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})")
            .Where(u => u.Length > 0)
            .ToArray();
    }

    /// <summary>
    /// JsonElement 转 Object
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <returns></returns>
    internal static object ToObject(this JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.String:
                return jsonElement.GetString();

            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                return default;

            case JsonValueKind.Number:
                return jsonElement.GetDecimal();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return jsonElement.GetBoolean();

            case JsonValueKind.Object:
                var enumerateObject = jsonElement.EnumerateObject();
                var dic = new Dictionary<string, object>();
                foreach (var item in enumerateObject)
                {
                    dic.Add(item.Name, item.Value.ToObject());
                }
                return dic;

            case JsonValueKind.Array:
                var enumerateArray = jsonElement.EnumerateArray();
                var list = new List<object>();
                foreach (var item in enumerateArray)
                {
                    list.Add(item.ToObject());
                }
                return list;

            default:
                return default;
        }
    }

    /// <summary>
    /// 清除字符串前后缀
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="pos">0：前后缀，1：后缀，-1：前缀</param>
    /// <param name="affixes">前后缀集合</param>
    /// <returns></returns>
    internal static string ClearStringAffixes(this string str, int pos = 0, params string[] affixes)
    {
        // 空字符串直接返回
        if (string.IsNullOrWhiteSpace(str)) return str;

        // 空前后缀集合直接返回
        if (affixes == null || affixes.Length == 0) return str;

        var startCleared = false;
        var endCleared = false;

        string tempStr = null;
        foreach (var affix in affixes)
        {
            if (string.IsNullOrWhiteSpace(affix)) continue;

            if (pos != 1 && !startCleared && str.StartsWith(affix, StringComparison.OrdinalIgnoreCase))
            {
                tempStr = str[affix.Length..];
                startCleared = true;
            }
            if (pos != -1 && !endCleared && str.EndsWith(affix, StringComparison.OrdinalIgnoreCase))
            {
                var _tempStr = !string.IsNullOrWhiteSpace(tempStr) ? tempStr : str;
                tempStr = _tempStr[..^affix.Length];
                endCleared = true;

                if (string.IsNullOrWhiteSpace(tempStr))
                {
                    tempStr = null;
                    endCleared = false;
                }
            }
            if (startCleared && endCleared) break;
        }

        return !string.IsNullOrWhiteSpace(tempStr) ? tempStr : str;
    }

    /// <summary>
    /// 首字母小写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string ToLowerCamelCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        return string.Concat(str.First().ToString().ToLower(), str.AsSpan(1));
    }

    /// <summary>
    /// 首字母大写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string ToUpperCamelCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        return string.Concat(str.First().ToString().ToUpper(), str.AsSpan(1));
    }

    /// <summary>
    /// 判断集合是否为空
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="collection">集合对象</param>
    /// <returns><see cref="bool"/> 实例，true 表示空集合，false 表示非空集合</returns>
    internal static bool IsEmpty<T>(this IEnumerable<T> collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// 获取类型自定义特性
    /// </summary>
    /// <typeparam name="TAttribute">特性类型</typeparam>
    /// <param name="type">类类型</param>
    /// <param name="inherit">是否继承查找</param>
    /// <returns>特性对象</returns>
    internal static TAttribute GetTypeAttribute<TAttribute>(this Type type, bool inherit = false)
        where TAttribute : Attribute
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);

        // 检查特性并获取特性对象
        return type.IsDefined(typeof(TAttribute), inherit)
            ? type.GetCustomAttribute<TAttribute>(inherit)
            : default;
    }
}