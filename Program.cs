using Microsoft.EntityFrameworkCore;
using QuickBiteAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration sources to include Config folder
builder.Configuration
    .SetBasePath(Path.Combine(builder.Environment.ContentRootPath, "Config"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "QuickBite API",
        Version = "v1",
        Description = "A comprehensive RESTful API for managing restaurant menu items. This API provides full CRUD operations for menu items including creation, retrieval, updating, and deletion with proper validation and error handling.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "QuickBite Development Team",
            Email = "dev@quickbite.com"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add example schemas
    c.EnableAnnotations();
});

// Add Entity Framework with SQLite (only in non-testing environment)
if (!builder.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<QuickBiteDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

// Ensure database is created (only in non-testing environment)
if (!app.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<QuickBiteDbContext>();
        context.Database.EnsureCreated();
    }
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }
