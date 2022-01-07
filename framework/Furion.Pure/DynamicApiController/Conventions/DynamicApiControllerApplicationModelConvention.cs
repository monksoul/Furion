// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.Extensions;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Furion.DynamicApiController
{
    /// <summary>
    /// 动态接口控制器应用模型转换器
    /// </summary>
    internal sealed class DynamicApiControllerApplicationModelConvention : IApplicationModelConvention
    {
        /// <summary>
        /// 动态接口控制器配置实例
        /// </summary>
        private readonly DynamicApiControllerSettingsOptions _dynamicApiControllerSettings;

        /// <summary>
        /// 带版本的名称正则表达式
        /// </summary>
        private readonly Regex _nameVersionRegex;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DynamicApiControllerApplicationModelConvention()
        {
            _dynamicApiControllerSettings = App.GetConfig<DynamicApiControllerSettingsOptions>("DynamicApiControllerSettings", true);
            LoadVerbToHttpMethodsConfigure();
            _nameVersionRegex = new Regex(@"V(?<version>[0-9_]+$)");
        }

        /// <summary>
        /// 配置应用模型信息
        /// </summary>
        /// <param name="application">引用模型</param>
        public void Apply(ApplicationModel application)
        {
            var controllers = application.Controllers.Where(u => Penetrates.IsApiController(u.ControllerType));
            foreach (var controller in controllers)
            {
                var controllerType = controller.ControllerType;

                // 判断是否处理 Mvc控制器
                if (typeof(ControllerBase).IsAssignableFrom(controller.ControllerType))
                {
                    if (!_dynamicApiControllerSettings.SupportedMvcController.Value || controller.ApiExplorer?.IsVisible == false)
                    {
                        // 控制器默认处理规范化结果
                        if (UnifyContext.EnabledUnifyHandler)
                        {
                            foreach (var action in controller.Actions)
                            {
                                // 配置动作方法规范化特性
                                ConfigureActionUnifyResultAttribute(action);
                            }
                        }

                        continue;
                    }
                }

                var controllerApiDescriptionSettings = controllerType.IsDefined(typeof(ApiDescriptionSettingsAttribute), true) ? controllerType.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true) : default;
                ConfigureController(controller, controllerApiDescriptionSettings);
            }
        }

        /// <summary>
        /// 配置控制器
        /// </summary>
        /// <param name="controller">控制器模型</param>
        /// <param name="controllerApiDescriptionSettings">接口描述配置</param>
        private void ConfigureController(ControllerModel controller, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings)
        {
            // 配置区域
            ConfigureControllerArea(controller, controllerApiDescriptionSettings);

            // 配置控制器名称
            ConfigureControllerName(controller, controllerApiDescriptionSettings);

            // 存储排序给 Swagger 使用
            Penetrates.ControllerOrderCollection.TryAdd(controller.ControllerName, controllerApiDescriptionSettings?.Order ?? 0);

            var actions = controller.Actions;

            // 查找所有重复的方法签名
            var repeats = actions.GroupBy(u => new { u.ActionMethod.ReflectedType.Name, Signature = u.ActionMethod.ToString() })
                                 .Where(u => u.Count() > 1)
                                 .SelectMany(u => u.Where(u => u.ActionMethod.ReflectedType.Name != u.ActionMethod.DeclaringType.Name));

            // 2021年04月01日 https://docs.microsoft.com/en-US/aspnet/core/web-api/?view=aspnetcore-5.0#binding-source-parameter-inference
            // 判断是否贴有 [ApiController] 特性
            var hasApiControllerAttribute = controller.Attributes.Any(u => u.GetType() == typeof(ApiControllerAttribute));

            foreach (var action in actions)
            {
                // 跳过相同方法签名
                if (repeats.Contains(action))
                {
                    action.ApiExplorer.IsVisible = false;
                    continue;
                };

                var actionMethod = action.ActionMethod;
                var actionApiDescriptionSettings = actionMethod.IsDefined(typeof(ApiDescriptionSettingsAttribute), true) ? actionMethod.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true) : default;
                ConfigureAction(action, actionApiDescriptionSettings, controllerApiDescriptionSettings, hasApiControllerAttribute);
            }
        }

        /// <summary>
        /// 配置控制器区域
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="controllerApiDescriptionSettings"></param>
        private void ConfigureControllerArea(ControllerModel controller, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings)
        {
            // 如果配置了区域，则跳过
            if (controller.RouteValues.ContainsKey("area")) return;

            // 如果没有配置区域，则跳过
            var area = controllerApiDescriptionSettings?.Area ?? _dynamicApiControllerSettings.DefaultArea;
            if (string.IsNullOrWhiteSpace(area)) return;

            controller.RouteValues["area"] = area;
        }

        /// <summary>
        /// 配置控制器名称
        /// </summary>
        /// <param name="controller">控制器模型</param>
        /// <param name="controllerApiDescriptionSettings">接口描述配置</param>
        private void ConfigureControllerName(ControllerModel controller, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings)
        {
            var (Name, _, _) = ConfigureControllerAndActionName(controllerApiDescriptionSettings, controller.ControllerType.Name, _dynamicApiControllerSettings.AbandonControllerAffixes, _ => _);
            controller.ControllerName = Name;
        }

        /// <summary>
        /// 配置动作方法
        /// </summary>
        /// <param name="action">控制器模型</param>
        /// <param name="apiDescriptionSettings">接口描述配置</param>
        /// <param name="controllerApiDescriptionSettings">控制器接口描述配置</param>
        /// <param name="hasApiControllerAttribute">是否贴有 ApiController 特性</param>
        private void ConfigureAction(ActionModel action, ApiDescriptionSettingsAttribute apiDescriptionSettings, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings, bool hasApiControllerAttribute)
        {
            // 配置动作方法接口可见性
            ConfigureActionApiExplorer(action);

            // 配置动作方法名称
            var (isLowercaseRoute, isKeepName) = ConfigureActionName(action, apiDescriptionSettings, controllerApiDescriptionSettings);

            // 配置动作方法请求谓词特性
            ConfigureActionHttpMethodAttribute(action);

            // 配置引用类型参数
            ConfigureClassTypeParameter(action);

            // 配置动作方法路由特性
            ConfigureActionRouteAttribute(action, apiDescriptionSettings, controllerApiDescriptionSettings, isLowercaseRoute, isKeepName, hasApiControllerAttribute);

            // 配置动作方法规范化特性
            if (UnifyContext.EnabledUnifyHandler) ConfigureActionUnifyResultAttribute(action);
        }

        /// <summary>
        /// 配置动作方法接口可见性
        /// </summary>
        /// <param name="action">动作方法模型</param>
        private static void ConfigureActionApiExplorer(ActionModel action)
        {
            if (!action.ApiExplorer.IsVisible.HasValue) action.ApiExplorer.IsVisible = true;
        }

        /// <summary>
        /// 配置动作方法名称
        /// </summary>
        /// <param name="action">动作方法模型</param>
        /// <param name="apiDescriptionSettings">接口描述配置</param>
        /// <param name="controllerApiDescriptionSettings"></param>
        /// <returns></returns>
        private (bool IsLowercaseRoute, bool IsKeepName) ConfigureActionName(ActionModel action, ApiDescriptionSettingsAttribute apiDescriptionSettings, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings)
        {
            var (Name, IsLowercaseRoute, IsKeepName) = ConfigureControllerAndActionName(apiDescriptionSettings, action.ActionMethod.Name, _dynamicApiControllerSettings.AbandonActionAffixes, (tempName) =>
              {
                  // 处理动作方法名称谓词
                  if (!CheckIsKeepVerb(apiDescriptionSettings, controllerApiDescriptionSettings))
                  {
                      var words = tempName.SplitCamelCase();
                      var verbKey = words.First().ToLower();
                      // 处理类似 getlist,getall 多个单词
                      if (words.Length > 1 && Penetrates.VerbToHttpMethods.ContainsKey((words[0] + words[1]).ToLower()))
                      {
                          tempName = tempName[(words[0] + words[1]).Length..];
                      }
                      else if (Penetrates.VerbToHttpMethods.ContainsKey(verbKey)) tempName = tempName[verbKey.Length..];
                  }

                  return tempName;
              }, controllerApiDescriptionSettings);
            action.ActionName = Name;

            return (IsLowercaseRoute, IsKeepName);
        }

        /// <summary>
        /// 配置动作方法请求谓词特性
        /// </summary>
        /// <param name="action">动作方法模型</param>
        private void ConfigureActionHttpMethodAttribute(ActionModel action)
        {
            var selectorModel = action.Selectors[0];
            // 跳过已配置请求谓词特性的配置
            if (selectorModel.ActionConstraints.Count > 0) return;

            // 解析请求谓词
            var words = action.ActionMethod.Name.SplitCamelCase();
            var verbKey = words.First().ToLower();

            // 处理类似 getlist,getall 多个单词
            if (words.Length > 1 && Penetrates.VerbToHttpMethods.ContainsKey((words[0] + words[1]).ToLower()))
            {
                verbKey = (words[0] + words[1]).ToLower();
            }

            var verb = Penetrates.VerbToHttpMethods.ContainsKey(verbKey)
                ? Penetrates.VerbToHttpMethods[verbKey] ?? _dynamicApiControllerSettings.DefaultHttpMethod.ToUpper()
                : _dynamicApiControllerSettings.DefaultHttpMethod.ToUpper();

            // 添加请求约束
            selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { verb }));

            // 添加请求谓词特性
            HttpMethodAttribute httpMethodAttribute = verb switch
            {
                "GET" => new HttpGetAttribute(),
                "POST" => new HttpPostAttribute(),
                "PUT" => new HttpPutAttribute(),
                "DELETE" => new HttpDeleteAttribute(),
                "PATCH" => new HttpPatchAttribute(),
                "HEAD" => new HttpHeadAttribute(),
                _ => throw new NotSupportedException($"{verb}")
            };

            selectorModel.EndpointMetadata.Add(httpMethodAttribute);
        }

        /// <summary>
        /// 处理类类型参数（添加[FromBody] 特性）
        /// </summary>
        /// <param name="action"></param>
        private void ConfigureClassTypeParameter(ActionModel action)
        {
            // 没有参数无需处理
            if (action.Parameters.Count == 0) return;

            // 如果动作方法请求谓词只有GET和HEAD，则将类转查询参数
            if (_dynamicApiControllerSettings.ModelToQuery.Value)
            {
                var httpMethods = action.Selectors
                    .SelectMany(u => u.ActionConstraints.Where(u => u is HttpMethodActionConstraint)
                        .SelectMany(u => (u as HttpMethodActionConstraint).HttpMethods));

                if (httpMethods.All(u => u.Equals("GET") || u.Equals("HEAD"))) return;
            }

            var parameters = action.Parameters;
            foreach (var parameterModel in parameters)
            {
                // 如果参数已有绑定特性，则跳过
                if (parameterModel.BindingInfo != null) continue;

                var parameterType = parameterModel.ParameterType;
                // 如果是基元类型，则跳过
                if (parameterType.IsRichPrimitive()) continue;

                // 如果是文件类型，则跳过
                if (typeof(IFormFile).IsAssignableFrom(parameterType) || typeof(IFormFileCollection).IsAssignableFrom(parameterType)) continue;

                parameterModel.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromBodyAttribute() });
            }
        }

        /// <summary>
        /// 配置动作方法路由特性
        /// </summary>
        /// <param name="action">动作方法模型</param>
        /// <param name="apiDescriptionSettings">接口描述配置</param>
        /// <param name="controllerApiDescriptionSettings">控制器接口描述配置</param>
        /// <param name="isLowercaseRoute"></param>
        /// <param name="isKeepName"></param>
        /// <param name="hasApiControllerAttribute"></param>
        private void ConfigureActionRouteAttribute(ActionModel action, ApiDescriptionSettingsAttribute apiDescriptionSettings, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings, bool isLowercaseRoute, bool isKeepName, bool hasApiControllerAttribute)
        {
            var selectorModel = action.Selectors[0];
            // 跳过已配置路由特性的配置
            if (selectorModel.AttributeRouteModel != null) return;

            // 读取模块
            var module = apiDescriptionSettings?.Module;

            string template;
            string controllerRouteTemplate = null;
            // 如果动作方法名称为空、参数值为空，且无需保留谓词，则只生成控制器路由模板
            if (action.ActionName.Length == 0 && !isKeepName && action.Parameters.Count == 0)
            {
                template = GenerateControllerRouteTemplate(action.Controller, controllerApiDescriptionSettings);
            }
            else
            {
                // 生成参数路由模板
                var parameterRouteTemplate = GenerateParameterRouteTemplates(action, isLowercaseRoute, hasApiControllerAttribute);

                // 生成控制器模板
                controllerRouteTemplate = GenerateControllerRouteTemplate(action.Controller, controllerApiDescriptionSettings, parameterRouteTemplate);

                // 拼接动作方法路由模板
                var ActionStartTemplate = parameterRouteTemplate != null ? (parameterRouteTemplate.ActionStartTemplates.Count == 0 ? null : string.Join("/", parameterRouteTemplate.ActionStartTemplates)) : null;
                var ActionEndTemplate = parameterRouteTemplate != null ? (parameterRouteTemplate.ActionEndTemplates.Count == 0 ? null : string.Join("/", parameterRouteTemplate.ActionEndTemplates)) : null;

                // 判断是否定义了控制器路由，如果定义，则不拼接控制器路由
                template = string.IsNullOrWhiteSpace(controllerRouteTemplate)
                     ? $"{(string.IsNullOrWhiteSpace(module) ? "/" : $"{module}/")}{ActionStartTemplate}/{(string.IsNullOrWhiteSpace(action.ActionName) ? null : "[action]")}/{ActionEndTemplate}"
                     : $"{controllerRouteTemplate}/{(string.IsNullOrWhiteSpace(module) ? null : $"{module}/")}{ActionStartTemplate}/{(string.IsNullOrWhiteSpace(action.ActionName) ? null : "[action]")}/{ActionEndTemplate}";
            }

            AttributeRouteModel actionAttributeRouteModel = null;
            if (!string.IsNullOrWhiteSpace(template))
            {
                // 处理多个斜杆问题
                template = Regex.Replace(isLowercaseRoute ? template.ToLower() : template, @"\/{2,}", "/");

                // 生成路由
                actionAttributeRouteModel = string.IsNullOrWhiteSpace(template) ? null : new AttributeRouteModel(new RouteAttribute(template));
            }

            // 拼接路由
            selectorModel.AttributeRouteModel = string.IsNullOrWhiteSpace(controllerRouteTemplate)
                ? (actionAttributeRouteModel == null ? null : AttributeRouteModel.CombineAttributeRouteModel(action.Controller.Selectors[0].AttributeRouteModel, actionAttributeRouteModel))
                : actionAttributeRouteModel;
        }

        /// <summary>
        /// 生成控制器路由模板
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="apiDescriptionSettings"></param>
        /// <param name="parameterRouteTemplate">参数路由模板</param>
        /// <returns></returns>
        private string GenerateControllerRouteTemplate(ControllerModel controller, ApiDescriptionSettingsAttribute apiDescriptionSettings, ParameterRouteTemplate parameterRouteTemplate = default)
        {
            var selectorModel = controller.Selectors[0];
            // 跳过已配置路由特性的配置
            if (selectorModel.AttributeRouteModel != null) return default;

            // 读取模块
            var module = apiDescriptionSettings?.Module ?? _dynamicApiControllerSettings.DefaultModule;

            // 路由默认前缀
            var routePrefix = _dynamicApiControllerSettings.DefaultRoutePrefix;

            // 生成路由模板
            // 如果参数路由模板为空或不包含任何控制器参数模板，则返回正常的模板
            if (parameterRouteTemplate == null || (parameterRouteTemplate.ControllerStartTemplates.Count == 0 && parameterRouteTemplate.ControllerEndTemplates.Count == 0))
                return $"{(string.IsNullOrWhiteSpace(routePrefix) ? null : $"{routePrefix}/")}{(string.IsNullOrWhiteSpace(module) ? null : $"{module}/")}[controller]";

            // 拼接控制器路由模板
            var controllerStartTemplate = parameterRouteTemplate.ControllerStartTemplates.Count == 0 ? null : string.Join("/", parameterRouteTemplate.ControllerStartTemplates);
            var controllerEndTemplate = parameterRouteTemplate.ControllerEndTemplates.Count == 0 ? null : string.Join("/", parameterRouteTemplate.ControllerEndTemplates);
            var template = $"{(string.IsNullOrWhiteSpace(routePrefix) ? null : $"{routePrefix}/")}{(string.IsNullOrWhiteSpace(module) ? null : $"{module}/")}{controllerStartTemplate}/[controller]/{controllerEndTemplate}";

            return template;
        }

        /// <summary>
        /// 生成参数路由模板（非引用类型）
        /// </summary>
        /// <param name="action">动作方法模型</param>
        /// <param name="isLowercaseRoute"></param>
        /// <param name="hasApiControllerAttribute"></param>
        private ParameterRouteTemplate GenerateParameterRouteTemplates(ActionModel action, bool isLowercaseRoute, bool hasApiControllerAttribute)
        {
            // 如果没有参数，则跳过
            if (action.Parameters.Count == 0) return default;

            var parameterRouteTemplate = new ParameterRouteTemplate();
            var parameters = action.Parameters;

            // 判断是否贴有 [QueryParameters] 特性
            var isQueryParametersAction = action.Attributes.Any(u => u is QueryParametersAttribute);

            // 遍历所有参数
            foreach (var parameterModel in parameters)
            {
                var parameterType = parameterModel.ParameterType;
                var parameterAttributes = parameterModel.Attributes;

                // 处理小写参数路由匹配问题
                if (isLowercaseRoute) parameterModel.ParameterName = parameterModel.ParameterName.ToLower();

                // 判断是否贴有任何 [FromXXX] 特性了
                var hasFormAttribute = parameterAttributes.Any(u => typeof(IBindingSourceMetadata).IsAssignableFrom(u.GetType()));

                // 判断方法贴有 [QueryParameters] 特性且当前参数没有任何 [FromXXX] 特性，则添加 [FromQuery] 特性
                if (isQueryParametersAction && !hasFormAttribute)
                {
                    parameterModel.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromQueryAttribute() });
                    continue;
                }

                // 如果没有贴 [FromRoute] 特性且不是基元类型，则跳过
                // 如果没有贴 [FromRoute] 特性且有任何绑定特性，则跳过
                if (!parameterAttributes.Any(u => u is FromRouteAttribute)
                    && (!parameterType.IsRichPrimitive() || hasFormAttribute)) continue;

                // 处理基元数组数组类型，还有全局配置参数问题
                if (_dynamicApiControllerSettings?.UrlParameterization == true || parameterType.IsArray)
                {
                    parameterModel.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromQueryAttribute() });
                    continue;
                }

                // 处理 [ApiController] 特性情况
                // https://docs.microsoft.com/en-US/aspnet/core/web-api/?view=aspnetcore-5.0#binding-source-parameter-inference
                if (!hasFormAttribute && hasApiControllerAttribute) continue;

                // 判断是否可以为null
                var canBeNull = parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Nullable<>);

                // 判断是否贴有路由约束特性
                string constraint = default;
                if (parameterAttributes.FirstOrDefault(u => u is RouteConstraintAttribute) is RouteConstraintAttribute routeConstraint && !string.IsNullOrWhiteSpace(routeConstraint.Constraint))
                {
                    constraint = !routeConstraint.Constraint.StartsWith(":")
                        ? $":{routeConstraint.Constraint}" : routeConstraint.Constraint;
                }

                var template = $"{{{parameterModel.ParameterName}{(canBeNull ? "?" : string.Empty)}{constraint}}}";
                // 如果没有贴路由位置特性，则默认添加到动作方法后面
                if (parameterAttributes.FirstOrDefault(u => u is ApiSeatAttribute) is not ApiSeatAttribute apiSeat)
                {
                    parameterRouteTemplate.ActionEndTemplates.Add(template);
                    continue;
                }

                // 生成路由参数位置
                switch (apiSeat.Seat)
                {
                    // 控制器名之前
                    case ApiSeats.ControllerStart:
                        parameterRouteTemplate.ControllerStartTemplates.Add(template);
                        break;
                    // 控制器名之后
                    case ApiSeats.ControllerEnd:
                        parameterRouteTemplate.ControllerEndTemplates.Add(template);
                        break;
                    // 动作方法名之前
                    case ApiSeats.ActionStart:
                        parameterRouteTemplate.ActionStartTemplates.Add(template);
                        break;
                    // 动作方法名之后
                    case ApiSeats.ActionEnd:
                        parameterRouteTemplate.ActionEndTemplates.Add(template);
                        break;

                    default: break;
                }
            }

            return parameterRouteTemplate;
        }

        /// <summary>
        /// 配置控制器和动作方法名称
        /// </summary>
        /// <param name="apiDescriptionSettings"></param>
        /// <param name="orignalName"></param>
        /// <param name="affixes"></param>
        /// <param name="configure"></param>
        /// <param name="controllerApiDescriptionSettings"></param>
        /// <returns></returns>
        private (string Name, bool IsLowercaseRoute, bool IsKeepName) ConfigureControllerAndActionName(ApiDescriptionSettingsAttribute apiDescriptionSettings, string orignalName, string[] affixes, Func<string, string> configure, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings = default)
        {
            // 获取版本号
            var apiVersion = apiDescriptionSettings?.Version;
            var isKeepName = false;

            // 解析控制器名称
            // 判断是否有自定义名称
            var tempName = apiDescriptionSettings?.Name;
            if (string.IsNullOrWhiteSpace(tempName))
            {
                // 处理版本号
                var (name, version) = ResolveNameVersion(orignalName);
                tempName = name;
                apiVersion ??= version;

                // 清除指定前后缀
                tempName = tempName.ClearStringAffixes(affixes: affixes);

                isKeepName = CheckIsKeepName(controllerApiDescriptionSettings == null ? null : apiDescriptionSettings, controllerApiDescriptionSettings ?? apiDescriptionSettings);

                // 判断是否保留原有名称
                if (!isKeepName)
                {
                    // 自定义配置
                    tempName = configure.Invoke(tempName);

                    // 处理骆驼命名
                    if (CheckIsSplitCamelCase(controllerApiDescriptionSettings == null ? null : apiDescriptionSettings, controllerApiDescriptionSettings ?? apiDescriptionSettings))
                    {
                        tempName = string.Join(_dynamicApiControllerSettings.CamelCaseSeparator, tempName.SplitCamelCase());
                    }
                }
            }

            // 拼接名称和版本号
            var newName = $"{tempName}{(string.IsNullOrWhiteSpace(apiVersion) ? null : $"{_dynamicApiControllerSettings.VersionSeparator}{apiVersion}")}";

            var isLowercaseRoute = CheckIsLowercaseRoute(controllerApiDescriptionSettings == null ? null : apiDescriptionSettings, controllerApiDescriptionSettings ?? apiDescriptionSettings);
            return (isLowercaseRoute ? newName.ToLower() : newName, isLowercaseRoute, isKeepName);
        }

        /// <summary>
        /// 检查是否设置了 KeepName参数
        /// </summary>
        /// <param name="apiDescriptionSettings"></param>
        /// <param name="controllerApiDescriptionSettings"></param>
        /// <returns></returns>
        private bool CheckIsKeepName(ApiDescriptionSettingsAttribute apiDescriptionSettings, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings)
        {
            bool isKeepName;

            // 判断 Action 是否配置了 KeepName 属性
            if (apiDescriptionSettings?.KeepName != null)
            {
                var canParse = bool.TryParse(apiDescriptionSettings.KeepName.ToString(), out var value);
                isKeepName = canParse && value;
            }
            // 判断 Controller 是否配置了 KeepName 属性
            else if (controllerApiDescriptionSettings?.KeepName != null)
            {
                var canParse = bool.TryParse(controllerApiDescriptionSettings.KeepName.ToString(), out var value);
                isKeepName = canParse && value;
            }
            // 取全局配置
            else isKeepName = _dynamicApiControllerSettings?.KeepName == true;

            return isKeepName;
        }

        /// <summary>
        /// 检查是否设置了 KeepVerb 参数
        /// </summary>
        /// <param name="apiDescriptionSettings"></param>
        /// <param name="controllerApiDescriptionSettings"></param>
        /// <returns></returns>
        private bool CheckIsKeepVerb(ApiDescriptionSettingsAttribute apiDescriptionSettings, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings)
        {
            bool isKeepVerb;

            // 判断 Action 是否配置了 KeepVerb 属性
            if (apiDescriptionSettings?.KeepVerb != null)
            {
                var canParse = bool.TryParse(apiDescriptionSettings.KeepVerb.ToString(), out var value);
                isKeepVerb = canParse && value;
            }
            // 判断 Controller 是否配置了 KeepVerb 属性
            else if (controllerApiDescriptionSettings?.KeepVerb != null)
            {
                var canParse = bool.TryParse(controllerApiDescriptionSettings.KeepVerb.ToString(), out var value);
                isKeepVerb = canParse && value;
            }
            // 取全局配置
            else isKeepVerb = _dynamicApiControllerSettings?.KeepVerb == true;

            return isKeepVerb;
        }

        /// <summary>
        /// 判断切割命名参数是否配置
        /// </summary>
        /// <param name="apiDescriptionSettings"></param>
        /// <param name="controllerApiDescriptionSettings"></param>
        /// <returns></returns>
        private static bool CheckIsSplitCamelCase(ApiDescriptionSettingsAttribute apiDescriptionSettings, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings)
        {
            bool isSplitCamelCase;

            // 判断 Action 是否配置了 SplitCamelCase 属性
            if (apiDescriptionSettings?.SplitCamelCase != null)
            {
                var canParse = bool.TryParse(apiDescriptionSettings.SplitCamelCase.ToString(), out var value);
                isSplitCamelCase = !canParse || value;
            }
            // 判断 Controller 是否配置了 SplitCamelCase 属性
            else if (controllerApiDescriptionSettings?.SplitCamelCase != null)
            {
                var canParse = bool.TryParse(controllerApiDescriptionSettings.SplitCamelCase.ToString(), out var value);
                isSplitCamelCase = !canParse || value;
            }
            // 取全局配置
            else isSplitCamelCase = true;

            return isSplitCamelCase;
        }

        /// <summary>
        /// 检查是否启用小写路由
        /// </summary>
        /// <param name="apiDescriptionSettings"></param>
        /// <param name="controllerApiDescriptionSettings"></param>
        /// <returns></returns>
        private bool CheckIsLowercaseRoute(ApiDescriptionSettingsAttribute apiDescriptionSettings, ApiDescriptionSettingsAttribute controllerApiDescriptionSettings)
        {
            bool isLowercaseRoute;

            // 判断 Action 是否配置了 LowercaseRoute 属性
            if (apiDescriptionSettings?.LowercaseRoute != null)
            {
                var canParse = bool.TryParse(apiDescriptionSettings.LowercaseRoute.ToString(), out var value);
                isLowercaseRoute = !canParse || value;
            }
            // 判断 Controller 是否配置了 LowercaseRoute 属性
            else if (controllerApiDescriptionSettings?.LowercaseRoute != null)
            {
                var canParse = bool.TryParse(controllerApiDescriptionSettings.LowercaseRoute.ToString(), out var value);
                isLowercaseRoute = !canParse || value;
            }
            // 取全局配置
            else isLowercaseRoute = (_dynamicApiControllerSettings?.LowercaseRoute) != false;

            return isLowercaseRoute;
        }

        /// <summary>
        /// 配置规范化结果类型
        /// </summary>
        /// <param name="action"></param>
        private static void ConfigureActionUnifyResultAttribute(ActionModel action)
        {
            // 判断是否手动添加了标注或跳过规范化处理
            if (UnifyContext.CheckSucceededNonUnify(action.ActionMethod, out var _, false)) return;

            // 获取真实类型
            var returnType = action.ActionMethod.GetRealReturnType();
            if (returnType == typeof(void)) return;

            // 添加规范化结果特性
            action.Filters.Add(new UnifyResultAttribute(returnType, StatusCodes.Status200OK));
        }

        /// <summary>
        /// 解析名称中的版本号
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>名称和版本号</returns>
        private (string name, string version) ResolveNameVersion(string name)
        {
            if (!_nameVersionRegex.IsMatch(name)) return (name, default);

            var version = _nameVersionRegex.Match(name).Groups["version"].Value.Replace("_", ".");
            return (_nameVersionRegex.Replace(name, ""), version);
        }

        /// <summary>
        /// 获取方法名映射 [HttpMethod] 规则
        /// </summary>
        /// <returns></returns>
        private void LoadVerbToHttpMethodsConfigure()
        {
            var defaultVerbToHttpMethods = Penetrates.VerbToHttpMethods;

            // 获取配置的复写映射规则
            var verbToHttpMethods = _dynamicApiControllerSettings.VerbToHttpMethods;

            if (verbToHttpMethods is not null)
            {
                // 获取所有参数大于1的配置
                var settingsVerbToHttpMethods = verbToHttpMethods
                    .Where(u => u.Length > 1)
                    .ToDictionary(u => u[0].ToString().ToLower(), u => u[1]?.ToString());

                defaultVerbToHttpMethods.AddOrUpdate(settingsVerbToHttpMethods);
            }
        }
    }
}