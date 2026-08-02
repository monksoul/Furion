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

namespace Furion.Utilities;

/// <summary>
///     并发执行实用方法
/// </summary>
public static class ParallelUtility
{
    /// <summary>
    ///     默认的最大并发数
    /// </summary>
    internal const int DefaultMaxDegreeOfParallelism = 4;

    /// <summary>
    ///     对集合中的每个元素并发执行异步操作
    /// </summary>
    /// <typeparam name="T">集合元素的类型</typeparam>
    /// <param name="source">要遍历的集合</param>
    /// <param name="action">要对每个元素执行的异步操作，接收元素和取消令牌</param>
    /// <param name="maxDegreeOfParallelism">最大并发数，默认值为：4</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Task ForEachAsync<T>(IEnumerable<T> source, Func<T, CancellationToken, Task> action,
        int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        // 有效性检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        // 初始化 ParallelOptions 实例
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = cancellationToken
        };

        // 并发执行
        return Parallel.ForEachAsync(source, parallelOptions,
            async (item, token) => await action(item, token));
    }

    /// <summary>
    ///     对集合中的每个元素并发执行异步操作（带返回值）
    /// </summary>
    /// <typeparam name="T">集合元素的类型</typeparam>
    /// <typeparam name="TResult">返回结果的类型</typeparam>
    /// <param name="source">要遍历的集合</param>
    /// <param name="action">要对每个元素执行的异步操作，接收元素和取消令牌，返回结果</param>
    /// <param name="maxDegreeOfParallelism">最大并发数，默认值为：4</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns><typeparamref name="TResult" /> 集合</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TResult[]> ForEachAsync<T, TResult>(IEnumerable<T> source,
        Func<T, CancellationToken, Task<TResult>> action, int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        // 有效性检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        // 将集合转为列表以获取索引
        var itemList = source.ToList();

        // 初始化结果数组（按原始顺序）
        var results = new TResult[itemList.Count];

        // 初始化 ParallelOptions 实例
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = cancellationToken
        };

        // 并发执行，通过索引保证结果顺序
        await Parallel.ForEachAsync(itemList.Select((item, index) => (item, index)), parallelOptions,
            async (entry, token) => { results[entry.index] = await action(entry.item, token); });

        return results;
    }

    /// <summary>
    ///     并发执行多个不同的异步操作
    /// </summary>
    /// <param name="operations">要并发执行的异步操作集合，每个操作接收取消令牌</param>
    /// <param name="maxDegreeOfParallelism">最大并发数，默认值为：4</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Task RunAsync(IEnumerable<Func<CancellationToken, Task>> operations,
        int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operations);

        // 有效性检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        // 初始化 ParallelOptions 实例
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = cancellationToken
        };

        // 并发执行
        return Parallel.ForEachAsync(operations, parallelOptions,
            async (operation, token) => await operation(token));
    }

    /// <summary>
    ///     并发执行多个不同的异步操作
    /// </summary>
    /// <remarks>最大并发数等于操作数量。</remarks>
    /// <param name="operations">要并发执行的异步操作，每个操作接收取消令牌</param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task RunAsync(params Func<CancellationToken, Task>[] operations)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operations);

        // 使用操作数量作为最大并发数
        return RunAsync(operations, operations.Length);
    }

    /// <summary>
    ///     并发执行多个不同的异步操作（带返回值）
    /// </summary>
    /// <typeparam name="TResult">返回结果的类型</typeparam>
    /// <param name="operations">要并发执行的异步操作集合，每个操作接收取消令牌并返回结果</param>
    /// <param name="maxDegreeOfParallelism">最大并发数，默认值为：4</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns><typeparamref name="TResult" /> 集合</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TResult[]> RunAsync<TResult>(
        IEnumerable<Func<CancellationToken, Task<TResult>>> operations,
        int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operations);

        // 有效性检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        // 将操作转为列表以获取索引
        var operationList = operations.ToList();

        // 初始化结果数组（按原始顺序）
        var results = new TResult[operationList.Count];

        // 初始化 ParallelOptions 实例
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = cancellationToken
        };

        // 并发执行，通过索引保证结果顺序
        await Parallel.ForEachAsync(operationList.Select((op, index) => (op, index)), parallelOptions,
            async (entry, token) => { results[entry.index] = await entry.op(token); });

        return results;
    }

    /// <summary>
    ///     并发执行多个不同的异步操作（带返回值）
    /// </summary>
    /// <remarks>最大并发数等于操作数量。</remarks>
    /// <typeparam name="TResult">返回结果的类型</typeparam>
    /// <param name="operations">要并发执行的异步操作，每个操作接收取消令牌并返回结果</param>
    /// <returns><typeparamref name="TResult" /> 集合</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Task<TResult[]> RunAsync<TResult>(params Func<CancellationToken, Task<TResult>>[] operations)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operations);

        // 使用操作数量作为最大并发数
        return RunAsync(operations, operations.Length);
    }

    /// <summary>
    ///     对集合中的每个元素并发执行同步操作
    /// </summary>
    /// <typeparam name="T">集合元素的类型</typeparam>
    /// <param name="source">要遍历的集合</param>
    /// <param name="action">要对每个元素执行的操作</param>
    /// <param name="maxDegreeOfParallelism">最大并发数，默认值为：4</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void ForEach<T>(IEnumerable<T> source, Action<T> action,
        int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        // 有效性检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        // 初始化 ParallelOptions 实例
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

        // 并发执行
        Parallel.ForEach(source, parallelOptions, action);
    }

    /// <summary>
    ///     对集合中的每个元素并发执行同步操作（带返回值）
    /// </summary>
    /// <typeparam name="T">集合元素的类型</typeparam>
    /// <typeparam name="TResult">返回结果的类型</typeparam>
    /// <param name="source">要遍历的集合</param>
    /// <param name="action">要对每个元素执行的操作，返回结果</param>
    /// <param name="maxDegreeOfParallelism">最大并发数，默认值为：4</param>
    /// <returns><typeparamref name="TResult" /> 集合</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static TResult[] ForEach<T, TResult>(IEnumerable<T> source, Func<T, TResult> action,
        int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        // 有效性检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        // 将集合转为列表以获取索引
        var itemList = source.ToList();

        // 初始化结果数组（按原始顺序）
        var results = new TResult[itemList.Count];

        // 初始化 ParallelOptions 实例
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

        // 并发执行，通过索引保证结果顺序
        Parallel.ForEach(itemList.Select((item, index) => (item, index)), parallelOptions,
            entry => { results[entry.index] = action(entry.item); });

        return results;
    }

    /// <summary>
    ///     并发执行多个不同的同步操作
    /// </summary>
    /// <param name="operations">要并发执行的操作集合</param>
    /// <param name="maxDegreeOfParallelism">最大并发数，默认值为：4</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void Run(IEnumerable<Action> operations,
        int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operations);

        // 有效性检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        // 初始化 ParallelOptions 实例
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

        // 并发执行
        Parallel.ForEach(operations, parallelOptions, operation => operation());
    }

    /// <summary>
    ///     并发执行多个不同的同步操作
    /// </summary>
    /// <remarks>最大并发数等于操作数量。</remarks>
    /// <param name="operations">要并发执行的操作</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void Run(params Action[] operations)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operations);

        // 使用操作数量作为最大并发数
        Run(operations, operations.Length);
    }

    /// <summary>
    ///     并发执行多个不同的同步操作（带返回值）
    /// </summary>
    /// <typeparam name="TResult">返回结果的类型</typeparam>
    /// <param name="operations">要并发执行的操作集合，每个操作返回结果</param>
    /// <param name="maxDegreeOfParallelism">最大并发数，默认值为：4</param>
    /// <returns><typeparamref name="TResult" /> 集合</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static TResult[] Run<TResult>(IEnumerable<Func<TResult>> operations,
        int maxDegreeOfParallelism = DefaultMaxDegreeOfParallelism)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operations);

        // 有效性检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        // 将操作转为列表以获取索引
        var operationList = operations.ToList();

        // 初始化结果数组（按原始顺序）
        var results = new TResult[operationList.Count];

        // 初始化 ParallelOptions 实例
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

        // 并发执行，通过索引保证结果顺序
        Parallel.ForEach(operationList.Select((op, index) => (op, index)), parallelOptions,
            entry => { results[entry.index] = entry.op(); });

        return results;
    }

    /// <summary>
    ///     并发执行多个不同的同步操作（带返回值）
    /// </summary>
    /// <remarks>最大并发数等于操作数量。</remarks>
    /// <typeparam name="TResult">返回结果的类型</typeparam>
    /// <param name="operations">要并发执行的操作，每个操作返回结果</param>
    /// <returns><typeparamref name="TResult" /> 集合</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TResult[] Run<TResult>(params Func<TResult>[] operations)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operations);

        // 使用操作数量作为最大并发数
        return Run(operations, operations.Length);
    }
}