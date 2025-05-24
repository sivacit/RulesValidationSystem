using Microsoft.OpenApi.Models;
using RulesValidationSystem.Data;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Adding Swagger and OPENAPI:
builder.Services.AddEndpointsApiExplorer();
// ✅ Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Your Vue app URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "File Processor API",
        Version = "v1",
        Description = "API for uploading and validating CSV, Excel, JSON, and XML files using RulesEngine.",
        Contact = new OpenApiContact
        {
            Name = "Sutherland Global",
            Email = "sivakumar.krishnasamy@sutherlandglobal.com"
        }
    });

    // Include XML comments (optional but recommended)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// 👇 Use SQLite instead of SQL Server
builder.Services.AddDbContext<RulesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));



var app = builder.Build();
app.UseCors("AllowVueApp");
app.UseAuthorization();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
}


// Adding Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "File Processor API V1");
    options.RoutePrefix = "docs"; // Serve Swagger at root (optional)
});


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
