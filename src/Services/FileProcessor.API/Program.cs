using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Localization;
using FileProcessor.API.Services;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Redis Configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));

// File Parser Services
builder.Services.AddScoped<IFileParser, CsvParser>();
builder.Services.AddScoped<CsvParser>();
builder.Services.AddScoped<ExcelParser>();
builder.Services.AddScoped<JsonParser>();
builder.Services.AddScoped<XmlParser>();
builder.Services.AddScoped<FileParserFactory>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();

// Controllers
builder.Services.AddControllers();

// Localization Services
builder.Services.AddSingleton<IStringLocalizerFactory, PropertiesStringLocalizerFactory>();
builder.Services.AddLocalization();

// Configure supported cultures (consolidated - removed duplicate)
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "ar" };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
});

// HTTP Client for RuleEngine
builder.Services.AddHttpClient("RuleEngine", client =>
{
    client.BaseAddress = new Uri("http://localhost:5182"); // RuleEngine.API
});

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Your Vue app URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ⚠️ IMPORTANT: Add Swagger Generation Service
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

    // Include XML comments (with safety check)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Set default culture
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");
Console.WriteLine($"Assembly Name: {Assembly.GetExecutingAssembly().GetName().Name}");
Console.WriteLine($"Active culture: {CultureInfo.CurrentUICulture}");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRequestLocalization();
app.UseCors("AllowVueApp");

// Swagger setup (only in development, but properly configured)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "File Processor API V1");
        options.RoutePrefix = "docs"; // Serve Swagger at /docs
    });
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();