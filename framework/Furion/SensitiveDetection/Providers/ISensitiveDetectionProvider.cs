// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Furion.SensitiveDetection
{
    /// <summary>
    /// 脱敏词汇（脱敏）提供器
    /// </summary>
    public interface ISensitiveDetectionProvider
    {
        /// <summary>
        /// 返回所有脱敏词汇
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetWordsAsync();

        /// <summary>
        /// 判断脱敏词汇是否有效（支持自定义算法）
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        Task<bool> VaildedAsync(string text);

        /// <summary>
        /// 替换敏感词汇
        /// </summary>
        /// <param name="text"></param>
        /// <param name="transfer"></param>
        /// <returns></returns>
        Task<string> ReplaceAsync(string text, char transfer = '*');
    }
}