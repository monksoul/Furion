using Furion.EventBus;
using Furion.Extensitions.EventBus;
using Microsoft.Extensions.Logging;

namespace Furion.Application;

public class TestEventBus : IDynamicApiController, IDisposable
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IEventBusFactory _eventBusFactory;
    public TestEventBus(IEventPublisher eventPublisher, IEventBusFactory eventBusFactory)
    {
        _eventPublisher = eventPublisher;
        _eventBusFactory = eventBusFactory;

        _eventPublisher.OnExecuted += Subscribe;
    }

    void Subscribe(object sender, EventHandlerEventArgs args)
    {
        Console.WriteLine($"事件 {args.Source.EventId} 执行状态：{args.Status}，异常：{args.Exception}, 执行结果：{args.Result}");
    }

    // 发布 ToDo:Create 消息
    public async Task CreateDoTo()
    {
        await _eventPublisher.PublishAsync("ToDo:Create", "Furion");
    }

    // 发布枚举消息
    public async Task CreateEnum()
    {
        await _eventPublisher.PublishAsync(ValidationTypes.Numeric);
    }

    // 发送电话
    public async Task SendPhone()
    {
        await _eventPublisher.PublishAsync("13800138000");
        await _eventPublisher.PublishAsync("13434563233");
    }

    // 发送错误
    public async Task SendError()
    {
        await _eventPublisher.PublishAsync("test:error");
    }

    public async Task AddSubscriber()
    {
        await _eventBusFactory.Subscribe("xxx", async (c) =>
        {
            Console.WriteLine("我是动态的");
            await Task.CompletedTask;
        });
    }

    public async Task SendDynamic(string eventId)
    {
        await _eventPublisher.PublishAsync(eventId);
    }

    public async Task RemoveDynamic(string eventId)
    {
        await _eventBusFactory.Unsubscribe(eventId);
    }

    // 发布 ToDo:Create 消息
    public void 测试高频事件()
    {
        Parallel.For(0, 10000, (i) =>
        {
            _eventPublisher.PublishAsync("ToDo:Create");
        });
    }

    public void Dispose()
    {
        _eventPublisher.OnExecuted -= Subscribe;
    }
}

// 实现 IEventSubscriber 接口
public class ToDoEventSubscriber : IEventSubscriber, ISingleton
{
    private readonly ILogger<ToDoEventSubscriber> _logger;
    public ToDoEventSubscriber(ILogger<ToDoEventSubscriber> logger)
    {
        _logger = logger;
    }

    [EventSubscribe("ToDo:Create")]
    public async Task CreateToDo(EventHandlerExecutingContext context)
    {
        var todo = context.Source;
        _logger.LogInformation("创建一个 ToDo：{Name}", context.GetPayload<string>());
        context.SetResult("这是结果");
        await Task.CompletedTask;
    }

    [EventSubscribe("ToDo:Create")]
    public async Task CreateToDo2(EventHandlerExecutingContext<string> context)
    {
        var todo = context.Source;
        _logger.LogInformation("创建一个 ToDo：{Name}，这是泛型的", context.Payload);
        context.SetResult("这是结果");
        await Task.CompletedTask;
    }

    [EventSubscribe("ToDo:Create")]
    public async Task CreateToDo(EventHandlerExecutingContext context, CancellationToken cancellationToken)
    {
        var todo = context.Source;
        _logger.LogInformation("创建一个 ToDo：{Name}，带 cancellationToken", context.GetPayload<string>());
        context.SetResult("这是结果");
        await Task.CompletedTask;
    }

    [EventSubscribe("ToDo:Create")]
    public async Task CreateToDo2(EventHandlerExecutingContext<string> context, CancellationToken cancellationToken)
    {
        var todo = context.Source;
        _logger.LogInformation("创建一个 ToDo：{Name}，这是泛型的，带 cancellationToken", context.Payload);
        context.SetResult("这是结果");
        await Task.CompletedTask;
    }

    [EventSubscribe(ValidationTypes.Numeric)]
    public async Task CreateEnum(EventHandlerExecutingContext context)
    {
        var eventEnum = context.Source.EventId.ParseToEnum();
        await Task.CompletedTask;
    }

    [EventSubscribe("(^1[3456789][0-9]{9}$)|((^[0-9]{3,4}\\-[0-9]{3,8}$)|(^[0-9]{3,8}$)|(^\\([0-9]{3,4}\\)[0-9]{3,8}$)|(^0{0,1}13[0-9]{9}$))", FuzzyMatch = true)]
    public async Task 测试电话正则表达式(EventHandlerExecutingContext context)
    {
        Console.WriteLine(context.Source.EventId);
        await Task.CompletedTask;
    }

    [EventSubscribe("test:error", NumRetries = 3, FallbackPolicy = typeof(EventFallbackPolicy))]  // 重试三次
    public async Task 测试异常重试(EventHandlerExecutingContext context)
    {
        Console.WriteLine("我执行啦~~");
        throw new NotImplementedException();
    }
}

public class EventFallbackPolicy : IEventFallbackPolicy
{
    private readonly ILogger<EventFallbackPolicy> _logger;
    public EventFallbackPolicy(ILogger<EventFallbackPolicy> logger)
    {
        _logger = logger;
    }

    public async Task CallbackAsync(EventHandlerExecutingContext context, Exception ex, CancellationToken cancellationToken)
    {
        _logger.LogError(ex, "重试了多次最终还是失败了");
        await Task.CompletedTask;
    }
}
