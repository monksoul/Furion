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
using System.Diagnostics.CodeAnalysis;

namespace Furion.Utilities;

/// <summary>
///     提供文件实用方法
/// </summary>
public static class FileUtility
{
    /// <summary>
    ///     尝试验证文件扩展名
    /// </summary>
    /// <remarks>特别说明：不支持扩展名中包含通配符，如 <c>*</c>。</remarks>
    /// <param name="fileName">文件的名称</param>
    /// <param name="allowedFileExtensions">允许的文件扩展名字符串，用分号分隔</param>
    /// <param name="validFileExtensions">有效的文件扩展名集合</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool TryValidateExtension(string fileName, [NotNullWhen(false)] string? allowedFileExtensions,
        [NotNullWhen(false)] out HashSet<string>? validFileExtensions)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        // 初始化 out 返回值
        validFileExtensions = null;

        return string.IsNullOrWhiteSpace(allowedFileExtensions) || TryValidateExtension(fileName,
            allowedFileExtensions.Split(';', StringSplitOptions.RemoveEmptyEntries), out validFileExtensions);
    }

    /// <summary>
    ///     尝试验证文件扩展名
    /// </summary>
    /// <remarks>特别说明：不支持扩展名中包含通配符，如 <c>*</c>。</remarks>
    /// <param name="fileName">文件的名称</param>
    /// <param name="allowedFileExtensions">允许的文件扩展名数组</param>
    /// <param name="validFileExtensions">有效的文件扩展名集合</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public static bool TryValidateExtension(string fileName, [NotNullWhen(false)] string[]? allowedFileExtensions,
        [NotNullWhen(false)] out HashSet<string>? validFileExtensions)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        // 初始化 out 返回值
        validFileExtensions = null;

        // 空检查
        if (allowedFileExtensions is null or { Length: 0 })
        {
            return true;
        }

        // 获取有效的文件扩展名集合
        validFileExtensions = GetValidFileExtensions(allowedFileExtensions);

        // 获取文件扩展名
        var extension = Path.GetExtension(fileName);

        return validFileExtensions.Contains(extension);
    }

    /// <summary>
    ///     验证文件扩展名
    /// </summary>
    /// <remarks>特别说明：不支持扩展名中包含通配符，如 <c>*</c>。</remarks>
    /// <param name="fileName">文件的名称</param>
    /// <param name="allowedFileExtensions">允许的文件扩展名字符串，用分号分隔</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void ValidateExtension(string fileName, string? allowedFileExtensions) =>
        ValidateExtension(fileName,
            (allowedFileExtensions ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries));

    /// <summary>
    ///     验证文件扩展名
    /// </summary>
    /// <remarks>特别说明：不支持扩展名中包含通配符，如 <c>*</c>。</remarks>
    /// <param name="fileName">文件的名称</param>
    /// <param name="allowedFileExtensions">允许的文件扩展名数组</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void ValidateExtension(string fileName, string[]? allowedFileExtensions)
    {
        if (!TryValidateExtension(fileName, allowedFileExtensions, out var validFileExtensions))
        {
            throw new InvalidOperationException(
                $"The file type is not allowed. Only the following file types are supported: `{string.Join(", ", validFileExtensions)}`.");
        }
    }

    /// <summary>
    ///     尝试验证文件大小
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="maxFileSizeInBytes">允许的文件大小。以字节为单位。</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool TryValidateSize(string filePath, long maxFileSizeInBytes)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        // 小于或等于 0 检查
        if (maxFileSizeInBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFileSizeInBytes),
                "Max file size in bytes must be greater than zero.");
        }

        // 初始化 FileInfo 实例
        var fileInfo = new FileInfo(filePath);

        return fileInfo.Length <= maxFileSizeInBytes;
    }

    /// <summary>
    ///     验证文件大小
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="maxFileSizeInBytes">允许的文件大小。以字节为单位。</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void ValidateSize(string filePath, long maxFileSizeInBytes)
    {
        var unit = maxFileSizeInBytes < 1024 ? "KB" : "MB";

        if (!TryValidateSize(filePath, maxFileSizeInBytes))
        {
            throw new InvalidOperationException(
                $"The file size exceeds the maximum allowed size of `{maxFileSizeInBytes.ToSizeUnits(unit):F2} {unit}`.");
        }
    }

    /// <summary>
    ///     获取有效的文件扩展名集合
    /// </summary>
    /// <param name="allowedFileExtensions">允许的文件扩展名数组</param>
    /// <returns>
    ///     <see cref="HashSet{T}" />
    /// </returns>
    internal static HashSet<string> GetValidFileExtensions(string[] allowedFileExtensions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(allowedFileExtensions);

        // 初始化 HashSet<string> 实例
        var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 逐条添加到 HashSet<string> 中
        foreach (var extension in allowedFileExtensions)
        {
            // 空检查
            if (!string.IsNullOrWhiteSpace(extension))
            {
                // 确保扩展名以 . 开头
                hashSet.Add('.' + extension.TrimStart('.'));
            }
        }

        return hashSet;
    }
}