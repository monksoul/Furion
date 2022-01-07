// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 视图引擎编译选项
    /// </summary>
    [SuppressSniffer]
    public class ViewEngineOptions
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ViewEngineOptions()
        {
            ReferencedAssemblies = new HashSet<Assembly>()
            {
                typeof(object).Assembly,
                typeof(ViewEngineModel).Assembly,
                typeof(System.Collections.IList).Assembly,
                typeof(IEnumerable<>).Assembly,
                Reflect.GetAssembly("Microsoft.CSharp"),
                Reflect.GetAssembly("System.Runtime"),
                Reflect.GetAssembly("System.Linq"),
                Reflect.GetAssembly("System.Linq.Expressions")
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
        public string TemplateNamespace { get; set; } = "Furion.ViewEngine";

        /// <summary>
        /// 继承
        /// </summary>
        public string Inherits { get; set; } = "Furion.ViewEngine.Template.Models";

        /// <summary>
        /// 默认 Using
        /// </summary>
        public HashSet<string> DefaultUsings { get; set; } = new HashSet<string>()
        {
            "System.Linq",
            "System.Collections",
            "System.Collections.Generic"
        };
    }
}