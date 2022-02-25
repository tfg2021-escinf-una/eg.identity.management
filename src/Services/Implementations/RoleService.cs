using EG.IdentityManagement.Microservice.Entities;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<Role> _roleManager;

        public RoleService(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IActionResult> CreateRole(Role role)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name))
            {
                if ((await _roleManager.CreateAsync(role)).Succeeded)
                {
                    return new CreatedResult("/", new GenericResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.Created,
                        Data = role,
                    });
                }
            }

            return new ConflictObjectResult(new GenericResponse
            {
                Data = new { },
                StatusCode = System.Net.HttpStatusCode.Conflict,
                Errors = new
                {
                    Description = "Role already exists"
                }
            });
        }

        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role != null)
            {
                if ((await _roleManager.DeleteAsync(role)).Succeeded)
                {
                    return new OkObjectResult(new GenericResponse
                    {
                        Data = role,
                        StatusCode = System.Net.HttpStatusCode.OK,
                    });
                }

                return new BadRequestObjectResult(new GenericResponse
                {
                    Data = role,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Errors = new object[]
                    {
                        new { Description = "Could not delete the role" }
                    }
                });
            }

            return new NotFoundObjectResult(new GenericResponse
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            });
        }

        public async Task<IActionResult> UpdateRole(string roleId, Role role)
        {
            var roleToUpdate = await _roleManager.FindByIdAsync(roleId);

            if (roleToUpdate != null)
            {
                roleToUpdate.Name = role.Name;

                if ((await _roleManager.UpdateAsync(roleToUpdate)).Succeeded)
                {
                    return new OkObjectResult(new GenericResponse
                    {
                        Data = role,
                        StatusCode = System.Net.HttpStatusCode.OK,
                    });
                }

                return new BadRequestObjectResult(new GenericResponse
                {
                    Data = role,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Errors = new object[]
                    {
                        new { Description = "Could not update the role" }
                    }
                });
            }

            return new NotFoundObjectResult(new GenericResponse
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            });
        }

        public async Task<IActionResult> GetRoleById(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role != null)
            {
                return new OkObjectResult(new GenericResponse
                {
                    Data = role,
                    StatusCode = System.Net.HttpStatusCode.OK,
                });
            }

            return new NotFoundObjectResult(new GenericResponse
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            });
        }

        public IActionResult GetRoles()
        {
            return new OkObjectResult(new GenericResponse
            {
                Data = _roleManager.Roles.ToList(),
                StatusCode = System.Net.HttpStatusCode.OK,
            });
        }
    }
}