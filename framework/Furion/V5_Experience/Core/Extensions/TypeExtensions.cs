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

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Furion.Extensions;

/// <summary>
///     <see cref="Type" /> 扩展类
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    ///     检查类型是否是数组或集合类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="underlyingType">元素类型</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsArrayOrCollection(this Type type, [NotNullWhen(true)] out Type? underlyingType)
    {
        underlyingType = null;

        // 检查类型是否是数组类型
        if (type.IsArray)
        {
            underlyingType = type.GetElementType()!;
            return true;
        }

        // 如果不是泛型类型
        if (!type.IsGenericType)
        {
            return false;
        }

        // 获取泛型参数
        var genericArguments = type.GetGenericArguments();

        // 检查类型是否是为单个泛型参数类型且实现了 IEnumerable<> 接口
        if (genericArguments.Length != 1 || !typeof(IEnumerable<>).IsDefinitionEquals(type))
        {
            return false;
        }

        underlyingType = genericArguments[0];
        return true;
    }

    /// <summary>
    ///     检查类型是否是基本类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsBasicType(this Type type)
    {
        while (true)
        {
            // 如果是基元类型则直接返回
            if (type.IsPrimitive)
            {
                return true;
            }

            // 处理可空类型
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                return type == typeof(string) || type == typeof(decimal) ||
                       type == typeof(Guid) ||
                       type == typeof(DateTime) ||
                       type == typeof(DateTimeOffset) || type == typeof(DateOnly) || type == typeof(TimeSpan) ||
                       type == typeof(TimeOnly) || type == typeof(char) || type == typeof(IntPtr) ||
                       type == typeof(UIntPtr);
            }

            var underlyingType = type.GetGenericArguments()[0];
            type = underlyingType;
        }
    }

    /// <summary>
    ///     检查类型是否是基本类型或枚举类型或由它们组成的数组或集合类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsBaseTypeOrEnumOrCollection(this Type type) =>
        type.IsBasicType() || type.IsEnum || (type.IsArrayOrCollection(out var underlyingType) &&
                                              underlyingType.IsBaseTypeOrEnumOrCollection());

    /// <summary>
    ///     检查类型是否是静态类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsStatic(this Type type) => type is { IsSealed: true, IsAbstract: true };

    /// <summary>
    ///     检查类型是否具有 internal 可见性
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsInternal(this Type type) =>
        type.IsNested ? type.IsNestedAssembly || type.IsNestedFamORAssem /* protected internal */ : !type.IsPublic;

    /// <summary>
    ///     检查类型是否是匿名类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsAnonymous(this Type type)
    {
        // 检查是否贴有 [CompilerGenerated] 特性
        if (!type.IsDefined(typeof(CompilerGeneratedAttribute), false))
        {
            return false;
        }

        // 类型限定名是否以 <> 开头且以 AnonymousType 结尾
        return type.FullName is not null
               && type.FullName.StartsWith("<>")
               && type.FullName.Contains("AnonymousType");
    }

    /// <summary>
    ///     检查类型是否可实例化
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsInstantiable(this Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && !type.IsStatic();

    /// <summary>
    ///     检查类型是否是结构类型
    /// </summary>
    /// <remarks>唯有如 <c>public struct StructName {}</c> 类型定义才符合验证要求。</remarks>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsStruct(this Type type) => type is { IsValueType: true, IsPrimitive: false, IsEnum: false };

    /// <summary>
    ///     检查类型是否派生自指定类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="fromType">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsAlienAssignableTo(this Type type, Type fromType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(fromType);

        return fromType != type
               && fromType.IsAssignableFrom(type);
    }

    /// <summary>
    ///     获取指定特性实例
    /// </summary>
    /// <remarks>若特性不存在则返回 null</remarks>
    /// <typeparam name="TAttribute">特性类型</typeparam>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="inherit">是否在基类中搜索</param>
    /// <returns>
    ///     <typeparamref name="TAttribute" />
    /// </returns>
    internal static TAttribute? GetDefinedCustomAttribute<TAttribute>(this Type type, bool inherit = false)
        where TAttribute : Attribute =>
        // 检查是否定义
        !type.IsDefined(typeof(TAttribute), inherit)
            ? null
            : type.GetCustomAttribute<TAttribute>(inherit);

    /// <summary>
    ///     检查类型是否定义了公开无参构造函数
    /// </summary>
    /// <remarks>用于 <see cref="Activator.CreateInstance(Type)" /> 实例化</remarks>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool HasDefinePublicParameterlessConstructor(this Type type) =>
        type.IsInstantiable()
        && type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes) is not null;

    /// <summary>
    ///     检查类型和指定类型定义是否相等
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="compareType">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsDefinitionEqual(this Type type, Type? compareType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(compareType);

        return type == compareType
               || (type.IsGenericType
                   && compareType.IsGenericType
                   && type.IsGenericTypeDefinition // 💡
                   && type == compareType.GetGenericTypeDefinition());
    }

    /// <summary>
    ///     检查类型和指定类型定义是否相等
    /// </summary>
    /// <remarks>将查找所有派生的基类和实现的接口。</remarks>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="compareType">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsDefinitionEquals(this Type type, Type? compareType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(compareType);

        // 检查类型和指定类型定义是否相等
        if (type.IsDefinitionEqual(compareType))
        {
            return true;
        }

        // 递归查找所有基类
        var baseType = compareType.BaseType;
        while (baseType is not null && baseType != typeof(object))
        {
            // 检查类型和指定类型定义是否相等
            if (type.IsDefinitionEqual(baseType))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        // 检查所有实现的接口定义是否一致
        return compareType.GetInterfaces().Any(type.IsDefinitionEqual);
    }

    /// <summary>
    ///     检查类型和指定继承类型是否兼容
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="inheritType">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsCompatibilityTo(this Type type, Type? inheritType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(inheritType);

        return inheritType != typeof(object)
               && inheritType.IsAssignableFrom(type)
               && (!type.IsGenericType
                   || (type.IsGenericType
                       && inheritType.IsGenericType
                       && type.GetTypeInfo().GenericTypeParameters.SequenceEqual(inheritType.GenericTypeArguments)));
    }

    /// <summary>
    ///     检查类型是否定义了指定方法
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="name">方法名称</param>
    /// <param name="accessibilityBindingFlags">可访问性成员绑定标记</param>
    /// <param name="methodInfo">
    ///     <see cref="MethodInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsDeclarationMethod(this Type type
        , string name
        , BindingFlags accessibilityBindingFlags
        , out MethodInfo? methodInfo)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        methodInfo = type.GetMethod(name,
            accessibilityBindingFlags | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        return methodInfo is not null;
    }

    /// <summary>
    ///     检查类型是否是整数类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsInteger(this Type type)
    {
        // 如果是枚举或浮点类型则直接返回
        if (type.IsEnum || type.IsDecimal())
        {
            return false;
        }

        // 检查 TypeCode
        return Type.GetTypeCode(type) is TypeCode.Byte
            or TypeCode.SByte
            or TypeCode.Int16
            or TypeCode.Int32
            or TypeCode.Int64
            or TypeCode.UInt16
            or TypeCode.UInt32
            or TypeCode.UInt64;
    }

    /// <summary>
    ///     检查类型是否是小数类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsDecimal(this Type type)
    {
        // 如果是浮点类型则直接返回
        if (type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float))
        {
            return true;
        }

        // 检查 TypeCode
        return Type.GetTypeCode(type) is TypeCode.Double or TypeCode.Decimal;
    }

    /// <summary>
    ///     检查类型是否是数值类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsNumeric(this Type type) =>
        type.IsInteger()
        || type.IsDecimal();

    /// <summary>
    ///     检查类型是否是 <see cref="KeyValuePair{TKey,TValue}" /> 类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsKeyValuePair(this Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);

    /// <summary>
    ///     检查类型是否是键值对集合类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="isKeyValuePairCollection">是否是 <see cref="KeyValuePair{TKey,TValue}" /> 集合类型</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsKeyValueCollection(this Type type, out bool isKeyValuePairCollection)
    {
        isKeyValuePairCollection = false;

        // 如果类型不是一个集合类型则直接返回
        if (!typeof(IEnumerable).IsAssignableFrom(type))
        {
            return false;
        }

        // 如果是 Hashtable 或 NameValueCollection 则直接返回
        if (type == typeof(Hashtable) || type == typeof(NameValueCollection))
        {
            return true;
        }

        // 如果是 IDictionary<,> 类型则直接返回
        if ((type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
        {
            isKeyValuePairCollection = type.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          ((i.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                            i.GetGenericArguments()[0].IsKeyValuePair()) ||
                           (i.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                            i.GetGenericArguments()[0].IsKeyValuePair())));
            return true;
        }

        // 检查是否是 KeyValuePair<,> 数组类型
        if (type.IsArray)
        {
            // 获取数组元素类型
            var elementType = type.GetElementType();

            // 检查元素类型是否是 KeyValuePair<,> 类型
            if (elementType is null || !elementType.IsKeyValuePair())
            {
                return false;
            }

            isKeyValuePairCollection = true;
            return true;
        }

        // 检查是否是 KeyValuePair<,> 集合类型
        if (type is not { IsGenericType: true, GenericTypeArguments.Length: 1 }
            || !type.GenericTypeArguments[0].IsKeyValuePair())
        {
            return false;
        }

        isKeyValuePairCollection = true;
        return true;
    }

    /// <summary>
    ///     获取 <see cref="KeyValuePair{TKey,TValue}" /> 或 <c>Newtonsoft.Json.Linq.JProperty</c> 类型键值属性值访问器
    /// </summary>
    /// <param name="keyValuePairType">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="Tuple{T1,T2}" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static (Func<object, object?> KeyGetter, Func<object, object?> ValueGetter)
        GetKeyValuePairOrJPropertyGetters(
            this Type keyValuePairType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(keyValuePairType);

        // 检查类型是否是 KeyValuePair<,> 类型或者是 Newtonsoft.Json.Linq.JProperty 类型
        if (keyValuePairType.IsKeyValuePair() || keyValuePairType.FullName == "Newtonsoft.Json.Linq.JProperty")
        {
            // 反射搜索成员方式
            const BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance;

            // 创建 Key/Name 和 Value 属性值访问器
            var keyGetter =
                keyValuePairType.CreatePropertyGetter(keyValuePairType.GetProperty("Key", bindingAttr) ??
                                                      keyValuePairType.GetProperty("Name", bindingAttr)!);
            var valueGetter =
                keyValuePairType.CreatePropertyGetter(keyValuePairType.GetProperty("Value",
                    bindingAttr)!);

            return (keyGetter, valueGetter);
        }

        throw new InvalidOperationException(
            $"The type `{keyValuePairType}` is not a `KeyValuePair<,>` or `Newtonsoft.Json.Linq.JProperty` type.");
    }

    /// <summary>
    ///     创建实例属性值设置器
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <remarks>不支持 <c>struct</c> 类型设置属性值。</remarks>
    /// <param name="propertyInfo">
    ///     <see cref="PropertyInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="Action{T1, T2}" />
    /// </returns>
    internal static Action<object, object?> CreatePropertySetter(this Type type, PropertyInfo propertyInfo)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(propertyInfo);

        // 检查类型是否是值类型
        if (type.IsValueType)
        {
            throw new NotSupportedException("Value types are not supported for setters.");
        }

        // 获取属性的设置方法，并允许非公开访问
        var setMethod = propertyInfo.GetSetMethod(true);

        // 空检查
        ArgumentNullException.ThrowIfNull(setMethod);

        // 检查属性是否是静态属性
        if (setMethod.IsStatic)
        {
            throw new ArgumentException("Property setter must be an instance method.", nameof(propertyInfo));
        }

        // 创建一个新的动态方法，并为其命名，命名格式为类型全名_设置_属性名
        var setterMethod = new DynamicMethod(
            $"{type.FullName}_Set_{propertyInfo.Name}",
            null,
            [typeof(object), typeof(object)],
            typeof(TypeExtensions).Module,
            true
        );

        // 获取动态方法的 IL 生成器
        var ilGenerator = setterMethod.GetILGenerator();

        // 将目标对象加载到堆栈上，并将其转换为所需的类型
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Castclass, type);

        // 将要分配的值加载到堆栈上
        ilGenerator.Emit(OpCodes.Ldarg_1);

        // 检查属性类型是否为值类型
        // 将值转换为属性类型
        // 对值进行拆箱，转换为适当的值类型
        ilGenerator.Emit(propertyInfo.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass,
            propertyInfo.PropertyType);

        // 在目标对象上调用设置方法
        ilGenerator.Emit(OpCodes.Callvirt, setMethod);

        // 从动态方法返回
        ilGenerator.Emit(OpCodes.Ret);

        // 创建一个委托并将其转换为适当的 Action 类型
        return (Action<object, object?>)setterMethod.CreateDelegate(typeof(Action<object, object?>));
    }

    /// <summary>
    ///     创建实例字段值设置器
    /// </summary>
    /// <remarks>不支持 <c>struct</c> 类型设置字段值</remarks>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="fieldInfo">
    ///     <see cref="FieldInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="Action{T1, T2}" />
    /// </returns>
    internal static Action<object, object?> CreateFieldSetter(this Type type, FieldInfo fieldInfo)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(fieldInfo);

        // 检查类型是否是值类型
        if (type.IsValueType)
        {
            throw new NotSupportedException("Value types are not supported for setters.");
        }

        // 检查字段是否是静态字段
        if (fieldInfo.IsStatic)
        {
            throw new ArgumentException("Field must be an instance field.", nameof(fieldInfo));
        }

        // 创建一个新的动态方法，并为其命名，命名格式为类型全名_设置_字段名
        var setterMethod = new DynamicMethod(
            $"{type.FullName}_Set_{fieldInfo.Name}",
            null,
            [typeof(object), typeof(object)],
            typeof(TypeExtensions).Module,
            true
        );

        // 获取动态方法的 IL 生成器
        var ilGenerator = setterMethod.GetILGenerator();

        // 将目标对象加载到堆栈上，并将其转换为所需的类型
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Castclass, type);

        // 将要分配的值加载到堆栈上
        ilGenerator.Emit(OpCodes.Ldarg_1);

        // 检查字段类型是否为值类型
        // 将值转换为字段类型
        // 对值进行拆箱，转换为适当的值类型
        ilGenerator.Emit(fieldInfo.FieldType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, fieldInfo.FieldType);

        // 将堆栈上的值存储到字段中
        ilGenerator.Emit(OpCodes.Stfld, fieldInfo);

        // 从动态方法返回
        ilGenerator.Emit(OpCodes.Ret);

        // 创建一个委托并将其转换为适当的 Action 类型
        return (Action<object, object?>)setterMethod.CreateDelegate(typeof(Action<object, object?>));
    }

    /// <summary>
    ///     创建实例属性值访问器
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="propertyInfo">
    ///     <see cref="PropertyInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="Func{T1, T2}" />
    /// </returns>
    internal static Func<object, object?> CreatePropertyGetter(this Type type, PropertyInfo propertyInfo)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(propertyInfo);

        // 获取属性的获取方法，并允许非公开访问
        var getMethod = propertyInfo.GetGetMethod(true);

        // 空检查
        ArgumentNullException.ThrowIfNull(getMethod);

        // 检查属性是否是静态属性
        if (getMethod.IsStatic)
        {
            throw new ArgumentException("Property getter must be an instance method.", nameof(propertyInfo));
        }

        // 创建一个新的动态方法，并为其命名，命名格式为类型全名_获取_属性名
        var dynamicMethod = new DynamicMethod(
            $"{type.FullName}_Get_{propertyInfo.Name}",
            typeof(object),
            [typeof(object)],
            typeof(TypeExtensions).Module,
            true
        );

        // 获取动态方法的 IL 生成器
        var ilGenerator = dynamicMethod.GetILGenerator();

        // 将目标对象加载到堆栈上
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(type.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, type);

        // 调用获取方法
        ilGenerator.EmitCall(OpCodes.Callvirt, getMethod, null);

        // 如果属性类型为值类型，则装箱为 object 类型
        if (propertyInfo.PropertyType.IsValueType)
        {
            ilGenerator.Emit(OpCodes.Box, propertyInfo.PropertyType);
        }

        // 从动态方法返回
        ilGenerator.Emit(OpCodes.Ret);

        // 创建一个委托并将其转换为适当的 Func 类型
        return (Func<object, object?>)dynamicMethod.CreateDelegate(typeof(Func<object, object?>));
    }

    /// <summary>
    ///     创建静态属性值访问器
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="propertyInfo">
    ///     <see cref="PropertyInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="Func{T1}" />
    /// </returns>
    internal static Func<object?> CreateStaticPropertyGetter(this Type type, PropertyInfo propertyInfo)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(propertyInfo);

        // 获取属性的获取方法，并允许非公开访问
        var getMethod = propertyInfo.GetGetMethod(true);

        // 空检查
        ArgumentNullException.ThrowIfNull(getMethod);

        // 检查属性是否是非静态属性
        if (!getMethod.IsStatic)
        {
            throw new ArgumentException("Property getter must be static.", nameof(propertyInfo));
        }

        // 创建一个新的动态方法，并为其命名，命名格式为类型全名_获取_属性名
        var dynamicMethod = new DynamicMethod(
            $"{type.FullName}_Get_{propertyInfo.Name}",
            typeof(object),
            Type.EmptyTypes, // 没有参数
            typeof(TypeExtensions).Module,
            true
        );

        // 获取动态方法的 IL 生成器
        var ilGenerator = dynamicMethod.GetILGenerator();

        // 调用静态属性的获取方法
        ilGenerator.EmitCall(OpCodes.Call, getMethod, null);

        // 如果属性类型为值类型，则装箱为 object 类型
        if (propertyInfo.PropertyType.IsValueType)
        {
            ilGenerator.Emit(OpCodes.Box, propertyInfo.PropertyType);
        }

        // 从动态方法返回
        ilGenerator.Emit(OpCodes.Ret);

        // 创建一个委托并将其转换为适当的 Func 类型
        return (Func<object?>)dynamicMethod.CreateDelegate(typeof(Func<object?>));
    }

    /// <summary>
    ///     创建实例字段值访问器
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="fieldInfo">
    ///     <see cref="FieldInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="Func{T1, T2}" />
    /// </returns>
    internal static Func<object, object?> CreateFieldGetter(this Type type, FieldInfo fieldInfo)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(fieldInfo);

        // 检查字段是否是静态字段
        if (fieldInfo.IsStatic)
        {
            throw new ArgumentException("Field must be an instance field.", nameof(fieldInfo));
        }

        // 创建一个新的动态方法，并为其命名，命名格式为类型全名_获取_字段名
        var dynamicMethod = new DynamicMethod(
            $"{type.FullName}_Get_{fieldInfo.Name}",
            typeof(object),
            [typeof(object)],
            typeof(TypeExtensions).Module,
            true
        );

        // 获取动态方法的 IL 生成器
        var ilGenerator = dynamicMethod.GetILGenerator();

        // 将目标对象加载到堆栈上，并将其转换为字段的声明类型
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(type.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, type);

        // 加载字段的值到堆栈上
        ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);

        // 如果字段类型为值类型，则装箱为 object 类型
        if (fieldInfo.FieldType.IsValueType)
        {
            ilGenerator.Emit(OpCodes.Box, fieldInfo.FieldType);
        }

        // 从动态方法返回
        ilGenerator.Emit(OpCodes.Ret);

        // 创建一个委托并将其转换为适当的 Func 类型
        return (Func<object, object?>)dynamicMethod.CreateDelegate(typeof(Func<object, object?>));
    }

    /// <summary>
    ///     创建静态字段值访问器
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <param name="fieldInfo">
    ///     <see cref="FieldInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="Func{T1}" />
    /// </returns>
    internal static Func<object?> CreateStaticFieldGetter(this Type type, FieldInfo fieldInfo)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(fieldInfo);

        // 检查字段是否是非静态字段
        if (!fieldInfo.IsStatic)
        {
            throw new ArgumentException("Field must be static.", nameof(fieldInfo));
        }

        // 创建一个新的动态方法，并为其命名，命名格式为类型全名_获取_字段名
        var dynamicMethod = new DynamicMethod(
            $"{type.FullName}_Get_{fieldInfo.Name}",
            typeof(object),
            Type.EmptyTypes,
            typeof(TypeExtensions).Module,
            true
        );

        // 获取动态方法的 IL 生成器
        var ilGenerator = dynamicMethod.GetILGenerator();

        // 加载静态字段的值到堆栈上
        ilGenerator.Emit(OpCodes.Ldsfld, fieldInfo);

        // 如果字段类型为值类型，则装箱为 object 类型
        if (fieldInfo.FieldType.IsValueType)
        {
            ilGenerator.Emit(OpCodes.Box, fieldInfo.FieldType);
        }

        // 从动态方法返回
        ilGenerator.Emit(OpCodes.Ret);

        // 创建一个委托并将其转换为适当的 Func 类型
        return (Func<object?>)dynamicMethod.CreateDelegate(typeof(Func<object?>));
    }

    /// <summary>
    ///     输出类型的友好字符串
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ToFriendlyString(this Type? type)
    {
        // 空检查
        if (type is null)
        {
            return null;
        }

        // 检查是否是泛型类型
        if (type.IsGenericType)
        {
            // 获取类型名称
            var typeName = type.Name.Split('`')[0];

            // 获取泛型参数
            var genericArguments = type.GetGenericArguments().Select(ToFriendlyString).ToArray();

            return $"{type.Namespace}.{typeName}<{string.Join(',', genericArguments)}>";
        }

        // 检查是否是非泛型且不是数组类型
        // ReSharper disable once InvertIf
        if (type.IsArray)
        {
            var rank = new string(',', type.GetArrayRank() - 1);
            return $"{type.GetElementType()!.ToFriendlyString()}[{rank}]";
        }

        return type.FullName ?? type.Name;
    }
}