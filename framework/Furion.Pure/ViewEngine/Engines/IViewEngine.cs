// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;
using System.Threading.Tasks;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 视图引擎接口
    /// </summary>
    public interface IViewEngine
    {
        /// <summary>
        /// 编译并运行
        /// </summary>
        /// <param name="content"></param>
        /// <param name="model"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        string RunCompile(string content, object model = null, Action<IViewEngineOptionsBuilder> builderAction = null);

        /// <summary>
        /// 编译并运行
        /// </summary>
        /// <param name="content"></param>
        /// <param name="model"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<string> RunCompileAsync(string content, object model = null, Action<IViewEngineOptionsBuilder> builderAction = null);

        /// <summary>
        /// 编译并运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="model"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        string RunCompile<T>(string content, T model, Action<IViewEngineOptionsBuilder> builderAction = null)
            where T : class, new();

        /// <summary>
        /// 编译并运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="model"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<string> RunCompileAsync<T>(string content, T model, Action<IViewEngineOptionsBuilder> builderAction = null)
            where T : class, new();

        /// <summary>
        /// 通过缓存解析模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="model"></param>
        /// <param name="cacheFileName"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        string RunCompileFromCached(string content, object model = null, string cacheFileName = default, Action<IViewEngineOptionsBuilder> builderAction = null);

        /// <summary>
        /// 通过缓存解析模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="model"></param>
        /// <param name="cacheFileName"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<string> RunCompileFromCachedAsync(string content, object model = null, string cacheFileName = default, Action<IViewEngineOptionsBuilder> builderAction = null);

        /// <summary>
        /// 通过缓存解析模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="model"></param>
        /// <param name="cacheFileName"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        string RunCompileFromCached<T>(string content, T model, string cacheFileName = default, Action<IViewEngineOptionsBuilder> builderAction = null)
            where T : class, new();

        /// <summary>
        /// 通过缓存解析模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="model"></param>
        /// <param name="cacheFileName"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<string> RunCompileFromCachedAsync<T>(string content, T model, string cacheFileName = default, Action<IViewEngineOptionsBuilder> builderAction = null)
            where T : class, new();

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        IViewEngineTemplate Compile(string content, Action<IViewEngineOptionsBuilder> builderAction = null);

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<IViewEngineTemplate> CompileAsync(string content, Action<IViewEngineOptionsBuilder> builderAction = null);

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        IViewEngineTemplate<T> Compile<T>(string content, Action<IViewEngineOptionsBuilder> builderAction = null)
            where T : IViewEngineModel;

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<IViewEngineTemplate<T>> CompileAsync<T>(string content, Action<IViewEngineOptionsBuilder> builderAction = null)
            where T : IViewEngineModel;
    }
}