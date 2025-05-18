using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using RulesEngine.Models;

using Microsoft.Extensions.Localization;
using System;
using System.IO;
using RuleValidationSystem.Models;
using System.Text.Json;
using System.Dynamic;



/// <summary>
/// Handles file uploads and validation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IStringLocalizer _localizer;
    private readonly FileParserFactory _factory;
    private readonly IRedisRepository _redis;

    private readonly RulesEngine.RulesEngine _rulesEngine;

    private readonly IWebHostEnvironment _env;

    public static ExpandoObject ToExpando<T>(T obj)
    {
        var expando = new ExpandoObject();
        var dict = (IDictionary<string, object>)expando;

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            dict[prop.Name] = prop.GetValue(obj);
        }

        return expando;
    }

    public FileUploadController(FileParserFactory factory, IRedisRepository redis, IStringLocalizerFactory localizerFactory, IWebHostEnvironment env)
    {
        this._factory = factory;
        this._redis = redis;
        this._localizer = localizerFactory.Create("lang", "FileProcessor.API");
        this._env = env;

        Console.WriteLine("Assembly: " + Assembly.GetExecutingAssembly().GetName().Name);
        Console.WriteLine($"Using localizer: lang, assembly: {Assembly.GetExecutingAssembly().GetName().Name}");

    }

    [HttpGet("dump-localization")]
    public IActionResult Dump()
    {
        var all = _localizer.GetAllStrings(true)
            .Select(x => new { x.Name, x.Value });
        return Ok(all);
    }

    /// <summary>
    /// Uploads a file (.csv, .json, .xml, .xlsx) and stores validated records in Redis.
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <returns>Success message and Redis key</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "lang.en.json");
        Console.WriteLine($"---------------- File Path {filePath}");

        if (file == null || file.Length == 0)
            return BadRequest(_localizer["FileEmpty"]);

        // Load rules and create the RulesEngine instance
        var rulesPath = Path.Combine(_env.ContentRootPath, "rules.json");
        var rulesJson = System.IO.File.ReadAllText(rulesPath);
        var workflows = JsonSerializer.Deserialize<Workflow[]>(rulesJson);
        var rulesEngine = new RulesEngine.RulesEngine(workflows, null); // ✅ use this instead of _rulesEngine

        try
        {
            var parser = _factory.GetParser(file.ContentType, file.FileName);
            using var stream = file.OpenReadStream();
            var records = await parser.ParseAsync(stream);

            // Limit to first 100 records
            var limitedRecords = records?.Take(100).ToList();

            // Store validated records for response
            var validatedRecords = new List<ExpandoObject>();

            foreach (var record in limitedRecords)
            {
                var expando = ToExpando(record); // Convert FileRecord to ExpandoObject
                var dict = (IDictionary<string, object>)expando;

                var errors = new List<string>();

                 var input = new RuleParameter("input1", dict);
                var ruleResults = await rulesEngine.ExecuteAllRulesAsync("CSVWorkflow", new[] { input });

                // Run rules engine against the expando object
                // var ruleResults = await rulesEngine.ExecuteAllRulesAsync("CSVWorkflow", expando);
                Console.WriteLine($" {dict } --------------- {ruleResults}");
                foreach (var result in ruleResults)
                {
                    if (!result.IsSuccess)
                    {
                        errors.Add(result.ExceptionMessage ?? result.Rule?.ErrorMessage ?? "Validation failed.");
                    }
                }

                dict["ValidationErrors"] = errors;
                validatedRecords.Add(expando);
            }

            Console.WriteLine(validatedRecords);

            // Store full original records in Redis
            var key = $"upload:{Guid.NewGuid()}";
            await _redis.StoreAsync(key, records);
            Console.WriteLine(validatedRecords);
            return Ok(new
            {
                message = _localizer["FileUploaded"],
                redisKey = key,
                data = validatedRecords // return validated 100 records with ValidationErrors
            });
        }
        catch (NotSupportedException)
        {
            return BadRequest(_localizer["FileTypeInvalid"]);
        }
    }
}
