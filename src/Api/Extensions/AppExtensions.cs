using System.Text;
using Application.Interfaces.Services;
using Core.Models;
using Infraestructure.Context;
using Infraestructure.Services;
using Infraestructure.Services.Auth;
using Infraestructure.Services.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions;

public static class AppExtensions
{
    public static void AddAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        
        //Identity
        builder.Services.AddIdentityCore<AppUser>(options => 
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<AppDbContext>();

        //Authentication JWT
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]))
                };
            });
        
        builder.Services.Configure<CookiePolicyOptions>(options => {
            options.MinimumSameSitePolicy = SameSiteMode.Lax; 
            options.HttpOnly = HttpOnlyPolicy.Always; 
            options.Secure = CookieSecurePolicy.SameAsRequest;
        });

        //AppServices
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<TokenService>();
        
        //Background Tasks
        builder.Services.AddHostedService<RefreshTokenCleanup>();
    }
}