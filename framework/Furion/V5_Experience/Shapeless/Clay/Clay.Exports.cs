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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Furion.Shapeless;

/// <summary>
///     流变对象
/// </summary>
public partial class Clay
{
    /// <summary>
    ///     初始化 <c>Microsoft.AspNetCore.Mvc.JsonResult</c> 类型
    /// </summary>
    internal static readonly Lazy<Type> _jsonResultType = new(() =>
        System.Type.GetType("Microsoft.AspNetCore.Mvc.JsonResult, Microsoft.AspNetCore.Mvc.Core")!);

    /// <summary>
    ///     <inheritdoc cref="Clay" />
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    public Clay(ClayOptions? options = null)
        : this(ClayType.Object, options)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="Clay" />
    /// </summary>
    /// <param name="clayType">
    ///     <see cref="ClayType" />
    /// </param>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    public Clay(ClayType clayType, ClayOptions? options = null)
    {
        // 初始化 ClayOptions
        Options = options ?? ClayOptions.Default;

        // 创建 JsonNode 选项
        var (jsonNodeOptions, jsonDocumentOptions) = CreateJsonNodeOptions(Options);

        // 创建 JsonObject 实例并指示属性名称是否不区分大小写
        JsonCanvas = JsonNode.Parse(clayType is ClayType.Object ? "{}" : "[]", jsonNodeOptions, jsonDocumentOptions)!;

        IsObject = clayType is ClayType.Object;
        IsArray = clayType is ClayType.Array;
        Type = clayType;
    }

    /// <summary>
    ///     索引
    /// </summary>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    public dynamic? this[object identifier]
    {
        get => GetValue(identifier);
        set => SetValue(identifier, value);
    }

    /// <summary>
    ///     <see cref="Range" /> 索引
    /// </summary>
    /// <remarks>截取 <see cref="Clay" /> 并返回新的 <see cref="Clay" />。</remarks>
    /// <param name="range">
    ///     <see cref="Range" />
    /// </param>
    public Clay this[Range range] => (Clay)this[range as object]!;

    /// <summary>
    ///     路径索引
    /// </summary>
    /// <remarks>根据路径获取值。</remarks>
    /// <param name="identifier">带路径的标识符</param>
    /// <param name="isPath">是否是带路径的标识符</param>
    public dynamic? this[string identifier, bool isPath] => isPath ? PathValue(identifier) : GetValue(identifier);

    /// <summary>
    ///     判断是否为单一对象
    /// </summary>
    public bool IsObject { get; }

    /// <summary>
    ///     判断是否为集合或数组
    /// </summary>
    public bool IsArray { get; }

    /// <summary>
    ///     获取流变对象的基本类型
    /// </summary>
    public ClayType Type { get; }

    /// <summary>
    ///     判断是否为只读模式
    /// </summary>
    public bool IsReadOnly => Options.ReadOnly;

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        // 空检查
        if (format is null)
        {
            return JsonCanvas.ToString();
        }

        // 将格式化字符串转换为字符数组
        var chars = format.ToUpper().ToCharArray();

        // 命名策略不能同时指定
        if (chars.Contains('C') && chars.Contains('P'))
        {
            throw new FormatException(
                $"The format string `{format}` cannot contain both 'C' and 'P', as they specify conflicting naming strategies.");
        }

        // 初始化 JsonSerializerOptions 实例
        var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerOptions.Default) { WriteIndented = true };

        // 添加压缩（取消格式化）处理
        if (chars.Contains('Z'))
        {
            jsonSerializerOptions.WriteIndented = false;
        }

        // 添加取消中文 Unicode 编码处理
        if (chars.Contains('U'))
        {
            jsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }

        // 添加小驼峰命名处理
        if (chars.Contains('C'))
        {
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        // 添加帕斯卡（大驼峰）命名处理
        if (chars.Contains('P'))
        {
            jsonSerializerOptions.PropertyNamingPolicy = new PascalCaseNamingPolicy();
        }

        return ToJsonString(jsonSerializerOptions);
    }

    /// <summary>
    ///     解构函数
    /// </summary>
    /// <param name="clay">dynamic 类型的 <see cref="Clay" /></param>
    /// <param name="enumerableClay">
    ///     <see cref="IEnumerable{T}" />
    /// </param>
    public void Deconstruct(out dynamic clay, out IEnumerable<dynamic?> enumerableClay)
    {
        clay = this;
        enumerableClay = this;
    }

    /// <summary>
    /// </summary>
    /// <param name="clay">dynamic 类型的 <see cref="Clay" /></param>
    /// <param name="enumerableClay">
    ///     <see cref="IEnumerable{T}" />
    /// </param>
    /// <param name="rawClay">
    ///     <see cref="Clay" />
    /// </param>
    public void Deconstruct(out dynamic clay, out IEnumerable<dynamic?> enumerableClay, out Clay rawClay)
    {
        clay = this;
        enumerableClay = this;
        rawClay = this;
    }

    /// <summary>
    ///     创建空的单一对象
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay EmptyObject(ClayOptions? options = null) => new(options);

    /// <summary>
    ///     创建空的集合或数组
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay EmptyArray(ClayOptions? options = null) => new(ClayType.Array, options);

    /// <summary>
    ///     将对象转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="obj">
    ///     <see cref="object" />
    /// </param>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay Parse(object? obj, ClayOptions? options = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(obj);

        // 初始化 ClayOptions 实例
        var clayOptions = options ?? ClayOptions.Default;

        // 创建 JsonNode 选项
        var (jsonNodeOptions, jsonDocumentOptions) = CreateJsonNodeOptions(clayOptions);

        // 将对象转换为 JsonNode 实例
        var jsonNode = obj switch
        {
            // 处理 application/x-www-form-urlencoded 格式数据
            string formData when formData.IsUrlEncodedFormFormat() => formData.ParseUrlEncodedFormToJsonObject(),
            string rawJson => JsonNode.Parse(rawJson, jsonNodeOptions, jsonDocumentOptions),
            Stream utf8Json => JsonNode.Parse(utf8Json, jsonNodeOptions, jsonDocumentOptions),
            byte[] utf8JsonBytes => JsonNode.Parse(utf8JsonBytes, jsonNodeOptions, jsonDocumentOptions),
            _ => SerializeToNode(obj, clayOptions)
        };

        // 处理是否将键值对格式的 JSON 字符串解析为单一对象
        if (clayOptions.KeyValueJsonToObject &&
            TryConvertKeyValueJsonToObject(jsonNode, jsonNodeOptions, jsonDocumentOptions, out var jsonObject))
        {
            jsonNode = jsonObject;
        }

        return new Clay(jsonNode, clayOptions);
    }

    /// <summary>
    ///     将对象转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="obj">
    ///     <see cref="object" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay Parse(object? obj, Action<ClayOptions> configure) =>
        Parse(obj, ClayOptions.Default.Configure(configure));

    /// <summary>
    ///     将 <see cref="Utf8JsonReader" /> 转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="utf8JsonReader">
    ///     <see cref="Utf8JsonReader" />
    /// </param>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay Parse(ref Utf8JsonReader utf8JsonReader, ClayOptions? options = null) =>
        Parse(utf8JsonReader.GetRawText(), options);

    /// <summary>
    ///     将 <see cref="Utf8JsonReader" /> 转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="utf8JsonReader">
    ///     <see cref="Utf8JsonReader" />
    /// </param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay Parse(ref Utf8JsonReader utf8JsonReader, Action<ClayOptions> configure) =>
        Parse(ref utf8JsonReader, ClayOptions.Default.Configure(configure));

    /// <summary>
    ///     从文件中读取数据并转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay ParseFromFile(string path, ClayOptions? options = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // 打开文件并读取流
        using var fileStream = File.OpenRead(path);

        return Parse(fileStream, options);
    }

    /// <summary>
    ///     从文件中读取数据并转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay ParseFromFile(string path, Action<ClayOptions> configure) =>
        ParseFromFile(path, ClayOptions.Default.Configure(configure));

    /// <summary>
    ///     二次解析指定路径的双重 JSON 序列化字符串
    /// </summary>
    /// <remarks>处理双重序列化问题。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <param name="requireJsonObjectOrArrayString">是否要求必须为字符串形式的 JSON 对象（{}）或数组（[]），默认值为：<c>true</c></param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public Clay Unwrap(string path, bool requireJsonObjectOrArrayString = true)
    {
        // 根据路径查找原始值
        var rawValue = PathValue(path);

        // 检查是否要求必须为字符串形式的 JSON 对象（{}）或数组（[]）
        if (requireJsonObjectOrArrayString)
        {
            // 仅当值为字符串且表示 JSON 对象（{}）或数组（[]）时才解析
            if (rawValue is string jsonString && IsJsonObjectOrArray(jsonString))
            {
                // 执行解析并更新路径值
                SetPathValue(path, Parse(rawValue, Options));
            }
        }
        else
        {
            // 执行解析并更新路径值
            SetPathValue(path, Parse(rawValue, Options));
        }

        return this;
    }

    /// <summary>
    ///     二次解析双重 JSON 序列化字符串
    /// </summary>
    /// <param name="maxDepth">最大递归解析深度。默认值为：3</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Clay Unwrap(int maxDepth = 3)
    {
        // 小于或等于 0 检查
        if (maxDepth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDepth),
                "Max depth must be greater than zero.");
        }

        // 处理对象或数组类型双重 JSON 序列化问题
        var scalarValueKey = Options.ScalarValueKey;
        var clay = IsScalarValue && IsJsonObjectOrArray(this[scalarValueKey] as string)
            ? Unwrap(scalarValueKey)[scalarValueKey]!
            : this;

        return ParseRecursive(clay, maxDepth);

        // 局部函数，用于执行带深度限制的解析逻辑
        Clay ParseRecursive(Clay current, int depth)
        {
            // 检查是否达到最大递归深度
            if (depth <= 0)
            {
                return current;
            }

            // 创建快照用来避免枚举修改异常问题（Collection was modified; enumeration operation may not execute.）
            // 此操作会有一点性能损耗
            var snapshot = current.AsEnumerable().ToArray();
            foreach (var (key, value) in snapshot)
            {
                switch (value)
                {
                    // 仅当值为字符串且表示 JSON 对象（{}）或数组（[]）时才解析
                    case string jsonString when IsJsonObjectOrArray(jsonString):
                        // 执行解析并更新当前键值
                        current.Set(key, Parse(jsonString, current.Options));
                        ParseRecursive((Clay)DeserializeNode(current.FindNode(key), current.Options)!, depth - 1);
                        break;
                    case Clay nestedClay:
                        ParseRecursive(nestedClay, depth);
                        break;
                }
            }

            return current;
        }
    }

    /// <summary>
    ///     检查标识符（路径）是否定义
    /// </summary>
    /// <param name="identifier">带路径的标识符</param>
    /// <param name="isPath">是否是带路径的标识符</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool Contains(string identifier, bool isPath) => isPath ? ContainsByPath(identifier) : Contains(identifier);

    /// <summary>
    ///     检查标识符是否定义
    /// </summary>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool Contains(object identifier)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(identifier);

        // 检查是否是单一对象
        if (IsObject)
        {
            // 检查键是否是不受支持的类型
            ThrowIfUnsupportedKeyType(identifier);

            // 将标识符转换为字符串类型
            var propertyName = identifier.ToString()!;

            return DelegateMap.ContainsKey(propertyName) || JsonCanvas.AsObject().ContainsKey(propertyName);
        }

        // 检查是否是 Range 实例
        if (identifier is Range)
        {
            throw new NotSupportedException(
                $"Checking containment using a System.Range `{identifier}` is not supported in the Clay.");
        }

        // 将 JsonCanvas 转换为 JsonArray 实例
        var jsonArray = JsonCanvas.AsArray();

        // 检查是否是 Index 实例
        var stringIndex = (identifier is Index idx
            ? idx.IsFromEnd ? jsonArray.Count - idx.Value : idx.Value
            : identifier).ToString();

        // 尝试将字符串标识符转换为整数索引
        if (int.TryParse(stringIndex, out var intIndex))
        {
            return intIndex >= 0 && intIndex < jsonArray.Count;
        }

        return false;
    }

    /// <summary>
    ///     根据路径检查标识符是否定义
    /// </summary>
    /// <remarks>支持简单的 JSON Path 语法。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool ContainsByPath(string path)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(path);

        // 检查是否是 JSON Path 路径
        const string jsonPathPrefix = "$.";
        var isJsonPath = path.StartsWith(jsonPathPrefix, StringComparison.Ordinal);

        // 路径标准化（仅支持简单的 JSON Path 语法）
        var normalizedPath = isJsonPath ? ArrayIndexPatternRegex().Replace(path[jsonPathPrefix.Length..], ".$1") : path;
        var pathSeparator = isJsonPath ? ["."] : Options.PathSeparator;

        // 根据路径分隔符进行分割，并确保至少有一个标识符
        var pathSegments = normalizedPath.Split(pathSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments is { Length: 0 })
        {
            return false;
        }

        var current = this;

        // 遍历所有的标识符
        for (var i = 0; i < pathSegments.Length; i++)
        {
            var segment = pathSegments[i];

            // 检查当前标识符是否定义
            if (!current.Contains(segment))
            {
                return false;
            }

            // 检查是否存在下一级
            if (i + 1 >= pathSegments.Length)
            {
                continue;
            }

            // 检查下一级是否是 Clay 类型
            if (current[segment] is not Clay clay)
            {
                return false;
            }

            current = clay;
        }

        return true;
    }

    /// <summary>
    ///     检查标识符是否定义
    /// </summary>
    /// <remarks>兼容旧版本粘土对象。</remarks>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool IsDefined(object identifier) => Contains(identifier);

    /// <summary>
    ///     检查标识符（路径）是否定义
    /// </summary>
    /// <param name="identifier">带路径的标识符</param>
    /// <param name="isPath">是否是带路径的标识符</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool IsDefined(string identifier, bool isPath) => Contains(identifier, isPath);

    /// <summary>
    ///     检查属性（键）是否定义
    /// </summary>
    /// <param name="propertyName">属性名（键）</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool HasProperty(string propertyName)
    {
        // 检查是否是集合或数组实例调用
        ThrowIfMethodCalledOnArrayCollection(nameof(HasProperty));

        return Contains(propertyName);
    }

    /// <summary>
    ///     获取集合或数组中指定项（元素）的索引
    /// </summary>
    /// <param name="value">项（元素）</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public int IndexOf(object? value)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(IndexOf));

        return Values.Select((item, index) => new { item, index }).FirstOrDefault(x => object.Equals(x.item, value))
            ?.index ?? -1;
    }

    /// <summary>
    ///     根据标识符获取值
    /// </summary>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? Get(object identifier) => GetValue(identifier);

    /// <summary>
    ///     截取 <see cref="Clay" /> 并返回新的 <see cref="Clay" />
    /// </summary>
    /// <param name="range">
    ///     <see cref="Range" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    public Clay Get(Range range)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject($"{nameof(Get)}(Range)");

        return this[range];
    }

    /// <summary>
    ///     根据标识符获取目标类型的值
    /// </summary>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    /// <exception cref="InvalidCastException"></exception>
    public object? Get(object identifier, Type resultType, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 尝试根据键获取委托
        if (TryGetDelegate(identifier, out var @delegate))
        {
            // 空检查或检查目标委托类型是否一致
            if (@delegate is null || @delegate.GetType() == resultType)
            {
                return @delegate;
            }

            throw new InvalidCastException(
                $"The delegate type `{@delegate.GetType().FullName}` cannot be cast to the target type `{resultType.FullName}`.");
        }

        // 根据标识符查找 JsonNode 节点
        var jsonNode = FindNode(identifier);

        // 处理 object 类型生成 JsonElement 问题
        if (resultType == typeof(object))
        {
            return DeserializeNode(jsonNode, Options);
        }

        return IsClay(resultType)
            ? new Clay(jsonNode, Options)
            : jsonNode.As(resultType, jsonSerializerOptions ?? Options.JsonSerializerOptions);
    }

    /// <summary>
    ///     根据标识符获取目标类型的值
    /// </summary>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    public TResult? Get<TResult>(object identifier, JsonSerializerOptions? jsonSerializerOptions = null) =>
        (TResult?)Get(identifier, typeof(TResult), jsonSerializerOptions);

    /// <summary>
    ///     根据路径获取值
    /// </summary>
    /// <remarks>不支持获取自定义委托。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? PathValue(string path) => PathValue<object>(path);

    /// <summary>
    ///     根据路径获取值
    /// </summary>
    /// <remarks>不支持获取自定义委托。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? PathValue(string path, Type resultType, JsonSerializerOptions? jsonSerializerOptions = null) =>
        // 根据路径查找 JsonNode 节点
        !FindNodeByPath(path, out _, out var parentClay, out var pathSegments)
            ? null
            : parentClay.Get(pathSegments[^1], resultType, jsonSerializerOptions);

    /// <summary>
    ///     根据路径获取值
    /// </summary>
    /// <remarks>不支持获取自定义委托。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    public TResult? PathValue<TResult>(string path, JsonSerializerOptions? jsonSerializerOptions = null) =>
        (TResult?)PathValue(path, typeof(TResult), jsonSerializerOptions);

    /// <summary>
    ///     根据路径获取值
    /// </summary>
    /// <remarks>不支持获取自定义委托。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? GetPathValue(string path) => PathValue<object>(path);

    /// <summary>
    ///     根据路径获取值
    /// </summary>
    /// <remarks>不支持获取自定义委托。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? GetPathValue(string path, Type resultType, JsonSerializerOptions? jsonSerializerOptions = null) =>
        PathValue(path, resultType, jsonSerializerOptions);

    /// <summary>
    ///     根据路径获取值
    /// </summary>
    /// <remarks>不支持获取自定义委托。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    public TResult? GetPathValue<TResult>(string path, JsonSerializerOptions? jsonSerializerOptions = null) =>
        PathValue<TResult>(path, jsonSerializerOptions);

    /// <summary>
    ///     根据标识符查找 <see cref="JsonNode" /> 节点
    /// </summary>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <returns>
    ///     <see cref="JsonNode" />
    /// </returns>
    public JsonNode? FindNode(object identifier)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(identifier);

        return IsObject ? GetNodeFromObject(identifier) : GetNodeFromArray(identifier);
    }

    /// <summary>
    ///     根据路径查找 <see cref="JsonNode" /> 节点
    /// </summary>
    /// <remarks>支持简单的 JSON Path 语法。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <returns>
    ///     <see cref="JsonNode" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public JsonNode? FindNodeByPath(string path) =>
        !FindNodeByPath(path, out var jsonNode, out _, out _) ? null : jsonNode;

    /// <summary>
    ///     根据路径查找 <see cref="JsonNode" /> 节点
    /// </summary>
    /// <remarks>支持简单的 JSON Path 语法。</remarks>
    /// <param name="path">带路径的标识符</param>
    /// <param name="jsonNode">
    ///     <see cref="JsonNode" />
    /// </param>
    /// <param name="parentClay">上级 <see cref="Clay" /></param>
    /// <param name="pathSegments">根据路径分隔符进行分割后的数组</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool FindNodeByPath(string path, out JsonNode? jsonNode, out Clay parentClay, out string[] pathSegments)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(path);

        // 检查是否是 JSON Path 路径
        const string jsonPathPrefix = "$.";
        var isJsonPath = path.StartsWith(jsonPathPrefix, StringComparison.Ordinal);

        // 路径标准化（仅支持简单的 JSON Path 语法）
        var normalizedPath = isJsonPath ? ArrayIndexPatternRegex().Replace(path[jsonPathPrefix.Length..], ".$1") : path;
        var pathSeparator = isJsonPath ? ["."] : Options.PathSeparator;

        // 根据路径分隔符进行分割，并确保至少有一个标识符
        pathSegments = normalizedPath.Split(pathSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments is { Length: 0 })
        {
            jsonNode = null;
            parentClay = this;
            return false;
        }

        // 根据标识符查找 JsonNode 节点
        var currentNode = FindNode(pathSegments[0]);
        if (currentNode is null)
        {
            jsonNode = currentNode;
            parentClay = this;
            return Contains(pathSegments[0]);
        }

        // 记录最后成功访问的流变对象
        Clay? lastClay = null;

        // 遍历剩余的标识符
        for (var i = 1; i < pathSegments.Length; i++)
        {
            // 将 currentNode 转换为对象实例
            var currentValue = DeserializeNode(currentNode, Options);

            // 检查是否是 Clay 类型
            if (!IsClay(currentValue))
            {
                throw new InvalidOperationException(
                    $"The identifier `{pathSegments[i - 1]}` at path `{pathSegments[i - 1]}:{pathSegments[i]}` does not support further lookup.");
            }

            // 进行下一级查找
            var currentClay = (Clay)currentValue;
            lastClay = currentClay;
            currentNode = currentClay.FindNode(pathSegments[i]);

            // 空检查
            if (currentNode is not null)
            {
                continue;
            }

            jsonNode = currentNode;
            parentClay = currentClay;
            return currentClay.Contains(pathSegments[i]);
        }

        jsonNode = currentNode;
        parentClay = lastClay ?? this;
        return true;
    }

    /// <summary>
    ///     根据标识符设置值
    /// </summary>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <param name="value">值</param>
    public void Set(object identifier, object? value) => SetValue(identifier, value);

    /// <summary>
    ///     在指定索引处插入项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="index">索引</param>
    /// <param name="value">值</param>
    /// <exception cref="NotSupportedException"></exception>
    public void Insert(int index, object? value)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Insert));

        SetValue(index, value, true);
    }

    /// <summary>
    ///     在指定索引处插入项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="index">索引</param>
    /// <param name="value">值</param>
    /// <exception cref="NotSupportedException"></exception>
    public void Insert(Index index, object? value)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Insert));

        Insert(index.IsFromEnd ? JsonCanvas.AsArray().Count - index.Value : index.Value, value);
    }

    /// <summary>
    ///     在指定索引处批量插入项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="index">索引</param>
    /// <param name="values">值集合</param>
    /// <exception cref="NotSupportedException"></exception>
    public void InsertRange(int index, params object?[] values)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(values);

        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(InsertRange));

        // 初始化待插入索引位置
        var currentIndex = index;

        // 逐条在指定索引处插入项
        foreach (var value in values)
        {
            SetValue(currentIndex++, value, true);
        }
    }

    /// <summary>
    ///     在指定索引处批量插入项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="index">索引</param>
    /// <param name="values">值集合</param>
    /// <exception cref="NotSupportedException"></exception>
    public void InsertRange(Index index, params object?[] values)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(values);

        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(InsertRange));

        InsertRange(index.IsFromEnd ? JsonCanvas.AsArray().Count - index.Value : index.Value, values);
    }

    /// <summary>
    ///     在末尾处添加项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="value">值</param>
    /// <exception cref="NotSupportedException"></exception>
    public void Add(object? value)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Add));

        SetValue(JsonCanvas.AsArray().Count, value);
    }

    /// <summary>
    ///     在末尾处追加项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="value">值</param>
    /// <exception cref="NotSupportedException"></exception>
    public void Append(object? value)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Append));

        SetValue(JsonCanvas.AsArray().Count, value);
    }

    /// <summary>
    ///     在末尾处批量添加项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="values">值集合</param>
    /// <exception cref="NotSupportedException"></exception>
    public void AddRange(params object?[] values)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(values);

        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(AddRange));

        // 将 JsonCanvas 转换为 JsonArray 实例
        var jsonArray = JsonCanvas.AsArray();

        // 逐条追加项
        foreach (var value in values)
        {
            SetValue(jsonArray.Count, value);
        }
    }

    /// <summary>
    ///     在末尾处添加项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="value">值</param>
    /// <exception cref="NotSupportedException"></exception>
    public void Push(object? value)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Push));

        SetValue(JsonCanvas.AsArray().Count, value);
    }

    /// <summary>
    ///     移除末尾处的项
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    public bool Pop()
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Pop));

        // 获取 JsonArray 最大索引
        var maxIndex = JsonCanvas.AsArray().Count - 1;

        return maxIndex > -1 && RemoveValue(maxIndex);
    }

    /// <summary>
    ///     反转 <see cref="Clay" /> 并返回新 <see cref="Clay" />
    /// </summary>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public Clay Reverse(ClayOptions? options = null) =>
        Parse(IsObject ? AsEnumerateObject().Reverse().ToDictionary() : Values.Reverse(), options);

    /// <summary>
    ///     截取 <see cref="Clay" /> 并返回新的 <see cref="Clay" />
    /// </summary>
    /// <param name="range">
    ///     <see cref="Range" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    public Clay Slice(Range range)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Slice));

        return this[range];
    }

    /// <summary>
    ///     截取 <see cref="Clay" /> 并返回新的 <see cref="Clay" />
    /// </summary>
    /// <param name="start">范围的包含起始索引</param>
    /// <param name="end">范围的非包含结束索引</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    public Clay Slice(Index start, Index end)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Slice));

        return Slice(new Range(start, end));
    }

    /// <summary>
    ///     组合多个 <see cref="Clay" /> 并返回新 <see cref="Clay" />
    /// </summary>
    /// <param name="clays">
    ///     <see cref="Clay" /> 集合
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public Clay Combine(params Clay[] clays)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(clays);

        // 检查是否有任何 Clay 对象为空
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (clays.Any(clay => clay is null))
        {
            throw new ArgumentException("Clay array contains one or more null elements.", nameof(clays));
        }

        // 检查流变对象类型是否一致
        if (clays.Any(u => u.Type != Type))
        {
            throw new InvalidOperationException("All Clay objects must be of the same type.");
        }

        // 检查是否是集合或数组
        if (IsArray)
        {
            return Parse(Values.Concat(clays.SelectMany(u => u.Values)));
        }

        // 深度克隆当前 Clay 实例
        var combineClay = DeepClone();

        // 遍历所有 Clay 并设置值
        foreach (var clay in clays)
        {
            foreach (var (key, value) in clay.AsEnumerateObject())
            {
                combineClay[key] = value;
            }
        }

        return combineClay;
    }

    /// <summary>
    ///     扩展属性或项
    /// </summary>
    /// <param name="values">值集合</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Clay Extend(params object?[] values)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(values);

        // 检查是否是集合或数组
        if (IsArray)
        {
            AddRange(values);

            return this;
        }

        // 遍历所有值
        foreach (var item in values)
        {
            // 检查值是否为空值或基本类型的值
            if (item is null || item.GetType().IsBasicType())
            {
                throw new InvalidOperationException("Cannot extend a single object with null or basic type values.");
            }

            // 将对象转换为字典集合
            var dictionary = item is Clay clayItem
                ? clayItem.AsEnumerateObject().ToDictionary(object (u) => u.Key, u => u.Value)
                : item.ObjectToDictionary();

            // 遍历字典键值并设置
            foreach (var (key, value) in dictionary!)
            {
                this[key] = value;
            }
        }

        return this;
    }

    /// <summary>
    ///     根据标识符删除数据
    /// </summary>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool Remove(object identifier) => RemoveValue(identifier);

    /// <summary>
    ///     根据范围删除数据
    /// </summary>
    /// <param name="start">范围的包含起始索引</param>
    /// <param name="end">范围的非包含结束索引</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    public bool Remove(Index start, Index end)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Remove));

        return Remove(new Range(start, end));
    }

    /// <summary>
    ///     根据标识符（路径）删除数据
    /// </summary>
    /// <param name="identifier">带路径的标识符</param>
    /// <param name="isPath">是否是带路径的标识符</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool Remove(string identifier, bool isPath) => isPath ? RemovePathValue(identifier) : Remove(identifier);

    /// <summary>
    ///     根据路径删除值
    /// </summary>
    /// <param name="path">带路径的标识符</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool RemovePathValue(string path) =>
        FindNodeByPath(path, out _, out var parentClay, out var pathSegments) &&
        // 从父节点删除
        parentClay.Remove(pathSegments[^1]);

    /// <summary>
    ///     根据标识符删除数据
    /// </summary>
    /// <remarks>兼容旧版本粘土对象。</remarks>
    /// <param name="identifier">标识符，可以是键（字符串）或索引（整数）或索引运算符（Index）或范围运算符（Range）</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool Delete(object identifier) => Remove(identifier);

    /// <summary>
    ///     根据范围删除数据
    /// </summary>
    /// <param name="start">范围的包含起始索引</param>
    /// <param name="end">范围的非包含结束索引</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    public bool Delete(Index start, Index end)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Delete));

        return Remove(start, end);
    }

    /// <summary>
    ///     根据标识符（路径）删除数据
    /// </summary>
    /// <param name="identifier">带路径的标识符</param>
    /// <param name="isPath">是否是带路径的标识符</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool Delete(string identifier, bool isPath) => isPath ? RemovePathValue(identifier) : Remove(identifier);

    /// <summary>
    ///     根据路径删除值
    /// </summary>
    /// <param name="path">带路径的标识符</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool DeletePathValue(string path) => RemovePathValue(path);

    /// <summary>
    ///     根据路径设置值
    /// </summary>
    /// <param name="path">带路径的标识符</param>
    /// <param name="value">值</param>
    public void SetPathValue(string path, object? value)
    {
        // 根据路径查找 JsonNode 节点
        if (FindNodeByPath(path, out _, out var parentClay, out var pathSegments))
        {
            // 从父节点更新值
            parentClay.Set(pathSegments[^1], value);
        }
    }

    /// <summary>
    ///     将 <see cref="Clay" /> 转换为目标类型
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? As(Type resultType, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 检查是否是 Clay 类型或 IEnumerable<dynamic?> 类型或 object 类型
        if (IsClay(resultType) || resultType == typeof(IEnumerable<dynamic?>) || resultType == typeof(object))
        {
            return this;
        }

        // 检查是否是 IEnumerable<KeyValuePair<object, dynamic?>> 类型
        if (resultType == typeof(IEnumerable<KeyValuePair<object, dynamic?>>))
        {
            return AsEnumerable();
        }

        // 检查是否是 IEnumerable<KeyValuePair<string, dynamic?>> 类型且是单一对象
        if (typeof(IEnumerable<KeyValuePair<string, dynamic?>>).IsAssignableFrom(resultType) && IsObject)
        {
            return resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                ? AsEnumerateObject().ToDictionary(u => u.Key, u => u.Value)
                : AsEnumerateObject();
        }

        // 检查是否是 IEnumerable<KeyValuePair<int, dynamic?>> 类型且是集合或数组
        if (typeof(IEnumerable<KeyValuePair<int, dynamic?>>).IsAssignableFrom(resultType) && IsArray)
        {
            // 将流变对象转换为键值对集合
            var keyValuePairs =
                AsEnumerateArray().Select((item, index) => new KeyValuePair<int, dynamic?>(index, item));

            return resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                ? keyValuePairs.ToDictionary(u => u.Key, u => u.Value)
                : keyValuePairs;
        }

        // 检查是否是 IActionResult 类型
        if (resultType.FullName?.Contains("Microsoft.AspNetCore.Mvc.IActionResult") == true)
        {
            return Activator.CreateInstance(_jsonResultType.Value, this);
        }

        // 检查是否是 string 类型
        if (resultType == typeof(string))
        {
            return ToJsonString(jsonSerializerOptions);
        }

        // 将 JsonNode 转换为目标类型
        var result = JsonCanvas.As(resultType, jsonSerializerOptions ?? Options.JsonSerializerOptions);

        // 检查是否启用转换后执行模型验证
        if (result is not null && Options.ValidateAfterConversion)
        {
            Validator.ValidateObject(result, new ValidationContext(result), true);
        }

        return result;
    }

    /// <summary>
    ///     将 <see cref="Clay" /> 转换为目标类型
    /// </summary>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    public TResult? As<TResult>(JsonSerializerOptions? jsonSerializerOptions = null) =>
        (TResult?)As(typeof(TResult), jsonSerializerOptions);

    /// <summary>
    ///     深度克隆
    /// </summary>
    /// <remarks>该操作不会复制自定义委托方法。</remarks>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public Clay DeepClone(ClayOptions? options = null) => Parse(JsonCanvas.ToJsonString(), options);

    /// <summary>
    ///     清空数据
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Clear()
    {
        // 确保当前实例不在只读模式下
        EnsureNotReadOnlyBeforeModify();

        // 检查是否是单一对象
        if (IsObject)
        {
            JsonCanvas.AsObject().Clear();
            DelegateMap.Clear();
        }
        else
        {
            JsonCanvas.AsArray().Clear();
        }
    }

    /// <summary>
    ///     写入提供的 <see cref="Utf8JsonWriter" /> 作为 JSON
    /// </summary>
    /// <param name="writer">
    ///     <see cref="Utf8JsonWriter" />
    /// </param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    public void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions? jsonSerializerOptions = null) =>
        JsonCanvas.WriteTo(writer, jsonSerializerOptions ?? Options.JsonSerializerOptions);

    /// <summary>
    ///     设置为只读模式
    /// </summary>
    public void AsReadOnly() => Options.ReadOnly = true;

    /// <summary>
    ///     设置为可变（默认）模式
    /// </summary>
    public void AsMutable() => Options.ReadOnly = false;

    /// <summary>
    ///     支持格式化字符串输出
    /// </summary>
    /// <param name="format">格式化字符串</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string ToString(string? format) => ToString(format, null);

    /// <inheritdoc />
    public override string ToString() => ToString(null, null);

    /// <summary>
    ///     将 <see cref="Clay" /> 输出为 JSON 格式字符串
    /// </summary>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string ToJsonString(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 获取提供的 JSON 序列化选项或默认选项
        var serializerOptions = jsonSerializerOptions ?? Options.JsonSerializerOptions;

        // 如果指定了命名策略，则对 JsonCanvas 进行键名转换；否则直接使用原 JsonCanvas
        var jsonCanvasToSerialize = serializerOptions.PropertyNamingPolicy is not null
            ? JsonCanvas.TransformKeysWithNamingPolicy(serializerOptions.PropertyNamingPolicy)
            : JsonCanvas;

        // 空检查
        ArgumentNullException.ThrowIfNull(jsonCanvasToSerialize);

        return jsonCanvasToSerialize.ToJsonString(serializerOptions);
    }

    /// <summary>
    ///     将 <see cref="Clay" /> 输出为 XML 格式字符串
    /// </summary>
    /// <param name="xmlWriterSettings">
    ///     <see cref="XmlWriterSettings" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string ToXmlString(XmlWriterSettings? xmlWriterSettings = null)
    {
        // 初始化 Utf8StringWriter 实例
        using var stringWriter = new Utf8StringWriter();

        // 初始化 XmlWriter 实例
        // 注意：如果使用 using var xmlWriter = ...; 代码方式，则需要手动调用 xmlWriter.Flush(); 方法来确保所有数据都被写入
        using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
        {
            // 将 XElement 的内容保存到 XmlWriter 中
            As<XElement>()?.Save(xmlWriter);
        }

        return stringWriter.ToString();
    }

    /// <summary>
    ///     检查类型是否是 <see cref="Clay" /> 类型
    /// </summary>
    /// <param name="type">
    ///     <see cref="Type" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool IsClay(Type type) => type == typeof(Clay) || typeof(Clay).IsAssignableFrom(type);

    /// <summary>
    ///     检查类型是否是 <see cref="Clay" /> 类型
    /// </summary>
    /// <param name="obj">
    ///     <see cref="object" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool IsClay([NotNullWhen(true)] object? obj) =>
        obj is not null && IsClay(obj as Type ?? obj.GetType());

    /// <summary>
    ///     按照键升序排序并返回新的 <see cref="Clay" />
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    public Clay KSort(ClayOptions? options = null)
    {
        // 检查是否是集合或数组实例调用
        ThrowIfMethodCalledOnArrayCollection(nameof(KSort));

        // 初始化升序排序字典
        var sorted =
            new SortedDictionary<string, JsonNode?>(JsonCanvas.AsObject().ToDictionary(), StringComparer.Ordinal);

        return Parse(sorted, options);
    }

    /// <summary>
    ///     按照键降序排序并返回新的 <see cref="Clay" />
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    // ReSharper disable once InconsistentNaming
    public Clay KRSort(ClayOptions? options = null)
    {
        // 检查是否是集合或数组实例调用
        ThrowIfMethodCalledOnArrayCollection(nameof(KRSort));

        // 初始化降序排序字典
        var sortedDesc =
            new SortedDictionary<string, JsonNode?>(Comparer<string>.Create((x, y) =>
                string.Compare(y, x, StringComparison.Ordinal)));

        // 将 JsonCanvas 转换为 JsonObject 实例
        var jsonObject = JsonCanvas.AsObject();

        // 遍历 JsonObject 所有键并追加到字典中
        foreach (var property in jsonObject)
        {
            sortedDesc.Add(property.Key, property.Value);
        }

        return Parse(sortedDesc, options);
    }

    /// <summary>
    ///     重建 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public Clay Rebuilt(ClayOptions? options = null)
    {
        // 初始化 ClayOptions 实例
        var clayOptions = options ?? ClayOptions.Default;

        // 创建 JsonNode 选项
        var (jsonNodeOptions, jsonDocumentOptions) = CreateJsonNodeOptions(clayOptions);

        // 处理是否将键值对格式的 JSON 字符串解析为单一对象
        if (clayOptions.KeyValueJsonToObject &&
            TryConvertKeyValueJsonToObject(JsonCanvas, jsonNodeOptions, jsonDocumentOptions, out var jsonObject))
        {
            JsonCanvas = jsonObject;
        }
        else
        {
            JsonCanvas = JsonNode.Parse(JsonCanvas.ToJsonString(), jsonNodeOptions, jsonDocumentOptions)!;
        }

        Options = clayOptions;

        return this;
    }

    /// <summary>
    ///     重建 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public Clay Rebuilt(Action<ClayOptions> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        return Rebuilt(Options.Configure(configure));
    }

    /// <summary>
    ///     检查字符串是否是 JSON 对象（{}）或数组（[]）
    /// </summary>
    /// <param name="input">字符串</param>
    /// <param name="allowTrailingCommas">是否允许末尾多余逗号。默认值为：<c>false</c>。</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool IsJsonObjectOrArray(string? input, bool allowTrailingCommas = false)
    {
        // 检查输入是否为字符串类型，且字符串不是由空白字符组成
        if (input is null || string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        // 去除字符串两端空格
        var text = input.Trim();

        // 检查字符串是否以 '{' 开头和 '}' 结尾，或者以 '[' 开头和 ']' 结尾
        if ((!text.StartsWith('{') || !text.EndsWith('}')) && (!text.StartsWith('[') || !text.EndsWith(']')))
        {
            return false;
        }

        try
        {
            // 使用 JsonDocument 解析字符串，若解析成功，说明是一个有效的 JSON 格式
            using var jsonDocument = JsonDocument.Parse(text,
                new JsonDocumentOptions { AllowTrailingCommas = allowTrailingCommas });

            return jsonDocument.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    ///     将 <see cref="Clay" /> 实例通过转换管道传递并返回新的 <see cref="Clay" />（失败时抛出异常）
    /// </summary>
    /// <param name="transformer">转换函数</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Clay Pipe(Func<dynamic, dynamic?> transformer) => ExecuteTransformation(transformer, true);

    /// <summary>
    ///     尝试将 <see cref="Clay" /> 实例通过转换管道传递，失败时返回原始对象
    /// </summary>
    /// <param name="transformer">转换函数</param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public Clay PipeTry(Func<dynamic, dynamic?> transformer) => ExecuteTransformation(transformer, false);

    /// <summary>
    ///     数组索引正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(@"\[(\d+)\]")]
    private static partial Regex ArrayIndexPatternRegex();

    /// <summary>
    ///     单一对象
    /// </summary>
    public sealed class Object : Clay
    {
        /// <summary>
        ///     <inheritdoc cref="Object" />
        /// </summary>
        /// <param name="options">
        ///     <see cref="ClayOptions" />
        /// </param>
        public Object(ClayOptions? options = null) : base(options)
        {
        }
    }

    /// <summary>
    ///     集合或数组
    /// </summary>
    public sealed class Array : Clay
    {
        /// <summary>
        ///     <inheritdoc cref="Array" />
        /// </summary>
        /// <param name="options">
        ///     <see cref="ClayOptions" />
        /// </param>
        public Array(ClayOptions? options = null) : base(ClayType.Array, options)
        {
        }
    }
}