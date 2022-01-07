// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;

namespace Furion.DistributedIDGenerator
{
    /// <summary>
    /// 随机数帮助类
    /// </summary>
    internal static class RandomHelpers
    {
        /// <summary>
        /// 随机数对象
        /// </summary>
        private static readonly Random Random = new();

        /// <summary>
        /// 线程锁
        /// </summary>
        private static readonly object ThreadLock = new();

        /// <summary>
        /// 生成线程安全的范围内随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GenerateNumberInRange(int min, int max)
        {
            lock (ThreadLock)
            {
                return Random.Next(min, max);
            }
        }
    }
}