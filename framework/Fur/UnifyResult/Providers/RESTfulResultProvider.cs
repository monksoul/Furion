using Fur.DependencyInjection;
using Fur.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fur.UnifyResult
{
    /// <summary>
    /// RESTful 风格返回值
    /// </summary>
    [SkipScan, UnifyModel(typeof(RESTfulResult<>))]
    public class RESTfulResultProvider : IUnifyResultProvider
    {
        /// <summary>
        /// 异常返回值
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IActionResult OnException(ExceptionContext context)
        {
            // 解析异常信息
            var (ErrorCode, ErrorContent) = UnifyResultContext.GetExceptionMetadata(context);

            return new JsonResult(new RESTfulResult<object>
            {
                StatusCode = ErrorCode,
                Successed = false,
                Data = null,
                Errors = ErrorContent,
                Extras = UnifyResultContext.Take(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        /// <summary>
        /// 成功返回值
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IActionResult OnSuccessed(ActionExecutedContext context)
        {
            object data;
            // 处理内容结果
            if (context.Result is ContentResult contentResult) data = contentResult.Content;
            // 处理对象结果
            else if (context.Result is ObjectResult objectResult) data = objectResult.Value;
            else return null;

            return new JsonResult(new RESTfulResult<object>
            {
                StatusCode = context.Result is EmptyResult ? StatusCodes.Status204NoContent : StatusCodes.Status200OK,  // 处理没有返回值情况 204
                Successed = true,
                Data = data,
                Errors = null,
                Extras = UnifyResultContext.Take(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        /// <summary>
        /// 验证失败返回值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="modelStates"></param>
        /// <param name="validationResults"></param>
        /// <param name="validateFailedMessage"></param>
        /// <returns></returns>
        public IActionResult OnValidateFailed(ActionExecutingContext context, ModelStateDictionary modelStates, Dictionary<string, IEnumerable<string>> validationResults, string validateFailedMessage)
        {
            return new JsonResult(new RESTfulResult<object>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Successed = false,
                Data = null,
                Errors = validationResults,
                Extras = UnifyResultContext.Take(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        /// <summary>
        /// 处理输出状态码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public async Task OnResponseStatusCodes(HttpContext context, int statusCode)
        {
            switch (statusCode)
            {
                // 处理 401 状态码
                case StatusCodes.Status401Unauthorized:
                    await context.Response.WriteAsJsonAsync(new RESTfulResult<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Successed = false,
                        Data = null,
                        Errors = "401 Unauthorized",
                        Extras = UnifyResultContext.Take(),
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }, JsonSerializerUtility.GetDefaultJsonSerializerOptions());
                    break;
                // 处理 403 状态码
                case StatusCodes.Status403Forbidden:
                    await context.Response.WriteAsJsonAsync(new RESTfulResult<object>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Successed = false,
                        Data = null,
                        Errors = "403 Forbidden",
                        Extras = UnifyResultContext.Take(),
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }, JsonSerializerUtility.GetDefaultJsonSerializerOptions());
                    break;

                default:
                    break;
            }
        }
    }
}