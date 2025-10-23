using Data.Abstractions;
using Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs;

namespace MinimalApi.Endpoints
{
    /// <summary>
    /// Configures the API endpoints for authentication-related operations.
    /// </summary>
    public static class AuthEndpoints
    {
        /// <summary>
        /// Configures the API endpoints for authentication-related operations.
        /// </summary>
        /// <param name="app">The endpoint route builder</param>
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Authentication");
            // Defined auth-related endpoints
            group.MapPost("/register", Register)
                .WithSummary("Create a new user");
            group.MapPost("/login", Login)
                .WithSummary("login using Email and Password");
            group.MapPost("/validate", ValidateToken)
                .WithSummary("Token validations");
        }
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <remarks> Register a new user with email and password.</remarks>
        /// <param name="request">The registration request containing email and password.</param>
        /// <param name="userManager">The user manager for handling user operations.</param>
        /// <param name="tokenService">The token service for generating JWT tokens.</param>
        /// <returns>A result indicating the outcome of the registration.</returns>
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
            // Save user
            var result = await userManager.CreateAsync(user, request.Password);
            // Register failed
            if (!result.Succeeded)
                return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            // Generate token
            var token = tokenService.GenerateToken(user);
            // Return response 
            return Results.Ok(new AuthResponse(
                token,
                user.Email!));
        }
        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="request">The login request containing email and password.</param>
        /// <param name="userManager">The user manager for handling user operations.</param>
        /// <param name="signInManager">The sign-in manager for handling user sign-in.</param>
        /// <param name="tokenService">The token service for generating JWT tokens.</param>
        /// <returns>A result indicating the outcome of the login.</returns>
        private static async Task<IResult> Login(
            [FromBody] LoginRequest request,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            // Find user
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Results.Unauthorized();
            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Results.Unauthorized();
            // Generate token
            var token = tokenService.GenerateToken(user);
            // Return response
            return Results.Ok(new AuthResponse(
                token,
                user.Email!));
        }
        /// <summary> 
        /// Validates a JWT token.
        /// </summary>
        /// <param name="request">The token validation request containing the token.</param>
        /// <param name="tokenService">The token service for validating JWT tokens.</param>
        /// <returns>A result indicating whether the token is valid.</returns>
        private static IResult ValidateToken(
            [FromBody] ValidateTokenRequest request,
            ITokenService tokenService)
        {
            var principal = tokenService.ValidateToken(request.Token);
            return principal != null ? Results.Ok(new { valid = true }) : Results.Ok(new { valid = false });
        }
    }
}