using Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace Application.Abstractions
{
    public interface IUserRepository
    {
        // Basic CRUD operations
        Task<User?> GetById(string userId);
        Task<User?> GetByEmail(string email);
        Task<bool> Exists(string userId);
        Task<User> Create(User user);

        // Additional methods for user-related operations
        Task<User> GetUserWithTasks(string userId);
        Task<int> GetUserTaskCount(string userId);


    }
}
