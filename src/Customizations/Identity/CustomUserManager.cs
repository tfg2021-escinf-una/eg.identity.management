using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Customizations.Identity
{
    public class CustomUserManager<TUser> : UserManager<TUser>
        where TUser : User

    {
        private readonly ILogger<UserManager<TUser>> _logger;
        private readonly JwtSettings _jwtSettings;

        public CustomUserManager(IUserStore<TUser> store,
                                IOptions<IdentityOptions> optionsAccessor,
                                IPasswordHasher<TUser> passwordHasher,
                                IEnumerable<IUserValidator<TUser>> userValidators,
                                IEnumerable<IPasswordValidator<TUser>> passwordValidators,
                                ILookupNormalizer keyNormalizer,
                                IdentityErrorDescriber errors,
                                IServiceProvider services,
                                ILogger<UserManager<TUser>> logger,
                                IOptions<JwtSettings> jwtSettings)
           : base(store,
                  optionsAccessor,
                  passwordHasher,
                  userValidators,
                  passwordValidators,
                  keyNormalizer,
                  errors,
                  services,
                  logger)
        {
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException("Jwt settings should not come as null");
            _logger = logger;
        }

        public Task<string> SetJwtTokenAsync(TUser user, string loginProvider, string name, string token)
        {
            var securityToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            JwtToken jwtToken = new JwtToken
            {
                Id = Guid.NewGuid().ToString(),
                IssuedAt = securityToken.ValidFrom,
                ExpiresAt = securityToken.ValidTo,
                Value = token,
                LoginProvider = loginProvider,
                UserId = user.Id.ToString(),
                Name = name
            };

            user.JwtTokens.Add(jwtToken);
            return Task.FromResult(jwtToken.Id);
        }

        public Task<string> SetRefreshTokenAsync(TUser user, string loginProvider, string name, string jwtId, string value)
        {
            RefreshToken refreshToken = new RefreshToken
            {
                IssuedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(_jwtSettings.RefreshTokenExpiresIn),
                Value = value,
                Used = false,
                LoginProvider = loginProvider,
                UserId = user.Id.ToString(),
                Name = name
            };

            user.RefreshTokens.Add(refreshToken);
            return Task.FromResult(refreshToken.Id);
        }
    }
}