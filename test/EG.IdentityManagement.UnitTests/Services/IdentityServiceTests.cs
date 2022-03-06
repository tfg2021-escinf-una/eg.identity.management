using EG.IdentityManagement.Microservice.Entities;
using EG.IdentityManagement.Microservice.Entities.Const;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Services.Implementations;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using EG.IdentityManagement.UnitTests.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using opt = Microsoft.AspNetCore.Identity;

namespace EG.IdentityManagement.UnitTests.Services
{
    public class IdentityServiceTests
    {
        private Mock<FakeSignInManager> fakeSignInManager;
        private Mock<FakeUserManager> fakeUserManager;
        private User fakeUser;
        private IIdentityService identityService;

        [SetUp]
        public void Initialize()
        {
            fakeSignInManager = new();
            fakeUserManager = new();
            fakeUser = new();
        }

        [Test]
        public async Task UserNotFound_ShouldReturn404WithErrors()
        {
            fakeUserManager.Setup(method => method.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User) null);
            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            var result = await identityService.Login("fake@fake.com", "afrodita123");
            var notFoundResult = result as NotFoundObjectResult;
            var notFoundValue = notFoundResult.Value as GenericResponse;

            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.That(notFoundValue.Errors, Is.Not.Null);
        }

        [Test]
        public async Task UserLoginSucceeded_ThenGenerateNewJwtAndRefreshToken()
        {
            // Setup 
            fakeUser.JwtToken = new JwtToken { ExpiresAt = System.DateTime.Today };
            fakeUserManager.Setup(method => method.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
            fakeSignInManager.Setup(method => method.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(opt.SignInResult.Success);
            fakeUserManager.Setup(method => method.HasUserAliveJwtToken(It.IsAny<User>())).ReturnsAsync(false);
            fakeUserManager.Setup(method => method.PurgeAuthTokens(It.IsAny<User>())).Verifiable();
            fakeUserManager.Setup(method => method.SetJwtTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.JWT.ToString(), It.IsAny<string>())).Verifiable();
            fakeUserManager.Setup(method => method.SetRefreshTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.JWT.ToString(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            fakeUserManager.Setup(method => method.GenerateUserTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.JWT.ToString())).ReturnsAsync("jsonwebtoken");
            fakeUserManager.Setup(method => method.GenerateUserTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.RefreshToken.ToString())).ReturnsAsync("refreshtoken");
            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            // Run code
            var result = await identityService.Login("fake@fake.com", "afrodita123");

            // Assert
            var OkObjectResult = result as OkObjectResult;
            var OkObjectResultValue = OkObjectResult.Value as GenericResponse;
            dynamic expectedObject = new ExpandoObject();
            expectedObject.jwtToken = "jsonwebtoken";
            expectedObject.refreshToken = "refreshtoken";
            expectedObject.expiresat = fakeUser.JwtToken.ExpiresAt.ToLocalTime();
            
            Assert.That(OkObjectResultValue.Data, Is.EqualTo(expectedObject));
            Assert.That(OkObjectResultValue.Errors, Is.EqualTo(new List<object>()));
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }

        [Test]
        public async Task UserLoginSucceeded_ThenItHasAuthTokensAlive()
        {
            // Setup 
            fakeUser.JwtToken = new JwtToken { ExpiresAt = System.DateTime.Today };
            fakeUserManager.Setup(method => method.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
            fakeSignInManager.Setup(method => method.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(opt.SignInResult.Success);
            fakeUserManager.Setup(method => method.HasUserAliveJwtToken(It.IsAny<User>())).ReturnsAsync(true);
            fakeUserManager.Setup(method => method.GetActiveAuthTokens(It.IsAny<User>())).Returns(
                new Tuple<string, string>(
                    "jsonwebtoken",
                    "refreshtoken"
                ));

            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            // Run code
            var result = await identityService.Login("fake@fake.com", "afrodita123");

            // Assert
            var OkObjectResult = result as OkObjectResult;
            var OkObjectResultValue = OkObjectResult.Value as GenericResponse;
            dynamic expectedObject = new ExpandoObject();
            expectedObject.jwtToken = "jsonwebtoken";
            expectedObject.refreshToken = "refreshtoken";
            expectedObject.expiresat = fakeUser.JwtToken.ExpiresAt.ToLocalTime();

            Assert.That(OkObjectResultValue.Data, Is.EqualTo(expectedObject));
            Assert.That(OkObjectResultValue.Errors, Is.EqualTo(new List<object>()));
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }

        [Test]
        public async Task UserLoginInvalid_ThenItReturnsABadRequestObject()
        {
            // Setup 
            fakeUserManager.Setup(method => method.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
            fakeSignInManager.Setup(method => method.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(opt.SignInResult.Failed);
            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            // Run code
            var result = await identityService.Login("fake@fake.com", "afrodita123");

            // Assert
            var BadRequestObjectResult = result as BadRequestObjectResult;
            var BadRequestObjectResultValue = BadRequestObjectResult.Value as GenericResponse;

            Assert.That(BadRequestObjectResultValue.Data, Is.EqualTo(new ExpandoObject()));
            Assert.That(BadRequestObjectResultValue.Errors, Is.Not.Empty);
            Assert.AreEqual(400, BadRequestObjectResult.StatusCode);

        }

        [Test]
        public async Task UserRegistrationSucceeded_ShouldReturn201WithOutErrors()
        {
            // Setup
            fakeUser.Id = "UnitTestID";
            fakeUserManager.Setup(method => method.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(opt.IdentityResult.Success);
            fakeUserManager.Setup(method => method.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(opt.IdentityResult.Success);
            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            // Run code
            var result = await identityService.Register(fakeUser);

            // Assert

            var CreatedResult = result as CreatedResult;
            var CreatedResultValue = CreatedResult.Value as GenericResponse;
            dynamic ExpectedObject = new ExpandoObject();
            ExpectedObject.IdAssigned = fakeUser.Id;
            ExpectedObject.RoleAssigned = Constants.ApplicationRoles.StandardUser.ToString();

            Assert.That(CreatedResultValue.Data, Is.EqualTo(ExpectedObject));
            Assert.That(CreatedResultValue.Errors, Is.EqualTo(new List<List<IdentityError>>()));
            Assert.AreEqual(201, CreatedResult.StatusCode);
        }

        [Test]
        public async Task UserRegistrationFailed_ShouldReturn400WithErrors()
        {
            // Setup

            var identityError = new IdentityError
            {
                Code = "FAKE400",
                Description = "This is a fake test scenario"
            };

            fakeUser.Id = "UnitTestID";
            fakeUserManager.Setup(method => method.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(opt.IdentityResult.Failed(new IdentityError[] {
                identityError
            }));

            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            // Run code
            var result = await identityService.Register(fakeUser);

            // Assert

            var BadRequestObjectResult = result as BadRequestObjectResult;
            var BadRequestObjectResultValue = BadRequestObjectResult.Value as GenericResponse;

            var ListErrorsExpected = new List<List<IdentityError>>() {
                new List<IdentityError>
                {
                    identityError
                }
            };

            Assert.That(BadRequestObjectResultValue.Data, Is.EqualTo(new ExpandoObject()));
            Assert.That(BadRequestObjectResultValue.Errors, Is.EqualTo((object) ListErrorsExpected));
            Assert.AreEqual(400, BadRequestObjectResult.StatusCode);
        }

        [Test]
        public async Task UserRegistrationSucceeded_ButRoleAssignationFailed_ShouldReturn400WithErrors()
        {
            // Setup

            var identityError = new IdentityError
            {
                Code = "FAKE400",
                Description = "This is a fake test scenario"
            };

            fakeUser.Id = "UnitTestID";
            fakeUserManager.Setup(method => method.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(opt.IdentityResult.Success);
            fakeUserManager.Setup(method => method.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(opt.IdentityResult.Failed(new IdentityError[] { 
                identityError
            }));

            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            // Run code
            var result = await identityService.Register(fakeUser);

            // Assert

            var BadRequestObjectResult = result as BadRequestObjectResult;
            var BadRequestObjectResultValue = BadRequestObjectResult.Value as GenericResponse;
            dynamic ExpectedObject = new ExpandoObject();
            ExpectedObject.IdAssigned = fakeUser.Id;

            var ListErrorsExpected = new List<List<IdentityError>>() {
                new List<IdentityError>
                {
                    identityError
                }
            };

            Assert.That(BadRequestObjectResultValue.Data, Is.EqualTo(ExpectedObject));
            Assert.That(BadRequestObjectResultValue.Errors, Is.EqualTo((object)ListErrorsExpected));
            Assert.AreEqual(400, BadRequestObjectResult.StatusCode);
        }

        [Test]
        public async Task UserNotFoundWhileRefreshingToken_ShouldReturn404WithErrors()
        {
            fakeUserManager.Setup(method => method.GetUserByJwtAsync(It.IsAny<string>())).ReturnsAsync((User)null);
            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            var result = await identityService.Refresh("jwtToken", "refreshToken");
            var notFoundResult = result as NotFoundObjectResult;
            var notFoundValue = notFoundResult.Value as GenericResponse;

            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.That(notFoundValue.Errors, Is.Not.Null);
        }

        [Test]
        public async Task UserRefreshTokenIsExpired_ShouldReturn400WithErrors()
        {
            fakeUserManager.Setup(method => method.GetUserByJwtAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
            fakeUserManager.Setup(method => method.VerifyUserTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.RefreshToken.ToString(), It.IsAny<string>())).ReturnsAsync(false);
            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            var result = await identityService.Refresh("jwtToken", "refreshToken");
            var BadRequestObjectResult = result as BadRequestObjectResult;
            var BadRequestObjectResultValue = BadRequestObjectResult.Value as GenericResponse;

            Assert.That(BadRequestObjectResultValue.Data, Is.InstanceOf(typeof(object)));
            Assert.That(BadRequestObjectResultValue.Errors, Is.Not.Empty);
            Assert.AreEqual(400, BadRequestObjectResult.StatusCode);
        }

        [Test]
        public async Task UserRefreshTokenDoesNotMatchWithTheJwt_ShouldReturn400WithErrors()
        {
            fakeUserManager.Setup(method => method.GetUserByJwtAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
            fakeUserManager.Setup(method => method.VerifyUserTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.RefreshToken.ToString(), It.IsAny<string>())).ReturnsAsync(true);
            fakeUserManager.Setup(method => method.CheckAuthenticityBetweenJwtAndRefreshToken(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            var result = await identityService.Refresh("jwtToken", "refreshToken");
            var BadRequestObjectResult = result as BadRequestObjectResult;
            var BadRequestObjectResultValue = BadRequestObjectResult.Value as GenericResponse;

            Assert.That(BadRequestObjectResultValue.Data, Is.InstanceOf(typeof(object)));
            Assert.That(BadRequestObjectResultValue.Errors, Is.Not.Empty);
            Assert.AreEqual(400, BadRequestObjectResult.StatusCode);
        }

        [Test]
        public async Task UserRefreshTokenSucceeded_ShouldReturn200WithTheNewTokens()
        {
            fakeUser.JwtToken = new JwtToken { ExpiresAt = System.DateTime.Today };
            fakeUserManager.Setup(method => method.GetUserByJwtAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
            fakeUserManager.Setup(method => method.VerifyUserTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.RefreshToken.ToString(), It.IsAny<string>())).ReturnsAsync(true);
            fakeUserManager.Setup(method => method.CheckAuthenticityBetweenJwtAndRefreshToken(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            fakeUserManager.Setup(method => method.GenerateUserTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.JWT.ToString())).ReturnsAsync("jsonwebtoken");
            fakeUserManager.Setup(method => method.GenerateUserTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.RefreshToken.ToString())).ReturnsAsync("refreshtoken");
            fakeUserManager.Setup(method => method.PurgeAuthTokens(It.IsAny<User>())).Verifiable();
            fakeUserManager.Setup(method => method.SetJwtTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.JWT.ToString(), It.IsAny<string>())).Verifiable();
            fakeUserManager.Setup(method => method.SetRefreshTokenAsync(It.IsAny<User>(), Constants.AuthTokenProvider, Constants.TokenPurpose.JWT.ToString(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            identityService = new IdentityService(fakeSignInManager.Object, fakeUserManager.Object);

            var result = await identityService.Refresh("jwtToken", "refreshToken");
            
            // Assert
            var OkObjectResult = result as OkObjectResult;
            var OkObjectResultValue = OkObjectResult.Value as GenericResponse;
            dynamic expectedObject = new ExpandoObject();
            expectedObject.jwtToken = "jsonwebtoken";
            expectedObject.refreshToken = "refreshtoken";
            expectedObject.expiresat = fakeUser.JwtToken.ExpiresAt.ToLocalTime();

            Assert.That(OkObjectResultValue.Data, Is.EqualTo(expectedObject));
            Assert.That(OkObjectResultValue.Errors, Is.EqualTo(new List<object>()));
            Assert.AreEqual(200, OkObjectResult.StatusCode);
        }

    }
    
}
