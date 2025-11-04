using Domain.Models;

namespace Application.Abstractions.Repositories
{

    public interface ISubtaskRepository
    {

        // Get all Categories with related entities
        Task<ICollection<Subtask>> GetAllByTask(string taskId);

        Task<Subtask?> GetById(string subtaskId);


        // CRUD operations
        Task<Subtask> Create(Subtask subtask);

        Task<Subtask?> Update(Subtask subtask);

        Task<bool> Delete(string id);
    }
}
