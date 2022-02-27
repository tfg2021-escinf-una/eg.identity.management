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
        private readonly IMongoRepository<JwtToken> _jwtRepository;
        private readonly IMongoRepository<RefreshToken> _refreshRepository;
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
                                 IMongoRepository<JwtToken> jwtRepository,
                                 IMongoRepository<RefreshToken> refreshRepository,
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
            _jwtRepository = jwtRepository;
            _userRepository = userRepository;
            _refreshRepository = refreshRepository;
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
                Id = Guid.NewGuid().ToString(),
                JwtId = securityToken.Id,
                IssuedAt = securityToken.ValidFrom,
                ExpiresAt = securityToken.ValidTo,
                Value = token,
                LoginProvider = loginProvider,
                UserId = user.Id.ToString(),
                Name = name
            };

            if (await _jwtRepository.InsertOneAsync(jwtToken))
            {
                user.JwtTokens.Add(jwtToken.Id);
                _ = await _userRepository.FindOneAndUpdateAsync(
                    Builders<User>.Filter.Where(item => item.Id == user.Id),
                    Builders<User>.Update
                        .Set(prop => prop.JwtTokens, user.JwtTokens));

                return jwtToken.Id;
            }

            return string.Empty;
        }

        public async Task<string> SetRefreshTokenAsync(TUser user,
                                                 string loginProvider,
                                                 string name,
                                                 string jwtId,
                                                 string value)
        {
            RefreshToken refreshToken = new RefreshToken
            {
                IssuedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(_jwtSettings.RefreshTokenExpiresIn),
                Value = value,
                Used = false,
                LoginProvider = loginProvider,
                UserId = user.Id.ToString(),
                Name = name,
                JwtId = jwtId
            };

            if (await _refreshRepository.InsertOneAsync(refreshToken))
            {
                user.RefreshTokens.Add(refreshToken.Id);
                _ = await _userRepository.FindOneAndUpdateAsync(
                       Builders<User>.Filter.Where(item => item.Id == user.Id),
                       Builders<User>.Update
                           .Set(prop => prop.RefreshTokens, user.RefreshTokens));

                return refreshToken.Id;
            }

            return string.Empty;
        }

        public async Task<bool> HasUserAliveJwtToken(TUser user)
        {
            var result = await _userRepository.FindOneLookupAsync<User, JwtToken, UserTokenLookup>(
                "JwtTokens",
                "_id",
                "UserId",
                "jwtTokens",
                Builders<User>.Filter.Where(item => item.Id == user.Id));

            foreach (var token in result.jwtTokens)
            {
                if (token.ExpiresAt.ToLocalTime() > DateTime.Now)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task PurgeAuthTokens(TUser user)
        {
            if (user.JwtTokens.Count > 0)
            {
                _ = await _jwtRepository.DeleteOneAsync(
                        Builders<JwtToken>.Filter.Where(item => item.Id == user.JwtTokens[0])
                    );
            }

            if (user.RefreshTokens.Count > 0)
            {
                _ = await _refreshRepository.DeleteOneAsync(
                         Builders<RefreshToken>.Filter.Where(item => item.Id == user.RefreshTokens[0])
                    );
            }

            _ = await _userRepository.FindOneAndUpdateAsync(
                Builders<User>.Filter.Where(item => item.Id == user.Id),
                Builders<User>.Update
                    .Set(prop => prop.JwtTokens, new List<string>())
                    .Set(prop => prop.RefreshTokens, new List<string>())
            );

            user.JwtTokens.Clear();
            user.RefreshTokens.Clear();
        }

        public Tuple<string, string> GetActiveAuthTokens(TUser user)
            => new Tuple<string, string>(
                    _jwtRepository.Find(Builders<JwtToken>.Filter.Where(item => item.Id == user.JwtTokens[0])).Value,
                    _refreshRepository.Find(Builders<RefreshToken>.Filter.Where(item => item.Id == user.RefreshTokens[0])).Value
                );
    }

    public class UserTokenLookup : User
    {
        public List<JwtToken> jwtTokens { set; get; }
    }
}