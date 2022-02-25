using EG.IdentityManagement.Microservice.Entities.Const;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

        public AuthTokenProvider(IDataProtectionProvider dataProtectionProvider,
                                 IOptions<DataProtectionTokenProviderOptions> options,
                                 ILogger<DataProtectorTokenProvider<TUser>> logger,
                                 IOptions<JwtSettings> jwtSettings)
            : base(dataProtectionProvider, options, logger)
        {
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException("Values should not come as null");
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
                    break;

                case "RefreshToken":
                    break;
            }

            return Task.FromResult(true);
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

            if (manager.IsInRoleAsync(user, Constants.ApplicationRoles.Administrator.ToString()).Result)
                claims.Add(new Claim(ClaimTypes.Role, Constants.ApplicationRoles.Administrator.ToString()));
            else if (manager.IsInRoleAsync(user, Constants.ApplicationRoles.Moderator.ToString()).Result)
                claims.Add(new Claim(ClaimTypes.Role, Constants.ApplicationRoles.Administrator.ToString()));
            else
                claims.Add(new Claim(ClaimTypes.Role, Constants.ApplicationRoles.StandardUser.ToString()));

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

        #endregion "Private Methods"
    }
}