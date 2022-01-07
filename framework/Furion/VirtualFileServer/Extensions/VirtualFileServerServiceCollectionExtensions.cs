// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Furion.VirtualFileServer;
using Microsoft.Extensions.FileProviders;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 虚拟文件服务服务拓展
    /// </summary>
    [SuppressSniffer]
    public static class VirtualFileServerServiceCollectionExtensions
    {
        /// <summary>
        /// 文件提供器系统服务拓展
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddVirtualFileServer(this IServiceCollection services)
        {
            // 解析文件提供器
            services.AddSingleton(provider =>
            {
                static IFileProvider fileProviderResolve(FileProviderTypes fileProviderTypes, object args)
                {
                    // 根据类型创建对应 提供器
                    IFileProvider fileProvider = fileProviderTypes switch
                    {
                        FileProviderTypes.Embedded => new EmbeddedFileProvider(args as Assembly),
                        FileProviderTypes.Physical => new PhysicalFileProvider(args as string),
                        _ => throw new NotSupportedException()
                    };

                    return fileProvider;
                }

                // 转换成委托
                return (Func<FileProviderTypes, object, IFileProvider>)fileProviderResolve;
            });

            return services;
        }
    }
}