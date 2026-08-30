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

using Furion.DataEncryption;
using Furion.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text;

namespace Furion.ViewEngine;

/// <summary>
/// 视图引擎实现类
/// </summary>
internal sealed class ViewEngine : IViewEngine
{
    /// <summary>
    /// 全局默认编译选项
    /// </summary>
    private readonly ViewEngineOptions _globalOptions;

    /// <summary>
    /// Razor 引擎缓存
    /// </summary>
    private readonly MemoryCache _razorEngineCache = new(new MemoryCacheOptions
    {
        SizeLimit = 100
    });

    /// <summary>
    /// 编译结果缓存
    /// </summary>
    private readonly MemoryCache _compilationCache = new(new MemoryCacheOptions
    {
        SizeLimit = 500
    });

    /// <summary>
    /// 缓存是否启用
    /// </summary>
    private readonly bool _enableCache = Environment.GetEnvironmentVariable("FURION_VIEWENGINE_CACHE") != "false";

    /// <summary>
    /// 元数据引用缓存
    /// </summary>
    private readonly ConcurrentDictionary<string, MetadataReference> _metadataReferenceCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 文件锁字典
    /// </summary>
    private readonly ConcurrentDictionary<string, object> _fileLocks = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 编译缓存条目
    /// </summary>
    private sealed class CompilationCacheEntry
    {
        /// <summary>
        /// 程序集字节数组
        /// </summary>
        public byte[] AssemblyBytes { get; init; } = default!;

        /// <summary>
        /// 模板类型全名
        /// </summary>
        public string TemplateTypeName { get; init; } = Penetrates.TemplateTypeName;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="globalOptions"></param>
    public ViewEngine(ViewEngineOptions globalOptions)
    {
        _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
    }

    /// <inheritdoc/>
    public string RunCompile(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null)
    {
        using var template = Compile(content, builderAction);
        return template.Run(model);
    }

    /// <inheritdoc/>
    public async Task<string> RunCompileAsync(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null)
    {
        using var template = await CompileAsync(content, builderAction);
        return await template.RunAsync(model);
    }

    /// <inheritdoc/>
    public string RunCompileFromCached(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
    {
        using var template = CompileFromCached(content, builderAction, cacheFileName);
        return template.Run(model);
    }

    /// <inheritdoc/>
    public async Task<string> RunCompileFromCachedAsync(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
    {
        using var template = await CompileFromCachedAsync(content, builderAction, cacheFileName);
        return await template.RunAsync(model);
    }

    /// <inheritdoc/>
    public IViewEngineTemplate Compile(string content, Action<IViewEngineCompileOptions> builderAction = null)
    {
        var compileOptions = new ViewEngineCompileOptions(_globalOptions);
        compileOptions.Inherits(typeof(ViewEngineModel));
        builderAction?.Invoke(compileOptions);

        var options = compileOptions.GetOptions();
        var cacheKey = _enableCache ? GenerateCacheKey(content, options) : null;
        var cacheEntry = GetOrCompile(cacheKey, options, content);
        return new ViewEngineTemplate(cacheEntry.AssemblyBytes, cacheEntry.TemplateTypeName);
    }

    /// <inheritdoc/>
    public Task<IViewEngineTemplate> CompileAsync(string content, Action<IViewEngineCompileOptions> builderAction = null)
        => Task.Run(() => Compile(content, builderAction));

    /// <inheritdoc/>
    public IViewEngineTemplate CompileFromCached(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
    {
        var compileOptionsForCacheKey = BuildOptionsForCacheKey(builderAction, typeof(ViewEngineModel));
        var fileName = cacheFileName ?? GenerateCacheKey(content, compileOptionsForCacheKey);
        fileName = Path.GetFileName(fileName);
        var templatePath = GetTemplateFileName(fileName);

        var fileLock = _fileLocks.GetOrAdd(templatePath, _ => new object());
        lock (fileLock)
        {
            try
            {
                if (File.Exists(templatePath))
                {
                    var bytes = File.ReadAllBytes(templatePath);
                    var (_, alc) = Penetrates.LoadTemplateType(bytes);
                    alc.Unload();
                    return new ViewEngineTemplate(bytes, Penetrates.TemplateTypeName, templatePath);
                }
            }
            catch (Exception ex) when (ex is IOException or BadImageFormatException or InvalidOperationException)
            {
                try { File.Delete(templatePath); } catch { }
            }

            var template = Compile(content, builderAction);
            Penetrates.SaveTemplateAtomically(templatePath, template);
            return template;
        }
    }

    /// <inheritdoc/>
    public Task<IViewEngineTemplate> CompileFromCachedAsync(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        => Task.Run(() => CompileFromCached(content, builderAction, cacheFileName));

    /// <inheritdoc/>
    public string RunCompile<TModel>(string content, TModel model, Action<IViewEngineCompileOptions> builderAction = null)
        where TModel : class
    {
        using var template = Compile<TModel>(content, builderAction);
        return template.Run(model);
    }

    /// <inheritdoc/>
    public async Task<string> RunCompileAsync<TModel>(string content, TModel model, Action<IViewEngineCompileOptions> builderAction = null)
        where TModel : class
    {
        using var template = await CompileAsync<TModel>(content, builderAction);
        return await template.RunAsync(model);
    }

    /// <inheritdoc/>
    public string RunCompileFromCached<TModel>(string content, TModel model, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where TModel : class
    {
        using var template = CompileFromCached<TModel>(content, builderAction, cacheFileName);
        return template.Run(model);
    }

    /// <inheritdoc/>
    public async Task<string> RunCompileFromCachedAsync<TModel>(string content, TModel model, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where TModel : class
    {
        using var template = await CompileFromCachedAsync<TModel>(content, builderAction, cacheFileName);
        return await template.RunAsync(model);
    }

    /// <inheritdoc/>
    public IViewEngineTemplate<TModel> Compile<TModel>(string content, Action<IViewEngineCompileOptions> builderAction = null)
        where TModel : class
    {
        if (typeof(TModel).IsAnonymous())
        {
            return CompileForAnonymousType<TModel>(content, builderAction);
        }

        var baseType = typeof(ViewEngineModel<TModel>);
        var compileOptions = new ViewEngineCompileOptions(_globalOptions);
        compileOptions.AddAssemblyReference(typeof(TModel).Assembly);
        compileOptions.Inherits(baseType);
        builderAction?.Invoke(compileOptions);

        var options = compileOptions.GetOptions();
        var cacheKey = _enableCache ? GenerateCacheKey(content, options) : null;
        var cacheEntry = GetOrCompile(cacheKey, options, content);
        return new ViewEngineTemplate<TModel>(cacheEntry.AssemblyBytes, cacheEntry.TemplateTypeName);
    }

    /// <inheritdoc/>
    public Task<IViewEngineTemplate<TModel>> CompileAsync<TModel>(string content, Action<IViewEngineCompileOptions> builderAction = null)
        where TModel : class
        => Task.Run(() => Compile<TModel>(content, builderAction));

    /// <inheritdoc/>
    public IViewEngineTemplate<TModel> CompileFromCached<TModel>(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where TModel : class
    {
        if (typeof(TModel).IsAnonymous())
        {
            return CompileFromCachedForAnonymousType<TModel>(content, builderAction, cacheFileName);
        }

        var baseType = typeof(ViewEngineModel<TModel>);
        var compileOptionsForCacheKey = BuildOptionsForCacheKey(builderAction, baseType);
        var fileName = cacheFileName ?? GenerateCacheKey(content, compileOptionsForCacheKey);
        fileName = Path.GetFileName(fileName);
        var templatePath = GetTemplateFileName(fileName);

        var fileLock = _fileLocks.GetOrAdd(templatePath, _ => new object());
        lock (fileLock)
        {
            try
            {
                if (File.Exists(templatePath))
                {
                    var bytes = File.ReadAllBytes(templatePath);
                    var (_, alc) = Penetrates.LoadTemplateType(bytes);
                    alc.Unload();
                    return new ViewEngineTemplate<TModel>(bytes, Penetrates.TemplateTypeName, templatePath);
                }
            }
            catch (Exception ex) when (ex is IOException or BadImageFormatException or InvalidOperationException)
            {
                try { File.Delete(templatePath); } catch { }
            }

            var template = Compile<TModel>(content, builderAction);
            Penetrates.SaveTemplateAtomically(templatePath, template);
            return template;
        }
    }

    /// <inheritdoc/>
    public Task<IViewEngineTemplate<TModel>> CompileFromCachedAsync<TModel>(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where TModel : class
        => Task.Run(() => CompileFromCached<TModel>(content, builderAction, cacheFileName));

    /// <summary>
    /// 编译匿名类型模型
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    private IViewEngineTemplate<TModel> CompileForAnonymousType<TModel>(string content, Action<IViewEngineCompileOptions> builderAction)
        where TModel : class
    {
        var compileOptions = new ViewEngineCompileOptions(_globalOptions);
        compileOptions.Inherits(typeof(ViewEngineModel));
        builderAction?.Invoke(compileOptions);

        var options = compileOptions.GetOptions();
        var cacheKey = _enableCache ? GenerateCacheKey(content, options) : null;
        var cacheEntry = GetOrCompile(cacheKey, options, content);
        return new ViewEngineTemplate<TModel>(cacheEntry.AssemblyBytes, cacheEntry.TemplateTypeName);
    }

    /// <summary>
    /// 从缓存编译匿名类型模型
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    private IViewEngineTemplate<TModel> CompileFromCachedForAnonymousType<TModel>(string content, Action<IViewEngineCompileOptions> builderAction, string cacheFileName)
        where TModel : class
    {
        var compileOptionsForCacheKey = BuildOptionsForCacheKey(builderAction, typeof(ViewEngineModel));
        var fileName = cacheFileName ?? GenerateCacheKey(content, compileOptionsForCacheKey);
        fileName = Path.GetFileName(fileName);
        var templatePath = GetTemplateFileName(fileName);

        var fileLock = _fileLocks.GetOrAdd(templatePath, _ => new object());
        lock (fileLock)
        {
            try
            {
                if (File.Exists(templatePath))
                {
                    var bytes = File.ReadAllBytes(templatePath);
                    var (_, alc) = Penetrates.LoadTemplateType(bytes);
                    alc.Unload();
                    return new ViewEngineTemplate<TModel>(bytes, Penetrates.TemplateTypeName, templatePath);
                }
            }
            catch (Exception ex) when (ex is IOException or BadImageFormatException or InvalidOperationException)
            {
                try { File.Delete(templatePath); } catch { }
            }

            var template = CompileForAnonymousType<TModel>(content, builderAction);
            Penetrates.SaveTemplateAtomically(templatePath, template);
            return template;
        }
    }

    /// <summary>
    /// 获取或编译缓存条目
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="options"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private CompilationCacheEntry GetOrCompile(string cacheKey, ViewEngineOptions options, string content)
    {
        if (_enableCache && !string.IsNullOrEmpty(cacheKey))
        {
            return _compilationCache.GetOrCreate(cacheKey, entry =>
            {
                entry.Size = 1;
                entry.SlidingExpiration = options.CacheSlidingExpiration;
                using var ms = CreateAndCompileToStream(content, options);
                return new CompilationCacheEntry
                {
                    AssemblyBytes = ms.ToArray(),
                    TemplateTypeName = Penetrates.TemplateTypeName
                };
            })!;
        }

        using var ms = CreateAndCompileToStream(content, options);
        return new CompilationCacheEntry
        {
            AssemblyBytes = ms.ToArray(),
            TemplateTypeName = Penetrates.TemplateTypeName
        };
    }

    /// <summary>
    /// 生成缓存键
    /// </summary>
    /// <param name="content"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static string GenerateCacheKey(string content, ViewEngineOptions options)
    {
        var hashContent = MD5Encryption.Encrypt(content);

        var assemblyNames = options.ReferencedAssemblies
            .Where(a => a != null && !string.IsNullOrEmpty(a.GetName().Name))
            .Select(a => a.GetName().Name)
            .OrderBy(n => n);
        var sortedUsings = options.DefaultUsings.OrderBy(u => u);

        var inheritName = options.Inherits ?? string.Empty;
        var namespaceName = options.TemplateNamespace ?? string.Empty;
        var metadataRefs = options.MetadataReferences
            .Select(m => m.Display ?? m.ToString())
            .OrderBy(d => d);

        var combined = string.Join("|",
            assemblyNames
            .Concat(sortedUsings)
            .Append(inheritName)
            .Append(namespaceName)
            .Concat(metadataRefs));

        var hashOptions = MD5Encryption.Encrypt(combined);

        return hashContent + hashOptions;
    }

    /// <summary>
    /// 构建用于缓存键生成的选项
    /// </summary>
    /// <param name="builderAction"></param>
    /// <param name="modelBaseType"></param>
    /// <returns></returns>
    private ViewEngineOptions BuildOptionsForCacheKey(Action<IViewEngineCompileOptions> builderAction, Type modelBaseType)
    {
        var compileOptions = new ViewEngineCompileOptions(_globalOptions);
        compileOptions.Inherits(modelBaseType);
        builderAction?.Invoke(compileOptions);

        return compileOptions.GetOptions();
    }

    /// <summary>
    /// 将模板内容编译并输出内存流
    /// </summary>
    /// <remarks>参考文献：https://lebang2020.cn/details/201225gy5nu0gd.html</remarks>
    /// <param name="templateSource"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private MemoryStream CreateAndCompileToStream(string templateSource, ViewEngineOptions options)
    {
        templateSource = WriteDirectives(templateSource, options);

        var engineKey = options.TemplateNamespace ?? "Furion.ViewEngine";
        var engine = _razorEngineCache.GetOrCreate(engineKey, entry =>
        {
            entry.Size = 1;
            entry.SlidingExpiration = options.CacheSlidingExpiration;

            return RazorProjectEngine.Create(
                RazorConfiguration.Default,
                RazorProjectFileSystem.Create("."),
                builder => builder.SetNamespace(options.TemplateNamespace ?? "Furion.ViewEngine"));
        })!;

        var fileName = Path.GetRandomFileName();
        var document = RazorSourceDocument.Create(templateSource, fileName);
        var codeDocument = engine.Process(document, null, new List<RazorSourceDocument>(), new List<TagHelperDescriptor>());
        var razorCSharpDocument = codeDocument.GetCSharpDocument();

        var syntaxTree = CSharpSyntaxTree.ParseText(
            razorCSharpDocument.GeneratedCode,
            new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var metadataReferences = new List<MetadataReference>();

        foreach (var assembly in options.ReferencedAssemblies)
        {
            if (assembly == null || assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location) || !File.Exists(assembly.Location))
            {
                continue;
            }

            if (seen.Add(assembly.FullName ?? assembly.GetName().Name))
            {
                metadataReferences.Add(_metadataReferenceCache.GetOrAdd(assembly.Location, loc => MetadataReference.CreateFromFile(loc)));
            }
        }

        metadataReferences.AddRange(options.MetadataReferences);

        var compilation = CSharpCompilation.Create(
            fileName,
            [syntaxTree],
            metadataReferences,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                nullableContextOptions: NullableContextOptions.Disable,
                warningLevel: 4,
                allowUnsafe: false,
                checkOverflow: false,
                deterministic: true,
                concurrentBuild: true));

        var memoryStream = new MemoryStream();
        var emitResult = compilation.Emit(memoryStream);

        if (!emitResult.Success)
        {
            memoryStream.Dispose();
            var exception = new ViewEngineTemplateException
            {
                Errors = emitResult.Diagnostics.ToList(),
                GeneratedCode = razorCSharpDocument.GeneratedCode,
                ContextLines = options.CodeContextLines
            };

            throw exception;
        }

        memoryStream.Position = 0;

        return memoryStream;
    }

    /// <summary>
    /// 写入 Razor 指令
    /// </summary>
    /// <param name="content"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static string WriteDirectives(string content, ViewEngineOptions options)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"@inherits {options.Inherits}");

        foreach (var entry in options.DefaultUsings)
        {
            stringBuilder.AppendLine($"@using {entry}");
        }

        stringBuilder.Append(content);

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取模板文件完整路径
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private string GetTemplateFileName(string fileName)
    {
        var templateSaveDir = _globalOptions.CacheDirectory;

        if (string.IsNullOrWhiteSpace(templateSaveDir))
        {
            templateSaveDir = Path.Combine(AppContext.BaseDirectory, "templates");
        }

        if (!Directory.Exists(templateSaveDir)) Directory.CreateDirectory(templateSaveDir);

        if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) fileName += ".dll";

        return Path.Combine(templateSaveDir, "~" + fileName);
    }
}