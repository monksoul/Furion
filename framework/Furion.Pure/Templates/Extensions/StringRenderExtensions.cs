// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.ClayObject.Extensions;
using Furion.DependencyInjection;
using Furion.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Furion.Templates.Extensions
{
    /// <summary>
    /// 字符串渲染模板拓展类
    /// </summary>
    [SuppressSniffer]
    public static class StringRenderExtensions
    {
        /// <summary>
        /// 模板正则表达式
        /// </summary>
        private const string commonTemplatePattern = @"\{(?<p>.+?)\}";

        /// <summary>
        /// 读取配置模板正则表达式
        /// </summary>
        private const string configTemplatePattern = @"\#\((?<p>.*?)\)";

        /// <summary>
        /// 渲染模板
        /// </summary>
        /// <param name="template"></param>
        /// <param name="templateData"></param>
        /// <param name="encode"></param>
        public static void Render(this string template, object templateData, bool encode = false)
        {
            if (template == null) return;

            template.Render(templateData == null ? default : templateData.ToDictionary(), encode);
        }

        /// <summary>
        /// 渲染模板
        /// </summary>
        /// <param name="template"></param>
        /// <param name="templateData"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Render(this string template, IDictionary<string, object> templateData, bool encode = false)
        {
            if (template == null) return default;

            // 如果模板为空，则跳过
            if (templateData == null || templateData.Count == 0) return template;

            // 判断字符串是否包含模板
            if (!Regex.IsMatch(template, commonTemplatePattern)) return template;

            // 获取所有匹配的模板
            var templateValues = Regex.Matches(template, commonTemplatePattern)
                                                       .Select(u => new {
                                                           Template = u.Groups["p"].Value,
                                                           Value = MatchTemplateValue(u.Groups["p"].Value, templateData)
                                                       });

            // 循环替换模板
            foreach (var item in templateValues)
            {
                template = template.Replace($"{{{item.Template}}}", encode ? Uri.EscapeDataString(item.Value?.ToString() ?? string.Empty) : item.Value?.ToString());
            }

            return template;
        }

        /// <summary>
        /// 从配置中渲染字符串模板
        /// </summary>
        /// <param name="template"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Render(this string template, bool encode = false)
        {
            if (template == null) return default;

            // 判断字符串是否包含模板
            if (!Regex.IsMatch(template, configTemplatePattern)) return template;

            // 获取所有匹配的模板
            var templateValues = Regex.Matches(template, configTemplatePattern)
                                                       .Select(u => new {
                                                           Template = u.Groups["p"].Value,
                                                           Value = App.Configuration[u.Groups["p"].Value]
                                                       });

            // 循环替换模板
            foreach (var item in templateValues)
            {
                template = template.Replace($"#({item.Template})", encode ? Uri.EscapeDataString(item.Value?.ToString() ?? string.Empty) : item.Value?.ToString());
            }

            return template;
        }

        /// <summary>
        /// 匹配模板值
        /// </summary>
        /// <param name="template"></param>
        /// <param name="templateData"></param>
        /// <returns></returns>
        private static object MatchTemplateValue(string template, IDictionary<string, object> templateData)
        {
            string tmpl;
            if (!template.Contains('.', StringComparison.CurrentCulture)) tmpl = template;
            else tmpl = template.Split('.', StringSplitOptions.RemoveEmptyEntries).First();

            var templateValue = templateData.ContainsKey(tmpl) ? templateData[tmpl] : default;
            return ResolveTemplateValue(template, templateValue);
        }

        /// <summary>
        /// 解析模板的值
        /// </summary>
        /// <param name="template"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static object ResolveTemplateValue(string template, object data)
        {
            // 根据 . 分割模板
            var propertyCrumbs = template.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return GetValue(propertyCrumbs, data);

            // 静态本地函数
            static object GetValue(string[] propertyCrumbs, object data)
            {
                if (data == null || propertyCrumbs == null || propertyCrumbs.Length <= 1) return data;
                var dataType = data.GetType();

                // 如果是基元类型则直接返回
                if (dataType.IsRichPrimitive()) return data;
                object value = null;

                // 递归获取下一级模板值
                for (var i = 1; i < propertyCrumbs.Length; i++)
                {
                    var propery = dataType.GetProperty(propertyCrumbs[i]);
                    if (propery == null) break;

                    value = propery.GetValue(data);
                    if (i + 1 < propertyCrumbs.Length)
                    {
                        value = GetValue(propertyCrumbs.Skip(i).ToArray(), value);
                    }
                    else break;
                }

                return value;
            }
        }
    }
}