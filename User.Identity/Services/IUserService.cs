using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 检查或创建用户（当用户手机号不存在的时候创建用户）
        /// </summary>
        /// <param name="phone">手机号</param>
        Task<UserInfo> CheckOrCreate(string phone);
    }
}
