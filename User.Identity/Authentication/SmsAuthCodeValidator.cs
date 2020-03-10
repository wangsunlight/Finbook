using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using IdentityServer4.Models;
using User.Identity.Services;
using System.Security.Claims;

namespace User.Identity.Authentication
{
    public class SmsAuthCodeValidator : IExtensionGrantValidator
    {
        private readonly IAuthCodeService _authCodeService;

        private readonly IUserService _userService;

        public SmsAuthCodeValidator(
            IAuthCodeService authCodeService,
            IUserService userService
            )
        {
            this._authCodeService = authCodeService;
            this._userService = userService;
        }
        public string GrantType => "sms_auth_code";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var phone = context.Request.Raw["phone"];
            var code = context.Request.Raw["auth_code"];
            var errorValidationResult = new GrantValidationResult(TokenRequestErrors.InvalidGrant);

            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(code))
            {
                context.Result = errorValidationResult;
                return;
            }

            //检查验证码
            if (!_authCodeService.Validate(phone, code))
            {
                context.Result = errorValidationResult;
                return;
            }

            //完成用户注册
            var userInfo = await _userService.CheckOrCreate(phone);
            if (userInfo == null)
            {
                context.Result = errorValidationResult;
                return;
            }

            var claims = new Claim[] {
                new Claim("name",userInfo.Name??String.Empty),
                new Claim("title",userInfo.Title??String.Empty),
                new Claim("company",userInfo.Company??String.Empty),
                new Claim("avatar",userInfo.Avatar??String.Empty),
            };

            context.Result = new GrantValidationResult(userInfo.id.ToString(), GrantType, claims);
        }
    }
}
