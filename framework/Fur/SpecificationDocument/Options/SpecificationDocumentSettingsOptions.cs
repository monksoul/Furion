using Fur.ConfigurableOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Linq;

namespace Fur.SpecificationDocument
{
    /// <summary>
    /// 规范化文档配置选项
    /// </summary>
    public sealed class SpecificationDocumentSettingsOptions : IConfigurableOptions<SpecificationDocumentSettingsOptions>
    {
        /// <summary>
        /// 文档标题
        /// </summary>
        public string DocumentTitle { get; set; }

        /// <summary>
        /// 默认分组名
        /// </summary>
        public string DefaultGroupName { get; set; }

        /// <summary>
        /// 启用授权支持
        /// </summary>
        public bool? EnableAuthorized { get; set; }

        /// <summary>
        /// 格式化为V2版本
        /// </summary>
        public bool? FormatAsV2 { get; set; }

        /// <summary>
        /// 配置规范化文档地址
        /// </summary>
        public string RoutePrefix { get; set; }

        /// <summary>
        /// 配置虚拟目录
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// 文档展开设置
        /// </summary>
        public DocExpansion? DocExpansionState { get; set; }

        /// <summary>
        /// XML 描述文件
        /// </summary>
        public string[] XmlComments { get; set; }

        /// <summary>
        /// 分组信息
        /// </summary>
        public SpecificationOpenApiInfo[] GroupOpenApiInfos { get; set; }

        /// <summary>
        /// 安全定义
        /// </summary>
        public SpecificationOpenApiSecurityScheme[] SecurityDefinitions { get; set; }

        /// <summary>
        /// 配置 Servers
        /// </summary>
        public OpenApiServer[] Servers { get; set; }

        /// <summary>
        /// 隐藏 Servers
        /// </summary>
        public bool? HideServers { get; set; }

        /// <summary>
        /// 后期配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>
        public void PostConfigure(SpecificationDocumentSettingsOptions options, IConfiguration configuration)
        {
            options.DocumentTitle ??= "Specification Api Document";
            options.DefaultGroupName ??= "Default";
            options.FormatAsV2 ??= false;
            //options.RoutePrefix ??= "api";
            options.DocExpansionState ??= DocExpansion.List;
            XmlComments ??= App.Assemblies.Where(u => u.GetName().Name != "Fur").Select(t => t.GetName().Name).ToArray();
            GroupOpenApiInfos ??= new SpecificationOpenApiInfo[]
            {
                new SpecificationOpenApiInfo()
                {
                    Group=options.DefaultGroupName
                }
            };
            SecurityDefinitions ??= Array.Empty<SpecificationOpenApiSecurityScheme>();
            Servers ??= Array.Empty<OpenApiServer>();
            HideServers ??= false;
        }
    }
}