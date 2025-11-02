using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Abstractions.Repositories;
using Infraestructure.Repositories;
using Infraestructure.Identity;
using Infraestructure.Services;
using Infraestructure.Abstractions;

namespace Infraestructure
{
    /// <summary>
    /// Dependency Injection configuration for Infrastructure layer
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers all infrastructure services (Database, Identity, Authentication, Repositories)
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>Service collection with registered services</returns>
        public static IServiceCollection AddInfraestructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ✅ DATABASE
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // ✅ IDENTITY
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ✅ JWT AUTHENTICATION
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // ✅ AUTHORIZATION
            services.AddAuthorization();

            // ✅ TOKEN SERVICE
            services.AddScoped<ITokenService, TokenService>();

            // ✅ REPOSITORIES
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ISubtaskRepository, SubtaskRepository>();
            services.AddScoped<ITagRepository, TagRepository>();

            return services;
        }

    }
}