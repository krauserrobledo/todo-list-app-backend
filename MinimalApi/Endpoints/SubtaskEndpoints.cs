using Application.Abstractions.Services;
using Application.DTOs.SubtaskDTOs;
using Microsoft.AspNetCore.Mvc;

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
            group.MapPost("/task/{taskId}", CreateSubtask)
                .WithSummary("Create a new subtask for a specific task")
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
        /// <param name="subtaskService">The subtask repository.</param>
        /// <param name="taskService">The task repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns the created subtask.</returns>
        private static async Task<IResult> CreateSubtask(
            string taskId,
            [FromBody] SubtaskCreateRequest request,
            ISubtaskService subtaskService,
            HttpContext context)
        {
            // Logic to create a subtask
            try
            {
                // Get user ID from context (if needed for further validation)
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                // Validate request
                if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest("Title is required.");

                if (string.IsNullOrWhiteSpace(taskId)) return Results.BadRequest("TaskId is required.");

                // Save to repository
                var subtask = await subtaskService.CreateSubtask
                    (
                    request.Title,
                    taskId,
                    userId
                    );

                var response = SubtaskResponse.FromDomain(subtask);

                // Return success response
                return Results.Created($"/api/subtasks/{subtask.Id}", response);
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
        /// <param name="subtaskService">The subtask Service.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns the updated subtask.</returns>
        private static async Task<IResult> UpdateSubtask(
           string id,
           [FromBody] SubtaskUpdateRequest request,
           ISubtaskService subtaskService,
           HttpContext context)
        {
            try
            {
                // get user from context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                // Check if the subtask exists
                var existingSubtask = await subtaskService.GetSubtaskById(id,userId);

                if (existingSubtask == null) return Results.NotFound($"Subtask with ID {id} not found");

                // Update properties
                if (!string.IsNullOrWhiteSpace(request.Title)) existingSubtask.Title = request.Title;

                // Service call
                var updatedSubtask = await subtaskService.UpdateSubtask(

                    id, 
                    request.Title, 
                    userId);

                if (updatedSubtask == null) return Results.NotFound($"Subtask with ID {id} not found");

                // Response
                var response = SubtaskResponse.FromDomain(updatedSubtask); 

                return Results.Ok(response);
            }

            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
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
        /// <param name="subtaskService">The subtask service.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns a result indicating the outcome of the delete operation.</returns>
        private static async Task<IResult> DeleteSubtask(
            string id,
            ISubtaskService subtaskService,
            HttpContext context)
        {

            try
            {
                //get user from context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userId == null) return Results.Unauthorized();

                // Verify ID
                var existingSubtask = await subtaskService.GetSubtaskById(id, userId);

                if (existingSubtask == null) return Results.NotFound($"Subtask with ID {id} not found");

                // Service call
                var deleted = await subtaskService.DeleteSubtask(id, userId);

                // Verify deletion
                if (!deleted) return Results.NotFound($"Subtask with ID {id} not found");

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
        /// <param name="subtaskService"> Subtask Service Class</param>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        private static async Task<IResult> GetAllSubtasksByTask(
            string taskId,
            ISubtaskService subtaskService,
            HttpContext context)
        {

            try
            {
                //Get user from context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if(userId == null) return Results.Unauthorized();

                // Service call
                var subtasks = await subtaskService.GetSubtasksByTask(taskId, userId);

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
        /// <param name="subtaskService">Service Class for subtask</param>
        /// <param name="context"> HTTP Context</param>
        /// <returns> Ok, Problem, Not Found</returns>
        private static async Task<IResult> GetSubtaskById(
            string id,
            ISubtaskService subtaskService,
            HttpContext context)
        {

            try
            {
                //Get user by context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userId == null) return Results.Unauthorized(); 

                // Service call
                var subtask = await subtaskService.GetSubtaskById(id, userId);

                // Conditional Return
                if (subtask == null) return Results.NotFound($"Subtask with ID {id} not found");

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