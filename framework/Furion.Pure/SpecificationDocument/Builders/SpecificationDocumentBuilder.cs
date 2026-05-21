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

using Furion.DynamicApiController;
using Furion.Extensions;
using Furion.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Furion.SpecificationDocument;

/// <summary>
/// 规范化文档构建器
/// </summary>
public static class SpecificationDocumentBuilder
{
    /// <summary>
    /// 所有分组默认的组名 Key
    /// </summary>
    private const string AllGroupsKey = "All Groups";

    /// <summary>
    /// 规范化文档配置
    /// </summary>
    private static readonly SpecificationDocumentSettingsOptions _specificationDocumentSettings;

    /// <summary>
    /// 应用全局配置
    /// </summary>
    private static readonly AppSettingsOptions _appSettings;

    /// <summary>
    /// 分组信息
    /// </summary>
    private static readonly IEnumerable<GroupExtraInfo> DocumentGroupExtras;

    /// <summary>
    /// 带排序的分组名
    /// </summary>
    private static readonly Regex _groupOrderRegex;

    /// <summary>
    /// 文档分组列表
    /// </summary>
    public static readonly List<string> DocumentGroups=new();

    /// <summary>
    /// 构造函数
    /// </summary>
    static SpecificationDocumentBuilder()
    {
        // 载入配置
        _specificationDocumentSettings = App.GetConfig<SpecificationDocumentSettingsOptions>("SpecificationDocumentSettings", true);
        _appSettings = App.Settings;

        // 初始化常量
        _groupOrderRegex = new Regex(@"@(?<order>[0-9]+$)", RegexOptions.Compiled);
        GetActionGroupsCached = new ConcurrentDictionary<MethodInfo, IEnumerable<GroupExtraInfo>>();
        GetControllerGroupsCached = new ConcurrentDictionary<Type, IEnumerable<GroupExtraInfo>>();
        GetGroupOpenApiInfoCached = new ConcurrentDictionary<string, SpecificationOpenApiInfo>();
        GetControllerTagCached = new ConcurrentDictionary<string, string>();
        GetActionTagCached = new ConcurrentDictionary<string, IList<string>>();

        // 默认分组，支持多个逗号分割
        DocumentGroupExtras = new List<GroupExtraInfo> { ResolveGroupExtraInfo(_specificationDocumentSettings.DefaultGroupName) };
        
    }

    /// <summary>
    /// 检查方法是否在分组中
    /// </summary>
    /// <param name="currentGroup"></param>
    /// <param name="apiDescription"></param>
    /// <returns></returns>
    public static bool CheckApiDescriptionInCurrentGroup(string currentGroup, ApiDescription apiDescription)
    {
        if (!apiDescription.TryGetMethodInfo(out var method)) return false;
        // 处理 Mvc 和 WebAPI 混合项目路由问题
        if (typeof(Controller).IsAssignableFrom(method.DeclaringType) && apiDescription.ActionDescriptor.ActionConstraints == null)
        {
            return false;
        }

        // 处理 All Groups
        if (currentGroup == AllGroupsKey)
        {
            return true;
        }
        
        // 判断是否是 Minimal API
        var isMinimalApi = apiDescription.ActionDescriptor is not ControllerActionDescriptor;
        if (isMinimalApi)
        {
            // var groupAttribute = apiDescription.ActionDescriptor?.EndpointMetadata?.OfType<EndpointGroupNameAttribute>().LastOrDefault();
            // var groupName = groupAttribute?.EndpointGroupName;
            // return (string.IsNullOrWhiteSpace(groupName) && currentGroup == _specificationDocumentSettings.DefaultGroupName) || currentGroup == groupName;
            
            // 如果用endpoint的groupname
            var endpointGroups = apiDescription.ActionDescriptor.EndpointMetadata.OfType<EndpointGroupNameAttribute>()
                .Select(gp=>gp.EndpointGroupName)
                .ToList();
            // 如果用WithMetadata方式添加ApiDescriptionSettingsAttribute
            var apiDescriptionSettingsGroups = apiDescription.ActionDescriptor.EndpointMetadata
                .OfType<ApiDescriptionSettingsAttribute>()
                // 不确定为什么有GroupName, 先兼容一下, 反正也是group, 但没有改 GetActionGroups 方法, 添加 GroupName
                .SelectMany(setting=> (List<string>)[..setting.Groups, setting.GroupName])
                .ToList();

            List<string> groups = [..endpointGroups, ..apiDescriptionSettingsGroups];                
            var isInDefaultGroup = currentGroup == _specificationDocumentSettings.DefaultGroupName;
            return  (groups.Count == 0 && isInDefaultGroup) || groups.Contains(currentGroup) ;
        }
        else
        {
            // 处理贴有 [ApiExplorerSettings(IgnoreApi = true)] 或者 [ApiDescriptionSettings(false)] 特性的接口
            var apiExplorerSettings = method.GetFoundAttribute<ApiExplorerSettingsAttribute>(true, true);
            var apiDescriptionSettings = method.GetFoundAttribute<ApiDescriptionSettingsAttribute>(true, true);
            if (apiExplorerSettings?.IgnoreApi == true || apiDescriptionSettings?.IgnoreApi == true) return false;

            return GetActionGroups(method).Any(u => u.Group == currentGroup);
        }
    }

    /// <summary>
    /// 获取所有的规范化分组信息
    /// </summary>
    /// <returns></returns>
    public static List<SpecificationOpenApiInfo> GetOpenApiGroups()
    {
        var openApiGroups = new List<SpecificationOpenApiInfo>();
        foreach (var group in DocumentGroups)
        {
            openApiGroups.Add(GetGroupOpenApiInfo(group));
        }

        return openApiGroups;
    }

    /// <summary>
    /// 获取分组信息缓存集合
    /// </summary>
    private static readonly ConcurrentDictionary<string, SpecificationOpenApiInfo> GetGroupOpenApiInfoCached;

    /// <summary>
    /// 获取分组配置信息
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public static SpecificationOpenApiInfo GetGroupOpenApiInfo(string group)
    {
        return GetGroupOpenApiInfoCached.GetOrAdd(group, u =>
        {
            var groupInfo = CreateBaseGroupOpenApiInfo(u);
            ApplyExternalConfig(groupInfo, u);
            return groupInfo;
        });
    }

    /// <summary>
    /// 创建基础的分组信息
    /// </summary>
    private static SpecificationOpenApiInfo CreateBaseGroupOpenApiInfo(string group)
    {
        // 替换路由模板
        var routeTemplate = _specificationDocumentSettings.RouteTemplate.Replace("{documentName}", Uri.EscapeDataString(group));
        if (!string.IsNullOrWhiteSpace(_specificationDocumentSettings.ServerDir))
        {
            routeTemplate = _specificationDocumentSettings.ServerDir + "/" + routeTemplate;
        }

        // 处理虚拟目录问题
        var template = $"{_appSettings.VirtualPath}/{routeTemplate}";

        var groupInfo = _specificationDocumentSettings.GroupOpenApiInfos.FirstOrDefault(u => u.Group == group);
        if (groupInfo != null)
        {
            groupInfo.RouteTemplate = template;
            groupInfo.Title ??= group;
        }
        else
        {
            groupInfo = new SpecificationOpenApiInfo { Group = group, RouteTemplate = template };
        }

        return groupInfo;
    }

    /// <summary>
    /// 应用外部配置（appsettings.json 的 [openapi:{group}] 节点）
    /// </summary>
    private static void ApplyExternalConfig(SpecificationOpenApiInfo groupInfo, string group)
    {
        var groupKey = string.Format("[openapi:{0}]", group);
        if (!App.Configuration.Exists(groupKey)) return;

        SetProperty<int>(group, nameof(SpecificationOpenApiInfo.Order), value => groupInfo.Order = value);
        SetProperty<bool>(group, nameof(SpecificationOpenApiInfo.Visible), value => groupInfo.Visible = value);
        SetProperty<string>(group, nameof(SpecificationOpenApiInfo.RouteTemplate), value => groupInfo.RouteTemplate = value);
        SetProperty<string>(group, nameof(SpecificationOpenApiInfo.Title), value => groupInfo.Title = value);
        SetProperty<string>(group, nameof(SpecificationOpenApiInfo.Description), value => groupInfo.Description = value);
        SetProperty<string>(group, nameof(SpecificationOpenApiInfo.Version), value => groupInfo.Version = value);
        SetProperty<Uri>(group, nameof(SpecificationOpenApiInfo.TermsOfService), value => groupInfo.TermsOfService = value);
        SetProperty<OpenApiContact>(group, nameof(SpecificationOpenApiInfo.Contact), value => groupInfo.Contact = value);
        SetProperty<OpenApiLicense>(group, nameof(SpecificationOpenApiInfo.License), value => groupInfo.License = value);
    }


    /// <summary>
    /// 设置额外配置的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="group"></param>
    /// <param name="propertyName"></param>
    /// <param name="action"></param>
    private static void SetProperty<T>(string group, string propertyName, Action<T> action)
    {
        var propertyKey = string.Format("[openapi:{0}]:{1}", group, propertyName);
        if (App.Configuration.Exists(propertyKey))
        {
            var value = App.GetConfig<T>(propertyKey);
            action?.Invoke(value);
        }
    }

    private static IApplicationBuilder? _app = null;
    /// <summary>
    /// 构建Swagger全局配置
    /// </summary>
    /// <param name="swaggerOptions">Swagger 全局配置</param>
    /// <param name="configure"></param>
    /// <param name="app">需要app获取endpoint的datasource</param>
    internal static void Build(SwaggerOptions swaggerOptions, Action<SwaggerOptions>? configure = null, IApplicationBuilder app = null)
    {
        _app = app;
        // 加载所有分组
        DocumentGroups.AddRange(ReadGroups());

        // 生成V2版本
        swaggerOptions.OpenApiVersion = _specificationDocumentSettings.FormatAsV2 == true
            ? OpenApiSpecVersion.OpenApi2_0
            : _specificationDocumentSettings.LatestVersion == true
                ? OpenApiSpecVersion.OpenApi3_1
                : OpenApiSpecVersion.OpenApi3_0;

        // 判断是否启用 Server
        if (_specificationDocumentSettings.HideServers != true)
        {
            // 启动服务器 Servers
            swaggerOptions.PreSerializeFilters.Add((swagger, request) =>
            {
                // 默认 Server
                var servers = new List<OpenApiServer> {
                        new() { Url = $"{request.Scheme}://{request.Host.Value}{_appSettings.VirtualPath}",Description="Default" }
                };
                servers.AddRange(_specificationDocumentSettings.Servers);

                swagger.Servers = servers;
            });
        }

        // 配置路由模板
        swaggerOptions.RouteTemplate = _specificationDocumentSettings.RouteTemplate;

        // 自定义配置
        configure?.Invoke(swaggerOptions);
    }

    /// <summary>
    /// Swagger 生成器构建
    /// </summary>
    /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
    /// <param name="configure">自定义配置</param>
    internal static void BuildGen(SwaggerGenOptions swaggerGenOptions, Action<SwaggerGenOptions> configure = null)
    {
        // 创建分组文档
        CreateSwaggerDocs(swaggerGenOptions);

        // 加载分组控制器和动作方法列表
        LoadGroupControllerWithActions(swaggerGenOptions);

        // 配置 Swagger OperationIds
        ConfigureOperationIds(swaggerGenOptions);

        // 配置 Swagger SchemaId
        ConfigureSchemaIds(swaggerGenOptions);

        // 配置标签
        ConfigureTagsAction(swaggerGenOptions);

        // 配置 Action 排序
        ConfigureActionSequence(swaggerGenOptions);

        if (_specificationDocumentSettings.EnableXmlComments == true)
        {
            // 加载注释描述文件
            LoadXmlComments(swaggerGenOptions);
        }

        // 配置授权
        ConfigureSecurities(swaggerGenOptions);

        //使得 Swagger 能够正确地显示 Enum 的对应关系
        if (_specificationDocumentSettings.EnableEnumSchemaFilter == true) swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();

        // 修复 editor.swagger.io 生成不能正常处理 C# object 类型问题
        swaggerGenOptions.SchemaFilter<AnySchemaFilter>();

        // 添加 Action 操作过滤器
        swaggerGenOptions.OperationFilter<ApiActionFilter>();

        // 自定义配置
        configure?.Invoke(swaggerGenOptions);

        // 支持控制器排序操作
        if (_specificationDocumentSettings.EnableTagsOrderDocumentFilter == true) swaggerGenOptions.DocumentFilter<TagsOrderDocumentFilter>();
    }

    /// <summary>
    /// Swagger UI 构建
    /// </summary>
    /// <param name="swaggerUIOptions"></param>
    /// <param name="routePrefix"></param>
    /// <param name="configure"></param>
    /// <param name="withProxy">解决 Swagger 被代理问题</param>
    internal static void BuildUI(SwaggerUIOptions swaggerUIOptions, string routePrefix = default, Action<SwaggerUIOptions> configure = null, bool withProxy = false)
    {
        // 配置分组终点路由
        CreateGroupEndpoint(swaggerUIOptions, routePrefix, withProxy);

        // 配置文档标题
        swaggerUIOptions.DocumentTitle = _specificationDocumentSettings.DocumentTitle;

        // 配置UI地址（处理二级虚拟目录）
        swaggerUIOptions.RoutePrefix = _specificationDocumentSettings.RoutePrefix ?? routePrefix ?? "api";

        // 文档展开设置
        swaggerUIOptions.DocExpansion(_specificationDocumentSettings.DocExpansionState.Value);

        // 自定义 Swagger 首页
        CustomizeIndex(swaggerUIOptions);

        // 配置多语言和自动登录token
        AddDefaultInterceptor(swaggerUIOptions);

        // 自定义配置
        configure?.Invoke(swaggerUIOptions);
        
        // 要求UseInject必须放在minimalapi注册之后,所以补充根路径 "/"跳转到
        if (string.IsNullOrEmpty(routePrefix))
        {
            // 检查现有路由表中是否已经有根路径 "/"
            var hasRootEndpoint = endpointDatasources?
                .SelectMany(ds => ds.Endpoints)
                .OfType<RouteEndpoint>()
                .Any(re => re.RoutePattern.RawText == string.Empty || re.RoutePattern.RawText == "/")??false;

            // 3. 只有当开发者自己没有写首页路由时，我们的 Swagger 才去接管首页
            if (!hasRootEndpoint)
            {
                endpointRouteBuilder?.MapGet("/", async context =>
                {
                    context.Response.Redirect("index.html", permanent: false);
                    await Task.CompletedTask;
                }).WithOrder(int.MinValue); // 确保击穿后再兜底
            }
        }
    }

    /// <summary>
    /// 创建分组文档
    /// </summary>
    /// <param name="swaggerGenOptions">Swagger生成器对象</param>
    private static void CreateSwaggerDocs(SwaggerGenOptions swaggerGenOptions)
    {
        foreach (var group in DocumentGroups)
        {
            if (swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.ContainsKey(group)) continue;

            var groupOpenApiInfo = GetGroupOpenApiInfo(group) as OpenApiInfo;
            swaggerGenOptions.SwaggerDoc(group, groupOpenApiInfo);
        }
    }

    /// <summary>
    /// 加载分组控制器和动作方法列表
    /// </summary>
    /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
    private static void LoadGroupControllerWithActions(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.DocInclusionPredicate(CheckApiDescriptionInCurrentGroup);
    }

    /// <summary>
    ///  配置标签
    /// </summary>
    /// <param name="swaggerGenOptions"></param>
    private static void ConfigureTagsAction(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.TagActionsBy(apiDescription => GetActionTag(apiDescription));
    }

    /// <summary>
    /// 默认 ApiDescriptionSettings 特性实例
    /// </summary>
    private static readonly ApiDescriptionSettingsAttribute DefaultApiDescriptionSettings = new();

    /// <summary>
    ///  配置 Action 排序
    /// </summary>
    /// <param name="swaggerGenOptions"></param>
    private static void ConfigureActionSequence(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.OrderActionsBy(apiDesc =>
        {
            var apiDescriptionSettings = apiDesc.CustomAttributes()
                                   .OfType<ApiDescriptionSettingsAttribute>()
                                   .FirstOrDefault() ?? DefaultApiDescriptionSettings;

            return (int.MaxValue - apiDescriptionSettings.Order).ToString()
                            .PadLeft(int.MaxValue.ToString().Length, '0');
        });
    }

    /// <summary>
    /// 配置 Swagger OperationIds
    /// </summary>
    /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
    private static void ConfigureOperationIds(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.CustomOperationIds(apiDescription =>
        {
            var isMethod = apiDescription.TryGetMethodInfo(out var method);

            // 判断是否自定义了 [OperationId] 特性
            if (isMethod && method.IsDefined(typeof(OperationIdAttribute), false))
            {
                return method.GetCustomAttribute<OperationIdAttribute>(false).OperationId;
            }

            var operationId = apiDescription.RelativePath.Replace("/", "-")
                                       .Replace("{", "-")
                                       .Replace("}", "-") + "-" + apiDescription.HttpMethod.ToLower().ToUpperCamelCase();

            return operationId.Replace("--", "-");
        });
    }

    /// <summary>
    /// 配置 Swagger SchemaIds
    /// </summary>
    /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
    private static void ConfigureSchemaIds(SwaggerGenOptions swaggerGenOptions)
    {
        // 本地函数
        static string DefaultSchemaIdSelector(Type modelType)
        {
            var modelName = modelType.Name;

            // 处理泛型类型问题
            if (modelType.IsConstructedGenericType)
            {
                var prefix = modelType.GetGenericArguments()
                    .Select(genericArg => DefaultSchemaIdSelector(genericArg))
                    .Aggregate((previous, current) => previous + current);

                // 通过 _ 拼接多个泛型
                modelName = modelName.Split('`').First() + "_" + prefix;
            }

            // 判断是否自定义了 [SchemaId] 特性，解决模块化多个程序集命名冲突
            var isCustomize = modelType.IsDefined(typeof(SchemaIdAttribute));
            if (isCustomize)
            {
                var schemaIdAttribute = modelType.GetCustomAttribute<SchemaIdAttribute>();
                if (!schemaIdAttribute.Replace) return schemaIdAttribute.SchemaId + modelName;
                else return schemaIdAttribute.SchemaId;
            }

            return modelName;
        }

        // 调用本地函数
        swaggerGenOptions.CustomSchemaIds(modelType => DefaultSchemaIdSelector(modelType));
    }

    /// <summary>
    /// 加载注释描述文件
    /// </summary>
    /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
    private static void LoadXmlComments(SwaggerGenOptions swaggerGenOptions)
    {
        var xmlComments = _specificationDocumentSettings.XmlComments ?? [];

        foreach (var xmlComment in xmlComments)
        {
            var assemblyXmlName = xmlComment.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ? xmlComment : $"{xmlComment}.xml";
            var assemblyXmlPath = Path.Combine(AppContext.BaseDirectory, assemblyXmlName);

            if (!File.Exists(assemblyXmlPath))
            {
                continue;
            }

            var xmlDoc = XDocument.Load(assemblyXmlPath, LoadOptions.PreserveWhitespace);

            // 获取所有成员节点
            var members = xmlDoc.XPathSelectElements("/doc/members/member[@name]").ToList();

            // 构建成员索引字典
            var memberIndex = new Dictionary<string, XElement>(StringComparer.Ordinal);
            foreach (var memberElement in members)
            {
                var nameAttr = memberElement.Attribute("name");
                if (nameAttr == null) continue;

                var memberName = nameAttr.Value;
                // 普通注释：存入索引供 inheritdoc 引用
                memberIndex[memberName] = memberElement;
            }

            // 处理 inheritdoc 注释
            foreach (var memberElement in members)
            {
                var nameAttr = memberElement.Attribute("name");
                if (nameAttr == null) continue;

                var inheritdocElement = memberElement.Element("inheritdoc");
                if (inheritdocElement != null)
                {
                    // inheritdoc 注释：解析 cref 并替换内容
                    ProcessInheritdoc(memberElement, inheritdocElement, memberIndex, nameAttr.Value);
                }
            }

            swaggerGenOptions.IncludeXmlComments(() => new XPathDocument(xmlDoc.CreateReader()), true);
        }
    }

    /// <summary>
    /// 处理 inheritdoc 注释节点
    /// </summary>
    private static void ProcessInheritdoc(XElement memberElement, XElement inheritdocElement, Dictionary<string, XElement> memberIndex, string memberName)
    {
        var cref = inheritdocElement.Attribute("cref")?.Value;

        if (string.IsNullOrEmpty(cref))
        {
            cref = ResolveInheritdocCref(memberName, memberIndex);
        }

        if (!string.IsNullOrWhiteSpace(cref) &&
            memberIndex.TryGetValue(cref, out var realDocMember))
        {
            memberElement.SetAttributeValue("_ref_", cref);
            inheritdocElement.Parent?.ReplaceNodes(realDocMember.Nodes());
        }
    }

    /// <summary>
    /// 解析 inheritdoc 的 cref 属性（当未显式指定时）
    /// </summary>
    private static string ResolveInheritdocCref(string memberName, Dictionary<string, XElement> memberIndex)
    {
        if (memberName.Contains('#'))
        {
            return ResolveImplicitInterfaceCref(memberName);
        }

        if (memberName.Contains('('))
        {
            var match = s_memberNameRegex.Match(memberName);
            if (!match.Success) return null;

            var noParams = match.Value;
            var className = ExtractClassName(noParams);
            return GenerateInheritdocCref(memberIndex, memberName, className);
        }

        var className2 = ExtractClassName(memberName);
        return GenerateInheritdocCref(memberIndex, memberName, className2);
    }

    /// <summary>
    /// 解析隐式接口实现的 cref
    /// </summary>
    private static string ResolveImplicitInterfaceCref(string memberName)
    {
        if (memberName.IndexOf('#') < 0) return memberName;

        var prefixEnd = memberName.IndexOf(':');
        if (prefixEnd < 0) return memberName;

        var prefix = memberName[..(prefixEnd + 1)];
        var rest = memberName[(prefixEnd + 1)..];
        var resolved = rest.Replace('#', '.');
        return prefix + resolved;
    }

    /// <summary>
    /// 从成员名称中提取类名前缀
    /// </summary>
    private static string ExtractClassName(string memberName)
    {
        var start = memberName.IndexOf(':');
        var end = memberName.LastIndexOf('.');

        if (start < 0 || end <= start) return string.Empty;

        return memberName[start..end];
    }

    /// <summary>
    /// 生成 inheritdoc 的目标 cref 值
    /// </summary>
    private static string GenerateInheritdocCref(Dictionary<string, XElement> memberIndex, string memberName, string className)
    {
        var classKey = "T" + className;
        if (!memberIndex.TryGetValue(classKey, out var classElement)) return null;

        var refValue = classElement.Attribute("_ref_")?.Value;
        if (string.IsNullOrEmpty(refValue)) return null;

        var colonIndex = refValue.IndexOf(':');
        if (colonIndex < 0) return null;

        var classCrefSuffix = refValue.Substring(colonIndex);
        return memberName.Replace(className, classCrefSuffix);
    }

    /// <summary>
    /// 静态编译的正则表达式
    /// </summary>
    private static readonly Regex s_memberNameRegex = new(@"[A-Z]:[a-zA-Z0-9_@.]+", RegexOptions.Compiled);

    /// <summary>
    /// 配置授权
    /// </summary>
    /// <param name="swaggerGenOptions">Swagger 生成器配置</param>
    private static void ConfigureSecurities(SwaggerGenOptions swaggerGenOptions)
    {
        // 判断是否启用了授权
        if (_specificationDocumentSettings.EnableAuthorized != true || _specificationDocumentSettings.SecurityDefinitions.Length == 0) return;

        // 生成安全定义
        foreach (var securityDefinition in _specificationDocumentSettings.SecurityDefinitions)
        {
            // Id 必须定义
            if (string.IsNullOrWhiteSpace(securityDefinition.Id)
                || swaggerGenOptions.SwaggerGeneratorOptions.SecuritySchemes.ContainsKey(securityDefinition.Id)) continue;

            // 添加安全定义
            var openApiSecurityScheme = securityDefinition as OpenApiSecurityScheme;
            swaggerGenOptions.AddSecurityDefinition(securityDefinition.Id, openApiSecurityScheme);
        }

        // 添加安全需求
        swaggerGenOptions.AddSecurityRequirement(document =>
        {
            var openApiSecurityRequirement = new OpenApiSecurityRequirement();

            foreach (var securityDefinition in _specificationDocumentSettings.SecurityDefinitions)
            {
                // Id 必须定义
                if (string.IsNullOrWhiteSpace(securityDefinition.Id)) continue;

                openApiSecurityRequirement.Add(new OpenApiSecuritySchemeReference(securityDefinition.Id, document), securityDefinition.Requirement?.Accesses ?? []);
            }

            return openApiSecurityRequirement;
        });
    }

    /// <summary>
    /// 配置分组终点路由
    /// </summary>
    /// <param name="swaggerUIOptions"></param>
    /// <param name="routePrefix"></param>
    /// <param name="withProxy">解决 Swagger 被代理问题</param>
    private static void CreateGroupEndpoint(SwaggerUIOptions swaggerUIOptions, string routePrefix = default, bool withProxy = false)
    {
        var routePrefixArrs = (routePrefix ?? swaggerUIOptions.RoutePrefix).Split('/', StringSplitOptions.RemoveEmptyEntries);
        var routePrefixList = routePrefixArrs.Length == 0 ? routePrefixArrs.Concat([string.Empty]) : routePrefixArrs;

        foreach (var group in DocumentGroups)
        {
            var groupOpenApiInfo = GetGroupOpenApiInfo(group);

            swaggerUIOptions.SwaggerEndpoint((withProxy ? string.Join(string.Empty, routePrefixList.Select(c => "../")) : "/") + groupOpenApiInfo.RouteTemplate.TrimStart('/'), groupOpenApiInfo?.Title ?? group);
        }
    }

    /// <summary>
    /// 自定义 Swagger 首页
    /// </summary>
    /// <param name="swaggerUIOptions"></param>
    private static void CustomizeIndex(SwaggerUIOptions swaggerUIOptions)
    {
        var thisType = typeof(SpecificationDocumentBuilder);
        var thisAssembly = thisType.Assembly;

        // 获取自定义 Swagger 页面
        var customIndex = $"{Reflect.GetAssemblyName(thisAssembly)}{thisType.Namespace.Replace(nameof(Furion), string.Empty)}.Assets.index.html";
        swaggerUIOptions.IndexStream = () =>
        {
            StringBuilder htmlBuilder;
            // 自定义首页模板参数
            var indexArguments = new Dictionary<string, string>
            {
                {"%(VirtualPath)", _appSettings.VirtualPath }
            };

            // 读取文件内容
            using (var stream = thisAssembly.GetManifestResourceStream(customIndex))
            {
                using var reader = new StreamReader(stream);
                htmlBuilder = new StringBuilder(reader.ReadToEnd());
            }

            // 替换模板参数
            foreach (var (template, value) in indexArguments)
            {
                htmlBuilder.Replace(template, value);
            }

            // 返回新的内存流
            var byteArray = Encoding.UTF8.GetBytes(htmlBuilder.ToString());
            return new MemoryStream(byteArray);
        };

        // 添加登录信息配置
        var additionals = _specificationDocumentSettings.LoginInfo;
        if (additionals != null)
        {
            swaggerUIOptions.ConfigObject.AdditionalItems.Add(nameof(_specificationDocumentSettings.LoginInfo), new JsonObject
            {
                [nameof(SpecificationLoginInfo.Enabled)] = additionals.Enabled || (App.HostEnvironment.IsProduction() && additionals.EnableOnProduction),
                [nameof(SpecificationLoginInfo.CheckUrl)] = additionals.CheckUrl,
                [nameof(SpecificationLoginInfo.SubmitUrl)] = additionals.SubmitUrl,
                [nameof(SpecificationLoginInfo.DefaultUsername)] = additionals.DefaultUsername,
                [nameof(SpecificationLoginInfo.DefaultPassword)] = additionals.DefaultPassword
            });
        }

        // 添加深色主题
        swaggerUIOptions.ConfigObject.AdditionalItems.Add(nameof(_specificationDocumentSettings.DarkMode), _specificationDocumentSettings.DarkMode == true);
    }

    /// <summary>
    /// 添加默认请求/响应拦截器
    /// </summary>
    /// <param name="swaggerUIOptions"></param>
    private static void AddDefaultInterceptor(SwaggerUIOptions swaggerUIOptions)
    {
        // 配置多语言和自动登录token
        swaggerUIOptions.UseRequestInterceptor("function(request) { return defaultRequestInterceptor(request); }");
        swaggerUIOptions.UseResponseInterceptor("function(response) { return defaultResponseInterceptor(response); }");
    }
    
    private static IEnumerable<EndpointDataSource>? endpointDatasources = null;
    private static IEndpointRouteBuilder? endpointRouteBuilder = null;
    private static IEnumerable<EndpointDataSource> GetMinimalApiDataSources(IApplicationBuilder app)
    {
        // 1. 优先尝试直接强转（适用于 WebApplication 或已执行 UseRouting 的场景）
        if (app is IEndpointRouteBuilder routeBuilder)
        {
            endpointRouteBuilder = routeBuilder;
            return routeBuilder.DataSources;
        }
    
        // 2. 如果强转失败，去 Properties 字典中寻找内部隐藏的 RouteBuilder
        //    __EndpointRouteBuilder 是微软内部定义好的标准 Key
        if (app.Properties.TryGetValue("__EndpointRouteBuilder", out var obj) 
            && obj is IEndpointRouteBuilder hiddenRouteBuilder)
        {
            endpointRouteBuilder = hiddenRouteBuilder;
            return hiddenRouteBuilder.DataSources;
        }
    
        // 3. 如果还是没有，说明当前阶段 UseRouting() 还没被调用，路由数据尚未初始化
        // 此时可以选择返回空，或者抛出更明确的异常提醒调用时机太早
        return endpointDatasources = Enumerable.Empty<EndpointDataSource>();
    }


    /// <summary>
    /// 读取所有分组信息
    /// </summary>
    private static List<string> ReadGroups()
    {
        var finalGroups = new List<GroupExtraInfo>();

        // 配置文件定义的分组
        if (_specificationDocumentSettings.GroupOpenApiInfos?.Any() == true)
        {
            finalGroups.AddRange(_specificationDocumentSettings.GroupOpenApiInfos
                .Where(u => !string.IsNullOrWhiteSpace(u.Group))
                .Select(u => new GroupExtraInfo
                {
                    Group = u.Group,
                    Order = u.Order ?? 0,
                    Visible = u.Visible ?? true
                }));
        }
        
        // 获取minimalapi的分组        
        var endpointSources = GetMinimalApiDataSources(_app);
        // 过滤掉名字包含 "ControllerActionEndpointDataSource" 的数据源, (因为下面原来已经有了controller action的判断, 排除掉controller的endpoint)
        // 目前调试发现 RouteEndpointDataSource, RouteGroupBuilder.GroupEndpointDataSource 和 ControllerActionEndpointDataSource
        var minimalApiSources = endpointSources
            .Where(src => src.GetType().Name != "ControllerActionEndpointDataSource")
            .ToList();
        foreach (var source in minimalApiSources)
        {
            foreach (var endpoint in source.Endpoints)
            {
                // 1. 获取 WithMetadata(ApiDescriptionSettingsAttribute) 设置的分组
                var metaAttr = endpoint.Metadata.GetMetadata<ApiDescriptionSettingsAttribute>();
                var apiDescriptionSettingsGroups = endpoint.Metadata.OfType<ApiDescriptionSettingsAttribute>()
                    .SelectMany(m => {
                        List<string> gps = m.Groups.ToList();
                        // 这里猜测GroupName也可以作为group的, 不知道是不是这个用意
                        if (!string.IsNullOrEmpty(m.GroupName))
                        {
                            gps.Add(m.GroupName);
                        }

                        return gps.Select(g => new GroupExtraInfo
                        {
                            Group = g,
                            Order = metaAttr.Order,
                            Visible = true
                        }).Where(g=>!string.IsNullOrWhiteSpace(g.Group));
                    })
                    .ToList();
                if (apiDescriptionSettingsGroups.Any())
                {
                    finalGroups.AddRange(apiDescriptionSettingsGroups);
                }
                
                // 2. 获取 WithGroupName() 设置的分组
                var endpointGroups = endpoint.Metadata.OfType<EndpointGroupNameAttribute>()
                    .Select(m => m.EndpointGroupName)
                    .Where(name=>!string.IsNullOrWhiteSpace(name))
                    .Select(name=>new GroupExtraInfo
                    {
                        Group = name,
                        // Order = metaAttr.Order,
                        Visible = true
                    })
                    .ToList();
                if (endpointGroups.Any())
                {
                    finalGroups.AddRange(endpointGroups);
                }
            }
        }
        
        // 获取所有的控制器和Action方法 
        var controllers = App.EffectiveTypes.Where(Penetrates.IsApiController).ToArray();

        if (controllers.Length == 0)
        {
            // 添加默认分组
            if (!string.IsNullOrWhiteSpace(_specificationDocumentSettings.DefaultGroupName))
            {
                finalGroups.Add(new GroupExtraInfo
                {
                    Group = _specificationDocumentSettings.DefaultGroupName,
                    Order = 0,
                    Visible = true
                });
            }
        }
        else
        {
            var actions = controllers.SelectMany(c =>
                c.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(u => IsApiAction(u, c)));

            // 合并所有分组
            var groupOrders = controllers.SelectMany(GetControllerGroups)
                .Concat(actions.SelectMany(GetActionGroups))
                .Where(u => u?.Visible == true && !string.IsNullOrWhiteSpace(u.Group))
                .GroupBy(u => u.Group)
                .Select(g => new GroupExtraInfo
                {
                    Group = g.Key,
                    Order = g.Max(x => x.Order),
                    Visible = true
                });

            finalGroups.AddRange(groupOrders);
        }

        // 分组排序去重
        var sortedGroups = finalGroups
            .OrderByDescending(u => u.Order)
            .ThenBy(u => u.Group)
            .Select(u => u.Group)
            .Union(_specificationDocumentSettings.PackagesGroups ?? [])
            .Distinct()
            .ToList();

        // 启用总分组功能
        if (_specificationDocumentSettings.EnableAllGroups == true)
        {
            sortedGroups.Add(AllGroupsKey);
        }

        return sortedGroups;
    }

    /// <summary>
    /// 获取控制器组缓存集合
    /// </summary>
    private static readonly ConcurrentDictionary<Type, IEnumerable<GroupExtraInfo>> GetControllerGroupsCached;

    /// <summary>
    /// 获取控制器分组列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<GroupExtraInfo> GetControllerGroups(Type type)
    {
        return GetControllerGroupsCached.GetOrAdd(type, Function);

        // 本地函数
        static IEnumerable<GroupExtraInfo> Function(Type type)
        {
            // 如果控制器没有定义 [ApiDescriptionSettings] 特性，则返回默认分组
            if (!type.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return DocumentGroupExtras;

            // 读取分组
            var apiDescriptionSettings = type.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
            if (apiDescriptionSettings.Groups == null || apiDescriptionSettings.Groups.Length == 0) return DocumentGroupExtras;

            // 处理分组额外信息
            var groupExtras = new List<GroupExtraInfo>();
            foreach (var group in apiDescriptionSettings.Groups)
            {
                groupExtras.Add(ResolveGroupExtraInfo(group));
            }

            return groupExtras;
        }
    }

    /// <summary>
    /// <see cref="GetActionGroups(MethodInfo)"/> 缓存集合
    /// </summary>
    private static readonly ConcurrentDictionary<MethodInfo, IEnumerable<GroupExtraInfo>> GetActionGroupsCached;

    /// <summary>
    /// 获取动作方法分组列表
    /// </summary>
    /// <param name="method">方法</param>
    /// <returns></returns>
    public static IEnumerable<GroupExtraInfo> GetActionGroups(MethodInfo method)
    {
        return GetActionGroupsCached.GetOrAdd(method, Function);

        // 本地函数
        static IEnumerable<GroupExtraInfo> Function(MethodInfo method)
        {
            // 如果动作方法没有定义 [ApiDescriptionSettings] 特性，则返回所在控制器分组
            if (!method.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return GetControllerGroups(method.ReflectedType);

            // 读取分组
            var apiDescriptionSettings = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
            if (apiDescriptionSettings.Groups == null || apiDescriptionSettings.Groups.Length == 0) return GetControllerGroups(method.ReflectedType);

            // 处理排序
            var groupExtras = new List<GroupExtraInfo>();
            foreach (var group in apiDescriptionSettings.Groups)
            {
                groupExtras.Add(ResolveGroupExtraInfo(group));
            }

            return groupExtras;
        }
    }

    /// <summary>
    /// <see cref="GetControllerTag(ControllerActionDescriptor)"/> 缓存集合
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> GetControllerTagCached;

    /// <summary>
    /// 获取控制器标签
    /// </summary>
    /// <param name="controllerActionDescriptor">控制器接口描述器</param>
    /// <returns></returns>
    public static string GetControllerTag(ControllerActionDescriptor controllerActionDescriptor)
    {
        var cacheKey = $"{controllerActionDescriptor.ControllerTypeInfo.FullName}::{controllerActionDescriptor.ControllerName}";

        return GetControllerTagCached.GetOrAdd(cacheKey, _ =>
        {
            var type = controllerActionDescriptor.ControllerTypeInfo;
            // 如果动作方法没有定义 [ApiDescriptionSettings] 特性，则返回所在控制器名
            if (!type.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return controllerActionDescriptor.ControllerName;

            // 读取标签
            var apiDescriptionSettings = type.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
            return string.IsNullOrWhiteSpace(apiDescriptionSettings.Tag) ? controllerActionDescriptor.ControllerName : apiDescriptionSettings.Tag;
        });
    }

    /// <summary>
    /// <see cref="GetActionTag(ApiDescription)"/> 缓存集合
    /// </summary>
    private static readonly ConcurrentDictionary<string, IList<string>> GetActionTagCached;

    /// <summary>
    /// 获取动作方法标签
    /// </summary>
    /// <param name="apiDescription">接口描述器</param>
    /// <returns></returns>
    public static IList<string> GetActionTag(ApiDescription apiDescription)
    {
        // 判断是否是 Minimal API
        var isMinimalApi = apiDescription.ActionDescriptor is not ControllerActionDescriptor;
        if (isMinimalApi)
        {
            var tagsAttribute = apiDescription.ActionDescriptor?.EndpointMetadata?.OfType<TagsAttribute>().LastOrDefault();
            return tagsAttribute?.Tags.ToArray() ?? [Assembly.GetEntryAssembly().GetName().Name];
        }
        else
        {
            var cacheKey = apiDescription.TryGetMethodInfo(out var method) && apiDescription.ActionDescriptor is ControllerActionDescriptor descriptor
            ? $"{descriptor.ControllerTypeInfo.FullName}::{descriptor.ActionName}::{apiDescription.HttpMethod}"
            : Assembly.GetEntryAssembly().GetName().Name;

            return GetActionTagCached.GetOrAdd(cacheKey, _ =>
            {
                if (!apiDescription.TryGetMethodInfo(out var method)
                    || apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor) return [Assembly.GetEntryAssembly().GetName().Name];

                // 如果动作方法没有定义 [ApiDescriptionSettings] 特性，则返回所在控制器名
                if (!method.IsDefined(typeof(ApiDescriptionSettingsAttribute), true)) return [GetControllerTag(controllerActionDescriptor)];

                // 读取标签
                var apiDescriptionSettings = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
                return [string.IsNullOrWhiteSpace(apiDescriptionSettings.Tag) ? GetControllerTag(controllerActionDescriptor) : apiDescriptionSettings.Tag];
            });
        }
    }

    /// <summary>
    /// 是否是动作方法
    /// </summary>
    /// <param name="method">方法</param>
    /// <param name="ReflectedType">声明类型</param>
    /// <returns></returns>
    public static bool IsApiAction(MethodInfo method, Type ReflectedType)
    {
        // 不是非公开、抽象、静态、泛型方法
        if (!method.IsPublic || method.IsAbstract || method.IsStatic || method.IsGenericMethod) return false;

        // 如果所在类型不是控制器，则该行为也被忽略
        if (method.ReflectedType != ReflectedType || method.DeclaringType == typeof(object)) return false;

        return true;
    }

    /// <summary>
    /// 解析分组附加信息
    /// </summary>
    /// <param name="group">分组名</param>
    /// <returns></returns>
    private static GroupExtraInfo ResolveGroupExtraInfo(string group)
    {
        string realGroup;
        var order = 0;

        if (!_groupOrderRegex.IsMatch(group)) realGroup = group;
        else
        {
            realGroup = _groupOrderRegex.Replace(group, "");
            order = int.Parse(_groupOrderRegex.Match(group).Groups["order"].Value);
        }

        var groupOpenApiInfo = GetGroupOpenApiInfo(realGroup);
        return new GroupExtraInfo
        {
            Group = realGroup,
            Order = groupOpenApiInfo.Order ?? order,
            Visible = groupOpenApiInfo.Visible ?? true
        };
    }
}