using Data.Abstractions;
using Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs.Auth;

namespace MinimalApi.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth");

            group.MapPost("/register", Register);
            group.MapPost("/login", Login);
            group.MapPost("/validate", ValidateToken);

        }

        private static async Task<IResult> Register(
            [FromBody] RegisterRequest request,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            // Verify if user exists
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Results.BadRequest(new { error = "User already exists" });

            // Create user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            // Generate token
            var token = tokenService.GenerateToken(user);

            return Results.Ok(new AuthResponse(
                token,
                user.Email!));
        }

        private static async Task<IResult> Login(
            [FromBody] LoginRequest request,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Results.Unauthorized();

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Results.Unauthorized();

            // Generate token
            var token = tokenService.GenerateToken(user);

            return Results.Ok(new AuthResponse(
                token,
                user.Email!));
        }

        private static IResult ValidateToken(
            [FromBody] ValidateTokenRequest request,
            ITokenService tokenService)
        {
            var principal = tokenService.ValidateToken(request.Token);
            return principal != null ? Results.Ok(new { valid = true }) : Results.Ok(new { valid = false });
        }
    }
}