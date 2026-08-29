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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 声明式提取器上下文
/// </summary>
public sealed class HttpDeclarativeParsingContext : IServiceProvider
{
    /// <summary>
    ///     冻结参数类型集合
    /// </summary>
    /// <remarks>此类参数类型不应作为外部提取对象。</remarks>
    internal static readonly HashSet<Type> _frozenParameterTypes =
    [
        typeof(Action<HttpRequestBuilder>), typeof(Action<HttpMultipartFormDataBuilder>), typeof(HttpCompletionOption),
        typeof(CancellationToken)
    ];

    /// <summary>
    ///     <see cref="IServiceProvider" /> 委托
    /// </summary>
    internal Func<Type, object?>? _serviceProvider;

    /// <summary>
    ///     <inheritdoc cref="HttpDeclarativeParsingContext" />
    /// </summary>
    /// <param name="method">被调用方法</param>
    /// <param name="args">被调用方法的参数值数组</param>
    /// <param name="metadata">
    ///     <see cref="HttpDeclarativeMetadata" />
    /// </param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    internal HttpDeclarativeParsingContext(MethodInfo method, object?[] args,
        HttpDeclarativeMetadata metadata, IServiceProvider? serviceProvider = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(metadata);

        // 空检查
        if (serviceProvider is not null)
        {
            var localServiceProvider = serviceProvider;
            InitializeServiceProvider(localServiceProvider.GetService);
        }

        Method = method;
        Args = args;
        Metadata = metadata;

        // 初始化被调用方法的参数键值字典
        Parameters = method.GetParameters().Select((p, i) => new { Parameter = p, Value = args[i] })
            .ToDictionary(u => u.Parameter, u => u.Value).AsReadOnly();

        // 初始化被调用方法的非冻结类型参数键值字典
        UnFrozenParameters = Parameters.Where(u => !IsFrozenParameter(u.Key)).ToDictionary(u => u.Key, u => u.Value)
            .AsReadOnly();
    }

    /// <summary>
    ///     被调用方法
    /// </summary>
    public MethodInfo Method { get; }

    /// <summary>
    ///     被调用方法的参数值数组
    /// </summary>
    public object?[] Args { get; }

    /// <inheritdoc cref="HttpDeclarativeMetadata" />
    public HttpDeclarativeMetadata Metadata { get; }

    /// <summary>
    ///     被调用方法的参数键值字典
    /// </summary>
    public IReadOnlyDictionary<ParameterInfo, object?> Parameters { get; }

    /// <summary>
    ///     被调用方法的非冻结类型参数键值字典
    /// </summary>
    public IReadOnlyDictionary<ParameterInfo, object?> UnFrozenParameters { get; }

    /// <inheritdoc />
    public object? GetService(Type serviceType) => _serviceProvider?.Invoke(serviceType);

    /// <summary>
    ///     判断参数是否为冻结参数
    /// </summary>
    /// <remarks>此类参数不应作为外部提取对象。</remarks>
    /// <param name="parameter">
    ///     <see cref="ParameterInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool IsFrozenParameter(ParameterInfo parameter)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameter);

        return _frozenParameterTypes.Contains(parameter.ParameterType);
    }

    /// <summary>
    ///     检查被调用方法是否定义了指定特性
    /// </summary>
    /// <param name="attribute">
    ///     <typeparamref name="TAttribute" />
    /// </param>
    /// <param name="inherit">是否在基类或被代理的接口中搜索</param>
    /// <typeparam name="TAttribute">
    ///     <see cref="Attribute" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool IsMethodDefined<TAttribute>([NotNullWhen(true)] out TAttribute? attribute, bool inherit = false)
        where TAttribute : Attribute
    {
        // 获取指定特性实例
        attribute = Metadata.MethodAttributes?.OfType<TAttribute>().FirstOrDefault();

        // 检查是否定义了指定特性
        var isDefined = attribute is not null;
        if (isDefined || !inherit)
        {
            return isDefined;
        }

        // 尝试查找所在声明类或被代理的接口是否定义了指定特性
        attribute = Metadata.InterfaceAttributes?.OfType<TAttribute>().FirstOrDefault();
        isDefined = attribute is not null;

        return isDefined;
    }

    /// <summary>
    ///     获取被调用方法指定特性的所有实例
    /// </summary>
    /// <param name="inherit">是否在基类或被代理的接口中搜索</param>
    /// <param name="methodScanFirst">是否优先查找 <see cref="MethodInfo" /> 的特性。默认值为：<c>true</c></param>
    /// <typeparam name="TAttribute">
    ///     <see cref="Attribute" />
    /// </typeparam>
    /// <returns>
    ///     <typeparamref name="TAttribute" /><c>[]</c>
    /// </returns>
    public TAttribute[]? GetMethodDefinedCustomAttributes<TAttribute>(bool inherit = false, bool methodScanFirst = true)
        where TAttribute : Attribute
    {
        // 初始化指定特性集合
        var attributes = new List<TAttribute>();

        // 获取指定特性集合
        attributes.AddRange(Metadata.MethodAttributes?.OfType<TAttribute>() ?? []);

        // 尝试获取所在声明类上指定特性集合
        // ReSharper disable once InvertIf
        if (inherit && Metadata.InterfaceAttributes is not null)
        {
            var declaringAttributes = Metadata.InterfaceAttributes.OfType<TAttribute>();

            // 是否优先查找方法特性
            if (methodScanFirst)
            {
                attributes.AddRange(declaringAttributes);
            }
            // 否则添加到头部
            else
            {
                attributes.InsertRange(0, declaringAttributes);
            }
        }

        return attributes.Count > 0 ? attributes.ToArray() : null;
    }

    /// <summary>
    ///     初始化 <see cref="IServiceProvider" />
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider" /> 委托</param>
    internal void InitializeServiceProvider(Func<Type, object?> serviceProvider) => _serviceProvider = serviceProvider;
}