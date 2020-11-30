using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图编译构建器
    /// </summary>
    public interface IViewEngineCompilationOptionsBuilder
    {
        /// <summary>
        /// 视图编译选项
        /// </summary>
        ViewEngineCompilationOptions Options { get; set; }

        /// <summary>
        /// 添加程序集引用
        /// </summary>
        /// <param name="assemblyName"></param>
        void AddAssemblyReferenceByName(string assemblyName);

        /// <summary>
        /// 添加程序集引用
        /// </summary>
        /// <param name="assembly"></param>
        void AddAssemblyReference(Assembly assembly);

        /// <summary>
        /// 添加程序集引用
        /// </summary>
        /// <param name="type"></param>
        void AddAssemblyReference(Type type);

        /// <summary>
        /// 添加元数据引用
        /// </summary>
        /// <param name="reference"></param>
        void AddMetadataReference(MetadataReference reference);

        /// <summary>
        /// 添加 Using
        /// </summary>
        /// <param name="namespaceName"></param>
        void AddUsing(string namespaceName);

        /// <summary>
        /// 添加继承类型
        /// </summary>
        /// <param name="type"></param>
        void Inherits(Type type);
    }
}