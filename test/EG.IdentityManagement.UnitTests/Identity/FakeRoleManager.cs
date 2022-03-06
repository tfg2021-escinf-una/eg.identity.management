using EG.IdentityManagement.Microservice.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EG.IdentityManagement.UnitTests.Identity
{
    public class FakeRoleManager : RoleManager<Role>
    {
        public FakeRoleManager()
            : base(Mock.Of<IRoleStore<Role>>(),
                   new List<IRoleValidator<Role>>(),
                   Mock.Of<ILookupNormalizer>(),
                   Mock.Of<IdentityErrorDescriber>(),
                   Mock.Of<ILogger<RoleManager<Role>>>())
        {
        }
    }
}
