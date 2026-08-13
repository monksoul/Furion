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

using Furion.HttpRemote.Extensions;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     设置模拟的 <see cref="HttpResponseMessage" />
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder MockResponse(HttpResponseMessage httpResponseMessage)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 释放旧的 MockedResponse 实例
        MockedResponse?.Dispose();

        MockedResponse = httpResponseMessage;
        MockedException = null;

        return this;
    }

    /// <summary>
    ///     设置模拟的 <see cref="HttpResponseMessage" />
    /// </summary>
    /// <param name="content">
    ///     <typeparamref name="T" />
    /// </param>
    /// <param name="statusCode">响应状态码</param>
    /// <param name="contentType">内容类型</param>
    /// <typeparam name="T">内容对象类型</typeparam>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder MockResponse<T>(T content, HttpStatusCode statusCode = HttpStatusCode.OK,
        string? contentType = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(content);

        // 初始化 HttpResponseMessage 实例
        var httpResponseMessage = new HttpResponseMessage(statusCode);

        // 序列化内容对象并设置给 Content 属性
        httpResponseMessage.Content = new StringContent(content.ToJsonString(), Encoding.UTF8,
            contentType ?? MediaTypeNames.Application.Json);

        return MockResponse(httpResponseMessage);
    }

    /// <summary>
    ///     设置模拟的异常
    /// </summary>
    /// <param name="exception">
    ///     <see cref="Exception" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder MockException(Exception exception)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(exception);

        // 释放旧的 MockedResponse 实例
        MockedResponse?.Dispose();

        MockedException = exception;
        MockedResponse = null;

        return this;
    }

    /// <summary>
    ///     清除所有模拟设置
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder ClearMock()
    {
        // 释放旧的 MockedResponse 实例
        MockedResponse?.Dispose();

        MockedResponse = null;
        MockedException = null;

        return this;
    }

    /// <summary>
    ///     检查当前构建器是否配置了模拟响应或模拟异常
    /// </summary>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool IsMocked() => MockedResponse is not null || MockedException is not null;
}