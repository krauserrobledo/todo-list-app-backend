using Application.Abstractions.Services;
using Application.Tools;
using Application.Abstractions.Repositories;
using Domain.Models;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tagRepository"></param>
    public class TagService(ITagRepository tagRepository, ITaskRepository taskRepository) : ITagService
    {

        private readonly ITagRepository _tagRepository = tagRepository;
        private readonly ITaskRepository _taskRepository = taskRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Tag> CreateTag(string name, string userId)
        {

            // Bussinnes logic
            if (string.IsNullOrEmpty(name)) 
                throw new ArgumentException("Tag name is required");

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required");

            var nameExists = await _tagRepository.NameExists(name, userId);

            if (nameExists)
                throw new InvalidOperationException("A category with the same name already exists for this user");

            var tag = new Tag
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                UserId = userId
            };

            // Delegate creation to repository
            return await _tagRepository.Create(tag);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Tag?> UpdateTag(string tagId, string userId, string? name)
        {

            // Check ownership of category 
            var existingTag = await _tagRepository.GetById(tagId);
            if (existingTag == null || existingTag.UserId != userId)
                return null;

            // Validate and update name if provided
            if (!string.IsNullOrWhiteSpace(name))
            {

                if (name.Trim() != existingTag.Name)
                {

                    var nameExists = await _tagRepository.NameExists(name.Trim(), userId);

                    if (nameExists)
                        throw new InvalidOperationException("A category with the same name already exists for this user");

                    existingTag.Name = name.Trim();
                }
            }

            return await _tagRepository.Update(existingTag);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteTag(string tagId, string userId)
        {

            // Verify ownership before deletion
            var category = await _tagRepository.GetById(tagId);

            if (category == null || category.UserId != userId)
                return false;

            return await _tagRepository.Delete(tagId);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Tag?> GetTagById(string id, string userId)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Task ID is required");

            if (!string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User Id required");

            var tag = await _tagRepository.GetById(id);


            // Return tag
            return  tag?.UserId == userId ? tag : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ICollection<Tag>> GetTagsByTask(string id, string userId)
        {
            // Check if belongs to user
            var task = await _taskRepository.GetById(id);
            if (task == null || task.UserId != userId)
                return [];

            return await _tagRepository.GetByTask(id, userId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ICollection<Tag>> GetUserTags(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User Id required");

            return await _tagRepository.GetByUser(userId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        
    }
}
