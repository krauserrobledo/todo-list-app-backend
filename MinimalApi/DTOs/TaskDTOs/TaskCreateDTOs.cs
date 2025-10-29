namespace MinimalApi.DTOs.TaskDTOs

{
    // Task Create DTO
    public record TaskCreateDTO
        (

        string Title,

        string Description,

        DateTime? DueDate,

        string Status
        );
}
