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
///     HTTP 远程请求请求断言构建器
/// </summary>
public sealed partial class HttpAssertionBuilder
{
    /// <summary>
    ///     断言请求 URI 等于指定字符串
    /// </summary>
    /// <param name="expectedUri">期望的请求 URI</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder RequestUri(string expectedUri)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedUri);

        _requestAssertions.Add(async context =>
        {
            if (context.RequestMessage?.RequestUri == null ||
                !string.Equals(context.RequestMessage.RequestUri.ToString(), expectedUri, StringComparison.Ordinal))
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request URI to be '{expectedUri}', but found '{context.RequestMessage?.RequestUri}'.");
            }
        });

        return this;
    }

    /// <summary>
    ///     断言请求 HTTP 方法等于指定的 <see cref="HttpMethod" />
    /// </summary>
    /// <param name="expectedMethod">期望的 HTTP 方法</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder RequestMethod(HttpMethod expectedMethod)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(expectedMethod);

        _requestAssertions.Add(async context =>
        {
            if (context.RequestMessage?.Method != expectedMethod)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request method to be {expectedMethod}, but found {context.RequestMessage?.Method}.");
            }
        });

        return this;
    }

    /// <summary>
    ///     断言指定的请求标头存在
    /// </summary>
    /// <param name="name">标头名</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder RequestHeaderExists(string name)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _requestAssertions.Add(async context =>
        {
            // 尝试从请求标头中检查，若不存在再尝试从内容标头中检查
            var exists = context.RequestMessage?.Headers.TryGetValues(name, out _) == true ||
                         context.RequestMessage?.Content?.Headers.TryGetValues(name, out _) == true;

            if (!exists)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request header '{name}' to exist, but it was not found.");
            }
        });

        return this;
    }

    /// <summary>
    ///     断言请求标头的第一个值严格等于指定字符串（区分大小写）
    /// </summary>
    /// <param name="name">标头名</param>
    /// <param name="expectedValue">期望值</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder RequestHeaderEquals(string name, string expectedValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(expectedValue);

        _requestAssertions.Add(async context =>
        {
            string? actual = null;

            // 尝试从请求标头中获取值
            if (context.RequestMessage?.Headers.TryGetValues(name, out var vals) == true)
            {
                actual = vals.FirstOrDefault();
            }
            // 尝试从请求内容标头中获取值
            else if (context.RequestMessage?.Content?.Headers.TryGetValues(name, out vals) == true)
            {
                actual = vals.FirstOrDefault();
            }

            // 检查是否存在匹配项（区分大小写）
            if (actual != expectedValue)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request header '{name}' to be '{expectedValue}', but found '{actual}'.");
            }
        });

        return this;
    }

    /// <summary>
    ///     断言请求标头的任意一个值包含指定子字符串（不区分大小写）
    /// </summary>
    /// <param name="name">标头名</param>
    /// <param name="expectedValue">期望值</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder RequestHeaderContains(string name, string expectedValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedValue);

        _requestAssertions.Add(async context =>
        {
            string[]? values = null;

            // 尝试从请求标头中获取值
            if (context.RequestMessage?.Headers.TryGetValues(name, out var vals) == true)
            {
                values = vals.ToArray();
            }
            // 尝试从请求内容标头中获取值
            else if (context.RequestMessage?.Content?.Headers.TryGetValues(name, out vals) == true)
            {
                values = vals.ToArray();
            }

            // 空检查
            if (values == null || values.Length == 0)
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request header '{name}' to contain '{expectedValue}', but the header was not found.");
            }

            // 检查是否存在匹配项（不区分大小写）
            if (!values.Any(v => v.Contains(expectedValue, StringComparison.OrdinalIgnoreCase)))
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request header '{name}' to contain '{expectedValue}', but actual values were: [{string.Join(", ", values)}].");
            }
        });

        return this;
    }

    /// <summary>
    ///     断言请求内容包含指定子字符串（不区分大小写）
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
    public HttpAssertionBuilder RequestContentContains(string expectedSubstring,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrEmpty(expectedSubstring);

        _requestAssertions.Add(async context =>
        {
            // 读取请求内容字符串
            var content = await context.ReadRequestAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(content) ||
                !content.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase))
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request content to contain '{expectedSubstring}', but it was not found.");
            }
        });

        return this;
    }

    /// <summary>
    ///     断言请求内容完全等于指定字符串
    /// </summary>
    /// <param name="expected">期望完全相等的字符串</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="HttpAssertionException"></exception>
    public HttpAssertionBuilder RequestContentEquals(string expected, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrEmpty(expected);

        _requestAssertions.Add(async context =>
        {
            // 读取请求内容字符串
            var content = await context.ReadRequestAsStringAsync(cancellationToken);

            if (!string.Equals(content, expected, StringComparison.Ordinal))
            {
                await HttpAssertionException.ThrowAsync(
                    $"Expected request content to be '{expected}', but found '{content}'.");
            }
        });

        return this;
    }

    /// <summary>
    ///     自定义请求消息断言
    /// </summary>
    /// <param name="assertion">断言委托，参数为 <see cref="HttpRequestMessage" /></param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpAssertionBuilder RequestSatisfies(Action<HttpRequestMessage> assertion)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(assertion);

        _requestAssertions.Add(context =>
        {
            if (context.RequestMessage is not null)
            {
                assertion(context.RequestMessage);
            }

            return Task.CompletedTask;
        });

        return this;
    }

    /// <summary>
    ///     自定义请求消息断言
    /// </summary>
    /// <param name="assertion">异步断言委托</param>
    /// <returns>
    ///     <see cref="HttpAssertionBuilder" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpAssertionBuilder RequestSatisfies(Func<HttpRequestMessage, Task> assertion)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(assertion);

        _requestAssertions.Add(async context =>
        {
            if (context.RequestMessage is not null)
            {
                await assertion(context.RequestMessage);
            }
        });

        return this;
    }
}