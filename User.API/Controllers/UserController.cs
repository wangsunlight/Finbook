using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch;
using User.API.Models;
using DotNetCore.CAP;
using User.API.Dtos;

namespace User.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : BaseController
    {
        private UserContext _userContext;
        private ILogger<UserController> _logger;
        private ICapPublisher _capPublisher;
        public UserController(UserContext userContext, ILogger<UserController> logger, ICapPublisher capPublisher)
        {
            this._userContext = userContext;
            this._logger = logger;
            this._capPublisher = capPublisher;
        }


        private void RaiseUserprofileChangedEvent(AppUser user)
        {
            if (_userContext.Entry(user).Property(nameof(user.Name)).IsModified ||
                _userContext.Entry(user).Property(nameof(user.Title)).IsModified ||
                _userContext.Entry(user).Property(nameof(user.Company)).IsModified ||
                _userContext.Entry(user).Property(nameof(user.Avatar)).IsModified)
            {
                _capPublisher.Publish("finbook.userapi.userprofilechanged",
                    new UserIdentity
                    {
                        UserId = user.Id,
                        Title = user.Title,
                        Name = user.Name,
                        Avatar = user.Avatar,
                        Company = user.Company
                    }); ;

            }
        }


        [Route("get")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userContext.Users
                .AsNoTracking()
                .Include(u => u.Properties)
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            if (user == null)
                throw new UserOperationExcepton($"错误的用户上下文Id = {UserIdentity.UserId}");

            return new JsonResult(user);
        }


        [Route("patch")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<AppUser> patch)
        {
            var user = await _userContext.Users
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            patch.ApplyTo(user);

            foreach (var property in user?.Properties)
            {
                _userContext.Entry(property).State = EntityState.Detached;
            }

            var orginProperites = await _userContext.UserProperties
                .Where(u => u.AppUserId == UserIdentity.UserId).ToListAsync();

            var allProperites = orginProperites.Union(user.Properties).Distinct();

            var removeProperites = orginProperites.Except(user.Properties);
            var newProperites = allProperites.Except(orginProperites);

            foreach (var property in removeProperites)
            {
                _userContext.Remove(property);
            }
            foreach (var property in newProperites)
            {
                _userContext.Add(property);
            }

            using (var trans = _userContext.Database.BeginTransaction())
            {

                //发布用户属性变更消息
                RaiseUserprofileChangedEvent(user);

                _userContext.Users.Update(user);
                _userContext.SaveChanges();

                trans.Commit();
            }

            return new JsonResult(user);
        }

        /// <summary>
        /// 检查或创建用户（当用户手机号不存在的时候创建用户）
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [Route("check-or-create/{phone}")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreate(string phone)
        {
            var user = _userContext.Users.SingleOrDefault(u => u.Phone == phone);
            if (user == null)
            {
                user = new AppUser { Phone = phone };
                _userContext.Users.Add(user);
                await _userContext.SaveChangesAsync();
            }

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar,
            });
        }

        /// <summary>
        /// 获取用户标签数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetUserTags()
        {
            return Ok(await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync());
        }

        /// <summary>
        /// 根据手机号查找用户资料
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("search/{phone}")]
        public async Task<IActionResult> Search(string phone)
        {
            return Ok(await _userContext.Users.Include(u => u.Properties).SingleOrDefaultAsync(u => u.Phone == phone));
        }

        /// <summary>
        /// 更新用户标签数据
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("tags")]
        public async Task<IActionResult> UpdateUserTags([FromBody]List<string> tags)
        {
            var orginTags = await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync();

            //差集  前面有后面没有的
            var newTags = tags.Except(orginTags.Select(t => t.Tag));

            await _userContext.UserTags.AddRangeAsync(newTags.Select(t => new UserTag
            {
                CreatedTime = DateTime.Now,
                UserId = UserIdentity.UserId,
                Tag = t
            }));

            await _userContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Route("beasinfo/{userId}")]
        public async Task<IActionResult> GetBaseInfo(int userId)
        {
            //检查用户是否好友关系


            var user = await _userContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                userId = user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar
            });
        }
    }
}