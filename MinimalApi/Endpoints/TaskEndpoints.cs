using Application.Abstractions.Services;
using Application.DTOs.TaskDTOs;
using MinimalApi.Extensions;

namespace MinimalApi.Endpoints
{
    public static class TaskEndpoints
    {
        public static void MapTaskEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/tasks")
                .WithTags("Tasks")
                .RequireAuthorization();


            // Create Task
            group.MapPost("/", async (TaskCreateRequest request, ITaskService taskService, HttpContext context) =>
            {
                try
                {
                    var task = await taskService.CreateTask(request, context.RequireUserId());

                    return Results.Created($"/api/tasks/{task.Id}", TaskResponse.FromDomain(task));
                }

                catch (UnauthorizedAccessException)
                {
                    return Results.Unauthorized();
                }

                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(ex.Message);
                }
            })
            .WithSummary("Create a Task");


            // Update Task
            group.MapPut("/{id}", async (string id, TaskUpdateRequest request, ITaskService taskService, HttpContext context) =>
            {
                try
                {
                    var updated = await taskService.UpdateTask(request, id, context.RequireUserId());

                    return updated is null
                        ? Results.NotFound($"Task {id} not found or access denied")
                        : Results.Ok(TaskResponse.FromDomain(updated));
                }

                catch (UnauthorizedAccessException)
                {
                    return Results.Unauthorized();
                }

                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(ex.Message);
                }
            })
            .WithSummary("Update a Task");


            // Delete Task
            group.MapDelete("/{id}", async (string id, ITaskService taskService, HttpContext context) =>
            {
                try
                {
                    var deleted = await taskService.DeleteTask(id);

                    return deleted
                        ? Results.NoContent()
                        : Results.NotFound($"Task {id} not found or access denied");
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Unauthorized();
                }
            })
            .WithSummary("Remove selected Task by Id");


            // Get Task by Id
            group.MapGet("/{id}", async (string id, ITaskService taskService, HttpContext context) =>
            {
                try
                {
                    var task = await taskService.GetTaskById(id, context.RequireUserId());

                    return task is null
                        ? Results.NotFound($"Task {id} not found or access denied")
                        : Results.Ok(TaskResponse.FromDomain(task));
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Unauthorized();
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .WithSummary("Get Task by Id");


            // Get Tasks by User
            group.MapGet("/user", async (ITaskService taskService, HttpContext context) =>
            {
                try
                {
                    var tasks = await taskService.GetUserTasks(context.RequireUserId());

                    return Results.Ok(tasks.Select(TaskResponse.FromDomain));
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Unauthorized();
                }
            })
            .WithSummary("Get Tasks by User Id with Details");


            // Add Category to Task
            group.MapPost("/{taskId}/categories/{categoryId}", async (string taskId, string categoryId, ITaskService taskService) =>
            {
                try
                {
                    var updatedTask = await taskService.AddCategoryToTask(taskId, categoryId);

                    return Results.Ok(TaskResponse.FromDomain(updatedTask!));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .WithSummary("Add Category to existing task");


            // Add Tag to Task
            group.MapPost("/{taskId}/tags/{tagId}", async (string taskId, string tagId, ITaskService taskService) =>
            {
                try
                {
                    var updatedTask = await taskService.AddTagToTask(taskId, tagId);

                    return Results.Ok(TaskResponse.FromDomain(updatedTask!));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .WithSummary("Add Tag to existing Task");


            // Remove Category from Task
            group.MapDelete("/{taskId}/categories/{categoryId}", async (string taskId, string categoryId, ITaskService taskService) =>
            {
                try
                {
                    var updatedTask = await taskService.RemoveCategoryFromTask(taskId, categoryId);

                    return Results.Ok(

                        TaskResponse.FromDomain(updatedTask!));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .WithSummary("Remove selected category from Task by Id");

            // Remove Tag from Task
            group.MapDelete("/{taskId}/tags/{tagId}", async (string taskId, string tagId, ITaskService taskService) =>
            {
                try
                {
                    var updatedTask = await taskService.RemoveTagFromTask(taskId, tagId);

                    return Results.Ok(TaskResponse.FromDomain(updatedTask!));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .WithSummary("Remove selected Tag from Task by Id");
        }
    }
}
