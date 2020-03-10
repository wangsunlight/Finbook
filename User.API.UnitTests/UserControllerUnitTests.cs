using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.Linq;

namespace User.API.UnitTests
{
    public class UserControllerUnitTests
    {
        private Data.UserContext GetUserContext()
        {
            var option = new DbContextOptionsBuilder<Data.UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var useContext = new Data.UserContext(option);

            useContext.Users.Add(new Models.AppUser
            {
                Id = 1,
                Name = "zhaoyang"
            });
            useContext.SaveChanges();

            return useContext;
        }

        private (Controllers.UserController controller, Data.UserContext userContext) GetController()
        {
            var context = GetUserContext();
            var loggerMoq = new Mock<ILogger<Controllers.UserController>>();

            //loggerMoq.Setup(l=>l.LogError())
            var logger = loggerMoq.Object;

            return (controller: new Controllers.UserController(context, logger, null), userContext: context);
        }
        [Fact]
        public async Task Get_ReturnRigthUser_WinExpectedParameters()
        {
            (Controllers.UserController controller, Data.UserContext userContext) = GetController();

            var response = await controller.Get();

            //Assert.IsType<JsonResult>(response);

            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;
            appUser.Id.Should().Be(1);
            appUser.Name.Should().Be("zhaoyang");
        }

        [Fact]
        public async Task Patch_ReturnNewName_WinExpectedNewNameParameters()
        {
            (Controllers.UserController controller, Data.UserContext userContext) = GetController();

            var document = new JsonPatchDocument<Models.AppUser>();
            document.Replace(user => user.Name, "wang");

            var response = await controller.Patch(document);

            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;

            appUser.Name.Should().Be("wang");


            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Should().NotBeNull();
            userModel.Name.Should().Be("wang");

        }

        [Fact]
        public async Task Patch_ReturnNewProperties_WinExpectedNewPropertiesParameters()
        {
            (Controllers.UserController controller, Data.UserContext userContext) = GetController();

            var document = new JsonPatchDocument<Models.AppUser>();
            document.Replace(user => user.Properties, new List<Models.UserProperty> {
            new Models.UserProperty{Key="fin_industry",Value ="»¥ÁªÍø",Text="»¥ÁªÍø" }
            });

            var response = await controller.Patch(document);

            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<Models.AppUser>().Subject;

            appUser.Properties.Count.Should().Be(1);
            appUser.Properties.First().Value.Should().Be("»¥ÁªÍø");
            appUser.Properties.First().Key.Should().Be("fin_industry");


            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Properties.Count.Should().Be(1);
            userModel.Properties.First().Value.Should().Be("»¥ÁªÍø");
            userModel.Properties.First().Key.Should().Be("fin_industry");

        }
    }
}
