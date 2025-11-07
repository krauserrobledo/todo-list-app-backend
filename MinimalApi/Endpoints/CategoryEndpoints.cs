using Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.CategoryDTOs;
using Microsoft.AspNetCore.Authorization;

namespace MinimalApi.Endpoints
{

    /// <summary>
    /// Endpoints for managing categories.
    /// </summary>
    public static class CategoryEndpoints
    {

        /// <summary>
        /// Configures the API endpoints for category-related operations.
        /// </summary>
        /// <param name="app">The application builder.</param>
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
            
            group.MapGet("/user/", GetCategoriesByUser)
                .WithSummary("Get Categories by User ID")
                .RequireAuthorization();
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <remarks>Validates the request and ensures the category name is unique for the user before creation.</remarks>
        /// <param name="request">The category creation request.</param>
        /// <param name="categoryService">The category service.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A result indicating the outcome of the category creation.</returns>
        private static async Task<IResult> CreateCategory(
            
            [FromBody] CategoryCreateRequest request,
            ICategoryService categoryService,
            HttpContext context)
        {

            // Logic to create a category
            try
            {
                // Get user ID from context
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                // Create from body request and keep current user
                var category = await categoryService.CreateCategory
                    (
                    request.Name, 
                    request.Color, 
                    userId);

                var response = CategoryResponse.FromDomain(category);

                return Results.Created($"/api/categories/{category.Id}", response);
            }

            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }

            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
            }

            catch (Exception ex)
            {
                return Results.Problem($"Error creating category: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <remarks>Validates the request, checks for category existence, and ensures the category name is unique for the user before updating.</remarks>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="request">The category update request.</param>
        /// <param name="categoryService">The category service.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A result indicating the outcome of the category update.</returns>
        private static async Task<IResult> UpdateCategory(
            string id,
            [FromBody] CategoryUpdateRequest request,
            ICategoryService categoryService,
            HttpContext context)
        {

            // Logic to update a category
            try
            {
                // Limit update by user
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var updatedCategory = await categoryService.UpdateCategory(

                    id,
                    userId,
                    request.Name,
                    request.Color);

                if (updatedCategory == null) return Results.NotFound($"Category with ID {id} not found or access denied");

                var response = CategoryResponse.FromDomain(updatedCategory);
                return Results.Ok(response);
            }

            catch (InvalidOperationException ex)
            {

                return Results.Conflict(ex.Message);
            }

            catch (Exception ex)
            {

                return Results.Problem($"Error updating category: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Deletes a category by ID.
        /// </summary>
        /// <remarks>Verifies the category existence before deletion.</remarks>
        /// <param name="id">The ID of the category to delete.</param>
        /// <param name="categoryService">Service for category .</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Results indicating the outcome of the deletion.</returns>
        private static async Task<IResult> DeleteCategory(
            string id,
            ICategoryService categoryService,
            HttpContext context)
        {
            // Logic to delete a category
            try
            {
                // Check user
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                // Delete from service
                var deleted = await categoryService.DeleteCategory(id, userId);

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

        /// <summary>
        /// Gets a category by ID.
        /// </summary>
        /// <remarks>Retrieves a category based on the provided ID.</remarks>
        /// <param name="id">Category Id</param>
        /// <param name="categoryService"> Service class for category</param>
        /// <param name="context"> HTTP context</param>
        /// <returns>Categories collection with details if exists or valid</returns>
        private static async Task<IResult> GetCategoryById(
            string id,
            ICategoryService categoryService,
            HttpContext context)
        {
            // Logic to get a category by ID
            try
            {
                // Check user
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var category = await categoryService.GetCategoryById(id);

                // Check existence
                if (category == null) return Results.NotFound($"Category with ID {id} not found.");

                var response = CategoryResponse.FromDomain(category);

                return Results.Ok(response);
            }

            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving category: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets categories by user ID.
        /// </summary>
        /// <remarks>Retrieves all categories associated with the authenticated user.</remarks>
        /// <param name="categoryService">Category Service class implementing interface </param>
        /// <param name="context">HTTP context</param>
        /// <returns> categories for an user by context</returns>
        private static async Task<IResult> GetCategoriesByUser(
            ICategoryService categoryService,
            HttpContext context)
        {

            // Logic to get categories by user
            try
            {
                // Check user 
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var categories = await categoryService.GetUserCategories(userId);

                var response = categories.Select(CategoryResponse.FromDomain);

                return Results.Ok(response);
            }

            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving categories: {ex.Message}");
            }
        }
    }
}
