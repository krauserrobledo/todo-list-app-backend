namespace MinimalApi.DTOs.AuthDTOs

{
    // DTO for login request
    public record LoginRequestDTO
        (

        string Email,

        string Password
        );
}
