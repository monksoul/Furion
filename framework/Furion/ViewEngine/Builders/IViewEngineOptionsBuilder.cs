// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.CodeAnalysis;
using System;
using System.Reflection;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 视图引擎选项构建器
    /// </summary>
    public interface IViewEngineOptionsBuilder
    {
        /// <summary>
        /// 视图编译选项
        /// </summary>
        ViewEngineOptions Options { get; set; }

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