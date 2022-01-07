// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

namespace Furion.DistributedIDGenerator
{
    /// <summary>
    /// 短 ID 约束
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// 最小长度
        /// </summary>
        public const int MinimumAutoLength = 8;

        /// <summary>
        /// 最大长度
        /// </summary>
        public const int MaximumAutoLength = 14;

        /// <summary>
        /// 最小可选字符长度
        /// </summary>
        public const int MinimumCharacterSetLength = 50;
    }
}