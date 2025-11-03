using Application;
using Infraestructure;
using Microsoft.OpenApi.Models;
using MinimalApi.Endpoints;
using MinimalApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ToDo List API",
        Version = "v1",
        Description = "Task Manager API",
        Contact = new OpenApiContact
        {
            Name = "Daniel Robledo",
            Email = "al.daniel.robledo.lobato@iesportada.org"
        }
    });

    // JWT Configuration for Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CLEAN ARCHITECTURE DEPENDENCY INJECTION CONTAINERS
// FIRST All infrastructure services (DbContext, Identity, Authentication, Repositories) are registered here
builder.Services.AddInfraestructure(builder.Configuration);

// SECOND All application services (Business logic services) are registered here  
builder.Services.AddApplication();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Swagger Middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo List API v1");
    options.RoutePrefix = "swagger";
});

// HTTPS Middleware
app.UseHttpsRedirection();

// Custom Middlewares
app.UseExceptionHandlingMiddleware();
app.UseRequestLoggingMiddleware();

// JWT Middleware
app.UseAuthentication();

// Auth Middleware
app.UseAuthorization();

// Map Endpoints
app.MapAuthEndpoints();
app.MapTaskEndpoints();
app.MapTagEndpoints();
app.MapCategoryEndpoints();
app.MapSubtaskEndpoints();

app.Run();