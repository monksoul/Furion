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
        var totalByteCount = keyValuePairs.Max(h => h.Key.Length) + 5;

        // 初始化 StringBuilder 实例
        var stringBuilder = new StringBuilder();

        // 检查是否设置了摘要
        var hasSummary = !string.IsNullOrWhiteSpace(summary);

        // 用于控制只在有输出内容时才添加换行
        var hasOutput = false;

        // 逐条构建摘要信息
        foreach (var (key, value) in keyValuePairs)
        {
            // 获取格式化后的值
            var formatValue = AddTabToEachLine(string.Join(", ", value), true);

            // 检查是否跳过值为空的项
            if (skipEmptyValues && string.IsNullOrWhiteSpace(formatValue))
            {
                continue;
            }

            // 非首条输出前添加换行
            if (hasOutput)
            {
                stringBuilder.Append("\r\n");
            }

            hasOutput = true;

            // 检查是否包含摘要，如果有则添加制表符（两个空白）
            if (hasSummary)
            {
                stringBuilder.Append("  ");
            }

            // 处理空 Key 问题
            if (!string.IsNullOrWhiteSpace(key))
            {
                stringBuilder.Append($"{(key + ':').PadStringToByteLength(totalByteCount)} {formatValue}");
            }
            else
            {
                stringBuilder.Append($"{string.Join(", ", formatValue)}");
            }
        }

        // 如果没有任何输出项，直接返回 null
        if (!hasOutput)
        {
            return null;
        }

        // 获取字符串
        var formatString = stringBuilder.ToString();

        return hasSummary ? $"\e[36m\e[1m{summary}:\e[0m \r\n{formatString}" : formatString;
    }

    /// <summary>
    ///     在字符串每一行添加制表符（两个空白）
    /// </summary>
    /// <param name="input">文本</param>
    /// <param name="skipFirstLine">是否跳过第一行</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? AddTabToEachLine(string? input, bool skipFirstLine = false)
    {
        // 空检查
        if (input is null)
        {
            return input;
        }

        // 使用 Environment.NewLine 以确保跨平台兼容性
        return string.Join(Environment.NewLine, input.Split([Environment.NewLine, "\n"], StringSplitOptions.None)
            .Select((line, i) => (skipFirstLine && i == 0 ? string.Empty : "  ") + line));
    }
}