// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

namespace Furion.ViewEngine;

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
    string RunCompile(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null);

    /// <summary>
    /// 编译并运行
    /// </summary>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    Task<string> RunCompileAsync(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null);

    /// <summary>
    /// 通过缓存编译并运行
    /// </summary>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    string RunCompileFromCached(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default);

    /// <summary>
    /// 通过缓存编译并运行
    /// </summary>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    Task<string> RunCompileFromCachedAsync(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default);

    /// <summary>
    /// 编译模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    IViewEngineTemplate Compile(string content, Action<IViewEngineCompileOptions> builderAction = null);

    /// <summary>
    /// 编译模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    Task<IViewEngineTemplate> CompileAsync(string content, Action<IViewEngineCompileOptions> builderAction = null);

    /// <summary>
    /// 从缓存编译模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    IViewEngineTemplate CompileFromCached(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default);

    /// <summary>
    /// 从缓存编译模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    Task<IViewEngineTemplate> CompileFromCachedAsync(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default);

    /// <summary>
    /// 编译并运行
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    string RunCompile<TModel>(string content, TModel model, Action<IViewEngineCompileOptions> builderAction = null)
        where TModel : class;

    /// <summary>
    /// 编译并运行
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    Task<string> RunCompileAsync<TModel>(string content, TModel model, Action<IViewEngineCompileOptions> builderAction = null)
        where TModel : class;

    /// <summary>
    /// 通过缓存编译并运行
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    string RunCompileFromCached<TModel>(string content, TModel model, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where TModel : class;

    /// <summary>
    /// 通过缓存编译并运行
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    Task<string> RunCompileFromCachedAsync<TModel>(string content, TModel model, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where TModel : class;

    /// <summary>
    /// 编译模板
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    IViewEngineTemplate<TModel> Compile<TModel>(string content, Action<IViewEngineCompileOptions> builderAction = null)
        where TModel : class;

    /// <summary>
    /// 编译模板
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    Task<IViewEngineTemplate<TModel>> CompileAsync<TModel>(string content, Action<IViewEngineCompileOptions> builderAction = null)
        where TModel : class;

    /// <summary>
    /// 从缓存编译模板
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    IViewEngineTemplate<TModel> CompileFromCached<TModel>(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where TModel : class;

    /// <summary>
    /// 从缓存编译模板
    /// </summary>
    /// <typeparam name="TModel">模型类型</typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    Task<IViewEngineTemplate<TModel>> CompileFromCachedAsync<TModel>(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where TModel : class;
}