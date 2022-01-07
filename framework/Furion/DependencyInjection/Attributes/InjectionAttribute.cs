// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;

namespace Furion.DependencyInjection
{
    /// <summary>
    /// 设置依赖注入方式
    /// </summary>
    [SuppressSniffer, AttributeUsage(AttributeTargets.Class)]
    public class InjectionAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptInterfaces"></param>
        public InjectionAttribute(params Type[] exceptInterfaces)
            : this(InjectionActions.Add, exceptInterfaces)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="exceptInterfaces"></param>
        public InjectionAttribute(InjectionActions action, params Type[] exceptInterfaces)
        {
            Action = action;
            Pattern = InjectionPatterns.All;
            ExceptInterfaces = exceptInterfaces ?? Array.Empty<Type>();
            Order = 0;
        }

        /// <summary>
        /// 添加服务方式，存在不添加，或继续添加
        /// </summary>
        public InjectionActions Action { get; set; }

        /// <summary>
        /// 注册选项
        /// </summary>
        public InjectionPatterns Pattern { get; set; }

        /// <summary>
        /// 注册别名
        /// </summary>
        /// <remarks>多服务时使用</remarks>
        public string Named { get; set; }

        /// <summary>
        /// 排序，排序越大，则在后面注册
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 排除接口
        /// </summary>
        public Type[] ExceptInterfaces { get; set; }

        /// <summary>
        /// 代理类型，必须继承 DispatchProxy、IDispatchProxy
        /// </summary>
        public Type Proxy { get; set; }
    }
}