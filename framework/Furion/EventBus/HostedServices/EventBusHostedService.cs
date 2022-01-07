// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Furion.EventBus
{
    /// <summary>
    /// 事件总线后台主机服务
    /// </summary>
    internal sealed class EventBusHostedService : BackgroundService
    {
        /// <summary>
        /// 避免由 CLR 的终结器捕获该异常从而终止应用程序，让所有未觉察异常被觉察
        /// </summary>
        internal event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        /// <summary>
        /// 日志对象
        /// </summary>
        private readonly ILogger<EventBusHostedService> _logger;

        /// <summary>
        /// 事件源存储器
        /// </summary>
        private readonly IEventSourceStorer _eventSourceStorer;

        /// <summary>
        /// 事件处理程序集合
        /// </summary>
        private readonly HashSet<EventHandlerWrapper> _eventHandlers = new();

        /// <summary>
        /// 事件处理程序监视器
        /// </summary>
        private IEventHandlerMonitor Monitor { get; }

        /// <summary>
        /// 事件处理程序执行器
        /// </summary>
        private IEventHandlerExecutor Executor { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志对象</param>
        /// <param name="serviceProvider">服务提供器</param>
        /// <param name="eventSourceStorer">事件源存储器</param>
        /// <param name="eventSubscribers">事件订阅者集合</param>
        public EventBusHostedService(ILogger<EventBusHostedService> logger
            , IServiceProvider serviceProvider
            , IEventSourceStorer eventSourceStorer
            , IEnumerable<IEventSubscriber> eventSubscribers)
        {
            _logger = logger;
            _eventSourceStorer = eventSourceStorer;
            Monitor = serviceProvider.GetService<IEventHandlerMonitor>();
            Executor = serviceProvider.GetService<IEventHandlerExecutor>();

            var bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            // 逐条获取事件处理程序并进行包装
            foreach (var eventSubscriber in eventSubscribers)
            {
                // 获取事件订阅者类型
                var eventSubscriberType = eventSubscriber.GetType();

                // 查找所有公开且贴有 [EventSubscribe] 的实例方法
                var eventHandlerMethods = eventSubscriberType.GetMethods(bindingAttr)
                    .Where(u => u.IsDefined(typeof(EventSubscribeAttribute), false));

                // 遍历所有事件订阅者处理方法
                foreach (var eventHandlerMethod in eventHandlerMethods)
                {
                    // 将方法转换成 Func<EventHandlerExecutingContext, Task> 委托
                    var handler = eventHandlerMethod.CreateDelegate<Func<EventHandlerExecutingContext, Task>>(eventSubscriber);

                    // 处理同一个事件处理程序支持多个事件 Id 情况
                    var eventSubscribeAttributes = eventHandlerMethod.GetCustomAttributes<EventSubscribeAttribute>(false);

                    // 逐条包装并添加到 HashSet 集合中
                    foreach (var eventSubscribeAttribute in eventSubscribeAttributes)
                    {
                        _eventHandlers.Add(new EventHandlerWrapper(eventSubscribeAttribute.EventId)
                        {
                            Handler = handler
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 执行后台任务
        /// </summary>
        /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
        /// <returns><see cref="Task"/> 实例</returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventBus Hosted Service is running.");

            // 注册后台主机服务停止监听
            stoppingToken.Register(() =>
                _logger.LogDebug($"EventBus Hosted Service is stopping."));

            // 监听服务是否取消
            while (!stoppingToken.IsCancellationRequested)
            {
                // 执行具体任务
                await BackgroundProcessing(stoppingToken);
            }

            _logger.LogCritical($"EventBus Hosted Service is stopped.");
        }

        /// <summary>
        /// 后台调用事件处理程序
        /// </summary>
        /// <param name="stoppingToken">后台主机服务停止时取消任务 Token</param>
        /// <returns><see cref="Task"/> 实例</returns>
        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            // 从事件存储器中读取一条
            var eventSource = await _eventSourceStorer.ReadAsync(stoppingToken);

            // 空检查
            if (string.IsNullOrWhiteSpace(eventSource?.EventId))
            {
                _logger.LogWarning("Invalid EventId, EventId cannot be <null> or an empty string.");

                return;
            }

            // 查找事件 Id 匹配的事件处理程序
            var eventHandlersThatShouldRun = _eventHandlers.Where(t => t.ShouldRun(eventSource.EventId));

            // 空订阅
            if (!eventHandlersThatShouldRun.Any())
            {
                _logger.LogWarning("Subscriber with event ID <{EventId}> was not found.", eventSource.EventId);

                return;
            }

            // 创建一个任务工厂并保证执行任务都使用当前的计划程序
            var taskFactory = new TaskFactory(System.Threading.Tasks.TaskScheduler.Current);

            // 逐条创建新线程调用
            foreach (var eventHandlerThatShouldRun in eventHandlersThatShouldRun)
            {
                // 创建新的线程执行
                await taskFactory.StartNew(async () =>
                {
                    // 创建共享上下文数据对象
                    var properties = new Dictionary<object, object>();

                    // 创建执行前上下文
                    var eventHandlerExecutingContext = new EventHandlerExecutingContext(eventSource, properties)
                    {
                        ExecutingTime = DateTime.UtcNow
                    };

                    // 执行异常对象
                    InvalidOperationException executionException = default;

                    try
                    {
                        // 处理任务取消
                        if (eventSource.CancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        // 调用执行前监视器
                        if (Monitor != default)
                        {
                            await Monitor.OnExecutingAsync(eventHandlerExecutingContext);
                        }

                        // 判断是否自定义了执行器
                        if (Executor == default)
                        {
                            await eventHandlerThatShouldRun.Handler!(eventHandlerExecutingContext);
                        }
                        else
                        {
                            await Executor.ExecuteAsync(eventHandlerExecutingContext, eventHandlerThatShouldRun.Handler!);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 输出异常日志
                        _logger.LogError(ex, "Error occurred executing {EventId}.", eventSource.EventId);

                        // 标记异常
                        executionException = new InvalidOperationException(string.Format("Error occurred executing {0}.", eventSource.EventId), ex);

                        // 捕获 Task 任务异常信息并统计所有异常
                        if (UnobservedTaskException != default)
                        {
                            var args = new UnobservedTaskExceptionEventArgs(
                                ex as AggregateException ?? new AggregateException(ex));

                            UnobservedTaskException.Invoke(this, args);
                        }
                    }
                    finally
                    {
                        // 调用执行后监视器
                        if (Monitor != default)
                        {
                            // 创建执行后上下文
                            var eventHandlerExecutedContext = new EventHandlerExecutedContext(eventSource, properties)
                            {
                                ExecutedTime = DateTime.UtcNow,
                                Exception = executionException
                            };

                            await Monitor.OnExecutedAsync(eventHandlerExecutedContext);
                        }
                    }
                }, stoppingToken);
            }
        }
    }
}