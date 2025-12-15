namespace Application.DTOs.TaskDTOs
{
    // Task Update DTO
    public record TaskUpdateRequest
        (

        string Title,

        string Description,

        DateTime? DueDate,

        string Status
        );
}
