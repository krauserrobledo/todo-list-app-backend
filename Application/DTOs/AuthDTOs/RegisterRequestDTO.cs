namespace Application.DTOs.AuthDTOs

{
    // DTO for registration request
    public record RegisterRequestDTO
        (
        string UserName,

        string Email,

        string Password
        );

}
