using Application.Abstractions.Services;
using Application.DTOs.CategoryDTOs;
using Application.DTOs.TagDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MinimalApi.Endpoints
{

    /// <summary>
    /// Configures the API endpoints for tag-related operations.
    /// </summary>
    public static class TagEndpoints
    {

        private static string? newTag;

        /// <summary>
        /// Configures the API endpoints for tag-related operations.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public static void MapTagEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/tags")
                .WithTags("Tags");
            // Define tag-r
            // elated endpoints here
            group.MapPost("/", CreateTag)
                .WithSummary("Create a new Tag");

            group.MapPut("/{id}", UpdateTag)
                .WithSummary("Update an existing Tag");

            group.MapDelete("/{id}", DeleteTag)
                .WithSummary("Delete a Tag by ID");

            group.MapGet("/{id}", GetTagById)
                .WithSummary("Get a Tag by ID");

            group.MapGet("/user/", GetTagsByUser).RequireAuthorization()
                .WithSummary("Get Tags by User ID");
        }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <remarks> Validates if a tag with the same name exists for the user before creating a new one.</remarks>
        /// <param name="request">The tag creation request.</param>
        /// <param name="tagRepository"> The tag Repository Interface Implementation</param>
        /// <param name="context"></param>
        /// <returns>Returns the created Tag.</returns>
        private static async Task<IResult> CreateTag(
            [FromBody] TagCreateRequest request,
            ITagService tagService,
            HttpContext context, string? newTag)
        {

            // Logic to create a tag
            try
            {

                // Validate request
                if (string.IsNullOrWhiteSpace(request.Name))
                    return Results.BadRequest("Tag name is required.");

                // Get user ID from context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                // Create new Tag entity' without using direct domain model

                // Save to repository
                var createdTag = await tagService.CreateTag(newTag, userId);

                // Return success response
                return Results.Created($"/api/tags/{createdTag.Id}", new
                {
                    Name = request.Name,
                    id = createdTag.Id,
                    name = createdTag.Name,
                });
            }

            catch (Exception ex)
            {
                // Log exception 
                return Results.Problem("An error occurred while creating the tag: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        /// <remarks> Validates if the tag exists and updates its details.</remarks>
        /// <param name="id">The ID of the tag to update.</param>
        /// <param name="request">The tag update request.</param>
        /// <param name="tagService">The tag service.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns the updated Tag.</returns>
        private static async Task<IResult> UpdateTag(
            string id,
            [FromBody] TagUpdateRequest request,
            ITagService tagService,
            HttpContext context)
        {
            // Logic to update a tag
            try
            {
                // Get user ID from context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //Check if EXISTS
                var existingTag = await tagService.GetTagById(id, userId);

                if (existingTag == null)
                {

                    return Results.NotFound($"Tag with ID {id} not found.");
                }

                // Check if Name exist or holding old name
                if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != existingTag.Name)
                {

                    existingTag.Name = request.Name;
                }

                // Update in repository
                var updatedTag = await tagService.UpdateTag(existingTag.Id, userId, request.Name);

                if (updatedTag == null)
                {
                    return Results.Problem("Failed to update the tag.");
                }

                // Return success response
                return Results.Ok(new
                {

                    id = existingTag.Id,
                    name = updatedTag.Name ?? existingTag.Name
                });
            }

            catch (Exception ex)
            {

                return Results.Problem(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a tag by ID.
        /// </summary>
        /// <remarks> Validates if the tag exists before deletion.</remarks>
        /// <param name="id">The ID of the tag to delete.</param>
        /// <param name="tagService">The tag service.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns a result indicating the outcome of the deletion.</returns>
        private static async Task<IResult> DeleteTag(
            string id,
            ITagService tagService,
            HttpContext context)
        {

            // Logic to delete a tag
            try
            {

                // Get user ID from context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Id Validation
                var existingTag = await tagService.GetTagById(id, userId);

                if (existingTag == null)
                {

                    return Results.NotFound($"Tag with ID {id} not found.");
                }

                // Service call
                var deleted = await tagService.DeleteTag(id, userId);

                if (!deleted)
                {

                    return Results.Problem("Failed to delete the tag.");
                }

                // Return success response
                return Results.NoContent();
            }

            catch (Exception ex)
            {

                return Results.Problem($"Error deleting tag: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a tag by ID.
        /// </summary>
        /// <param name="id">The ID of the tag to retrieve.</param>
        /// <param name="tagService">The tag service.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns the requested tag or a not found response.</returns>
        private static async Task<IResult> GetTagById(
            string id,
            ITagService tagService,
            HttpContext context)
        {

            // Logic to get a tag by ID
            try
            {

                // Get user ID from context
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Results.Unauthorized();


                // Id Validation
                var tag = await tagService.GetTagById(id, userId);

                if (tag == null)
                    return Results.NotFound($"Tag not Found");

                // Return success response
                return Results.Ok(new
                {

                    id = tag.Id,
                    name = tag.Name,
                    userId = tag.UserId
                });
            }

            catch (Exception ex)
            {

                return Results.Problem($" Error getting Tag: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets tags by user ID.
        /// </summary>
        /// <remarks> Retrieves all tags associated with the authenticated user.</remarks>
        /// <param name="tagService">Tag Service</param>
        /// <param name="context">HTTP context</param>
        /// <returns>Returns a list of tags associated with the user.</returns>
        private static async Task<IResult> GetTagsByUser(
            ITagService tagService,
            HttpContext context)
        {

            // Logic to get tags by user ID
            try
            {

                // Check user / Get user fromcontext
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var tag = await tagService.GetUserTags( userId);


                var response = tag.Select(TagResponse.FromDomain);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving categories: {ex.Message}");
            }
        }
    }
}

