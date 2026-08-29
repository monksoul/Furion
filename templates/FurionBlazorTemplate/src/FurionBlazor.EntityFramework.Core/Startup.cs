using Furion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FurionBlazor.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "FurionBlazor.Database.Migrations");
    }
}
