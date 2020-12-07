using Fur.DependencyInjection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图引擎模板编译异常类
    /// </summary>
    [SkipScan]
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
        /// <param name="message"></param>
        public ViewEngineTemplateException(string message) : base(message)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ViewEngineTemplateException(string message, Exception innerException) : base(message, innerException)
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
    }
}