using Fur.DependencyInjection;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图编译构建器
    /// </summary>
    [SkipScan]
    public class ViewEngineCompilationOptionsBuilder : IViewEngineCompilationOptionsBuilder
    {
        /// <summary>
        /// 视图编译选项
        /// </summary>
        public ViewEngineCompilationOptions Options { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public ViewEngineCompilationOptionsBuilder(ViewEngineCompilationOptions options = null)
        {
            Options = options ?? new ViewEngineCompilationOptions();
        }

        /// <summary>
        /// 添加程序集引用
        /// </summary>
        /// <param name="assemblyName"></param>
        public void AddAssemblyReferenceByName(string assemblyName)
        {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            AddAssemblyReference(assembly);
        }

        /// <summary>
        /// 添加程序集引用
        /// </summary>
        /// <param name="assembly"></param>
        public void AddAssemblyReference(Assembly assembly)
        {
            Options.ReferencedAssemblies.Add(assembly);
        }

        /// <summary>
        /// 添加程序集引用
        /// </summary>
        /// <param name="type"></param>
        public void AddAssemblyReference(Type type)
        {
            AddAssemblyReference(type.Assembly);

            foreach (var argumentType in type.GenericTypeArguments)
            {
                AddAssemblyReference(argumentType);
            }
        }

        /// <summary>
        /// 添加元数据引用
        /// </summary>
        /// <param name="reference"></param>
        public void AddMetadataReference(MetadataReference reference)
        {
            Options.MetadataReferences.Add(reference);
        }

        /// <summary>
        /// 添加 Using
        /// </summary>
        /// <param name="namespaceName"></param>
        public void AddUsing(string namespaceName)
        {
            Options.DefaultUsings.Add(namespaceName);
        }

        /// <summary>
        /// 添加继承类型
        /// </summary>
        /// <param name="type"></param>
        public void Inherits(Type type)
        {
            Options.Inherits = RenderTypeName(type);
            AddAssemblyReference(type);
        }

        /// <summary>
        /// 渲染类型名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string RenderTypeName(Type type)
        {
            var result = type.Namespace + "." + type.Name;

            if (result.Contains('`'))
            {
                result = result.Substring(0, result.IndexOf("`"));
            }

            if (type.GenericTypeArguments.Length == 0)
            {
                return result;
            }

            return result + "<" + string.Join(",", type.GenericTypeArguments.Select(RenderTypeName)) + ">";
        }
    }
}