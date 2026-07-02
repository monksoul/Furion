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
using Furion.Utilities;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Furion.HttpRemote;

/// <summary>
///     <see cref="MultipartFormDataContent" /> 构建器
/// </summary>
public sealed class HttpMultipartFormDataBuilder
{
    /// <inheritdoc cref="HttpRequestBuilder" />
    internal readonly HttpRequestBuilder _httpRequestBuilder;

    /// <summary>
    ///     <see cref="MultipartFormDataItem" /> 集合
    /// </summary>
    internal readonly List<MultipartFormDataItem> _partContents;

    /// <summary>
    ///     多部分表单内容项排序委托
    /// </summary>
    /// <remarks>
    ///     接收原始 <see cref="MultipartFormDataItem" /> 集合，返回排序后的可枚举集合。为 <c>null</c>
    ///     时使用默认排序：非二进制内容（如字符串、对象）优先，二进制内容（如文件流、字节数组）后置。
    /// </remarks>
    internal Func<IEnumerable<MultipartFormDataItem>, IEnumerable<MultipartFormDataItem>>? FormItemsSorter;

    /// <summary>
    ///     用于处理在添加 <see cref="HttpContent" /> 表单项内容前，对表单名称进行自定义转换的转换器
    /// </summary>
    internal Func<string, string?>? FormNameTransformer;

    /// <inheritdoc cref="OnPreAddContent" />
    internal Action<HttpContent, string>? _onPreAddContent;

    /// <summary>
    ///     <inheritdoc cref="HttpMultipartFormDataBuilder" />
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    internal HttpMultipartFormDataBuilder(HttpRequestBuilder httpRequestBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);

        _httpRequestBuilder = httpRequestBuilder;
        _partContents = [];
    }

    /// <summary>
    ///     多部分表单内容的边界
    /// </summary>
    public string? Boundary { get; set; } = $"--------------------------{DateTime.Now.Ticks:x}";

    /// <summary>
    ///     是否移除默认的多部分内容的 <c>Content-Type</c>
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool OmitContentType { get; set; } = true;

    /// <summary>
    ///     用于处理在添加 <see cref="HttpContent" /> 表单项内容时的操作
    /// </summary>
    internal Action<HttpContent, string>? OnPreAddContent
    {
        get => _onPreAddContent;
        private set => _onPreAddContent = value;
    }

    /// <summary>
    ///     设置多部分表单内容的边界
    /// </summary>
    /// <param name="boundary">多部分表单内容的边界</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder SetBoundary(string? boundary)
    {
        Boundary = boundary;

        return this;
    }

    /// <summary>
    ///     设置用于处理在添加 <see cref="HttpContent" /> 表单项内容时的操作
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder SetOnPreAddContent(Action<HttpContent, string> configure)
    {
        configure.Combine(ref _onPreAddContent);

        return this;
    }

    /// <summary>
    ///     设置用于处理在添加 <see cref="HttpContent" /> 表单项内容前，对表单名称进行自定义转换的转换器
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder SetFormNameTransformer(Func<string, string>? configure)
    {
        FormNameTransformer = configure;

        return this;
    }

    /// <summary>
    ///     设置用于处理在添加 <see cref="HttpContent" /> 表单项内容前，对表单名称进行自定义转换的转换器
    /// </summary>
    /// <param name="namingPolicy">
    ///     <see cref="FormNamingPolicy" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public HttpMultipartFormDataBuilder SetFormNameTransformer(FormNamingPolicy namingPolicy) =>
        SetFormNameTransformer(namingPolicy switch
        {
            FormNamingPolicy.None => null,
            FormNamingPolicy.CamelCase => JsonNamingPolicy.CamelCase.ConvertName,
            FormNamingPolicy.SnakeCaseLower => JsonNamingPolicy.SnakeCaseLower.ConvertName,
            FormNamingPolicy.SnakeCaseUpper => JsonNamingPolicy.SnakeCaseUpper.ConvertName,
            FormNamingPolicy.KebabCaseLower => JsonNamingPolicy.KebabCaseLower.ConvertName,
            FormNamingPolicy.KebabCaseUpper => JsonNamingPolicy.KebabCaseUpper.ConvertName,
            _ => throw new ArgumentOutOfRangeException(nameof(namingPolicy), namingPolicy, null)
        });

    /// <summary>
    ///     设置多部分表单内容项的排序规则
    /// </summary>
    /// <param name="sorter">
    ///     接收原始 <see cref="MultipartFormDataItem" /> 集合，返回排序后的可枚举集合。为 <c>null</c>
    ///     时使用默认排序：非二进制内容（如字符串、对象）优先，二进制内容（如文件流、字节数组）后置
    /// </param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder SetFormItemsSorter(
        Func<IEnumerable<MultipartFormDataItem>, IEnumerable<MultipartFormDataItem>>? sorter)
    {
        FormItemsSorter = sorter;

        return this;
    }

    /// <summary>
    ///     添加 JSON 内容
    /// </summary>
    /// <param name="rawJson">JSON 字符串/原始对象</param>
    /// <param name="name">表单名称。该值不为空时作为表单的一项。否则将遍历对象类型的每一个公开属性作为表单的项。</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    /// <exception cref="JsonException"></exception>
    public HttpMultipartFormDataBuilder AddJson(object? rawJson, string? name = null, Encoding? contentEncoding = null,
        string? contentType = null, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 检查是否配置表单名或不是字符串类型
        if (!string.IsNullOrWhiteSpace(name) || rawJson is not string rawString)
        {
            // 当传入 jsonSerializerOptions 参数时，直接进行序列化后再传入
            var extractedJson = jsonSerializerOptions is null
                ? rawJson
                : JsonSerializer.Serialize(rawJson, jsonSerializerOptions);

            return AddObject(extractedJson, name, contentType ?? MediaTypeNames.Application.Json, contentEncoding);
        }

        // 尝试验证并获取 JsonDocument 实例（需 using）
        var jsonDocument = JsonUtility.Parse(rawString);

        // 添加请求结束时需要释放的对象
        _httpRequestBuilder.AddDisposable(jsonDocument);

        return AddObject(jsonDocument, name, contentType ?? MediaTypeNames.Application.Json, contentEncoding);
    }

    /// <summary>
    ///     添加 JSON 内容（宽松模式）
    /// </summary>
    /// <remarks>跳过对 JSON 字符串校验。</remarks>
    /// <param name="rawJson">JSON 字符串</param>
    /// <param name="name">表单名称</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddJsonWithoutValidation(string? rawJson, string name,
        Encoding? contentEncoding = null,
        string? contentType = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return AddObject(rawJson, name, contentType ?? MediaTypeNames.Application.Json, contentEncoding);
    }

    /// <summary>
    ///     添加单个表单项内容
    /// </summary>
    /// <param name="value">表单值</param>
    /// <param name="name">表单名称</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddFormItem(object? value, string name, Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return AddObject(value, name, MediaTypeNames.Text.Plain, contentEncoding);
    }

    /// <summary>
    ///     添加 HTML 内容
    /// </summary>
    /// <param name="htmlString">HTML 字符串</param>
    /// <param name="name">表单名称</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddHtml(string? htmlString, string name, Encoding? contentEncoding = null,
        string? contentType = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return AddObject(htmlString, name, contentType ?? MediaTypeNames.Text.Html, contentEncoding);
    }

    /// <summary>
    ///     添加 XML 内容
    /// </summary>
    /// <param name="xmlString">XML 字符串</param>
    /// <param name="name">表单名称</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddXml(string? xmlString, string name, Encoding? contentEncoding = null,
        string? contentType = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return AddObject(xmlString, name, contentType ?? MediaTypeNames.Text.Xml, contentEncoding);
    }

    /// <summary>
    ///     添加文本内容
    /// </summary>
    /// <param name="text">文本</param>
    /// <param name="name">表单名称</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddText(string? text, string name, Encoding? contentEncoding = null,
        string? contentType = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return AddObject(text, name, contentType ?? MediaTypeNames.Text.Plain, contentEncoding);
    }

    /// <summary>
    ///     添加对象内容
    /// </summary>
    /// <param name="rawObject">原始对象</param>
    /// <param name="name">表单名称。该值不为空时作为表单的一项。否则将遍历对象类型的每一个公开属性作为表单的项。</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddObject(object? rawObject, string? name = null, string? contentType = null,
        Encoding? contentEncoding = null)
    {
        // 解析内容类型字符串
        Encoding? encoding = null;
        var mediaType = string.IsNullOrWhiteSpace(contentType)
            ? MediaTypeNames.Text.Plain
            : ParseContentType(contentType, contentEncoding, out encoding);

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

        // 检查是否配置表单名
        if (!string.IsNullOrWhiteSpace(name))
        {
            _partContents.Add(new MultipartFormDataItem(name)
            {
                ContentType = mediaType, RawContent = rawObject, ContentEncoding = encoding
            });

            return this;
        }

        // 空检查
        ArgumentNullException.ThrowIfNull(rawObject);

        // 将对象转换为字典集合再追加
        var formDataItems = rawObject.ObjectToDictionary()!;

        // 遍历字典集合并逐条追加
        foreach (var (itemName, rawContent) in formDataItems)
        {
            // 初始化表单名称
            var formName = itemName.ToInvariantCultureString()!;

            switch (rawContent)
            {
                // 检查原始请求内容是否是 MultipartFile 类型
                case MultipartFile multipartFile:
                    AddFile(multipartFile, formName);
                    break;
                // 检查原始请求内容是否是 FileInfo 类型
                case FileInfo fileInfo:
                    AddFile(fileInfo, formName);
                    break;
                default:
                    _partContents.Add(new MultipartFormDataItem(formName)
                    {
                        ContentType = Helpers.GetContentTypeOrDefault(rawContent, MediaTypeNames.Text.Plain),
                        RawContent = rawContent,
                        ContentEncoding = encoding
                    });
                    break;
            }
        }

        return this;
    }

    /// <summary>
    ///     从互联网 URL 中添加文件
    /// </summary>
    /// <remarks>文件大小限制在 <c>100MB</c> 以内。</remarks>
    /// <param name="url">互联网 URL 地址</param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="configure">自定义配置委托</param>
    /// <param name="httpMethod"><see cref="HttpMethod" />，默认值为：<see cref="HttpMethod.Get" /></param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public HttpMultipartFormDataBuilder AddFileFromRemote(string url, string name = "file", string? fileName = null,
        string? contentType = null, Encoding? contentEncoding = null,
        Action<HttpClient, HttpRequestMessage>? configure = null, HttpMethod? httpMethod = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 尝试从 Uri 地址中解析文件的名称
        var newFileName = fileName ?? Helpers.GetFileNameFromUri(new Uri(url, UriKind.Absolute));

        // 尝试从互联网 URL 地址中加载流
        var fileStream = Helpers.GetStreamFromRemote(url, configure, httpMethod: httpMethod);

        return AddStream(fileStream, name, newFileName, contentType, contentEncoding, true);
    }

    /// <summary>
    ///     从 Base64 字符串中添加文件
    /// </summary>
    /// <remarks>文件大小限制在 <c>100MB</c> 以内。</remarks>
    /// <param name="base64String">Base64 字符串</param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public HttpMultipartFormDataBuilder AddFileFromBase64String(string base64String, string name = "file",
        string? fileName = null, string? contentType = null, Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(base64String);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 将 Base64 字符串转换成字节数组
        var bytes = Convert.FromBase64String(base64String);

        // 获取字节数组长度
        var fileLength = bytes.Length;

        // 限制文件字节数组大小在 100MB 以内
        const long maxFileSizeInBytes = 104857600L;
        if (fileLength > maxFileSizeInBytes)
        {
            throw new InvalidOperationException(
                $"The file size exceeds the maximum allowed size of `{maxFileSizeInBytes.ToSizeUnits("MB"):F2}MB`.");
        }

        return AddByteArray(bytes, name, fileName, contentType, contentEncoding);
    }

    /// <summary>
    ///     从本地路径中添加文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddFileAsStream(string filePath, string name = "file", string? fileName = null,
        string? contentType = null, Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 检查文件是否存在
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The specified file `{filePath}` does not exist.");
        }

        // 获取文件的名称
        var newFileName = fileName ?? Path.GetFileName(filePath);

        // 读取文件流（没有 using）
        var fileStream = File.OpenRead(filePath);

        return AddStream(fileStream, name, newFileName, contentType, contentEncoding,
            true);
    }

    /// <summary>
    ///     从本地路径中添加文件（带文件传输进度）
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="progressChannel">文件传输进度信息的通道</param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddFileWithProgressAsStream(string filePath,
        Channel<FileTransferProgress> progressChannel, string name = "file", string? fileName = null,
        string? contentType = null, Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(progressChannel);

        // 检查文件是否存在
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The specified file `{filePath}` does not exist.");
        }

        // 获取文件的名称
        var newFileName = fileName ?? Path.GetFileName(filePath);

        // 读取文件流（没有 using）
        var fileStream = File.OpenRead(filePath);

        // 初始化带读写进度的文件流
        var progressFileStream = new ProgressFileStream(fileStream, filePath, progressChannel, newFileName);

        return AddStream(progressFileStream, name, newFileName, contentType, contentEncoding,
            true);
    }

    /// <summary>
    ///     从本地路径中添加文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddFileAsByteArray(string filePath, string name = "file",
        string? fileName = null, string? contentType = null, Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 检查文件是否存在
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The specified file `{filePath}` does not exist.");
        }

        // 获取文件的名称
        var newFileName = fileName ?? Path.GetFileName(filePath);

        // 读取文件字节数组
        var bytes = File.ReadAllBytes(filePath);

        return AddByteArray(bytes, name, newFileName, contentType, contentEncoding);
    }

    /// <summary>
    ///     添加文件
    /// </summary>
    /// <param name="fileInfo">
    ///     <see cref="FileInfo" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddFile(FileInfo fileInfo, string? name = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(fileInfo);

        _partContents.Add(new MultipartFormDataItem(name ?? "file")
        {
            ContentType = Helpers.GetContentTypeOrDefault(fileInfo, MediaTypeNames.Application.Octet),
            RawContent = fileInfo,
            FileName = fileInfo.Name
        });

        return this;
    }

    /// <summary>
    ///     添加文件
    /// </summary>
    /// <remarks>使用 <c>MultipartFile.CreateFrom[Source]</c> 静态方法创建。</remarks>
    /// <param name="multipartFile">
    ///     <see cref="MultipartFile" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddFile(MultipartFile multipartFile, string? name = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(multipartFile);

        // 获取表单名称
        var formName = name ?? multipartFile.Name ?? "file";

        switch (multipartFile.FileSourceType)
        {
            // 字节数组
            case FileSourceType.ByteArray:
                return AddByteArray((byte[])multipartFile.Source!, formName, multipartFile.FileName,
                    multipartFile.ContentType, multipartFile.ContentEncoding);
            // Stream
            case FileSourceType.Stream:
                return AddStream((Stream)multipartFile.Source!, formName, multipartFile.FileName,
                    multipartFile.ContentType, multipartFile.ContentEncoding);
            // 本地文件路径
            case FileSourceType.Path:
                return AddFileAsStream((string)multipartFile.Source!, formName, multipartFile.FileName,
                    multipartFile.ContentType, multipartFile.ContentEncoding);
            // Base64 字符串文件
            case FileSourceType.Base64String:
                return AddFileFromBase64String((string)multipartFile.Source!, formName,
                    multipartFile.FileName, multipartFile.ContentType, multipartFile.ContentEncoding);
            // 互联网文件地址
            case FileSourceType.Remote:
                return AddFileFromRemote((string)multipartFile.Source!, formName, multipartFile.FileName,
                    multipartFile.ContentType, multipartFile.ContentEncoding);
            // 不做处理
            case FileSourceType.None:
            default:
                return this;
        }
    }

    /// <summary>
    ///     添加流
    /// </summary>
    /// <param name="stream">
    ///     <see cref="Stream" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="disposeStreamOnRequestCompletion">是否在请求结束后自动释放流。默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddStream(Stream stream, string name = "file", string? fileName = null,
        string? contentType = null, Encoding? contentEncoding = null, bool disposeStreamOnRequestCompletion = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 检查流是否可读
        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream must be readable.", nameof(stream));
        }

        // 检查流是否支持查找
        if (stream.CanSeek)
        {
            // 重置到起始位置
            stream.Position = 0;
        }

        // 解析内容类型字符串
        var mediaType = ParseContentType(contentType, contentEncoding, out var encoding);

        // 获取文件 MIME 类型
        var mimeType = !string.IsNullOrWhiteSpace(mediaType) ? mediaType :
            string.IsNullOrWhiteSpace(fileName) ? MediaTypeNames.Application.Octet :
            FileTypeMapper.GetContentType(fileName);

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);

        _partContents.Add(new MultipartFormDataItem(name)
        {
            ContentType = mimeType, RawContent = stream, ContentEncoding = encoding, FileName = fileName
        });

        // 是否在请求结束后自动释放流
        if (disposeStreamOnRequestCompletion)
        {
            _httpRequestBuilder.AddDisposable(stream);
        }

        return this;
    }

    /// <summary>
    ///     添加字节数组
    /// </summary>
    /// <param name="byteArray">字节数组</param>
    /// <param name="name">表单名称</param>
    /// <param name="fileName">文件的名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddByteArray(byte[] byteArray, string name = "file", string? fileName = null,
        string? contentType = null, Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(byteArray);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 解析内容类型字符串
        var mediaType = ParseContentType(contentType, contentEncoding, out var encoding);

        // 获取文件 MIME 类型
        var mimeType = !string.IsNullOrWhiteSpace(mediaType) ? mediaType :
            string.IsNullOrWhiteSpace(fileName) ? MediaTypeNames.Application.Octet :
            FileTypeMapper.GetContentType(fileName);

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);

        _partContents.Add(new MultipartFormDataItem(name)
        {
            ContentType = mimeType, RawContent = byteArray, ContentEncoding = encoding, FileName = fileName
        });

        return this;
    }

    /// <summary>
    ///     添加 URL 编码表单
    /// </summary>
    /// <param name="rawObject">原始对象</param>
    /// <param name="name">表单名称</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="useStringContent">
    ///     是否使用 <see cref="StringContent" /> 构建
    ///     <see cref="FormUrlEncodedContent" />。默认值为： <c>false</c>
    /// </param>
    /// <param name="useUrlEncode">是否对表单数据进行 URL 编码，默认值为： <c>true</c></param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddFormUrlEncoded(object? rawObject, string name,
        Encoding? contentEncoding = null, bool useStringContent = false, bool useUrlEncode = true)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _partContents.Add(new MultipartFormDataItem(name)
        {
            ContentType = MediaTypeNames.Application.FormUrlEncoded,
            RawContent = rawObject,
            ContentEncoding = contentEncoding
        });

        // 检查是否对表单数据进行 URL 编码
        if (!useUrlEncode)
        {
            _httpRequestBuilder.AddHttpContentProcessors(() =>
                [new StringContentForFormUrlEncodedContentProcessor { UrlEncode = false }]);
        }
        else
        {
            // 检查是否启用 StringContent 方式构建 application/x-www-form-urlencoded 请求内容
            if (useStringContent)
            {
                _httpRequestBuilder.AddStringContentForFormUrlEncodedContentProcessor();
            }
        }

        return this;
    }

    /// <summary>
    ///     添加多部分表单内容
    /// </summary>
    /// <param name="rawObject">原始对象</param>
    /// <param name="name">表单名称</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder AddMultipartFormData(object? rawObject, string name,
        Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _partContents.Add(new MultipartFormDataItem(name)
        {
            ContentType = MediaTypeNames.Multipart.FormData,
            RawContent = rawObject,
            ContentEncoding = contentEncoding
        });

        return this;
    }

    /// <summary>
    ///     添加 <see cref="HttpContent" />
    /// </summary>
    /// <param name="httpContent">
    ///     <see cref="HttpContent" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder Add(HttpContent httpContent, string? name, string? contentType = null,
        Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpContent);

        // 尝试从 ContentDisposition 中解析 Name
        var formName = string.IsNullOrWhiteSpace(name) ? httpContent.Headers.ContentDisposition?.Name : name;

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(formName, nameof(name));

        string? mediaType;
        Encoding? encoding = null;
        MediaTypeHeaderValue? mediaTypeHeaderValue = null;

        // 空检查
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            mediaType = ParseContentType(contentType, contentEncoding, out encoding);
        }
        else
        {
            mediaTypeHeaderValue = httpContent.Headers.ContentType;
            mediaType = mediaTypeHeaderValue?.MediaType;
        }

        // 尝试从 FileName 中解析 MediaType
        if (string.IsNullOrWhiteSpace(mediaType))
        {
            mediaType = FileTypeMapper.GetContentType(
                httpContent.Headers.ContentDisposition?.FileName?.TrimStart('"').TrimEnd('"')!,
                null!);
        }

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType, nameof(contentType));

        // 设置或解析内容编码
        encoding = contentEncoding ?? encoding ?? (string.IsNullOrWhiteSpace(mediaTypeHeaderValue?.CharSet)
            ? null
            : Encoding.GetEncoding(mediaTypeHeaderValue.CharSet));

        _partContents.Add(new MultipartFormDataItem(formName)
        {
            ContentType = mediaType, RawContent = httpContent, ContentEncoding = encoding
        });

        return this;
    }

    /// <summary>
    ///     设置是否移除默认的多部分内容的 <c>Content-Type</c>
    /// </summary>
    /// <param name="omit">如果为 <c>true</c> 则移除，默认值为：<c>true</c></param>
    /// <returns>
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </returns>
    public HttpMultipartFormDataBuilder SetOmitContentType(bool omit)
    {
        OmitContentType = omit;

        return this;
    }

    /// <summary>
    ///     构建 <see cref="MultipartFormDataContent" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="httpContentProcessorFactory">
    ///     <see cref="IHttpContentProcessorFactory" />
    /// </param>
    /// <param name="processors"><see cref="IHttpContentProcessor" /> 集合</param>
    /// <returns>
    ///     <see cref="MultipartFormDataContent" />
    /// </returns>
    internal MultipartFormDataContent? Build(HttpRemoteOptions httpRemoteOptions,
        IHttpContentProcessorFactory httpContentProcessorFactory, params IHttpContentProcessor[]? processors)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);
        ArgumentNullException.ThrowIfNull(httpContentProcessorFactory);

        // 空检查
        if (_partContents.IsNullOrEmpty())
        {
            return null;
        }

        // 获取多部分表单内容的边界；注意：这里可能出现前后双引号问题
        var boundary = Boundary?.TrimStart('"').TrimEnd('"');

        // 初始化 multipartFormDataContent 实例
        var multipartFormDataContent = string.IsNullOrWhiteSpace(boundary)
            ? new MultipartFormDataContent()
            : new MultipartFormDataContent(boundary);

        // 处理 OSS 对象存储服务必须设置 Content-Type 问题
        if (!string.IsNullOrWhiteSpace(boundary))
        {
            multipartFormDataContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse($"{MediaTypeNames.Multipart.FormData}; boundary={boundary}");
        }

        // 调用自定义多部分表单内容项排序委托
        // 为 null 时使用默认排序：非二进制内容（如字符串、对象）优先，二进制内容（如文件流、字节数组）后置：主要解决请求分析工具不能完整显示所有表单数据问题（避免因打印裁剪导致信息丢失）
        var sortedPartContents = FormItemsSorter?.Invoke(_partContents) ??
                                 _partContents.OrderBy(u => u.RawContent is byte[] or Stream or HttpContent ? 1 : 0);

        // 逐条遍历添加
        foreach (var dataItem in sortedPartContents)
        {
            // 尝试获取转换后的表单名称
            var transformedName = FormNameTransformer?.Invoke(dataItem.Name) ?? dataItem.Name;

            // 构建 HttpContent 实例
            var httpContent = BuildHttpContent(dataItem, transformedName, httpContentProcessorFactory, processors);

            // 空检查
            if (httpContent is null)
            {
                continue;
            }

            // 检查是否移除默认的多部分内容的 Content-Type，解决对接 Java 程序时可能出现失败问题
            if (OmitContentType)
            {
                httpContent.Headers.ContentType = null;
            }

            // 调用用于处理在添加 HttpContent 表单项内容时的操作
            OnPreAddContent?.Invoke(httpContent, transformedName);

            // 添加 HttpContent 表单项内容
            multipartFormDataContent.Add(httpContent, transformedName);
        }

        return multipartFormDataContent;
    }

    /// <summary>
    ///     构建 <see cref="HttpContent" /> 实例
    /// </summary>
    /// <param name="multipartFormDataItem">
    ///     <see cref="MultipartFormDataItem" />
    /// </param>
    /// <param name="name">表单名称</param>
    /// <param name="httpContentProcessorFactory">
    ///     <see cref="IHttpContentProcessorFactory" />
    /// </param>
    /// <param name="processors"><see cref="IHttpContentProcessor" /> 集合</param>
    /// <returns>
    ///     <see cref="HttpContent" />
    /// </returns>
    internal HttpContent? BuildHttpContent(MultipartFormDataItem multipartFormDataItem, string name,
        IHttpContentProcessorFactory httpContentProcessorFactory, params IHttpContentProcessor[]? processors)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(multipartFormDataItem);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(httpContentProcessorFactory);

        // 空检查
        var contentType = multipartFormDataItem.ContentType;
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        // 初始化 HttpContentProcessorContext 实例
        var processorContext =
            new HttpContentProcessorContext(multipartFormDataItem.RawContent, contentType,
                multipartFormDataItem.ContentEncoding)
            {
                HttpClientName = _httpRequestBuilder.HttpClientName, AsFormItem = true
            };

        // 构建 HttpContent 实例
        var httpContent = httpContentProcessorFactory.Build(processorContext, processors);

        // 是否在请求结束后自动释放流
        if (processorContext.CompletionDisposable is { } disposable)
        {
            _httpRequestBuilder.AddDisposable(disposable);
        }

        // 空检查
        if (httpContent is not null && httpContent.Headers.ContentDisposition is null)
        {
            // 设置表单项内容 Content-Disposition 标头
            httpContent.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("form-data")
                {
                    Name = name.AddQuotes(),
                    FileName =
                        (string.IsNullOrWhiteSpace(multipartFormDataItem.FileName) &&
                         contentType.IsIn([MediaTypeNames.Application.Octet], StringComparer.OrdinalIgnoreCase)
                            // 解决发送文件或二进制【未设置】文件名问题
                            ? $"Unnamed_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}"
                            : multipartFormDataItem.FileName).AddQuotes()
                };
        }

        return httpContent;
    }

    /// <summary>
    ///     解析内容类型字符串
    /// </summary>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="encoding">内容编码</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ParseContentType(string? contentType, Encoding? contentEncoding, out Encoding? encoding)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(contentType))
        {
            encoding = null;
            return null;
        }

        // 解析内容类型字符串
        var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);

        // 解析/设置内容编码
        encoding = contentEncoding ?? (!string.IsNullOrWhiteSpace(mediaTypeHeaderValue.CharSet)
            ? Encoding.GetEncoding(mediaTypeHeaderValue.CharSet)
            : contentEncoding);

        return mediaTypeHeaderValue.MediaType;
    }
}