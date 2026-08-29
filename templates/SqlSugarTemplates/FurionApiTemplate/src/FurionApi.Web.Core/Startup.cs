using Furion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FurionApi.Web.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddConsoleFormatter();
        services.AddJwt<JwtHandler>();

        services.AddCorsAccessor();

        services.AddControllers()
                .AddInjectWithUnifyResult();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCorsAccessor();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseInject(string.Empty);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
