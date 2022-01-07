// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.JsonSerialization;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace System.Text.Json
{
    /// <summary>
    /// System.Text.Json 拓展
    /// </summary>
    [SuppressSniffer]
    public static class SystemTextJsonExtensions
    {
        /// <summary>
        /// 添加时间格式化
        /// </summary>
        /// <param name="converters"></param>
        /// <param name="formatString"></param>
        /// <param name="outputToLocalDateTime">自动转换 DateTimeOffset 为当地时间</param>
        public static void AddDateFormatString(this IList<JsonConverter> converters, string formatString, bool outputToLocalDateTime = false)
        {
            converters.Add(new DateTimeJsonConverter(formatString));
            converters.Add(new DateTimeOffsetJsonConverter(formatString, outputToLocalDateTime));
        }
    }
}