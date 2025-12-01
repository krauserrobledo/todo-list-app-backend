using Application.Abstractions.Services;
using Application.DTOs.TagDTOs;
using System.Security.Claims;

namespace MinimalApi.Endpoints
{
    public static class TagEndpoints
    {
        public static void MapTagEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/tags")
                .WithTags("Tags");

            // Create Tag
            group.MapPost("/", async (TagCreateRequest request, ITagService tagService, HttpContext context) =>
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return Results.BadRequest("Tag name is required.");

                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var createdTag = await tagService.CreateTag(request.Name, userId);

                return Results.Created($"/api/tags/{createdTag.Id}", new
                {
                    id = createdTag.Id,
                    name = createdTag.Name
                });
            })
            .WithSummary("Create a new Tag");

            // Update Tag
            group.MapPut("/{id}", async (string id, TagUpdateRequest request, ITagService tagService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTag = await tagService.GetTagById(id, userId);
                if (existingTag == null) return Results.NotFound($"Tag with ID {id} not found.");

                var updatedTag = await tagService.UpdateTag(existingTag.Id, userId, request.Name);
                if (updatedTag == null) return Results.Problem("Failed to update the tag.");

                return Results.Ok(new
                {
                    id = existingTag.Id,
                    name = updatedTag.Name ?? existingTag.Name
                });
            })
            .WithSummary("Update an existing Tag");

            // Delete Tag
            group.MapDelete("/{id}", async (string id, ITagService tagService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var existingTag = await tagService.GetTagById(id, userId);
                if (existingTag == null) return Results.NotFound($"Tag with ID {id} not found.");

                var deleted = await tagService.DeleteTag(id, userId);
                if (!deleted) return Results.Problem("Failed to delete the tag.");

                return Results.NoContent();
            })
            .WithSummary("Delete a Tag by ID");

            // Get Tag by ID
            group.MapGet("/{id}", async (string id, ITagService tagService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var tag = await tagService.GetTagById(id, userId);
                if (tag == null) return Results.NotFound("Tag not found");

                return Results.Ok(new
                {
                    id = tag.Id,
                    name = tag.Name,
                    userId = tag.UserId
                });
            })
            .WithSummary("Get a Tag by ID");

            // Get Tags by User
            group.MapGet("/user/", async (ITagService tagService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var tags = await tagService.GetUserTags(userId);
                var response = tags.Select(TagResponse.FromDomain);

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithSummary("Get Tags by User ID");
        }
    }
}
