using Domain.Abstractions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs.SubtaskDTOs;

namespace MinimalApi.Endpoints
{
    /// <summary>
    /// Configures the API endpoints for subtask-related operations.
    /// </summary>
    public static class SubtaskEndpoints
    {
        /// <summary>
        /// Configures the API endpoints for subtask-related operations.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public static void MapSubtaskEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/subtasks")
                .WithTags("Subtasks");
            // Define subtask-related endpoints here
            group.MapPost("/", CreateSubtask)
                .WithSummary("Create a new subtask")
                .RequireAuthorization();
            group.MapPut("/{id}", UpdateSubtask)
                .WithSummary("Update an existing subtask")
                .RequireAuthorization();
            group.MapDelete("/{id}", DeleteSubtask)
                .WithSummary("Delete a subtask by ID")
                .RequireAuthorization();
            group.MapGet("/task/{taskId}", GetAllSubtasksByTask)
                .WithSummary("Get all subtasks for a specific task")
                .RequireAuthorization();
            group.MapGet("/{id}", GetSubtaskById)
                .WithSummary("Get a subtask by ID")
                .RequireAuthorization();   
        }
        /// <summary>
        /// Creates a new subtask.
        /// </summary>
        /// <remarks>Validates the request and creates a new subtask if the task exists.</remarks>
        /// <param name="request">The subtask creation request.</param>
        /// <param name="subtaskRepository">The subtask repository.</param>
        /// <param name="taskRepository">The task repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns the created subtask.</returns>
        private static async Task<IResult> CreateSubtask(
            [FromBody] SubtaskCreateDTO request,
            ISubtaskRepository subtaskRepository,
            ITaskRepository taskRepository,
            HttpContext context)
        {
            // Logic to create a subtask
            try
            {
                // Get user ID from context (if needed for further validation)
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Results.BadRequest("Title is required.");
                if (string.IsNullOrWhiteSpace(request.TaskId))
                    return Results.BadRequest("TaskId is required.");
                // Check if the associated task exists and belongs to the user
                var task = await taskRepository.GetTaskById(request.TaskId);
                if (task == null)
                    return Results.BadRequest("The specified TaskId does not exist.");
                // Create new Subtask entity
                var newSubtask = new Subtask
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = request.Title,
                    TaskId = request.TaskId
                };
                // Save to repository
                var createdSubtask = await subtaskRepository.CreateSubtask(newSubtask);
                // Return success response
                return Results.Created($"/api/subtasks/{createdSubtask.Id}", new
                {
                    id = createdSubtask.Id,
                    title = createdSubtask.Title,
                    taskId = createdSubtask.TaskId
                });
            }
            catch (Exception ex)
            {
                // Log exception 
                return Results.Problem("An error occurred while creating the subtask: " + ex.Message);
            }
        }
        /// <summary>
        /// Updates an existing subtask.
        /// </summary>
        /// <remarks>Validates the request and updates the subtask if it exists.</remarks>
        /// <param name="id">The ID of the subtask to update.</param>
        /// <param name="request">The subtask update request.</param>
        /// <param name="subtaskRepository">The subtask repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns the updated subtask.</returns>
        private static async Task<IResult> UpdateSubtask(
           string id,
           [FromBody] SubtaskUpdateDTO request,
           ISubtaskRepository subtaskRepository,
           HttpContext context)
        {
            try
            {
                // Check if the subtask exists
                var existingSubtask = await subtaskRepository.GetSubtaskById(id);
                if (existingSubtask == null)
                    return Results.NotFound($"Subtask with ID {id} not found");
                // Update properties
                if (!string.IsNullOrWhiteSpace(request.Title))
                    existingSubtask.Title = request.Title;
                // Repository call
                var updatedSubtask = await subtaskRepository.UpdateSubtask(existingSubtask);
                if (updatedSubtask == null)
                    return Results.NotFound($"Subtask with ID {id} not found");
                // Return response
                return Results.Ok(new
                {
                    id = updatedSubtask.Id,
                    title = updatedSubtask.Title,
                    taskId = updatedSubtask.TaskId
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating subtask: {ex.Message}");
            }
        }
        /// <summary>
        /// Deletes a subtask by ID.
        /// </summary>
        /// <param name="id">The ID of the subtask to delete.</param>
        /// <param name="subtaskRepository">The subtask repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns a result indicating the outcome of the delete operation.</returns>
        private static async Task<IResult> DeleteSubtask(
            string id,
            ISubtaskRepository subtaskRepository,
            HttpContext context)
        {
            try
            {
                // Verify ID
                var existingSubtask = await subtaskRepository.GetSubtaskById(id);
                if (existingSubtask == null)
                    return Results.NotFound($"Subtask with ID {id} not found");
                // Repository call
                var deleted = await subtaskRepository.DeleteSubtask(id);
                // Verify deletion
                if (!deleted)
                    return Results.NotFound($"Subtask with ID {id} not found");
                // Return response
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting subtask: {ex.Message}");
            }
        }
        /// <summary>
        /// Gets all subtasks for a specific task.
        /// </summary>
        /// <remarks>Retrieves all subtasks associated with the specified task ID.</remarks>
        /// <param name="taskId">The ID of the task to retrieve subtasks for.</param>
        /// <param name="subtaskRepository"> Subtask Repository Class</param>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        private static async Task<IResult> GetAllSubtasksByTask(
            string taskId,
            ISubtaskRepository subtaskRepository,
            HttpContext context)
        {
            try
            {
                // Repository call
                var subtasks = await subtaskRepository.GetAllSubtasksByTask(taskId);
                // Return response
                return Results.Ok(subtasks.Select(st => new
                {
                    id = st.Id,
                    title = st.Title,
                    taskId = st.TaskId
                }));
            }
            /// Handle exceptions
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving subtasks: {ex.Message}");
            }
        }
        /// <summary>
        /// Gets a subtask by ID.
        /// </summary>
        /// <remarks>Retrieves a subtask based on its unique identifier.</remarks>
        /// <param name="id">Subtask Id</param>
        /// <param name="subtaskRepository">Repository Class </param>
        /// <param name="context"> HTTP Context</param>
        /// <returns> Ok, Problem, Not Found</returns>
        private static async Task<IResult> GetSubtaskById(
            string id,
            ISubtaskRepository subtaskRepository,
            HttpContext context)
        {
            try
            {
                // Repository call
                var subtask = await subtaskRepository.GetSubtaskById(id);
                // Conditional Return
                if (subtask == null)
                    return Results.NotFound($"Subtask with ID {id} not found");
                return Results.Ok(new
                {
                    id = subtask.Id,
                    title = subtask.Title,
                    taskId = subtask.TaskId
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving subtask: {ex.Message}");
            }
        }
    }
}