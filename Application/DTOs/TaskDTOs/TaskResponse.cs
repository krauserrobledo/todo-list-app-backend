using Application.DTOs.CategoryDTOs;
using Application.DTOs.SubtaskDTOs;
using Application.DTOs.TagDTOs;

namespace Application.DTOs.TaskDTOs
{
    public class TaskResponse
    {
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public virtual ICollection<SubtaskResponse> Subtasks { get; set; } = new List<SubtaskResponse>();

        public virtual ICollection<CategoryResponse> Categories { get; set; } = new List<CategoryResponse>();

        public virtual ICollection<TagResponse> Tags { get; set; } = new List<TagResponse>();

        public static TaskResponse FromDomain(Domain.Models.Task task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                UserId = task.UserId,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                Subtasks = task.Subtasks
                    .Select(SubtaskResponse.FromDomain)
                    .ToList(),
                Categories = task.TaskCategories
                    .Select(tc => CategoryResponse.FromDomain(tc.Category))
                    .ToList(),
                Tags = (ICollection<TagResponse>)task.TaskTags
                    .Select(tt => TagResponse.FromDomain(tt.Tag))
                    .ToList()

            };
        }
    }
}
