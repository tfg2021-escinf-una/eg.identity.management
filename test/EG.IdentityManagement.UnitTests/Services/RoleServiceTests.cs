using EG.IdentityManagement.Microservice.Entities;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Services.Implementations;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using EG.IdentityManagement.UnitTests.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EG.IdentityManagement.UnitTests.Services
{
    public class RoleServiceTests
    {
        private Mock<FakeRoleManager> fakeRoleManager;
        private IRoleService roleService;
        private List<Role> listRole;
        private Role fakeRole;

        [SetUp]
        public void Initialize()
        {
            fakeRole = new();
            fakeRoleManager = new();
            listRole = new()
            {
                new Role { Name = "Administrator" },
                new Role { Name = "StandardUser" },
                new Role { Name = "Moderator" },
            };
        }

        [Test]
        public void GetAllRoles_ShouldReturn200AndAListOfRoles()
        {
            fakeRoleManager.Setup(method => method.Roles)
                           .Returns(listRole.AsQueryable());

            roleService = new RoleService(fakeRoleManager.Object);

            var result = roleService.GetRoles();

            // Assert

            var OkObjectResult = result as OkObjectResult;
            var OkObjectResultValue = OkObjectResult.Value as GenericResponse;

            Assert.That(OkObjectResultValue.Data, Is.EqualTo(listRole));
            Assert.AreEqual(200, OkObjectResult.StatusCode);

        }

        [Test]
        public async Task GetRoleById_ShouldReturn200AndTheRole()
        {
            fakeRoleManager.Setup(method => method.FindByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync(fakeRole);

            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.GetRoleById("fakeID");

            // Assert

            var OkObjectResult = result as OkObjectResult;
            var OkObjectResultValue = OkObjectResult.Value as GenericResponse;

            Assert.That(OkObjectResultValue.Data, Is.EqualTo(fakeRole));
            Assert.AreEqual(200, OkObjectResult.StatusCode);

        }

        [Test]
        public async Task GetRoleById_ShouldReturn404NotFound()
        {
            fakeRoleManager.Setup(method => method.FindByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync((Role) null);

            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.GetRoleById("fakeID");

            // Assert

            var NotFoundObjectResult = result as NotFoundObjectResult;
            Assert.AreEqual(404, NotFoundObjectResult.StatusCode);

        }
    }
}
