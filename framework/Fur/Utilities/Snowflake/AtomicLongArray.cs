using Fur.DependencyInjection;
using System.Collections.Generic;
using System.Threading;

namespace Fur.Utilities
{
    /// <summary>
    /// 线程安全的长整形数组
    /// </summary>
    [SkipScan]
    public class AtomicLongArray
    {
        /// <summary>
        /// 长整形数组
        /// </summary>
        private readonly long[] _value;

        /// <summary>
        /// 获取一个只读的长整形数组集合
        /// </summary>
        public IReadOnlyCollection<long> Value => _value;

        /// <summary>
        /// 获取数组下标的值
        /// </summary>
        /// <param name="index">数组下标</param>
        /// <returns>数组下标的值</returns>
        public long this[int index] => _value[index];

        /// <summary>
        /// 实例化一个线程安全的长整形数组
        /// </summary>
        /// <param name="capacity">数组的容量</param>
        public AtomicLongArray(int capacity) => _value = new long[capacity];

        /// <summary>
        /// 替换数组中指定下标的值，如果下标的当前值与期望的值相等，则替换成功；不相等说明发生了并发冲突，则替换失败
        /// </summary>
        /// <param name="index">数组下标</param>
        /// <param name="currentValue">期望的值</param>
        /// <param name="newValue">新的值</param>
        /// <returns>是否替换成功</returns>
        public bool CompareAndSet(int index, long currentValue, long newValue)
        {
            return Interlocked.CompareExchange(ref _value[index], newValue, currentValue) == currentValue;
        }
    }
}