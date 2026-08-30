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

using Furion.Extensions;

namespace Furion.ViewEngine;

/// <summary>
/// 视图模板实现类
/// </summary>
public class ViewEngineTemplate : IViewEngineTemplate
{
    /// <summary>
    /// 程序集字节码
    /// </summary>
    private byte[] _assemblyBytes;

    /// <summary>
    /// 模板类型全名
    /// </summary>
    private string _templateTypeName;

    /// <summary>
    /// 是否已释放
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// 缓存文件路径
    /// </summary>
    private string? _cacheFilePath;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="assemblyBytes">程序集字节数组</param>
    /// <param name="templateTypeName">模板类型全名</param>
    internal ViewEngineTemplate(byte[] assemblyBytes, string templateTypeName)
        : this(assemblyBytes, templateTypeName, null)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="assemblyBytes">程序集字节数组</param>
    /// <param name="templateTypeName">模板类型全名</param>
    /// <param name="cacheFilePath">缓存文件路径</param>
    internal ViewEngineTemplate(byte[] assemblyBytes, string templateTypeName, string? cacheFilePath)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(assemblyBytes);
        ArgumentNullException.ThrowIfNull(templateTypeName);

        _assemblyBytes = assemblyBytes;
        _templateTypeName = templateTypeName;
        _cacheFilePath = cacheFilePath;
    }

    /// <summary>
    /// 保存到流中
    /// </summary>
    /// <param name="stream"></param>
    public void SaveToStream(Stream stream)
    {
        ThrowIfDisposed();
        stream.Write(_assemblyBytes, 0, _assemblyBytes.Length);
    }

    /// <summary>
    /// 保存到流中
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public Task SaveToStreamAsync(Stream stream)
    {
        SaveToStream(stream);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 保存到文件
    /// </summary>
    /// <param name="fullName"></param>
    public void SaveToFile(string fullName)
    {
        ThrowIfDisposed();
        File.WriteAllBytes(fullName, _assemblyBytes);
    }

    /// <summary>
    /// 保存到文件
    /// </summary>
    /// <param name="fullName"></param>
    /// <returns></returns>
    public Task SaveToFileAsync(string fullName)
    {
        SaveToFile(fullName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 执行模板
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public string Run(object model = null)
    {
        ThrowIfDisposed();

        if (model != null && model.IsAnonymous())
        {
            model = new AnonymousTypeWrapper(model);
        }

        var (type, alc) = Penetrates.LoadTemplateType(_assemblyBytes);
        try
        {
            var instance = (IViewEngineModel)Activator.CreateInstance(type);
            instance.Model = model;

            instance.Execute();
            return instance.Result();
        }
        catch (InvalidCastException ex)
        {
            throw GetCacheMismatchException(ex);
        }
        finally
        {
            alc.Unload();
        }
    }

    /// <summary>
    /// 执行模板
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<string> RunAsync(object model = null)
    {
        ThrowIfDisposed();

        if (model != null && model.IsAnonymous())
        {
            model = new AnonymousTypeWrapper(model);
        }

        var (type, alc) = Penetrates.LoadTemplateType(_assemblyBytes);
        try
        {
            var instance = (IViewEngineModel)Activator.CreateInstance(type);
            instance.Model = model;

            await instance.ExecuteAsync();
            return await instance.ResultAsync();
        }
        catch (InvalidCastException ex)
        {
            throw GetCacheMismatchException(ex);
        }
        finally
        {
            alc.Unload();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // 释放引用
        _assemblyBytes = null!;
        _templateTypeName = null!;
        _cacheFilePath = null!;
    }

    /// <summary>
    /// 检查对象是否已释放
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ViewEngineTemplate));
        }
    }

    /// <summary>
    /// 生成缓存不匹配异常
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    private Exception GetCacheMismatchException(InvalidCastException ex)
    {
        if (_cacheFilePath != null)
        {
            return new InvalidOperationException(
                $"Failed to cast template type. The cached file may be incompatible. File path: `{_cacheFilePath}`.", ex);
        }
        return ex;
    }
}

/// <summary>
/// 视图模板实现类
/// </summary>
/// <typeparam name="TModel">模型类型</typeparam>
public class ViewEngineTemplate<TModel> : IViewEngineTemplate<TModel>
    where TModel : class
{
    /// <summary>
    /// 程序集字节码
    /// </summary>
    private byte[] _assemblyBytes;

    /// <summary>
    /// 模板类型全名
    /// </summary>
    private string _templateTypeName;

    /// <summary>
    /// 是否已释放
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// 缓存文件路径
    /// </summary>
    private string? _cacheFilePath;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="assemblyBytes">程序集字节数组</param>
    /// <param name="templateTypeName">模板类型全名</param>
    internal ViewEngineTemplate(byte[] assemblyBytes, string templateTypeName)
        : this(assemblyBytes, templateTypeName, null)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="assemblyBytes">程序集字节数组</param>
    /// <param name="templateTypeName">模板类型全名</param>
    /// <param name="cacheFilePath">缓存文件路径</param>
    internal ViewEngineTemplate(byte[] assemblyBytes, string templateTypeName, string? cacheFilePath)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(assemblyBytes);
        ArgumentNullException.ThrowIfNull(templateTypeName);

        _assemblyBytes = assemblyBytes;
        _templateTypeName = templateTypeName;
        _cacheFilePath = cacheFilePath;
    }

    /// <summary>
    /// 保存到流中
    /// </summary>
    /// <param name="stream"></param>
    public void SaveToStream(Stream stream)
    {
        ThrowIfDisposed();
        stream.Write(_assemblyBytes, 0, _assemblyBytes.Length);
    }

    /// <summary>
    /// 保存到流中
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public Task SaveToStreamAsync(Stream stream)
    {
        SaveToStream(stream);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 保存到文件
    /// </summary>
    /// <param name="fullName"></param>
    public void SaveToFile(string fullName)
    {
        ThrowIfDisposed();
        File.WriteAllBytes(fullName, _assemblyBytes);
    }

    /// <summary>
    /// 保存到文件
    /// </summary>
    /// <param name="fullName"></param>
    /// <returns></returns>
    public Task SaveToFileAsync(string fullName)
    {
        SaveToFile(fullName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 执行模板
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public string Run(TModel model)
    {
        ThrowIfDisposed();

        var (type, alc) = Penetrates.LoadTemplateType(_assemblyBytes);
        try
        {
            var instance = Activator.CreateInstance(type);

            if (instance is ViewEngineModel<TModel> strongTypedInstance)
            {
                strongTypedInstance.Model = model;
                strongTypedInstance.Execute();

                return strongTypedInstance.Result();
            }
            else
            {
                var dynamicInstance = (IViewEngineModel)instance;
                dynamicInstance.Model = model != null && model.IsAnonymous() ? new AnonymousTypeWrapper(model) : model;
                dynamicInstance.Execute();

                return dynamicInstance.Result();
            }
        }
        catch (InvalidCastException ex)
        {
            throw GetCacheMismatchException(ex);
        }
        finally
        {
            alc.Unload();
        }
    }

    /// <summary>
    /// 执行模板
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<string> RunAsync(TModel model)
    {
        ThrowIfDisposed();

        var (type, alc) = Penetrates.LoadTemplateType(_assemblyBytes);
        try
        {
            var instance = Activator.CreateInstance(type);

            if (instance is ViewEngineModel<TModel> strongTypedInstance)
            {
                strongTypedInstance.Model = model;
                await strongTypedInstance.ExecuteAsync();

                return await strongTypedInstance.ResultAsync();
            }
            else
            {
                var dynamicInstance = (IViewEngineModel)instance;
                dynamicInstance.Model = model != null && model.IsAnonymous() ? new AnonymousTypeWrapper(model) : model;
                await dynamicInstance.ExecuteAsync();

                return await dynamicInstance.ResultAsync();
            }
        }
        catch (InvalidCastException ex)
        {
            throw GetCacheMismatchException(ex);
        }
        finally
        {
            alc.Unload();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // 释放引用
        _assemblyBytes = null!;
        _templateTypeName = null!;
        _cacheFilePath = null!;
    }

    /// <summary>
    /// 检查对象是否已释放
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ViewEngineTemplate<TModel>));
        }
    }

    /// <summary>
    /// 生成缓存不匹配异常
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    private Exception GetCacheMismatchException(InvalidCastException ex)
    {
        if (_cacheFilePath != null)
        {
            return new InvalidOperationException(
                $"Failed to cast template type. The cached file may be incompatible. File path: `{_cacheFilePath}`.", ex);
        }
        return ex;
    }
}