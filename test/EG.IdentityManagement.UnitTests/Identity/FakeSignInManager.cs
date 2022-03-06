using EG.IdentityManagement.Microservice.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EG.IdentityManagement.UnitTests.Identity
{
    public class FakeSignInManager : SignInManager<User>
    {   
        public FakeSignInManager() 
            : base(new FakeUserManager(),
                  Mock.Of<IHttpContextAccessor>(),
                  Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                  Mock.Of<IOptions<IdentityOptions>>(),
                  Mock.Of<ILogger<SignInManager<User>>>(),
                  Mock.Of<IAuthenticationSchemeProvider>(),
                  Mock.Of<IUserConfirmation<User>>())
        {
        }
    }
}
