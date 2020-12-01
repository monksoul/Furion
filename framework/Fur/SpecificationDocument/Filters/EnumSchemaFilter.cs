using Fur.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fur.SpecificationDocument
{
    /// <summary>
    /// 修正 规范化文档 Enum 提示
    /// </summary>
    /// <remarks>
    /// 参考https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1269#issuecomment-577182931
    /// </remarks>
    [SkipScan]
    public class EnumSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// 实现过滤器方法
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            var type = context.Type;

            // 排除其他程序集的枚举
            if (type.IsEnum && App.Assemblies.Contains(type.Assembly))
            {
                model.Enum.Clear();
                var stringBuilder = new StringBuilder();
                stringBuilder.Append($"{model.Description}<br />");

                var enumValues = Enum.GetValues(type);
                foreach (var option in enumValues)
                {
                    var value = (int)option;

                    // 获取枚举成员特性
                    var fieldinfo = type.GetField(Enum.GetName(type, value));
                    var descriptionAttribute = fieldinfo.GetCustomAttribute<DescriptionAttribute>(true);

                    model.Enum.Add(new OpenApiInteger(value));
                    stringBuilder.Append($"&nbsp;{descriptionAttribute?.Description} {option} = {value}<br />");
                }
                model.Description = stringBuilder.ToString();
            }
        }
    }
}