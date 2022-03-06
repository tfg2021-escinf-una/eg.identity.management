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
            var errors = new List<object>();
            string jwtToken = string.Empty;
            string refreshToken = string.Empty;

            using (var user = await _userManager.FindByEmailAsync(email))
            {
                if (user == null)
                    goto UserNotFound;

                if ((await _signInManager.PasswordSignInAsync(user, password, true, true)).Succeeded)
                {
                    if (!await _userManager.HasUserAliveJwtToken(user))
                    {
                        jwtToken = await _userManager.GenerateUserTokenAsync(user,
                                 Constants.AuthTokenProvider,
                                 Constants.TokenPurpose.JWT.ToString());

                        refreshToken = await _userManager.GenerateUserTokenAsync(user,
                                 Constants.AuthTokenProvider,
                                 Constants.TokenPurpose.RefreshToken.ToString());

                        await _userManager.PurgeAuthTokens(user);

                        string tokenId = await _userManager.SetJwtTokenAsync(user,
                                Constants.AuthTokenProvider,
                                Constants.TokenPurpose.JWT.ToString(),
                                jwtToken);

                        _ = await _userManager.SetRefreshTokenAsync(user,
                                Constants.AuthTokenProvider,
                                Constants.TokenPurpose.RefreshToken.ToString(),
                                tokenId,
                                refreshToken);
                    }
                    else
                    {
                        var tupleTokens = _userManager.GetActiveAuthTokens(user);
                        jwtToken = tupleTokens.Item1;
                        refreshToken = tupleTokens.Item2;
                    }

                    data.jwtToken = jwtToken;
                    data.refreshToken = refreshToken;
                    data.expiresat = user.JwtToken.ExpiresAt
                        .ToLocalTime();

                    return new OkObjectResult(new GenericResponse
                    {
                        Data = data,
                        Errors = errors,
                        StatusCode = HttpStatusCode.OK
                    });
                }
                else
                {
                    goto InvalidPassword;
                }
            }

            UserNotFound:
                errors.Add(new { Description = "Provided email address does not match with any of our users." });
                return new NotFoundObjectResult(new GenericResponse
                {
                    Data = data,
                    Errors = errors,
                    StatusCode = HttpStatusCode.NotFound
                });

            InvalidPassword:
                errors.Add(new { Description = "User has provided an invalid password" });
                return new BadRequestObjectResult(new GenericResponse
                {
                    Data = data,
                    Errors = errors,
                    StatusCode = HttpStatusCode.BadRequest
                });
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
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            return new CreatedResult("/", new GenericResponse
            {
                Errors = errors,
                Data = data,
                StatusCode = HttpStatusCode.Created
            });
        }

        public async Task<IActionResult> Refresh(string jwtToken, string refreshToken)
        {
            dynamic data = new ExpandoObject();
            var errors = new List<object>();
            using (var user = await _userManager.GetUserByJwtAsync(jwtToken))
            {
                if (user == null)
                    goto UserNotFound;

                if (await _userManager.VerifyUserTokenAsync(user, Constants.AuthTokenProvider, Constants.TokenPurpose.RefreshToken.ToString(), refreshToken))
                {
                    if (await _userManager.CheckAuthenticityBetweenJwtAndRefreshToken(user, jwtToken, refreshToken))
                    {
                        jwtToken = await _userManager.GenerateUserTokenAsync(user,
                              Constants.AuthTokenProvider,
                              Constants.TokenPurpose.JWT.ToString());

                        refreshToken = await _userManager.GenerateUserTokenAsync(user,
                              Constants.AuthTokenProvider,
                              Constants.TokenPurpose.RefreshToken.ToString());

                        await _userManager.PurgeAuthTokens(user);

                        var tokenId = await _userManager.SetJwtTokenAsync(user,
                                                            Constants.AuthTokenProvider,
                                                            Constants.TokenPurpose.JWT.ToString(),
                                                            jwtToken);

                        _ = await _userManager.SetRefreshTokenAsync(user,
                                Constants.AuthTokenProvider,
                                Constants.TokenPurpose.RefreshToken.ToString(),
                                tokenId,
                                refreshToken);

                        data.jwtToken = jwtToken;
                        data.refreshToken = refreshToken;
                        data.expiresat = user.JwtToken.ExpiresAt
                            .ToLocalTime();

                        return new OkObjectResult(new GenericResponse
                        {
                            Data = data,
                            Errors = errors,
                            StatusCode = HttpStatusCode.OK
                        });
                    }

                    goto JwtAndRefreshAreNotMatching;
                }

                goto RefreshTokenExpired;
            }

            UserNotFound:
                errors.Add(new { Description = "Provided JwtToken does not match with any user" });
                return new NotFoundObjectResult(new GenericResponse
                {
                    Data = new object(),
                    Errors = errors,
                    StatusCode = HttpStatusCode.NotFound
                });

            RefreshTokenExpired:
                errors.Add(new { Description = "Refresh Token is expired" });
                goto FinalReturn;

            JwtAndRefreshAreNotMatching:
                errors.Add(new { Description = "Jwt and Refresh tokens are not matching for the provided user" });
                goto FinalReturn;    

            FinalReturn:
                return new BadRequestObjectResult(new GenericResponse
                {
                    Data = new object(),
                    Errors = errors,
                    StatusCode = HttpStatusCode.BadRequest
                });
        }
    }
}