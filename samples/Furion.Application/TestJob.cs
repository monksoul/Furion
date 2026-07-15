using Furion.Schedule;
using Microsoft.Extensions.Logging;

namespace Furion.Application;

//[Period(10000)]
[At("2024-12-31 23:59:59")]
public class TestJob : IJob, IDisposable
{
    private readonly ILogger<TestJob> _logger;
    public TestJob(ILogger<TestJob> logger)
    {
        _logger = logger;
    }

    public void Dispose()
    {
        Console.WriteLine("释放了");
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        // 获取手动传递的自定义数据
        if (context.Mode == 1 && context.Items.Count > 0)
        {
            Console.WriteLine($"手动触发作业，传递的自定义数据：{string.Join(", ", context.Items.Select(kv => $"{kv.Key}: {kv.Value}"))}");
        }

        _logger.LogWarning($"{context}");
        await Task.CompletedTask;
    }
}