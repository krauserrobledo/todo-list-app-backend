using Application.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class SubTaskRepository : ISubTaskRepository
    {
        // DB Context
        private readonly AppDbContext _context;
        // Dependency Injection of DbContext would be here
        public SubTaskRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<SubTask> CreateSubTask(SubTask subTask)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(subTask.Title))
                throw new ArgumentException("SubTask title cannot be null or WhiteSpace.", nameof(subTask.Title));
            
            if (subTask.Title.Length > 100)
                throw new ArgumentException("SubTask title cannot exceed 100 characters.", nameof(subTask.Title));

            if (string.IsNullOrEmpty(subTask.Title))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(subTask.TaskId));

            if (string.IsNullOrWhiteSpace(subTask.TaskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(subTask.TaskId));


            // Generate a new GUID for the ID if not provided
            if (string.IsNullOrEmpty(subTask.Id) || string.IsNullOrWhiteSpace(subTask.Id))
                subTask.Id = Guid.NewGuid().ToString();


            // Clean up Title and TaskId
            subTask.Title = subTask.Title.Trim();
            subTask.TaskId = subTask.TaskId.Trim();
            // subTask.Task = null!; // Avoid EF Core tracking issues

            // Add to DbContext and save changes    
            await _context.Subtasks.AddAsync(subTask);
            await _context.SaveChangesAsync();
            return subTask;
        }

        public async Task<bool> DeleteSubTask(string id)
        {
            // Validate input
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("SubTask ID cannot be null or empty.", nameof(id));

            if(string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("SubTask ID cannot be whitespace.", nameof(id));

            // Find the SubTask by ID
            var subTask = await _context.Subtasks.FindAsync(id);

            // If not found, return false
            if (subTask == null)
                return false;

            // Remove from DbContext and save changes
            _context.Subtasks.Remove(subTask);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<ICollection<SubTask>> GetAllSubTasksByTask(string taskId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.", nameof(taskId));
            if(string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("Task ID cannot be whitespace.", nameof(taskId));
            // Query SubTasks by TaskId
            var subTasks = await _context.Subtasks
                .Where(st => st.TaskId == taskId)
                .ToListAsync();
            return subTasks;
        }

        public Task<SubTask?> GetSubTaskById(string subTaskId)
        {
            // Validate input
            if (string.IsNullOrEmpty(subTaskId))
                throw new ArgumentException("SubTask ID cannot be null or empty.", nameof(subTaskId));
            if(string.IsNullOrWhiteSpace(subTaskId))
                throw new ArgumentException("SubTask ID cannot be whitespace.", nameof(subTaskId));
            // Query SubTask by ID
            return _context.Subtasks
                .FirstOrDefaultAsync(st => st.Id == subTaskId);
        }

        public async Task<SubTask?> UpdateSubTask(SubTask subTask)
        {
            // Validate input
            if (subTask == null)
                throw new ArgumentNullException(nameof(subTask));

            if (string.IsNullOrEmpty(subTask.Id))
                throw new ArgumentException("SubTask ID cannot be null or empty.");

            // Search for existing SubTask
            var existingSubTask = await _context.Subtasks
                .FirstOrDefaultAsync(st => st.Id == subTask.Id);
            // If not found, return null
            if (existingSubTask == null)
                return null;

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(subTask.Title))
                existingSubTask.Title = subTask.Title.Trim();


            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated SubTask
            return existingSubTask;
        }
    
    }
}
