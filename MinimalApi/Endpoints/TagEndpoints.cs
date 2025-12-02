using Application.Abstractions.Services;
using Application.DTOs.TagDTOs;
using MinimalApi.Extensions;

namespace MinimalApi.Endpoints
{
    public static class TagEndpoints
    {
        public static void MapTagEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/tags")
                .WithTags("Tags")
                .RequireAuthorization();

            // Create Tag
            group.MapPost("/", async (TagCreateRequest request, ITagService tagService, HttpContext context) =>
            {
                try
                {
                    var tag = await tagService.CreateTag(request.Name, context.RequireUserId());
                    return Results.Created($"/api/tags/{tag.Id}", TagResponse.FromDomain(tag));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
                catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
                catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            })
            .WithSummary("Create a new Tag");

            // Update Tag
            group.MapPut("/{id}", async (string id, TagUpdateRequest request, ITagService tagService, HttpContext context) =>
            {
                try
                {
                    var updated = await tagService.UpdateTag(id, context.RequireUserId(), request.Name);
                    return updated is null
                        ? Results.NotFound($"Tag {id} not found or access denied")
                        : Results.Ok(TagResponse.FromDomain(updated));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
                catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            })
            .WithSummary("Update an existing Tag");

            // Delete Tag
            group.MapDelete("/{id}", async (string id, ITagService tagService, HttpContext context) =>
            {
                try
                {
                    var deleted = await tagService.DeleteTag(id, context.RequireUserId());
                    return deleted ? Results.NoContent() : Results.NotFound($"Tag {id} not found or access denied");
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            })
            .WithSummary("Delete a Tag by ID");

            // Get Tag by ID
            group.MapGet("/{id}", async (string id, ITagService tagService, HttpContext context) =>
            {
                try
                {
                    var tag = await tagService.GetTagById(id, context.RequireUserId());
                    return tag is null
                        ? Results.NotFound($"Tag {id} not found or access denied")
                        : Results.Ok(TagResponse.FromDomain(tag));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
                catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
            })
            .WithSummary("Get a Tag by ID");

            // Get Tags by User
            group.MapGet("/user", async (ITagService tagService, HttpContext context) =>
            {
                try
                {
                    var tags = await tagService.GetUserTags(context.RequireUserId());
                    return Results.Ok(tags.Select(TagResponse.FromDomain));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            })
            .WithSummary("Get Tags by User ID");
        }
    }
}
