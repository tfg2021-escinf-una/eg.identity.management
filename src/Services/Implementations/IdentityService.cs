using EG.IdentityManagement.Microservice.Customizations.Identity;
using EG.IdentityManagement.Microservice.Entities;
using EG.IdentityManagement.Microservice.Entities.Const;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EG.IdentityManagement.Microservice.Services.Implementations
{
    public class IdentityService : IIdentityService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly CustomUserManager<User> _userManager;

        public IdentityService(SignInManager<User> signInManager,
                               CustomUserManager<User> userManager)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException("SignInManager property should not come as null");
            _userManager = userManager ?? throw new ArgumentNullException("UserManager property should not come as null");
        }

        public async Task<IActionResult> Login(string email,
                                               string password)
        {
            dynamic data = new ExpandoObject();
            var errors = new List<List<object>>();

            var providedUser = await _userManager.FindByEmailAsync(email);

            if (providedUser != null)
            {
                var loggingResult = await _signInManager.PasswordSignInAsync(providedUser,
                                                                             password,
                                                                             true,
                                                                             true);

                if (loggingResult.Succeeded)
                {
                    string jwtToken = await _userManager.GenerateUserTokenAsync(providedUser,
                                                                                Constants.AuthTokenProvider,
                                                                                Constants.TokenPurpose.JWT.ToString());

                    string refreshToken = await _userManager.GenerateUserTokenAsync(providedUser,
                                                                                    Constants.AuthTokenProvider,
                                                                                    Constants.TokenPurpose.RefreshToken.ToString());

                    if (!string.IsNullOrEmpty(jwtToken) &&
                       !string.IsNullOrEmpty(refreshToken))
                    {
                        string tokenId = await _userManager.SetJwtTokenAsync(providedUser,
                                                            Constants.AuthTokenProvider,
                                                            Constants.TokenPurpose.JWT.ToString(),
                                                            jwtToken);

                        await _userManager.SetRefreshTokenAsync(providedUser,
                                                                Constants.AuthTokenProvider,
                                                                Constants.TokenPurpose.RefreshToken.ToString(),
                                                                tokenId,
                                                                refreshToken);

                        data.jwtToken = jwtToken;
                        data.refreshToken = refreshToken;

                        return new OkObjectResult(new GenericResponse
                        {
                            Data = data,
                            Errors = errors,
                            StatusCode = HttpStatusCode.OK
                        });
                    }
                }
            }

            return new OkObjectResult(new { });
        }

        public async Task<IActionResult> Register(User user)
        {
            dynamic data = new ExpandoObject();
            var errors = new List<List<IdentityError>>();
            var registerResult = await _userManager.CreateAsync(user, user.Password);

            if (registerResult.Succeeded)
            {
                data.IdAssigned = user.Id;
                var roleResult = await _userManager.AddToRoleAsync(user, Constants.ApplicationRoles.StandardUser.ToString());

                if (roleResult.Succeeded)
                    data.RoleAssigned = Constants.ApplicationRoles.StandardUser.ToString();
                else
                    errors.Add(roleResult.Errors.ToList());
            }
            else
                errors.Add(registerResult.Errors.ToList());

            if (errors.Count != 0)
            {
                return new BadRequestObjectResult(new GenericResponse
                {
                    Errors = errors,
                    Data = data,
                    StatusCode = HttpStatusCode.Created
                });
            }

            return new CreatedResult("/", new GenericResponse
            {
                Errors = errors,
                Data = data,
                StatusCode = HttpStatusCode.Created
            });
        }
    }
}