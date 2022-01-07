// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DatabaseAccessor;
using Furion.DependencyInjection;
using StackExchange.Profiling.Internal;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// MiniProfiler 服务拓展类
    /// </summary>
    [SuppressSniffer]
    public static class MiniProfilerServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 EF Core 进程监听拓展
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMiniProfilerBuilder AddRelationalDiagnosticListener(this IMiniProfilerBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IMiniProfilerDiagnosticListener, RelationalDiagnosticListener>();

            return builder;
        }
    }
}