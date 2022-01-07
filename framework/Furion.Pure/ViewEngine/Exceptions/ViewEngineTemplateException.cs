// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 视图引擎模板编译异常类
    /// </summary>
    [SuppressSniffer]
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
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ViewEngineTemplateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="innerException"></param>
        public ViewEngineTemplateException(Exception innerException) : base(null, innerException)
        {
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public List<Diagnostic> Errors { get; set; }

        /// <summary>
        /// 生成的代码
        /// </summary>
        public string GeneratedCode { get; set; }

        /// <summary>
        /// 重写异常消息
        /// </summary>
        public override string Message => $"Unable to compile template: {string.Join("\n", Errors.Where(w => w.IsWarningAsError || w.Severity == DiagnosticSeverity.Error))}";
    }
}