using Domain.Abstractions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs;
using System.Security.Claims;

namespace MinimalApi.Endpoints
{
    public static class TagEndpoints
    {
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
            group.MapGet("/user/{userId}", GetTagsByUser).RequireAuthorization()
                .WithSummary("Get Tags by User ID");

        }

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

        private static async Task<IResult> GetTagsByUser(
            ITagRepository tagRepository,
            HttpContext context)
        {
            // Logic to get tags by user ID
            try
            {
                // Get user fromcontext
                // Check user 
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
