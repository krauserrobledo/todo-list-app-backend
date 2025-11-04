using Application.Abstractions.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Repositories
{

    /// <summary>
    /// Repository for managing Tag entities
    /// </summary>
    /// <param name="context"></param>
    public class TagRepository(AppDbContext context) : ITagRepository
    {

        private readonly AppDbContext _context = context;

        /// <summary>
        /// Create a new tag
        /// </summary>
        /// <remarks> Validates if a tag with the same name exists for the user before creating a new one</remarks>
        /// <param name="tag"></param>
        /// <returns>Returns the created Tag</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Tag> Create(Tag tag)
        {

            // Validate input using LINQ
            var existingTag = await _context.Tags
                 .FirstOrDefaultAsync(t => t.Name == tag.Name && t.UserId == tag.UserId);

            // Add to DbContext and save changes
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        /// <summary>
        /// Updates an existing tag for a specific user.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="tag"></param>
        /// <returns>If tag exists sets the tag name, else returns null</returns>
        public async Task<Tag?> Update(Tag tag)
        {

            // Retrieve existing tag
            var tagExists = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == tag.Id);

            // Update and save if exists
                await _context.SaveChangesAsync();
                return tag;
        }

        /// <summary>
        /// Delete an existing tag
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns>if tag exists delete and return true, else returns false</returns>
        public async Task<bool> Delete(string tagId)
        {

            // Validate id using LINQ
            var tagExist = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == tagId);

            // Delete and save if exists
            if (tagExist == null)
            {
                return false;
            }
            // Remove tag
            _context.Tags.Remove(tagExist);
            await _context.SaveChangesAsync();
            return true;
        }
        
        /// <summary>
        /// Get a tag by Id
        /// </summary>
        /// <remarks> Find a tag by Id and returns</remarks>
        /// <param name="tagId"></param>
        /// <returns>Returns Tag if exists, else returns exception</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tag?> GetById(string tagId)
        {
            // Validate input using LINQ
            return await _context.Tags
                .Where(t => t.Id == tagId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get Tags from Tasks and Users
        /// </summary>
        /// <remarks>Joins TaskTags and Tags tables to get Tags for a specific task and user</remarks>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns> Returns async list containing Tags asigned to selected Task</returns>
        public async Task<ICollection<Tag>> GetByTask(string taskId, string userId)
        {

            // Get all tags for a task validated by user id using LINQ
            return await _context.TaskTags
                .Where(tt => tt.TaskId == taskId && tt.Task.UserId == userId)
                .Include(tt => tt.Tag)
                .Select(tt => tt.Tag!)
                .ToListAsync();
        }

        /// <summary>
        /// Get tags which belongs to a specific user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Returns Tags async list </returns>
        public async Task<ICollection<Tag>> GetByUser(string userId)
        {

            // Validate input and get using LINQ
            return await _context.Tags
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Id)
                .ToListAsync(); 
        }

        /// <summary>
        /// Check if tag name exists for a specific user
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns>Returns existing tags by  userId and name, using  'await' and 'AnyAsync' method containing </returns>
        public async Task<bool> NameExists(string name, string userId)
        {

            // Validate input and check existence using LINQ
            return await _context.Tags
                .AnyAsync(t => t.Name == name && t.UserId == userId);
        }               
    }
}
