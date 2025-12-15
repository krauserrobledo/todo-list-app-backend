using Domain.Models;

namespace Application.Abstractions.Repositories
{

    /// <summary>
    /// Interface for Tag repository
    /// </summary>
    public interface ITagRepository
    {

        // Get all tags with related entities
        Task<ICollection<Tag>> GetByUser(string userId);

        Task<Tag?> GetById(string tagId);

        Task<ICollection<Tag>> GetByTask(string taskId, string userId);

        Task<bool> NameExists(string name, string userId);


        // CRUD operations
        Task<Tag> Create(Tag tag);

        Task<Tag?> Update(Tag tag);

        Task<bool> Delete(string tagId);
    }
}
