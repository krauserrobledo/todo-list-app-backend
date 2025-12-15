using Application.DTOs.AuthDTOs;
using Infraestructure.Abstractions;
using Infraestructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace MinimalApi.Endpoints
{
    public static class AuthEndpoints
    {
        /// <summary>
        /// Maps the authentication endpoints to the specified application.
        /// </summary>
        /// <param name="app">Application instance to map the endpoints to.</param>
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Authentication");

            // Register
            group.MapPost("/register", async (
                RegisterRequestDTO request,
                UserManager<ApplicationUser> userManager,
                ITokenService tokenService) =>
            {
                var existingUser = await userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                    return Results.BadRequest(new { error = "User already exists" });

                var user = new ApplicationUser
                {
                    UserName = request.UserName,
                    Email = request.Email
                };

                var result = await userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errorMessages = result.Errors.Select(e => e.Description).ToList();
                    return Results.BadRequest(new { errors = errorMessages });
                }

                var token = tokenService.GenerateToken(user);
                return Results.Ok(new AuthResponseDTO(token, user.Email!, user.UserName!));
            })
            .WithSummary("Create a new user");

            // Login
            group.MapPost("/login", async (
                LoginRequestDTO request,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                ITokenService tokenService) =>
            {
                var user = await userManager.FindByEmailAsync(request.Email);
                if (user == null) return Results.Unauthorized();

                var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded) return Results.Unauthorized();

                var token = tokenService.GenerateToken(user);
                return Results.Ok(new AuthResponseDTO(token, user.Email!, user.UserName!));
            })
            .WithSummary("Login using Email and Password");

            // Validate Token
            group.MapPost("/validate", (
                ValidateTokenRequestDTO request,
                ITokenService tokenService) =>
            {
                var principal = tokenService.ValidateToken(request.Token);
                return principal != null
                    ? Results.Ok(new { valid = true })
                    : Results.Ok(new { valid = false });
            })
            .WithSummary("Token validations");
        }
    }
}
