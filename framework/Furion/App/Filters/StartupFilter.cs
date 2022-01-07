// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Furion
{
    /// <summary>
    /// 应用启动时自动注册中间件
    /// </summary>
    /// <remarks>
    /// </remarks>
    [SuppressSniffer]
    public class StartupFilter : IStartupFilter
    {
        /// <summary>
        /// 配置中间件
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                // 存储根服务
                InternalApp.RootServices = app.ApplicationServices;

                // 环境名
                var envName = App.HostEnvironment?.EnvironmentName ?? "Unknown";

                // 设置响应报文头信息
                app.Use(async (context, next) =>
                {
                    // 输出当前环境标识
                    context.Response.Headers["environment"] = envName;

                    // 执行下一个中间件
                    await next.Invoke();

                    // 释放所有未托管的服务提供器
                    App.DisposeUnmanagedObjects();
                });

                // 调用默认中间件
                app.UseApp();

                // 配置所有 Starup Configure
                UseStartups(app);

                // 调用启动层的 Startup
                next(app);
            };
        }

        /// <summary>
        /// 配置 Startup 的 Configure
        /// </summary>
        /// <param name="app">应用构建器</param>
        private static void UseStartups(IApplicationBuilder app)
        {
            // 反转，处理排序
            var startups = App.AppStartups.Reverse();
            if (!startups.Any()) return;

            // 处理【部署】二级虚拟目录
            var virtualPath = App.Settings.VirtualPath;
            if (!string.IsNullOrWhiteSpace(virtualPath) && virtualPath.StartsWith("/"))
            {
                app.Map(virtualPath, _app => UseStartups(startups, _app));
                return;
            }

            UseStartups(startups, app);
        }

        private static void UseStartups(IEnumerable<AppStartup> startups, IApplicationBuilder app)
        {
            // 遍历所有
            foreach (var startup in startups)
            {
                var type = startup.GetType();

                // 获取所有符合依赖注入格式的方法，如返回值void，且第一个参数是 IApplicationBuilder 类型
                var configureMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(u => u.ReturnType == typeof(void)
                        && u.GetParameters().Length > 0
                        && u.GetParameters().First().ParameterType == typeof(IApplicationBuilder));

                if (!configureMethods.Any()) continue;

                // 自动安装属性调用
                foreach (var method in configureMethods)
                {
                    method.Invoke(startup, ResolveMethodParameterInstances(app, method));
                }
            }

            // 释放内存
            App.AppStartups.Clear();
        }

        /// <summary>
        /// 解析方法参数实例
        /// </summary>
        /// <param name="app"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static object[] ResolveMethodParameterInstances(IApplicationBuilder app, MethodInfo method)
        {
            // 获取方法所有参数
            var parameters = method.GetParameters();
            var parameterInstances = new object[parameters.Length];
            parameterInstances[0] = app;

            // 解析服务
            for (var i = 1; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                parameterInstances[i] = app.ApplicationServices.GetRequiredService(parameter.ParameterType);
            }

            return parameterInstances;
        }
    }
}