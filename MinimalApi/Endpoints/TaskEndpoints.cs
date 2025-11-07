using Application.Abstractions.Services;
using Application.DTOs.TaskDTOs;
using Application.Services;
using Azure;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs.TaskDTOs;
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
        private static async Task<IResult> CreateTask(
            [FromBody] TaskCreateRequest request,
            ITaskService taskService,
            HttpContext context)
        {

            // Logic to create a task
            try
            {

                // Get user ID from context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();


                // Validate request
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Results.BadRequest("Task title is required.");

                // Create new Task entity
                var task = await taskService.CreateTask(
                    request.Title,
                    request.Description,
                    request.DueDate,
                    userId,
                    request.Status);

                // Save to repository
                var response = TaskResponse.FromDomain(task);
                return Results.Created($"/api/tasks/{task.Id}", response);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating task: {ex.Message}");
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
        /// <param name="taskService">The service used to access and update task data.</param>
        /// <param name="context">The HTTP context associated with the current request.</param>
        /// <returns>An <see cref="IResult"/> representing the outcome of the operation. Returns <see cref="Results.NotFound"/>
        /// if the task does not exist, <see cref="Results.Conflict"/> if a task with the same title already exists for
        /// the user, <see cref="Results.Problem"/> if an error occurs during the update, or <see cref="Results.Ok"/>
        /// with the updated task details on success.</returns>
        private static async Task<IResult> UpdateTask(
            string id,
            [FromBody] TaskUpdateRequest request,
            ITaskService taskService,
            HttpContext context)
        {
            // Logic to update a task
            try
            {
                // Get user from context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Check if exists
                var existingTask = await taskService.GetTaskById(id, userId);
                if (existingTask == null)
                {
                    return Results.NotFound($"Task not found");
                }
                // Check for duplicate title
                var taskExists = await taskService.TaskTitleExists(request.Title, existingTask.UserId);
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
                var updatedTask = await taskService.UpdateTask(
                   id, request.Title, request.Description,
                   request.DueDate);

                if (updatedTask == null)
                    return Results.NotFound($"Task with ID {id} not found or access denied");

                var response = TaskResponse.FromDomain(updatedTask);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
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
        /// <param name="taskService">The service used to access and manage tasks.</param>
        /// <param name="context">The HTTP context containing the authenticated user's information.</param>
        /// <returns>A result indicating the outcome of the operation: <list type="bullet"> <item><description><see
        /// cref="Results.Unauthorized"/> if the user is not authenticated.</description></item> <item><description><see
        /// cref="Results.NotFound"/> if the task does not exist or does not belong to the user.</description></item>
        /// <item><description><see cref="Results.Problem"/> if an error occurs during deletion.</description></item>
        /// <item><description><see cref="Results.NoContent"/> if the task is successfully deleted.</description></item>
        /// </list></returns>
        private static async Task<IResult> DeleteTask(
            string id,
            ITaskService taskService,
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
                var existingTask = await taskService.GetTaskById(id, userId);
                if (existingTask == null || existingTask.UserId != userId)
                {
                    return Results.NotFound($"Task not found");
                }
                // Delete from repository
                var deleted = await taskService.DeleteTask(id);
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
        /// <param name="taskService">The service used to access task data.</param>
        /// <param name="context">The HTTP context associated with the current request.</param>
        /// <returns>An <see cref="IResult"/> representing the outcome of the operation. Returns <see cref="Results.Ok"/> with
        /// the task details if the task is found, <see cref="Results.NotFound"/> if the task does not exist, or <see
        /// cref="Results.Problem"/> if an error occurs during retrieval.</returns>
        private static async Task<IResult> GetTaskById(
            string id,
            ITaskService taskService,
            HttpContext context)
        {
            // Logic to get a task by ID
            try
            {
                // Get user from context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var task = await taskService.GetTaskById(id, userId);

                if (task == null) return Results.NotFound($"Task with ID {id} not found.");

                return Results.Ok(new

                {
                    id = task.Id,
                    title = task.Title,
                    description = task.Description,
                    dueDate = task.DueDate,
                    status = task.Status,
                    userId = task.UserId,
                    subtasks = task.Subtasks?.Select(st => new
                    {
                        id = st.Id,
                        title = st.Title,
                        createdAt = st.CreatedAt
                    }) ?? Enumerable.Empty<object>(),
                    categories = task.TaskCategories?.Select(tc => new
                    {
                        id = tc.Category?.Id,
                        name = tc.Category?.Name,
                        color = tc.Category?.Color
                    }) ?? Enumerable.Empty<object>(),
                    tags = task.TaskTags?.Select(tt => new
                    {
                        id = tt.Tag?.Id,
                        name = tt.Tag?.Name
                    }) ?? Enumerable.Empty<object>()
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
        /// <param name="taskService">The service used to access task data.</param>
        /// <param name="categoryService">The service used to access category data.</param>
        /// <param name="tagService">The service used to access tag data.</param>
        /// <param name="context">The HTTP context containing the authenticated user's information.</param>
        /// <returns>An <see cref="IResult"/> containing the tasks with their associated categories and tags if the user is
        /// authenticated; otherwise, an unauthorized or error response.</returns>
        private static async Task<IResult> GetTasksByUser(
            ITaskService taskService,
            ICategoryService categoryService,
            ITagService tagService,
            HttpContext context)
        {

            // Logic to get tasks by user ID
            try
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                // Retrieve tasks with details
                var tasks = await taskService.GetUserTasks(userId);
                return Results.Ok(tasks.Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    description = t.Description,
                    dueDate = t.DueDate,
                    status = t.Status,
                    userId = t.UserId,
                    subtasks = t.Subtasks?.Select(st => new
                    {
                        id = st.Id,
                        title = st.Title,
                        createdAt = st.CreatedAt
                    }) ?? Enumerable.Empty<object>(),
                    categories = t.TaskCategories?.Select(tc => new
                    {
                        id = tc.Category?.Id,
                        name = tc.Category?.Name,
                        color = tc.Category?.Color
                    }) ?? Enumerable.Empty<object>(),
                    tags = t.TaskTags?.Select(tt => new
                    {
                        id = tt.Tag?.Id,
                        name = tt.Tag?.Name
                    }) ?? Enumerable.Empty<object>()
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
        /// <param name="taskService">Task Service class</param>
        /// <param name="categoryService"> Category Service class</param>
        /// <param name="context">HTTP context</param>
        /// <returns>NotFound, Problem, Error</returns>
        private static async Task<IResult> RemoveCategoryFromTask(
            string taskId, 
            string categoryId,
            ITaskService taskService,
            ICategoryService categoryService,
            HttpContext context)
        {
            // Logic to delete a task
            try
            {
                // Get user from context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                // Check if task exists
                var existingTask = await taskService.GetTaskById(taskId, userId);

                if (existingTask == null) return Results.NotFound($"Task not found");


                // Check if category exists
                var existingCategory = await categoryService.GetCategoryById(categoryId);

                if (existingCategory == null) return Results.NotFound($"Category not found");

                // Remove category from task
                await taskService.RemoveCategoryFromTask(taskId, categoryId);

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
        /// <param name="tagService">Service class for Tags</param>
        /// <param name="taskService">Service class for Task</param>
        /// <param name="context">HTTP context</param>
        /// <returns> Ok, NotFound, Unauthorized</returns>
        private static async Task<IResult> RemoveTagFromTask(
            string tagId,
            string taskId,
            ITagService tagService,
            ITaskService taskService,
            HttpContext context)
        {
            // Logic to delete a task
            try
            {
                // Get User From context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                // Check if task exists
                var existingTask = await taskService.GetTaskById(taskId, userId);

                if (existingTask == null || existingTask.UserId != userId) return Results.NotFound($"Task not found");

                // Check if tag exists
                var existingTag = await tagService.GetTagById(tagId, userId);

                if (existingTag == null) return Results.NotFound($"Tag not found");

                // Remove tag from task
                await taskService.RemoveTagFromTask(taskId, tagId);
                
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
        /// <param name="taskService">Service class for Task</param>
        /// <param name="categoryService">Service class for Category</param>
        /// <param name="context">HTTP context</param>
        /// <returns> Unauthorized, NotFound, Problem, Ok</returns>
        private static async Task<IResult> AddCategoryToTask(
            string taskId, 
            string categoryId,
            ITaskService taskService,
            ICategoryService categoryService,
            HttpContext context)
        {

            // Logic to add a category to a task
            try
            {

                // Get User From context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                // Check if task exists
                var existingTask = await taskService.GetTaskById(taskId, userId);

                if (existingTask == null || existingTask.UserId != userId) return Results.NotFound($"Task not found");

                // Check if category exists
                var existingCategory = await categoryService.GetCategoryById(categoryId);

                if (existingCategory == null || existingTask.UserId !=userId) return Results.NotFound($"Category not found");

                // Add category to task
                await taskService.AddCategoryToTask(taskId, categoryId);

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
        /// <param name="tagService">Service class for Tags</param>
        /// <param name="taskService">Service class for Task</param>
        /// <param name="context">HTTP context</param>
        /// <returns> NotFound, Ok, Unauthorized, Problem</returns>
        private static async Task<IResult> AddTagToTask(
            string taskId,
            string tagId,
            ITagService tagService,
            ITaskService taskService,
            HttpContext context)
        {

            // Logic to add a tag to a task
            try
            {

                // Get User From context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                // Check if task exists
                var existingTask = await taskService.GetTaskById(taskId, userId);

                if (existingTask == null || existingTask.UserId != userId) return Results.NotFound($"Task not found");

                // Check if tag exists
                var existingTag = await tagService.GetTagById(tagId, userId);

                if (existingTag == null || existingTask.UserId != userId) return Results.NotFound($"Tag not found");

                // Add tag to task
                await taskService.AddTagToTask(taskId, tagId);

                return Results.Ok($"Tag {tagId} added to Task {taskId} successfully");
            }

            catch (Exception ex)
            {
                return Results.Problem($"Error adding tag to task: {ex.Message}");
            }
        }
    }
}
