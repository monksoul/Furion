using Fur.DependencyInjection;
using System.IO;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 常量、公共方法配置类
    /// </summary>
    [SkipScan]
    internal static class Penetrates
    {
        /// <summary>
        /// 模板保存目录
        /// </summary>
        internal static string TemplateSaveDir = "templates";

        /// <summary>
        /// 获取模板文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string GetTemplateFileName(string fileName)
        {
            if (!Directory.Exists(TemplateSaveDir)) Directory.CreateDirectory(TemplateSaveDir);

            if (!fileName.EndsWith(".dll", System.StringComparison.OrdinalIgnoreCase)) fileName += ".dll";
            var templatePath = Path.Combine(TemplateSaveDir, "~" + fileName);

            return templatePath;
        }
    }
}