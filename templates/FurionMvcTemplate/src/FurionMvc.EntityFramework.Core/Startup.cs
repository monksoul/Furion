using Furion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FurionMvc.EntityFramework.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseAccessor(options =>
        {
            options.AddDbPool<DefaultDbContext>();
        }, "FurionMvc.Database.Migrations");
    }
}
