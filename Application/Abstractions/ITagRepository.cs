using Domain.Models;

namespace Application.Abstractions
{
    public interface ITagRepository
    {
        // Get all tags with related entities
        Task<ICollection<Tag>> GetTagsByUser(string userId);
        Task<Tag?> GetTagById(string tagId);
        Task<ICollection<Tag>> GetTagsByTask(string taskId, string userId);

        Task<bool> TagNameExists(string name, string userId);

        // CRUD operations
        Task<Tag> CreateTag(Tag tag);
        Task<Tag?> UpdateTag(Tag tag);
        Task<bool> DeleteTag(string tagId);
    }
}
