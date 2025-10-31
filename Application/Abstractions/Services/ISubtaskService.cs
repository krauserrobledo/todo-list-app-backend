using Domain.Models;

namespace Application.Abstractions.Services
{
    /// <summary>
    /// Interface for Subtask service 
    /// </summary>
    public interface ISubtaskService
    {

        // business logic for subtask management
        Task<Subtask> CreateSubtask(string title, string taskId, string userId);
        Task<Subtask?> UpdateSubtask(string subtaskId, string? title, string userId);
        Task<bool> DeleteSubtask(string subtaskId, string userId);
        Task<ICollection<Subtask>> GetSubtasksByTask(string taskId, string userId);
        Task<Subtask?> GetSubtaskById(string subtaskId, string userId);

    }
}
