// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2.
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
// See the Mulan PSL v2 for more details.

using Furion.ClayObject.Extensions;
using Furion.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace System.Net.Http
{
    /// <summary>
    /// HttpRequestMessage 拓展
    /// </summary>
    [SuppressSniffer]
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// 追加查询参数
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="queries"></param>
        /// <param name="isEncode"></param>
        public static void AppendQueries(this HttpRequestMessage httpRequest, IDictionary<string, object> queries, bool isEncode = true)
        {
            if (queries == null || queries.Count == 0) return;

            // 获取原始地址
            var finalRequestUrl = httpRequest.RequestUri.OriginalString;

            // 拼接
            var urlParameters = queries.Where(u => u.Value != null)
                            .Select(u => $"{u.Key}={(isEncode ? Uri.EscapeDataString(u.Value?.ToString() ?? string.Empty) : (u.Value?.ToString() ?? string.Empty))}");
            finalRequestUrl += $"{(finalRequestUrl.IndexOf("?") > -1 ? "&" : "?")}{string.Join("&", urlParameters)}";

            // 重新设置地址
            httpRequest.RequestUri = new Uri(finalRequestUrl, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// 追加查询参数
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="queries"></param>
        /// <param name="isEncode"></param>
        public static void AppendQueries(this HttpRequestMessage httpRequest, object queries, bool isEncode = true)
        {
            if (queries == null) return;

            httpRequest.AppendQueries(queries.ToDictionary(), isEncode);
        }
    }
}