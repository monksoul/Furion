// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

using System.Security.Cryptography;

namespace Furion.DistributedIDGenerator;

/// <summary>
/// 连续 GUID ID 生成器
/// <para>代码参考自：https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/blob/ebe011a6f1b2a2a9709fe558cfc7ed3215b55c37/src/EFCore.MySql/ValueGeneration/Internal/MySqlSequentialGuidValueGenerator.cs </para>
/// </summary>
public class SequentialGuidIDGenerator : IDistributedIDGenerator
{
    /// <summary>
    /// 随机数生成器
    /// </summary>
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    private static long _lastTimestamp = DateTime.UtcNow.Ticks;
    private static int _counter;

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

        var timestamp = (options?.TimeNow ?? DateTimeOffset.UtcNow).Ticks;

        timestamp = EnsureMonotonicTimestamp(timestamp);

        var ticks = (ulong)timestamp;

        var randomBytes = new byte[9];
        _rng.GetBytes(randomBytes);

        const ushort uuidVersion = 4;
        const ushort uuidVariant = 0b1000;

        var ticksPart3 = (uint)((ticks >> 4) & 0x0FFFUL);
        var ticksAndVersion = (ticksPart3 & 0x0FFFU) | ((uint)uuidVersion << 12);

        var ticksPart4 = (uint)(ticks & 0x0FUL);
        var ticksAndVariant = (ticksPart4 & 0x0FU) | ((uint)uuidVariant << 4);

        if (options?.LittleEndianBinary16Format == true)
        {
            var guidBytes = new byte[16];

            for (var i = 0; i < 6; i++)
            {
                guidBytes[i] = (byte)(ticks >> (40 - i * 8));
            }

            guidBytes[6] = (byte)(((uint)uuidVersion << 4) | (uint)((ticks >> 4) & 0x0FUL));
            guidBytes[7] = (byte)(((uint)(ticks & 0x0FUL) << 4) | (uint)(randomBytes[0] & 0x0F));
            guidBytes[8] = (byte)(((uint)uuidVariant << 4) | (uint)(randomBytes[1] & 0x0F));
            Buffer.BlockCopy(randomBytes, 2, guidBytes, 9, 7);

            return new Guid(guidBytes);
        }

        var guid = new Guid(
            (uint)(ticks >> 32),
            (ushort)(ticks << 32 >> 48),
            (ushort)ticksAndVersion,
            (byte)ticksAndVariant,
            randomBytes[0],
            randomBytes[1],
            randomBytes[2],
            randomBytes[3],
            randomBytes[4],
            randomBytes[5],
            randomBytes[6]);

        return guid;
    }

    /// <summary>
    /// 确保时间戳单调递增
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    private static long EnsureMonotonicTimestamp(long timestamp)
    {
        while (true)
        {
            var original = Interlocked.Read(ref _lastTimestamp);
            var newValue = Math.Max(timestamp, original + 1);

            if (Interlocked.CompareExchange(ref _lastTimestamp, newValue, original) == original)
            {
                if (newValue > timestamp)
                {
                    Interlocked.Increment(ref _counter);
                }
                return newValue;
            }
        }
    }
}