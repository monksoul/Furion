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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable

using static System.Reflection.AsyncDispatchProxyGenerator;

namespace System.Reflection;

public abstract class DispatchProxyAsync
{
    /// <summary>
    ///     创建指定接口的代理实例
    /// </summary>
    /// <typeparam name="T">要代理的接口类型</typeparam>
    /// <typeparam name="TProxy">
    ///     <see cref="DispatchProxyAsync" />
    /// </typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    public static T Create<T, TProxy>() where TProxy : DispatchProxyAsync =>
        (T)CreateProxyInstance(typeof(TProxy), typeof(T));

    /// <summary>
    ///     创建指定接口的代理实例
    /// </summary>
    /// <param name="type">要代理的接口类型</param>
    /// <param name="proxyType">
    ///     <see cref="DispatchProxyAsync" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public static object Create(Type type, Type proxyType) =>
        CreateProxyInstance(proxyType, type);

    /// <summary>
    ///     同步方法调用入口
    /// </summary>
    /// <param name="method">被调用的方法信息</param>
    /// <param name="args">方法参数数组</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public abstract object Invoke(MethodInfo method, object[] args);

    /// <summary>
    ///     返回 <see cref="Task" /> 的异步方法调用入口
    /// </summary>
    /// <param name="method">被调用的方法信息</param>
    /// <param name="args">方法参数数组</param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    public abstract Task InvokeAsync(MethodInfo method, object[] args);

    /// <summary>
    ///     返回 <see cref="Task{TResult}" /> 的异步方法调用入口
    /// </summary>
    /// <typeparam name="T">异步操作的返回值类型</typeparam>
    /// <param name="method">被调用的方法信息</param>
    /// <param name="args">方法参数数组</param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    public abstract Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args);

    /// <summary>
    ///     返回 <see cref="ValueTask" /> 的异步方法调用入口
    /// </summary>
    /// <param name="method">被调用的方法信息</param>
    /// <param name="args">方法参数数组</param>
    /// <returns>
    ///     <see cref="ValueTask" />
    /// </returns>
    public abstract ValueTask InvokeValueTaskAsync(MethodInfo method, object[] args);

    /// <summary>
    ///     返回 <see cref="ValueTask{TResult}" /> 的异步方法调用入口
    /// </summary>
    /// <typeparam name="T">异步操作的返回值类型</typeparam>
    /// <param name="method">被调用的方法信息</param>
    /// <param name="args">方法参数数组</param>
    /// <returns>
    ///     <see cref="ValueTask{TResult}" />
    /// </returns>
    public abstract ValueTask<T> InvokeValueTaskAsyncT<T>(MethodInfo method, object[] args);
}