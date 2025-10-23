using Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs;
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
        /// <param name="taskRepository">The repository used to interact with the task data store.</param>
        /// <param name="context">The HTTP context containing information about the current user and request.</param>
        /// <returns>An <see cref="IResult"/> representing the outcome of the operation. Possible results include: <list
        /// type="bullet"> <item><description><see cref="Results.Created"/> if the task is successfully
        /// created.</description></item> <item><description><see cref="Results.BadRequest"/> if the request data is
        /// invalid.</description></item> <item><description><see cref="Results.Unauthorized"/> if the user is not
        /// authenticated.</description></item> <item><description><see cref="Results.Conflict"/> if a task with the
        /// same title already exists for the user.</description></item> <item><description><see
        /// cref="Results.Problem"/> if an unexpected error occurs during task creation.</description></item> </list></returns>
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
        /// <summary>
        /// Updates an existing task with the specified details.
        /// </summary>
        /// <remarks>This method validates the existence of the task and ensures that the updated title
        /// does not conflict with another task for the same user. If the update is successful, the updated task details
        /// are returned.</remarks>
        /// <param name="id">The unique identifier of the task to update.</param>
        /// <param name="request">An object containing the updated task details.</param>
        /// <param name="taskRepository">The repository used to access and update task data.</param>
        /// <param name="context">The HTTP context associated with the current request.</param>
        /// <returns>An <see cref="IResult"/> representing the outcome of the operation. Returns <see cref="Results.NotFound"/>
        /// if the task does not exist, <see cref="Results.Conflict"/> if a task with the same title already exists for
        /// the user, <see cref="Results.Problem"/> if an error occurs during the update, or <see cref="Results.Ok"/>
        /// with the updated task details on success.</returns>
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
        /// <summary>
        /// Deletes a task with the specified identifier if it exists and belongs to the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the task to delete.</param>
        /// <param name="taskRepository">The repository used to access and manage tasks.</param>
        /// <param name="context">The HTTP context containing the authenticated user's information.</param>
        /// <returns>A result indicating the outcome of the operation: <list type="bullet"> <item><description><see
        /// cref="Results.Unauthorized"/> if the user is not authenticated.</description></item> <item><description><see
        /// cref="Results.NotFound"/> if the task does not exist or does not belong to the user.</description></item>
        /// <item><description><see cref="Results.Problem"/> if an error occurs during deletion.</description></item>
        /// <item><description><see cref="Results.NoContent"/> if the task is successfully deleted.</description></item>
        /// </list></returns>
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
        /// <summary>
        /// Retrieves a task by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the task to retrieve. Cannot be null or empty.</param>
        /// <param name="taskRepository">The repository used to access task data.</param>
        /// <param name="context">The HTTP context associated with the current request.</param>
        /// <returns>An <see cref="IResult"/> representing the outcome of the operation. Returns <see cref="Results.Ok"/> with
        /// the task details if the task is found, <see cref="Results.NotFound"/> if the task does not exist, or <see
        /// cref="Results.Problem"/> if an error occurs during retrieval.</returns>
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
        /// <summary>
        /// Retrieves a list of tasks associated with the authenticated user, including their categories and tags.
        /// </summary>
        /// <remarks>This method retrieves tasks for the currently authenticated user based on their user
        /// ID, which is extracted from the HTTP context. Each task includes its details, associated categories, and
        /// tags. If the user is not authenticated, the method returns an unauthorized response. In case of an error, a
        /// problem response is returned with the error details.</remarks>
        /// <param name="taskRepository">The repository used to access task data.</param>
        /// <param name="categoryRepository">The repository used to access category data.</param>
        /// <param name="tagRepository">The repository used to access tag data.</param>
        /// <param name="context">The HTTP context containing the authenticated user's information.</param>
        /// <returns>An <see cref="IResult"/> containing the tasks with their associated categories and tags if the user is
        /// authenticated; otherwise, an unauthorized or error response.</returns>
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
        /// <summary>
        /// Removes a category from a specified task.
        /// </summary>
        /// <remarks>This method checks for the existence of both the task and category before
        /// <param name="taskId">Id to determine a Tag</param>
        /// <param name="categoryId">Id for Categories</param>
        /// <param name="taskRepository">Task REpository class</param>
        /// <param name="categoryRepository"> Category Repository class</param>
        /// <param name="context">HTTP context</param>
        /// <returns>NotFound, Problem, Error</returns>
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
        /// <summary>
        /// Removes Tag from a Task
        /// </summary>
        /// <remarks> Validate entities before deleting</remarks>
        /// <param name="tagId">Id for tags</param>
        /// <param name="taskId"> Id for Task</param>
        /// <param name="tagRepository">Repository class for Tags</param>
        /// <param name="taskRepository">Repository class for Task</param>
        /// <param name="context">HTTP context</param>
        /// <returns> Ok, NotFound, Unauthorized</returns>
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
        /// <summary>
        /// Adds a category to a specified task.
        /// </summary>
        /// <remarks>This method checks for the existence of both the task and category before adding the category to the task.</remarks>
        /// <param name="taskId"> Id for a task</param>
        /// <param name="categoryId">Id for a category</param>
        /// <param name="taskRepository">Repository class for Task</param>
        /// <param name="categoryRepository">Repository class for Category</param>
        /// <param name="context">HTTP context</param>
        /// <returns> Unauthorized, NotFound, Problem, Ok</returns>
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
        /// <summary>
        /// Adds a tag to a specified task.
        /// </summary>
        /// <remarks>This method checks for the existence of both the task and tag before adding the tag to the task.</remarks>
        /// <param name="taskId">Id references Task</param>
        /// <param name="tagId">Id references Tag</param>
        /// <param name="tagRepository">Repository class for Tags</param>
        /// <param name="taskRepository">Repository class for Task</param>
        /// <param name="context">HTTP context</param>
        /// <returns> NotFound, Ok, Unauthorized, Problem</returns>
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
