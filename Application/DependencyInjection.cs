using Application.Abstractions.Services;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    /// <summary>
    /// Container for Services dependency injections
    /// </summary>
    public static class DependencyInjection
    {

        /// <summary>
        /// Get all the interfaces together for Dependency injection inprogram.cs
        /// </summary>
        /// <param name="services">Services Collection</param>
        /// <returns></returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            // Application services Injection
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ISubtaskService, SubtaskService>();
            services.AddScoped<ITagService, TagService>();

            return services;
        }
    }
}