using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Repositories;
using EG.IdentityManagement.Microservice.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Customizations.Identity
{
    public class CustomUserManager<TUser> : UserManager<TUser>
        where TUser : User
    {
        private readonly IMongoRepository<User> _userRepository;
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
                                 IOptions<JwtSettings> jwtSettings,
                                 IMongoRepository<User> userRepository)
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
            _userRepository = userRepository ?? throw new ArgumentNullException("Repository should not come as null");
            _logger = logger;
        }

        public async Task<string> SetJwtTokenAsync(TUser user,
                                                   string loginProvider,
                                                   string name,
                                                   string token)
        {
            var securityToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            JwtToken jwtToken = new JwtToken
            {
                Id = securityToken.Id,
                IssuedAt = securityToken.ValidFrom,
                ExpiresAt = securityToken.ValidTo,
                Value = token,
                LoginProvider = loginProvider,
                Name = name
            };

            user.JwtToken = jwtToken;
            _ = await _userRepository.FindOneAndUpdateAsync(
                    Builders<User>.Filter.Where(item => item.Id == user.Id),
                    Builders<User>.Update
                        .Set(prop => prop.JwtToken, user.JwtToken));

            return jwtToken.Id;
        }

        public async Task<string> SetRefreshTokenAsync(TUser user,
                                                 string loginProvider,
                                                 string name,
                                                 string jwtId,
                                                 string value)
        {
            RefreshToken refreshToken = new RefreshToken
            {
                JwtId = jwtId,
                IssuedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(_jwtSettings.RefreshTokenExpiresIn),
                Value = value,
                Used = false,
                LoginProvider = loginProvider,
                Name = name
            };

            user.RefreshToken = refreshToken;
            _ = await _userRepository.FindOneAndUpdateAsync(
                      Builders<User>.Filter.Where(item => item.Id == user.Id),
                      Builders<User>.Update
                          .Set(prop => prop.RefreshToken, user.RefreshToken));

            return refreshToken.JwtId;
        }

        public Task<bool> HasUserAliveJwtToken(TUser user)
        {
            if (user.JwtToken != null && user.RefreshToken != null)
            {
                using (var token = user.JwtToken)
                {
                    if (token.ExpiresAt.ToLocalTime() > DateTime.Now)
                    {
                        return Task.FromResult(true);
                    }
                }
            }

            return Task.FromResult(false);
        }

        public async Task PurgeAuthTokens(TUser user)
        {
            if (user.JwtToken != null || user.RefreshToken != null)
            {
                _ = await _userRepository.FindOneAndUpdateAsync(
                    Builders<User>.Filter.Where(item => item.Id == user.Id),
                    Builders<User>.Update
                        .Set(prop => prop.JwtToken, null)
                        .Set(prop => prop.RefreshToken, null)
                );

                user.JwtToken = null;
                user.RefreshToken = null;
            }
        }

        public Tuple<string, string> GetActiveAuthTokens(TUser user)
            => new Tuple<string, string>(
                    user.JwtToken?.Value,
                    user.RefreshToken?.Value
                );

        public Task<User> GetUserByJwtAsync(string jwtToken)
            => Task.FromResult(_userRepository.Find(
                    Builders<User>.Filter.Where(item => item.JwtToken.Value == jwtToken)
                ));

        public Task<bool> CheckAuthenticityBetweenJwtAndRefreshToken(TUser user, string jwtToken, string refreshToken)
            => Task.FromResult(user.JwtToken.Value == jwtToken &&
                    user.RefreshToken.Value == refreshToken &&
                    user.RefreshToken.JwtId == user.JwtToken.Id);
    }
}