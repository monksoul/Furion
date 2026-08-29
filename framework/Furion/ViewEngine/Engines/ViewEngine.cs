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
        SizeLimit = 100 // 缓存最大条目数
    });

    /// <summary>
    /// 编译结果缓存
    /// </summary>
    private readonly MemoryCache _compilationCache = new(new MemoryCacheOptions
    {
        SizeLimit = 500 // 缓存最大条目数
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
    /// 编译缓存条目
    /// </summary>
    private sealed class CompilationCacheEntry
    {
        public byte[] AssemblyBytes { get; init; } = default!;
        public Type TemplateType { get; init; } = default!;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="globalOptions"></param>
    public ViewEngine(ViewEngineOptions globalOptions)
    {
        _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
    }

    /// <summary>
    /// 编译并运行
    /// </summary>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public string RunCompile(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null)
    {
        using var template = Compile(content, builderAction);
        var result = template.Run(model);
        return result;
    }

    /// <summary>
    /// 编译并运行
    /// </summary>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public async Task<string> RunCompileAsync(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null)
    {
        using var template = await CompileAsync(content, builderAction);
        var result = await template.RunAsync(model);
        return result;
    }

    /// <summary>
    /// 编译并运行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public string RunCompile<T>(string content, T model, Action<IViewEngineCompileOptions> builderAction = null)
        where T : class, new()
    {
        using var template = Compile<ViewEngineModel<T>>(content, builderAction);
        var result = template.Run(u =>
        {
            u.Model = model;
        });
        return result;
    }

    /// <summary>
    /// 编译并运行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public async Task<string> RunCompileAsync<T>(string content, T model, Action<IViewEngineCompileOptions> builderAction = null)
        where T : class, new()
    {
        using var template = await CompileAsync<ViewEngineModel<T>>(content, builderAction);
        var result = await template.RunAsync(u =>
        {
            u.Model = model;
        });
        return result;
    }

    /// <summary>
    /// 通过缓存解析模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    public string RunCompileFromCached(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
    {
        IViewEngineTemplate template = null;

        try
        {
            template = CompileFromCached(content, builderAction, cacheFileName);
            var result = template.Run(model);
            return result;
        }
        finally
        {
            template?.Dispose();
        }
    }

    /// <summary>
    /// 通过缓存解析模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    public async Task<string> RunCompileFromCachedAsync(string content, object model = null, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
    {
        IViewEngineTemplate template = null;

        try
        {
            template = await CompileFromCachedAsync(content, builderAction, cacheFileName);
            var result = await template.RunAsync(model);
            return result;
        }
        finally
        {
            template?.Dispose();
        }
    }

    /// <summary>
    /// 通过缓存解析模板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    public string RunCompileFromCached<T>(string content, T model, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where T : class, new()
    {
        IViewEngineTemplate<ViewEngineModel<T>> template = null;

        try
        {
            template = CompileFromCached<ViewEngineModel<T>>(content, builderAction, cacheFileName);
            var result = template.Run(u =>
            {
                u.Model = model;
            });
            return result;
        }
        finally
        {
            template?.Dispose();
        }
    }

    /// <summary>
    /// 通过缓存解析模板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="model"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    public async Task<string> RunCompileFromCachedAsync<T>(string content, T model, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where T : class, new()
    {
        IViewEngineTemplate<ViewEngineModel<T>> template = null;

        try
        {
            template = await CompileFromCachedAsync<ViewEngineModel<T>>(content, builderAction, cacheFileName);
            var result = await template.RunAsync(u =>
            {
                u.Model = model;
            });
            return result;
        }
        finally
        {
            template?.Dispose();
        }
    }

    /// <summary>
    /// 从缓存中编译模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    public IViewEngineTemplate CompileFromCached(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
    {
        var fileName = cacheFileName ?? GenerateCacheKey(content, BuildOptionsForCacheKey(builderAction));
        var templatePath = GetTemplateFileName(fileName);

        IViewEngineTemplate template = null;

        if (File.Exists(templatePath))
        {
            template = ViewEngineTemplate.LoadFromFile(templatePath);
        }
        else
        {
            template = Compile(content, builderAction);
            template.SaveToFile(templatePath);
        }

        return template;
    }

    /// <summary>
    /// 编译模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public IViewEngineTemplate Compile(string content, Action<IViewEngineCompileOptions> builderAction = null)
    {
        var compileOptions = new ViewEngineCompileOptions(_globalOptions);
        compileOptions.Inherits(typeof(ViewEngineModel));

        builderAction?.Invoke(compileOptions);

        var options = compileOptions.GetOptions();
        var cacheKey = _enableCache ? GenerateCacheKey(content, options) : null;

        CompilationCacheEntry cacheEntry;

        if (_enableCache && !string.IsNullOrEmpty(cacheKey))
        {
            cacheEntry = _compilationCache.GetOrCreate(cacheKey, entry =>
            {
                entry.Size = 1;
                entry.SlidingExpiration = options.CacheSlidingExpiration;

                using var memoryStream = CreateAndCompileToStream(content, options);
                var assemblyBytes = memoryStream.ToArray();
                var templateType = Penetrates.LoadTemplateType(assemblyBytes);

                return new CompilationCacheEntry { AssemblyBytes = assemblyBytes, TemplateType = templateType };
            })!;
        }
        else
        {
            using var memoryStream = CreateAndCompileToStream(content, options);
            var assemblyBytes = memoryStream.ToArray();
            var templateType = Penetrates.LoadTemplateType(assemblyBytes);

            cacheEntry = new CompilationCacheEntry { AssemblyBytes = assemblyBytes, TemplateType = templateType };
        }

        return new ViewEngineTemplate(cacheEntry.AssemblyBytes, cacheEntry.TemplateType);
    }

    /// <summary>
    /// 从缓存中编译模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    public async Task<IViewEngineTemplate> CompileFromCachedAsync(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
    {
        var fileName = cacheFileName ?? GenerateCacheKey(content, BuildOptionsForCacheKey(builderAction));
        var templatePath = GetTemplateFileName(fileName);

        IViewEngineTemplate template = null;

        if (File.Exists(templatePath))
        {
            template = await ViewEngineTemplate.LoadFromFileAsync(templatePath);
        }
        else
        {
            template = await CompileAsync(content, builderAction);
            await template.SaveToFileAsync(templatePath);
        }

        return template;
    }

    /// <summary>
    /// 编译模板
    /// </summary>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public async Task<IViewEngineTemplate> CompileAsync(string content, Action<IViewEngineCompileOptions> builderAction = null)
    {
        return await Task.Run(() => Compile(content, builderAction));
    }

    /// <summary>
    /// 从缓存中编译模板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    public IViewEngineTemplate<T> CompileFromCached<T>(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where T : IViewEngineModel
    {
        var fileName = cacheFileName ?? GenerateCacheKey(content, BuildOptionsForCacheKey(builderAction, typeof(T)));
        var templatePath = GetTemplateFileName(fileName);

        IViewEngineTemplate<T> template = null;

        if (File.Exists(templatePath))
        {
            template = ViewEngineTemplate<T>.LoadFromFile(templatePath);
        }
        else
        {
            template = Compile<T>(content, builderAction);
            template.SaveToFile(templatePath);
        }

        return template;
    }

    /// <summary>
    /// 编译模板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public IViewEngineTemplate<T> Compile<T>(string content, Action<IViewEngineCompileOptions> builderAction = null)
        where T : IViewEngineModel
    {
        var compileOptions = new ViewEngineCompileOptions(_globalOptions);

        compileOptions.AddAssemblyReference(typeof(T).Assembly);
        compileOptions.Inherits(typeof(T));

        builderAction?.Invoke(compileOptions);

        var options = compileOptions.GetOptions();
        var cacheKey = _enableCache ? GenerateCacheKey(content, options) : null;

        CompilationCacheEntry cacheEntry;

        if (_enableCache && !string.IsNullOrEmpty(cacheKey))
        {
            cacheEntry = _compilationCache.GetOrCreate(cacheKey, entry =>
            {
                entry.Size = 1;
                entry.SlidingExpiration = options.CacheSlidingExpiration;

                using var memoryStream = CreateAndCompileToStream(content, options);
                var assemblyBytes = memoryStream.ToArray();
                var templateType = Penetrates.LoadTemplateType(assemblyBytes);

                return new CompilationCacheEntry { AssemblyBytes = assemblyBytes, TemplateType = templateType };
            })!;
        }
        else
        {
            using var memoryStream = CreateAndCompileToStream(content, options);
            var assemblyBytes = memoryStream.ToArray();
            var templateType = Penetrates.LoadTemplateType(assemblyBytes);

            cacheEntry = new CompilationCacheEntry { AssemblyBytes = assemblyBytes, TemplateType = templateType };
        }

        return new ViewEngineTemplate<T>(cacheEntry.AssemblyBytes, cacheEntry.TemplateType);
    }

    /// <summary>
    /// 从缓存中编译模板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <param name="cacheFileName"></param>
    /// <returns></returns>
    public async Task<IViewEngineTemplate<T>> CompileFromCachedAsync<T>(string content, Action<IViewEngineCompileOptions> builderAction = null, string cacheFileName = default)
        where T : IViewEngineModel
    {
        var fileName = cacheFileName ?? GenerateCacheKey(content, BuildOptionsForCacheKey(builderAction, typeof(T)));
        var templatePath = GetTemplateFileName(fileName);

        IViewEngineTemplate<T> template = null;

        if (File.Exists(templatePath))
        {
            template = await ViewEngineTemplate<T>.LoadFromFileAsync(templatePath);
        }
        else
        {
            template = await CompileAsync<T>(content, builderAction);
            await template.SaveToFileAsync(templatePath);
        }

        return template;
    }

    /// <summary>
    /// 编译模板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="content"></param>
    /// <param name="builderAction"></param>
    /// <returns></returns>
    public async Task<IViewEngineTemplate<T>> CompileAsync<T>(string content, Action<IViewEngineCompileOptions> builderAction = null)
        where T : IViewEngineModel
    {
        return await Task.Run(() => Compile<T>(content, builderAction));
    }

    /// <summary>
    /// 生成缓存键
    /// </summary>
    private static string GenerateCacheKey(string content, ViewEngineOptions options)
    {
        var hashContent = MD5Encryption.Encrypt(content);

        var assemblyNames = options.ReferencedAssemblies
            .Where(a => a != null && !string.IsNullOrEmpty(a.FullName))
            .Select(a => a.FullName)
            .OrderBy(n => n);
        var sortedUsings = options.DefaultUsings.OrderBy(u => u);

        var hashOptions = MD5Encryption.Encrypt(string.Join("|", assemblyNames.Concat(sortedUsings)));

        return hashContent + hashOptions;
    }

    /// <summary>
    /// 构建用于缓存键生成的选项
    /// </summary>
    private ViewEngineOptions BuildOptionsForCacheKey(Action<IViewEngineCompileOptions> builderAction, Type modelType = null)
    {
        var compileOptions = new ViewEngineCompileOptions(_globalOptions);

        if (modelType != null)
        {
            compileOptions.AddAssemblyReference(modelType);
            compileOptions.Inherits(modelType);
        }
        else
        {
            compileOptions.Inherits(typeof(ViewEngineModel));
        }

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
    internal MemoryStream CreateAndCompileToStream(string templateSource, ViewEngineOptions options)
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
                (builder) =>
                {
                    builder.SetNamespace(options.TemplateNamespace ?? "Furion.ViewEngine");
                });
        })!;

        var fileName = Path.GetRandomFileName();

        var document = RazorSourceDocument.Create(templateSource, fileName);

        var codeDocument = engine.Process(
            document,
            null,
            new List<RazorSourceDocument>(),
            new List<TagHelperDescriptor>());

        var razorCSharpDocument = codeDocument.GetCSharpDocument();

        var syntaxTree = CSharpSyntaxTree.ParseText(razorCSharpDocument.GeneratedCode,
            new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var metadataReferences = new List<MetadataReference>();

        foreach (var assembly in options.ReferencedAssemblies)
        {
            if (assembly == null || assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                continue;

            if (!File.Exists(assembly.Location))
                continue;

            var assemblyLocation = assembly.Location;
            if (seen.Add(assembly.FullName ?? assembly.GetName().Name))
            {
                metadataReferences.Add(_metadataReferenceCache.GetOrAdd(assemblyLocation, loc => MetadataReference.CreateFromFile(loc)));
            }
        }

        // 添加自定义元数据引用
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
                concurrentBuild: true
            ));

        var memoryStream = new MemoryStream();

        var emitResult = compilation.Emit(memoryStream);

        if (!emitResult.Success)
        {
            memoryStream.Dispose();

            //var errors = emitResult.Diagnostics
            //    .Where(d => d.Severity == DiagnosticSeverity.Error || d.IsWarningAsError)
            //    .Select(d => d.ToString())
            //    .ToArray();

            var exception = new ViewEngineTemplateException()
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
    /// 写入 Razor 命令
    /// </summary>
    /// <param name="content"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    internal static string WriteDirectives(string content, ViewEngineOptions options)
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
    /// 获取模板文件名
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    internal string GetTemplateFileName(string fileName)
    {
        var templateSaveDir = _globalOptions.CacheDirectory;

        if (string.IsNullOrWhiteSpace(templateSaveDir))
        {
            templateSaveDir = Path.Combine(AppContext.BaseDirectory, "templates");
        }

        if (!Directory.Exists(templateSaveDir)) Directory.CreateDirectory(templateSaveDir);

        if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) fileName += ".dll";
        var templatePath = Path.Combine(templateSaveDir, "~" + fileName);

        return templatePath;
    }
}