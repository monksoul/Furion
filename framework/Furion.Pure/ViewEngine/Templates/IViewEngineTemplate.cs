// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Furion.ViewEngine
{
    /// <summary>
    /// 视图引擎模板（编译后）
    /// </summary>
    public interface IViewEngineTemplate
    {
        /// <summary>
        /// 保存到流中
        /// </summary>
        /// <param name="stream"></param>
        void SaveToStream(Stream stream);

        /// <summary>
        /// 保存到流中
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task SaveToStreamAsync(Stream stream);

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="fileName"></param>
        void SaveToFile(string fileName);

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task SaveToFileAsync(string fileName);

        /// <summary>
        /// 执行编译
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string Run(object model = null);

        /// <summary>
        /// 执行编译
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<string> RunAsync(object model = null);
    }

    /// <summary>
    /// 泛型视图编译模板接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IViewEngineTemplate<out T>
        where T : IViewEngineModel
    {
        /// <summary>
        /// 保存到流中
        /// </summary>
        /// <param name="stream"></param>
        void SaveToStream(Stream stream);

        /// <summary>
        /// 保存到流中
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task SaveToStreamAsync(Stream stream);

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="fileName"></param>
        void SaveToFile(string fileName);

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task SaveToFileAsync(string fileName);

        /// <summary>
        /// 执行编译
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        string Run(Action<T> initializer);

        /// <summary>
        /// 执行编译
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        Task<string> RunAsync(Action<T> initializer);
    }
}