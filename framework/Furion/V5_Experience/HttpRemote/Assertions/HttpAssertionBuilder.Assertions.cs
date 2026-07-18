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

using System.Net;

namespace Furion.HttpRemote;

/// <summary>
///     HTTP 远程请求断言构建器
/// </summary>
public sealed partial class HttpAssertionBuilder
{
    /// <summary>
    ///     断言响应状态码等于指定的 <see cref="HttpStatusCode" /> 值
    /// </summary>
    /// <param name="expected">期望值</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    public HttpAssertionBuilder StatusCode(HttpStatusCode expected) => StatusCode((int)expected);

    /// <summary>
    ///     断言响应状态码等于指定的整数值
    /// </summary>
    /// <param name="expected">期望值</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder StatusCode(int expected) =>
        AddAssertion(async context =>
        {
            // 获取 HTTP 状态码
            var actual = (int)context.StatusCode;

            if (actual != expected)
            {
                await HttpAssertionException.ThrowAsync($"Expected status code to be {expected}, but found {actual}.");
            }
        });

    /// <summary>
    ///     断言响应状态码在指定的允许状态码列表中
    /// </summary>
    /// <param name="allowedStatusCodes">允许的状态码列表</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder StatusCodeIn(params int[] allowedStatusCodes)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(allowedStatusCodes);

        // 空数组检查
        if (allowedStatusCodes is { Length: 0 })
        {
            throw new ArgumentException("The allowed status codes array cannot be null or empty.",
                nameof(allowedStatusCodes));
        }

        return AddAssertion(async context =>
        {
            // 获取 HTTP 状态码
            var actual = (int)context.StatusCode;

            if (!allowedStatusCodes.Contains(actual))
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected status code to be one of [{string.Join(", ", allowedStatusCodes)}], but found {actual}.");
            }
        });
    }

    /// <summary>
    ///     断言请求成功（即响应状态码为 2xx 范围）
    /// </summary>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder IsSuccessStatusCode() =>
        AddAssertion(async context =>
        {
            if (!context.IsSuccessStatusCode)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request to be successful (2xx status code), but found status code {(int)context.StatusCode}.");
            }
        });

    /// <summary>
    ///     断言响应内容包含指定的子字符串（不区分大小写）
    /// </summary>
    /// <param name="expectedSubstring">期望包含的子字符串</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder ContentContains(string expectedSubstring, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrEmpty(expectedSubstring);

        return AddAssertion(async context =>
        {
            // 读取响应内容字符串
            var content = await context.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(content) ||
                !content.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase))
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected response content to contain '{expectedSubstring}', but it was not found.");
            }
        });
    }

    /// <summary>
    ///     断言指定的响应标头存在（可在响应标头或内容标头中）
    /// </summary>
    /// <param name="name">标头名</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder HeaderExists(string name)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return AddAssertion(async context =>
        {
            // 尝试从响应标头或内容标头中检查
            var exists = context.ResponseMessage.Headers.Contains(name) ||
                         context.ResponseMessage.Content.Headers.Contains(name);

            if (!exists)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected response header '{name}' to exist, but it was not found.");
            }
        });
    }

    /// <summary>
    ///     断言响应标头的第一个值严格等于指定字符串（区分大小写）
    /// </summary>
    /// <param name="name">标头名</param>
    /// <param name="expectedValue">期望值</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder HeaderEquals(string name, string expectedValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(expectedValue);

        return AddAssertion(async context =>
        {
            string? actualValue = null;

            // 尝试从响应标头中获取值
            if (context.ResponseMessage.Headers.TryGetValues(name, out var headerValues))
            {
                actualValue = headerValues.FirstOrDefault();
            }
            // 尝试从响应内容标头中获取值
            else if (context.ResponseMessage.Content.Headers.TryGetValues(name, out var contentHeaderValues))
            {
                actualValue = contentHeaderValues.FirstOrDefault();
            }

            // 检查是否存在匹配项（区分大小写）
            if (actualValue != expectedValue)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected response header '{name}' to be '{expectedValue}', but found '{actualValue}'.");
            }
        });
    }

    /// <summary>
    ///     断言响应标头的任意一个值包含指定的子字符串（不区分大小写）
    /// </summary>
    /// <param name="name">标头名</param>
    /// <param name="expectedValue">期望值</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder HeaderContains(string name, string expectedValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedValue);

        return AddAssertion(async context =>
        {
            string[]? values = null;

            // 尝试从响应标头中获取值
            if (context.ResponseMessage.Headers.TryGetValues(name, out var headerValues))
            {
                values = headerValues.ToArray();
            }
            // 尝试从响应内容标头中获取值
            else if (context.ResponseMessage.Content.Headers.TryGetValues(name, out var contentHeaderValues))
            {
                values = contentHeaderValues.ToArray();
            }

            // 空检查
            if (values is null || values.Length == 0)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected response header '{name}' to contain '{expectedValue}', but the header was not found.");
            }

            // 检查是否存在匹配项（不区分大小写）
            if (!values.Any(value => value.Contains(expectedValue, StringComparison.OrdinalIgnoreCase)))
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected response header '{name}' to contain '{expectedValue}', but actual values were: [{string.Join(", ", values)}].");
            }
        });
    }

    /// <summary>
    ///     断言请求耗时低于指定的毫秒数
    /// </summary>
    /// <param name="maxMilliseconds">最大允许耗时（毫秒）</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpAssertionBuilder DurationUnder(double maxMilliseconds) =>
        // 小于或等于 0 检查
        maxMilliseconds <= 0
            ? throw new ArgumentException("Max milliseconds must be greater than 0.", nameof(maxMilliseconds))
            : DurationUnder(TimeSpan.FromMilliseconds(maxMilliseconds));

    /// <summary>
    ///     断言请求耗时低于指定的时间跨度
    /// </summary>
    /// <param name="maxDuration">最大允许耗时</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder DurationUnder(TimeSpan maxDuration)
    {
        // 小于或等于 0 检查
        if (maxDuration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Max duration must be greater than 0.", nameof(maxDuration));
        }

        return AddAssertion(async context =>
        {
            // 获取实际耗时
            var actualDuration = TimeSpan.FromMilliseconds(context.RequestDuration);

            if (actualDuration > maxDuration)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request duration to be under {maxDuration.TotalMilliseconds:F2}ms, but it took {actualDuration.TotalMilliseconds:F2}ms.");
            }
        });
    }
}