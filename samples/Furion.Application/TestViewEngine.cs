using Furion.ViewEngine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Furion.Application;

/// <summary>
/// 测试视图引擎
/// </summary>
/// <param name="viewEngine"></param>
/// <param name="serviceProvider"></param>
/// <param name="configuration"></param>
public class TestViewEngine(IViewEngine viewEngine, IServiceProvider serviceProvider, IConfiguration configuration) : IDynamicApiController
{
    public async Task<string> Case1()
    {
        var result = await viewEngine.RunCompileAsync("Hello @Model.Name", new { Name = "Furion" });

        return result;
    }

    public async Task<string> Case2()
    {
        var result = await viewEngine.RunCompileAsync(@"
Hello @Model.Name
@foreach(var item in Model.Items)
{
    <p>@item</p>
}
", new TestModel
        {
            Name = "Furion",
            Items = [3, 1, 2]
        });

        return result;
    }

    public async Task<string> Case3()
    {
        var result = await viewEngine.RunCompileFromCachedAsync(@"
Hello @Model.Name
@foreach(var item in Model.Items)
{
    <p>@item</p>
}
", new TestModel
        {
            Name = "Furion",
            Items = [3, 1, 2]
        });

        return result;
    }

    public async Task<string> Case4()
    {
        var result = await viewEngine.RunCompileFromCachedAsync(@"
Hello @Model.Name
@foreach(var item in Model.Items)
{
    <p>@item</p>
}
", new TestModel
        {
            Name = "Furion",
            Items = [5, 6, 7, 8]
        });

        return result;
    }

    public async Task<string> Case5()
    {
        var result = await viewEngine.RunCompileFromCachedAsync(@"<div>@System.IO.Path.Combine(""Furion"", ""ViewEngine"")</div>", null, builder =>
        {
            builder.AddAssemblyReferenceByName("System.IO");
        });

        return result;
    }

    public async Task<string> Case6()
    {
        var result = await viewEngine.RunCompileFromCachedAsync(@"<div>@Path.Combine(""Furion"", ""ViewEngine"")</div>", null, builder =>
        {
            builder.AddUsing("System.IO");
            builder.AddAssemblyReferenceByName("System.IO");
        });

        return result;
    }

    public async Task<string> Case7()
    {
        var result = await viewEngine.RunCompileFromCachedAsync(@"
<area>
    @{ RecursionTest(3); }
</area>

@{
  void RecursionTest(int level)
  {
	if (level <= 0)
	{
		return;
	}

	<div>LEVEL: @level</div>
	@{ RecursionTest(level - 1); }
  }
}
");

        return result;
    }

    public async Task<string> Case8()
    {
        var content = @"Hello @Name, @Age, @Describe(""developer"")";

        var template = await viewEngine.CompileAsync<CustomModel>(content);   // 推荐使用 CompileFromCachedAsync 方法（缓存）

        var result = await template.RunAsync(new CustomModel
        {
            Name = "百小僧",
            Age = 30
        });

        return result;
    }

    public async Task<string> Case9()
    {
        var content = @"Hello @Model.Name, @Model.Age, @Model.Describe(""developer"")";

        var result = await viewEngine.RunCompileAsync(content, new CustomModel2
        {
            Name = "百小僧",
            Age = 30
        });

        return result;
    }

    public async Task<string> Case10()
    {
        var content = @"
@using Microsoft.Extensions.Configuration
@using Microsoft.Extensions.DependencyInjection

Hello @Name, @Age, @Describe(""developer"")

@Configuration[""Logging:LogLevel:Default""]

@{
    var configuration = ServiceProvider.GetService<IConfiguration>();
}

@configuration[""Logging:LogLevel:Default""]
";

        var template = await viewEngine.CompileAsync<CustomModel3>(content, builder => // 推荐使用 CompileFromCachedAsync 方法（缓存）
        {
            builder.AddAssemblyReference(typeof(IServiceProvider).Assembly);
            builder.AddAssemblyReference(typeof(ServiceProviderServiceExtensions).Assembly);
            builder.AddAssemblyReference(typeof(IConfiguration).Assembly);
        });

        var result = await template.RunAsync(new CustomModel3
        {
            Name = "百小僧",
            Age = 30,
            ServiceProvider = serviceProvider,
            Configuration = configuration
        });

        return result;
    }

    public async Task<string> Case11()
    {
        var content = @"
@{
    var numbers = new List<int> { 5, 12, 3, 8, 15 };
    var grouped = numbers.GroupBy(n => n % 2 == 0 ? ""Even"" : ""Odd"");
}

@foreach(var group in grouped)
{
    <h3>@group.Key Numbers</h3>
    <ul>
    @foreach(var num in group.OrderByDescending(n => n))
    {
        <li>@num</li>
    }
    </ul>
}
";

        var result = await viewEngine.RunCompileFromCachedAsync(content);

        return result;
    }

    public async Task<string> Case12()
    {
        var result = await viewEngine.RunCompileFromCachedAsync(@"<p>@Model.Description.Truncate(50)</p>",
     new DescModel { Description = "这是一个很长的描述，需要截断显示。" },
     builder =>
     {
         builder.AddAssemblyReference(typeof(StringExtensions).Assembly);
         builder.AddUsing("Furion.Application");
     });

        return result;
    }

    public async Task<string> Case13()
    {
        var headerTemplate = await viewEngine.RunCompileFromCachedAsync("<header>@Model.Title</header>", new { Title = "Furion" });
        var footerTemplate = await viewEngine.RunCompileFromCachedAsync("<footer>@Model.Year</footer>", new { DateTime.Now.Year });

        var result = await viewEngine.RunCompileFromCachedAsync(@"<body>@Model.Header @Model.Body @Model.Footer</body>",
            new
            {
                Header = headerTemplate,
                Body = "<p>Main Content</p>",
                Footer = footerTemplate
            });

        return result;
    }
}

public class CustomModel : ViewEngineModel
{
    public string Name { get; set; }
    public int Age { get; set; }

    public string Describe(string role)
    {
        return $"{Name} is a {Age}-year-old {role}.";
    }
}

public class CustomModel2
{
    public string Name { get; set; }
    public int Age { get; set; }

    public string Describe(string role)
    {
        return $"{Name} is a {Age}-year-old {role}.";
    }
}

public class CustomModel3 : ViewEngineModel
{
    public string Name { get; set; }
    public int Age { get; set; }

    public IServiceProvider ServiceProvider { get; set; }

    public IConfiguration Configuration { get; set; }

    public string Describe(string role)
    {
        return $"{Name} is a {Age}-year-old {role}.";
    }
}

public class DescModel
{
    public string Description { get; set; }
}