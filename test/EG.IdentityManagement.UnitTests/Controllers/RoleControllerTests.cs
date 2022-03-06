using AutoMapper;
using EG.IdentityManagement.Microservice.Controllers;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Entities.ViewModels;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace EG.IdentityManagement.UnitTests.Controllers
{
    public class RoleControllerTests
    {

        private Mock<IRoleService> fakeRoleService;
        private Mock<IMapper> fakeRoleMapper;
        private RoleController roleController;
        private RoleViewModel fakeRoleViewModel;
        private Role fakeRole;

        [SetUp]
        public void Initialize()
        {
            fakeRoleMapper = new();
            fakeRoleService = new();
            fakeRoleViewModel = new();
            fakeRole = new();
            roleController = new(fakeRoleService.Object, fakeRoleMapper.Object);
        }

        [Test]
        public void RoleServiceThrowsNullException_WhileInitializingServices()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                roleController = new(null, null);
            });
        }

        [Test]
        public void MapperThrowsNullException_WhileInitializingServices()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                roleController = new(fakeRoleService.Object, null);
            });
        }

        [Test]
        public async Task CreateRole_ShouldReturn200OK()
        {
            fakeRoleService.Setup(method => method.CreateRole(It.IsAny<Role>())).ReturnsAsync(new OkObjectResult(new { }));
            var result = await roleController.CreateRole(fakeRoleViewModel);
            var OkObjectResult = result as OkObjectResult;
            Assert.AreEqual(200, OkObjectResult.StatusCode);

        }

        [Test]
        public void GetRoles_ShouldReturn200OK()
        {
            fakeRoleService.Setup(method => method.GetRoles()).Returns(new OkObjectResult(new { }));
            var result = roleController.GetRoles();
            var OkObjectResult = result as OkObjectResult;
            Assert.AreEqual(200, OkObjectResult.StatusCode);

        }

        [Test]
        public async Task GetRoleById_ShouldReturn200OK()
        {
            fakeRoleService.Setup(method => method.GetRoleById(It.IsAny<string>())).ReturnsAsync(new OkObjectResult(new { }));
            var result = await roleController.GetRoleById("fakeRoleID");
            var OkObjectResult = result as OkObjectResult;
            Assert.AreEqual(200, OkObjectResult.StatusCode);

        }

        [Test]
        public async Task UpdateRole_ShouldReturn200OK()
        {
            fakeRoleService.Setup(method => method.UpdateRole(It.IsAny<string>(), It.IsAny<Role>())).ReturnsAsync(new OkObjectResult(new { }));
            var result = await roleController.UpdateRole("fakeRoleID", fakeRoleViewModel);
            var OkObjectResult = result as OkObjectResult;
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }

        [Test]
        public async Task DeleteRole_ShouldReturn200OK()
        {
            fakeRoleService.Setup(method => method.DeleteRole(It.IsAny<string>())).ReturnsAsync(new OkObjectResult(new { }));
            var result = await roleController.DeleteRole("fakeRoleID");
            var OkObjectResult = result as OkObjectResult;
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }
    }
}
