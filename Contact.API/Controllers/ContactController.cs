using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Contact.API.Models;
using Contact.API.Data;
using Contact.API.Service;
using System.Threading;
using Contact.API.ViewModels;

namespace Contact.API.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactController : BaseController
    {
        private IContactApplyRequestRespository _contactApplyRequestRespository;
        private IContactRepository _contactRepository;
        private IUserService _userService;
        public ContactController(IContactApplyRequestRespository contactApplyRequestRespository, IUserService userService, IContactRepository contactRepository)
        {
            this._contactApplyRequestRespository = contactApplyRequestRespository;
            this._userService = userService;
            this._contactRepository = contactRepository;
        }

        /// <summary>
        /// 获取联系人列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactAsync(UserIdentity.UserId, cancellationToken));
        }

        /// <summary>
        /// 更新好友标签
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("tag")]
        public async Task<IActionResult> TagContact([FromBody]TagContactInputViewModel viewModel, CancellationToken cancellationToken)
        {
            var result = await _contactRepository.TagContactAsync(UserIdentity.UserId, viewModel.ContactId, viewModel.Tags, cancellationToken);
            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("apply-requests")]
        public async Task<IActionResult> GetApplyRequests(CancellationToken cancellationToken)
        {
            var request = await _contactApplyRequestRespository.GetRequestListAsync(UserIdentity.UserId, cancellationToken);

            return Ok(request);
        }

        /// <summary>
        /// 添加好友申请
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("apply-requests/{userId}")]
        public async Task<IActionResult> AddApplyRequest(int userId, CancellationToken cancellationToken)
        {
            //var userBaseUserInfo = await _userService.GetBaseUserInfoAsync(userId);
            //if (userBaseUserInfo == null)
            //{
            //    throw new Exception("用户参数错误");
            //}

            var result = await _contactApplyRequestRespository.AddRequestAsync(new ContactApplyRequest
            {
                UserId = userId,
                ApplierId = UserIdentity.UserId,
                ApplyTime = DateTime.Now,
                Name = UserIdentity.Name,
                Company = UserIdentity.Company,
                Title = UserIdentity.Title,
                Avatar = UserIdentity.Avatar
            }, cancellationToken);

            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        /// <summary>
        /// 通过好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("apply-requests")]
        public async Task<IActionResult> ApprovalApplyRequest(int applierId, CancellationToken cancellationToken)
        {
            var result = await _contactApplyRequestRespository.ApprovalAsync(UserIdentity.UserId, applierId, cancellationToken);

            if (!result)
            {
                return BadRequest();
            }

            var applier = await _userService.GetBaseUserInfoAsync(applierId);
            var userInfo = await _userService.GetBaseUserInfoAsync(UserIdentity.UserId);

            await _contactRepository.AddContactAsync(UserIdentity.UserId, applier, cancellationToken);
            await _contactRepository.AddContactAsync(applierId, userInfo, cancellationToken);

            return Ok();
        }
    }
}
