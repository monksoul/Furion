// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

namespace Furion.DependencyInjection
{
    /// <summary>
    /// 外部注册类型模型
    /// </summary>
    [SuppressSniffer]
    public sealed class ExternalService
    {
        /// <summary>
        /// 接口类型，格式："程序集名称;接口完整名称"
        /// </summary>
        public string Interface { get; set; }

        /// <summary>
        /// 实例类型，格式："程序集名称;接口完整名称"
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// 注册类型
        /// </summary>
        public RegisterType RegisterType { get; set; }

        /// <summary>
        /// 添加服务方式，存在不添加，或继续添加
        /// </summary>
        public InjectionActions Action { get; set; } = InjectionActions.Add;

        /// <summary>
        /// 注册选项
        /// </summary>
        public InjectionPatterns Pattern { get; set; } = InjectionPatterns.All;

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
        /// 代理类型，格式："程序集名称;接口完整名称"
        /// </summary>
        public string Proxy { get; set; }
    }
}