using System;
using System.Threading.Tasks;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图引擎接口
    /// </summary>
    public interface IViewEngine
    {
        /// <summary>
        /// 编译模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        IViewEngineCompiledTemplate<T> Compile<T>(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null)
            where T : IViewEngineTemplate;

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<IViewEngineCompiledTemplate<T>> CompileAsync<T>(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null)
            where T : IViewEngineTemplate;

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        IViewEngineCompiledTemplate Compile(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null);

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        Task<IViewEngineCompiledTemplate> CompileAsync(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null);
    }
}