using Application.Abstractions.Services;
using Application.DTOs.CategoryDTOs;
using MinimalApi.Extensions;

namespace MinimalApi.Endpoints
{
    public static class CategoryEndpoints
    {
        /// <summary>
        /// Maps the category endpoints to the specified web application.
        /// </summary>
        /// <param name="app">Application to map the endpoints to.</param>
        public static void MapCategoryEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/categories")
                .WithTags("Categories")
                .RequireAuthorization();

            // Create a Category
            group.MapPost("/", async (CategoryCreateRequest request, ICategoryService categoryService, HttpContext context) =>
            {
                try
                {
                    var category = await categoryService.CreateCategory(
                        request.Name, request.Color, context.RequireUserId());

                    return Results.Created($"/api/categories/{category.Id}", CategoryResponse.FromDomain(category));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
                catch (ArgumentException ex) { return Results.BadRequest(ex.Message); }
                catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            }).WithSummary("Create a new Category");

            // Update an existing Category
            group.MapPut("/{id}", async (string id, CategoryUpdateRequest request, ICategoryService categoryService, HttpContext context) =>
            {
                try
                {
                    var updated = await categoryService.UpdateCategory(
                        id, context.RequireUserId(), request.Name, request.Color);

                    return updated is null
                        ? Results.NotFound($"Category {id} not found or access denied")
                        : Results.Ok(CategoryResponse.FromDomain(updated));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
                catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
            }).WithSummary("Update an existing Category");

            //Delete an existing Category
            group.MapDelete("/{id}", async (string id, ICategoryService categoryService, HttpContext context) =>
            {
                try
                {
                    var deleted = await categoryService.DeleteCategory(id, context.RequireUserId());
                    return deleted ? Results.NoContent() : Results.NotFound($"Category {id} not found or access denied");
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            }).WithSummary("Delete an existing Category");

            //Get a Category by Id
            group.MapGet("/{id}", async (string id, ICategoryService categoryService, HttpContext context) =>
            {
                try
                {
                    var category = await categoryService.GetCategoryById(id);
                    return category is null
                        ? Results.NotFound($"Category {id} not found")
                        : Results.Ok(CategoryResponse.FromDomain(category));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            }).WithSummary("Get details from an existing Category");

            // Get Categories by User
            group.MapGet("/user/", async (ICategoryService categoryService, HttpContext context) =>
            {
                try
                {
                    var categories = await categoryService.GetUserCategories(context.RequireUserId());
                    return Results.Ok(categories.Select(CategoryResponse.FromDomain));
                }
                catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
            }).WithSummary("Get all Categories which belong to current user");
        }
    }
}
