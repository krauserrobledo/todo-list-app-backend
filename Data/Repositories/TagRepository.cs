using Domain.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
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
        public async Task<Tag> CreateTag(Tag tag)
        {

            // Validate input using LINQ
            var existingTag = await _context.Tags
                 .FirstOrDefaultAsync(t => t.Name == tag.Name && t.UserId == tag.UserId);

            if (existingTag != null)
            {
                throw new InvalidOperationException("A tag with the same name already exists for this user.");
            }

            // Generate a new GUID for the ID if not provided
            if (string.IsNullOrEmpty(tag.Id) || string
                .IsNullOrWhiteSpace(tag.Id))
                tag.Id = Guid
                    .NewGuid()
                    .ToString();

            // Add to DbContext and save changes
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        /// <summary>
        /// Delete an existing tag
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns>if tag exists delete and return true, else returns false</returns>
        public async Task<bool> DeleteTag(string tagId)
        {

            // Validate id using LINQ
            var tagExist = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == tagId);

            // Delete and save if exists
            if (tagExist != null) 
            {
                _context.Tags.Remove(tagExist);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a tag by Id
        /// </summary>
        /// <remarks> Find a tag by Id and returns</remarks>
        /// <param name="tagId"></param>
        /// <returns>Returns Tag if exists, else returns exception</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tag?> GetTagById(string tagId)
        {

            // Validate input
            if (string.IsNullOrWhiteSpace(tagId))
                throw new ArgumentException("Tag ID cannot be whitespace or null.", nameof(tagId));

            // Find
            var tag = await _context.Tags.FindAsync(tagId);
            return tag;
        }

        /// <summary>
        /// Get Tags from Tasks and Users
        /// </summary>
        /// <remarks>Joins TaskTags and Tags tables to get Tags for a specific task and user</remarks>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns> Returns async list containing Tags asigned to selected Task</returns>
        public async Task<ICollection<Tag>> GetTagsByTask(string taskId, string userId)
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
        public async Task<ICollection<Tag>> GetTagsByUser(string userId)
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
        public async Task<bool> TagNameExists(string name, string userId)
        {

            // Validate input and check existence using LINQ
            return await _context.Tags
                .AnyAsync(t => t.Name == name && t.UserId == userId);
        }

        /// <summary>
        /// Updates an existing tag for a specific user.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="tag"></param>
        /// <returns>If tag exists sets the tag name, else returns null</returns>
        public async Task<Tag?> UpdateTag(Tag tag)
        {

            // Validate input and check existence using LINQ
            var tagExists = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == tag.Id);

            // Update and save if exists
            if (tagExists != null)
                {
                tagExists.Name = tag.Name?? tagExists.Name;
                await _context.SaveChangesAsync();
                return tagExists;
            }
            return null;
        }
    }
}
