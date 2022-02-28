using EG.IdentityManagement.Microservice.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<IActionResult> Register(User user);

        Task<IActionResult> Login(string email, string password);

        Task<IActionResult> Refresh(string jwtToken, string refreshToken);
    }
}