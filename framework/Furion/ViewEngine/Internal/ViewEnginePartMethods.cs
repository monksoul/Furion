// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 字符串模板执行部件
    /// </summary>
    public sealed partial class ViewEnginePart
    {
        /// <summary>
        /// 编译并运行
        /// </summary>
        public string RunCompile()
        {
            return InvokeRunCompileMethod(nameof(IViewEngine.RunCompile)) as string;
        }

        /// <summary>
        /// 编译并运行
        /// </summary>
        public Task<string> RunCompileAsync()
        {
            return InvokeRunCompileMethod(nameof(IViewEngine.RunCompileAsync)) as Task<string>;
        }

        /// <summary>
        /// 通过缓存解析模板
        /// </summary>
        public string RunCompileFromCached()
        {
            return InvokeRunCompileMethod(nameof(IViewEngine.RunCompileFromCached), true) as string;
        }

        /// <summary>
        /// 通过缓存解析模板
        /// </summary>
        public Task<string> RunCompileFromCachedAsync()
        {
            return InvokeRunCompileMethod(nameof(IViewEngine.RunCompileFromCachedAsync), true) as Task<string>;
        }

        /// <summary>
        /// 执行模板方法
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="isCached"></param>
        /// <returns></returns>
        private object InvokeRunCompileMethod(string methodName, bool isCached = false)
        {
            var viewEngine = GetViewEngine();
            var viewEngineType = viewEngine.GetType();

            // 反射获取视图引擎方法
            var runCompileMethod = TemplateModel.Type == typeof(object)
                ? viewEngineType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                           .First(m => m.Name == methodName && !m.IsGenericMethod)
                : viewEngineType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                           .First(m => m.Name == methodName && m.IsGenericMethod)
                           .MakeGenericMethod(TemplateModel.Type);

            return !isCached
                ? runCompileMethod.Invoke(viewEngine, new object[] {
                    Template,TemplateModel.Model,TemplateOptionsBuilder
                })
                : runCompileMethod.Invoke(viewEngine, new object[] {
                    Template,TemplateModel.Model,TemplateCachedFileName, TemplateOptionsBuilder
                });
        }

        /// <summary>
        /// 获取视图引擎对象
        /// </summary>
        /// <returns></returns>
        private IViewEngine GetViewEngine()
        {
            return App.GetService<IViewEngine>(ViewEngineScoped ?? App.RootServices)
                ?? throw new InvalidOperationException("Please confirm whether the view engine is registered successfully.");
        }
    }
}