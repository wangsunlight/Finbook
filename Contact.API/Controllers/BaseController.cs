using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.Controllers
{
    public class BaseController : ControllerBase
    {
        protected UserIdentity UserIdentity
        {
            get
            {
                return new UserIdentity
                {
                    UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "sub").Value),
                    Name = User.Claims.FirstOrDefault(c => c.Type == "name").Value,
                    Company = User.Claims.FirstOrDefault(c => c.Type == "sub").Value,
                    Title = User.Claims.FirstOrDefault(c => c.Type == "title").Value,
                    Avatar = User.Claims.FirstOrDefault(c => c.Type == "avatar").Value,
                };
            }
        }
    }
}
