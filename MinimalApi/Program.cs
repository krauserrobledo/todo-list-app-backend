using Application;
using Infraestructure;
using Microsoft.OpenApi;
using MinimalApi.Endpoints;
using MinimalApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",    // Angular dev server
                "https://localhost:4200"   // Angular dev server HTTPS
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
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

    // JWT Bearer definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Introduce tu JWT con el formato: Bearer {token}"
    });

    // Requirement (v10 syntax)
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
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

app.UseCors("AllowAngularApp");

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