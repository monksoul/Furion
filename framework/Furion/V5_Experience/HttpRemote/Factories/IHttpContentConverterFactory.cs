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
///     <see cref="IHttpContentConverter{TResult}" /> 工厂
/// </summary>
public interface IHttpContentConverterFactory
{
    /// <summary>
    ///     <inheritdoc cref="IServiceProvider" />
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     将 <see cref="HttpResponseMessage" /> 转换为
    ///     <typeparamref name="TResult" />
    ///     实例
    /// </summary>
    /// <param name="context">
    ///     <see cref="HttpContentConverterContext" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpContentConverterResult{TResult}" />
    /// </returns>
    HttpContentConverterResult<TResult> Read<TResult>(HttpContentConverterContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     将 <see cref="HttpResponseMessage" /> 转换为 <see cref="object" /> 实例
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="context">
    ///     <see cref="HttpContentConverterContext" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpContentConverterResult" />
    /// </returns>
    HttpContentConverterResult Read(Type resultType, HttpContentConverterContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     将 <see cref="HttpResponseMessage" /> 转换为
    ///     <typeparamref name="TResult" />
    ///     实例
    /// </summary>
    /// <param name="context">
    ///     <see cref="HttpContentConverterContext" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="HttpContentConverterResult{TResult}" />
    /// </returns>
    Task<HttpContentConverterResult<TResult>> ReadAsync<TResult>(HttpContentConverterContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     将 <see cref="HttpResponseMessage" /> 转换为 <see cref="object" /> 实例
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="context">
    ///     <see cref="HttpContentConverterContext" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpContentConverterResult" />
    /// </returns>
    Task<HttpContentConverterResult> ReadAsync(Type resultType, HttpContentConverterContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     根据目标类型获取对应的 <see cref="IHttpContentConverter{TResult}" /> 实例
    /// </summary>
    /// <param name="context"><see cref="HttpContentConverterContext" /> 集合</param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="IHttpContentConverter{TResult}" />
    /// </returns>
    IHttpContentConverter<TResult> GetConverter<TResult>(HttpContentConverterContext context);

    /// <summary>
    ///     根据目标类型获取对应的 <see cref="IHttpContentConverter" /> 实例
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="context"><see cref="HttpContentConverterContext" /> 集合</param>
    /// <returns>
    ///     <see cref="IHttpContentConverter" />
    /// </returns>
    IHttpContentConverter GetConverter(Type resultType, HttpContentConverterContext context);
}