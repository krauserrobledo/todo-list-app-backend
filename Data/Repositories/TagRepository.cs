using Domain.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Data.Repositories
{
    public class TagRepository(AppDbContext context) : ITagRepository
    {

        private readonly AppDbContext _context = context;

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

            // Get all tags for a task validated by user id using LINQ
            return await _context.TaskTags
                .Where(tt => tt.TaskId == taskId && tt.Task.UserId == userId)
                .Include(tt => tt.Tag)
                .Select(tt => tt.Tag!)
                .ToListAsync();
        }

        public async Task<ICollection<Tag>> GetTagsByUser(string userId)
        {
            // Validate input and get using LINQ
            return await _context.Tags
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Id)
                .ToListAsync(); 
        }

        public async Task<bool> TagNameExists(string name, string userId)
        {
            // Validate input and check existence using LINQ
            return await _context.Tags
                .AnyAsync(t => t.Name == name && t.UserId == userId);

        }

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
