namespace MinimalApi.DTOs
{
    // Task DTOs
    public record TaskCreateDTO(
        string Title,
        string Description,
        DateTime? DueDate,
        string Status
        );

    public record TaskUpdateDTO(
        string Id,
        string Title,
        string Description,
        DateTime? DueDate,
        string Status,
<<<<<<< HEAD
        List<string> TagIds,
        List<string> CategoryIds);
=======
        List<int> TagIds,
        List<int> CategoryIds);
>>>>>>> 5210ae3 (fix(endpoints):bug fix and OpenApi implementation)
}
