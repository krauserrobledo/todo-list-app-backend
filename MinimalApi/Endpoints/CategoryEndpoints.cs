using Domain.Abstractions;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.DTOs;

namespace MinimalApi.Endpoints
{
    public static class CategoryEndpoints
    {

        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/categories")
                .WithTags("Categories");
            // Defined category endpoints
            group.MapPost("/", CreateCategory)
                .WithSummary("Create a new Category")
                .RequireAuthorization();
            group.MapPut("/{id}", UpdateCategory)
                .WithSummary("Update an existing Category")
                .RequireAuthorization();
            group.MapDelete("/{id}", DeleteCategory)
                .WithSummary("Delete a Category by ID")
                .RequireAuthorization();
            group.MapGet("/{id}", GetCategoryById)
                .WithSummary("Get a Category by ID")
                .RequireAuthorization();                
            group.MapGet("/user/{userId}", GetCategoriesByUser)
                .WithSummary("Get Categories by User ID")
                .RequireAuthorization();
        }
        private static async Task<IResult> CreateCategory(
            [FromBody] CategoryCreateDTO request,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to create a category
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Name))
                    return Results.BadRequest("Category name is required.");
                // Get user ID from context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();
                // Check for duplicate category name for the same user
                var nameExists = await categoryRepository.CategoryNameExists(request.Name, userId);
                if (nameExists)
                {
                    return Results.Conflict("A category with the same name already exists for this user.");
                }
                // Create new Category entity
                var newCategory = new Domain.Models.Category
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Name = request.Name,
                    Color = request.Color
                };
                // Save to repository
                var createdCategory = await categoryRepository.CreateCategory(newCategory);
                // Return success response
                return Results.Created($"/api/categories/{createdCategory.Id}", new
                {
                    id = createdCategory.Id,
                    name = createdCategory.Name,
                    color = createdCategory.Color
                });
            }
            catch (Exception ex)
            {
                // Log ex
                return Results.Problem($"Error while creating Category: {ex.Message}");
            }
        }

        private static async Task<IResult> UpdateCategory(
            string id,
            [FromBody] CategoryUpdateDTO request,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to update a category
            try
            {
                
                // Check if exists 
                var existingCategory = await categoryRepository.GetCategoryById(id);
                if (existingCategory == null)
                {
                    return Results.NotFound($"Category not found");
                }
                // Limit update by user
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != existingCategory.UserId)
                    return Results.Unauthorized();
                // Check for duplicate name
                var nameExists = await categoryRepository.CategoryNameExists(request.Name, existingCategory.UserId);
                if (nameExists)
                {
                    return Results.Conflict("A category with the same name already exists for this user.");
                }
                // Color Validation

                existingCategory.Name = request.Name;

                existingCategory.Color = request.Color;

                // Update in repository
                var updatedCategory = await categoryRepository.UpdateCategory(existingCategory);
                if (updatedCategory == null)
                {
                    return Results.Problem("Error while updating Category");
                }
                // Return response
                return Results.Ok(new
                {
                    id = existingCategory.Id,
                    name = updatedCategory.Name?? existingCategory.Name,
                    color = updatedCategory.Color?? existingCategory.Color
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while updating Category{ex.Message}");
            }
        }

        private static async Task<IResult> DeleteCategory(
            string id,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to delete a category
            try
            {
                // Verify ID
                var existingCategory = await categoryRepository.GetCategoryById(id);
                if (existingCategory == null)
                {
                    return Results.NotFound($"Category with ID {id} not found.");
                }
                // Delete from repository
                var deleted = await categoryRepository.DeleteCategory(id);
                if (!deleted)
                {
                    return Results.Problem("Error while deleting Category");
                }
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while deleting Category: {ex.Message}");
            }

        }
        private static async Task<IResult> GetCategoryById(
            string id,
            ICategoryRepository categoryRepository,
            HttpContext context)
        {
            // Logic to get a category by ID
            try
            {
                var category = await categoryRepository.GetCategoryById(id);
                if (category == null)
                {
                    return Results.NotFound($"Category with ID {id} not found.");
                }
                return Results.Ok(new
                {
                    id = category.Id,
                    name = category.Name,
                    color = category.Color
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while retrieving Category: {ex.Message}");
            }
        }
        private static async Task<IResult> GetCategoriesByUser(
            ICategoryRepository categoryRepository,
            HttpContext context)

        {
            // Logic to get categories by user
            try
            {
                // Check user 
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                var categories = await categoryRepository.GetCategoriesByUser(userId);
                return Results.Ok(categories.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    color = c.Color
                }));
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error while retrieving Categories: {ex.Message}");
            }
        }
    }
}
