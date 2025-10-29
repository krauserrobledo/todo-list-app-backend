using Domain.Models;

namespace Application.Abstractions.Repositories
{
    public interface ISubtaskRepository
    {

        // Get all Categories with related entities
        Task<ICollection<Subtask>> GetAllSubtasksByTask(string taskId);

        Task<Subtask?> GetSubtaskById(string subtaskId);



        // CRUD operations
        Task<Subtask> CreateSubtask(Subtask subtask);

        Task<Subtask?> UpdateSubtask(Subtask subtask);

        Task<bool> DeleteSubtask(string id);
    }
}
