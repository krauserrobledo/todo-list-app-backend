namespace MinimalApi.DTOs.AuthDTOs

{
    // DTO for authentication response
    public record AuthResponseDTO
        (

        string Token,
        
        string Email
        );

}
