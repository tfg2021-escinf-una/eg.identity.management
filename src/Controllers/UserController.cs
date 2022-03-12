using AutoMapper;
using EG.IdentityManagement.Microservice.Entities.ViewModels;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public UserController(IIdentityService identityService,
                              IMapper mapper)
        {
            _identityService = identityService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] CredentialsViewModel credentials)
            => await _identityService.Login(credentials.EmailAddress, credentials.Password);

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] IdentityViewModel user)
            => await _identityService.Register(_mapper.Map<User>(user));

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokensViewModel token)
            => await _identityService.Refresh(token.jwtToken, token.refreshToken);
    }
}