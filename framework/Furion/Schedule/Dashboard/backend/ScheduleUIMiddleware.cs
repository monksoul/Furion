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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace Furion.Schedule;

/// <summary>
/// Schedule 模块 UI 中间件
/// </summary>
public sealed class ScheduleUIMiddleware
{
    private const string STATIC_FILES_PATH = "/__schedule__";

    /// <summary>
    /// 请求委托
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// 作业计划工厂
    /// </summary>
    private readonly ISchedulerFactory _schedulerFactory;

    /// <summary>
    /// 主机生命周期服务
    /// </summary>
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    /// <summary>
    /// 静态资源缓存 Key
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> _staticContentCache = new();

    /// <summary>
    /// JSON 序列化选项
    /// </summary>
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static ScheduleUIMiddleware()
    {
        // 处理时间类型
        if (!ScheduleOptionsBuilder.UseUtcTimestampProperty)
        {
            _jsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="next">请求委托</param>
    /// <param name="schedulerFactory">作业计划工厂</param>
    /// <param name="options">UI 配置选项</param>
    /// <param name="hostApplicationLifetime">主机生命周期服务</param>
    public ScheduleUIMiddleware(RequestDelegate next
        , ISchedulerFactory schedulerFactory
        , ScheduleUIOptions options
        , IHostApplicationLifetime hostApplicationLifetime)
    {
        _next = next;
        _schedulerFactory = schedulerFactory;
        Options = options;
        ApiRequestPath = $"{options.RequestPath}/api";
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    /// <summary>
    /// UI 配置选项
    /// </summary>
    public ScheduleUIOptions Options { get; }

    /// <summary>
    /// API 入口地址
    /// </summary>
    public string ApiRequestPath { get; }

    /// <summary>
    /// 中间件执行方法
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/></param>
    /// <returns><see cref="Task"/></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // 非看板请求跳过
        if (!context.Request.Path.StartsWithSegments(Options.RequestPath, StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // ================================ 处理静态文件请求 ================================
        var staticFilePath = Options.RequestPath + "/";
        if (context.Request.Path.Equals(staticFilePath, StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.Equals(staticFilePath + "apiconfig.js", StringComparison.OrdinalIgnoreCase))
        {
            var targetPath = context.Request.Path.Value?[staticFilePath.Length..];
            var isIndex = string.IsNullOrEmpty(targetPath);
            var fileName = isIndex ? "index.html" : targetPath;

            // 尝试从缓存获取
            var cacheKey = $"{fileName}_{Options.VirtualPath}_{Options.RequestPath}_{Options.DisplayEmptyTriggerJobs}_{Options.DisplayHead}_{Options.DefaultExpandAllJobs}_{ScheduleOptionsBuilder.UseUtcTimestampProperty}_{Options.Title}_{Options.LoginConfig?.DefaultUsername}_{Options.LoginConfig?.DefaultPassword}";
            if (!_staticContentCache.TryGetValue(cacheKey, out var content))
            {
                // 获取当前类型所在程序集和对应嵌入式文件路径
                var currentAssembly = typeof(ScheduleUIExtensions).Assembly;
                var resourceName = $"{currentAssembly.GetName().Name}.Schedule.Dashboard.frontend.{fileName}";

                // 读取配置文件内容
                byte[] buffer;
                using (var readStream = currentAssembly.GetManifestResourceStream(resourceName))
                {
                    buffer = new byte[readStream.Length];
                    _ = await readStream.ReadAsync(buffer);
                }

                // 替换配置占位符
                using (var stream = new MemoryStream(buffer))
                {
                    using var streamReader = new StreamReader(stream, new UTF8Encoding(false));
                    content = await streamReader.ReadToEndAsync();

                    if (isIndex)
                    {
                        content = content.Replace(STATIC_FILES_PATH, $"{Options.VirtualPath}{Options.RequestPath}");
                    }
                    else
                    {
                        content = content.Replace("%(RequestPath)", $"{Options.VirtualPath}{Options.RequestPath}")
                                         .Replace("%(DisplayEmptyTriggerJobs)", Options.DisplayEmptyTriggerJobs ? "true" : "false")
                                         .Replace("%(DisplayHead)", Options.DisplayHead ? "true" : "false")
                                         .Replace("%(DefaultExpandAllJobs)", Options.DefaultExpandAllJobs ? "true" : "false")
                                         .Replace("%(UseUtcTimestamp)", ScheduleOptionsBuilder.UseUtcTimestampProperty ? "true" : "false")
                                         .Replace("%(Title)", Options.Title ?? string.Empty)
                                         .Replace("%(Login.DefaultUsername)", Options.LoginConfig?.DefaultUsername ?? string.Empty)
                                         .Replace("%(Login.DefaultPassword)", Options.LoginConfig?.DefaultPassword ?? string.Empty);
                    }
                }

                // 存入缓存
                _staticContentCache.TryAdd(cacheKey, content);
            }

            // 输出到客户端
            context.Response.ContentType = $"text/{(isIndex ? "html" : "javascript")}; charset=utf-8";
            await context.Response.WriteAsync(content);
            return;
        }

        // 处理刷新登录页面出现 404 情况
        if (context.Request.Path.Equals(staticFilePath + "login", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect(staticFilePath);
            return;
        }

        // ================================ 处理 API 请求 ================================

        // 如果不是以 API_REQUEST_PATH 开头，则跳过
        if (!context.Request.Path.StartsWithSegments(ApiRequestPath, StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 只处理 GET/POST 请求
        if (!context.Request.Method.Equals("GET", StringComparison.CurrentCultureIgnoreCase) && !context.Request.Method.Equals("POST", StringComparison.CurrentCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 获取匹配的路由标识
        var action = context.Request.Path.Value?[ApiRequestPath.Length..]?.ToLower();

        // 允许跨域，设置返回 json
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers.AccessControlAllowOrigin = "*";
        context.Response.Headers.AccessControlAllowHeaders = "*";

        // 如果未配置登录密钥或者长度不足 16 个字符，则直接返回 401
        if (string.IsNullOrWhiteSpace(Options.LoginConfig?.AppSecret) || Options.LoginConfig.AppSecret.Trim().Length < 16)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("System login key is not properly configured.");
            return;
        }

        // 读取密钥
        var appSecret = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(appSecret))
        {
            appSecret = context.Request.Query["appsecret"].FirstOrDefault();
        }
        // 验证密钥
        if (action != "/login" && !string.Equals(appSecret, Options.LoginConfig.AppSecret, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("401 Unauthorized");
            return;
        }

        // 路由匹配
        switch (action)
        {
            // 获取所有作业
            case "/get-jobs":
                var jobs = _schedulerFactory.GetJobsOfModels().OrderBy(u => u.JobDetail.GroupName).ThenBy(u => u.JobDetail.JobId);

                // 输出 JSON
                await context.Response.WriteAsync(SerializeToJson(jobs));
                break;
            // 添加作业 
            case "/add-job":
                {
                    // 读取内容
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                    var jsonContent = await reader.ReadToEndAsync();

                    // 空检查
                    if (string.IsNullOrWhiteSpace(jsonContent))
                    {
                        // 标识状态码为 400
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;

                        // 输出 JSON
                        await context.Response.WriteAsync(SerializeToJson(new {
                            msg = "Request body is empty.",
                            ok = false
                        }));

                        return;
                    }

                    // 添加作业
                    var sResult = _schedulerFactory.TryAddJob(SchedulerBuilder.From(jsonContent.Trim()), out _);

                    // 处理添加识别情况
                    if (sResult != ScheduleResult.Succeed)
                    {
                        // 标识状态码为 500
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                        // 输出 JSON
                        await context.Response.WriteAsync(SerializeToJson(new {
                            msg = sResult.ToString(),
                            ok = false
                        }));

                        return;
                    }

                    // 输出 JSON
                    await context.Response.WriteAsync(SerializeToJson(new { ok = true, msg = sResult.ToString() }));
                    break;
                }
            // 获取所有运行记录
            case "/timelines-log":
                var allTimelines = _schedulerFactory.GetJobs()
                    .SelectMany(u => u.GetTriggers().SelectMany(s => s.GetTimelines()))
                    .OrderByDescending(u => u.CreatedTime)
                    .Take(20);  // 默认取 20 条

                // 输出 JSON
                await context.Response.WriteAsync(SerializeToJson(allTimelines));
                break;
            // 操作作业
            case "/operate-job":
                // 获取作业 Id
                var jobId = context.Request.Query["jobid"];
                // 获取操作方法
                var operate = context.Request.Query["action"];

                // 获取作业计划
                var scheduleResult = _schedulerFactory.TryGetJob(jobId, out var scheduler);

                // 处理找不到作业情况
                if (scheduleResult != ScheduleResult.Succeed)
                {
                    // 标识状态码为 500
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    // 输出 JSON
                    await context.Response.WriteAsync(SerializeToJson(new {
                        msg = scheduleResult.ToString(),
                        ok = false
                    }));

                    return;
                }

                switch (operate)
                {
                    // 启动作业
                    case "start":
                        scheduler?.Start();
                        break;
                    // 暂停作业
                    case "pause":
                        scheduler?.Pause();
                        break;
                    // 移除作业
                    case "remove":
                        _schedulerFactory.RemoveJob(jobId);
                        break;
                    // 手动执行
                    case "run":
                        _schedulerFactory.RunJob(jobId);
                        break;
                }

                // 输出 JSON
                await context.Response.WriteAsync(SerializeToJson(new {
                    msg = ScheduleResult.Succeed.ToString(),
                    ok = true
                }));

                break;
            // 操作触发器
            case "/operate-trigger":
                // 获取作业 Id
                var jobId1 = context.Request.Query["jobid"];
                var triggerId = context.Request.Query["triggerid"];
                // 获取操作方法
                var operate1 = context.Request.Query["action"];

                // 获取作业计划
                var scheduleResult1 = _schedulerFactory.TryGetJob(jobId1, out var scheduler1);

                // 处理找不到作业情况
                if (scheduleResult1 != ScheduleResult.Succeed)
                {
                    // 标识状态码为 500
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    // 输出 JSON
                    await context.Response.WriteAsync(SerializeToJson(new {
                        msg = scheduleResult1.ToString(),
                        ok = false
                    }));

                    return;
                }

                switch (operate1)
                {
                    // 启动作业触发器
                    case "start":
                        scheduler1?.StartTrigger(triggerId);
                        break;
                    // 暂停作业触发器
                    case "pause":
                        scheduler1?.PauseTrigger(triggerId);
                        break;
                    // 移除作业触发器
                    case "remove":
                        scheduler1?.RemoveTrigger(triggerId);
                        break;
                    // 手动执行
                    case "run":
                        {
                            // 读取内容
                            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                            var jsonContent = await reader.ReadToEndAsync();

                            Dictionary<string, object>? customData = null;

                            // 空检查
                            if (!string.IsNullOrWhiteSpace(jsonContent))
                            {
                                customData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent.Trim(), _jsonSerializerOptions);
                            }

                            scheduler1?.Run(triggerId, customData);
                            break;
                        }
                    // 获取作业触发器最近运行时间
                    case "timelines":
                        var trigger = scheduler1?.GetTrigger(triggerId);
                        var timelines = trigger?.GetTimelines() ?? [];

                        // 输出 JSON
                        await context.Response.WriteAsync(SerializeToJson(timelines));
                        return;
                }

                // 输出 JSON
                await context.Response.WriteAsync(SerializeToJson(new {
                    msg = ScheduleResult.Succeed.ToString(),
                    ok = true
                }));

                break;

            // 推送更新
            case "/check-change":
                // 检查请求类型，是否为 text/event-stream 格式
                if (!context.WebSockets.IsWebSocketRequest && context.Request.Headers.Accept.ToString().Contains("text/event-stream"))
                {
                    // 设置响应头的 content-type 为 text/event-stream
                    context.Response.ContentType = "text/event-stream";

                    // 设置响应头，启用响应发送保持活动性
                    context.Response.Headers.CacheControl = "no-cache";

                    // 防止 Nginx 缓存 Server-Sent Events
                    context.Response.Headers["X-Accel-Buffering"] = "no";

                    var channel = Channel.CreateUnbounded<JobDetail>();
                    EventHandler<SchedulerEventArgs> subscribeHandler = null;

                    // 创建关联取消 Token
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, _hostApplicationLifetime.ApplicationStopping);
                    var cancelToken = linkedCts.Token;

                    try
                    {
                        // 监听作业计划变化
                        subscribeHandler = (sender, args) =>
                        {
                            channel.Writer.TryWrite(args.JobDetail);
                        };

                        _schedulerFactory.OnChanged += subscribeHandler;

                        // 持续发送 SSE 协议数据
                        await foreach (var jobDetail in channel.Reader.ReadAllAsync(cancelToken))
                        {
                            // 如果请求已终止或服务器关闭则停止推送
                            if (cancelToken.IsCancellationRequested)
                            {
                                break;
                            }

                            var message = "data: " + SerializeToJson(jobDetail) + "\n\n";
                            await context.Response.WriteAsync(message, cancelToken);
                        }
                    }
                    catch { }
                    finally
                    {
                        // 取消订阅
                        if (subscribeHandler != null)
                        {
                            _schedulerFactory.OnChanged -= subscribeHandler;
                        }

                        // 标记 Channel 写入完成
                        channel.Writer.Complete();
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    await context.Response.WriteAsync("Only SSE (text/event-stream) is supported.");
                }
                break;
            // 登录验证
            case "/login":
                var username = context.Request.Form["username"];
                var password = context.Request.Form["password"];

                try
                {
                    // 调用自定义验证逻辑
                    if (Options.LoginConfig?.OnLoging is not null && await Options.LoginConfig.OnLoging(username, password, context))
                    {
                        context.Response.Headers["access-token"] = Options.LoginConfig.AppSecret;
                        context.Response.Headers.Append("Access-Control-Expose-Headers", "access-token");
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        await context.Response.WriteAsync("OK");
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Invalid username or password");
                    }
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync(ex.Message);
                }

                break;
            // 未处理接口
            default:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("404 Not Found");
                return;
        }
    }

    /// <summary>
    /// 将对象输出为 JSON 字符串
    /// </summary>
    /// <param name="obj">对象</param>
    /// <returns><see cref="string"/></returns>
    private static string SerializeToJson(object obj)
    {
        return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
    }
}