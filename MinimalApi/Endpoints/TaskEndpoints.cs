using Application.Abstractions.Services;
using Application.DTOs.TaskDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MinimalApi.Endpoints
{

    /// <summary>
    /// Contains task-related API endpoints
    /// </summary>
    public static class TaskEndpoints
    {

        /// <summary>
        /// Configures the API endpoints for task-related operations.
        /// </summary>
        /// <remarks>This method defines a group of endpoints under the route <c>/api/tasks</c>, which are
        /// used to manage tasks and their associated categories and tags. The endpoints require authorization and
        /// include operations for creating, updating, deleting, and retrieving tasks, as well as managing task
        /// associations with categories and tags.</remarks>
        /// <param name="app">The <see cref="IEndpointRouteBuilder"/> used to configure the endpoints.</param>
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

            group.MapGet("/user/", GetTasksByUser)
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

        /// <summary>
        /// Creates a new task for the authenticated user based on the provided request data.
        /// </summary>
        /// <remarks>This method validates the input request, ensures the user is authenticated, checks
        /// for duplicate task titles, and verifies the task status before creating a new task. The created task is
        /// associated with the authenticated user.</remarks>
        /// <param name="request">The data required to create the task, including title, description, due date, and status.</param>
        /// <param name="taskService">The service used to interact with the task data store.</param>
        /// <param name="context">The HTTP context containing information about the current user and request.</param>
        /// <returns>An <see cref="IResult"/> representing the outcome of the operation. Possible results include: <list
        /// type="bullet"> <item><description><see cref="Results.Created"/> if the task is successfully
        /// created.</description></item> <item><description><see cref="Results.BadRequest"/> if the request data is
        /// invalid.</description></item> <item><description><see cref="Results.Unauthorized"/> if the user is not
        /// authenticated.</description></item> <item><description><see cref="Results.Conflict"/> if a task with the
        /// same title already exists for the user.</description></item> <item><description><see
        /// cref="Results.Problem"/> if an unexpected error occurs during task creation.</description></item> </list></returns>
        // CREATE TASK
        private static async Task<IResult> CreateTask(
            [FromBody] TaskCreateRequest request,
            ITaskService taskService,
            HttpContext context)
        {
            try
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(request.Title))
                    return Results.BadRequest("Task title is required.");

                var task = await taskService.CreateTask(
                    request.Title,
                    request.Description,
                    request.DueDate,
                    userId,
                    request.Status);

                if (task == null) return Results.Problem("Error creating task");

                var response = TaskResponse.FromDomain(task);
                return Results.Created($"/api/tasks/{task.Id}", response);
            }
            catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            catch (Exception ex) { return Results.Problem($"Error creating task: {ex.Message}"); }
        }

        // UPDATE TASK
        private static async Task<IResult> UpdateTask(
            string id,
            [FromBody] TaskUpdateRequest request,
            ITaskService taskService,
            HttpContext context)
        {
            try
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
            }
            catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            catch (Exception ex) { return Results.Problem($"Error updating task: {ex.Message}"); }
        }

        // DELETE TASK
        private static async Task<IResult> DeleteTask(
            string id,
            ITaskService taskService,
            HttpContext context)
        {
            try
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTask = await taskService.GetTaskById(id, userId);
                if (existingTask == null) return Results.NotFound("Task not found");

                var deleted = await taskService.DeleteTask(id);
                if (!deleted) return Results.Problem("Error deleting task");

                return Results.NoContent();
            }
            catch (Exception ex) { return Results.Problem($"Error deleting task: {ex.Message}"); }
        }

        // GET TASK BY ID
        private static async Task<IResult> GetTaskById(
            string id,
            ITaskService taskService,
            HttpContext context)
        {
            try
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var task = await taskService.GetTaskById(id, userId);
                if (task == null) return Results.NotFound($"Task with ID {id} not found.");

                var response = TaskResponse.FromDomain(task);
                return Results.Ok(response);
            }
            catch (Exception ex) { return Results.Problem($"Error retrieving task: {ex.Message}"); }
        }

        // GET TASKS BY USER
        private static async Task<IResult> GetTasksByUser(
            ITaskService taskService,
            HttpContext context)
        {
            try
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var tasks = await taskService.GetUserTasks(userId);
                var response = tasks.Select(TaskResponse.FromDomain);
                return Results.Ok(response);
            }
            catch (Exception ex) { return Results.Problem($"Error retrieving tasks: {ex.Message}"); }
        }

        // REMOVE CATEGORY FROM TASK
        private static async Task<IResult> RemoveCategoryFromTask(
            string taskId,
            string categoryId,
            ITaskService taskService,
            ICategoryService categoryService,
            HttpContext context)
        {
            try
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
            }
            catch (Exception ex) { return Results.Problem($"Error removing category: {ex.Message}"); }
        }

        // REMOVE TAG FROM TASK
        private static async Task<IResult> RemoveTagFromTask(
            string tagId,
            string taskId,
            ITagService tagService,
            ITaskService taskService,
            HttpContext context)
        {
            try
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
            }
            catch (Exception ex) { return Results.Problem($"Error removing tag: {ex.Message}"); }
        }

        // ADD CATEGORY TO TASK
        private static async Task<IResult> AddCategoryToTask(
            string taskId,
            string categoryId,
            ITaskService taskService,
            ICategoryService categoryService,
            HttpContext context)
        {
            try
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
            }
            catch (Exception ex) { return Results.Problem($"Error adding category: {ex.Message}"); }
        }

        // ADD TAG TO TASK
        private static async Task<IResult> AddTagToTask(
            string taskId,
            string tagId,
            ITagService tagService,
            ITaskService taskService,
            HttpContext context)
        {
            try
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
            }
            catch (Exception ex) { return Results.Problem($"Error adding tag: {ex.Message}"); }
        }
    }
}
