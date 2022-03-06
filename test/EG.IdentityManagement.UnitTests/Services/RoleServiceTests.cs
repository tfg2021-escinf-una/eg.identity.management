using EG.IdentityManagement.Microservice.Entities;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Services.Implementations;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using EG.IdentityManagement.UnitTests.Identity;
using Microsoft.AspNetCore.Identity;
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

        [Test]
        public async Task CreateRole_ShouldReturn201()
        {
            fakeRoleManager.Setup(method => method.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            fakeRoleManager.Setup(method => method.CreateAsync(It.IsAny<Role>())).ReturnsAsync(IdentityResult.Success);
            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.CreateRole(fakeRole);

            var ObjectResult = result as CreatedResult;
            var ObjectResultValue = ObjectResult.Value as GenericResponse;

            Assert.That(ObjectResultValue.Data, Is.EqualTo(fakeRole));
            Assert.AreEqual(201, ObjectResult.StatusCode);

        }

        [Test]
        public async Task CreateRoleThatAlreadyExists_ShouldReturn409Conflict()
        {
            fakeRoleManager.Setup(method => method.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.CreateRole(fakeRole);

            var ObjectResult = result as ConflictObjectResult;
            var ObjectResultValue = ObjectResult.Value as GenericResponse;

            Assert.That(ObjectResultValue.Data, Is.InstanceOf(typeof(object)));
            Assert.That(ObjectResultValue.Errors, Is.Not.Null);
            Assert.AreEqual(409, ObjectResult.StatusCode);

        }

        [Test]
        public async Task DeleteRoleThatAlreadyExists_ShouldReturn200OK()
        {
            fakeRoleManager.Setup(method => method.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(fakeRole);
            fakeRoleManager.Setup(method => method.DeleteAsync(It.IsAny<Role>())).ReturnsAsync(IdentityResult.Success);
            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.DeleteRole("fakeRoleId");

            // Assert

            var OkObjectResult = result as OkObjectResult;
            var OkObjectResultValue = OkObjectResult.Value as GenericResponse;

            Assert.That(OkObjectResultValue.Data, Is.EqualTo(fakeRole));
            Assert.AreEqual(200, OkObjectResult.StatusCode);

        }

        [Test]
        public async Task DeleteRoleButErrorReceivedByRoleManager_ShouldReturn400BadRequest()
        {
            fakeRoleManager.Setup(method => method.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(fakeRole);
            fakeRoleManager.Setup(method => method.DeleteAsync(It.IsAny<Role>())).ReturnsAsync(IdentityResult.Failed());
            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.DeleteRole("fakeRoleId");

            // Assert

            var BadRequestObjectResult = result as BadRequestObjectResult;
            var BadRequestObjectResultValue = BadRequestObjectResult.Value as GenericResponse;

            Assert.That(BadRequestObjectResultValue.Data, Is.EqualTo(fakeRole));
            Assert.That(BadRequestObjectResultValue.Errors, Is.Not.Empty);
            Assert.AreEqual(400, BadRequestObjectResult.StatusCode);

        }

        [Test]
        public async Task DeleteRoleThatNotExists_ShouldReturn404NotFound()
        {
            fakeRoleManager.Setup(method => method.FindByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync((Role)null);

            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.DeleteRole("fakeID");

            // Assert

            var NotFoundObjectResult = result as NotFoundObjectResult;
            Assert.AreEqual(404, NotFoundObjectResult.StatusCode);

        }

        [Test]
        public async Task UpdateRoleThatNotExists_ShouldReturn404NotFound()
        {
            fakeRoleManager.Setup(method => method.FindByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync((Role)null);

            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.UpdateRole("fakeID", fakeRole);

            // Assert

            var NotFoundObjectResult = result as NotFoundObjectResult;
            Assert.AreEqual(404, NotFoundObjectResult.StatusCode);

        }

        [Test]
        public async Task UpdateRoleThatExists_ShouldReturn200OK()
        {
            fakeRoleManager.Setup(method => method.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(fakeRole);
            fakeRoleManager.Setup(method => method.UpdateAsync(fakeRole)).ReturnsAsync(IdentityResult.Success);

            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.UpdateRole("fakeID", fakeRole);

            // Assert

            var OkObjectResult = result as OkObjectResult;
            var OkObjectResultValue = OkObjectResult.Value as GenericResponse;

            Assert.That(OkObjectResultValue.Data, Is.EqualTo(fakeRole));
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }

        [Test]
        public async Task UpdateRoleButErrorReceivedByRoleManager_ShouldReturn400BadRequest()
        {
            fakeRoleManager.Setup(method => method.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(fakeRole);
            fakeRoleManager.Setup(method => method.UpdateAsync(It.IsAny<Role>())).ReturnsAsync(IdentityResult.Failed());
            roleService = new RoleService(fakeRoleManager.Object);

            var result = await roleService.UpdateRole("fakeRoleId", fakeRole);

            // Assert

            var BadRequestObjectResult = result as BadRequestObjectResult;
            var BadRequestObjectResultValue = BadRequestObjectResult.Value as GenericResponse;

            Assert.That(BadRequestObjectResultValue.Data, Is.EqualTo(fakeRole));
            Assert.That(BadRequestObjectResultValue.Errors, Is.Not.Empty);
            Assert.AreEqual(400, BadRequestObjectResult.StatusCode);

        }
    }
}
