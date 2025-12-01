using Application.Abstractions.Services;
using Application.DTOs.CategoryDTOs;
using System.Security.Claims;

namespace MinimalApi.Endpoints
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/categories")
                .WithTags("Categories")
                .RequireAuthorization();

            // Create Category
            group.MapPost("/", async (CategoryCreateRequest request, ICategoryService categoryService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var category = await categoryService.CreateCategory(request.Name, request.Color, userId);
                var response = CategoryResponse.FromDomain(category);

                return Results.Created($"/api/categories/{category.Id}", response);
            })
            .WithSummary("Create a new Category");

            // Update Category
            group.MapPut("/{id}", async (string id, CategoryUpdateRequest request, ICategoryService categoryService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var updatedCategory = await categoryService.UpdateCategory(id, userId, request.Name, request.Color);
                if (updatedCategory == null) return Results.NotFound($"Category with ID {id} not found or access denied");

                var response = CategoryResponse.FromDomain(updatedCategory);
                return Results.Ok(response);
            })
            .WithSummary("Update an existing Category");

            // Delete Category
            group.MapDelete("/{id}", async (string id, ICategoryService categoryService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var deleted = await categoryService.DeleteCategory(id, userId);
                if (!deleted) return Results.Problem("Error while deleting Category");

                return Results.NoContent();
            })
            .WithSummary("Delete a Category by ID");

            // Get Category by Id
            group.MapGet("/{id}", async (string id, ICategoryService categoryService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var category = await categoryService.GetCategoryById(id);
                if (category == null) return Results.NotFound($"Category with ID {id} not found.");

                var response = CategoryResponse.FromDomain(category);
                return Results.Ok(response);
            })
            .WithSummary("Get a Category by ID");

            // Get Categories by User
            group.MapGet("/user/", async (ICategoryService categoryService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var categories = await categoryService.GetUserCategories(userId);
                var response = categories.Select(CategoryResponse.FromDomain);

                return Results.Ok(response);
            })
            .WithSummary("Get Categories by User ID");
        }
    }
}
