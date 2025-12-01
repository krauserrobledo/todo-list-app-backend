using Application.Abstractions.Services;
using Application.DTOs.TaskDTOs;
using System.Security.Claims;

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
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
                if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest("Task title is required.");

                var task = await taskService.CreateTask(
                    request.Title,
                    request.Description,
                    request.DueDate,
                    userId,
                    request.Status);

                if (task == null) return Results.Problem("Error creating task");

                var response = TaskResponse.FromDomain(task);
                return Results.Created($"/api/tasks/{task.Id}", response);
            })
            .WithSummary("Create a Task");

            // Update Task
            group.MapPut("/{id}", async (string id, TaskUpdateRequest request, ITaskService taskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTask = await taskService.GetTaskById(id, userId);
                if (existingTask == null) return Results.NotFound("Task not found");

                var taskExists = await taskService.TaskTitleExists(request.Title, userId);
                if (taskExists && !string.Equals(existingTask.Title, request.Title, StringComparison.OrdinalIgnoreCase))
                    return Results.Conflict("A task with the same title already exists for this user.");

                var updatedTask = await taskService.UpdateTask(
                    id,
                    request.Title ?? existingTask.Title,
                    request.Description ?? existingTask.Description,
                    request.DueDate ?? existingTask.DueDate,
                    request.Status ?? existingTask.Status);

                if (updatedTask == null) return Results.NotFound($"Task with ID {id} not found or access denied");

                var response = TaskResponse.FromDomain(updatedTask);
                return Results.Ok(response);
            })
            .WithSummary("Update a Task");

            // Delete Task
            group.MapDelete("/{id}", async (string id, ITaskService taskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTask = await taskService.GetTaskById(id, userId);
                if (existingTask == null) return Results.NotFound("Task not found");

                var deleted = await taskService.DeleteTask(id);
                if (!deleted) return Results.Problem("Error deleting task");

                return Results.NoContent();
            })
            .WithSummary("Remove selected Task by Id");

            // Get Task by Id
            group.MapGet("/{id}", async (string id, ITaskService taskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var task = await taskService.GetTaskById(id, userId);
                if (task == null) return Results.NotFound($"Task with ID {id} not found.");

                var response = TaskResponse.FromDomain(task);
                return Results.Ok(response);
            })
            .WithSummary("Get Task by Id");

            // Get Tasks by User
            group.MapGet("/user/", async (ITaskService taskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var tasks = await taskService.GetUserTasks(userId);
                var response = tasks.Select(TaskResponse.FromDomain);
                return Results.Ok(response);
            })
            .WithSummary("Get Tasks by User Id with Details");

            // Add Category to Task
            group.MapPost("/{taskId}/categories/{categoryId}", async (string taskId, string categoryId, ITaskService taskService, ICategoryService categoryService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTask = await taskService.GetTaskById(taskId, userId);
                if (existingTask == null) return Results.NotFound("Task not found");

                var existingCategory = await categoryService.GetCategoryById(categoryId);
                if (existingCategory == null) return Results.NotFound("Category not found");

                var updatedTask = await taskService.AddCategoryToTask(taskId, categoryId);
                var response = TaskResponse.FromDomain(updatedTask);
                return Results.Ok(response);
            })
            .WithSummary("Add Category to existing task");

            // Add Tag to Task
            group.MapPost("/{taskId}/tags/{tagId}", async (string taskId, string tagId, ITagService tagService, ITaskService taskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTask = await taskService.GetTaskById(taskId, userId);
                if (existingTask == null) return Results.NotFound("Task not found");

                var existingTag = await tagService.GetTagById(tagId, userId);
                if (existingTag == null) return Results.NotFound("Tag not found");

                var updatedTask = await taskService.AddTagToTask(taskId, tagId);
                var response = TaskResponse.FromDomain(updatedTask);
                return Results.Ok(response);
            })
            .WithSummary("Add Tag to existing Task");

            // Remove Category from Task
            group.MapDelete("/{taskId}/categories/{categoryId}", async (string taskId, string categoryId, ITaskService taskService, ICategoryService categoryService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTask = await taskService.GetTaskById(taskId, userId);
                if (existingTask == null) return Results.NotFound("Task not found");

                var existingCategory = await categoryService.GetCategoryById(categoryId);
                if (existingCategory == null) return Results.NotFound("Category not found");

                var updatedTask = await taskService.RemoveCategoryFromTask(taskId, categoryId);
                var response = TaskResponse.FromDomain(updatedTask);
                return Results.Ok(response);
            })
            .WithSummary("Remove selected category from Task by Id");

            // Remove Tag from Task
            group.MapDelete("/{taskId}/tags/{tagId}", async (string taskId, string tagId, ITagService tagService, ITaskService taskService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTask = await taskService.GetTaskById(taskId, userId);
                if (existingTask == null) return Results.NotFound("Task not found");

                var existingTag = await tagService.GetTagById(tagId, userId);
                if (existingTag == null) return Results.NotFound("Tag not found");

                var updatedTask = await taskService.RemoveTagFromTask(taskId, tagId);
                var response = TaskResponse.FromDomain(updatedTask);
                return Results.Ok(response);
            })
            .WithSummary("Remove selected Tag from Task by Id");
        }
    }
}
