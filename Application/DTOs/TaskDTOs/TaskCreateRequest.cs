namespace MinimalApi.DTOs.TaskDTOs

{
    // Task Create DTO
    public record TaskCreateRequest
        (

        string Title,

        string Description,

        DateTime? DueDate,

        string Status
        );
}
