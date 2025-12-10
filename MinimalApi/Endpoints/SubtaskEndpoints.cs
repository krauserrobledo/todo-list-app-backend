using Application.Abstractions.Services;
using Application.DTOs.SubtaskDTOs;
using MinimalApi.Extensions;

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
                try
                {
                    var subtask = await subtaskService.CreateSubtask(request.Title, taskId, context.RequireUserId());
                    return Results.Created($"/api/subtasks/{subtask.Id}", SubtaskResponse.FromDomain(subtask));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
                catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
                catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            })
                .WithSummary("Create a new subtask for a specific task");

            // Update Subtask
            group.MapPut("/{id}", async (string id, SubtaskUpdateRequest request, ISubtaskService subtaskService, HttpContext context) =>
            {
                try
                {
                    var updated = await subtaskService.UpdateSubtask(id, request.Title, context.RequireUserId());
                    return updated is null
                        ? Results.NotFound($"Subtask {id} not found or access denied")
                        : Results.Ok(SubtaskResponse.FromDomain(updated));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
                catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            })
                .WithSummary("Update an existing subtask");

            // Delete Subtask
            group.MapDelete("/{id}", async (string id, ISubtaskService subtaskService, HttpContext context) =>
            {
                try
                {
                    var deleted = await subtaskService.DeleteSubtask(id, context.RequireUserId());
                    return deleted ? Results.NoContent() : Results.NotFound($"Subtask {id} not found or access denied");
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            }).WithSummary("Delete a subtask by ID");

            // Get All Subtasks by Task
            group.MapGet("/task/{taskId}", async (string taskId, ISubtaskService subtaskService, HttpContext context) =>
            {
                try
                {
                    var subtasks = await subtaskService.GetSubtasksByTask(taskId, context.RequireUserId());
                    return Results.Ok(subtasks.Select(SubtaskResponse.FromDomain));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            }).WithSummary("Get all subtasks for a specific task");

            // Get Subtask by Id
            group.MapGet("/{id}", async (string id, ISubtaskService subtaskService, HttpContext context) =>
            {
                try
                {
                    var subtask = await subtaskService.GetSubtaskById(id, context.RequireUserId());
                    if (subtask == null) return Results.NotFound($"Subtask with ID {id} not found");

                    return Results.Ok(new
                    {
                        id = subtask.Id,
                        title = subtask.Title,
                        taskId = subtask.TaskId
                    });
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            }).WithSummary("Get a subtask by ID");
        }
    }
}
