using EG.IdentityManagement.Microservice.Entities.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IActionResult> CreateRole(Role role);

        Task<IActionResult> UpdateRole(string roleId, Role role);

        Task<IActionResult> DeleteRole(string roleId);

        IActionResult GetRoles();

        Task<IActionResult> GetRoleById(string roleId);
    }
}