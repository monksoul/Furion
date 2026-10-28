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

using Furion.Extensions;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Furion.SpecificationDocument;

/// <summary>
/// 应用自定义多套序列化配置生成 Schema
/// </summary>
public class UnifySerializerSchemaFilter : IOperationFilter
{
    /// <summary>
    /// 每个文档、每套序列化配置独立的 Schema 缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, SchemaRepository> _schemaRepositories = new();

    /// <summary>
    /// 实现过滤器方法
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;
        if (method == null) return;

        // 查找 [UnifySerializerSetting] 特性，方法没有则查找声明类
        var unifySerializerSettingAttribute = method.GetFoundAttribute<UnifySerializerSettingAttribute>(true, true);
        if (unifySerializerSettingAttribute == null || string.IsNullOrWhiteSpace(unifySerializerSettingAttribute.Name)) return;

        // 获取注册的自定义序列化配置
        var serializerSettings = UnifyContext.GetSerializerSettings(unifySerializerSettingAttribute.Name);
        if (serializerSettings is not JsonSerializerOptions jsonSerializerOptions) return;

        // 构建应用自定义序列化配置的 Schema 生成器
        var schemaGenerator = CreateSchemaGenerator(unifySerializerSettingAttribute.Name, jsonSerializerOptions);

        // 获取当前文档独立的 Schema 
        var repositoryKey = $"{context.DocumentName}::{unifySerializerSettingAttribute.Name}";
        var schemaRepository = _schemaRepositories.GetOrAdd(repositoryKey, _ => new SchemaRepository(context.DocumentName));

        // 重新生成请求体 Schema
        GenerateRequestBodySchema(operation, context, schemaGenerator, schemaRepository);

        // 重新生成响应体 Schema
        GenerateResponseSchemas(operation, context, schemaGenerator, schemaRepository);

        // 将自定义 Schema 合并到当前文档
        MergeSchemas(context, schemaRepository);
    }

    /// <summary>
    /// 构建应用自定义序列化配置的 Schema 生成器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    private static SchemaGenerator CreateSchemaGenerator(string name, JsonSerializerOptions jsonSerializerOptions)
    {
        // 复制当前全局 Schema 生成器配置
        var generatorOptions = App.GetOptions<SchemaGeneratorOptions>();

        var dataContractResolver = new JsonSerializerDataContractResolver(jsonSerializerOptions, generatorOptions);
        return new SchemaGenerator(generatorOptions, dataContractResolver);
    }

    /// <summary>
    /// 重新生成请求体 Schema
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    /// <param name="schemaGenerator"></param>
    /// <param name="schemaRepository"></param>
    private static void GenerateRequestBodySchema(OpenApiOperation operation, OperationFilterContext context
        , ISchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
    {
        if (operation.RequestBody?.Content == null || operation.RequestBody.Content.Count == 0) return;

        // 仅处理 [FromBody] 参数，表单参数不经过 JSON 序列化
        var bodyParameter = context.ApiDescription.ParameterDescriptions.FirstOrDefault(u => u.Source == BindingSource.Body);
        if (bodyParameter?.ModelMetadata?.ModelType == null) return;

        foreach (var mediaType in operation.RequestBody.Content.Values)
        {
            mediaType.Schema = schemaGenerator.GenerateSchema(bodyParameter.ModelMetadata.ModelType, schemaRepository
                , bodyParameter.PropertyInfo(), bodyParameter.ParameterInfo());
        }
    }

    /// <summary>
    /// 重新生成响应体 Schema
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    /// <param name="schemaGenerator"></param>
    /// <param name="schemaRepository"></param>
    private static void GenerateResponseSchemas(OpenApiOperation operation, OperationFilterContext context
        , ISchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
    {
        if (operation.Responses == null) return;

        // 按状态码分组所有返回类型
        var responseTypesGroup = context.ApiDescription.SupportedResponseTypes
            .GroupBy(u => u.IsDefaultResponse ? "default" : u.StatusCode.ToString())
            .ToDictionary(u => u.Key
                , u => u.Select(x => x.ModelMetadata?.ModelType ?? x.Type).Where(x => x != null).Distinct().ToList());

        foreach (var (statusCode, response) in operation.Responses)
        {
            if (response.Content == null
                || !responseTypesGroup.TryGetValue(statusCode, out var responseTypes) || responseTypes.Count == 0) continue;

            foreach (var mediaType in response.Content.Values)
            {
                var schemas = responseTypes.Select(u => schemaGenerator.GenerateSchema(u, schemaRepository)).ToList();
                mediaType.Schema = schemas.Count == 1 ? schemas[0] : new OpenApiSchema { AnyOf = schemas };
            }
        }
    }

    /// <summary>
    /// 将自定义 Schema 合并到当前文档
    /// </summary>
    /// <param name="context"></param>
    /// <param name="schemaRepository"></param>
    private static void MergeSchemas(OperationFilterContext context, SchemaRepository schemaRepository)
    {
        foreach (var schema in schemaRepository.Schemas)
        {
            context.SchemaRepository.Schemas[schema.Key] = schema.Value;
        }
    }
}
