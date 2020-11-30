using Fur.DependencyInjection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图编译选项
    /// </summary>
    [SkipScan]
    public class ViewEngineCompilationOptions
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ViewEngineCompilationOptions()
        {
            ReferencedAssemblies = new HashSet<Assembly>()
            {
                typeof(object).Assembly,
                Assembly.Load(new AssemblyName("Microsoft.CSharp")),
                typeof(ViewEngineTemplate).Assembly,
                Assembly.Load(new AssemblyName("System.Runtime")),
                Assembly.Load(new AssemblyName("System.Linq")),
                Assembly.Load(new AssemblyName("System.Linq.Expressions")),
                Assembly.Load(new AssemblyName("netstandard"))
            };
        }

        /// <summary>
        /// 引用程序集
        /// </summary>
        public HashSet<Assembly> ReferencedAssemblies { get; set; }

        /// <summary>
        /// 元数据引用
        /// </summary>
        public HashSet<MetadataReference> MetadataReferences { get; set; } = new HashSet<MetadataReference>();

        /// <summary>
        /// 模板命名空间
        /// </summary>
        public string TemplateNamespace { get; set; } = "TemplateNamespace";

        /// <summary>
        /// 继承
        /// </summary>
        public string Inherits { get; set; } = "Fur.ViewEngine.ViewEngineTemplate";

        /// <summary>
        /// 默认 Using
        /// </summary>
        public HashSet<string> DefaultUsings { get; set; } = new HashSet<string>()
        {
            "System.Linq"
        };
    }
}