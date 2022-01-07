// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 字符串模板执行部件
    /// </summary>
    public sealed partial class ViewEnginePart
    {
        /// <summary>
        /// 设置模板
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public ViewEnginePart SetTemplate(string template)
        {
            if (!string.IsNullOrWhiteSpace(template)) Template = template;
            return this;
        }

        /// <summary>
        /// 设置模板数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public ViewEnginePart SetTemplateModel<T>(T model)
            where T : class, new()
        {
            TemplateModel = (typeof(T), model);
            return this;
        }

        /// <summary>
        /// 设置模板数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ViewEnginePart SetTemplateModel(object model)
        {
            return SetTemplateModel<object>(model);
        }

        /// <summary>
        /// 设置模板构建选项
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public ViewEnginePart SetTemplateOptionsBuilder(Action<IViewEngineOptionsBuilder> optionsBuilder = default)
        {
            if (optionsBuilder != null) TemplateOptionsBuilder = optionsBuilder;
            return this;
        }

        /// <summary>
        /// 设置模板缓存文件名（不含拓展名）
        /// </summary>
        /// <param name="cachedFileName"></param>
        /// <returns></returns>
        public ViewEnginePart SetTemplateCachedFileName(string cachedFileName)
        {
            if (!string.IsNullOrWhiteSpace(cachedFileName)) TemplateCachedFileName = cachedFileName;
            return this;
        }

        /// <summary>
        /// 视图模板服务作用域
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public ViewEnginePart SetViewEngineScoped(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null) ViewEngineScoped = serviceProvider;
            return this;
        }
    }
}