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

using Furion.Extensions;
using Furion.HttpRemote.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Furion.HttpRemote;

/// <inheritdoc cref="IHttpContentConverterFactory" />
internal sealed class HttpContentConverterFactory : IHttpContentConverterFactory
{
    /// <summary>
    ///     <see cref="IHttpContentConverter{TResult}" /> 字典集合
    /// </summary>
    internal readonly Dictionary<Type, IHttpContentConverter> _converters;

    /// <summary>
    ///     泛型 <see cref="IHttpContentConverter" /> 响应内容转换器字典缓存
    /// </summary>
    internal readonly ConcurrentDictionary<Type, IHttpContentConverter?> _genericConverterCache = new();

    /// <summary>
    ///     泛型 <see cref="IHttpContentConverter" /> 工厂委托字典集合
    /// </summary>
    internal readonly Dictionary<Type, List<Func<Type[], IHttpContentConverter>>> _genericConverters;

    /// <inheritdoc cref="IHttpRemoteLogger" />
    internal readonly IHttpRemoteLogger _logger;

    /// <summary>
    ///     <inheritdoc cref="HttpContentConverterFactory" />
    /// </summary>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <param name="logger">
    ///     <see cref="IHttpRemoteLogger" />
    /// </param>
    /// <param name="converters"><see cref="IHttpContentConverter{TResult}" /> 集合</param>
    /// <param name="genericConverters"><see cref="GenericHttpContentConverter" /> 集合</param>
    public HttpContentConverterFactory(IServiceProvider serviceProvider, IHttpRemoteLogger logger,
        IEnumerable<IHttpContentConverter> converters, IEnumerable<GenericHttpContentConverter> genericConverters)
    {
        ServiceProvider = serviceProvider;
        _logger = logger;

        // 初始化响应内容转换器
        _converters = new Dictionary<Type, IHttpContentConverter>
        {
            [typeof(HttpResponseMessageConverter)] = new HttpResponseMessageConverter(),
            [typeof(StringContentConverter)] = new StringContentConverter(),
            [typeof(ByteArrayContentConverter)] = new ByteArrayContentConverter(),
            [typeof(StreamContentConverter)] = new StreamContentConverter(),
            [typeof(VoidContentConverter)] = new VoidContentConverter()
        };

        // 添加自定义 IHttpContentConverter 集合
        _converters.TryAdd(converters, value => value.GetType());

        // 初始化泛型响应内容转换器工厂委托
        _genericConverters = new Dictionary<Type, List<Func<Type[], IHttpContentConverter>>>
        {
            [typeof(IAsyncEnumerable<>)] =
            [
                typeArgs => (IHttpContentConverter)Activator.CreateInstance(
                    typeof(AsyncEnumerableContentConverter<>).MakeGenericType(typeArgs[0]))!
            ],
            [typeof(HttpRemoteResultBase<>)] =
            [
                typeArgs => (IHttpContentConverter)Activator.CreateInstance(
                    typeof(HttpRemoteResultBaseContentConverter<>).MakeGenericType(typeArgs[0]))!
            ],
            [typeof(HttpRemoteResult<>)] =
            [
                typeArgs => (IHttpContentConverter)Activator.CreateInstance(
                    typeof(HttpRemoteResultContentConverter<>).MakeGenericType(typeArgs[0]))!
            ]
        };

        // 添加自定义泛型响应内容转换器工厂委托集合
        foreach (var (genericType, factory) in genericConverters ?? [])
        {
            // 尝试根据泛型类型查找工作委托集合
            if (!_genericConverters.TryGetValue(genericType, out var factories))
            {
                factories = [];
                _genericConverters[genericType] = factories;
            }

            factories.Add(factory);
        }
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public HttpContentConverterResult<TResult> Read<TResult>(HttpContentConverterContext context,
        CancellationToken cancellationToken = default)
    {
        // 获取 HttpResponseMessage 实例
        var httpResponseMessage = context.ResponseMessage;

        try
        {
            // 根据目标类型获取对应的 IHttpContentConverter<TResult> 实例
            var httpContentConverter = GetConverter<TResult>(context);

            // 将 HttpResponseMessage 转换为 TResult 实例
            var result = httpContentConverter.Read(context, cancellationToken);

            return new HttpContentConverterResult<TResult>(httpResponseMessage, result, httpContentConverter);
        }
        catch (Exception e)
        {
            // 输出转换异常日志
            LogContentConversionError(typeof(TResult), httpResponseMessage, e);

            throw;
        }
    }

    /// <inheritdoc />
    public HttpContentConverterResult Read(Type resultType, HttpContentConverterContext context,
        CancellationToken cancellationToken = default)
    {
        // 获取 HttpResponseMessage 实例
        var httpResponseMessage = context.ResponseMessage;

        try
        {
            // 根据目标类型获取对应的 IHttpContentConverter 实例
            var httpContentConverter = GetConverter(resultType, context);

            // 将 HttpResponseMessage 转换为 resultType 实例
            var result = httpContentConverter.Read(resultType, context, cancellationToken);

            return new HttpContentConverterResult(httpResponseMessage, result, httpContentConverter);
        }
        catch (Exception e)
        {
            // 输出转换异常日志
            LogContentConversionError(resultType, httpResponseMessage, e);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<HttpContentConverterResult<TResult>> ReadAsync<TResult>(HttpContentConverterContext context,
        CancellationToken cancellationToken = default)
    {
        // 获取 HttpResponseMessage 实例
        var httpResponseMessage = context.ResponseMessage;

        try
        {
            // 根据目标类型获取对应的 IHttpContentConverter<TResult> 实例
            var httpContentConverter = GetConverter<TResult>(context);

            // 将 HttpResponseMessage 转换为 TResult 实例
            var result = await httpContentConverter.ReadAsync(context, cancellationToken);

            return new HttpContentConverterResult<TResult>(httpResponseMessage, result, httpContentConverter);
        }
        catch (Exception e)
        {
            // 输出转换异常日志
            LogContentConversionError(typeof(TResult), httpResponseMessage, e);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<HttpContentConverterResult> ReadAsync(Type resultType, HttpContentConverterContext context,
        CancellationToken cancellationToken = default)
    {
        // 获取 HttpResponseMessage 实例
        var httpResponseMessage = context.ResponseMessage;

        try
        {
            // 根据目标类型获取对应的 IHttpContentConverter 实例
            var httpContentConverter = GetConverter(resultType, context);

            // 将 HttpResponseMessage 转换为 resultType 实例
            var result = await httpContentConverter.ReadAsync(resultType, context, cancellationToken);

            return new HttpContentConverterResult(httpResponseMessage, result, httpContentConverter);
        }
        catch (Exception e)
        {
            // 输出转换异常日志
            LogContentConversionError(resultType, httpResponseMessage, e);

            throw;
        }
    }

    /// <inheritdoc />
    public IHttpContentConverter<TResult> GetConverter<TResult>(HttpContentConverterContext context)
    {
        // 初始化新的 IHttpContentConverter 字典集合
        var unionConverters = new Dictionary<Type, IHttpContentConverter>(_converters);

        // 添加自定义 IHttpContentConverter 数组
        unionConverters.TryAdd(context.Converters, value => value.GetType());

        // 初始化目标类型响应内容转换器
        IHttpContentConverter<TResult> targetConverter;

        // 检查是否启用 JSON 响应反序列化包装器或目标转换类型为 void/VoidContent
        if (context.ResponseMessage.ShouldUseJsonResponseWrapper(ServiceProvider) &&
            !(typeof(TResult) == typeof(void) || typeof(VoidContent).IsAssignableFrom(typeof(TResult))))
        {
            // 调用 IObjectContentConverterFactory 实例的 GetConverter<TResult> 返回
            targetConverter = ServiceProvider.GetRequiredService<IObjectContentConverterFactory>()
                .GetConverter<TResult>(context);
        }
        else
        {
            // 查找可以处理目标类型的响应内容转换器
            var typeConverter = unionConverters.Values.OfType<IHttpContentConverter<TResult>>().LastOrDefault();

            // 空检查
            if (typeConverter is not null)
            {
                targetConverter = typeConverter;
            }
            else
            {
                // 尝试解析泛型 IHttpContentConverter<TResult> 内容转换器
                targetConverter = TryResolveGenericConverter(typeof(TResult)) as IHttpContentConverter<TResult> ??
                                  // 如果未找到，则调用 IObjectContentConverterFactory 实例的 GetConverter<TResult> 返回
                                  ServiceProvider.GetRequiredService<IObjectContentConverterFactory>()
                                      .GetConverter<TResult>(context);
            }
        }

        // 设置服务提供器
        targetConverter.ServiceProvider ??= ServiceProvider;

        return targetConverter;
    }

    /// <inheritdoc />
    public IHttpContentConverter GetConverter(Type resultType, HttpContentConverterContext context)
    {
        // 初始化新的 IHttpContentConverter 字典集合
        var unionConverters = new Dictionary<Type, IHttpContentConverter>(_converters);

        // 添加自定义 IHttpContentConverter 数组
        unionConverters.TryAdd(context.Converters, value => value.GetType());

        // 初始化目标类型响应内容转换器
        IHttpContentConverter targetConverter;

        // 检查是否启用 JSON 响应反序列化包装器或目标转换类型为 void/VoidContent
        if (context.ResponseMessage.ShouldUseJsonResponseWrapper(ServiceProvider) &&
            !(resultType == typeof(void) || typeof(VoidContent).IsAssignableFrom(resultType)))
        {
            // 调用 IObjectContentConverterFactory 实例的 GetConverter 返回
            targetConverter = ServiceProvider.GetRequiredService<IObjectContentConverterFactory>()
                .GetConverter(resultType, context);
        }
        else
        {
            // 初始化 IHttpContentConverter<TResult> 类型
            var exactType = typeof(IHttpContentConverter<>).MakeGenericType(resultType);

            // 查找可以处理目标类型的响应内容转换器
            var typeConverter = unionConverters.Values.Where(exactType.IsInstanceOfType).LastOrDefault();

            // 空检查
            if (typeConverter is not null)
            {
                targetConverter = typeConverter;
            }
            else
            {
                // 尝试解析泛型 IHttpContentConverter<TResult> 泛型内容转换器
                targetConverter = TryResolveGenericConverter(resultType) ??
                                  // 如果未找到，则调用 IObjectContentConverterFactory 实例的 GetConverter 返回
                                  ServiceProvider.GetRequiredService<IObjectContentConverterFactory>()
                                      .GetConverter(resultType, context);
            }
        }

        // 设置服务提供器
        targetConverter.ServiceProvider ??= ServiceProvider;

        return targetConverter;
    }

    /// <summary>
    ///     输出转换异常日志
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="exception">
    ///     <see cref="Exception" />
    /// </param>
    internal void LogContentConversionError(Type resultType, HttpResponseMessage httpResponseMessage,
        Exception exception) =>
        _logger.LogError(exception,
            "Failed to convert HTTP response to type \e[31m\e[1m{ResultType}\e[0m. Status: {StatusCode} {StatusDescription}, URI: {RequestUri}",
            resultType.FullName!, (int)httpResponseMessage.StatusCode, httpResponseMessage.StatusCode.ToString(),
            httpResponseMessage.RequestMessage?.RequestUri?.ToString() ?? "unknown");

    /// <summary>
    ///     尝试解析泛型 <see cref="IHttpContentConverter{TResult}" /> 实例
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <returns>
    ///     <see cref="IHttpContentConverter" />
    /// </returns>
    internal IHttpContentConverter? TryResolveGenericConverter(Type resultType)
    {
        // 检查类型是否是泛型类型
        if (!resultType.IsGenericType)
        {
            return null;
        }

        // 查找泛型 IHttpContentConverter 响应内容转换器字典缓存是否存在该类型
        return _genericConverterCache.GetOrAdd(resultType, type =>
        {
            // 获取泛型定义类型
            var genericTypeDefinition = type.GetGenericTypeDefinition();

            // 查找是否注册了泛型内容转换器工厂委托
            return !_genericConverters.TryGetValue(genericTypeDefinition, out var factories)
                ? null
                : factories.LastOrDefault()?.Invoke(type.GetGenericArguments());
        });
    }
}