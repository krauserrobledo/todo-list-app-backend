namespace Application.DTOs.AuthDTOs

{
    // DTO for authentication response
    public record AuthResponseDTO
        (

        string Token,
        
        string Email,

        string UserName
        );

}
