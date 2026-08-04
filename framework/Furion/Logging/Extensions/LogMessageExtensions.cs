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
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Furion.Logging;

/// <summary>
/// <see cref="LogMessage"/> 扩展
/// </summary>
public static class LogMessageExtensions
{
    /// <summary>
    /// 高性能创建 JSON 对象字符串
    /// </summary>
    /// <param name="_"><see cref="LogMessage"/></param>
    /// <param name="writeAction"></param>
    /// <param name="writeIndented">是否对 JSON 格式化</param>
    /// <returns><see cref="string"/></returns>
    public static string Write(this LogMessage _, Action<Utf8JsonWriter> writeAction, bool writeIndented = false)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            // 解决中文乱码问题
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = writeIndented
        });

        writeAction?.Invoke(writer);

        writer.Flush();

        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }

    /// <summary>
    /// 高性能创建 JSON 数组字符串
    /// </summary>
    /// <param name="logMsg"><see cref="LogMessage"/></param>
    /// <param name="writeAction"></param>
    /// <param name="writeIndented">是否对 JSON 格式化</param>
    /// <returns><see cref="string"/></returns>
    public static string WriteArray(this LogMessage logMsg, Action<Utf8JsonWriter> writeAction, bool writeIndented = false)
    {
        return logMsg.Write(writer =>
        {
            writer.WriteStartArray();

            writeAction?.Invoke(writer);

            writer.WriteEndArray();
        }, writeIndented);
    }
}