namespace Application.DTOs.SubtaskDTOs
{
    // Subtask response DTO
    public class SubtaskResponse
    {
        public string Id { get; set; } = string.Empty;

        public string TaskId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;

        public static SubtaskResponse FromDomain(Domain.Models.Subtask subtask)
        {
            return new SubtaskResponse
            {
                Id = subtask.Id,
                TaskId =  subtask.TaskId,
                Title = subtask.Title,
                CreatedAt = subtask.CreatedAt,

            };
        }
    }
}
