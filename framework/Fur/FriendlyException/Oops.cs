using Fur.DependencyInjection;
using Fur.DynamicApiController;
using Fur.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Fur.FriendlyException
{
    /// <summary>
    /// 抛异常静态类
    /// </summary>
    [SkipScan]
    public static class Oops
    {
        /// <summary>
        /// MiniProfiler 分类名
        /// </summary>
        private const string MiniProfilerCategory = "errors";

        /// <summary>
        /// 方法错误异常特性
        /// </summary>
        private static readonly ConcurrentDictionary<MethodBase, MethodIfException> ErrorMethods;

        /// <summary>
        /// 错误代码类型
        /// </summary>
        private static readonly IEnumerable<Type> ErrorCodeTypes;

        /// <summary>
        /// 错误消息字典
        /// </summary>
        private static readonly Dictionary<string, string> ErrorCodeMessages;

        /// <summary>
        /// 友好异常设置
        /// </summary>
        private static readonly FriendlyExceptionSettingsOptions _friendlyExceptionSettings;

        /// <summary>
        /// 构造函数
        /// </summary>
        static Oops()
        {
            ErrorMethods = new ConcurrentDictionary<MethodBase, MethodIfException>();
            _friendlyExceptionSettings = App.GetService<IOptions<FriendlyExceptionSettingsOptions>>().Value;
            ErrorCodeTypes = GetErrorCodeTypes();
            ErrorCodeMessages = GetErrorCodeMessages();
        }

        /// <summary>
        /// 抛出字符串异常
        /// </summary>
        /// <param name="errorMessage">异常消息</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static Exception Oh(string errorMessage, params object[] args)
        {
            return new Exception(FormatErrorMessage(MontageErrorMessage(errorMessage), args));
        }

        /// <summary>
        /// 抛出字符串异常
        /// </summary>
        /// <param name="errorMessage">异常消息</param>
        /// <param name="exceptionType">具体异常类型</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static Exception Oh(string errorMessage, Type exceptionType, params object[] args)
        {
            return Activator.CreateInstance(exceptionType, new object[] { FormatErrorMessage(MontageErrorMessage(errorMessage), args) }) as Exception;
        }

        /// <summary>
        /// 抛出错误码异常
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static Exception Oh(object errorCode, params object[] args)
        {
            return new Exception(GetErrorCodeMessage(errorCode, args));
        }

        /// <summary>
        /// 抛出错误码异常
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="exceptionType">具体异常类型</param>
        /// <param name="args">String.Format 参数</param>
        /// <returns>异常实例</returns>
        public static Exception Oh(object errorCode, Type exceptionType, params object[] args)
        {
            return Activator.CreateInstance(exceptionType, new object[] { GetErrorCodeMessage(errorCode, args) }) as Exception;
        }

        /// <summary>
        /// 打印错误到 MiniProfiler 中
        /// </summary>
        /// <param name="exception"></param>
        internal static void PrintToMiniProfiler(Exception exception)
        {
            // 判断是否注入 MiniProfiler 组件
            if (App.Settings.InjectMiniProfiler != true) return;

            // 获取异常堆栈
            var traceFrame = new StackTrace(exception, true).GetFrame(0);

            // 获取出错的文件名
            var exceptionFileName = traceFrame.GetFileName();

            // 获取出错的行号
            var exceptionFileLineNumber = traceFrame.GetFileLineNumber();

            // 打印错误文件名和行号
            if (!string.IsNullOrEmpty(exceptionFileName) && exceptionFileLineNumber > 0)
            {
                App.PrintToMiniProfiler(MiniProfilerCategory, "Locator", $"{exceptionFileName}:line {exceptionFileLineNumber}", true);
            }

            // 打印完整的堆栈信息
            App.PrintToMiniProfiler(MiniProfilerCategory, "StackTrace", exception.ToString(), true);
        }

        /// <summary>
        /// 格式化错误消息
        /// </summary>
        /// <param name="errorMessage">错误消息</param>
        /// <param name="args">格式化参数</param>
        /// <returns></returns>
        internal static string FormatErrorMessage(string errorMessage, params object[] args)
        {
            return args == null || args.Length == 0 ? errorMessage : string.Format(errorMessage, args);
        }

        /// <summary>
        /// 获取错误码消息
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string GetErrorCodeMessage(object errorCode, params object[] args)
        {
            errorCode = HandleEnumErrorCode(errorCode);

            // 获取出错的方法
            var methodIfException = GetEndPointExceptionMethod();

            // 获取异常特性
            var ifExceptionAttribute = methodIfException.IfExceptionAttributes.FirstOrDefault(u => HandleEnumErrorCode(u.ErrorCode).ToString().Equals(errorCode.ToString()));

            // 获取错误码消息
            var errorCodeMessage = ifExceptionAttribute == null || string.IsNullOrEmpty(ifExceptionAttribute.ErrorMessage)
                ? (ErrorCodeMessages.GetValueOrDefault(errorCode.ToString()) ?? _friendlyExceptionSettings.DefaultErrorMessage)
                : ifExceptionAttribute.ErrorMessage;

            // 采用 [IfException] 格式化参数覆盖
            errorCodeMessage = FormatErrorMessage(errorCodeMessage, ifExceptionAttribute?.Args);

            // 字符串格式化
            return FormatErrorMessage(MontageErrorMessage(errorCodeMessage, errorCode.ToString()), args);
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
            if (ErrorCodeTypes.Any(u => u == errorType))
            {
                var fieldinfo = errorType.GetField(Enum.GetName(errorType, errorCode));
                if (fieldinfo.IsDefined(typeof(ErrorCodeItemMetadataAttribute), true))
                {
                    errorCode = GetErrorCodeItemMessage(fieldinfo).Key;
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
            // 查找所有公开的枚举贴有 [ErrorCodeType] 特性的类型
            var errorCodeTypes = App.CanBeScanTypes
                .Where(u => u.IsDefined(typeof(ErrorCodeTypeAttribute), true) && u.IsEnum)
                .ToList();

            // 获取错误代码提供器中定义的类型
            var errorCodeTypeProvider = App.GetService<IErrorCodeTypeProvider>();
            if (errorCodeTypeProvider is { Definitions: not null }) errorCodeTypes.AddRange(errorCodeTypeProvider.Definitions);

            return errorCodeTypes.Distinct();
        }

        /// <summary>
        /// 获取所有错误消息
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> GetErrorCodeMessages()
        {
            // 查找所有 [ErrorCodeType] 类型中的 [ErrorCodeMetadata] 元数据定义
            var errorCodeMessages = ErrorCodeTypes.SelectMany(u => u.GetFields().Where(u => u.IsDefined(typeof(ErrorCodeItemMetadataAttribute))))
                .Select(u => GetErrorCodeItemMessage(u))
               .ToDictionary(u => u.Key.ToString(), u => u.Value);

            // 加载配置文件状态码
            var errorCodeMessageSettings = App.GetDuplicateOptions<ErrorCodeMessageSettingsOptions>();
            if (errorCodeMessageSettings is { Definitions: not null })
            {
                // 获取所有参数大于1的配置
                var fitErrorCodes = errorCodeMessageSettings.Definitions
                    .Where(u => u.Length > 1)
                    .ToDictionary(u => u[0].ToString(), u => FixErrorCodeSettingMessage(u));

                // 合并两个字典
                errorCodeMessages = (errorCodeMessages ?? new Dictionary<string, string>()).AddOrUpdate(fitErrorCodes);
            }

            return errorCodeMessages;
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
            return FormatErrorMessage(errorMessage, args);
        }

        /// <summary>
        /// 获取堆栈中顶部抛异常方法
        /// </summary>
        /// <returns></returns>
        private static MethodIfException GetEndPointExceptionMethod()
        {
            // 获取调用堆栈信息
            var stackTrace = EnhancedStackTrace.Current();

            // 获取出错的堆栈信息
            var stackFrame = stackTrace.FirstOrDefault(u => typeof(ControllerBase).IsAssignableFrom(u.MethodInfo.DeclaringType) || typeof(IDynamicApiController).IsAssignableFrom(u.MethodInfo.DeclaringType));

            // 获取出错的方法
            var errorMethod = stackFrame.MethodInfo.MethodBase;

            // 判断是否已经缓存过该方法，避免重复解析
            var isCached = ErrorMethods.TryGetValue(errorMethod, out var methodIfException);
            if (isCached) return methodIfException;

            // 获取堆栈中所有的 [IfException] 特性
            var ifExceptionAttributes = stackTrace
                .Where(u => u.MethodInfo.MethodBase != null && u.MethodInfo.MethodBase.IsDefined(typeof(IfExceptionAttribute), true))
                .SelectMany(u => u.MethodInfo.MethodBase.GetCustomAttributes<IfExceptionAttribute>(true)).Where(u => u.ErrorCode != null);

            // 组装方法异常对象
            methodIfException = new MethodIfException
            {
                ErrorMethod = errorMethod,
                IfExceptionAttributes = ifExceptionAttributes
            };

            // 存入缓存
            ErrorMethods.TryAdd(errorMethod, methodIfException);

            return methodIfException;
        }

        /// <summary>
        /// 获取错误代码消息实体
        /// </summary>
        /// <param name="fieldInfo">字段对象</param>
        /// <returns>(object key, object value)</returns>
        private static (object Key, string Value) GetErrorCodeItemMessage(FieldInfo fieldInfo)
        {
            var errorCodeItemMetadata = fieldInfo.GetCustomAttribute<ErrorCodeItemMetadataAttribute>();
            return (errorCodeItemMetadata.ErrorCode ?? fieldInfo.Name, errorCodeItemMetadata.ErrorMessage);
        }

        /// <summary>
        /// 获取错误码字符串
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        private static string MontageErrorMessage(string errorMessage, string errorCode = default)
        {
            if (errorMessage.StartsWith("[Validation]")) return errorMessage;

            return (_friendlyExceptionSettings.HideErrorCode == true || string.IsNullOrEmpty(errorCode) ? string.Empty : $"[{errorCode}] ") + errorMessage;
        }
    }
}