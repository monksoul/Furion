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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Furion.JsonSerialization;

/// <summary>
/// System.Text.Json 序列化提供器（默认实现）
/// </summary>
internal class SystemTextJsonSerializerProvider : IJsonSerializerProvider
{
    /// <summary>
    /// 获取 JSON 配置选项
    /// </summary>
    private readonly JsonOptions _jsonOptions;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public SystemTextJsonSerializerProvider(IOptions<JsonOptions> options)
    {
        _jsonOptions = options.Value;
    }

    /// <summary>
    /// 序列化对象
    /// </summary>
    /// <param name="value"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    public string Serialize(object value, object jsonSerializerOptions = null)
    {
        return JsonSerializer.Serialize(value, GetJsonSerializerOptions(jsonSerializerOptions));
    }

    /// <summary>
    /// 反序列化字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    public T Deserialize<T>(string json, object jsonSerializerOptions = null)
    {
        return JsonSerializer.Deserialize<T>(json, GetJsonSerializerOptions(jsonSerializerOptions));
    }

    /// <summary>
    /// 反序列化字符串
    /// </summary>
    /// <param name="json"></param>
    /// <param name="returnType"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    public object Deserialize(string json, Type returnType, object jsonSerializerOptions = null)
    {
        return JsonSerializer.Deserialize(json, returnType, GetJsonSerializerOptions(jsonSerializerOptions));
    }

    /// <summary>
    /// 返回读取全局配置的 JSON 选项
    /// </summary>
    /// <returns></returns>
    public object GetSerializerOptions()
    {
        return _jsonOptions?.JsonSerializerOptions;
    }

    /// <summary>
    /// 获取默认的序列化配置
    /// </summary>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    private JsonSerializerOptions GetJsonSerializerOptions(object jsonSerializerOptions = null)
    {
        var source = (jsonSerializerOptions as JsonSerializerOptions)
                      ?? GetSerializerOptions() as JsonSerializerOptions
                      ?? new JsonSerializerOptions();

        var optionsCopy = new JsonSerializerOptions(source);

        // 默认不区分大小写匹配
        optionsCopy.PropertyNameCaseInsensitive = true;

        return optionsCopy;
    }
}