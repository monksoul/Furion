// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;

namespace Furion.DistributedIDGenerator
{
    /// <summary>
    /// 短 ID 生成配置选项
    /// </summary>
    [SuppressSniffer]
    public class GenerationOptions
    {
        /// <summary>
        /// 是否使用数字
        /// <para>默认 false</para>
        /// </summary>
        public bool UseNumbers { get; set; }

        /// <summary>
        /// 是否使用特殊字符
        /// <para>默认 true</para>
        /// </summary>
        public bool UseSpecialCharacters { get; set; } = true;

        /// <summary>
        /// 设置短 ID 长度
        /// </summary>
        public int Length { get; set; } = RandomHelpers.GenerateNumberInRange(Constants.MinimumAutoLength, Constants.MaximumAutoLength);
    }
}