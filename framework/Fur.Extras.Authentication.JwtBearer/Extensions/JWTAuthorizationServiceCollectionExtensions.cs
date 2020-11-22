using Fur.Authorization;
using Fur.DataEncryption;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// JWT 授权服务拓展类
    /// </summary>
    public static class JWTAuthorizationServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 JWT 授权
        /// </summary>
        /// <param name="authenticationBuilder"></param>
        /// <param name="tokenValidationParameters">token 验证参数</param>
        /// <param name="enableGlobalAuthorize">启动全局授权</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddJwt(this AuthenticationBuilder authenticationBuilder, object tokenValidationParameters = default, bool enableGlobalAuthorize = false)
        {
            var services = authenticationBuilder.Services;

            // 配置 JWT 选项
            ConfigureJWTOptions(services);

            var jwtSettings = services.BuildServiceProvider().GetService<IOptions<JWTSettingsOptions>>().Value;

            authenticationBuilder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = (tokenValidationParameters as TokenValidationParameters) ?? JWTEncryption.CreateTokenValidationParameters(jwtSettings);
            });

            //启用全局授权
            if (enableGlobalAuthorize)
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new AuthorizeFilter());
                });
            }

            return authenticationBuilder;
        }

        /// <summary>
        /// 添加 JWT 授权
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions">授权配置</param>
        /// <param name="tokenValidationParameters">token 验证参数</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddJwt(this IServiceCollection services, Action<AuthenticationOptions> configureOptions = null, object tokenValidationParameters = default)
        {
            // 配置 JWT 选项
            ConfigureJWTOptions(services);

            var jwtSettings = services.BuildServiceProvider().GetService<IOptions<JWTSettingsOptions>>().Value;

            // 添加默认授权
            return services.AddAuthentication(options =>
             {
                 if (configureOptions == null)
                 {
                     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                 }
                 else configureOptions.Invoke(options);
             }).AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = (tokenValidationParameters as TokenValidationParameters) ?? JWTEncryption.CreateTokenValidationParameters(jwtSettings);
             });
        }

        /// <summary>
        /// 添加 JWT 授权
        /// </summary>
        /// <param name="services"></param>
        private static void ConfigureJWTOptions(IServiceCollection services)
        {
            // 获取配置节点
            var jwtSettingsConfiguration = services.BuildServiceProvider()
                        .GetService<IConfiguration>()
                        .GetSection("JWTSettings");

            // 配置验证
            services.AddOptions<JWTSettingsOptions>()
                        .Bind(jwtSettingsConfiguration)
                        .ValidateDataAnnotations();

            // 选项后期配置
            services.PostConfigure<JWTSettingsOptions>(options =>
            {
                options.ValidateIssuerSigningKey ??= true;
                if (options.ValidateIssuerSigningKey == true)
                {
                    options.IssuerSigningKey ??= "U2FsdGVkX1+6H3D8Q//yQMhInzTdRZI9DbUGetbyaag=";
                }
                options.ValidateIssuer ??= true;
                if (options.ValidateIssuer == true)
                {
                    options.ValidIssuer ??= "dotnetchina";
                }
                options.ValidateAudience ??= true;
                if (options.ValidateAudience == true)
                {
                    options.ValidAudience ??= "powerby Fur";
                }
                options.ValidateLifetime ??= true;
                if (options.ValidateLifetime == true)
                {
                    options.ClockSkew ??= 10;
                }
                options.ExpiredTime ??= 20;
            });
        }
    }
}