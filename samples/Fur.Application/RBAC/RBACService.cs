using Fur.Authorization;
using Fur.Core;
using Fur.DatabaseAccessor;
using Fur.DataEncryption;
using Fur.DynamicApiController;
using Fur.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Fur.Application
{
    /// <summary>
    /// 角色管理服务
    /// </summary>
    [AppAuthorize, ApiDescriptionSettings("角色管理")]
    public class RBACService : IDynamicApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<RoleSecurity> _roleSecurityRepository;
        private readonly IRepository<Security> _securityRepository;
        private readonly IAuthorizationManager _authorizationManager;

        public RBACService(IHttpContextAccessor httpContextAccessor
            , IRepository<User> userRepository
            , IRepository<Role> roleRepository
            , IRepository<UserRole> userRoleRepository
            , IRepository<RoleSecurity> roleSecurityRepository
            , IRepository<Security> securityRepository
            , IAuthorizationManager authorizationManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _roleSecurityRepository = roleSecurityRepository;
            _securityRepository = securityRepository;
            _authorizationManager = authorizationManager;
        }

        /// <summary>
        /// 登录（免授权）
        /// </summary>
        /// <param name="input"></param>
        /// <remarks>管理员：admin/admin；普通用户：Fur/dotnetchina</remarks>
        /// <returns></returns>
        [AllowAnonymous, IfException(1000, ErrorMessage = "用户名或密码错误")]
        public LoginOutput Login(LoginInput input)
        {
            // 验证用户名和密码
            var user = _userRepository.FirstOrDefault(u => u.Account.Equals(input.Account) && u.Password.Equals(input.Password), false) ?? throw Oops.Oh(1000);

            var output = user.Adapt<LoginOutput>();

            // 生成 token
            var jwtSettings = App.GetOptions<JWTSettingsOptions>();
            var datetimeOffset = DateTimeOffset.UtcNow;

            output.AccessToken = JWTEncryption.Encrypt(jwtSettings.IssuerSigningKey, new Dictionary<string, object>()
            {
                { "UserId", user.Id },  // 存储Id
                { "Account",user.Account }, // 存储用户名

                { JwtRegisteredClaimNames.Iat, datetimeOffset.ToUnixTimeSeconds() },
                { JwtRegisteredClaimNames.Nbf, datetimeOffset.ToUnixTimeSeconds() },
                { JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddSeconds(jwtSettings.ExpiredTime.Value*60).ToUnixTimeSeconds() },
                { JwtRegisteredClaimNames.Iss, jwtSettings.ValidIssuer},
                { JwtRegisteredClaimNames.Aud, jwtSettings.ValidAudience }
            });

            // 设置 Swagger 刷新自动授权
            _httpContextAccessor.SigninToSwagger(output.AccessToken);

            return output;
        }

        /// <summary>
        /// 查看用户角色
        /// </summary>
        [SecurityDefine(SecurityConst.ViewRoles)]
        public List<RoleDto> ViewRoles()
        {
            // 获取用户Id
            var userId = _authorizationManager.GetUserId();

            var roles = _userRepository
                .DetachedEntities
                .Include(u => u.Roles)
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Roles)
                .ToList();

            return roles.Adapt<List<RoleDto>>();
        }

        /// <summary>
        /// 查看用户权限
        /// </summary>
        /// <returns></returns>
        [SecurityDefine(SecurityConst.ViewSecuries)]
        public List<SecurityDto> ViewSecuries()
        {
            // 获取用户Id
            var userId = _authorizationManager.GetUserId();

            var securities = _userRepository
                .Include(u => u.Roles, false)
                    .ThenInclude(u => u.Securities)
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Roles
                    .SelectMany(u => u.Securities))
                .ToList();

            return securities.Adapt<List<SecurityDto>>();
        }

        /// <summary>
        /// 角色列表
        /// </summary>
        [SecurityDefine(SecurityConst.GetRoles)]
        public List<RoleDto> GetRoles()
        {
            return _roleRepository.AsEnumerable(false).Adapt<List<RoleDto>>();
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        [SecurityDefine(SecurityConst.InsertRole)]
        public void InsertRole(RoleInput input)
        {
            _roleRepository.Insert(input.Adapt<Role>());
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        [SecurityDefine(SecurityConst.GiveUserRole)]
        public void GiveUserRole(int[] roleIds)
        {
            // 获取用户Id
            var userId = _authorizationManager.GetUserId();

            roleIds ??= Array.Empty<int>();
            _userRoleRepository.Delete(_userRoleRepository.Where(u => u.UserId == userId, false).ToList());

            var list = new List<UserRole>();
            foreach (var roleid in roleIds)
            {
                list.Add(new UserRole { UserId = userId, RoleId = roleid });
            }

            _userRoleRepository.Insert(list);
        }

        /// <summary>
        /// 查看系统所有的权限（免授权）
        /// </summary>
        [AllowAnonymous]
        public List<SecurityDto> GetSecurities()
        {
            return _securityRepository.AsEnumerable(false).Adapt<List<SecurityDto>>();
        }

        /// <summary>
        /// 为角色分配权限
        /// </summary>
        [SecurityDefine(SecurityConst.GiveRoleSecurity)]
        public void GiveRoleSecurity(int roleId, int[] securityIds)
        {
            securityIds ??= Array.Empty<int>();
            _roleSecurityRepository.Delete(_roleSecurityRepository.Where(u => u.RoleId == roleId, false).ToList());

            var list = new List<RoleSecurity>();
            foreach (var securityId in securityIds)
            {
                list.Add(new RoleSecurity { RoleId = roleId, SecurityId = securityId });
            }

            _roleSecurityRepository.Insert(list);
        }
    }
}