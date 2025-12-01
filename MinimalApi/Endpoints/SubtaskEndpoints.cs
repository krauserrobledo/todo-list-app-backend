using Application.Abstractions.Services;
using Application.DTOs.SubtaskDTOs;
using System.Security.Claims;

namespace MinimalApi.Endpoints
{
    public static class SubtaskEndpoints
    {
        public static void MapSubtaskEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/subtasks")
                .WithTags("Subtasks")
                .RequireAuthorization();

            // Create Subtask
            group.MapPost("/task/{taskId}", async (string taskId, SubtaskCreateRequest request, ISubtaskService subtaskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
                if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest("Title is required.");
                if (string.IsNullOrWhiteSpace(taskId)) return Results.BadRequest("TaskId is required.");

                var subtask = await subtaskService.CreateSubtask(request.Title, taskId, userId);
                var response = SubtaskResponse.FromDomain(subtask);

                return Results.Created($"/api/subtasks/{subtask.Id}", response);
            })
            .WithSummary("Create a new subtask for a specific task");

            // Update Subtask
            group.MapPut("/{id}", async (string id, SubtaskUpdateRequest request, ISubtaskService subtaskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingSubtask = await subtaskService.GetSubtaskById(id, userId);
                if (existingSubtask == null) return Results.NotFound($"Subtask with ID {id} not found");

                var updatedSubtask = await subtaskService.UpdateSubtask(id, request.Title, userId);
                if (updatedSubtask == null) return Results.NotFound($"Subtask with ID {id} not found");

                var response = SubtaskResponse.FromDomain(updatedSubtask);
                return Results.Ok(response);
            })
            .WithSummary("Update an existing subtask");

            // Delete Subtask
            group.MapDelete("/{id}", async (string id, ISubtaskService subtaskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingSubtask = await subtaskService.GetSubtaskById(id, userId);
                if (existingSubtask == null) return Results.NotFound($"Subtask with ID {id} not found");

                var deleted = await subtaskService.DeleteSubtask(id, userId);
                if (!deleted) return Results.NotFound($"Subtask with ID {id} not found");

                return Results.NoContent();
            })
            .WithSummary("Delete a subtask by ID");

            // Get All Subtasks by Task
            group.MapGet("/task/{taskId}", async (string taskId, ISubtaskService subtaskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var subtasks = await subtaskService.GetSubtasksByTask(taskId, userId);
                return Results.Ok(subtasks.Select(st => new
                {
                    id = st.Id,
                    title = st.Title,
                    taskId = st.TaskId
                }));
            })
            .WithSummary("Get all subtasks for a specific task");

            // Get Subtask by Id
            group.MapGet("/{id}", async (string id, ISubtaskService subtaskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var subtask = await subtaskService.GetSubtaskById(id, userId);
                if (subtask == null) return Results.NotFound($"Subtask with ID {id} not found");

                return Results.Ok(new
                {
                    id = subtask.Id,
                    title = subtask.Title,
                    taskId = subtask.TaskId
                });
            })
            .WithSummary("Get a subtask by ID");
        }
    }
}
