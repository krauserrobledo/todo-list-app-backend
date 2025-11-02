using Application.Abstractions.Services;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    /// <summary>
    /// 
    /// </summary>
    public static class DependencyInjection
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
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