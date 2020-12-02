using System.Text.Encodings.Web;
using System.Text.Json;

namespace Fur.Utilities
{
    /// <summary>
    /// Json序列化静态工具类
    /// </summary>
    internal static class JsonSerializerUtility
    {
        /// <summary>
        /// 是否启用属性大写操作
        /// </summary>
        internal static bool EnabledPascalPropertyNaming = false;

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        internal static string Serialize(object obj, JsonSerializerOptions jsonSerializerOptions = default)
        {
            return JsonSerializer.Serialize(obj, jsonSerializerOptions ?? GetDefaultJsonSerializerOptions());
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        internal static T Deserialize<T>(string json, JsonSerializerOptions jsonSerializerOptions = default)
        {
            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions ?? GetDefaultJsonSerializerOptions());
        }

        /// <summary>
        /// 获取默认 JSON 序列化选项
        /// </summary>
        /// <returns></returns>
        internal static JsonSerializerOptions GetDefaultJsonSerializerOptions()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            if (EnabledPascalPropertyNaming)
            {
                jsonSerializerOptions.PropertyNamingPolicy = null;
            }

            return jsonSerializerOptions;
        }
    }
}