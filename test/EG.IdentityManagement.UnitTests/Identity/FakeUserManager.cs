using EG.IdentityManagement.Microservice.Customizations.Identity;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Repositories;
using EG.IdentityManagement.Microservice.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using opt = Microsoft.Extensions.Options;
using Moq;
using System;

namespace EG.IdentityManagement.UnitTests.Identity
{
    public class FakeUserManager : CustomUserManager<User>
    {
        public FakeUserManager()
             : base(new Mock<IUserStore<User>>().Object,
                      new Mock<opt.IOptions<IdentityOptions>>().Object,
                      new Mock<IPasswordHasher<User>>().Object,
                      new IUserValidator<User>[0],
                      new IPasswordValidator<User>[0],
                      new Mock<ILookupNormalizer>().Object,
                      new Mock<IdentityErrorDescriber>().Object,
                      new Mock<IServiceProvider>().Object,
                      new Mock<ILogger<UserManager<User>>>().Object,
                      opt.Options.Create(new JwtSettings()),
                      new Mock<IMongoRepository<User>>().Object)
        {
        }
    }
}
