using System;
using System.IO;
using System.Threading.Tasks;

namespace Fur.ViewEngine
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