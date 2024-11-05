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

using Furion;
using Furion.DataValidation;
using Furion.Extensions;
using Furion.FriendlyException;
using Furion.Logging;
using Furion.Templates;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Diagnostics;
using System.Logging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace System;

/// <summary>
/// 强大的日志监听器
/// </summary>
/// <remarks>主要用于将请求的信息打印出来</remarks>
[SuppressSniffer, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class LoggingMonitorAttribute : Attribute, IAsyncActionFilter, IAsyncPageFilter, IOrderedFilter
{
    /// <summary>
    /// 过滤器排序
    /// </summary>
    private const int FilterOrder = -2000;

    /// <summary>
    /// 排序属性
    /// </summary>
    public int Order => FilterOrder;

    /// <summary>
    /// 构造函数
    /// </summary>
    public LoggingMonitorAttribute()
        : this(new LoggingMonitorSettings())
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="settings"></param>
    internal LoggingMonitorAttribute(LoggingMonitorSettings settings)
    {
        Settings = settings;
    }

    /// <summary>
    /// 日志标题
    /// </summary>
    public string Title { get; set; } = "Logging Monitor";

    /// <summary>
    /// 是否记录返回值
    /// </summary>
    /// <remarks>bool 类型，默认输出</remarks>
    public object WithReturnValue { get; set; } = null;

    /// <summary>
    /// 设置返回值阈值
    /// </summary>
    /// <remarks>配置返回值字符串阈值，超过这个阈值将截断，默认全量输出</remarks>
    public object ReturnValueThreshold { get; set; } = null;

    /// <summary>
    /// 配置 Json 输出行为
    /// </summary>
    public object JsonBehavior { get; set; } = null;

    /// <summary>
    /// 配置序列化忽略的属性名称
    /// </summary>
    public string[] IgnorePropertyNames { get; set; }

    /// <summary>
    /// 配置序列化忽略的属性类型
    /// </summary>
    public Type[] IgnorePropertyTypes { get; set; }

    /// <summary>
    /// JSON 输出格式化
    /// </summary>
    /// <remarks>bool 类型，默认输出</remarks>
    public object JsonIndented { get; set; } = null;

    /// <summary>
    /// 是否处理 Long 转 String
    /// </summary>
    /// <remarks>bool 类型，默认 false</remarks>
    public object LongTypeConverter { get; set; } = null;

    /// <summary>
    /// 序列化属性命名规则（返回值）
    /// </summary>
    public object ContractResolver { get; set; } = null;

    /// <summary>
    /// 配置信息
    /// </summary>
    private LoggingMonitorSettings Settings { get; set; }

    /// <summary>
    /// 监视 Action 执行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 获取动作方法描述器
        var actionMethod = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;

        // 处理 Blazor Server
        if (actionMethod == null)
        {
            _ = await next.Invoke();
            return;
        }

        await MonitorAsync(actionMethod, context.ActionArguments, context, next);
    }

    /// <summary>
    /// 模型绑定拦截
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 拦截请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // 获取动作方法描述器
        var actionMethod = context.HandlerMethod?.MethodInfo;

        // 处理 Blazor Server
        if (actionMethod == null)
        {
            _ = await next.Invoke();
            return;
        }

        await MonitorAsync(actionMethod, context.HandlerArguments, context, next);
    }

    /// <summary>
    /// 生成 JWT 授权信息日志模板
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="claimsPrincipal"></param>
    /// <param name="authorization"></param>
    /// <returns></returns>
    private List<string> GenerateAuthorizationTemplate(Utf8JsonWriter writer, ClaimsPrincipal claimsPrincipal, StringValues authorization)
    {
        var templates = new List<string>();

        if (!claimsPrincipal.Claims.Any()) return templates;

        templates.AddRange(new[]
        {
            $"━━━━━━━━━━━━━━━  授权信息 ━━━━━━━━━━━━━━━"
            , $"##JWT Token## {authorization}"
            , $""
        });

        // 遍历身份信息
        writer.WritePropertyName("authorizationClaims");
        writer.WriteStartArray();
        foreach (var claim in claimsPrincipal.Claims)
        {
            var valueType = claim.ValueType.Replace("http://www.w3.org/2001/XMLSchema#", "");
            var value = claim.Value;

            // 解析时间戳并转换
            if (!string.IsNullOrEmpty(value) && (claim.Type == "iat" || claim.Type == "nbf" || claim.Type == "exp"))
            {
                var succeed = long.TryParse(value, out var seconds);
                if (succeed)
                {
                    value = $"{value} ({DateTimeOffset.FromUnixTimeSeconds(seconds).ToLocalTime():yyyy-MM-dd HH:mm:ss:ffff(zzz) dddd} L)";
                }
            }

            writer.WriteStartObject();
            templates.Add($"##{claim.Type} ({valueType})## {value}");
            writer.WriteString("type", claim.Type);
            writer.WriteString("valueType", valueType);
            writer.WriteString("value", value);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        return templates;
    }

    /// <summary>
    /// 生成请求头日志模板
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    private List<string> GenerateRequestHeadersTemplate(Utf8JsonWriter writer, IHeaderDictionary headers)
    {
        var templates = new List<string>();

        if (!headers.Any()) return templates;

        templates.AddRange(new[]
        {
            $"━━━━━━━━━━━━━━━  请求头信息 ━━━━━━━━━━━━━━━"
            , $""
        });

        // 遍历请求头列表
        writer.WritePropertyName("requestHeaders");
        writer.WriteStartArray();
        foreach (var (key, value) in headers)
        {
            writer.WriteStartObject();
            templates.Add($"##{key}## {value}");
            writer.WriteString("key", key);
            writer.WriteString("value", value);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        return templates;
    }

    /// <summary>
    /// 生成请求参数信息日志模板
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="parameterValues"></param>
    /// <param name="method"></param>
    /// <param name="contentType"></param>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private List<string> GenerateParameterTemplate(Utf8JsonWriter writer, IDictionary<string, object> parameterValues, MethodInfo method, StringValues contentType, LoggingMonitorMethod monitorMethod)
    {
        var templates = new List<string>();
        writer.WritePropertyName("parameters");

        if (parameterValues.Count == 0)
        {
            writer.WriteStartArray();
            writer.WriteEndArray();
            return templates;
        }

        templates.AddRange(new[]
        {
            $"━━━━━━━━━━━━━━━  参数列表 ━━━━━━━━━━━━━━━"
            , $"##Content-Type## {contentType}"
            , $""
        });

        var parameters = method.GetParameters();

        writer.WriteStartArray();
        foreach (var parameter in parameters)
        {
            // 判断是否禁用记录特定参数
            if (parameter.IsDefined(typeof(SuppressMonitorAttribute), false)) continue;

            // 排除标记 [FromServices] 的解析
            if (parameter.IsDefined(typeof(FromServicesAttribute), false)) continue;

            var name = parameter.Name;
            var parameterType = parameter.ParameterType;

            _ = parameterValues.TryGetValue(name, out var value);
            writer.WriteStartObject();
            writer.WriteString("name", name);
            writer.WriteString("type", HandleGenericType(parameterType));

            object rawValue = default;

            // 文件类型参数
            if (value is IFormFile || value is List<IFormFile>)
            {
                writer.WritePropertyName("value");

                // 单文件
                if (value is IFormFile formFile)
                {
                    var fileSize = Math.Round(formFile.Length / 1024D);
                    templates.Add($"##{name} ({parameterType.Name})## [name]: {formFile.FileName}; [size]: {fileSize}KB; [content-type]: {formFile.ContentType}");

                    writer.WriteStartObject();
                    writer.WriteString(name, formFile.Name);
                    writer.WriteString("fileName", formFile.FileName);
                    writer.WriteNumber("length", formFile.Length);
                    writer.WriteString("contentType", formFile.ContentType);
                    writer.WriteEndObject();

                    goto writeEndObject;
                }
                // 多文件
                else if (value is List<IFormFile> formFiles)
                {
                    writer.WriteStartArray();
                    for (var i = 0; i < formFiles.Count; i++)
                    {
                        var file = formFiles[i];
                        var size = Math.Round(file.Length / 1024D);
                        templates.Add($"##{name}[{i}] ({nameof(IFormFile)})## [name]: {file.FileName}; [size]: {size}KB; [content-type]: {file.ContentType}");

                        writer.WriteStartObject();
                        writer.WriteString(name, file.Name);
                        writer.WriteString("fileName", file.FileName);
                        writer.WriteNumber("length", file.Length);
                        writer.WriteString("contentType", file.ContentType);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();

                    goto writeEndObject;
                }
            }
            // 处理 byte[] 参数类型
            else if (value is byte[] byteArray)
            {
                writer.WritePropertyName("value");
                templates.Add($"##{name} ({parameterType.Name})## [Length]: {byteArray.Length}");

                writer.WriteStartObject();
                writer.WriteNumber("length", byteArray.Length);
                writer.WriteEndObject();

                goto writeEndObject;
            }
            // 处理基元类型，字符串类型和空值
            else if (parameterType.IsPrimitive || value is string || value == null)
            {
                writer.WritePropertyName("value");
                rawValue = value;

                if (value == null) writer.WriteNullValue();
                else if (value is string str) writer.WriteStringValue(str);
                else if (double.TryParse(value.ToString(), out var r)) writer.WriteNumberValue(r);
                else writer.WriteStringValue(value.ToString());
            }
            // 其他类型统一进行序列化
            else
            {
                writer.WritePropertyName("value");
                rawValue = TrySerializeObject(value, monitorMethod, out var succeed);

                if (succeed) writer.WriteRawValue(rawValue?.ToString());
                else writer.WriteNullValue();
            }

            templates.Add($"##{name} ({parameterType.Name})## {rawValue}");

        writeEndObject: writer.WriteEndObject();
        }
        writer.WriteEndArray();

        return templates;
    }

    /// <summary>
    /// 生成返回值信息日志模板
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="resultContext"></param>
    /// <param name="method"></param>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private List<string> GenerateReturnInfomationTemplate(Utf8JsonWriter writer, dynamic resultContext, MethodInfo method, LoggingMonitorMethod monitorMethod)
    {
        var templates = new List<string>();

        object returnValue = null;
        Type finalReturnType;
        var result = resultContext.Result as IActionResult;

        // 解析返回值
        if (UnifyContext.CheckVaildResult(result, out var data))
        {
            returnValue = data;
            finalReturnType = data?.GetType();
        }
        // 处理文件类型
        else if (result is FileResult fresult)
        {
            returnValue = new {
                FileName = fresult.FileDownloadName,
                fresult.ContentType,
                Length = fresult is FileContentResult cresult ? (object)cresult.FileContents.Length : null
            };
            finalReturnType = fresult?.GetType();
        }
        else finalReturnType = result?.GetType();

        // 获取最终呈现值（字符串类型）
        var displayValue = TrySerializeObject(returnValue, monitorMethod, out var succeed);
        var originValue = displayValue;

        // 获取返回值阈值
        var threshold = GetReturnValueThreshold(monitorMethod);
        if (threshold > 0)
        {
            displayValue = displayValue.Length <= threshold ? displayValue : displayValue[..threshold];
        }

        var returnTypeName = HandleGenericType(method.ReturnType);
        var finalReturnTypeName = HandleGenericType(finalReturnType);

        // 获取请求返回的响应状态码
        var httpStatusCode = (resultContext as FilterContext).HttpContext.Response.StatusCode;

        templates.AddRange(new[]
        {
            $"━━━━━━━━━━━━━━━  返回信息 ━━━━━━━━━━━━━━━"
            , $"##HTTP响应状态码## {httpStatusCode}"
            , $"##原始类型## {returnTypeName}"
            , $"##最终类型## {finalReturnTypeName}"
            , $"##最终返回值## {displayValue}"
        });

        writer.WritePropertyName("returnInformation");
        writer.WriteStartObject();
        writer.WriteString("type", finalReturnTypeName);
        writer.WriteNumber(nameof(httpStatusCode), httpStatusCode);
        writer.WriteString("actType", returnTypeName);
        writer.WritePropertyName("value");
        if (succeed && method.ReturnType != typeof(void) && returnValue != null)
        {
            // 解决返回值被截断后 json 验证失败异常问题
            if (threshold > 0 && originValue != displayValue)
            {
                writer.WriteStringValue(displayValue);
            }
            else writer.WriteRawValue(displayValue);
        }
        else writer.WriteNullValue();

        writer.WriteEndObject();

        return templates;
    }

    /// <summary>
    /// 生成异常信息日志模板
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="exception"></param>
    /// <param name="isValidationException">是否是验证异常</param>
    /// <returns></returns>
    private List<string> GenerateExcetpionInfomationTemplate(Utf8JsonWriter writer, Exception exception, bool isValidationException)
    {
        var templates = new List<string>();

        if (exception == null)
        {
            writer.WritePropertyName("exception");
            writer.WriteNullValue();

            writer.WritePropertyName("validation");
            writer.WriteNullValue();
            return templates;
        }

        // 处理不是验证异常情况
        if (!isValidationException)
        {
            var exceptionTypeName = HandleGenericType(exception.GetType());
            templates.AddRange(new[]
            {
                $"━━━━━━━━━━━━━━━  异常信息 ━━━━━━━━━━━━━━━"
                , $"##类型## {exceptionTypeName}"
                , $"##消息## {exception.Message}"
                , $"##错误堆栈## {exception.StackTrace}"
            });

            writer.WritePropertyName("exception");
            writer.WriteStartObject();
            writer.WriteString("type", exceptionTypeName);
            writer.WriteString("message", exception.Message);
            writer.WriteString("stackTrace", exception.StackTrace?.ToString());

            // 添加更多信息
            if (exception is AppFriendlyException friendlyException)
            {
                writer.WriteString("errorCode", friendlyException.ErrorCode?.ToString());
                writer.WriteString("originErrorCode", friendlyException.OriginErrorCode?.ToString());
                writer.WriteString("statusCode", friendlyException.StatusCode.ToString());
            }

            writer.WriteEndObject();

            writer.WritePropertyName("validation");
            writer.WriteNullValue();
        }
        else
        {
            var friendlyException = exception as AppFriendlyException;
            templates.AddRange(new[]
            {
                $"━━━━━━━━━━━━━━━  业务异常 ━━━━━━━━━━━━━━━"
                , $"##业务码## {friendlyException?.ErrorCode}"
                , $"##业务码（原）## {friendlyException?.OriginErrorCode}"
                , $"##业务消息## {friendlyException?.ErrorMessage}"
            });

            writer.WritePropertyName("exception");
            writer.WriteNullValue();

            writer.WritePropertyName("validation");
            writer.WriteStartObject();
            writer.WriteString("errorCode", friendlyException?.ErrorCode?.ToString());
            writer.WriteString("originErrorCode", friendlyException?.OriginErrorCode?.ToString());
            writer.WriteString("message", friendlyException?.Message);
            writer.WriteString("statusCode", friendlyException?.StatusCode.ToString());
            writer.WriteEndObject();
        }

        return templates;
    }

    /// <summary>
    /// 生成附加信息日志模板
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    private List<string> GenerateExtraTemplate(Utf8JsonWriter writer, HttpContext httpContext)
    {
        if (!httpContext.Items.TryGetValue(LoggingMonitorContext.KEY, out var values))
        {
            return null;
        }

        if (values is not Dictionary<string, object> extras || extras.Count == 0)
        {
            return null;
        }

        var templates = new List<string>();

        templates.AddRange(new[]
        {
            $"━━━━━━━━━━━━━━━  附加信息 ━━━━━━━━━━━━━━━"
        });

        // 遍历附加信息
        writer.WritePropertyName("loggingExtras");
        writer.WriteStartObject();
        foreach (var (key, value) in extras)
        {
            templates.Add($"##{key}## {value}");
            writer.WriteString(key, value?.ToString());
        }
        writer.WriteEndObject();

        // 移除内存占用
        httpContext.Items.Remove(LoggingMonitorContext.KEY);

        return templates;
    }

    /// <summary>
    /// 序列化对象
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="monitorMethod"></param>
    /// <param name="succeed"></param>
    /// <returns></returns>
    private string TrySerializeObject(object obj, LoggingMonitorMethod monitorMethod, out bool succeed)
    {
        // 排除 IQueryable<> 泛型
        if (obj != null && obj.GetType().HasImplementedRawGeneric(typeof(IQueryable<>)))
        {
            succeed = true;
            return "{}";
        }

        try
        {
            var contractResolver = GetContractResolver(ContractResolver, monitorMethod);

            // 序列化默认配置
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                // 解决属性忽略问题
                ContractResolver = contractResolver == ContractResolverTypes.CamelCase
                ? new CamelCasePropertyNamesContractResolverWithIgnoreProperties(GetIgnorePropertyNames(monitorMethod), GetIgnorePropertyTypes(monitorMethod))
                : new DefaultContractResolverWithIgnoreProperties(GetIgnorePropertyNames(monitorMethod), GetIgnorePropertyTypes(monitorMethod)),

                // 解决循环引用问题
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                // 解决 DateTimeOffset 序列化/反序列化问题
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
            };

            if (CheckIsSetLongTypeConverter(monitorMethod))
            {
                // 解决 long 精度问题
                jsonSerializerSettings.Converters.AddLongTypeConverters();
            }

            // 解决 JsonElement 序列化问题
            jsonSerializerSettings.Converters.Add(new JsonElementConverter());

            // 解决粘土对象 序列化问题
            jsonSerializerSettings.Converters.AddClayConverters();

            // 解决 DateTimeOffset 序列化/反序列化问题
            if (obj is DateTimeOffset)
            {
                jsonSerializerSettings.Converters.Add(new IsoDateTimeConverter { DateTimeStyles = Globalization.DateTimeStyles.AssumeUniversal });
            }

            var result = Newtonsoft.Json.JsonConvert.SerializeObject(obj, jsonSerializerSettings);

            succeed = true;
            return result;
        }
        catch
        {
            succeed = true;
            return "{}";
        }
    }

    /// <summary>
    /// 检查是否开启启用返回值
    /// </summary>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private bool CheckIsSetWithReturnValue(LoggingMonitorMethod monitorMethod)
    {
        return WithReturnValue == null
            ? (monitorMethod?.WithReturnValue ?? Settings.WithReturnValue)
            : Convert.ToBoolean(WithReturnValue);
    }

    /// <summary>
    /// 检查是否开启 JSON 格式化
    /// </summary>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private bool CheckIsSetJsonIndented(LoggingMonitorMethod monitorMethod)
    {
        return JsonIndented == null
            ? (monitorMethod?.JsonIndented ?? Settings.JsonIndented)
            : Convert.ToBoolean(JsonIndented);
    }

    /// <summary>
    /// 检查是否开启 long 转 string
    /// </summary>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private bool CheckIsSetLongTypeConverter(LoggingMonitorMethod monitorMethod)
    {
        return LongTypeConverter == null
            ? (monitorMethod?.LongTypeConverter ?? Settings.LongTypeConverter)
            : Convert.ToBoolean(LongTypeConverter);
    }

    /// <summary>
    /// 获取返回值阈值
    /// </summary>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private int GetReturnValueThreshold(LoggingMonitorMethod monitorMethod)
    {
        return ReturnValueThreshold == null
            ? (monitorMethod?.ReturnValueThreshold ?? Settings.ReturnValueThreshold)
            : Convert.ToInt32(ReturnValueThreshold);
    }

    /// <summary>
    /// 获取 Json 输出行为
    /// </summary>
    /// <param name="jsonBehavior"></param>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private JsonBehavior GetJsonBehavior(object jsonBehavior, LoggingMonitorMethod monitorMethod)
    {
        return jsonBehavior == null
            ? (monitorMethod?.JsonBehavior ?? Settings.JsonBehavior)
            : (JsonBehavior)jsonBehavior;
    }

    /// <summary>
    /// 获取 序列化属性命名规则
    /// </summary>
    /// <param name="contractResolver"></param>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private ContractResolverTypes GetContractResolver(object contractResolver, LoggingMonitorMethod monitorMethod)
    {
        return contractResolver == null
            ? (monitorMethod?.ContractResolver ?? Settings.ContractResolver)
            : (ContractResolverTypes)contractResolver;
    }

    /// <summary>
    /// 获取忽略序列化属性名称集合
    /// </summary>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private string[] GetIgnorePropertyNames(LoggingMonitorMethod monitorMethod)
    {
        IEnumerable<string> ignorePropertyNamesList = IgnorePropertyNames ?? Array.Empty<string>();

        return ignorePropertyNamesList.Concat(monitorMethod?.IgnorePropertyNames ?? Array.Empty<string>())
                                      .Concat(Settings.IgnorePropertyNames ?? Array.Empty<string>())
                                      .ToArray();
    }

    /// <summary>
    /// 获取忽略序列化属性类型集合
    /// </summary>
    /// <param name="monitorMethod"></param>
    /// <returns></returns>
    private Type[] GetIgnorePropertyTypes(LoggingMonitorMethod monitorMethod)
    {
        IEnumerable<Type> ignorePropertyTypesList = IgnorePropertyTypes ?? Array.Empty<Type>();

        return ignorePropertyTypesList.Concat(monitorMethod?.IgnorePropertyTypes ?? Array.Empty<Type>())
                                      .Concat(Settings.IgnorePropertyTypes ?? Array.Empty<Type>())
                                      .ToArray();
    }

    /// <summary>
    /// 处理泛型类型转字符串打印问题
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static string HandleGenericType(Type type)
    {
        if (type == null) return string.Empty;

        var typeName = type.FullName ?? (!string.IsNullOrEmpty(type.Namespace) ? type.Namespace + "." : string.Empty) + type.Name;

        // 处理泛型类型问题
        if (type.IsConstructedGenericType)
        {
            var prefix = type.GetGenericArguments()
                .Select(genericArg => HandleGenericType(genericArg))
                .Aggregate((previous, current) => previous + ", " + current);

            typeName = typeName.Split('`').First() + "<" + prefix + ">";
        }

        return typeName;
    }

    private async Task MonitorAsync(MethodInfo actionMethod, IDictionary<string, object> parameterValues, FilterContext context, dynamic next)
    {
        // 排除 WebSocket 请求处理
        if (context.HttpContext.IsWebSocketRequest())
        {
            _ = await next();
            return;
        }

        // 判断是否是 Razor Pages
        var isPageDescriptor = context.ActionDescriptor is CompiledPageActionDescriptor;

        // 如果贴了 [SuppressMonitor] 特性则跳过
        if (actionMethod.IsDefined(typeof(SuppressMonitorAttribute), true)
            || actionMethod.DeclaringType.IsDefined(typeof(SuppressMonitorAttribute), true))
        {
            _ = await next();
            return;
        }

        // 判断是否自定义了日志筛选器，如果是则检查是否符合条件
        if (LoggingMonitorSettings.InternalWriteFilter?.Invoke(context) == false)
        {
            _ = await next();
            return;
        }

        // 获取方法完整名称
        var methodFullName = actionMethod.DeclaringType.FullName + "." + actionMethod.Name;

        // 只有方法没有贴有 [LoggingMonitor] 特性才判断全局，贴了特性优先级最大
        var isDefinedScopedAttribute = actionMethod.IsDefined(typeof(LoggingMonitorAttribute), true);

        // 解决局部和全局触发器同时配置触发两次问题
        if (isDefinedScopedAttribute && Settings.FromGlobalFilter == true)
        {
            _ = await next();
            return;
        }

        if (!isDefinedScopedAttribute)
        {
            // 解决通过 AddMvcFilter 的问题
            if (!Settings.IsMvcFilterRegister)
            {
                // 处理不启用但排除的情况
                if (!Settings.GlobalEnabled
                    && !Settings.IncludeOfMethods.Contains(methodFullName, StringComparer.OrdinalIgnoreCase))
                {
                    // 查找是否包含匹配，忽略大小写
                    _ = await next();
                    return;
                }

                // 处理启用但排除的情况
                if (Settings.GlobalEnabled
                    && Settings.ExcludeOfMethods.Contains(methodFullName, StringComparer.OrdinalIgnoreCase))
                {
                    _ = await next();
                    return;
                }
            }
        }

        // 获取全局 LoggingMonitorMethod 配置
        var monitorMethod = Settings.MethodsSettings.FirstOrDefault(m => m.FullName.Equals(methodFullName, StringComparison.OrdinalIgnoreCase));

        // 创建 json 写入器
        using var stream = new MemoryStream();
        var jsonWriterOptions = Settings.JsonWriterOptions;

        // 配置 JSON 格式化行为，是否美化
        jsonWriterOptions.Indented = CheckIsSetJsonIndented(monitorMethod);

        // 创建 JSON 写入器
        using var writer = new Utf8JsonWriter(stream, jsonWriterOptions);
        writer.WriteStartObject();
        writer.WriteString("title", Title);

        // 创建日志上下文
        var logContext = new LogContext();

        // 获取路由表信息
        var routeData = context.RouteData;
        var controllerName = routeData.Values["controller"];
        var actionName = routeData.Values["action"];
        var areaName = routeData.DataTokens["area"];
        writer.WriteString(nameof(controllerName), controllerName?.ToString());
        writer.WriteString("controllerTypeName", actionMethod.DeclaringType.Name);
        writer.WriteString(nameof(actionName), actionName?.ToString());
        writer.WriteString("actionTypeName", actionMethod.Name);
        writer.WriteString("areaName", areaName?.ToString());

        // 调用呈现链名称
        var displayName = methodFullName;
        writer.WriteString(nameof(displayName), displayName);

        // [DisplayName] 特性
        var displayNameAttribute = actionMethod.IsDefined(typeof(DisplayNameAttribute), true)
            ? actionMethod.GetCustomAttribute<DisplayNameAttribute>(true)
            : default;
        writer.WriteString("displayTitle", displayNameAttribute?.DisplayName);

        // 获取 HttpContext 和 HttpRequest 对象
        var httpContext = context.HttpContext;
        var httpRequest = httpContext.Request;

        // 获取服务端 IPv4 地址
        var localIPv4 = httpContext.GetLocalIpAddressToIPv4();
        writer.WriteString(nameof(localIPv4), localIPv4);

        // 获取服务端源端口
        var localPort = httpContext.Connection.LocalPort;
        writer.WriteNumber(nameof(localPort), localPort);

        // 获取客户端 IPv4 地址
        var remoteIPv4 = httpContext.GetRemoteIpAddressToIPv4();
        writer.WriteString(nameof(remoteIPv4), remoteIPv4);

        // 获取客户端远程端口
        var remotePort = httpContext.Connection.RemotePort;
        writer.WriteNumber(nameof(remotePort), remotePort);

        // 获取请求方式
        var httpMethod = httpContext.Request.Method;
        writer.WriteString(nameof(httpMethod), httpMethod);

        // 客户端连接 ID
        var traceId = App.GetTraceId();
        writer.WriteString(nameof(traceId), traceId);

        // 线程 Id
        var threadId = App.GetThreadId();
        writer.WriteNumber(nameof(threadId), threadId);

        // 获取请求的 Url 地址
        var requestUrl = Uri.UnescapeDataString(httpRequest.GetRequestUrlAddress());
        writer.WriteString(nameof(requestUrl), requestUrl);

        // 获取请求 HTTP 协议
        var protocol = Uri.UnescapeDataString(httpRequest.Protocol);
        writer.WriteString(nameof(protocol), protocol);

        // 获取来源 Url 地址
        var refererUrl = Uri.UnescapeDataString(httpRequest.GetRefererUrlAddress());
        writer.WriteString(nameof(refererUrl), refererUrl);

        // 客户端浏览器信息
        var userAgent = httpRequest.Headers["User-Agent"];
        writer.WriteString(nameof(userAgent), userAgent);

        // 客户端请求区域语言
        var acceptLanguage = httpRequest.Headers["accept-language"];
        writer.WriteString(nameof(acceptLanguage), acceptLanguage);

        // 请求来源（swagger还是其他）
        var requestFrom = httpRequest.Headers["request-from"].ToString();
        requestFrom = string.IsNullOrWhiteSpace(requestFrom) ? "client" : requestFrom;
        writer.WriteString(nameof(requestFrom), requestFrom);

        // 获取授权用户
        var user = httpContext.User;

        // 获取请求 cookies 信息
        var requestHeaderCookies = Uri.UnescapeDataString(httpRequest.Headers["cookie"].ToString());
        writer.WriteString(nameof(requestHeaderCookies), requestHeaderCookies);

        // 计算接口执行时间
        var timeOperation = Stopwatch.StartNew();
        var resultContext = await next();
        timeOperation.Stop();
        writer.WriteNumber("timeOperationElapsedMilliseconds", timeOperation.ElapsedMilliseconds);

        var resultHttpContext = (resultContext as FilterContext).HttpContext;

        // token 信息
        // 判断是否是授权访问
        var isAuth = actionMethod.GetFoundAttribute<AllowAnonymousAttribute>(true) == null
            && resultHttpContext.User != null
            && resultHttpContext.User.Identity.IsAuthenticated;
        // 获取响应头信息
        var accessToken = resultHttpContext.Response.Headers["access-token"].ToString();
        var authorization = string.IsNullOrWhiteSpace(accessToken)
            ? httpRequest.Headers["Authorization"].ToString()
            : "Bearer " + accessToken;
        writer.WriteString("accessToken", isAuth ? authorization : default);

        // 获取响应 cookies 信息
        var responseHeaderCookies = Uri.UnescapeDataString(resultHttpContext.Response.Headers["Set-Cookie"].ToString());
        writer.WriteString(nameof(responseHeaderCookies), responseHeaderCookies);

        // 获取系统信息
        var osDescription = RuntimeInformation.OSDescription;
        var osArchitecture = RuntimeInformation.OSArchitecture.ToString();
        var frameworkDescription = RuntimeInformation.FrameworkDescription;
        var basicFrameworkDescription = typeof(App).Assembly.GetName();
        var basicFramework = basicFrameworkDescription.Name;
        var basicFrameworkVersion = basicFrameworkDescription.Version?.ToString();
        writer.WriteString(nameof(osDescription), osDescription);
        writer.WriteString(nameof(osArchitecture), osArchitecture);
        writer.WriteString(nameof(frameworkDescription), frameworkDescription);
        writer.WriteString(nameof(basicFramework), basicFramework);
        writer.WriteString(nameof(basicFrameworkVersion), basicFrameworkVersion);

        // 获取启动信息
        var entryAssemblyName = Assembly.GetEntryAssembly().GetName().Name;
        writer.WriteString(nameof(entryAssemblyName), entryAssemblyName);

        // 获取进程信息
        var process = Process.GetCurrentProcess();
        var processName = process.ProcessName;
        writer.WriteString(nameof(processName), processName);

        // 获取部署程序
        var deployServer = processName == entryAssemblyName ? "Kestrel" : processName;
        writer.WriteString(nameof(deployServer), deployServer);

        // 获取主机启动地址
        var iServer = httpContext.RequestServices.GetRequiredService<IServer>();
        var startUrls = string.Join(";", iServer.GetServerAddresses());
        writer.WriteString(nameof(startUrls), startUrls);

        // 服务器环境
        var environment = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName;
        writer.WriteString(nameof(environment), environment);

        // 获取异常对象情况
        Exception exception = resultContext.Exception;
        if (exception == null)
        {
            // 解析存储的验证信息
            var validationFailedKey = nameof(DataValidationFilter) + nameof(ValidationMetadata);
            var validationMetadata = !httpContext.Items.ContainsKey(validationFailedKey)
                ? default
                : httpContext.Items[validationFailedKey] as ValidationMetadata;

            if (validationMetadata != null)
            {
                // 创建全局验证友好异常
                var error = TrySerializeObject(validationMetadata.ValidationResult, monitorMethod, out _);
                exception = new AppFriendlyException(error, validationMetadata.OriginErrorCode)
                {
                    ErrorCode = validationMetadata.ErrorCode,
                    StatusCode = validationMetadata.StatusCode ?? StatusCodes.Status400BadRequest,
                    ValidationException = true
                };
            }
        }

        // 判断是否是验证异常
        var isValidationException = exception is AppFriendlyException friendlyException && friendlyException.ValidationException;

        var monitorItems = new List<string>()
        {
            $"##控制器名称## {actionMethod.DeclaringType.Name}"
            , $"##操作名称## {actionMethod.Name}"
            , $"##显示名称## {displayNameAttribute?.DisplayName}"
            , $"##路由信息## [area]: {areaName}; [controller]: {controllerName}; [action]: {actionName}"
            , $"##请求方式## {httpMethod}"
            , $"##请求地址## {requestUrl}"
            , $"##HTTP 协议## {protocol}"
            , $"##来源地址## {refererUrl}"
            , $"##请求端源## {requestFrom}"
            , $"##浏览器标识## {userAgent}"
            , $"##客户端区域语言## {acceptLanguage}"
            , $"##客户端 IP 地址## {remoteIPv4}"
            , $"##客户端源端口## {remotePort}"
            , $"##服务端 IP 地址## {localIPv4}"
            , $"##服务端源端口## {localPort}"
            , $"##客户端连接 ID## {traceId}"
            , $"##服务线程 ID## #{threadId}"
            , $"##执行耗时## {timeOperation.ElapsedMilliseconds}ms"
            ,"━━━━━━━━━━━━━━━  Cookies ━━━━━━━━━━━━━━━"
            , $"##请求端## {requestHeaderCookies}"
            , $"##响应端## {responseHeaderCookies}"
            ,"━━━━━━━━━━━━━━━  系统信息 ━━━━━━━━━━━━━━━"
            , $"##系统名称## {osDescription}"
            , $"##系统架构## {osArchitecture}"
            , $"##基础框架## {basicFramework} v{basicFrameworkVersion}"
            , $"##.NET 架构## {frameworkDescription}"
            ,"━━━━━━━━━━━━━━━  启动信息 ━━━━━━━━━━━━━━━"
            , $"##Web 启动地址## {startUrls}"
            , $"##运行环境## {environment}"
            , $"##启动程序集## {entryAssemblyName}"
            , $"##进程名称## {processName}"
            , $"##托管程序## {deployServer}"
        };

        // 如果用户实际授权才打印
        if (isAuth)
        {
            // 添加 JWT 授权信息日志模板
            monitorItems.AddRange(GenerateAuthorizationTemplate(writer, user, authorization));
        }

        // 生成请求头日志模板
        monitorItems.AddRange(GenerateRequestHeadersTemplate(writer, httpRequest.Headers));

        // 添加请求参数信息日志模板
        monitorItems.AddRange(GenerateParameterTemplate(writer, parameterValues, actionMethod, httpRequest.Headers["Content-Type"], monitorMethod));

        // 判断是否启用返回值打印
        if (CheckIsSetWithReturnValue(monitorMethod))
        {
            // 添加返回值信息日志模板
            monitorItems.AddRange(GenerateReturnInfomationTemplate(writer, resultContext, actionMethod, monitorMethod));
        }

        // 添加附加信息模板
        monitorItems.AddRange(GenerateExtraTemplate(writer, resultHttpContext) ?? Enumerable.Empty<string>());

        // 添加异常信息日志模板
        monitorItems.AddRange(GenerateExcetpionInfomationTemplate(writer, exception, isValidationException));

        // 生成最终模板
        var monitorMessage = TP.Wrapper(Title, displayName, monitorItems.ToArray());

        // 创建日志记录器
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<LoggingMonitor>>();

        // 调用外部配置
        LoggingMonitorSettings.Configure?.Invoke(logger, logContext, resultContext as FilterContext);

        writer.WriteEndObject();
        writer.Flush();

        // 获取 json 字符串
        var jsonString = Encoding.UTF8.GetString(stream.ToArray());
        logContext.Set("loggingMonitor", jsonString);

        // 设置日志上下文
        using var scope = logger.ScopeContext(logContext);

        // 获取最终写入日志消息格式
        var finalMessage = GetJsonBehavior(JsonBehavior, monitorMethod) == Furion.Logging.JsonBehavior.OnlyJson ? jsonString : monitorMessage;

        // 写入日志，如果没有异常默认使用 LogInformation，否则使用 LogError
        if (exception == null)
        {
            logger.Log(Settings.LogLevel, finalMessage);
        }
        else
        {
            // 如果不是验证异常，写入 Error
            if (!isValidationException) logger.LogError(exception, finalMessage);
            else
            {
                // 读取配置的日志级别并写入
                logger.Log(Settings.BahLogLevel, finalMessage);
            }
        }
    }
}