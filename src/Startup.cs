using AspNetCore.Identity.Mongo;
using EG.IdentityManagement.Microservice.Customizations.Identity;
using EG.IdentityManagement.Microservice.Customizations.Providers;
using EG.IdentityManagement.Microservice.Entities.Const;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Extensions;
using EG.IdentityManagement.Microservice.Identity;
using EG.IdentityManagement.Microservice.Services.Implementations;
using EG.IdentityManagement.Microservice.Services.Interfaces;
using EG.IdentityManagement.Microservice.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace EG.IdentityManagement.Microservice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

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
                mongo.ConnectionString = "mongodb://127.0.0.1:27017/identity";
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
                    ClockSkew = TimeSpan.Zero
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

            #endregion "Services"
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EG.IdentityManagement.Microservice v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddlewarePipeline();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}