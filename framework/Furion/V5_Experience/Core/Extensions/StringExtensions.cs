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

using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Furion.Extensions;

/// <summary>
///     <see cref="string" /> 扩展类
/// </summary>
internal static partial class StringExtensions
{
    /// <summary>
    ///     为字符串前后添加双引号
    /// </summary>
    /// <param name="input">
    ///     <see cref="string" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? AddQuotes(this string? input)
    {
        // 空检查
        if (input is null)
        {
            return input;
        }

        // 检查是否已经有双引号，防止重复添加
        if (input.StartsWith('"') && input.EndsWith('"') && input.Length >= 2)
        {
            return input;
        }

        return $"\"{input}\"";
    }

    /// <summary>
    ///     将字符串首字母转换为小写
    /// </summary>
    /// <param name="input">
    ///     <see cref="string" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ToLowerFirstLetter(this string? input)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // 初始化字符串构建器
        var stringBuilder = new StringBuilder(input);

        // 设置字符串构建器首个字符为小写
        stringBuilder[0] = char.ToLower(stringBuilder[0]);

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     将字符串进行转义
    /// </summary>
    /// <param name="input">
    ///     <see cref="string" />
    /// </param>
    /// <param name="escape">是否转义字符串</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? EscapeDataString(this string? input, bool escape)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return !escape ? input : Uri.EscapeDataString(input);
    }

    /// <summary>
    ///     检查字符串是否存在于给定的集合中
    /// </summary>
    /// <param name="input">
    ///     <see cref="string" />
    /// </param>
    /// <param name="collection">
    ///     <see cref="IEnumerable{T}" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsIn(this string? input, IEnumerable<string?> collection,
        IEqualityComparer? comparer = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(collection);

        // 使用默认或提供的比较器
        comparer ??= EqualityComparer<string>.Default;

        return input is null ? collection.Any(u => u is null) : collection.Any(u => comparer.Equals(input, u));
    }

    /// <summary>
    ///     解析符合键值对格式的字符串为键值对列表
    /// </summary>
    /// <param name="keyValueString">键值对格式的字符串</param>
    /// <param name="separators">分隔符字符数组</param>
    /// <param name="trimChar">要删除的前导字符</param>
    /// <returns>
    ///     <see cref="List{T}" />
    /// </returns>
    internal static List<KeyValuePair<string, string?>> ParseFormatKeyValueString(this string keyValueString,
        char[]? separators = null, char? trimChar = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(keyValueString);

        // 空检查
        if (string.IsNullOrWhiteSpace(keyValueString))
        {
            return [];
        }

        // 默认隔符为 &
        separators ??= ['&'];

        var pairs = (trimChar is null ? keyValueString : keyValueString.TrimStart(trimChar.Value)).Split(separators);
        return (from pair in pairs
            select pair.Split('=', 2) // 限制只分割一次
            into keyValue
            where keyValue.Length == 2
            select new KeyValuePair<string, string?>(keyValue[0].Trim(), keyValue[1])).ToList();
    }

    /// <summary>
    ///     基于 GBK 编码将字符串右填充至指定的字节数
    /// </summary>
    /// <remarks>调用之前需确保上下文存在 <c>Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);</c> 代码。</remarks>
    /// <param name="output">字符串</param>
    /// <param name="totalByteCount">目标字节数</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static string? PadStringToByteLength(this string? output, int totalByteCount)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(output))
        {
            return output;
        }

        // 小于或等于 0 检查
        if (totalByteCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalByteCount),
                "Total byte count must be greater than zero.");
        }

        // 获取 GBK 编码实例
        var coding = Encoding.GetEncoding("gbk");

        // 获取字符串的字节数组
        var bytes = coding.GetBytes(output);
        var currentByteCount = bytes.Length;

        // 如果当前字节长度已经等于或超过目标字节长度，则直接返回原字符串
        if (currentByteCount >= totalByteCount)
        {
            return output;
        }

        // 计算需要添加的空格数量
        var spaceBytes = coding.GetByteCount(" ");
        var paddingSpaces = (totalByteCount - currentByteCount) / spaceBytes;

        // 确保填充不会超出范围
        if (currentByteCount + (paddingSpaces * spaceBytes) > totalByteCount)
        {
            paddingSpaces--;
        }

        // 创建新的字符串并进行填充
        var paddedChars = new char[output.Length + paddingSpaces];
        output.CopyTo(0, paddedChars, 0, output.Length);

        // 填充剩余部分
        for (var i = output.Length; i < output.Length + paddingSpaces; i++)
        {
            paddedChars[i] = ' ';
        }

        return new string(paddedChars);
    }

    /// <summary>
    ///     替换字符串中的占位符为实际值
    /// </summary>
    /// <param name="template">包含占位符的模板字符串</param>
    /// <param name="replacementSource">
    ///     <see cref="IDictionary{TKey,TValue}" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static string? ReplacePlaceholders(this string? template, IDictionary<string, string?> replacementSource)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(replacementSource);

        return template is null
            ? null
            : PlaceholderRegex().Replace(template, match =>
            {
                // 获取占位符中的路径
                var path = match.Groups[1].Value.Trim();
                // 检查是否以 ? 结尾
                var hasQuestionMark = match.Groups[2].Success;

                // 尝试将完整路径作为键直接查找
                if (replacementSource.TryGetValue(path, out var replacement))
                {
                    return replacement ?? string.Empty;
                }

                // 若直接查找失败，则按 . 和 [index] 逐级解析（支持索引及嵌套 JSON 对象/数组）
                var resolved = ResolveNestedPathFromDictionary(path, replacementSource);
                if (resolved is not null)
                {
                    return resolved;
                }

                return hasQuestionMark ? string.Empty : match.Value;
            });
    }

    /// <summary>
    ///     替换字符串中的占位符为实际值
    /// </summary>
    /// <param name="template">包含占位符的模板字符串</param>
    /// <param name="replacementSource">
    ///     <see cref="object" />
    /// </param>
    /// <param name="prefix">模板字符串前缀；默认值为：<c>model</c>。</param>
    /// <param name="bindingFlags">
    ///     <see cref="BindingFlags" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ReplacePlaceholders(this string? template, object? replacementSource,
        string prefix = "model",
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public) =>
        template is null
            ? null
            : PlaceholderRegex().Replace(template, match =>
            {
                // 获取占位符中的路径
                var path = match.Groups[1].Value.Trim();
                // 检查是否以 ? 结尾
                var hasQuestionMark = match.Groups[2].Success;

                // 根据路径从对象中获取属性值
                var replacement =
                    replacementSource.ResolveNestedPathFromObject(path, out var isMatch, prefix, bindingFlags);

                // 检查路径是否匹配成功
                if (isMatch)
                {
                    return replacement?.ToInvariantCultureString() ?? string.Empty;
                }

                return hasQuestionMark ? string.Empty : match.Value;
            });

    /// <summary>
    ///     替换字符串中的占位符为实际值
    /// </summary>
    /// <param name="template">包含占位符的模板字符串</param>
    /// <param name="replacementSource">
    ///     <see cref="IConfiguration" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ReplacePlaceholders(this string? template, IConfiguration? replacementSource)
    {
        // 空检查
        if (replacementSource is null)
        {
            return template;
        }

        return template is null
            ? null
            : ConfigurationKeyRegex().Replace(template, match =>
            {
                // 获取主键、备用键、默认值和问号标记
                var mainKey = match.Groups[1].Value.Trim();
                var backupKeysRaw = match.Groups[2].Value.Trim();
                var defaultValue = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null;
                var hasQuestionMark = match.Groups[4].Success;

                // 分割并清理备用键列表
                var backupKeys = backupKeysRaw.Split(['|'], StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

                // 合并主键和备用键列表
                var allKeys = new List<string> { mainKey };
                allKeys.AddRange(backupKeys);

                // 逐个匹配键，一旦找到有效的配置项，立即返回并停止查找
                foreach (var section in allKeys.Select(replacementSource.GetSection).Where(section => section.Exists()))
                {
                    return section.Value!;
                }

                // 检查是否提供了默认值
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    return defaultValue;
                }

                return hasQuestionMark ? string.Empty : match.Value;
            });
    }

    /// <summary>
    ///     转换输入字符串中的任何转义字符
    /// </summary>
    /// <param name="input">
    ///     <see cref="string" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? Unescape([NotNullIfNotNull(nameof(input))] this string? input) =>
        string.IsNullOrWhiteSpace(input) ? input : Regex.Unescape(input);

    /// <summary>
    ///     验证字符串是否是 <c>application/x-www-form-urlencoded</c> 格式
    /// </summary>
    /// <param name="output">字符串</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static bool IsUrlEncodedFormFormat(this string output)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(output);

        return UrlEncodedFormFormatRegex().IsMatch(output);
    }

    /// <summary>
    ///     将 <c>application/x-www-form-urlencoded</c> 格式的字符串解析为 <see cref="JsonObject" />
    /// </summary>
    /// <param name="formData">URL 编码的表单数据字符串</param>
    /// <returns>
    ///     <see cref="JsonObject" />
    /// </returns>
    internal static JsonObject? ParseUrlEncodedFormToJsonObject(this string? formData)
    {
        // 尝试移除开头的 ?
        formData = formData?.TrimStart('?');

        // 空检查
        if (string.IsNullOrWhiteSpace(formData))
        {
            return null;
        }

        // 初始化 JsonObject 实例
        var root = new JsonObject();

        // 按 & 分割每个键值对
        foreach (var part in formData.Split('&'))
        {
            // 查找第一个 =
            var eqIndex = part.IndexOf('=');

            // 键名为空或不存在 = 则跳过
            if (eqIndex <= 0)
            {
                continue;
            }

            // URL 解码键和值
            var key = WebUtility.UrlDecode(part[..eqIndex]);
            var value = WebUtility.UrlDecode(part[(eqIndex + 1)..]);

            // 将键名（如 user[0][name]）拆分为 token：["user", "0", "name"]
            var tokens = key.Replace("]", "").Split('[');
            var current = root;
            var i = 0;

            // 逐层构建嵌套结构
            while (i < tokens.Length)
            {
                // 空检查
                var token = tokens[i];
                if (string.IsNullOrEmpty(token))
                {
                    i++;
                    continue;
                }

                // 检查是否是最后一项
                if (i == tokens.Length - 1)
                {
                    current[token] = value;
                    break;
                }

                // 下一项为数组索引，按数组处理
                var nextToken = tokens[i + 1];
                if (int.TryParse(nextToken, out var index) && index >= 0)
                {
                    // 确保当前 token 是一个 JsonArray
                    if (current[token] is not JsonArray array)
                    {
                        array = [];
                        current[token] = array;
                    }

                    // 扩容数组
                    while (array.Count <= index)
                    {
                        array.Add(new JsonObject());
                    }

                    // 进入该数组元素并跳过数组名和索引
                    current = (JsonObject)array[index]!;
                    i += 2;
                }
                else
                {
                    // 下一个 token 是普通属性名则视为对象名
                    if (current[token] is not JsonObject child)
                    {
                        child = new JsonObject();
                        current[token] = child;
                    }

                    current = child;
                    i++;
                }
            }
        }

        return root;
    }

    /// <summary>
    ///     从字典中按路径逐级解析值
    /// </summary>
    /// <param name="path">占位符路径</param>
    /// <param name="replacementSource">
    ///     <see cref="IDictionary{TKey,TValue}" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ResolveNestedPathFromDictionary(string path, IDictionary<string, string?> replacementSource)
    {
        var index = 0;

        // 读取路径的第一个标识符
        var firstKey = ReadIdentifier(path, ref index);

        // 检查读取失败，或字典中不存在该键，或值为空
        if (firstKey is null || !replacementSource.TryGetValue(firstKey, out var current) || current is null)
        {
            return null;
        }

        // 逐段解析后续属性或索引
        while (index < path.Length)
        {
            // 跳过空白字符
            while (index < path.Length && char.IsWhiteSpace(path[index]))
            {
                index++;
            }

            if (index >= path.Length)
            {
                break;
            }

            if (path[index] == '.')
            {
                index++;

                // 读取属性名
                var prop = ReadIdentifier(path, ref index);

                // 空检查
                if (prop is null)
                {
                    return null;
                }

                // 尝试将字符串解析为 JsonNode
                var node = TryParseJsonNode(current);

                // 检查是否是 JsonObject
                if (node is JsonObject jsonObject && jsonObject.TryGetPropertyValue(prop, out var propNode))
                {
                    current = NodeToRawString(propNode);
                }
                else
                {
                    return null;
                }
            }
            else if (path[index] == '[')
            {
                index++;

                // 读取索引值
                var arrIndex = ReadInteger(path, ref index);

                // 小于 0 检查
                if (arrIndex < 0)
                {
                    return null;
                }

                // 跳过数字后可能存在的空格
                while (index < path.Length && char.IsWhiteSpace(path[index]))
                {
                    index++;
                }

                // 检查索引后面是否跟着 ']'
                if (index >= path.Length || path[index] != ']')
                {
                    return null;
                }

                index++;

                // 尝试将字符串解析为 JsonNode
                var node = TryParseJsonNode(current);

                // 检查是否是 JsonArray 且索引不越界
                if (node is JsonArray jsonArray && arrIndex < jsonArray.Count)
                {
                    current = NodeToRawString(jsonArray[arrIndex]);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                break;
            }
        }

        return current;
    }

    /// <summary>
    ///     从对象中按路径逐级解析值
    /// </summary>
    /// <param name="source">源对象</param>
    /// <param name="path">占位符路径</param>
    /// <param name="isMatch">是否成功匹配</param>
    /// <param name="prefix">路径前缀</param>
    /// <param name="bindingFlags">
    ///     <see cref="BindingFlags" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    internal static object? ResolveNestedPathFromObject(this object? source, string path, out bool isMatch,
        string prefix, BindingFlags bindingFlags)
    {
        isMatch = false;

        // 空检查
        if (source is null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var index = 0;

        // 处理前缀
        if (!string.IsNullOrEmpty(prefix))
        {
            // 读取路径的第一个标识符
            var first = ReadIdentifier(path, ref index);

            // 检查是否第一个标识符不存在，或者与 prefix 不匹配
            if (first is null || !string.Equals(first, prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // 检查是否路径只有前缀
            if (index >= path.Length)
            {
                isMatch = true;
                return source;
            }
        }

        var current = source;

        // 处理后续的每一段
        while (index < path.Length)
        {
            // 跳过空白字符
            while (index < path.Length && char.IsWhiteSpace(path[index]))
            {
                index++;
            }

            if (index >= path.Length)
            {
                break;
            }

            // 空检查
            if (current is null)
            {
                return null;
            }

            if (path[index] == '.')
            {
                index++;

                // 读取属性名
                var propName = ReadIdentifier(path, ref index);

                // 空检查
                if (propName is null)
                {
                    return null;
                }

                // 获取属性信息（忽略大小写）
                var prop = current.GetType().GetProperty(propName, bindingFlags) ?? current.GetType()
                    .GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                if (prop is not null)
                {
                    // 读取属性值
                    current = prop.GetValue(current);
                }
                else
                {
                    // 通过字符串键从字典中获取值
                    var dictValue = GetDictionaryValue(current, propName);

                    // 空检查
                    if (dictValue is null)
                    {
                        return null;
                    }

                    current = dictValue;
                }
            }
            else if (path[index] == '[')
            {
                index++;

                var arrIndex = ReadInteger(path, ref index);

                // 小于 0 检查
                if (arrIndex < 0)
                {
                    return null;
                }

                // 跳过空白字符
                while (index < path.Length && char.IsWhiteSpace(path[index]))
                {
                    index++;
                }

                if (index >= path.Length || path[index] != ']')
                {
                    return null;
                }

                index++;

                // 通过索引访问集合元素
                current = GetIndexValue(current, arrIndex);

                // 空检查
                if (current is null)
                {
                    return null;
                }
            }
            else
            {
                break;
            }
        }

        isMatch = true;
        return current;
    }

    /// <summary>
    ///     读取路径的第一个标识符
    /// </summary>
    /// <param name="path">路径字符串</param>
    /// <param name="index">当前读取位置，读取后指向标识符末尾</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ReadIdentifier(string path, ref int index)
    {
        // 跳过空白字符
        while (index < path.Length && char.IsWhiteSpace(path[index]))
        {
            index++;
        }

        // 首字符必须是字母、数字或下划线
        if (index >= path.Length || (!char.IsLetterOrDigit(path[index]) && path[index] != '_'))
        {
            return null;
        }

        var start = index;

        // 读取连续的有效字符
        while (index < path.Length && (char.IsLetterOrDigit(path[index]) || path[index] == '_'))
        {
            index++;
        }

        return path[start..index];
    }

    /// <summary>
    ///     从 <paramref name="path" /> 的当前位置读取一个非负整数
    /// </summary>
    /// <remarks>失败返回 -1。</remarks>
    /// <param name="path">路径字符串</param>
    /// <param name="index">当前读取位置，读取后指向数字末尾</param>
    /// <returns>
    ///     <see cref="int" />
    /// </returns>
    internal static int ReadInteger(string path, ref int index)
    {
        // 跳过空白字符
        while (index < path.Length && char.IsWhiteSpace(path[index]))
        {
            index++;
        }

        // 首字符必须是数字
        if (index >= path.Length || !char.IsDigit(path[index]))
        {
            return -1;
        }

        var value = 0;

        // 逐位累加
        while (index < path.Length && char.IsDigit(path[index]))
        {
            value = (value * 10) + (path[index] - '0');
            index++;
        }

        return value;
    }

    /// <summary>
    ///     尝试将字符串解析为 <see cref="JsonNode" />
    /// </summary>
    /// <param name="value">JSON 字符串</param>
    /// <returns>
    ///     <see cref="JsonNode" />
    /// </returns>
    internal static JsonNode? TryParseJsonNode(string? value)
    {
        // 空检查
        if (value is null)
        {
            return null;
        }

        try
        {
            // 尝试解析 JSON 字符串
            return JsonNode.Parse(value);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     将 <see cref="JsonNode" /> 转换为字符串
    /// </summary>
    /// <param name="node">
    ///     <see cref="JsonNode" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? NodeToRawString(JsonNode? node) =>
        node switch
        {
            null => null,
            JsonValue jsonValue when jsonValue.GetValueKind() == JsonValueKind.String => jsonValue.GetValue<string>(),
            JsonValue jsonValue => jsonValue.ToString(),
            _ => node.ToJsonString()
        };

    /// <summary>
    ///     通过索引访问集合元素
    /// </summary>
    /// <remarks>支持按整数索引获取集合中的元素。</remarks>
    /// <param name="source">集合对象</param>
    /// <param name="index">索引位置</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    internal static object? GetIndexValue(object source, int index) =>
        source switch
        {
            IList list when index >= 0 && index < list.Count => list[index],
            Array array when index >= 0 && index < array.Length => array.GetValue(index),
            IEnumerable enumerable => enumerable.Cast<object?>().ElementAtOrDefault(index),
            _ => null
        };

    /// <summary>
    ///     通过字符串键从字典中获取值
    /// </summary>
    /// <param name="source">字典对象</param>
    /// <param name="key">字符串键</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    internal static object? GetDictionaryValue(object source, string key)
    {
        // 处理非泛型 IDictionary（如 Hashtable）类型
        if (source is IDictionary dict && dict.Contains(key))
        {
            return dict[key];
        }

        var type = source.GetType();

        // 反射获取泛型字典（如 Dictionary<string, T>）的 ContainsKey 方法
        var containsKeyMethod = type.GetMethod("ContainsKey", [typeof(string)]);

        // 调用 ContainsKey 方法检查
        if (containsKeyMethod is not null && !(bool)containsKeyMethod.Invoke(source, [key])!)
        {
            return null;
        }

        // 尝试获取泛型字典 Item 属性（索引器）
        var indexerProperty = type.GetProperty("Item", [typeof(string)]);

        // 空检查
        if (indexerProperty is null)
        {
            return null;
        }

        try
        {
            return indexerProperty.GetValue(source, [key]);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     占位符匹配正则表达式
    /// </summary>
    /// <remarks>
    ///     占位符格式：<c>{Key}</c> 或 <c>{Key.Property}</c> 或 <c>{Key.Property.NestProperty}</c> 或 <c>{list[0]}</c> 或
    ///     <c>{user.names[0]}</c>。结尾可添加 <c>?</c> 表示值不存在时替换为空字符串，例如 <c>{Key?}</c>。占位符开头可添加 <c>**</c> 前缀，该前缀在解析时会被忽略，例如
    ///     <c>{**Key}</c> 等同于 <c>{Key}</c>。
    /// </remarks>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(@"\{\s*(?:\*\*)?\s*(\w+(?:\s*\.\s*\w+|\s*\[\s*\d+\s*\])*)\s*(\?)?\s*\}")]
    private static partial Regex PlaceholderRegex();

    /// <summary>
    ///     配置键匹配正则表达式
    /// </summary>
    /// <remarks>
    ///     占位符格式：<c>[[Key]]</c> 或 <c>[[Key:Sub]]</c> 或 <c>[[Key:Sub:Nest]]</c> 或 <c>[[Key | Key2 | Key3]]</c> 或
    ///     <c>[[Key | Key2 || 默认值]]</c>。结尾可添加 <c>?</c> 表示所有键都不存在且无默认值时替换为空字符串，
    ///     例如 <c>[[Key?]]</c>。占位符开头可添加 <c>**</c> 前缀，该前缀在解析时会被忽略，例如 <c>[[**Key]]</c> 等同于 <c>[[Key]]</c>。
    /// </remarks>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(@"\[\[\s*(?:\*\*)?\s*([\w\-:]+)((?:\s*\|\s*[\w\-:]+)*)\s*(?:\|\|\s*([^\]]*))?\s*(\?)?\s*\]\]")]
    private static partial Regex ConfigurationKeyRegex();

    /// <summary>
    ///     URL 编码格式验证正则表达式
    /// </summary>
    [GeneratedRegex(
        "^(?:(?:[a-zA-Z0-9-._~]|%[0-9A-Fa-f]{2})+=(?:[a-zA-Z0-9-._~+]|%[0-9A-Fa-f]{2})*)(?:&(?:[a-zA-Z0-9-._~]|%[0-9A-Fa-f]{2})+=(?:[a-zA-Z0-9-._~+]|%[0-9A-Fa-f]{2})*)*$",
        RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex UrlEncodedFormFormatRegex();
}