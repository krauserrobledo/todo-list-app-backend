using Domain.Models;

namespace Application.Abstractions.Services
{
    /// <summary>
    /// Interface for Tag Services 
    /// </summary>
    public interface ITagService
    {
        // business logic for tag management
        Task<Tag> CreateTag(string name, string userId);
        Task<Tag?> UpdateTag(string tagId, string userId, string? name);
        Task<bool> DeleteTag(string tagId, string userId);
        Task<ICollection<Tag>> GetUserTags(string userId);
        Task<Tag?> GetTagById(string id, string userId);
        Task<ICollection<Tag>> GetTagsByTask(string id, string userId);
    }
}
