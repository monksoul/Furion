using Fur.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Fur.DataEncryption
{
    /// <summary>
    /// JWT 加解密
    /// </summary>
    public class JWTEncryption
    {
        /// <summary>
        /// 生成 Token
        /// </summary>
        /// <param name="issuerSigningKey"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static string Encrypt(string issuerSigningKey, Dictionary<string, object> payload)
        {
            return Encrypt(issuerSigningKey, JsonSerializer.Serialize(payload));
        }

        /// <summary>
        /// 生成 Token
        /// </summary>
        /// <param name="issuerSigningKey"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static string Encrypt(string issuerSigningKey, string payload)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(payload, credentials);
        }

        /// <summary>
        /// 验证 Token
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="jwtSettings"></param>
        /// <returns></returns>
        public static (bool IsValid, JsonWebToken Token) Validate(string accessToken, JWTSettingsOptions jwtSettings)
        {
            // 加密Key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 创建Token验证参数
            var tokenValidationParameters = CreateTokenValidationParameters(jwtSettings);
            if (tokenValidationParameters.IssuerSigningKey == null) tokenValidationParameters.IssuerSigningKey = creds.Key;

            // 验证 Token
            var tokenHandler = new JsonWebTokenHandler();
            try
            {
                var tokenValidationResult = tokenHandler.ValidateToken(accessToken, tokenValidationParameters);
                if (!tokenValidationResult.IsValid) return (false, null);

                var jsonWebToken = tokenValidationResult.SecurityToken as JsonWebToken;
                return (true, jsonWebToken);
            }
            catch
            {
                return (false, default);
            }
        }

        /// <summary>
        /// 验证 Token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool ValidateJwtBearerToken(DefaultHttpContext httpContext, out JsonWebToken token)
        {
            // 获取 token
            var accessToken = GetJwtBearerToken(httpContext);
            if (string.IsNullOrEmpty(accessToken))
            {
                token = null;
                return false;
            }

            // 读取 Jwt 配置
            var jwtSettings = httpContext.RequestServices.GetService<IOptions<JWTSettingsOptions>>().Value;

            // 验证token
            var (IsValid, Token) = Validate(accessToken, jwtSettings);
            token = IsValid ? Token : null;

            return IsValid;
        }

        /// <summary>
        /// 获取 JWT Bearer Token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetJwtBearerToken(DefaultHttpContext httpContext)
        {
            // 判断请求报文头中是否有 "Authorization" 报文头
            var bearerToken = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(bearerToken)) return default;

            return bearerToken[7..];
        }

        /// <summary>
        /// 生成Token验证参数
        /// </summary>
        /// <param name="jwtSettings"></param>
        /// <returns></returns>
        public static TokenValidationParameters CreateTokenValidationParameters(JWTSettingsOptions jwtSettings)
        {
            return new TokenValidationParameters
            {
                // 验证签发方密钥
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey.Value,
                // 签发方密钥
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey)),
                // 验证签发方
                ValidateIssuer = jwtSettings.ValidateIssuer.Value,
                // 设置签发方
                ValidIssuer = jwtSettings.ValidIssuer,
                // 验证签收方
                ValidateAudience = jwtSettings.ValidateAudience.Value,
                // 设置接收方
                ValidAudience = jwtSettings.ValidAudience,
                // 验证生存期
                ValidateLifetime = jwtSettings.ValidateLifetime.Value,
                // 过期时间容错值
                ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkew.Value),
            };
        }
    }
}