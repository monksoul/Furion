using Fur.DependencyInjection;
using System;

namespace Fur.Utilities
{
    /// <summary>
    /// 雪花算法
    /// </summary>
    [SkipScan]
    public class Snowflake
    {
        /// <summary>
        /// 机房标识位
        /// </summary>
        private const int DATACENTER_ID_BITS = 5;

        /// <summary>
        /// 机器标识位
        /// </summary>
        private const int WORKER_ID_BITS = 5;

        /// <summary>
        /// 序列号标识位
        /// </summary>
        private const int SEQUENCE_BITS = 12;

        /// <summary>
        /// 最大机房ID = 32
        /// </summary>
        private const int MAX_DATACENTER_ID = -1 ^ -1 << DATACENTER_ID_BITS;

        /// <summary>
        /// 最大机器ID = 32
        /// </summary>
        private const int MAX_WORKER_ID = -1 ^ -1 << WORKER_ID_BITS;

        /// <summary>
        /// 最大序列号 = 4096（单节点每毫秒可产生的最大ID数）
        /// </summary>
        private const int SEQUENCE_MASK = -1 ^ -1 << SEQUENCE_BITS;

        /// <summary>
        /// 机器ID左位移长度 = 12
        /// </summary>
        private const int WORKER_ID_SHIFT_COUNT = SEQUENCE_BITS;

        /// <summary>
        /// 机房ID左位移长度 = 17
        /// </summary>
        private const int DATACENTER_ID_SHIFT_COUNT = WORKER_ID_BITS + SEQUENCE_BITS;

        /// <summary>
        /// 时间戳左位移长度 = 22
        /// </summary>
        private const int TIMESTAMP_SHIFT_COUNT = DATACENTER_ID_BITS + WORKER_ID_BITS + SEQUENCE_BITS;

        /// <summary>
        /// 历史ID存储数组长度
        /// </summary>
        private const int CAPACITY = 1000;

        /// <summary>
        /// 历史ID存储数组，该数组为解决时钟回拨而设计，如果历史ID反推出的时间戳大于当前时间戳，说明发生了时钟回拨，此时采用历史ID+1的方式生成新ID，直到时间追赶至回拨前的时间点
        /// </summary>
        private readonly AtomicLongArray _idCycle = new(CAPACITY);

        /// <summary>
        /// 基准时间
        /// </summary>
        private readonly DateTime START_TIME = new(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 机房ID
        /// </summary>
        public long DataCenterId { get; private set; }

        /// <summary>
        /// 机器ID
        /// </summary>
        public long WorkerId { get; private set; }

        /// <summary>
        /// 当前时间戳 = 当前时间 - 基础时间
        /// </summary>
        public long CurrentTimestamp => (long)(DateTime.UtcNow - START_TIME).TotalMilliseconds;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataCenterId"></param>
        /// <param name="workerId"></param>
        public Snowflake(long dataCenterId, long workerId)
        {
            DataCenterId = dataCenterId;
            WorkerId = workerId;

            if (DataCenterId < 0 || DataCenterId > MAX_DATACENTER_ID)
                throw new ArgumentException("DataCenterId");

            if (WorkerId < 0 || WorkerId > MAX_WORKER_ID)
                throw new ArgumentException("WorkerId");
        }

        /// <summary>
        /// 获取雪花Id
        /// </summary>
        /// <returns></returns>
        public long GetId()
        {
            do
            {
                var timestamp = CurrentTimestamp;

                // 计算槽位下标
                var index = (int)(timestamp % CAPACITY);

                // 通过槽位下标获取此时间戳的历史数据，如果 historyId 为 0 说明此时间戳并没有生成过 id
                var historyId = _idCycle[index];
                var historyTimestamp = (historyId >> TIMESTAMP_SHIFT_COUNT);

                // 如果此时间戳没有生成过 id，或时间戳小于当前时间，认为需要设置新的时间戳
                if (historyId == 0 || historyTimestamp < timestamp)
                {
                    var id = (timestamp << TIMESTAMP_SHIFT_COUNT)
                           | (DataCenterId << DATACENTER_ID_SHIFT_COUNT)
                           | (WorkerId << WORKER_ID_SHIFT_COUNT);

                    if (_idCycle.CompareAndSet(index, historyId, id))
                        return id;
                }

                // 如果 historyTimestamp 大于 timestamp 则表示发生了时间回退；
                // 如果 historyTimestamp 等于 timestamp 则表示在同一个时间戳正常的生成 id;
                // 上面两种情况都采用 historyId + 1 的方式
                if (historyTimestamp >= timestamp)
                {
                    var sequence = historyId & SEQUENCE_MASK;

                    // 该时间戳生成的 id 数超过上限
                    if (sequence >= SEQUENCE_MASK)
                        continue;

                    var id = historyId + 1;
                    if (_idCycle.CompareAndSet(index, historyId, id))
                        return id;
                }
            }
            while (true);
        }
    }
}