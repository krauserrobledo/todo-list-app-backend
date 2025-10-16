using Application.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Data.Repositories
{
    public class TagRepository : ITagRepository
    {

        private readonly AppDbContext _context;
        
        public TagRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tag> CreateTag(Tag tag)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(tag.Name))
                throw new ArgumentException("Tag name cannot be null or WhiteSpace.", nameof(tag.Name));

            // Check if exists
            if (await _context.Tags.AnyAsync(t => t.Name == tag.Name && t.UserId == tag.UserId))
                throw new ArgumentException("Tag with the same name already exists for this user.", nameof(tag.Name));

            // Generate a new GUID for the ID if not provided
            if (string.IsNullOrEmpty(tag.Id) || string
                .IsNullOrWhiteSpace(tag.Id))
                tag.Id = Guid.NewGuid().ToString();

            // Clean up and save
            tag.Name = tag.Name.Trim();
            tag.UserId = tag.UserId.Trim();
            tag.TaskTags = new List<TaskTag>();
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return tag;
        }

        public async Task<bool> DeleteTag(string tagId)
        {
            // Validate input   
            if (string.IsNullOrEmpty(tagId))
                throw new ArgumentException("Tag ID cannot be null or empty.", nameof(tagId));

            if (string.IsNullOrWhiteSpace(tagId))
                throw new ArgumentException("Tag ID cannot be whitespace.", nameof(tagId));

            // Find
            var tag = await _context.Tags.FindAsync(tagId);
            // Not found
            if (tag == null)
                return false;
            // Remove
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<Tag?> GetTagById(string tagId)
        {
            // Validate input
            if (string.IsNullOrEmpty(tagId))
                throw new ArgumentException("Tag ID cannot be null or empty.", nameof(tagId));
            if (string.IsNullOrWhiteSpace(tagId))
                throw new ArgumentException("Tag ID cannot be whitespace.", nameof(tagId));

            // Find
            var tag = await _context.Tags.FindAsync(tagId);

            return tag;
        }

        public async Task<ICollection<Tag>> GetTagsByTask(string taskId, string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("task ID cannot be null or empty.", nameof(taskId));
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("task ID cannot be whitespace.", nameof(taskId));
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));

            // Verify task belongs to user
            var task = await _context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null)
                throw new ArgumentException("Task not found or does not belong to the user.", nameof(taskId));


            // Get all tags for a task
            return await _context.TaskTags
                .Where(tt => tt.TaskId == taskId)
                .Include(tt => tt.Tag)
                .Select(tt => tt.Tag!)
                .ToListAsync();
        }

        public async Task<ICollection<Tag>> GetTagsByUser(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));

            // Get all tags for a user
            var tags = await Task.FromResult(_context.Tags
                .Where(t => t.UserId == userId)
                .ToList());

            return tags;

        }

        public async Task<bool> TagNameExists(string name, string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Tag name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be whitespace.", nameof(name));
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));
            
            // Check existence
            if (await _context.Tags.AnyAsync(t => t.Name == name && t.UserId == userId))
                return true;
            return false;

        }

        public async Task<Tag?> UpdateTag(Tag tag)
        {
            // Validate input
            if (tag == null)
                throw new ArgumentNullException(nameof(tag), "Tag cannot be null.");
            if (string.IsNullOrEmpty(tag.Id))
                throw new ArgumentException("Tag ID cannot be null or empty.", nameof(tag.Id));
            if (string.IsNullOrWhiteSpace(tag.Id))
                throw new ArgumentException("Tag ID cannot be whitespace.", nameof(tag.Id));
            if (string.IsNullOrEmpty(tag.Name))
                throw new ArgumentException("Tag name cannot be null or empty.", nameof(tag.Name));
            if (string.IsNullOrWhiteSpace(tag.Name))
                throw new ArgumentException("Tag name cannot be whitespace.", nameof(tag.Name));
            if (string.IsNullOrEmpty(tag.UserId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(tag.UserId));
            if (string.IsNullOrWhiteSpace(tag.UserId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(tag.UserId));
            // Find existing tag
            var existingTag = await _context.Tags.FindAsync(tag.Id);
            if (existingTag == null)
                return null;
            // Check for name conflict
            if (await _context.Tags.AnyAsync(t => t.Name == tag.Name && t.UserId == tag.UserId && t.Id != tag.Id))
                throw new ArgumentException("Another tag with the same name already exists for this user.", nameof(tag.Name));
            // Update and save
            existingTag.Name = tag.Name.Trim();
            _context.Tags.Update(existingTag);
            await _context.SaveChangesAsync();
            return existingTag;
        }

    }
}
