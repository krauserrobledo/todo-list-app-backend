namespace MinimalApi.DTOs.TaskDTOs
{

    // Task Update DTO
    public record TaskUpdateRequest
        (


        string Title,

        string Description,

        DateTime? DueDate,

        string Status,

        List<string>? TagIds,

        List<string>? CategoryIds
        );
}
