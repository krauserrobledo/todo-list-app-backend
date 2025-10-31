namespace Application.DTOs.CategoryDTOs
{
    public class CategoryResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public static CategoryResponse FromDomain(Domain.Models.Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Color = category.Color ?? string.Empty,
                UserId = category.UserId
            };
        }
    }
}
