using Fur.DatabaseAccessor;
using Fur.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;

namespace Fur.Core
{
    /// <summary>
    /// 权限管理器
    /// </summary>
    public class AuthorizationManager : IAuthorizationManager, ITransient
    {
        /// <summary>
        /// 请求上下文访问器
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 数据库仓储
        /// </summary>
        private readonly IRepository<User> _userRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="userRepository"></param>
        public AuthorizationManager(IHttpContextAccessor httpContextAccessor
            , IRepository<User> userRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 获取用户Id
        /// </summary>
        /// <returns></returns>
        public int GetUserId()
        {
            return int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue("UserId"));
        }

        /// <summary>
        /// 检查权限
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public bool CheckSecurity(string resourceId)
        {
            var userId = GetUserId();

            // ========= 以下代码应该缓存起来 ===========

            // 查询用户拥有的权限
            var securities = _userRepository
                .Include(u => u.Roles, false)
                    .ThenInclude(u => u.Securities)
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Roles
                    .SelectMany(u => u.Securities))
                .Select(u => u.UniqueName);

            if (!securities.Contains(resourceId)) return false;

            return true;
        }
    }
}