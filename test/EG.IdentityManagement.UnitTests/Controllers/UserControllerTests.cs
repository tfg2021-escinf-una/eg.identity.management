using AutoMapper;
using EG.IdentityManagement.Microservice.Controllers;
using EG.IdentityManagement.Microservice.Entities.ViewModels;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace EG.IdentityManagement.UnitTests.Controllers
{
    public class UserControllerTests
    {
        private Mock<IIdentityService> fakeIdentityService;
        private Mock<IMapper> fakeUserMapper;
        private UserController userController;
        private CredentialsViewModel fakeCredentialsViewModel;
        private TokensViewModel fakeTokenViewModel;
        private IdentityViewModel fakeIdentityViewModel;

        [SetUp]
        public void Initialize()
        {
            fakeUserMapper = new();
            fakeIdentityService = new();
            fakeCredentialsViewModel = new();
            fakeTokenViewModel = new();
            fakeCredentialsViewModel = new();
            userController = new(fakeIdentityService.Object, fakeUserMapper.Object);
        }

        [Test]
        public async Task UserLogin_ShouldReturn200OK()
        {
            fakeIdentityService.Setup(method => method.Login(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new OkObjectResult(new { }));
            var result = await userController.Login(fakeCredentialsViewModel);
            var OkObjectResult = result as OkObjectResult;
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }

        [Test]
        public async Task UserRegister_ShouldReturn200OK()
        {
            fakeIdentityService.Setup(method => method.Register(It.IsAny<User>())).ReturnsAsync(new OkObjectResult(new { }));
            var result = await userController.Register(fakeIdentityViewModel);
            var OkObjectResult = result as OkObjectResult;
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }

        [Test]
        public async Task UserRefresh_ShouldReturn200OK()
        {
            fakeIdentityService.Setup(method => method.Refresh(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new OkObjectResult(new { }));
            var result = await userController.Refresh(fakeTokenViewModel);
            var OkObjectResult = result as OkObjectResult;
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }
    }
}
