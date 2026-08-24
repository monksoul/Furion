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

using System.Text;
using System.Text.Json;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpRequestBuilder" /> 的预配置抽象基类
/// </summary>
/// <typeparam name="THttpBuilder">派生构建器自身类型</typeparam>
public abstract class HttpRequestBuilderConfigurator<THttpBuilder>
    where THttpBuilder : HttpRequestBuilderConfigurator<THttpBuilder>
{
    /// <summary>
    ///     <see cref="HttpRequestBuilder" /> 配置委托
    /// </summary>
    internal Action<HttpRequestBuilder>? _configureRequest;

    /// <summary>
    ///     派生构建器自身引用
    /// </summary>
    public THttpBuilder This => (THttpBuilder)this;

    /// <summary>
    ///     <see cref="HttpRequestBuilder" /> 配置委托
    /// </summary>
    public Action<HttpRequestBuilder>? Configure => _configureRequest;

    /// <summary>
    ///     配置 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">
    ///     自定义配置委托；可直接传入 <c>HttpRequestBuilder.Setup</c>（或 <c>HttpBuilder.Setup</c>）的链式配置结果，替代 <![CDATA[builder => builder]]> 写法
    /// </param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual THttpBuilder With(Action<HttpRequestBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        _configureRequest += configure;

        return This;
    }

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder Profiler() => With(builder => builder.Profiler(true));

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder Profiler(bool enabled) => With(builder => builder.Profiler(enabled));

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <param name="predicate">自定义处理委托</param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder Profiler(Action<HttpRemoteAnalyzer> predicate) =>
        With(builder => builder.Profiler(predicate));

    /// <summary>
    ///     设置禁用 HTTP 缓存
    /// </summary>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder DisableCache() => With(builder => builder.DisableCache());

    /// <summary>
    ///     设置禁用 HTTP 缓存
    /// </summary>
    /// <param name="disabled">是否禁用</param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder DisableCache(bool disabled) =>
        With(builder => builder.DisableCache(disabled));

    /// <summary>
    ///     设置 Bearer 身份认证凭据请求授权标头
    /// </summary>
    /// <param name="token">令牌</param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder AddBearerAuthentication(string token) =>
        With(builder => builder.AddBearerAuthentication(token));

    /// <summary>
    ///     设置 Bearer 身份认证凭据请求授权标头
    /// </summary>
    /// <param name="headerName">自定义标头</param>
    /// <param name="token">令牌</param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder AddBearerAuthentication(string headerName, string token) =>
        With(builder => builder.AddBearerAuthentication(headerName, token));

    /// <summary>
    ///     设置 JSON 内容
    /// </summary>
    /// <param name="rawJson">JSON 字符串/原始对象</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder SetJsonContent(object? rawJson, Encoding? contentEncoding = null,
        string? contentType = null, JsonSerializerOptions? jsonSerializerOptions = null) =>
        With(builder => builder.SetJsonContent(rawJson, contentEncoding, contentType, jsonSerializerOptions));

    /// <summary>
    ///     设置 JSON 内容（宽松模式）
    /// </summary>
    /// <remarks>跳过对 JSON 字符串校验。</remarks>
    /// <param name="rawJson">JSON 字符串</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder SetJsonContentWithoutValidation(string? rawJson,
        Encoding? contentEncoding = null, string? contentType = null) =>
        With(builder => builder.SetJsonContentWithoutValidation(rawJson, contentEncoding, contentType));

    /// <summary>
    ///     设置请求内容
    /// </summary>
    /// <param name="rawContent">原始请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="disposeResourcesOnRequestCompletion">是否在请求结束后自动释放资源。默认值为：<c>false</c></param>
    /// <returns>
    ///     <typeparamref name="THttpBuilder" />
    /// </returns>
    public virtual THttpBuilder SetContent(object? rawContent, string? contentType = null,
        Encoding? contentEncoding = null, bool disposeResourcesOnRequestCompletion = false) =>
        With(builder =>
            builder.SetContent(rawContent, contentType, contentEncoding, disposeResourcesOnRequestCompletion));
}