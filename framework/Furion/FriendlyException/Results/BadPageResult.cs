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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Furion.FriendlyException;

/// <summary>
/// 错误页面
/// </summary>
public class BadPageResult : StatusCodeResult
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public BadPageResult()
        : base(400)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="statusCode">状态码</param>
    public BadPageResult(int statusCode)
        : base(statusCode)
    {
    }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = "ModelState Invalid";

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = "User data verification failed. Please input it correctly.";

    /// <summary>
    /// 图标
    /// </summary>
    /// <remarks>必须是 base64 类型</remarks>
    public string Base64Icon { get; set; } = "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTYiIGhlaWdodD0iMTYiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PHBhdGggZD0iTTE0LjIxIDEzLjVsMS43NjcgMS43NzMtLjcwNC43MDRMMTMuNSAxNC4yMWwtMS43NzMgMS43NzMtLjcwNC0uNzEgMS43NzQtMS43NzQtMS43NzQtMS43NzMuNzA0LS43MDQgMS43NzMgMS43NzQgMS43NzMtMS43NzQuNzA0LjcxMUwxNC4yMSAxMy41ek0yIDE1aDh2MUgxVjBoOC43MUwxNCA0LjI5VjEwaC0xVjVIOVYxSDJ2MTR6bTgtMTFoMi4yOUwxMCAxLjcxVjR6IiBmaWxsPSIjMTAxMDEwIi8+PC9zdmc+";

    /// <summary>
    /// 错误代码
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// 错误代码语言
    /// </summary>
    public string CodeLang { get; set; } = "json";

    /// <summary>
    /// 返回通用 401 错误页
    /// </summary>
    public static BadPageResult Status401Unauthorized => new(StatusCodes.Status401Unauthorized)
    {
        Title = "401 Unauthorized",
        Code = "401 Unauthorized",
        Description = "",
        CodeLang = "txt"
    };

    /// <summary>
    /// 返回通用 403 错误页
    /// </summary>
    public static BadPageResult Status403Forbidden => new(StatusCodes.Status403Forbidden)
    {
        Title = "403 Forbidden",
        Code = "403 Forbidden",
        Description = "",
        CodeLang = "txt"
    };

    /// <summary>
    /// 返回通用 404 错误页
    /// </summary>
    public static BadPageResult Status404NotFound => new(StatusCodes.Status404NotFound)
    {
        Title = "404 Not Found",
        Code = "404 Not Found",
        Description = "",
        CodeLang = "txt"
    };

    /// <summary>
    /// 返回通用 500 错误页
    /// </summary>
    public static BadPageResult Status500InternalServerError => new(StatusCodes.Status500InternalServerError)
    {
        Title = "500 Internal Server Error",
        Code = "500 Internal Server Error",
        Description = "",
        CodeLang = "txt"
    };

    /// <summary>
    /// 嵌入式错误页模板缓存
    /// </summary>
    private static readonly Lazy<string> _errorTemplate = new(LoadErrorTemplate);

    /// <summary>
    /// 加载嵌入式错误页模板
    /// </summary>
    /// <returns></returns>
    private static string LoadErrorTemplate()
    {
        var thisType = typeof(BadPageResult);
        var thisAssembly = thisType.Assembly;

        // 读取嵌入式页面路径
        var errorhtml = $"{Reflect.GetAssemblyName(thisAssembly)}{thisType.Namespace.Replace(nameof(Furion), string.Empty)}.Assets.error.html";

        // 解析嵌入式文件流
        using var readStream = thisAssembly.GetManifestResourceStream(errorhtml);
        var buffer = new byte[readStream.Length];
        _ = readStream.Read(buffer, 0, buffer.Length);

        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// 重写返回结果
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task ExecuteResultAsync(ActionContext context)
    {
        var httpContext = context.HttpContext;

        // 如果 Response 已经完成输出或 WebSocket 请求，则禁止写入
        if (httpContext.IsWebSocketRequest() || httpContext.Response.HasStarted) return;

        await base.ExecuteResultAsync(context);
        await httpContext.Response.Body.WriteAsync(ToByteArray());
    }

    /// <summary>
    /// 将 <see cref="BadPageResult"/> 转换成字符串
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override string ToString()
    {
        // 从缓存读取模板内容并替换
        var content = _errorTemplate.Value;
        content = content.Replace($"@{{{nameof(Title)}}}", Title)
                         .Replace($"@{{{nameof(Description)}}}", Description)
                         .Replace($"@{{{nameof(StatusCode)}}}", StatusCode.ToString())
                         .Replace($"@{{{nameof(Code)}}}", Code)
                         .Replace($"@{{{nameof(CodeLang)}}}", CodeLang)
                         .Replace($"@{{{nameof(Base64Icon)}}}", Base64Icon);

        return content;
    }

    /// <summary>
    /// 将 <see cref="BadPageResult"/> 转换成字节数组
    /// </summary>
    /// <returns><see cref="byte"/></returns>
    public byte[] ToByteArray()
    {
        return Encoding.UTF8.GetBytes(ToString());
    }
}