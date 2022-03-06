using AspNetCore.Identity.Mongo;
using EG.IdentityManagement.Microservice.Customizations.Identity;
using EG.IdentityManagement.Microservice.Customizations.Providers;
using EG.IdentityManagement.Microservice.Entities.Const;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Health;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Repositories;
using EG.IdentityManagement.Microservice.Services.Implementations;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using EG.IdentityManagement.Microservice.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System;
using System.Text;

namespace EG.IdentityManagement.Microservice.Extensions
{
    public static class DependencyInjectionServicesExtensions
    {
        public static IConfiguration Configuration { set; get; }

        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
        {
            Configuration = services.BuildServiceProvider()
                .GetService<IConfiguration>();

            #region "IOptions Injection"

            services.Configure<MongoDbSettings>(model => Configuration.GetSection("MongoConfig").Bind(model));
            services.Configure<JwtSettings>(model => Configuration.GetSection("Jwt").Bind(model));

            #endregion "IOptions Injection"

            #region "Identity"

            services.AddIdentityMongoDbProvider<User, Role, string>(identity =>
            {
                identity.Password.RequiredLength = 8;
                identity.User.RequireUniqueEmail = true;
                identity.Lockout.MaxFailedAccessAttempts = 3;
                identity.Tokens.AuthenticatorTokenProvider = Constants.AuthTokenProvider;
            }, mongo =>
            {
                mongo.ConnectionString = services.BuildServiceProvider()
                    .GetService<IOptions<MongoDbSettings>>()?.Value
                    .ConnectionString();
            }).AddUserManager<CustomUserManager<User>>()
                .AddTokenProvider<AuthTokenProvider<User>>(Constants.AuthTokenProvider)
                .AddDefaultTokenProviders();

            #endregion "Identity"

            #region "Authentication"

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"])),
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });

            #endregion "Authentication"

            #region "Swagger"

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EG.IdentityManagement.Microservice", Version = "v1" });
            });

            #endregion "Swagger"

            #region "Services"

            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddTransient<IMongoClient, MongoClient>(_ => new MongoClient(services.BuildServiceProvider()
                    .GetService<IOptions<MongoDbSettings>>()?.Value
                    .ConnectionString()));
            services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

            #endregion "Services"

            #region "Health Checks"

            services.AddHealthChecks()
                .AddCheck<MongoHealthCheck>("MongoHealthCheck");

            #endregion

            return services;
        }
    }
}