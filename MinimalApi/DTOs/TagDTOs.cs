namespace MinimalApi.DTOs
{
    // Tag DTOs
<<<<<<< HEAD
    public record TagCreateDTO(string Name);
    public record TagUpdateDTO(string Name);
=======
    public record TagCreateDTO(string Name, string User);
    public record TagUpdateDTO(int Id, string Name);
>>>>>>> 5210ae3 (fix(endpoints):bug fix and OpenApi implementation)
}
