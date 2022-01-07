// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Furion.EventBus
{
    /// <summary>
    /// 事件总线配置选项构建器
    /// </summary>
    public sealed class EventBusOptionsBuilder
    {
        /// <summary>
        /// 事件订阅者类型集合
        /// </summary>
        private readonly List<Type> _eventSubscribers = new();

        /// <summary>
        /// 事件发布者类型
        /// </summary>
        private Type _eventPublisher;

        /// <summary>
        /// 事件存储器实现工厂
        /// </summary>
        private Func<IServiceProvider, IEventSourceStorer> _eventSourceStorerImplementationFactory;

        /// <summary>
        /// 事件处理程序监视器
        /// </summary>
        private Type _eventHandlerMonitor;

        /// <summary>
        /// 事件处理程序执行器
        /// </summary>
        private Type _eventHandlerExecutor;

        /// <summary>
        /// 默认内置事件源存储器内存通道容量
        /// </summary>
        /// <remarks>超过 n 条待处理消息，第 n+1 条将进入等待，默认为 3000</remarks>
        public int ChannelCapacity { get; set; } = 3000;

        /// <summary>
        /// 未察觉任务异常事件处理程序
        /// </summary>
        public EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskExceptionHandler { get; set; }

        /// <summary>
        /// 注册事件订阅者
        /// </summary>
        /// <typeparam name="TEventSubscriber">实现自 <see cref="IEventSubscriber"/></typeparam>
        /// <returns><see cref="EventBusOptionsBuilder"/> 实例</returns>
        public EventBusOptionsBuilder AddSubscriber<TEventSubscriber>()
            where TEventSubscriber : class, IEventSubscriber
        {
            _eventSubscribers.Add(typeof(TEventSubscriber));
            return this;
        }

        /// <summary>
        /// 批量注册事件订阅者
        /// </summary>
        /// <param name="assemblies">程序集</param>
        /// <returns><see cref="EventBusOptionsBuilder"/> 实例</returns>
        public EventBusOptionsBuilder AddSubscribers(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                throw new InvalidOperationException("The assemblies can be not null or empty.");
            }

            // 获取所有导出类型（非接口，非抽象类且实现 IEventSubscriber）接口
            var subscribers = assemblies.SelectMany(ass =>
                  ass.GetExportedTypes()
                     .Where(t => t.IsPublic && t.IsClass && !t.IsInterface && !t.IsAbstract && typeof(IEventSubscriber).IsAssignableFrom(t)));

            foreach (var subscriber in subscribers)
            {
                _eventSubscribers.Add(subscriber);
            }

            return this;
        }

        /// <summary>
        /// 替换事件发布者
        /// </summary>
        /// <typeparam name="TEventPublisher">实现自 <see cref="IEventPublisher"/></typeparam>
        /// <returns><see cref="EventBusOptionsBuilder"/> 实例</returns>
        public EventBusOptionsBuilder ReplacePublisher<TEventPublisher>()
            where TEventPublisher : class, IEventPublisher
        {
            _eventPublisher = typeof(TEventPublisher);
            return this;
        }

        /// <summary>
        /// 替换事件源存储器
        /// </summary>
        /// <param name="implementationFactory">自定义事件源存储器工厂</param>
        /// <returns><see cref="EventBusOptionsBuilder"/> 实例</returns>
        public EventBusOptionsBuilder ReplaceStorer(Func<IServiceProvider, IEventSourceStorer> implementationFactory)
        {
            _eventSourceStorerImplementationFactory = implementationFactory;
            return this;
        }

        /// <summary>
        /// 注册事件处理程序监视器
        /// </summary>
        /// <typeparam name="TEventHandlerMonitor">实现自 <see cref="IEventHandlerMonitor"/></typeparam>
        /// <returns><see cref="EventBusOptionsBuilder"/> 实例</returns>
        public EventBusOptionsBuilder AddMonitor<TEventHandlerMonitor>()
            where TEventHandlerMonitor : class, IEventHandlerMonitor
        {
            _eventHandlerMonitor = typeof(TEventHandlerMonitor);
            return this;
        }

        /// <summary>
        /// 注册事件处理程序执行器
        /// </summary>
        /// <typeparam name="TEventHandlerExecutor">实现自 <see cref="IEventHandlerExecutor"/></typeparam>
        /// <returns><see cref="EventBusOptionsBuilder"/> 实例</returns>
        public EventBusOptionsBuilder AddExecutor<TEventHandlerExecutor>()
            where TEventHandlerExecutor : class, IEventHandlerExecutor
        {
            _eventHandlerExecutor = typeof(TEventHandlerExecutor);
            return this;
        }

        /// <summary>
        /// 构建事件总线配置选项
        /// </summary>
        /// <param name="services">服务集合对象</param>
        internal void Build(IServiceCollection services)
        {
            // 注册事件订阅者
            foreach (var eventSubscriber in _eventSubscribers)
            {
                services.AddSingleton(typeof(IEventSubscriber), eventSubscriber);
            }

            // 替换事件发布者
            if (_eventPublisher != default)
            {
                services.Replace(ServiceDescriptor.Singleton(typeof(IEventPublisher), _eventPublisher));
            }

            // 替换事件存储器
            if (_eventSourceStorerImplementationFactory != default)
            {
                services.Replace(ServiceDescriptor.Singleton(_eventSourceStorerImplementationFactory));
            }

            // 注册事件监视器
            if (_eventHandlerMonitor != default)
            {
                services.AddSingleton(typeof(IEventHandlerMonitor), _eventHandlerMonitor);
            }

            // 注册事件执行器
            if (_eventHandlerExecutor != default)
            {
                services.AddSingleton(typeof(IEventHandlerExecutor), _eventHandlerExecutor);
            }
        }
    }
}