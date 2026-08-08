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

using System.Text;

namespace Furion.HttpRemote;

/// <summary>
///     cURL 命令词法分析器
/// </summary>
internal static class HttpCurlTokenizer
{
    /// <summary>
    ///     将 cURL 命令字符串拆分为 Token 集合
    /// </summary>
    /// <param name="curlCommand">cURL 命令字符串</param>
    /// <returns><see cref="string" /> 集合</returns>
    /// <exception cref="ArgumentException"></exception>
    internal static List<string> Tokenize(string curlCommand)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(curlCommand);

        var tokens = new List<string>();
        var currentToken = new StringBuilder();

        // 初始化单双引号标记
        var inSingleQuote = false;
        var inDoubleQuote = false;

        var i = 0;
        var length = curlCommand.Length;

        // 游标式解析
        while (i < length)
        {
            var ch = curlCommand[i];

            switch (ch)
            {
                // 处理转义字符
                case '\\' when i + 1 < length:
                    {
                        var nextCh = curlCommand[i + 1];

                        // 换行续行符（Linux/Mac）
                        if (nextCh is '\n' or '\r')
                        {
                            i++;
                            if (nextCh == '\r' && i + 1 < length && curlCommand[i + 1] == '\n')
                            {
                                i++;
                            }

                            i++;
                            continue;
                        }

                        if (inDoubleQuote && nextCh == '"')
                        {
                            currentToken.Append('"');
                            i += 2;
                            continue;
                        }

                        if (inSingleQuote && nextCh == '\'')
                        {
                            currentToken.Append('\'');
                            i += 2;
                            continue;
                        }

                        if (nextCh == '\\')
                        {
                            currentToken.Append('\\');
                            i += 2;
                            continue;
                        }

                        currentToken.Append(ch);
                        i++;
                        continue;
                    }
                // 处理 Windows CMD 换行续行符 ^
                case '^' when i + 1 < length && (curlCommand[i + 1] == '\n' || curlCommand[i + 1] == '\r'):
                    {
                        i++;
                        if (i < length && curlCommand[i] == '\n')
                        {
                            i++;
                        }

                        i++;
                        continue;
                    }
                // 处理引号切换
                case '\'' when !inDoubleQuote:
                    inSingleQuote = !inSingleQuote;
                    i++;
                    continue;
                case '"' when !inSingleQuote:
                    inDoubleQuote = !inDoubleQuote;
                    i++;
                    continue;
                // 处理空格分隔（仅在引号外）
                case ' ' or '\t' when !inSingleQuote && !inDoubleQuote:
                // 跳过换行符（引号外）
                case '\n' or '\r' when !inSingleQuote && !inDoubleQuote:
                    {
                        if (currentToken.Length > 0)
                        {
                            tokens.Add(currentToken.ToString());
                            currentToken.Clear();
                        }

                        i++;
                        continue;
                    }
                default:
                    // 普通字符
                    currentToken.Append(ch);
                    i++;
                    break;
            }
        }

        // 检查引号是否闭合
        if (inSingleQuote || inDoubleQuote)
        {
            throw new ArgumentException("Unterminated quote in cURL command.");
        }

        // 添加最后一个 Token
        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens;
    }
}