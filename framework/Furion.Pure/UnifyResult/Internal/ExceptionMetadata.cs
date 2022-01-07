// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;

namespace Furion.UnifyResult.Internal
{
    /// <summary>
    /// 异常元数据
    /// </summary>
    [SuppressSniffer]
    public sealed class ExceptionMetadata
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; internal set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public object ErrorCode { get; internal set; }

        /// <summary>
        /// 错误对象（信息）
        /// </summary>
        public object Errors { get; internal set; }
    }
}