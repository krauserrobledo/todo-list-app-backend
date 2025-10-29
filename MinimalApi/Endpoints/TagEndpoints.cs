using Domain.Abstractions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs.TagDTOs;
using System.Security.Claims;

namespace MinimalApi.Endpoints
{
    /// <summary>
    /// Configures the API endpoints for tag-related operations.
    /// </summary>
    public static class TagEndpoints
    {
        /// <summary>
        /// Configures the API endpoints for tag-related operations.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public static void MapTagEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/tags")
                .WithTags("Tags");
            // Define tag-related endpoints here
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
            [FromBody] TagCreateDTO request,
            ITagRepository tagRepository,
            HttpContext context)
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
                // Check for duplicate tag name for the same user
                var nameExists = await tagRepository.TagNameExists(request.Name, userId);
                if (nameExists)
                {
                    return Results.Conflict("A tag with the same name already exists for this user.");
                }
                // Create new Tag entity'
                var newTag = new Tag
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Name = request.Name
                };
                // Save to repository
                var createdTag = await tagRepository.CreateTag(newTag);
                // Return success response
                return Results.Created($"/api/tags/{createdTag.Id}", new
                {
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
        /// <param name="tagRepository">The tag repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns the updated Tag.</returns>
        private static async Task<IResult> UpdateTag(
            string id,
            [FromBody] TagUpdateDTO request,
            ITagRepository tagRepository,
            HttpContext context)
        {
            // Logic to update a tag
            try
            {
                //Check if EXISTS
                var existingTag = await tagRepository.GetTagById(id);
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
                var updatedTag = await tagRepository.UpdateTag(existingTag);
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
        /// <param name="tagRepository">The tag repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns a result indicating the outcome of the deletion.</returns>
        private static async Task<IResult> DeleteTag(
            string id,
            ITagRepository tagRepository,
            HttpContext context)
        {
            // Logic to delete a tag
            try
            {
                // Id Validation
                var existingTag = await tagRepository.GetTagById(id);
                if (existingTag == null)
                {
                    return Results.NotFound($"Tag with ID {id} not found.");
                }
                // Repository call
                var deleted = await tagRepository.DeleteTag(id);

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
        /// <param name="tagRepository">The tag repository.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Returns the requested tag or a not found response.</returns>
        private static async Task<IResult> GetTagById(
            string id,
            ITagRepository tagRepository,
            HttpContext context)
        {
            // Logic to get a tag by ID
            try
            {
                // Id Validation
                var tag = await tagRepository.GetTagById(id);
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
        /// <param name="tagRepository">Tag Repository</param>
        /// <param name="context">HTTP context</param>
        /// <returns>Returns a list of tags associated with the user.</returns>
        private static async Task<IResult> GetTagsByUser(
            ITagRepository tagRepository,
            HttpContext context)
        {
            // Logic to get tags by user ID
            try
            {
                // Check user / Get user fromcontext
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var tags = await tagRepository.GetTagsByUser(userId);
                var result = tags.Select(tag => new
                {
                    id = tag.Id,
                    name = tag.Name
                });
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem($" Error getting Tags: {ex.Message}");
            }
        }
    }
}
