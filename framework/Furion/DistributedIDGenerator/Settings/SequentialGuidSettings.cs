// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.DistributedIDGenerator
{
    /// <summary>
    /// 连续 GUID 配置
    /// </summary>
    [SuppressSniffer]
    public sealed class SequentialGuidSettings
    {
        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTimeOffset? TimeNow { get; set; }

        /// <summary>
        /// LittleEndianBinary 16 格式化
        /// </summary>
        public bool LittleEndianBinary16Format { get; set; }
    }
}