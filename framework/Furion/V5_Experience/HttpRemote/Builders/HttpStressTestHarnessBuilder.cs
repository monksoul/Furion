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

namespace Furion.HttpRemote;

/// <summary>
///     压力测试构建器
/// </summary>
/// <remarks>使用 <c>HttpRequestBuilder.StressTestHarness(requestUri, numberOfRequests)</c> 静态方法创建。</remarks>
public sealed class HttpStressTestHarnessBuilder
{
    /// <summary>
    ///     <see cref="HttpRequestBuilder" /> 配置委托
    /// </summary>
    internal Action<HttpRequestBuilder>? _configureRequest;

    /// <summary>
    ///     <inheritdoc cref="HttpStressTestHarnessBuilder" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpStressTestHarnessBuilder(HttpMethod httpMethod, Uri? requestUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);

        HttpMethod = httpMethod;
        RequestUri = requestUri;
    }

    /// <summary>
    ///     请求地址
    /// </summary>
    public Uri? RequestUri { get; }

    /// <summary>
    ///     请求方式
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    ///     并发请求数量
    /// </summary>
    /// <remarks>默认值为：100。</remarks>
    public int NumberOfRequests { get; private set; } = 100;

    /// <summary>
    ///     最大并发度
    /// </summary>
    /// <remarks>用于控制系统在同一时间内处理的请求数量。默认值为：100。</remarks>
    public int MaxDegreeOfParallelism { get; private set; } = 100;

    /// <summary>
    ///     压测轮次
    /// </summary>
    /// <remarks>默认值为：1。</remarks>
    public int NumberOfRounds { get; private set; } = 1;

    /// <summary>
    ///     设置并发请求数量
    /// </summary>
    /// <param name="numberOfRequests">并发请求数量</param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpStressTestHarnessBuilder SetNumberOfRequests(int numberOfRequests)
    {
        // 小于或等于 0 检查
        if (numberOfRequests <= 0)
        {
            throw new ArgumentException("Number of requests must be greater than 0.", nameof(numberOfRequests));
        }

        NumberOfRequests = numberOfRequests;

        return this;
    }

    /// <summary>
    ///     设置最大并发度
    /// </summary>
    /// <param name="maxDegreeOfParallelism">最大并发度</param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpStressTestHarnessBuilder SetMaxDegreeOfParallelism(int maxDegreeOfParallelism)
    {
        // 小于或等于 0 检查
        if (maxDegreeOfParallelism <= 0)
        {
            throw new ArgumentException("Max degree of parallelism must be greater than 0.",
                nameof(maxDegreeOfParallelism));
        }

        MaxDegreeOfParallelism = maxDegreeOfParallelism;

        return this;
    }

    /// <summary>
    ///     设置压测轮次
    /// </summary>
    /// <param name="numberOfRounds">压测轮次</param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpStressTestHarnessBuilder SetNumberOfRounds(int numberOfRounds)
    {
        // 小于或等于 0 检查
        if (numberOfRounds <= 0)
        {
            throw new ArgumentException("Number of rounds must be greater than 0.",
                nameof(numberOfRounds));
        }

        NumberOfRounds = numberOfRounds;

        return this;
    }

    /// <summary>
    ///     配置 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法。
    /// </param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpStressTestHarnessBuilder With(Action<HttpRequestBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        _configureRequest += configure;

        return this;
    }

    /// <summary>
    ///     设置禁用 HTTP 缓存
    /// </summary>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    public HttpStressTestHarnessBuilder DisableCache() => With(builder => builder.DisableCache());

    /// <summary>
    ///     设置禁用 HTTP 缓存
    /// </summary>
    /// <param name="disabled">是否禁用</param>
    /// <returns>
    ///     <see cref="HttpStressTestHarnessBuilder" />
    /// </returns>
    public HttpStressTestHarnessBuilder DisableCache(bool disabled) =>
        With(builder => builder.DisableCache(disabled));

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal HttpRequestBuilder Build(HttpRemoteOptions httpRemoteOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);

        // 初始化 HttpRequestBuilder 实例，并确保请求标头中添加了 X-Stress-Test: Harness；
        // 同时禁用请求分析工具和启用 HttpClient 池化管理
        var httpRequestBuilder = HttpRequestBuilder.Create(HttpMethod, RequestUri)
            .WithHeader(Constants.X_STRESS_TEST_HEADER, Constants.X_STRESS_TEST_VALUE, replace: true).Profiler(false)
            .PerformanceOptimization().UseHttpClientPool();

        // 调用自定义配置委托
        _configureRequest?.Invoke(httpRequestBuilder);

        return httpRequestBuilder;
    }
}