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

namespace Furion.HttpRemote;

/// <inheritdoc cref="IHttpContentProcessorFactory" />
internal sealed class HttpContentProcessorFactory : IHttpContentProcessorFactory
{
    /// <summary>
    ///     <see cref="IHttpContentProcessor" /> 字典集合
    /// </summary>
    internal readonly Dictionary<Type, IHttpContentProcessor> _processors;

    /// <summary>
    ///     <inheritdoc cref="HttpContentProcessorFactory" />
    /// </summary>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <param name="processors"><see cref="IHttpContentProcessor" /> 集合</param>
    public HttpContentProcessorFactory(IServiceProvider serviceProvider, IEnumerable<IHttpContentProcessor> processors)
    {
        ServiceProvider = serviceProvider;

        // 初始化请求内容处理器
        _processors = new Dictionary<Type, IHttpContentProcessor>
        {
            [typeof(StringContentProcessor)] = new StringContentProcessor(),
            [typeof(FormUrlEncodedContentProcessor)] = new FormUrlEncodedContentProcessor(),
            [typeof(ByteArrayContentProcessor)] = new ByteArrayContentProcessor(),
            [typeof(StreamContentProcessor)] = new StreamContentProcessor(),
            [typeof(MultipartFormDataContentProcessor)] = new MultipartFormDataContentProcessor(),
            [typeof(ReadOnlyMemoryContentProcessor)] = new ReadOnlyMemoryContentProcessor(),
            [typeof(JsonLinesContentProcessor)] = new JsonLinesContentProcessor(),
            [typeof(FileInfoContentProcessor)] = new FileInfoContentProcessor()
        };

        // 添加自定义 IHttpContentProcessor 集合
        _processors.TryAdd(processors, value => value.GetType());
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public HttpContent? Build(HttpContentProcessorContext context, params IHttpContentProcessor[]? processors)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(context);

        // 查找可以处理指定内容类型或数据类型的 IHttpContentProcessor 实例
        var httpContentProcessor = GetProcessor(context, processors);

        // 将原始请求内容转换为 HttpContent 实例
        return httpContentProcessor.Process(context);
    }

    /// <summary>
    ///     查找可以处理指定内容类型或数据类型的 <see cref="IHttpContentProcessor" /> 实例
    /// </summary>
    /// <param name="context">
    ///     <see cref="HttpContentProcessorContext" />
    /// </param>
    /// <param name="processors">自定义 <see cref="IHttpContentProcessor" /> 数组</param>
    /// <returns>
    ///     <see cref="IHttpContentProcessor" />
    /// </returns>
    internal IHttpContentProcessor GetProcessor(HttpContentProcessorContext context,
        params IHttpContentProcessor[]? processors)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(context);

        // 初始化新的 IHttpContentProcessor 字典集合
        var unionProcessors = new Dictionary<Type, IHttpContentProcessor>(_processors);

        // 添加自定义 IHttpContentProcessor 数组
        unionProcessors.TryAdd(processors, value => value.GetType());

        // 查找可以处理指定内容类型或数据类型的 IHttpContentProcessor 实例
        var processor = unionProcessors.Values.LastOrDefault(u => u.CanProcess(context)) ??
                        throw new InvalidOperationException(
                            $"No processor found that can handle the content type `{context.ContentType}` and the provided raw content of type `{context.RawContent?.GetType()}`. " +
                            "Please ensure that the correct content type is specified and that a suitable processor is registered.");

        // 设置服务提供器
        processor.ServiceProvider ??= ServiceProvider;

        return processor;
    }
}