using Furion.Application.Persons;
using Furion.Application.Services;
using Furion.DatabaseAccessor.Extensions;
using Furion.Localization;
using System.ComponentModel;

namespace Furion.Application;

/// <summary>
/// 用户管理
/// </summary>
[AllowAnonymous, ApiDescriptionSettings("Default@1")]
public class PersonService : IDynamicApiController
{
    private readonly IRepository<Person> _personRepository;
    private readonly ITestService _testService;
    private readonly ISql _sql;

    public PersonService(IRepository<Person> personRepository
        , ITestService testService
        , ISql sql)
    {
        _personRepository = personRepository;
        _testService = testService;
        _sql = sql;
    }

    /// <summary>
    /// 新增一条
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Description = "我是一段描述，显示更多内容 <button>我是按钮</button>")]
    public async Task<int> Insert(PersonInputDto input)
    {
        // 如果不需要返回自增Id，使用 InsertAsync即可
        var newEntity = await _personRepository.InsertNowAsync(input.Adapt<Person>());
        return newEntity.Entity.Id;

        // 还可以直接操作
        // await personDto.Adapt<Person>().InsertNowAsync();
    }

    /// <summary>
    /// 更新一条
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [Obsolete]
    public async Task Update(PersonInputDto input)
    {
        var person = await _personRepository.Include(u => u.PersonDetail, false)
                                            .Include(u => u.Childrens)
                                            .Include(u => u.Posts)
                                            .SingleAsync(u => u.Id == input.Id.Value);

        input.Adapt(person);

        await _personRepository.UpdateAsync(person);

        // 还可以直接操作
        // await personDto.Adapt<Person>().UpdateAsync();
    }

    /// <summary>
    /// 删除一条
    /// </summary>
    /// <param name="id"></param>
    public async Task Delete(int id)
    {
        await _personRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 查询一条
    /// </summary>
    /// <param name="id"></param>
    public async Task<PersonDto> Find(int id)
    {
        var person = await _personRepository.FindAsync(id);
        return person.Adapt<PersonDto>();
    }

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    public async Task<List<PersonDto>> GetAll()
    {
        var persons = _personRepository.AsQueryable(false)
                                       .ProjectToType<PersonDto>();
        return await persons.ToListAsync();
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(LowercaseRoute = false, KeepName = true)]
    public async Task<PagedList<PersonDto>> GetAllByPage(int pageIndex = 1, int pageSize = 10)
    {
        var pageResult = _personRepository.AsQueryable(false)
                                          .ProjectToType<PersonDto>();

        return await pageResult.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// 搜索数据
    /// </summary>
    /// <param name="name"></param>
    /// <param name="age"></param>
    /// <returns></returns>
    public async Task<List<PersonDto>> Search([FromQuery] string name, [FromQuery] int age)
    {
        var persons = _personRepository.Where(!string.IsNullOrEmpty(name), u => u.Name.Contains(name), false)
                                       .Where(age > 18, u => u.Age > 18)
                                       .ProjectToType<PersonDto>();

        return await persons.ToListAsync();
    }

    public string GetName()
    {
        return _testService.GetName();
    }

    public List<Person> GetProxyPersons()
    {
        return _sql.GetPersons();
    }

    public int TestRowEffects()
    {
        return _sql.UpdatePerson(1);
    }

    public Task<int> TestRowEffects2()
    {
        return _sql.UpdatePerson2(1);
    }

    /// <summary>
    /// 测试数据库参数少给或多个情况
    /// </summary>
    public void TestDbParameters()
    {
        var data = "select * from person where id=@Id".SqlQuery(new
        {
            Id = 1,
            Name = "Furion"
        });
    }

    /// <summary>
    /// 测试 Markdown
    /// </summary>
    /// <remarks>
    /// # 先知 / Furion ([探索版](https://gitee.com/dotnetchina/Furion/tree/experimental/))
    ///
    ///     一个应用程序框架，您可以将它集成到任何.NET/C# 应用程序中。
    ///
    /// An application framework that you can integrate into any.NET/C# application.
    ///
    /// ## 安装 / Installation
    ///
    /// - [Package Manager] (https://www.nuget.org/packages/Furion)
    ///
    /// ```powershell
    /// Install-Package Furion
    /// ```
    ///
    /// - [.NET CLI] (https://www.nuget.org/packages/Furion)
    ///
    /// ```powershell
    /// dotnet add package Furion
    /// ```
    ///
    /// ## 例子 / Examples
    ///
    /// 我们在[主页](https://furion.net)上有不少例子，这是让您入门的第一个：
    ///
    /// We have several examples [on the website] (https://furion.net). Here is the first one to get you started:
    ///
    /// ```cs
    /// Serve.Run();
    ///
    ///     [DynamicApiController]
    ///     public class HelloService
    ///     {
    ///         public string Say()
    ///         {
    ///             return "Hello, Furion";
    ///         }
    ///     }
    /// ```
    ///
    /// 打开浏览器访问 `https://localhost:5001`。
    ///
    /// Open browser access `https://localhost:5001`.
    ///
    /// ## 文档 / Documentation
    ///
    /// 您可以在[主页] (https://furion.net)或[备份主页](https://furion.net)找到 Furion 文档。
    ///
    /// You can find the Furion documentation[on the website](https://furion.net) or [on the backup website](https://furion.net).
    ///
    /// ## 贡献 / Contributing
    ///
    /// 该存储库的主要目的是继续发展 Furion 核心，使其更快、更易于使用。 Furion 的开发在[Gitee](https://gitee.com/dotnetchina/Furion) 上公开进行，我们感谢社区贡献错误修复和改进。
    ///
    /// 阅读[贡献指南] (https://furion.net/docs/contribute)内容，了解如何参与改进 Furion。
    ///
    /// The main purpose of this repository is to continue evolving Furion core, making it faster and easier to use.Development of Furion happens in the open on[Gitee] (https://gitee.com/dotnetchina/Furion), and we are grateful to the community for contributing bugfixes and improvements.
    ///
    /// Read[contribution documents] (https://furion.net/docs/contribute) to learn how you can take part in improving Furion.
    ///
    /// ## 许可证 / License
    ///
    /// Furion 采用[MIT](https://gitee.com/dotnetchina/Furion/blob/v4/LICENSE) 开源许可证。
    ///
    /// Furion uses the[MIT] (https://gitee.com/dotnetchina/Furion/blob/v4/LICENSE) open source license.
    ///
    /// ```
    /// Copyright© 2020-present 百小僧, Baiqian Co., Ltd.
    /// Furion is licensed under Mulan PSL v2.
    /// You can use this software according to the terms andconditions of the Mulan PSL v2.
    /// You may obtain a copy of Mulan PSL v2 at:
    ///             https://gitee.com/dotnetchina/Furion/blob/v4/LICENSE
    /// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUTWARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.
    /// See the Mulan PSL v2 for more details.
    /// ```
    ///
    /// </remarks>
    /// <returns></returns>
    public string MarkdownTestMore()
    {
        return "Furion";
    }

    /// <summary>
    /// 测试 Markdown
    /// </summary>
    /// <remarks>
    /// # 测试 `Markdown` 注释
    ///
    /// ![](https://localhost:44316/images/logo.png)
    ///
    /// ```cs
    /// Serve.Run();
    ///
    /// [DynamicApiController
    /// public class HelloService
    /// {
    ///     public string Say()
    ///     {
    ///         return nameof(Furion);
    ///     }
    /// }
    /// ```
    ///
    /// 功能还不错！！！
    ///
    /// | 商品         | 价格     | # 其他 |
    /// |--------------|-----------|------------|
    /// | Juicy Apples | 1.99      | *7*        |
    /// | Bananas      | **1.89**  | 5234       |
    /// | Bananas      | **1.89**  | 5234       |
    /// | Bananas      | **1.89**  | 5234       |
    ///
    /// -----
    ///
    /// # Furion 探索版
    ///
    /// > 在过去一年，实现 `Furion` 从无到有，编写文档已逾百万字，过程心酸开源人自知。
    /// >
    /// > 这一路日夜兼程，嘲讽批评常伴眼耳，即便辛苦无奈、想过放弃，但为了那微不足道的存在感依然努力着。
    /// >
    /// > 当然，也收获了不少...越来越多拥趸者，越发精湛技术能力，更高层次思维模式，还有许多跨界跨行朋友。
    /// >
    /// > 在 《[开源指北] (https://gitee.com/opensource-guide/comments/)》中，我曾说道：“开源如同人的脸，好坏一面便知，缺点可能会受到嘲讽批评，优点也会收获赞扬尊重。别担心，他们正在塑造更好的你。”
    /// >
    /// > 所以，这一次重新起航，重塑 `Furion` 重塑自己。也许未来在某个 IT 圈但凡有人谈起 `.NET` 还能瞟到 `Furion` 的身影。
    ///
    /// ---
    ///
    /// 🎉 探索 Furion 未来更多可能性，实现无第三方依赖的版本，所有模块功能按需安装，按需加载。
    ///
    /// - 作者：[百小僧] (https://gitee.com/monksoul)
    /// - 日期：2021 年 08 月 30 日
    ///
    /// ## 环境
    ///
    /// - IDE ：[Microsoft Visual Studio Enterprise 2022 Preview(64 位) 版本 17.0.0 Preview 3.1] (https://visualstudio.microsoft.com/zh-hans/vs/preview/)
    /// - SDK ：[.NET SDK 6] (https://dotnet.microsoft.com/download/dotnet/6.0)
    /// - 语言：[C# 10](https://docs.microsoft.com/zh-cn/dotnet/csharp/whats-new/csharp-10)
    ///
    /// ## 包说明
    ///
    /// - `Furion.Core`：无第三方依赖，可在 `.NET 6` 所有项目类型中运行。
    /// - `Furion`：内部依赖 `Furion.Core` 且无第三方依赖，**聚焦 `Web` 应用**，采用共享框架 `<FrameworkReference Include="Microsoft.AspNetCore.App" />` 模式
    ///  </remarks>
    [ApiDescriptionSettings(Order = 1000)]
    public void MarkdownTest()
    {
    }

    public string 测试多语言()
    {
        return L.Text["API 接口"];
    }

    public void 测试多线程没有创建作用域()
    {
        Task.Run(() =>
        {
            var c = Db.GetRepository<Person>().FirstOrDefault();
        });
    }

    [UnitOfWork]
    public async Task 测试环境事务(int id)
    {
        await _personRepository.DeleteNowAsync(id);
        var d = await _personRepository.SqlQueriesAsync("select * from persion2 d");
    }

    [DisplayName("生成注释")]
    public void 测试DsiplayName特性()
    {
    }

    /// <summary>
    /// 如果同时存在显示这个
    /// </summary>
    [DisplayName("生成注释")]
    public void 我是DisplayName()
    {

    }

    public async Task<List<PersonDto>> 测试分表联合查询()
    {
        var persons = _personRepository
            .FromSegments(table => [table, $"{table}_202408"])
            .Include(u => u.Posts)
            .Where(u => u.Id == 1)
            .Take(5)
            .ProjectToType<PersonDto>();

        return await persons.ToListAsync();
    }

    public async Task 测试分表插入()
    {
        await _personRepository
           .InsertFromSegmentsAsync(table => [table], new Person { Id = 2, Name = "Furion", Age = 32 });
    }

    public async Task 测试分表更新()
    {
        await _personRepository
           .UpdateFromSegmentsAsync(table => [table], new Person { Id = 1, Name = "Furion", Age = 32 });
    }

    public async Task 测试分表删除()
    {
        await _personRepository
           .DeleteFromSegmentsAsync(table => [table], 1, 2, 3, 4);
    }
}