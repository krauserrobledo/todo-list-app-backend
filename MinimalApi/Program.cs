using Application.Abstractions;
using Data;
<<<<<<< HEAD
<<<<<<< HEAD
using Data.Identity;
using Data.Repositories;
using Microsoft.AspNetCore.Identity;
=======
using Data.Repositories;
>>>>>>> 482bc95 (fix(data): Bug fix in data layer before Identity implementation)
=======
using Data.Identity;
using Data.Repositories;
using Microsoft.AspNetCore.Identity;
>>>>>>> af90a08 (feat(identity): Identity implementation)
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// var for connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.

// Add DbContext with SQL Server provider
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> af90a08 (feat(identity): Identity implementation)
// Identity DI
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

<<<<<<< HEAD
=======
>>>>>>> 482bc95 (fix(data): Bug fix in data layer before Identity implementation)
=======
>>>>>>> af90a08 (feat(identity): Identity implementation)
// Add repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ISubtaskRepository, SubtaskRepository>();
<<<<<<< HEAD
<<<<<<< HEAD
=======
builder.Services.AddScoped<IUserRepository, UserRepository>();
>>>>>>> 482bc95 (fix(data): Bug fix in data layer before Identity implementation)
=======
>>>>>>> af90a08 (feat(identity): Identity implementation)

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
