namespace MinimalApi.DTOs.TaskDTOs
{

    // Task Update DTO
    public record TaskUpdateDTO
        (

        string Id,

        string Title,

        string Description,

        DateTime? DueDate,

        string Status,

        List<string> TagIds,

        List<string> CategoryIds
        );
}
