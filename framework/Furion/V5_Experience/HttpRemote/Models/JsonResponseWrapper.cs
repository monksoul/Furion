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

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     JSON 响应反序列化包装器上下文
/// </summary>
/// <param name="Instance">
///     包装类型的具体实例（例如 <![CDATA[ApiResult<T>]]> 的实例）
/// </param>
/// <param name="Result">目标结果</param>
/// <param name="ResponseMessage">
///     <see cref="HttpResponseMessage" />
/// </param>
public sealed record JsonResponseWrapperContext(object? Instance, object? Result, HttpResponseMessage ResponseMessage)
{
    /// <summary>
    ///     包装类型属性值访问器缓存字典
    /// </summary>
    internal static readonly ConcurrentDictionary<(Type Type, string PropertyName), Func<object, object?>>
        _getterCache = new();

    /// <summary>
    ///     获取 <see cref="Instance" /> 指定属性值
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    /// <typeparam name="T">属性类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    public T? GetPropertyValue<T>(string propertyName)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // 空检查
        if (Instance is null)
        {
            return default;
        }

        // 获取或构建属性值访问器
        var getter = _getterCache.GetOrAdd((Instance.GetType(), propertyName),
            key => BuildPropertyGetter(key.Type, key.PropertyName));

        return (T?)getter(Instance);
    }

    /// <summary>
    ///     为指定的具体泛型类型动态构建属性值访问器
    /// </summary>
    /// <param name="concreteType">包装类型的具体类型</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>
    ///     <see cref="Func{T,TResult}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static Func<object, object?> BuildPropertyGetter(Type concreteType, string propertyName)
    {
        // 获取属性对象
        var property = concreteType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
                       ?? throw new InvalidOperationException(
                           $"Property '{propertyName}' not found on type '{concreteType}'.");

        // 构建 instance => instance.属性名 表达式
        var param = Expression.Parameter(typeof(object), "instance");
        var cast = Expression.Convert(param, concreteType);
        var propertyAccess = Expression.Property(cast, property);
        var convertToObject = Expression.Convert(propertyAccess, typeof(object));

        // 编译 Lambda 表达式
        return Expression.Lambda<Func<object, object?>>(convertToObject, param).Compile();
    }
}

/// <summary>
///     JSON 响应反序列化包装器
/// </summary>
public sealed class JsonResponseWrapper
{
    /// <summary>
    ///     <inheritdoc cref="JsonResponseWrapper" />
    /// </summary>
    /// <param name="genericType">包装泛型类型定义类型</param>
    /// <param name="propertyName">目标结果属性名</param>
    /// <exception cref="ArgumentException"></exception>
    public JsonResponseWrapper(Type genericType, string propertyName)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(genericType);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        // 检查是否是泛型定义类型且参数数量为一个
        if (!genericType.IsGenericTypeDefinition || genericType.GetGenericArguments().Length != 1)
        {
            throw new ArgumentException(
                $"The provided type must be a generic type definition (e.g., typeof(ApiResult<>)). Actual type: {genericType}.",
                nameof(genericType));
        }

        GenericType = genericType;
        PropertyName = propertyName;
    }

    /// <summary>
    ///     包装泛型类型定义类型
    /// </summary>
    public Type GenericType { get; }

    /// <summary>
    ///     目标结果属性名
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    ///     目标结果处理器
    /// </summary>
    public Func<JsonResponseWrapperContext, object?>? ResultHandler { get; set; }

    /// <summary>
    ///     获取目标结果值
    /// </summary>
    /// <param name="instance">
    ///     包装类型的具体实例（例如 <![CDATA[ApiResult<T>]]> 的实例）
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public object? GetResultValue(object? instance, HttpResponseMessage httpResponseMessage)
    {
        // 空检查
        if (instance is null)
        {
            // 调用目标结果处理器
            return ResultHandler?.Invoke(new JsonResponseWrapperContext(null, null, httpResponseMessage));
        }

        // 检查实例类型是否和包装泛型类型定义类型一致
        var actualType = instance.GetType();
        if (!actualType.IsGenericType || actualType.GetGenericTypeDefinition() != GenericType)
        {
            throw new ArgumentException(
                $"The instance type '{actualType}' is not a constructed generic type of '{GenericType}'.",
                nameof(instance));
        }

        // 获取或构建属性值访问器
        var getter = JsonResponseWrapperContext._getterCache.GetOrAdd((actualType, PropertyName),
            key => JsonResponseWrapperContext.BuildPropertyGetter(key.Type, key.PropertyName));

        // 获取目标结果属性值
        var result = getter(instance);

        // 调用目标结果处理器
        return ResultHandler is not null
            ? ResultHandler(new JsonResponseWrapperContext(instance, result, httpResponseMessage))
            : result;
    }
}