namespace MinimalApi.DTOs
{
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record ValidateTokenRequest(string Token);
    public record AuthResponse(string Token, string Email);

}
