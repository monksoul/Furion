// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System.Reflection;
using System.Threading.Tasks;

namespace Furion.Reflection
{
    /// <summary>
    /// 异步代理分发类
    /// </summary>
    public abstract class AspectDispatchProxy
    {
        /// <summary>
        /// 创建代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProxy"></typeparam>
        /// <returns></returns>
        public static T Create<T, TProxy>() where TProxy : AspectDispatchProxy
        {
            return (T)AspectDispatchProxyGenerator.CreateProxyInstance(typeof(TProxy), typeof(T));
        }

        /// <summary>
        /// 执行同步代理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract object Invoke(MethodInfo method, object[] args);

        /// <summary>
        /// 执行异步代理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract Task InvokeAsync(MethodInfo method, object[] args);

        /// <summary>
        /// 执行异步返回 Task{T} 代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args);
    }
}