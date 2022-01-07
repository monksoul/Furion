// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 字符串模板执行部件
    /// </summary>
    [SuppressSniffer]
    public sealed partial class ViewEnginePart
    {
        /// <summary>
        /// 静态缺省 视图 部件
        /// </summary>
        public static ViewEnginePart Default => new();

        /// <summary>
        /// 字符串模板
        /// </summary>
        public string Template { get; private set; }

        /// <summary>
        /// 视图配置选项
        /// </summary>
        public Action<IViewEngineOptionsBuilder> TemplateOptionsBuilder { get; private set; }

        /// <summary>
        /// 模型数据
        /// </summary>
        public (Type Type, object Model) TemplateModel { get; private set; } = (typeof(object), default);

        /// <summary>
        /// 模板缓存名称（不含拓展名）
        /// </summary>
        public string TemplateCachedFileName { get; private set; }

        /// <summary>
        /// 视图模板服务作用域
        /// </summary>
        public IServiceProvider ViewEngineScoped { get; private set; }
    }
}