using Domain.Models;

namespace Application.Abstractions.Services
{
    /// <summary>
    /// Interface for Subtask service 
    /// </summary>
    public interface ISubtaskService
    {

        // business logic for subtask management
        Task<Subtask> CreateSubtask(string title, string taskId);
        Task<Subtask?> UpdateSubtask(string subtaskId, string? title);
        Task<bool> DeleteSubtask(string subtaskId);
        Task<ICollection<Subtask>> GetAllSubtasksByTask(string taskId);
        Task<Subtask?> GetSubtaskById(string subtaskId);

    }
}
