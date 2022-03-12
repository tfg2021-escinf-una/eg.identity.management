using AutoMapper;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Entities.ViewModels;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public RoleController(IRoleService roleService,
                              IMapper mapper)
        {
            _roleService = roleService ?? throw new ArgumentNullException("Injected properties should not come as null");
            _mapper = mapper ?? throw new ArgumentNullException("Mapper should not come as null");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleViewModel role)
            => await _roleService.CreateRole(_mapper.Map<Role>(role));

        [HttpGet]
        public IActionResult GetRoles() => _roleService.GetRoles();

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
            => await _roleService.GetRoleById(id);

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleViewModel role)
            => await _roleService.UpdateRole(id, _mapper.Map<Role>(role));

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
            => await _roleService.DeleteRole(id);
    }
}