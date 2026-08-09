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
using System.Text;

namespace Furion.Utilities;

/// <summary>
///     提供字符串实用方法
/// </summary>
public static class StringUtility
{
    /// <summary>
    ///     格式化键值集合摘要
    /// </summary>
    /// <param name="keyValues">键值集合</param>
    /// <param name="summary">摘要</param>
    /// <param name="skipEmptyValues">是否跳过值为空的项，默认值为：<c>false</c></param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string? FormatKeyValuesSummary(IEnumerable<KeyValuePair<string, IEnumerable<string>>> keyValues,
        string? summary = null, bool skipEmptyValues = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(keyValues);

        // 获取键值集合数量
        var keyValuePairs = keyValues as KeyValuePair<string, IEnumerable<string>>[] ?? keyValues.ToArray();
        var count = keyValuePairs.Length;

        // 空检查
        if (count == 0)
        {
            return null;
        }

        // 注册 CodePagesEncodingProvider，使得程序能够识别并使用 Windows 代码页中的各种编码
        EncodingUtility.Initialize();

        // 获取最长键名长度用于对齐键名字符串
        var totalByteCount = keyValuePairs.Max(h => Encoding.Default.GetByteCount((h.Key ?? string.Empty) + ":")) + 5;

        // 初始化 StringBuilder 实例
        var stringBuilder = new StringBuilder();

        // 检查是否设置了摘要
        var hasSummary = !string.IsNullOrWhiteSpace(summary);

        // 用于控制只在有输出内容时才添加换行
        var hasOutput = false;

        // 逐条构建摘要信息
        foreach (var (key, value) in keyValuePairs)
        {
            // 获取前缀
            var linePrefix = hasSummary ? "  " : string.Empty;

            string? formatValue;
            string lineContent;

            // 处理值列表为空问题
            var joinedValue = value is null ? string.Empty : string.Join(", ", value);

            // 处理键名不为空
            if (!string.IsNullOrWhiteSpace(key))
            {
                // 获取格式化后的键名
                var keyPart = (key + ':').PadStringToByteLength(totalByteCount);

                // 获取键名在控制台中的实际显示宽度
                var keyPartWidth = GetDisplayWidth(keyPart);

                // 生成缩进
                var indent = new string(' ', linePrefix.Length + keyPartWidth + 1);
                formatValue = AddTabToEachLine(joinedValue, true, indent);

                lineContent = $"{linePrefix}{keyPart} {formatValue}";
            }
            // 处理键名为空
            else
            {
                // 生成缩进
                var indent = new string(' ', linePrefix.Length);
                formatValue = AddTabToEachLine(joinedValue, true, indent);

                // 存在换行的值拼接前缀
                lineContent = $"{linePrefix}{formatValue}";
            }

            // 检查是否跳过值为空的项
            if (skipEmptyValues && string.IsNullOrWhiteSpace(formatValue))
            {
                continue;
            }

            // 非首条输出前添加换行
            if (hasOutput)
            {
                stringBuilder.Append(Environment.NewLine);
            }

            hasOutput = true;
            stringBuilder.Append(lineContent);
        }

        // 如果没有任何输出项，直接返回 null
        if (!hasOutput)
        {
            return null;
        }

        // 获取字符串
        var formatString = stringBuilder.ToString();

        return hasSummary ? $"\e[36m\e[1m{summary}:\e[0m {Environment.NewLine}{formatString}" : formatString;
    }

    /// <summary>
    ///     在字符串每一行添加制表符（两个空白）或自定义缩进
    /// </summary>
    /// <param name="input">文本</param>
    /// <param name="skipFirstLine">是否跳过第一行</param>
    /// <param name="indent">自定义缩进字符串，为空则默认两个空格</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? AddTabToEachLine(string? input, bool skipFirstLine = false, string? indent = null)
    {
        // 空检查
        if (input is null)
        {
            return input;
        }

        // 默认缩进为两个空格
        indent ??= "  ";

        // 使用 Environment.NewLine 以确保跨平台兼容性
        return string.Join(Environment.NewLine, input.Split([Environment.NewLine, "\n"], StringSplitOptions.None)
            .Select((line, i) => (skipFirstLine && i == 0 ? string.Empty : indent) + line));
    }

    /// <summary>
    ///     获取字符串在控制台中的实际显示宽度
    /// </summary>
    /// <remarks>中文/全角字符占2个宽度，英文/半角占1个宽度。</remarks>
    /// <param name="text">要测量的文本</param>
    /// <returns>
    ///     <see cref="int" />
    /// </returns>
    internal static int GetDisplayWidth(string? text)
    {
        // 空检查
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var width = 0;
        foreach (var c in text)
        {
            // CJK 统一汉字、全角标点、全角字母/数字/符号
            if ((c >= 0x4E00 && c <= 0x9FA5) || // CJK 统一汉字
                (c >= 0x3000 && c <= 0x303F) || // CJK 标点符号
                (c >= 0xFF00 && c <= 0xFFEF)) // 全角 ASCII 变体
            {
                width += 2;
            }
            else
            {
                width += 1;
            }
        }

        return width;
    }
}