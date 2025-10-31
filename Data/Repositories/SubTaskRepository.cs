using Domain.Abstractions.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{

    /// <summary>
    /// Repository implementation for subtask repository interface
    /// </summary>
    /// <param name="context"></param>
    public class SubtaskRepository(AppDbContext context) : ISubtaskRepository
    {

        // DB Context
        private readonly AppDbContext _context = context;

        /// <summary>
        /// Creates a new subtask for the specified task, ensuring that the subtask title is unique within that task.
        /// </summary>
        /// <param name="subtask"></param>
        /// <returns>If subtask already exists returns exception, else returns created subtask</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Subtask> Create(Subtask subtask)
        {

            // Retrieve existing subtask to ensure uniqueness
            var existingSubtask = await _context.Subtasks
                .FirstOrDefaultAsync(st => st.Title == subtask.Title && st.TaskId == subtask.TaskId);

            // set CreatedAt time
            var currtime = DateTime.UtcNow;

            subtask.CreatedAt = currtime;

            // Add to DbContext and save changes
            await _context.Subtasks.AddAsync(subtask);
            await _context.SaveChangesAsync();
            return subtask;
        }

        /// <summary>
        /// Update an existing Subtask by title
        /// </summary>
        /// <param name="subTask"></param>
        /// <returns>Returns updated subtask or null if not found</returns>
        public async Task<Subtask?> Update(Subtask subTask)
        {

            // Find subtask using LINQ
            var subtaskExist = await _context.Subtasks
                .Where(t => t.Id == subTask.Id)
                .FirstOrDefaultAsync();

            // If exists update
            if (subtaskExist != null)
            {
                // Update attribute
                subtaskExist.Title = subTask.Title ?? subtaskExist.Title;

                // Save changes
                await _context.SaveChangesAsync();
                return subtaskExist;
            }
            return null;
        }

        /// <summary>
        /// Deletes a subtask by its ID if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns true if the subtask was deleted, false otherwise.</returns>
        public async Task<bool> Delete(string id)
        {

            // Retrieve existing subtask
            var subtaskExist = await _context.Subtasks
                .Where(st => st.Id == id)
                .FirstOrDefaultAsync();

            // If exists delete and save changes
            if (subtaskExist == null)
            
                return false;

            _context.Subtasks.Remove(subtaskExist);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets all subtasks associated with a specific task, ordered by descending ID.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns>Collection of Subtasks</returns>
        public async Task<ICollection<Subtask>> GetAllByTask(string taskId)
        {

            // Get using linq
            return await _context.Subtasks
                .Where(st => st.TaskId == taskId)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a subtask by its ID.
        /// </summary>
        /// <param name="subTaskId"></param>
        /// <returns>Returns existing subtask by FirstDefaultAsync method</returns>
        public async Task<Subtask?> GetById(string subTaskId)
        {

            // Get using Linq
            return await _context.Subtasks
                .FirstOrDefaultAsync(st =>  st.Id == subTaskId );
        }          
    }
}
