using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Localization;

using AspNetCore.Localizer.Json.Extensions;
using FileProcessor.API.Services;

using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using AspNetCore.Localizer.Json.JsonOptions;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
builder.Services.AddScoped<IFileParser, CsvParser>();
builder.Services.AddScoped<CsvParser>();
builder.Services.AddScoped<ExcelParser>();
builder.Services.AddScoped<JsonParser>();
builder.Services.AddScoped<XmlParser>();
builder.Services.AddScoped<FileParserFactory>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();
builder.Services.AddControllers();


builder.Services.AddSingleton<IStringLocalizerFactory, PropertiesStringLocalizerFactory>();
builder.Services.AddLocalization(); // Needed for request culture
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { "en", "ar" };
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(cultures);
    options.AddSupportedUICultures(cultures);
});

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


// ✅ Configure supported cultures
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "ar" };
    options.DefaultRequestCulture = new RequestCulture("en");
    Console.WriteLine($"Active culture: {CultureInfo.CurrentUICulture}");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
});


// Adding Swagger and OPENAPI:
builder.Services.AddEndpointsApiExplorer();
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

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");
Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name);

var app = builder.Build();
app.UseRequestLocalization();
app.UseCors("AllowVueApp");


app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Adding Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "File Processor API V1");
    options.RoutePrefix = "docs"; // Serve Swagger at root (optional)
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();