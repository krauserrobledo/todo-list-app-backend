using Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs;
using System.Security.Claims;

namespace MinimalApi.Endpoints
{
    public static class TaskEndpoints
    {
        public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/tasks")
                .WithTags("Tasks")
                .RequireAuthorization();
            // Defined task-related endpoints
            group.MapPost("/", CreateTask)
                .WithSummary("Create a Task");
            group.MapPut("/{id}", UpdateTask)
                .WithSummary("Update a Task");
            group.MapDelete("/{id}", DeleteTask)
                .WithSummary("Remove selected Task by Id");
            group.MapGet("/{id}", GetTaskById)
                .WithSummary("Get Task by Id");
            group.MapGet("/user/{userId}", GetTasksByUser)
                .WithSummary("Get Tasks by User Id with Details");
            group.MapPost("/{taskId}/categories/{categoryId}", AddCategoryToTask)
                .WithSummary("Add Category to existing task");
            group.MapPost("/{taskId}/tags/{tagId}", AddTagToTask)
                .WithSummary("Add Tag to existing Task");
            group.MapDelete("/{taskId}/categories/{categoryId}", RemoveCategoryFromTask)
                .WithSummary("Remove selected category from Task by Id");
            group.MapDelete("/{taskId}/tags/{tagId}", RemoveTagFromTask)
                .WithSummary("Remove selected Tag from Task by Id");            
        }
        private static async Task<IResult> CreateTask(
            [FromBody] TaskCreateDTO request,
            ITaskRepository taskRepository,
            HttpContext context)
        {
            // Logic to create a task
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Results.BadRequest("Task title is required.");
                // Get user ID from context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Check for duplicate task title for the same user
                var titleExists = await taskRepository.TaskTitleExists(request.Title.Trim(), userId);
                if (titleExists)
                {
                    return Results.Conflict("A task with the same title already exists for this user.");
                }
                // Validate status
                if (!Domain.Constants.TaskStatus.IsValid(request.Status))
                {
                    return Results.BadRequest("Invalid task status.");
                }
                // Create new Task entity
                var newTask = new Domain.Models.Task
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim(),
                    DueDate = request.DueDate,
                    Status = request.Status,
                    UserId = userId
                };
                // Save to repository
                var createdTask = await taskRepository.CreateTask(newTask);
                // Return success response
                return Results.Created($"/api/tasks/{createdTask.Id}", new
                {
                    id = createdTask.Id,
                    title = createdTask.Title,
                    description = createdTask.Description,
                    dueDate = createdTask.DueDate,
                    status = createdTask.Status
                });

            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred while creating the task:{ex.Message}");
            }
        }
        private static async Task<IResult> UpdateTask(
            string id,
            [FromBody] TaskUpdateDTO request,
            ITaskRepository taskRepository,
            HttpContext context)
        {
            // Logic to update a task
            try
            {
                // Check if exists
                var existingTask = await taskRepository.GetTaskById(id);
                if (existingTask == null)
                {
                    return Results.NotFound($"Task not found");
                }
                // Check for duplicate title
                var taskExists = await taskRepository.TaskTitleExists(request.Title, existingTask.UserId);
                if (taskExists && !string.Equals(existingTask.Title, request.Title, StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Conflict("A task with the same title already exists for this user.");
                }
                existingTask.Id = existingTask.Id; // keep the same id
                existingTask.Title = request.Title ?? existingTask.Title;
                existingTask.Description = request.Description ?? existingTask.Description;
                existingTask.DueDate = request.DueDate ?? existingTask.DueDate;
                existingTask.Status = request.Status ?? existingTask.Status;
                // Update in repository
                var updatedTask = await taskRepository.UpdateTask(existingTask);
                if (updatedTask == null)
                {
                    return Results.Problem("Error updating task");
                }
                // Return response
                return Results.Ok(new
                {
                    id = updatedTask.Id,
                    title = updatedTask.Title,
                    description = updatedTask.Description,
                    dueDate = updatedTask.DueDate,
                    status = updatedTask.Status
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating task: {ex.Message}");
            }
        }
        private static async Task<IResult> DeleteTask(
            string id,
            ITaskRepository taskRepository,
            HttpContext context)
        {
            // Logic to delete a task
            try
            {
                // Check if user exists
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                // Check if task exists and belongs to user
                var existingTask = await taskRepository.GetTaskById(id);
                if (existingTask == null || existingTask.UserId != userId)
                {
                    return Results.NotFound($"Task not found");
                }
                // Delete from repository
                var deleted = await taskRepository.DeleteTask(id);
                if (!deleted)
                {
                    return Results.Problem("Error deleting task");
                }
                // Return response
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting task: {ex.Message}");
            }
        }
        private static async Task<IResult> GetTaskById(
            string id,
            ITaskRepository taskRepository,
            HttpContext context)
        {
            // Logic to get a task by ID
            try
            {
                var task = await taskRepository.GetTaskById(id);
                if (task == null)
                {
                    return Results.NotFound($"Task with ID {id} not found.");
                }
                return Results.Ok(new
                {
                    id = task.Id,
                    title = task.Title,
                    description = task.Description,
                    dueDate = task.DueDate,
                    status = task.Status
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while retrieving Task: {ex.Message}");
            }
        }
        private static async Task<IResult> GetTasksByUser(
            ITaskRepository taskRepository,
            ICategoryRepository categoryRepository,
            ITagRepository tagRepository,
            HttpContext context)
        {
            // Logic to get tasks by user ID
            try
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                // Retrieve tasks with details
                var tasks = await taskRepository.GetTasksByUserWithDetails(userId);
                return Results.Ok(tasks.Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    description = t.Description,
                    dueDate = t.DueDate,
                    status = t.Status,
                    categories = categoryRepository
                        .GetCategoriesByUser(userId)
                        .Result
                        .Where(c => t.TaskCategories.Any(tc => tc.TaskId == t.Id))
                        .Select(c => new
                        {
                            id = c.Id,
                            name = c.Name,
                            color = c.Color
                        }),
                    tags = tagRepository
                        .GetTagsByTask(t.Id, userId)
                        .Result
                        .Select(tag => new
                        {
                            id = tag.Id,
                            name = tag.Name
                        })
                }));
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while retrieving Tasks: {ex.Message}");
            }
        }

        private static async Task<IResult> RemoveCategoryFromTask(
            string taskId, 
            string categoryId,
            ITaskRepository taskRepository,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to delete a task
            try
            {
                // Check if task exists
                var existingTask = await taskRepository.GetTaskById(taskId);
                if (existingTask == null)
                {
                    return Results.NotFound($"Task not found");
                }
                // Check if category exists
                var existingCategory = await categoryRepository.GetCategoryById(categoryId);
                if (existingCategory == null)
                {
                    return Results.NotFound($"Category not found");
                }
                // Remove category from task
                await taskRepository.RemoveCategoryFromTask(taskId, categoryId);
                return Results.Ok($"Category {categoryId} removed from Task {taskId} successfully");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error removing category from task: {ex.Message}");

            }
        }
        private static async Task<IResult> RemoveTagFromTask(
            string tagId,
            string taskId,
            ITagRepository tagRepository,
            ITaskRepository taskRepository,
            HttpContext context)
        {
            // Logic to delete a task
            try
            {
                // Get User From context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Check if task exists
                var existingTask = await taskRepository.GetTaskById(taskId);
                if (existingTask == null || existingTask.UserId != userId)
                    return Results.NotFound($"Task not found");
                // Check if tag exists
                var existingTag = await tagRepository.GetTagById(tagId);
                if (existingTag == null || existingTag.UserId != userId)
                    return Results.NotFound($"Tag not found");
                // Remove tag from task
                await taskRepository.RemoveTagFromTask(taskId, tagId);
                return Results.Ok($"Tag {tagId} removed from Task {taskId} successfully");

            }
            catch (Exception ex)
            {
                return Results.Problem($"Error removing tag from task: {ex.Message}");
            }

        }

        private static async Task<IResult> AddCategoryToTask(
            string taskId, 
            string categoryId,
            ITaskRepository taskRepository,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to add a category to a task
            try
            {
                // Get User From context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Check if task exists
                var existingTask = await taskRepository.GetTaskById(taskId);
                if (existingTask == null || existingTask.UserId != userId)
                    return Results.NotFound($"Task not found");

                // Check if category exists
                var existingCategory = await categoryRepository.GetCategoryById(categoryId);
                if (existingCategory == null || existingTask.UserId !=userId)
                {
                    return Results.NotFound($"Category not found");
                }
                // Add category to task
                await taskRepository.AddCategoryToTask(taskId, categoryId);
                return Results.Ok($"Category {categoryId} added to Task {taskId} successfully");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error adding category to task: {ex.Message}");
            }
        }
        private static async Task<IResult> AddTagToTask(
            string taskId,
            string tagId,
            ITagRepository tagRepository,
            ITaskRepository taskRepository,
            HttpContext context)
        {
            // Logic to add a tag to a task
            try
            {
                // Get User From context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Check if task exists
                var existingTask = await taskRepository.GetTaskById(taskId);
                if (existingTask == null || existingTask.UserId != userId)
                {
                    return Results.NotFound($"Task not found");
                }
                // Check if tag exists
                var existingTag = await tagRepository.GetTagById(tagId);
                if (existingTag == null || existingTask.UserId != userId)
                {
                    return Results.NotFound($"Tag not found");
                }
                // Add tag to task
                await taskRepository.AddTagToTask(taskId, tagId);
                return Results.Ok($"Tag {tagId} added to Task {taskId} successfully");

            }
            catch (Exception ex)
            {
                return Results.Problem($"Error adding tag to task: {ex.Message}");
            }
        }
    }
}
