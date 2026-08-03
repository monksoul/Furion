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
using Furion.Localization;
using Furion.Reflection;
using Furion.Templates.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Furion.FriendlyException;

/// <summary>
/// 抛异常静态类
/// </summary>
public static class Oops
{
    /// <summary>
    /// 方法错误异常特性
    /// </summary>
    private static readonly ConditionalWeakTable<MethodBase, MethodIfException> _errorMethods;

    /// <summary>
    /// 错误代码类型
    /// </summary>
    private static readonly HashSet<Type> _errorCodeTypes;

    /// <summary>
    /// 错误消息字典
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> _errorCodeMessages;

    /// <summary>
    /// 友好异常设置
    /// </summary>
    private static readonly FriendlyExceptionSettingsOptions _friendlyExceptionSettings;

    /// <summary>
    /// 枚举字段信息缓存
    /// </summary>
    private static readonly ConcurrentDictionary<(Type, string), (object Key, string Value)> _enumFieldCache;

    /// <summary>
    /// 构造函数
    /// </summary>
    static Oops()
    {
        _errorMethods = new();
        _friendlyExceptionSettings = App.GetConfig<FriendlyExceptionSettingsOptions>("FriendlyExceptionSettings", true);
        _errorCodeTypes = new(GetErrorCodeTypes());
        _errorCodeMessages = GetErrorCodeMessages();
        _enumFieldCache = new();
    }

    /// <summary>
    /// 抛出业务异常信息
    /// </summary>
    /// <param name="errorMessage">异常消息</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns>异常实例</returns>
    public static AppFriendlyException Bah(string errorMessage, params object[] args)
    {
        var friendlyException = Oh(errorMessage, typeof(ValidationException), args).StatusCode(StatusCodes.Status400BadRequest);
        friendlyException.ValidationException = true;
        return friendlyException;
    }

    /// <summary>
    /// 抛出业务异常信息
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns>异常实例</returns>
    public static AppFriendlyException Bah(object errorCode, params object[] args)
    {
        var friendlyException = Oh(errorCode, typeof(ValidationException), args).StatusCode(StatusCodes.Status400BadRequest);
        friendlyException.ValidationException = true;
        return friendlyException;
    }

    /// <summary>
    /// 抛出字符串异常
    /// </summary>
    /// <param name="errorMessage">异常消息</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns>异常实例</returns>
    public static AppFriendlyException Oh(string errorMessage, params object[] args)
    {
        var friendlyException = new AppFriendlyException(MontageErrorMessage(errorMessage, default, null, null, null, args), default);

        // 处理默认配置为业务异常问题
        if (_friendlyExceptionSettings.ThrowBah == true)
        {
            friendlyException.StatusCode(StatusCodes.Status400BadRequest);
            friendlyException.ValidationException = true;
        }
        return friendlyException;
    }

    /// <summary>
    /// 抛出字符串异常
    /// </summary>
    /// <param name="errorMessage">异常消息</param>
    /// <param name="exceptionType">具体异常类型</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns>异常实例</returns>
    public static AppFriendlyException Oh(string errorMessage, Type exceptionType, params object[] args)
    {
        var exceptionMessage = MontageErrorMessage(errorMessage, default, null, null, null, args);
        return new AppFriendlyException(exceptionMessage, default,
            Activator.CreateInstance(exceptionType, [exceptionMessage]) as Exception);
    }

    /// <summary>
    /// 抛出字符串异常
    /// </summary>
    /// <typeparam name="TException">具体异常类型</typeparam>
    /// <param name="errorMessage">异常消息</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns>异常实例</returns>
    public static AppFriendlyException Oh<TException>(string errorMessage, params object[] args)
        where TException : class
    {
        return Oh(errorMessage, typeof(TException), args);
    }

    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns>异常实例</returns>
    public static AppFriendlyException Oh(object errorCode, params object[] args)
    {
        var (ErrorCode, Message) = GetErrorCodeMessage(errorCode, null, args);
        var friendlyException = new AppFriendlyException(Message, errorCode) { ErrorCode = ErrorCode };

        // 处理默认配置为业务异常问题
        if (_friendlyExceptionSettings.ThrowBah == true)
        {
            friendlyException.StatusCode(StatusCodes.Status400BadRequest);
            friendlyException.ValidationException = true;
        }
        return friendlyException;
    }

    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <param name="exceptionType">具体异常类型</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns>异常实例</returns>
    public static AppFriendlyException Oh(object errorCode, Type exceptionType, params object[] args)
    {
        var (ErrorCode, Message) = GetErrorCodeMessage(errorCode, null, args);
        return new AppFriendlyException(Message, errorCode,
            Activator.CreateInstance(exceptionType, [Message]) as Exception)
        { ErrorCode = ErrorCode };
    }

    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <typeparam name="TException">具体异常类型</typeparam>
    /// <param name="errorCode">错误码</param>
    /// <param name="args">String.Format 参数</param>
    /// <returns>异常实例</returns>
    public static AppFriendlyException Oh<TException>(object errorCode, params object[] args)
        where TException : class
    {
        return Oh(errorCode, typeof(TException), args);
    }

    /// <summary>
    /// 获取错误码错误消息
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="hideErrorCode"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string Text(object errorCode, bool? hideErrorCode = null, params object[] args)
    {
        var (_, Message) = GetErrorCodeMessage(errorCode, hideErrorCode, args);
        return Message;
    }

    /// <summary>
    /// 获取错误码消息
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="hideErrorCode"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static (object ErrorCode, string Message) GetErrorCodeMessage(object errorCode, bool? hideErrorCode, params object[] args)
    {
        // 提取枚举字段名和全名
        string fieldName = null, fullName = null;
        if (errorCode != null && errorCode.GetType().IsEnum)
        {
            fieldName = Enum.GetName(errorCode.GetType(), errorCode);
            if (!string.IsNullOrEmpty(fieldName))
            {
                fullName = $"{errorCode.GetType().Name}.{fieldName}";
            }
        }

        errorCode = HandleEnumErrorCode(errorCode);

        // 获取出错的方法
        var methodIfException = GetEndPointExceptionMethod();

        // 获取当前状态码匹配异常特性
        var ifExceptionAttribute = methodIfException?.IfExceptionAttributes?.FirstOrDefault(u => u.ErrorCode != null && HandleEnumErrorCode(u.ErrorCode).ToString().Equals(errorCode.ToString()));

        // 获取错误码消息
        var errorCodeMessage = ifExceptionAttribute == null || string.IsNullOrWhiteSpace(ifExceptionAttribute.ErrorMessage)
            ? (_errorCodeMessages.GetValueOrDefault(errorCode.ToString()) ?? _friendlyExceptionSettings.DefaultErrorMessage)
            : ifExceptionAttribute.ErrorMessage;

        // 如果所有错误码都获取不到，则找全局 [IfException] 错误
        if (string.IsNullOrWhiteSpace(errorCodeMessage))
        {
            errorCodeMessage = methodIfException?.IfExceptionAttributes?.FirstOrDefault(u => u.ErrorCode == null && !string.IsNullOrWhiteSpace(u.ErrorMessage))?.ErrorMessage;
        }

        // 字符串格式化
        return (errorCode, MontageErrorMessage(errorCodeMessage, errorCode.ToString(), hideErrorCode, fieldName, fullName,
            args != null && args.Length > 0 ? args : ifExceptionAttribute?.Args));
    }

    /// <summary>
    /// 处理枚举类型错误码
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <returns></returns>
    private static object HandleEnumErrorCode(object errorCode)
    {
        // 获取类型
        var errorType = errorCode.GetType();

        // 判断是否是内置枚举类型，如果是解析特性
        if (_errorCodeTypes.Contains(errorType))
        {
            var enumName = Enum.GetName(errorType, errorCode);
            if (enumName != null)
            {
                var cacheKey = (errorType, enumName);
                var (Key, Value) = _enumFieldCache.GetOrAdd(cacheKey, key =>
                {
                    var field = key.Item1.GetField(key.Item2, BindingFlags.Public | BindingFlags.Static);
                    if (field != null && field.IsDefined(typeof(ErrorCodeItemMetadataAttribute), true))
                    {
                        return GetErrorCodeItemInformation(field);
                    }
                    return (errorCode, null);
                });

                if (Value != null)
                {
                    errorCode = Key;
                }
            }
        }

        return errorCode;
    }

    /// <summary>
    /// 获取错误代码类型
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<Type> GetErrorCodeTypes()
    {
        // 查找所有公开的枚举贴有 [ErrorCodeType] 特性的类型，包含嵌套类型
        var initialTypes = App.EffectiveTypes
            .Where(u => u.IsDefined(typeof(ErrorCodeTypeAttribute), true) && (u.IsEnum || u.IsClass))
            .ToList();

        // 获取错误代码提供器中定义的类型
        var errorCodeTypeProvider = App.GetService<IErrorCodeTypeProvider>(App.RootServices);
        if (errorCodeTypeProvider is { Definitions: not null }) initialTypes.AddRange(errorCodeTypeProvider.Definitions);

        // 构建最终错误代码类型
        var errorCodeTypes = new List<Type>();

        foreach (var type in initialTypes)
        {
            if (type.IsEnum)
            {
                errorCodeTypes.Add(type);
            }
            // 处理嵌套类型问题
            else if (type.IsClass)
            {
                var nestedEnums = type.GetNestedTypes(BindingFlags.Public)
                    .Where(u => u.IsDefined(typeof(ErrorCodeTypeAttribute), true) && u.IsEnum);

                errorCodeTypes.AddRange(nestedEnums);
            }
        }

        return errorCodeTypes.Distinct();
    }

    /// <summary>
    /// 获取所有错误消息
    /// </summary>
    /// <returns></returns>
    private static ConcurrentDictionary<string, string> GetErrorCodeMessages()
    {
        var defaultErrorCodeMessages = new ConcurrentDictionary<string, string>();

        // 查找所有 [ErrorCodeType] 类型中的 [ErrorCodeMetadata] 元数据定义
        var errorCodeMessages = _errorCodeTypes.SelectMany(u => u.GetFields(BindingFlags.Public | BindingFlags.Static).Where(u => u.IsDefined(typeof(ErrorCodeItemMetadataAttribute), true)))
            .Select(GetErrorCodeItemInformation)
            .ToDictionary(u => u.Key.ToString(), u => u.Value);

        defaultErrorCodeMessages.AddOrUpdate(errorCodeMessages);

        // 加载配置文件状态码
        var errorCodeMessageSettings = App.GetConfig<ErrorCodeMessageSettingsOptions>("ErrorCodeMessageSettings", true);
        if (errorCodeMessageSettings is { Definitions: not null })
        {
            // 获取所有参数大于1的配置
            var fitErrorCodes = errorCodeMessageSettings.Definitions
                .Where(u => u.Length > 1)
                .ToDictionary(u => u[0].ToString(), u => FixErrorCodeSettingMessage(u));

            defaultErrorCodeMessages.AddOrUpdate(fitErrorCodes);
        }

        return defaultErrorCodeMessages;
    }

    /// <summary>
    /// 处理异常配置数据
    /// </summary>
    /// <param name="errorCodes">错误消息配置对象</param>
    /// <remarks>
    /// 方式：数组第一个元素为错误码，第二个参数为错误消息，剩下的参数为错误码格式化字符串
    /// </remarks>
    /// <returns></returns>
    private static string FixErrorCodeSettingMessage(object[] errorCodes)
    {
        var args = errorCodes.Skip(2).ToArray();
        var errorMessage = errorCodes[1].ToString();
        return errorMessage.Format(args);
    }

    /// <summary>
    /// 获取堆栈中顶部抛异常方法
    /// </summary>
    /// <returns></returns>
    private static MethodIfException GetEndPointExceptionMethod()
    {
        try
        {
            // 获取调用堆栈信息
            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();

            if (frames == null || frames.Length == 0) return null;

            // 获取出错的堆栈信息，在 web 请求中获取控制器或动态API的方法，除外获取第一个出错的方法
            var stackFrame = frames.FirstOrDefault(u =>
            {
                var method = Reflect.UnwrapStateMachine(u.GetMethod());
                return method != null && (typeof(ControllerBase).IsAssignableFrom(method.DeclaringType) || typeof(IDynamicApiController).IsAssignableFrom(method.DeclaringType));
            }) ?? frames.FirstOrDefault(u =>
            {
                var method = Reflect.UnwrapStateMachine(u.GetMethod());
                return method != null && method.DeclaringType?.Namespace != typeof(Oops).Namespace;
            });

            if (stackFrame == null) return null;

            // 获取出错的方法
            var errorMethod = Reflect.UnwrapStateMachine(stackFrame.GetMethod());
            if (errorMethod == null) return null;

            // 判断是否已经缓存过该方法，避免重复解析
            if (_errorMethods.TryGetValue(errorMethod, out var methodIfException)) return methodIfException;

            // 获取堆栈中所有的 [IfException] 特性
            var ifExceptionAttributes = frames
                .Select(u => Reflect.UnwrapStateMachine(u.GetMethod()))
                .Where(m => m != null && m.IsDefined(typeof(IfExceptionAttribute), true))
                .SelectMany(m => m.GetCustomAttributes<IfExceptionAttribute>(true));

            // 组装方法异常对象
            methodIfException = new MethodIfException
            {
                ErrorMethod = errorMethod,
                IfExceptionAttributes = ifExceptionAttributes
            };

            return _errorMethods.GetValue(errorMethod, _ => methodIfException);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取错误代码信息
    /// </summary>
    /// <param name="fieldInfo">字段对象</param>
    /// <returns>(object key, object value)</returns>
    private static (object Key, string Value) GetErrorCodeItemInformation(FieldInfo fieldInfo)
    {
        var errorCodeItemMetadata = fieldInfo.GetCustomAttribute<ErrorCodeItemMetadataAttribute>();
        return (errorCodeItemMetadata.ErrorCode ?? fieldInfo.Name, errorCodeItemMetadata.ErrorMessage.Format(errorCodeItemMetadata.Args));
    }

    /// <summary>
    /// 获取错误码字符串
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="errorCode"></param>
    /// <param name="hideErrorCode">隐藏错误码</param>
    /// <param name="fieldName">枚举字段名</param>
    /// <param name="fullName">枚举全名，如 ErrorCodes.z1000</param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string MontageErrorMessage(string errorMessage, string errorCode, bool? hideErrorCode, string fieldName, string fullName, params object[] args)
    {
        // 支持读取配置渲染
        var realErrorMessage = errorMessage.Render();

        // 空检查
        if (realErrorMessage is null)
        {
            throw new InvalidOperationException($"No error message configured for error code [{errorCode}].");
        }

        // 多语言处理
        realErrorMessage = TryLocalize(realErrorMessage, fullName);

        // 判断是否隐藏错误码
        var msg = (hideErrorCode == true || _friendlyExceptionSettings.HideErrorCode == true || string.IsNullOrWhiteSpace(errorCode)
            ? string.Empty
            : $"[{errorCode}] ") + realErrorMessage;

        return msg.Format(args);
    }

    /// <summary>
    /// 尝试获取多语言翻译文本
    /// </summary>
    /// <param name="errorMessage">原始错误消息</param>
    /// <param name="fullName">枚举全名，如 ErrorCodes.z1000</param>
    /// <returns></returns>
    private static string TryLocalize(string errorMessage, string fullName)
    {
        if (L.Text == null) return errorMessage;

        // 尝试使用 ErrorMessage 原文作为键
        var localizedString = L.Text[errorMessage];
        if (!localizedString.ResourceNotFound) return localizedString.Value;

        // 若未找到，尝试使用枚举全名（如 ErrorCodes.z1000）
        if (!string.IsNullOrEmpty(fullName))
        {
            localizedString = L.Text[fullName];
            if (!localizedString.ResourceNotFound) return localizedString.Value;
        }

        return errorMessage;
    }
}