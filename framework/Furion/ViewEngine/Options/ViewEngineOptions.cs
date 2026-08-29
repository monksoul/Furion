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

using Furion.Reflection;
using Microsoft.CodeAnalysis;
using System.Data;
using System.Dynamic;
using System.Reflection;

namespace Furion.ViewEngine;

/// <summary>
/// 视图引擎编译选项
/// </summary>
public class ViewEngineOptions
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ViewEngineOptions()
    {
        ReferencedAssemblies = new(new AssemblyEqualityComparer())
        {
            typeof(object).Assembly,
            typeof(ViewEngineModel).Assembly,
            typeof(System.Collections.IList).Assembly,
            typeof(IEnumerable<>).Assembly,
            typeof(DataTable).Assembly,
            typeof(ExpandoObject).Assembly,
            Reflect.GetAssembly("Microsoft.CSharp"),
            Reflect.GetAssembly("System.Runtime"),
            Reflect.GetAssembly("System.Linq"),
            Reflect.GetAssembly("System.Linq.Expressions"),
            Reflect.GetAssembly("System.Collections"),
            Reflect.GetAssembly("netstandard"),
        };

        CacheSlidingExpiration = TimeSpan.FromHours(8);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="other"></param>
    internal ViewEngineOptions(ViewEngineOptions other)
    {
        ArgumentNullException.ThrowIfNull(other);

        ReferencedAssemblies = new HashSet<Assembly>(other.ReferencedAssemblies, new AssemblyEqualityComparer());
        MetadataReferences = new(other.MetadataReferences);
        TemplateNamespace = other.TemplateNamespace;
        Inherits = other.Inherits;
        DefaultUsings = new(other.DefaultUsings);
        CodeContextLines = other.CodeContextLines;
        CacheSlidingExpiration = other.CacheSlidingExpiration;
        CacheDirectory = other.CacheDirectory;
    }

    /// <summary>
    /// 引用程序集
    /// </summary>
    public HashSet<Assembly> ReferencedAssemblies { get; }

    /// <summary>
    /// 元数据引用
    /// </summary>
    public HashSet<MetadataReference> MetadataReferences { get; } = [];

    /// <summary>
    /// 模板命名空间
    /// </summary>
    public string TemplateNamespace { get; set; } = "Furion.ViewEngine";

    /// <summary>
    /// 继承
    /// </summary>
    public string Inherits { get; set; } = "Furion.ViewEngine.ViewEngineModel";

    /// <summary>
    /// 默认 Using
    /// </summary>
    public HashSet<string> DefaultUsings { get; } =
    [
        "System",
        "System.Linq",
        "System.Text",
        "System.IO",
        "System.Collections",
        "System.Collections.Generic",
        "System.Threading",
        "System.Threading.Tasks",
        "System.Dynamic"
    ];

    /// <summary>
    /// 编译错误时显示代码上下文的行数
    /// </summary>
    public int CodeContextLines { get; set; } = 3;

    /// <summary>
    /// 缓存滑动过期时间
    /// </summary>
    /// <remarks>默认 8 小时</remarks>
    public TimeSpan CacheSlidingExpiration { get; set; }

    /// <summary>
    /// 模板编译缓存文件存储目录
    /// </summary>
    public string CacheDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "templates");
}

/// <summary>
/// 程序集相等比较器
/// </summary>
internal class AssemblyEqualityComparer : IEqualityComparer<Assembly>
{
    public bool Equals(Assembly x, Assembly y)
    {
        if (x == null || y == null) return x == y;
        return x.FullName == y.FullName;
    }

    public int GetHashCode(Assembly obj)
    {
        return obj?.FullName?.GetHashCode() ?? 0;
    }
}