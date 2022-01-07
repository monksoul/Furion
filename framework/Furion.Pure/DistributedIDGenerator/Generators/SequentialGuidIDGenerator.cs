// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.Security.Cryptography;

namespace Furion.DistributedIDGenerator
{
    /// <summary>
    /// 连续 GUID ID 生成器
    /// <para>代码参考自：https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/blob/ebe011a6f1b2a2a9709fe558cfc7ed3215b55c37/src/EFCore.MySql/ValueGeneration/Internal/MySqlSequentialGuidValueGenerator.cs </para>
    /// </summary>
    public class SequentialGuidIDGenerator : IDistributedIDGenerator, ISingleton
    {
        /// <summary>
        /// 随机数生成器
        /// </summary>
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        /// <summary>
        /// 生成逻辑
        /// </summary>
        /// <param name="idGeneratorOptions"></param>
        /// <returns></returns>
        public object Create(object idGeneratorOptions = null)
        {
            // According to RFC 4122:
            // dddddddd-dddd-Mddd-Ndrr-rrrrrrrrrrrr
            // - M = RFC version, in this case '4' for random UUID
            // - N = RFC variant (plus other bits), in this case 0b1000 for variant 1
            // - d = nibbles based on UTC date/time in ticks
            // - r = nibbles based on random bytes

            var options = idGeneratorOptions as SequentialGuidSettings;

            var randomBytes = new byte[7];
            _rng.GetBytes(randomBytes);
            var ticks = (ulong)(options?.TimeNow == null ? DateTimeOffset.UtcNow : options.TimeNow.Value).Ticks;

            var uuidVersion = (ushort)4;
            var uuidVariant = (ushort)0b1000;

            var ticksAndVersion = (ushort)((ticks << 48 >> 52) | (ushort)(uuidVersion << 12));
            var ticksAndVariant = (byte)((ticks << 60 >> 60) | (byte)(uuidVariant << 4));

            if (options?.LittleEndianBinary16Format == true)
            {
                var guidBytes = new byte[16];
                var tickBytes = BitConverter.GetBytes(ticks);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(tickBytes);
                }

                Buffer.BlockCopy(tickBytes, 0, guidBytes, 0, 6);
                guidBytes[6] = (byte)(ticksAndVersion << 8 >> 8);
                guidBytes[7] = (byte)(ticksAndVersion >> 8);
                guidBytes[8] = ticksAndVariant;
                Buffer.BlockCopy(randomBytes, 0, guidBytes, 9, 7);

                return new Guid(guidBytes);
            }

            var guid = new Guid((uint)(ticks >> 32), (ushort)(ticks << 32 >> 48), ticksAndVersion,
                ticksAndVariant,
                randomBytes[0],
                randomBytes[1],
                randomBytes[2],
                randomBytes[3],
                randomBytes[4],
                randomBytes[5],
                randomBytes[6]);

            return guid;
        }
    }
}