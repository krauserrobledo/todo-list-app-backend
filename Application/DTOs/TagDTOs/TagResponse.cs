namespace Application.DTOs.TagDTOs
{
    // Tag response DTO
    public class TagResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public static TagResponse FromDomain(
            Domain.Models.Tag tag)
        {
            return new TagResponse
            {
                Id = tag.Id,
                Name = tag.Name,
                UserId = tag.UserId
            };
        }
    }
}
