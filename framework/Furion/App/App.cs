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

using Furion.ConfigurableOptions;
using Furion.Extensions;
using Furion.Reflection;
using Furion.Templates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Claims;
using System.Text.Json;

namespace Furion;

/// <summary>
/// 全局应用类
/// </summary>
public static class App
{
    /// <summary>
    /// 私有设置，避免重复解析
    /// </summary>
    internal static AppSettingsOptions _settings;

    /// <summary>
    /// 应用全局配置
    /// </summary>
    public static AppSettingsOptions Settings => _settings ??= GetConfig<AppSettingsOptions>("AppSettings", true);

    /// <summary>
    /// 全局配置选项
    /// </summary>
    public static IConfiguration Configuration => CatchOrDefault(() => InternalApp.Configuration.Reload(), new ConfigurationBuilder().Build());

    /// <summary>
    /// 获取Web主机环境，如，是否是开发环境，生产环境等
    /// </summary>
    public static IWebHostEnvironment WebHostEnvironment => InternalApp.WebHostEnvironment;

    /// <summary>
    /// 获取泛型主机环境，如，是否是开发环境，生产环境等
    /// </summary>
    public static IHostEnvironment HostEnvironment => InternalApp.HostEnvironment;

    /// <summary>
    /// 存储根服务，可能为空
    /// </summary>
    public static IServiceProvider RootServices => InternalApp.RootServices;

    /// <summary>
    /// 判断是否是单文件环境
    /// </summary>
    public static bool SingleFileEnvironment => string.IsNullOrWhiteSpace(Assembly.GetEntryAssembly().Location);

    /// <summary>
    /// 应用有效程序集
    /// </summary>
    public static readonly List<Assembly> Assemblies;

    /// <summary>
    /// 有效程序集类型
    /// </summary>
    public static readonly List<Type> EffectiveTypes;

    /// <summary>
    /// 获取请求上下文
    /// </summary>
    public static HttpContext HttpContext => CatchOrDefault(() => RootServices?.GetService<IHttpContextAccessor>()?.HttpContext);

    /// <summary>
    /// 异步上下文用户身份存储
    /// </summary>
    private static readonly AsyncLocal<ClaimsPrincipal> _asyncLocalUser = new();

    /// <summary>
    /// 获取请求上下文用户
    /// </summary>
    /// <remarks>只有授权访问的页面或接口才存在值，否则为 null</remarks>
    public static ClaimsPrincipal User
    {
        get
        {
            var httpContext = HttpContext;

            // 空检查
            if (httpContext?.User != null)
            {
                // 缓存当前线程的用户身份
                _asyncLocalUser.Value = httpContext.User;

                return httpContext.User;
            }

            return _asyncLocalUser.Value;
        }
    }

    /// <summary>
    /// 未托管的对象集合
    /// </summary>
    public static readonly ConcurrentBag<IDisposable> UnmanagedObjects;

    /// <summary>
    /// 单例服务类型缓存
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> _singletonServiceTypes = new();

    /// <summary>
    /// 解析服务提供器
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static IServiceProvider GetServiceProvider(Type serviceType)
    {
        // 处理控制台应用程序
        if (HostEnvironment == default) return RootServices;

        // 第一选择，判断是否是单例注册且单例服务不为空，如果是直接返回根服务提供器
        if (RootServices != null)
        {
            var serviceKey = serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType;
            var isSingleton = _singletonServiceTypes.GetOrAdd(serviceKey, key =>
                InternalApp.InternalServices.Any(u => u.ServiceType == key && u.Lifetime == ServiceLifetime.Singleton));
            if (isSingleton) return RootServices;
        }

        // 第二选择是获取 HttpContext 对象的 RequestServices
        var httpContext = HttpContext;
        if (httpContext?.RequestServices != null) return httpContext.RequestServices;
        // 第三选择，创建新的作用域并返回服务提供器
        else if (RootServices != null)
        {
            var scoped = RootServices.CreateScope();
            UnmanagedObjects.Add(scoped);
            return scoped.ServiceProvider;
        }
        // 第四选择，构建新的服务对象（性能最差）
        else
        {
            var serviceProvider = InternalApp.InternalServices.BuildServiceProvider();
            UnmanagedObjects.Add(serviceProvider);
            return serviceProvider;
        }
    }

    /// <summary>
    /// 获取请求生存周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static TService GetService<TService>(IServiceProvider serviceProvider = default)
        where TService : class
    {
        return GetService(typeof(TService), serviceProvider) as TService;
    }

    /// <summary>
    /// 获取请求生存周期的服务
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static object GetService(Type type, IServiceProvider serviceProvider = default)
    {
        return (serviceProvider ?? GetServiceProvider(type)).GetService(type);
    }

    /// <summary>
    /// 获取请求生存周期的服务集合
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IEnumerable<TService> GetServices<TService>(IServiceProvider serviceProvider = default)
        where TService : class
    {
        return (serviceProvider ?? GetServiceProvider(typeof(TService))).GetServices<TService>();
    }

    /// <summary>
    /// 获取请求生存周期的服务集合
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IEnumerable<object> GetServices(Type type, IServiceProvider serviceProvider = default)
    {
        return (serviceProvider ?? GetServiceProvider(type)).GetServices(type);
    }

    /// <summary>
    /// 获取请求生存周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static TService GetRequiredService<TService>(IServiceProvider serviceProvider = default)
        where TService : class
    {
        return GetRequiredService(typeof(TService), serviceProvider) as TService;
    }

    /// <summary>
    /// 获取请求生存周期的服务
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static object GetRequiredService(Type type, IServiceProvider serviceProvider = default)
    {
        return (serviceProvider ?? GetServiceProvider(type)).GetRequiredService(type);
    }

    /// <summary>
    /// 根据键获取请求生存周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="key"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static TService GetKeyedService<TService>(object? key, IServiceProvider serviceProvider = default)
        where TService : class
    {
        return (serviceProvider ?? GetServiceProvider(typeof(TService))).GetKeyedService<TService>(key);
    }

    /// <summary>
    /// 根据键获取请求生存周期的服务
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static object GetKeyedService(object? key, Type type, IServiceProvider serviceProvider = default)
    {
        return (serviceProvider ?? GetServiceProvider(type)).GetKeyedServices(type, key).FirstOrDefault();
    }

    /// <summary>
    /// 根据键获取请求生存周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="key"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static TService GetRequiredKeyedService<TService>(object? key, IServiceProvider serviceProvider = default)
        where TService : class
    {
        return (serviceProvider ?? GetServiceProvider(typeof(TService))).GetRequiredKeyedService<TService>(key);
    }

    /// <summary>
    /// 根据键获取请求生存周期的服务
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static object GetRequiredKeyedService(object? key, Type type, IServiceProvider serviceProvider = default)
    {
        return (serviceProvider ?? GetServiceProvider(type)).GetRequiredKeyedService(type, key);
    }

    /// <summary>
    /// 根据键获取请求生存周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="key"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IEnumerable<TService> GetKeyedServices<TService>(object? key, IServiceProvider serviceProvider = default)
        where TService : class
    {
        return (serviceProvider ?? GetServiceProvider(typeof(TService))).GetKeyedServices<TService>(key);
    }

    /// <summary>
    /// 根据键获取请求生存周期的服务
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IEnumerable<object> GetKeyedServices(object? key, Type type, IServiceProvider serviceProvider = default)
    {
        return (serviceProvider ?? GetServiceProvider(type)).GetKeyedServices(type, key);
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类</typeparam>
    /// <param name="path">配置中对应的Key</param>
    /// <param name="loadPostConfigure"></param>
    /// <returns>TOptions</returns>
    public static TOptions GetConfig<TOptions>(string path, bool loadPostConfigure = false)
    {
        var options = Configuration.GetSection(path).Get<TOptions>();

        // 加载默认选项配置
        if (loadPostConfigure && typeof(IConfigurableOptions).IsAssignableFrom(typeof(TOptions)))
        {
            var postConfigure = typeof(TOptions).GetMethod("PostConfigure");
            if (postConfigure != null)
            {
                options ??= Activator.CreateInstance<TOptions>();
                postConfigure.Invoke(options, [options, Configuration]);
            }
        }

        return options;
    }

    /// <summary>
    /// 获取选项
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类</typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns>TOptions</returns>
    public static TOptions GetOptions<TOptions>(IServiceProvider serviceProvider = default)
        where TOptions : class, new()
    {
        return Penetrates.GetOptionsOnStarting<TOptions>()
            ?? GetService<IOptions<TOptions>>(serviceProvider ?? RootServices)?.Value;
    }

    /// <summary>
    /// 获取选项
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类</typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns>TOptions</returns>
    public static TOptions GetOptionsMonitor<TOptions>(IServiceProvider serviceProvider = default)
        where TOptions : class, new()
    {
        return Penetrates.GetOptionsOnStarting<TOptions>()
            ?? GetService<IOptionsMonitor<TOptions>>(serviceProvider ?? RootServices)?.CurrentValue;
    }

    /// <summary>
    /// 获取选项
    /// </summary>
    /// <typeparam name="TOptions">强类型选项类</typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns>TOptions</returns>
    public static TOptions GetOptionsSnapshot<TOptions>(IServiceProvider serviceProvider = default)
        where TOptions : class, new()
    {
        // 这里不能从根服务解析，因为是 Scoped 作用域
        return Penetrates.GetOptionsOnStarting<TOptions>()
            ?? GetService<IOptionsSnapshot<TOptions>>(serviceProvider)?.Value;
    }

    /// <summary>
    /// 获取命令行配置
    /// </summary>
    /// <param name="args"></param>
    /// <param name="switchMappings"></param>
    /// <returns></returns>
    public static CommandLineConfigurationProvider GetCommandLineConfiguration(string[] args, IDictionary<string, string> switchMappings = null)
    {
        var commandLineConfiguration = new CommandLineConfigurationProvider(args, switchMappings);
        commandLineConfiguration.Load();

        return commandLineConfiguration;
    }

    /// <summary>
    /// 获取当前线程 Id
    /// </summary>
    /// <returns></returns>
    public static int GetThreadId()
    {
        return Environment.CurrentManagedThreadId;
    }

    /// <summary>
    /// 获取当前请求 TraceId
    /// </summary>
    /// <returns></returns>
    public static string GetTraceId()
    {
        return Activity.Current?.Id ?? (InternalApp.RootServices == null ? default : HttpContext?.TraceIdentifier);
    }

    /// <summary>
    /// 获取一段代码执行耗时
    /// </summary>
    /// <param name="action">委托</param>
    /// <returns><see cref="long"/></returns>
    public static long GetExecutionTime(Action action)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(action);

        // 计算接口执行时间
        var timeOperation = Stopwatch.StartNew();
        action();
        timeOperation.Stop();
        return timeOperation.ElapsedMilliseconds;
    }

    /// <summary>
    /// 获取服务注册的生命周期类型
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static ServiceLifetime? GetServiceLifetime(Type serviceType)
    {
        var serviceDescriptor = InternalApp.InternalServices
            .FirstOrDefault(u => u.ServiceType == (serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : serviceType));

        return serviceDescriptor?.Lifetime;
    }

    /// <summary>
    /// 编译 C# 类定义代码返回程序集
    /// </summary>
    /// <param name="csharpCode">字符串代码</param>
    /// <param name="assemblyName">自定义程序集名称</param>
    /// <param name="additionalAssemblies">附加的程序集</param>
    /// <returns><see cref="DynamicCompiledAssembly"/></returns>
    public static DynamicCompiledAssembly CompileCSharpClassCode(string csharpCode, string assemblyName = default, params Assembly[] additionalAssemblies)
    {
        // 编译代码
        using var memoryStream = CompileCSharpClassCodeToStream(csharpCode, assemblyName, additionalAssemblies);

        var alc = new AssemblyLoadContext("Furion.DynamicCompile", isCollectible: true);
        var assembly = alc.LoadFromStream(memoryStream);

        return new DynamicCompiledAssembly(assembly, alc);
    }

    /// <summary>
    /// 编译 C# 类定义代码保存为 dll 文件
    /// </summary>
    /// <param name="csharpCode">字符串代码</param>
    /// <param name="assemblyName">自定义程序集名称</param>
    /// <param name="additionalAssemblies">附加的程序集</param>
    /// <returns><see cref="DynamicCompiledAssembly"/></returns>
    public static DynamicCompiledAssembly CompileCSharpClassCodeToDllFile(string csharpCode, string assemblyName = default, params Assembly[] additionalAssemblies)
    {
        var assName = string.IsNullOrWhiteSpace(assemblyName)
            ? Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
            : Path.GetFileName(assemblyName.Trim());

        var dllPath = Path.Combine(AppContext.BaseDirectory, $"{assName}.dll");

        // 编译代码
        using var memoryStream = CompileCSharpClassCodeToStream(csharpCode, assName, additionalAssemblies);

        var tempPath = dllPath + ".tmp";
        using (var fileStream = new FileStream(
            path: tempPath,
            mode: FileMode.Create,
            access: FileAccess.Write,
            share: FileShare.None,
            bufferSize: 8192,
            useAsync: false))
        {
            memoryStream.CopyTo(fileStream);
        }
        File.Move(tempPath, dllPath, overwrite: true);

        // 加载程序集
        var alc = new AssemblyLoadContext("Furion.DynamicCompile", isCollectible: true);
        using var loadStream = new MemoryStream(memoryStream.ToArray());
        var assembly = alc.LoadFromStream(loadStream);

        return new DynamicCompiledAssembly(assembly, alc);
    }

    /// <summary>
    /// 编译 C# 类定义代码保存为 dll 文件
    /// </summary>
    /// <param name="csharpCode">字符串代码</param>
    /// <param name="assemblyName">自定义程序集名称</param>
    /// <param name="additionalAssemblies">附加的程序集</param>
    /// <returns><see cref="DynamicCompiledAssembly"/></returns>
    public static async Task<DynamicCompiledAssembly> CompileCSharpClassCodeToDllFileAsync(string csharpCode, string assemblyName = default, params Assembly[] additionalAssemblies)
    {
        var assName = string.IsNullOrWhiteSpace(assemblyName)
            ? Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
            : Path.GetFileName(assemblyName.Trim());

        var dllPath = Path.Combine(AppContext.BaseDirectory, $"{assName}.dll");

        // 编译代码
        using var memoryStream = CompileCSharpClassCodeToStream(csharpCode, assName, additionalAssemblies);

        var tempPath = dllPath + ".tmp";
        await using (var fileStream = new FileStream(
            path: tempPath,
            mode: FileMode.Create,
            access: FileAccess.Write,
            share: FileShare.None,
            bufferSize: 8192,
            useAsync: true))
        {
            await memoryStream.CopyToAsync(fileStream);
        }
        File.Move(tempPath, dllPath, overwrite: true);

        // 加载程序集
        var alc = new AssemblyLoadContext("Furion.DynamicCompile", isCollectible: true);
        using var loadStream = new MemoryStream(memoryStream.ToArray());
        var assembly = alc.LoadFromStream(loadStream);

        return new DynamicCompiledAssembly(assembly, alc);
    }

    /// <summary>
    /// 编译 C# 类定义代码返回内存流
    /// </summary>
    /// <param name="csharpCode">字符串代码</param>
    /// <param name="assemblyName">自定义程序集名称</param>
    /// <param name="additionalAssemblies">附加的程序集</param>
    /// <returns><see cref="MemoryStream"/></returns>
    public static MemoryStream CompileCSharpClassCodeToStream(string csharpCode, string assemblyName = default, params Assembly[] additionalAssemblies)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(csharpCode)) throw new ArgumentNullException(nameof(csharpCode));

        additionalAssemblies ??= [];

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var metadataReferences = new List<MetadataReference>();

        foreach (var assembly in additionalAssemblies)
        {
            if (assembly != null &&
                !string.IsNullOrEmpty(assembly.Location) &&
                File.Exists(assembly.Location) &&
                seen.Add(assembly.FullName ?? assembly.GetName().Name))
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                continue;

            if (seen.Add(assembly.FullName ?? assembly.GetName().Name))
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        // 生成语法树
        var syntaxTree = CSharpSyntaxTree.ParseText(
            csharpCode,
            new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None));

        // 创建编译单元
        var compilation = CSharpCompilation.Create(
            assemblyName: string.IsNullOrWhiteSpace(assemblyName) ? Path.GetRandomFileName() : assemblyName.Trim(),
            syntaxTrees: [syntaxTree],
            references: metadataReferences,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                nullableContextOptions: NullableContextOptions.Disable,
                warningLevel: 4,
                allowUnsafe: false,
                checkOverflow: false,
                deterministic: true,
                concurrentBuild: true
            ));

        // 编译代码
        var memoryStream = new MemoryStream();
        var emitResult = compilation.Emit(memoryStream);

        // 编译失败抛出异常
        if (!emitResult.Success)
        {
            memoryStream.Dispose();

            var errors = emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error || d.IsWarningAsError)
                .Select(d => d.ToString())
                .ToArray();

            throw new InvalidOperationException(
                $"Unable to compile class code:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        memoryStream.Position = 0;

        return memoryStream;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    static App()
    {
        // 未托管的对象
        UnmanagedObjects = [];

        // 加载程序集
        var assObject = GetAssemblies();
        Assemblies = assObject.Assemblies.ToList();
        ExternalAssemblies = assObject.ExternalAssemblies;
        PathOfExternalAssemblies = assObject.PathOfExternalAssemblies;

        // 获取有效的类型集合
        EffectiveTypes = Assemblies.SelectMany(GetTypes).ToList();

        AppStartups = [];
    }

    /// <summary>
    /// 应用所有启动配置对象
    /// </summary>
    internal static ConcurrentBag<AppStartup> AppStartups;

    /// <summary>
    /// 外部程序集
    /// </summary>
    internal static IEnumerable<Assembly> ExternalAssemblies;

    /// <summary>
    /// 外部程序集文件路径
    /// </summary>
    internal static IEnumerable<string> PathOfExternalAssemblies;

    /// <summary>
    /// 获取应用有效程序集
    /// </summary>
    /// <returns>IEnumerable</returns>
    private static (IEnumerable<Assembly> Assemblies, IEnumerable<Assembly> ExternalAssemblies, IEnumerable<string> PathOfExternalAssemblies) GetAssemblies()
    {
        // 需排除的程序集后缀
        var excludeAssemblyNames = new string[] {
                "Database.Migrations"
            };

        // 读取应用配置
        var supportPackageNamePrefixs = Settings.SupportPackageNamePrefixs ?? [];

        var loadedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        Assembly LoadOnce(string name)
        {
            if (string.IsNullOrEmpty(name) || !loadedNames.Add(name)) return null;
            return SafeGetAssembly(name);
        }

        IEnumerable<Assembly> scanAssemblies;

        // 获取入口程序集
        var entryAssembly = Assembly.GetEntryAssembly();

        // 非独立发布/非单文件发布
        if (!string.IsNullOrWhiteSpace(entryAssembly.Location))
        {
            // 查找 .deps.json 文件
            var depsJsonPath = Path.Combine(AppContext.BaseDirectory, $"{entryAssembly.GetName().Name}.deps.json");

            // 处理 IIS 等宿主进程导致入口程序集名称不匹配的情况
            if (!File.Exists(depsJsonPath))
            {
                var hostProcesses = new[] { "iisexpress", "w3wp", "testhost", "dotnet", "resharptestrunner", "microsoft.aspnetcore.testhost" };
                try
                {
                    var depsFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.deps.json", SearchOption.TopDirectoryOnly);
                    depsJsonPath = depsFiles.FirstOrDefault(f => !hostProcesses.Any(h => Path.GetFileNameWithoutExtension(f).Equals(h, StringComparison.OrdinalIgnoreCase)));
                }
                catch { }
            }

            // 存储程序集名称和对应的类型、运行时库名称列表和运行时库是否包含运行时资产
            var libraryTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var runtimeLibraryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var runtimeAssemblyFlags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(depsJsonPath) && File.Exists(depsJsonPath))
            {
                try
                {
                    using var stream = File.OpenRead(depsJsonPath);
                    using var doc = JsonDocument.Parse(stream);

                    // 解析 libraries 节点，获取所有库的类型
                    if (doc.RootElement.TryGetProperty("libraries", out var librariesElement))
                    {
                        foreach (var lib in librariesElement.EnumerateObject())
                        {
                            var parts = lib.Name.Split('/');
                            if (parts.Length > 0)
                            {
                                var name = parts[0];
                                var type = lib.Value.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
                                libraryTypes[name] = type;
                            }
                        }
                    }

                    // 获取当前运行时目标名称
                    string runtimeTargetName = null;
                    if (doc.RootElement.TryGetProperty("runtimeTarget", out var targetElement))
                    {
                        if (targetElement.ValueKind == JsonValueKind.Object && targetElement.TryGetProperty("name", out var nameProp))
                        {
                            runtimeTargetName = nameProp.GetString();
                        }
                        else if (targetElement.ValueKind == JsonValueKind.String)
                        {
                            runtimeTargetName = targetElement.GetString();
                        }
                    }

                    // 解析 targets 节点中对应 runtimeTarget 的库
                    if (!string.IsNullOrEmpty(runtimeTargetName) &&
                        doc.RootElement.TryGetProperty("targets", out var targetsElement) &&
                        targetsElement.TryGetProperty(runtimeTargetName, out var targetLibrariesElement))
                    {
                        foreach (var lib in targetLibrariesElement.EnumerateObject())
                        {
                            var parts = lib.Name.Split('/');
                            if (parts.Length > 0)
                            {
                                var name = parts[0];
                                runtimeLibraryNames.Add(name);

                                // 检查是否有运行时资产
                                var hasRuntime = false;
                                if (lib.Value.TryGetProperty("runtime", out var runtimeProp) && runtimeProp.ValueKind == JsonValueKind.Object)
                                {
                                    hasRuntime = runtimeProp.EnumerateObject().Any();
                                }
                                if (!hasRuntime && lib.Value.TryGetProperty("runtimeTargets", out var runtimeTargetsProp) && runtimeTargetsProp.ValueKind == JsonValueKind.Object)
                                {
                                    hasRuntime = runtimeTargetsProp.EnumerateObject().Any();
                                }
                                runtimeAssemblyFlags[name] = hasRuntime;
                            }
                        }
                    }
                }
                catch
                {
                    libraryTypes.Clear();
                    runtimeLibraryNames.Clear();
                    runtimeAssemblyFlags.Clear();
                }
            }

            if (runtimeLibraryNames.Count > 0)
            {
                // 读取项目程序集或 Furion 官方发布的包，或手动添加引用的dll，或配置特定的包前缀
                scanAssemblies = runtimeLibraryNames
                    .Select(name => new {
                        Name = name,
                        Type = libraryTypes.TryGetValue(name, out var t) ? t : null,
                        HasRuntimeAssemblies = runtimeAssemblyFlags.TryGetValue(name, out var has) && has
                    })
                    .Where(u =>
                    {
                        var name = u.Name;
                        var type = u.Type;

                        if (string.IsNullOrEmpty(type)) return false;

                        var isProject = type == "project";
                        var isPackage = type == "package";
                        var isReference = type == "reference";

                        // 判断是否是项目程序集且不在排除列表中
                        if (isProject && !excludeAssemblyNames.Any(j => name.EndsWith(j))) return true;

                        // 判断是否是包程序集
                        if (isPackage)
                        {
                            // 判断是否是 Furion 官方发布的包
                            if (name.StartsWith(nameof(Furion), StringComparison.OrdinalIgnoreCase)) return true;

                            // 判断是否匹配配置特定的包前缀，且存在运行时资产
                            if (supportPackageNamePrefixs.Any(p => IsMatchPattern(name, p) && u.HasRuntimeAssemblies)) return true;
                        }

                        // 判断是否启用引用程序集扫描
                        if (Settings.EnabledReferenceAssemblyScan == true && isReference) return true;

                        return false;
                    })
                    .Select(u => LoadOnce(u.Name))
                    .Where(a => a != null);
            }
            else
            {
                // 如果 .deps.json 无法解析或不存在，回退到已加载程序集扫描，同时排除系统程序集
                scanAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass =>
                    !ass.FullName.StartsWith(nameof(System)) &&
                    !ass.FullName.StartsWith(nameof(Microsoft)) &&
                    !ass.FullName.StartsWith("netstandard"));
            }
        }
        // 独立发布/单文件发布
        else
        {
            IEnumerable<Assembly> fixedSingleFileAssemblies = [entryAssembly];

            // 扫描实现 ISingleFilePublish 接口的类型
            var singleFilePublishType = entryAssembly.GetTypes()
                                                .FirstOrDefault(u => u.IsClass && !u.IsInterface && !u.IsAbstract && typeof(ISingleFilePublish).IsAssignableFrom(u));
            if (singleFilePublishType != null)
            {
                var singleFilePublish = Activator.CreateInstance(singleFilePublishType) as ISingleFilePublish;

                // 加载用户自定义配置单文件所需程序集
                var nativeAssemblies = singleFilePublish.IncludeAssemblies();
                var loadAssemblies = singleFilePublish.IncludeAssemblyNames().Select(LoadOnce).Where(a => a != null);

                fixedSingleFileAssemblies = fixedSingleFileAssemblies.Concat(nativeAssemblies).Concat(loadAssemblies);

                // 解决 Furion.Extras.ObjectMapper.Mapster 程序集不能加载问题
                try
                {
                    var mapsterAss = LoadOnce(ObjectMapperServiceCollectionExtensions.ASSEMBLY_NAME);
                    if (mapsterAss != null && !fixedSingleFileAssemblies.Any(u => u.GetName().Name.Equals(ObjectMapperServiceCollectionExtensions.ASSEMBLY_NAME)))
                    {
                        fixedSingleFileAssemblies = fixedSingleFileAssemblies.Concat([mapsterAss]);
                    }
                }
                catch { }
            }
            else
            {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                // 提示没有正确配置单文件配置
                Console.WriteLine(TP.Wrapper("Deploy Console"
                    , "Single file deploy error."
                    , "##Exception## Single file deployment configuration error."
                    , "##Documentation## https://furion.net/docs/singlefile"));
                Console.ResetColor();
            }

            // 通过 AppDomain.CurrentDomain 扫描，默认为延迟加载，正常只能扫描到 Furion 和 入口程序集（启动层）
            scanAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                                    .Where(ass =>
                                            // 排除 System，Microsoft，netstandard 开头的程序集
                                            !ass.FullName.StartsWith(nameof(System))
                                            && !ass.FullName.StartsWith(nameof(Microsoft))
                                            && !ass.FullName.StartsWith("netstandard"))
                                    .Concat(fixedSingleFileAssemblies)
                                    .Distinct();
        }

        IEnumerable<Assembly> externalAssemblies = [];
        IEnumerable<string> pathOfExternalAssemblies = [];

        // 加载 appsettings.json 配置的外部程序集
        if (Settings.ExternalAssemblies != null && Settings.ExternalAssemblies.Length != 0)
        {
            var externalList = new List<Assembly>();
            var pathList = new List<string>();
            var externalDlls = new List<string>();
            foreach (var item in Settings.ExternalAssemblies)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;

                var path = Path.Combine(AppContext.BaseDirectory, item);

                // 若以 .dll 结尾则认为是一个文件
                if (item.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(path)) externalDlls.Add(path);
                }
                // 否则作为目录查找或拼接 .dll 后缀作为文件名查找
                else
                {
                    // 作为目录查找所有 .dll 文件
                    if (Directory.Exists(path))
                    {
                        externalDlls.AddRange(Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories));
                    }
                    // 拼接 .dll 后缀查找
                    else
                    {
                        var pathDll = path + ".dll";
                        if (File.Exists(pathDll)) externalDlls.Add(pathDll);
                    }
                }
            }

            // 加载外部程序集
            foreach (var assemblyFileFullPath in externalDlls)
            {
                // 根据路径加载程序集
                var loadedAssembly = Reflect.LoadAssembly(assemblyFileFullPath);
                if (loadedAssembly == default) continue;

                if (!loadedNames.Add(loadedAssembly.GetName().Name)) continue;

                externalList.Add(loadedAssembly);
                pathList.Add(assemblyFileFullPath);
            }

            scanAssemblies = scanAssemblies.Concat(externalList);
            externalAssemblies = externalList;
            pathOfExternalAssemblies = pathList;
        }

        // 处理排除的程序集
        if (Settings.ExcludeAssemblies != null && Settings.ExcludeAssemblies.Length != 0)
        {
            scanAssemblies = scanAssemblies.Where(ass => !Settings.ExcludeAssemblies.Contains(ass.GetName().Name, StringComparer.OrdinalIgnoreCase));
        }

        return (scanAssemblies, externalAssemblies, pathOfExternalAssemblies);
    }

    /// <summary>
    /// 通配符匹配
    /// </summary>
    /// <param name="input"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    private static bool IsMatchPattern(string input, string pattern)
    {
        if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(input)) return false;

        // 如果不包含通配符，使用 StartsWith 前缀匹配行为（旧行为）
        if (pattern.IndexOf('*') < 0 && pattern.IndexOf('?') < 0)
        {
            return input.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
        }

        // 否则使用 Matcher 进行 Glob 匹配
        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        matcher.AddInclude(pattern);
        return matcher.Match(input).HasMatches;
    }

    /// <summary>
    /// 安全获取程序集
    /// </summary>
    /// <remarks>忽略 NuGet 扫描的脏依赖。</remarks>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    private static Assembly SafeGetAssembly(string assemblyName)
    {
        try
        {
            return Reflect.GetAssembly(assemblyName);
        }
        catch
        {
            Console.WriteLine($"Error load `{assemblyName}` assembly.");
            return null;
        }
    }

    /// <summary>
    /// 加载程序集中的所有类型
    /// </summary>
    /// <param name="ass"></param>
    /// <returns></returns>
    private static IEnumerable<Type> GetTypes(Assembly ass)
    {
        if (ass == null || ass.IsDefined(typeof(FurionAttribute), false))
        {
            return Array.Empty<Type>();
        }

        var types = Array.Empty<Type>();

        try
        {
            types = ass.GetTypes();
        }
        catch
        {
            Console.WriteLine($"Error load `{ass.FullName}` assembly.");
        }

        return types.Where(u =>
        {
            return (u.IsPublic || ObsoleteObjectExtensions.IsInternal(u))    // 支持 public 和 internal 声明类型
                   && !u.IsDefined(typeof(SuppressSnifferAttribute), false)
                   && !u.IsAnonymous(); // 排除匿名类型
        });
    }

    /// <summary>
    /// 释放所有未托管的对象
    /// </summary>
    public static void DisposeUnmanagedObjects()
    {
        foreach (var dsp in UnmanagedObjects)
        {
            try
            {
                dsp?.Dispose();
            }
            finally { }
        }

        // 强制手动回收 GC 内存
        if (!UnmanagedObjects.IsEmpty)
        {
            InternalGCUtility.Collect();
        }

        UnmanagedObjects.Clear();
    }

    /// <summary>
    /// 处理获取对象异常问题
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="action">获取对象委托</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>T</returns>
    private static T CatchOrDefault<T>(Func<T> action, T defaultValue = null)
        where T : class
    {
        try
        {
            return action();
        }
        catch
        {
            return defaultValue ?? null;
        }
    }
}