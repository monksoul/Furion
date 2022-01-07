// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Reflection;

namespace Furion.VirtualFileServer
{
    /// <summary>
    /// 虚拟文件服务静态类
    /// </summary>
    [SuppressSniffer]
    public static class FS
    {
        /// <summary>
        /// 获取物理文件提供器
        /// </summary>
        /// <param name="root"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IFileProvider GetPhysicalFileProvider(string root, IServiceProvider serviceProvider = default)
        {
            return GetFileProvider(FileProviderTypes.Physical, root, serviceProvider);
        }

        /// <summary>
        /// 获取嵌入资源文件提供器
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IFileProvider GetEmbeddedFileProvider(Assembly assembly, IServiceProvider serviceProvider = default)
        {
            return GetFileProvider(FileProviderTypes.Embedded, assembly, serviceProvider);
        }

        /// <summary>
        /// 文件提供器
        /// </summary>
        /// <param name="fileProviderTypes"></param>
        /// <param name="args"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IFileProvider GetFileProvider(FileProviderTypes fileProviderTypes, object args, IServiceProvider serviceProvider = default)
        {
            var fileProviderResolve = App.GetService<Func<FileProviderTypes, object, IFileProvider>>(serviceProvider ?? App.RootServices);
            return fileProviderResolve(fileProviderTypes, args);
        }
    }
}