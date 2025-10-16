using Application.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Tasks = Domain.Models.Task;

namespace Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context; 

        public UserRepository(AppDbContext context) 
        { 
            _context = context;
        }

        public async Task<User> Create(User user)
        {
            // Validate input
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            if (string.IsNullOrEmpty(user.Email))
                throw new ArgumentException("User email cannot be null or empty.", nameof(user.Email));
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("User email cannot be whitespace.", nameof(user.Email));
            if (string.IsNullOrEmpty(user.UserName))
                throw new ArgumentException("User name cannot be null or empty.", nameof(user.UserName));
            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new ArgumentException("User name cannot be whitespace.", nameof(user.UserName));

            // Generate a new GUID for the ID if not provided
            if (string.IsNullOrEmpty(user.Id) || string.IsNullOrWhiteSpace(user.Id))
                user.Id = Guid.NewGuid().ToString();
            // Clean up and save
            user.Email = user.Email.Trim();
            user.UserName = user.UserName.Trim();
            user.Tasks = new List<Tasks>();
            user.Tags = new List<Tag>();
            user.Categories = new List<Category>();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;

        }

        public async Task<bool> Exists(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));
            // Check existence
            if (await _context.Users.AnyAsync(u => u.Id == userId))
                return true;
            return false;
        }

        public async Task<User?> GetByEmail(string email)
        {
            // Validate input
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be whitespace.", nameof(email));
            // Retrieve user by email
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());
        }

        public async Task<User?> GetById(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));
            // Retrieve user by ID
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Trim());
        }

        public async Task<int> GetUserTaskCount(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));
            // Count tasks for the user
            return await _context.Tasks
                .CountAsync(t => t.UserId == userId.Trim());
        }
        
        async Task<User> IUserRepository.GetUserWithTasks(string userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be whitespace.", nameof(userId));
            // Retrieve user with tasks
            return await _context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == userId.Trim()) 
                ?? throw new InvalidOperationException($"User with ID {userId} not found.");

        }
    }
}
