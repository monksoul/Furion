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

using Microsoft.CodeAnalysis;
using System.Text;

namespace Furion.ViewEngine;

/// <summary>
/// 视图引擎模板编译异常类
/// </summary>
public class ViewEngineTemplateException : ViewEngineException
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ViewEngineTemplateException()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ViewEngineTemplateException(Exception innerException)
        : base(null, innerException)
    {
    }

    /// <summary>
    /// 编译错误诊断列表
    /// </summary>
    public List<Diagnostic> Errors { get; set; }

    /// <summary>
    /// 生成的源代码
    /// </summary>
    public string GeneratedCode { get; set; }

    /// <summary>
    /// 错误行上下文显示配置
    /// </summary>
    internal int ContextLines { get; set; } = 3;

    /// <summary>
    /// 缓存的消息
    /// </summary>
    private string _cachedMessage;

    /// <summary>
    /// 重写异常消息
    /// </summary>
    public override string Message
    {
        get
        {
            if (_cachedMessage != null) return _cachedMessage;

            var sb = new StringBuilder("Unable to compile template:");
            var errors = Errors?.Where(e => e.Severity == DiagnosticSeverity.Error || e.IsWarningAsError)
                                  .OrderBy(e => e.Location.GetLineSpan().StartLinePosition.Line)   // 按行号排序
                                  .ToList();

            if (errors == null || errors.Count == 0)
            {
                sb.AppendLine().Append("  No errors available.");
                return _cachedMessage = sb.ToString();
            }

            var lines = !string.IsNullOrEmpty(GeneratedCode)
                ? GeneratedCode.Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
                : null;

            var lastContextStart = -1;
            var lastContextEnd = -1;

            foreach (var error in errors)
            {
                var loc = error.Location.GetLineSpan();
                var line = loc.StartLinePosition.Line + 1;
                var col = loc.StartLinePosition.Character + 1;

                sb.AppendLine().AppendFormat("  [{0}] ({1},{2}): {3}", error.Id, line, col, error.GetMessage());

                // 显示错误代码上下文
                if (lines != null)
                {
                    var start = Math.Max(0, line - 1 - ContextLines);
                    var end = Math.Min(lines.Length - 1, line - 1 + ContextLines);

                    if (start == lastContextStart && end == lastContextEnd)
                        continue;

                    var pad = end.ToString().Length;

                    sb.AppendLine().AppendLine("  Code Context:");
                    for (var i = start; i <= end; i++)
                    {
                        var ln = i + 1;
                        var marker = ln == line ? ">>> " : "    ";
                        var code = lines[i];
                        if (code.Length > 120) code = code[..120] + "...";
                        sb.AppendFormat("{0}{1} | {2}{3}", marker, ln.ToString().PadLeft(pad), code, Environment.NewLine);
                    }

                    lastContextStart = start;
                    lastContextEnd = end;
                }
            }

            return _cachedMessage = sb.ToString();
        }
    }
}