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
///     cURL 多部分表单提取器
/// </summary>
internal sealed class CurlFormExtractor : HttpCurlExtractorBase
{
    /// <inheritdoc />
    protected override string[] Flags => ["-F", "--form"];

    /// <inheritdoc />
    protected override void Extract(HttpRequestBuilder httpRequestBuilder, string flag, string? argument)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(argument))
        {
            return;
        }

        // 解析 name=value 格式
        var separatorIndex = argument.IndexOf('=');
        if (separatorIndex <= 0)
        {
            throw new ArgumentException($"Invalid form format: '{argument}'. Expected 'name=value'.");
        }

        // 获取表单名称和原始值
        var name = argument[..separatorIndex];
        var rawValue = argument[(separatorIndex + 1)..];

        // 空检查
        if (httpRequestBuilder.MultipartFormDataBuilder is null)
        {
            // 设置多部分表单内容
            httpRequestBuilder.SetMultipartContent(multipart => ProcessFormItem(multipart, name, rawValue));
        }
        else
        {
            // 追加多部分表单内容
            httpRequestBuilder.WithMultipart(multipart => ProcessFormItem(multipart, name, rawValue));
        }
    }

    /// <summary>
    ///     处理单个表单项
    /// </summary>
    /// <param name="multipart">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="rawValue">原始值</param>
    internal static void ProcessFormItem(HttpMultipartFormDataBuilder multipart, string name, string rawValue)
    {
        // 检查是否是文件上传
        if (rawValue.StartsWith('@') && rawValue.Length > 1)
        {
            ProcessFileItem(multipart, name, rawValue[1..]);
        }
        else
        {
            ProcessTextItem(multipart, name, rawValue);
        }
    }

    /// <summary>
    ///     处理文件表单项
    /// </summary>
    /// <remarks>支持格式：<c>@filepath</c>、<c>@filepath;type=mime/type</c>、<c>@filepath;filename=custom.txt</c></remarks>
    /// <param name="multipart">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="filePart">文件部分字符串</param>
    internal static void ProcessFileItem(HttpMultipartFormDataBuilder multipart, string name, string filePart)
    {
        string? filePath = null;
        string? fileName = null;
        string? contentType = null;

        // 按 ; 分割附加属性
        var parts = filePart.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // 遍历分割后的部分
        foreach (var part in parts)
        {
            // 空检查
            if (filePath is null)
            {
                filePath = part;
            }
            // 解析 type= 属性
            else if (part.StartsWith("type=", StringComparison.OrdinalIgnoreCase))
            {
                contentType = part["type=".Length..];
            }
            // 解析 filename= 属性
            else if (part.StartsWith("filename=", StringComparison.OrdinalIgnoreCase))
            {
                fileName = part["filename=".Length..];
            }
        }

        // 空检查
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        // 检查是否是网络地址
        if (Uri.TryCreate(filePath, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            multipart.AddFileFromRemote(filePath, name, fileName, contentType);
        }
        else
        {
            multipart.AddFileAsStream(filePath, name, fileName, contentType);
        }
    }

    /// <summary>
    ///     处理文本表单项
    /// </summary>
    /// <remarks>支持格式：<c>value</c>、<c>value;type=mime/type</c></remarks>
    /// <param name="multipart">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="textPart">文本部分字符串</param>
    internal static void ProcessTextItem(HttpMultipartFormDataBuilder multipart, string name, string textPart)
    {
        string? value = null;
        string? contentType = null;

        // 按 ; 分割附加属性
        var parts = textPart.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // 遍历分割后的部分
        foreach (var part in parts)
        {
            // 空检查
            if (value is null)
            {
                value = part;
            }
            // 解析 type= 属性
            else if (part.StartsWith("type=", StringComparison.OrdinalIgnoreCase))
            {
                contentType = part["type=".Length..];
            }
        }

        // 空检查
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            multipart.AddObject(value, name, contentType);
        }
        else
        {
            multipart.AddFormItem(value, name);
        }
    }
}