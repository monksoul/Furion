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

using System.Runtime.Loader;

namespace Furion.ViewEngine;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 模板类型全名
    /// </summary>
    internal const string TemplateTypeName = "Furion.ViewEngine.Template";

    /// <summary>
    /// 从程序集字节中加载模板类型
    /// </summary>
    /// <param name="assemblyBytes"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static (Type Type, AssemblyLoadContext Context) LoadTemplateType(byte[] assemblyBytes)
    {
        var alc = new AssemblyLoadContext(TemplateTypeName, isCollectible: true);

        using var ms = new MemoryStream(assemblyBytes);
        var assembly = alc.LoadFromStream(ms);

        var type = assembly.GetType(TemplateTypeName)
            ?? throw new InvalidOperationException("Template type not found in compiled assembly.");

        return (type, alc);
    }

    /// <summary>
    /// 从文件加载模板
    /// </summary>
    internal static IViewEngineTemplate LoadTemplateFromFileSafely(string templatePath)
    {
        var bytes = File.ReadAllBytes(templatePath);

        var (_, alc) = LoadTemplateType(bytes);
        alc.Unload();

        return new ViewEngineTemplate(bytes, TemplateTypeName, templatePath);
    }

    /// <summary>
    /// 从文件加载模板
    /// </summary>
    internal static IViewEngineTemplate<T> LoadTemplateFromFileSafely<T>(string templatePath) where T : IViewEngineModel
    {
        var bytes = File.ReadAllBytes(templatePath);

        var (_, alc) = LoadTemplateType(bytes);
        alc.Unload();

        return new ViewEngineTemplate<T>(bytes, TemplateTypeName, templatePath);
    }

    /// <summary>
    /// 写入模板文件
    /// </summary>
    internal static void SaveTemplateAtomically(string templatePath, IViewEngineTemplate template)
    {
        var tempPath = templatePath + ".tmp";
        using (var stream = File.Create(tempPath))
        {
            template.SaveToStream(stream);
        }
        File.Move(tempPath, templatePath, overwrite: true);
    }

    /// <summary>
    /// 写入模板文件
    /// </summary>
    internal static void SaveTemplateAtomically<T>(string templatePath, IViewEngineTemplate<T> template) where T : IViewEngineModel
    {
        var tempPath = templatePath + ".tmp";
        using (var stream = File.Create(tempPath))
        {
            template.SaveToStream(stream);
        }
        File.Move(tempPath, templatePath, overwrite: true);
    }
}