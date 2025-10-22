using Domain.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class SubtaskRepository(AppDbContext context) : ISubtaskRepository
    {
        // DB Context
        private readonly AppDbContext _context = context;

        public async Task<Subtask> CreateSubtask(Subtask subtask)
        {
            // Validation using LINQ
            var existingSubtask = await _context.Subtasks
                .FirstOrDefaultAsync(st => st.Title == subtask.Title && st.TaskId == subtask.TaskId);

            if (existingSubtask != null)
                {
                throw new InvalidOperationException("A subtask with the same title already exists for this task.");
            }
            // Add to DbContext and save changes
            await _context.Subtasks.AddAsync(subtask);
            await _context.SaveChangesAsync();
            return subtask;

        }

        public async Task<bool> DeleteSubtask(string id)
        {
            // Verify Id with LINQ
            var subtaskExist = await _context.Subtasks
                .Where(st => st.Id == id)
                .FirstOrDefaultAsync();
 
            if (subtaskExist != null)
            {

                _context.Subtasks.Remove(subtaskExist);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<ICollection<Subtask>> GetAllSubtasksByTask(string taskId)
        {
            // Get using linq
            return await _context.Subtasks
                .Where(st => st.TaskId == taskId)
                .OrderByDescending(st => st.Id)
                .ToListAsync();

        }

        public async Task<Subtask?> GetSubtaskById(string subTaskId)
        {
            // Get using Linq
            return await _context.Subtasks
                .FirstOrDefaultAsync(st =>  st.Id == subTaskId );
        }

        public async Task<Subtask?> UpdateSubtask(Subtask subTask)
        {
            // Find subtask using LINQ
            var subtaskExist = await _context.Subtasks
                .Where(t =>  t.Id == subTask.Id)
                .FirstOrDefaultAsync();

            if(subtaskExist != null)
            {
                // Update attribute
                subtaskExist.Title = subTask.Title ?? subtaskExist.Title;
                
                // Save changes
                await _context.SaveChangesAsync();
                return subtaskExist;

            }
            return null;
        }
    
    }
}
