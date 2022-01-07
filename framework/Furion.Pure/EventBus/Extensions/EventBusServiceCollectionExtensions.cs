// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.EventBus;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// EventBus 模块服务拓展
    /// </summary>
    public static class EventBusServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 EventBus 模块注册
        /// </summary>
        /// <param name="services">服务集合对象</param>
        /// <param name="configureOptionsBuilder">事件总线配置选项构建器委托</param>
        /// <returns>服务集合实例</returns>
        public static IServiceCollection AddEventBus(this IServiceCollection services, Action<EventBusOptionsBuilder> configureOptionsBuilder)
        {
            // 创建初始事件总线配置选项构建器
            var eventBusOptionsBuilder = new EventBusOptionsBuilder();
            configureOptionsBuilder.Invoke(eventBusOptionsBuilder);

            return services.AddEventBus(eventBusOptionsBuilder);
        }

        /// <summary>
        /// 添加 EventBus 模块注册
        /// </summary>
        /// <param name="services">服务集合对象</param>
        /// <param name="eventBusOptionsBuilder">事件总线配置选项构建器</param>
        /// <returns>服务集合实例</returns>
        public static IServiceCollection AddEventBus(this IServiceCollection services, EventBusOptionsBuilder eventBusOptionsBuilder = default)
        {
            // 初始化事件总线配置项
            eventBusOptionsBuilder ??= new EventBusOptionsBuilder();

            // 注册内部服务
            services.AddInternalService(eventBusOptionsBuilder);

            // 通过工厂模式创建
            services.AddHostedService(serviceProvider =>
            {
                // 创建事件总线后台服务对象
                var eventBusHostedService = ActivatorUtilities.CreateInstance<EventBusHostedService>(serviceProvider);

                // 订阅未察觉任务异常事件
                var unobservedTaskExceptionHandler = eventBusOptionsBuilder.UnobservedTaskExceptionHandler;
                if (unobservedTaskExceptionHandler != default)
                {
                    eventBusHostedService.UnobservedTaskException += unobservedTaskExceptionHandler;
                }

                return eventBusHostedService;
            });

            // 构建事件总线服务
            eventBusOptionsBuilder.Build(services);

            return services;
        }

        /// <summary>
        /// 注册内部服务
        /// </summary>
        /// <param name="services">服务集合对象</param>
        /// <param name="eventBusOptions">事件总线配置选项</param>
        /// <returns>服务集合实例</returns>
        private static IServiceCollection AddInternalService(this IServiceCollection services, EventBusOptionsBuilder eventBusOptions)
        {
            // 注册后台任务队列接口/实例为单例，采用工厂方式创建
            services.AddSingleton<IEventSourceStorer>(_ =>
            {
                // 创建默认内存通道事件源对象
                return new ChannelEventSourceStorer(eventBusOptions.ChannelCapacity);
            });

            // 注册默认内存通道事件发布者
            services.AddSingleton<IEventPublisher, ChannelEventPublisher>();

            return services;
        }
    }
}