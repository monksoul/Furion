using Fur.DependencyInjection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图编译异常类
    /// </summary>
    [SkipScan]
    public class ViewEngineCompilationException : ViewEngineException
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ViewEngineCompilationException()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ViewEngineCompilationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public ViewEngineCompilationException(string message) : base(message)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ViewEngineCompilationException(string message, Exception innerException) : base(message, innerException)
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