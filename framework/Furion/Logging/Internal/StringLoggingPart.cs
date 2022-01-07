// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Furion.Logging
{
    /// <summary>
    /// 构建字符串日志部分类
    /// </summary>
    [SuppressSniffer]
    public sealed partial class StringLoggingPart
    {
        /// <summary>
        /// 静态缺省日志部件
        /// </summary>
        public static StringLoggingPart Default => new();

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel Level { get; private set; } = LogLevel.Information;

        /// <summary>
        /// 消息格式化参数
        /// </summary>
        public object[] Args { get; private set; }

        /// <summary>
        /// 事件 Id
        /// </summary>
        public EventId? EventId { get; private set; }

        /// <summary>
        /// 日志分类类型（从依赖注入中解析）
        /// </summary>
        public Type CategoryType { get; private set; } = typeof(System.Running.Logging);

        /// <summary>
        /// 日志分类名（总是创建新的实例）
        /// </summary>
        public string CategoryName { get; private set; }

        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// 日志对象所在作用域
        /// </summary>
        public IServiceProvider LoggerScoped { get; private set; }
    }
}