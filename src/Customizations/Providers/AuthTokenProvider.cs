using EG.IdentityManagement.Microservice.Entities.Const;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Repositories;
using EG.IdentityManagement.Microservice.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Customizations.Providers
{
    public class AuthTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
        where TUser : User
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IMongoRepository<Role> _roleRepository;

        public AuthTokenProvider(IDataProtectionProvider dataProtectionProvider,
                                 IOptions<DataProtectionTokenProviderOptions> options,
                                 ILogger<DataProtectorTokenProvider<TUser>> logger,
                                 IOptions<JwtSettings> jwtSettings,
                                 IMongoRepository<RefreshToken> refreshRepository,
                                 IMongoRepository<Role> roleRepository)
            : base(dataProtectionProvider, options, logger)
        {
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException("Values should not come as null");
            _roleRepository = roleRepository ?? throw new ArgumentNullException("Values should not come as null");
        }

        //
        // Summary:
        //     Generates a protected token for the specified user as an asynchronous operation.
        //
        // Parameters:
        //   purpose:
        //     The purpose the token will be used for.
        //
        //   manager:
        //     The Microsoft.AspNetCore.Identity.UserManager`1 to retrieve user properties from.
        //
        //   user:
        //     The TUser the token will be generated from.
        //
        // Returns:
        //     A System.Threading.Tasks.Task`1 representing the generated token.
        public override Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            string tokenValue = string.Empty;

            switch (purpose)
            {
                case "JWT":
                    JwtSecurityToken jwtToken = GenerateJwtSecurityToken(user, manager);
                    tokenValue = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                    break;

                case "RefreshToken":
                    tokenValue = GenerateRefreshToken();
                    break;

                default:
                    break;
            }

            return Task.FromResult(tokenValue);
        }

        //
        // Summary:
        //     Validates the protected token for the specified user and purpose as an asynchronous
        //     operation.
        //
        // Parameters:
        //   purpose:
        //     The purpose the token was be used for.
        //
        //   token:
        //     The token to validate.
        //
        //   manager:
        //     The Microsoft.AspNetCore.Identity.UserManager`1 to retrieve user properties from.
        //
        //   user:
        //     The TUser the token was generated for.
        //
        // Returns:
        //     A System.Threading.Tasks.Task`1 that represents the result of the asynchronous
        //     validation, containing true if the token is valid, otherwise false.

        public override Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            switch (purpose)
            {
                case "JWT":
                    SecurityToken validatedToken;
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = GetTokenValidationParameters(securityKey);

                    try
                    {
                        tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                    }
                    catch (Exception)
                    {
                        return Task.FromResult(false);
                    }

                    return Task.FromResult(true);

                case "RefreshToken":

                    if (user.RefreshToken != null)
                    {
                        using (var refreshToken = user.RefreshToken)
                        {
                            if (refreshToken.ExpiresAt.ToLocalTime() < DateTime.Now
                                && refreshToken.Used)
                                return Task.FromResult(false);

                            return Task.FromResult(true);
                        }
                    }

                    return Task.FromResult(false);

                default:
                    return Task.FromResult(false);
            }
        }

        #region "Private Methods"

        /// <summary>
        /// This method is used to generate a JwtSecurityToken object, it will create it based on the user
        /// claims.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A JwtSecurityToken object.</returns>
        private JwtSecurityToken GenerateJwtSecurityToken(TUser user,
                                                          UserManager<TUser> manager)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var expiresIn = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresIn);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.NameIdentifier, "" + user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(Constants.UserName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }.ToList();

            var systemAvailableRoles = _roleRepository.Find(_ => true)
                                            .Result
                                            .ToList();

            if (systemAvailableRoles.Count > 0)
            {
                systemAvailableRoles.ForEach(item =>
                {
                    if (manager.IsInRoleAsync(user, item.Name).Result)
                        claims.Add(new Claim(ClaimTypes.Role, item.Name));
                });
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, Constants.ApplicationRoles.Administrator.ToString()));
            }

            return new JwtSecurityToken(_jwtSettings.Issuer,
                                         _jwtSettings.Audience,
                                         claims,
                                         notBefore: DateTime.UtcNow,
                                         expires: expiresIn,
                                         signingCredentials: credentials);
        }

        /// <summary>
        /// This method generates a RefreshToken, it helps us in case the JWT expired. If it is still alive
        /// the refresh token will generate a new JWT to continue navigating on the web page.
        /// </summary>
        /// <returns></returns>
        private string GenerateRefreshToken()
        {
            var refreshToken = string.Empty;
            var randomNumber = new byte[32];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                refreshToken = Convert.ToBase64String(randomNumber);
            }

            return refreshToken;
        }

        private TokenValidationParameters GetTokenValidationParameters(SymmetricSecurityKey securityKey)
            => new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = securityKey,
                RequireExpirationTime = true,
                LifetimeValidator = (DateTime? notBefore,
                                     DateTime? expires,
                                     SecurityToken securityToken,
                                     TokenValidationParameters validationParameters) => expires >= DateTime.UtcNow
            };

        #endregion "Private Methods"
    }
}