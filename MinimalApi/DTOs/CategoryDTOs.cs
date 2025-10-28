namespace MinimalApi.DTOs
{
    // Category DTOs
    public record CategoryCreateDTO(string Name, string Color);

    public record CategoryUpdateDTO(string Name, string Color);

}
